import os
import json
import psycopg2
from psycopg2 import sql
from psycopg2.extras import execute_values
from dotenv import load_dotenv

load_dotenv()  # загрузить переменные из .env если есть

DB_CONFIG = {
    'host': os.getenv('DB_HOST', 'localhost'),
    'port': int(os.getenv('DB_PORT', 5432)),
    'dbname': os.getenv('DB_NAME', 'your_db'),
    'user': os.getenv('DB_USER', 'postgres'),
    'password': os.getenv('DB_PASSWORD', 'postgres')
}

JSON_FILE = 'questionnaire.json'

def get_connection():
    return psycopg2.connect(**DB_CONFIG)

def create_tables(cur):
    # 1. Добавить столбец gender в users, если отсутствует
    cur.execute("""
        ALTER TABLE users ADD COLUMN IF NOT EXISTS gender CHAR(1) CHECK (gender IN ('M', 'F'));
    """)

    # 2. Создать справочники и хранилища
    cur.execute("""
        CREATE TABLE IF NOT EXISTS body_systems (
            id SERIAL PRIMARY KEY,
            name VARCHAR(100) NOT NULL,
            code VARCHAR(50) NOT NULL UNIQUE
        );
        CREATE TABLE IF NOT EXISTS questions (
            id SERIAL PRIMARY KEY,
            system_id INTEGER NOT NULL REFERENCES body_systems(id) ON DELETE CASCADE,
            question_text TEXT NOT NULL,
            question_type VARCHAR(20) NOT NULL CHECK (question_type IN ('boolean','numeric','single_choice','multiple_choice','text')),
            gender_filter CHAR(1) DEFAULT NULL CHECK (gender_filter IN ('M','F', NULL)),
            required BOOLEAN DEFAULT TRUE,
            sort_order INTEGER DEFAULT 0,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            UNIQUE (system_id, question_text)
        );
        CREATE TABLE IF NOT EXISTS answer_options (
            id SERIAL PRIMARY KEY,
            question_id INTEGER NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
            option_text TEXT NOT NULL,
            option_code VARCHAR(50),
            sort_order INTEGER DEFAULT 0,
            UNIQUE (question_id, option_code)
        );
        CREATE TABLE IF NOT EXISTS user_questionnaire_assignments (
            id SERIAL PRIMARY KEY,
            user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            system_id INTEGER NOT NULL REFERENCES body_systems(id),
            status VARCHAR(20) DEFAULT 'pending' CHECK (status IN ('pending','in_progress','completed')),
            assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            completed_at TIMESTAMP,
            UNIQUE (user_id, system_id)
        );
        CREATE TABLE IF NOT EXISTS user_answers (
            id SERIAL PRIMARY KEY,
            user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            question_id INTEGER NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
            assignment_id INTEGER REFERENCES user_questionnaire_assignments(id),
            value_boolean BOOLEAN,
            value_numeric DOUBLE PRECISION,
            value_text TEXT,
            selected_option_id INTEGER REFERENCES answer_options(id),
            answered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            UNIQUE (user_id, question_id, selected_option_id)
        );
    """)

def import_data(cur, data):
    # Загружаем системы
    systems = data['body_systems']
    for sys in systems:
        cur.execute("""
            INSERT INTO body_systems (code, name)
            VALUES (%s, %s)
            ON CONFLICT (code) DO UPDATE SET name = EXCLUDED.name;
        """, (sys['code'], sys['name']))

    # Получаем маппинг code -> id
    cur.execute("SELECT id, code FROM body_systems")
    system_ids = {row[1]: row[0] for row in cur.fetchall()}

    # Загружаем вопросы
    questions = data['questions']
    for q in questions:
        sys_id = system_ids[q['system_code']]
        gender = q.get('gender_filter')  # может быть None, 'M', 'F'
        cur.execute("""
            INSERT INTO questions (system_id, question_text, question_type, gender_filter, sort_order)
            VALUES (%s, %s, %s, %s, %s)
            ON CONFLICT (system_id, question_text) DO UPDATE SET
                question_type = EXCLUDED.question_type,
                gender_filter = EXCLUDED.gender_filter,
                sort_order = EXCLUDED.sort_order
            RETURNING id;
        """, (sys_id, q['text'], q['type'], gender, q['sort_order']))
        question_id = cur.fetchone()[0]

        # Если есть варианты ответов
        options = q.get('options')
        if options:
            for opt in options:
                cur.execute("""
                    INSERT INTO answer_options (question_id, option_text, option_code, sort_order)
                    VALUES (%s, %s, %s, %s)
                    ON CONFLICT (question_id, option_code) DO NOTHING;
                """, (question_id, opt['text'], opt['code'], opt.get('sort_order', 0)))

def main():
    try:
        with open(JSON_FILE, 'r', encoding='utf-8') as f:
            data = json.load(f)
    except FileNotFoundError:
        print(f"Файл {JSON_FILE} не найден.")
        return

    conn = get_connection()
    try:
        with conn:
            with conn.cursor() as cur:
                create_tables(cur)
                import_data(cur, data)
        print("Импорт завершён успешно.")
    except Exception as e:
        print(f"Ошибка: {e}")
        conn.rollback()
    finally:
        conn.close()

if __name__ == '__main__':
    main()