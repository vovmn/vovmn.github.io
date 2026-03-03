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
            password_hash TEXT NOT NULL,
            role VARCHAR(20) DEFAULT 'user',
            created_at TIMESTAMP DEFAULT NOW()
        );
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