import psycopg

def main():
    conn = psycopg.connect(
        host="127.0.0.1",
        port=5433,
        dbname="auth_db",
        user="auth_user",
        password="superpassword",
    )

    conn.execute("""
        CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
    """)

    conn.execute("""
        CREATE TABLE IF NOT EXISTS users (
            id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
            username VARCHAR(100) UNIQUE NOT NULL,
            email VARCHAR(255) UNIQUE NOT NULL,
            password_hash TEXT NOT NULL,
            role VARCHAR(20) DEFAULT 'user',
            created_at TIMESTAMP DEFAULT NOW()
        );
    """)

    conn.execute("""
        ALTER TABLE users
        ADD COLUMN IF NOT EXISTS email VARCHAR(255) UNIQUE NOT NULL;
    """)

    conn.execute("""
        CREATE TABLE IF NOT EXISTS refresh_tokens (
            id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
            user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            token_hash TEXT NOT NULL,
            expires_at TIMESTAMP NOT NULL,
            revoked_at TIMESTAMP,
            created_at TIMESTAMP DEFAULT NOW()
        );
    """)

    conn.execute("""
        CREATE TABLE IF NOT EXISTS questionnaires (
            id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
            name VARCHAR(200) NOT NULL,
            description TEXT,
            system_type VARCHAR(100) NOT NULL,
            created_at TIMESTAMP DEFAULT NOW()
        );
    """)

    conn.execute("""
        CREATE TABLE IF NOT EXISTS questions (
            id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
            questionnaire_id UUID NOT NULL REFERENCES questionnaires(id) ON DELETE CASCADE,
            question_text TEXT NOT NULL,
            field_type VARCHAR(50) NOT NULL,
            system_type VARCHAR(100) NOT NULL,
            options JSONB DEFAULT '[]'::jsonb,
            is_required BOOLEAN DEFAULT TRUE,
            sort_order INT DEFAULT 0,
            created_at TIMESTAMP DEFAULT NOW()
        );
    """)

    result = conn.execute("""
        SELECT COUNT(*) FROM questionnaires;
    """)
    if result.fetchone()[0] == 0:
        questionnaire_result = conn.execute("""
            INSERT INTO questionnaires (name, description, system_type)
            VALUES ($1, $2, $3)
            RETURNING id;
        """, (
            'Медицинская анкета пациента',
            'Анкета для сбора первичных данных пациента и симптомов.',
            'medical'
        ))
        questionnaire_id = questionnaire_result.fetchone()[0]

        conn.execute("""
            INSERT INTO questions (questionnaire_id, question_text, field_type, system_type, options, is_required, sort_order)
            VALUES
              ($1, 'ФИО пациента', 'text', 'personal', '[]', TRUE, 1),
              ($1, 'Возраст', 'number', 'personal', '[]', TRUE, 2),
              ($1, 'Пол', 'radio', 'personal', '["Мужской","Женский","Другое"]', TRUE, 3),
              ($1, 'Дата рождения', 'date', 'personal', '[]', FALSE, 4),
              ($1, 'Основные жалобы', 'textarea', 'medical', '[]', TRUE, 5),
              ($1, 'Хронические заболевания', 'textarea', 'medical', '[]', FALSE, 6),
              ($1, 'Принимаемые лекарства', 'textarea', 'medical', '[]', FALSE, 7),
              ($1, 'Тип системы', 'select', 'system', '["Поликлиника","Стационар","Диагностика"]', TRUE, 8);
        """, [questionnaire_id])

    conn.execute("""
        CREATE INDEX IF NOT EXISTS idx_refresh_user_id 
        ON refresh_tokens(user_id);
    """)

    conn.execute("""
        CREATE INDEX IF NOT EXISTS idx_refresh_expires 
        ON refresh_tokens(expires_at);
    """)

    conn.commit()
    conn.close()

    print("Schema initialized successfully.")


if __name__ == "__main__":
    main()