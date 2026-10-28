# ER-модель базы Immunolab

Диаграмма соответствует канонической PostgreSQL-схеме из `backend/schema.sql`.
Таблицы `schema_migrations` и `questionnaire_releases` являются независимыми
реестрами: текущая схема не задаёт для них внешних ключей.

```mermaid
erDiagram
    USERS {
        uuid id PK
        varchar username UK
        varchar email UK
        text password_hash
        varchar role
        varchar phone "nullable"
        date birth_date "nullable"
        varchar residence "nullable"
        char gender "nullable: M or F"
        timestamp created_at
    }

    REFRESH_TOKENS {
        uuid id PK
        uuid user_id FK
        text token_hash UK
        timestamp expires_at
        timestamp created_at
    }

    BODY_SYSTEMS {
        uuid id PK
        varchar name
        varchar code UK
    }

    QUESTIONS {
        uuid id PK
        uuid system_id FK
        varchar code "nullable only when inactive"
        text question_text
        varchar question_type
        char gender_filter "nullable: M or F"
        boolean required
        boolean active
        boolean allow_unknown
        text help_text "nullable"
        varchar recall_period "nullable"
        jsonb display_condition "nullable"
        jsonb validation "nullable"
        jsonb alert "nullable"
        integer sort_order
        timestamp created_at
    }

    ANSWER_OPTIONS {
        uuid id PK
        uuid question_id FK
        text option_text
        varchar option_code
        integer sort_order
    }

    USER_QUESTIONNAIRE_ASSIGNMENTS {
        uuid id PK
        uuid user_id FK
        uuid system_id FK
        uuid assigned_by FK "nullable"
        varchar status
        varchar priority
        timestamp due_at "nullable"
        text note "nullable"
        varchar questionnaire_version "nullable, no FK"
        timestamp assigned_at
        timestamp started_at "nullable"
        timestamp completed_at "nullable"
        timestamp cancelled_at "nullable"
        timestamp updated_at
    }

    USER_ANSWERS {
        uuid id PK
        uuid user_id FK
        uuid question_id FK
        uuid assignment_id FK
        boolean value_boolean "nullable"
        double value_numeric "nullable"
        text value_text "nullable"
        boolean value_unknown
        boolean skipped
        varchar skip_reason "nullable"
        uuid selected_option_id FK "nullable"
        timestamp answered_at
    }

    APPOINTMENTS {
        uuid id PK
        uuid user_id FK
        timestamp scheduled_at
        varchar specialist "nullable"
        varchar doctor "nullable"
        varchar status
        text comment "nullable"
        timestamp created_at
    }

    QUESTIONNAIRE_RELEASES {
        varchar version PK
        char source_sha256
        integer question_count
        timestamp imported_at
    }

    SCHEMA_MIGRATIONS {
        varchar version PK
        text description
        timestamp applied_at
    }

    USERS ||--o{ REFRESH_TOKENS : owns
    USERS ||--o{ USER_QUESTIONNAIRE_ASSIGNMENTS : receives
    USERS o|--o{ USER_QUESTIONNAIRE_ASSIGNMENTS : assigns
    USERS ||--o{ USER_ANSWERS : gives
    USERS ||--o{ APPOINTMENTS : books
    BODY_SYSTEMS ||--o{ QUESTIONS : contains
    BODY_SYSTEMS ||--o{ USER_QUESTIONNAIRE_ASSIGNMENTS : scopes
    QUESTIONS ||--o{ ANSWER_OPTIONS : offers
    QUESTIONS ||--o{ USER_ANSWERS : answered_by
    USER_QUESTIONNAIRE_ASSIGNMENTS ||--o{ USER_ANSWERS : contains
    ANSWER_OPTIONS o|--o{ USER_ANSWERS : selected_in
```

## Составные связи и ограничения

- `(user_answers.assignment_id, user_answers.user_id)` ссылается на
  `(user_questionnaire_assignments.id, user_questionnaire_assignments.user_id)`.
  Ответ нельзя записать за другого пациента.
- `(user_answers.selected_option_id, user_answers.question_id)` ссылается на
  `(answer_options.id, answer_options.question_id)`. Вариант нельзя использовать
  для чужого вопроса.
- `(questions.system_id, questions.code)` уникально; у активного вопроса `code`
  не может быть `NULL`.
- `(answer_options.question_id, answer_options.option_code)` уникально.
- `user_answers_payload_check` допускает ровно одно значение, явное «не знаю»
  либо явный пропуск с причиной.
- `assignments_lifecycle_check` согласует статус назначения с датами начала,
  завершения и отмены.
- Уникальный индекс `(assignment_id, question_id, selected_option_id) NULLS NOT
  DISTINCT` предотвращает дубли ответов и одновременно допускает несколько
  разных вариантов для `multiple_choice`.

## Удаление связанных данных

- Удаление пользователя каскадно удаляет его токены, назначения, ответы и записи
  на приём.
- Удаление системы каскадно удаляет вопросы; назначения блокируют удаление
  используемой системы, поскольку для их FK не задан `ON DELETE`.
- Удаление вопроса каскадно удаляет варианты и ответы.
- Удаление назначения каскадно удаляет его ответы.
- Используемый вариант ответа нельзя удалить (`ON DELETE RESTRICT`).
