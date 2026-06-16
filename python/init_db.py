import psycopg
import json
import argparse
from pathlib import Path

def create_tables(conn):
    """Создание всех таблиц, если их нет"""
    conn.execute("""
        CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
    """)

    # users и refresh_tokens уже существуют, но убедимся, что gender есть
    conn.execute("""
        ALTER TABLE users ADD COLUMN IF NOT EXISTS gender CHAR(1) CHECK (gender IN ('M', 'F'));
    """)

    # Справочник систем организма
    conn.execute("""
        CREATE TABLE IF NOT EXISTS body_systems (
            id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
            name VARCHAR(100) NOT NULL,
            code VARCHAR(50) NOT NULL UNIQUE
        );
    """)

    # Банк вопросов
    conn.execute("""
        CREATE TABLE IF NOT EXISTS questions (
            id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
            system_id UUID NOT NULL REFERENCES body_systems(id) ON DELETE CASCADE,
            question_text TEXT NOT NULL,
            question_type VARCHAR(20) NOT NULL CHECK (question_type IN ('boolean','numeric','single_choice','multiple_choice','text')),
            gender_filter CHAR(1) DEFAULT NULL CHECK (gender_filter IN ('M','F', NULL)),
            required BOOLEAN DEFAULT TRUE,
            sort_order INTEGER DEFAULT 0,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            UNIQUE (system_id, question_text)
        );
    """)

    # Варианты ответов для single_choice / multiple_choice
    conn.execute("""
        CREATE TABLE IF NOT EXISTS answer_options (
            id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
            question_id UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
            option_text TEXT NOT NULL,
            option_code VARCHAR(50),
            sort_order INTEGER DEFAULT 0,
            UNIQUE (question_id, option_code)
        );
    """)

    # Назначение опросников пользователям
    conn.execute("""
        CREATE TABLE IF NOT EXISTS user_questionnaire_assignments (
            id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
            user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            system_id UUID NOT NULL REFERENCES body_systems(id),
            status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending','in_progress','completed')),
            assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            completed_at TIMESTAMP,
            UNIQUE (user_id, system_id)
        );
    """)

    # Ответы пользователей
    conn.execute("""
        CREATE TABLE IF NOT EXISTS user_answers (
            id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
            user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            question_id UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
            assignment_id UUID REFERENCES user_questionnaire_assignments(id),
            value_boolean BOOLEAN,
            value_numeric DOUBLE PRECISION,
            value_text TEXT,
            selected_option_id UUID REFERENCES answer_options(id),
            answered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            UNIQUE (user_id, question_id, selected_option_id)
        );
    """)

def create_indexes(conn):
    """Индексы для часто используемых запросов"""
    conn.execute("""
        CREATE INDEX IF NOT EXISTS idx_questions_system ON questions(system_id);
        CREATE INDEX IF NOT EXISTS idx_answer_options_question ON answer_options(question_id);
        CREATE INDEX IF NOT EXISTS idx_assignments_user ON user_questionnaire_assignments(user_id);
        CREATE INDEX IF NOT EXISTS idx_assignments_system ON user_questionnaire_assignments(system_id);
        CREATE INDEX IF NOT EXISTS idx_user_answers_user ON user_answers(user_id);
        CREATE INDEX IF NOT EXISTS idx_user_answers_question ON user_answers(question_id);
        CREATE INDEX IF NOT EXISTS idx_user_answers_assignment ON user_answers(assignment_id);
    """)

def load_json_data(conn, filepath):
    """Читает JSON и заполняет body_systems, questions, answer_options"""
    with open(filepath, 'r', encoding='utf-8') as f:
        data = json.load(f)

    # Вставка систем
    systems = data.get('body_systems', [])
    system_map = {}
    for s in systems:
        result = conn.execute("""
            INSERT INTO body_systems (name, code)
            VALUES (%(name)s, %(code)s)
            ON CONFLICT (code) DO UPDATE SET name = EXCLUDED.name
            RETURNING id, code;
        """, {'name': s['name'], 'code': s['code']}).fetchone()
        system_map[s['code']] = result[0]  # UUID

    # Вставка вопросов и их вариантов
    questions = data.get('questions', [])
    for q in questions:
        system_id = system_map.get(q['system_code'])
        if not system_id:
            print(f"Пропускаем вопрос: неизвестная система {q['system_code']}")
            continue

        # Вставка вопроса
        result = conn.execute("""
            INSERT INTO questions (system_id, question_text, question_type, gender_filter, sort_order)
            VALUES (%(system_id)s, %(text)s, %(type)s, %(gender)s, %(order)s)
            ON CONFLICT (system_id, question_text) DO UPDATE SET
                question_type = EXCLUDED.question_type,
                gender_filter = EXCLUDED.gender_filter,
                sort_order = EXCLUDED.sort_order
            RETURNING id;
        """, {
            'system_id': system_id,
            'text': q['text'],
            'type': q['type'],
            'gender': q.get('gender_filter'),
            'order': q.get('sort_order', 0)
        }).fetchone()

        question_id = result[0]

        # Вставка вариантов ответа, если есть
        options = q.get('options')
        if options:
            for opt in options:
                conn.execute("""
                    INSERT INTO answer_options (question_id, option_text, option_code, sort_order)
                    VALUES (%(question_id)s, %(text)s, %(code)s, %(order)s)
                    ON CONFLICT (question_id, option_code) DO NOTHING;
                """, {
                    'question_id': question_id,
                    'text': opt['text'],
                    'code': opt['code'],
                    'order': opt.get('sort_order', 0)
                })

def main():
    parser = argparse.ArgumentParser(description='Загрузка медицинской анкеты в БД из JSON')
    parser.add_argument('json_file', nargs='?', default='questionnaire.json',
                        help='Путь к JSON-файлу с анкетой (по умолчанию questionnaire.json)')
    args = parser.parse_args()

    json_path = Path(args.json_file)
    if not json_path.exists():
        print(f"Ошибка: файл {json_path} не найден")
        return

    conn = psycopg.connect(
        host="127.0.0.1",
        port=5433,
        dbname="auth_db",
        user="auth_user",
        password="superpassword",
    )

    try:
        create_tables(conn)
        create_indexes(conn)
        load_json_data(conn, json_path)
        conn.commit()
        print("Схема и данные из JSON успешно загружены.")
    except Exception as e:
        conn.rollback()
        print(f"Ошибка при инициализации: {e}")
    finally:
        conn.close()

if __name__ == "__main__":
    main()