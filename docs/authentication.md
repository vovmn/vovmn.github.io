# Аутентификация

Аутентификация в приложении осуществляется при помощи JWT-токена.
Он выступает в роли токена доступа (access token). После получения этот токен должен быть добавлен в каждый запрос в заголовок `Authorization`. Пример:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVC...
```

## Вход в систему

Для входа в систему необходимо выполнить POST запрос по адресу

```
{{baseUrl}}/api/Auth/Login
```

Тело запроса (application/json):

- `email` - электронная почта пользователя (строка, обязательный)
- `password` - пароль пользователя (строка, обязательный)

Успешный ответ (200 ОК):

- `accessToken` - JWT-токен доступа (строка, обязательный)
- `refreshToken` - refresh-токен (строка, обязательный)

Ошибка (401): аутентификация не удалась

Пример запроса:

```
{
    "email": "admin@test.com",
    "password: "Password123"
}
```

Пример ответа (200 ОК):

```
{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVC...",
    "refreshToken": "9ekXNW+YZLhCBYrA4g0bjo2XgPUZOUm46..."
}
```

## Обновление токена

Access-токен имеет короткое ограниченное время жизни. Поэтому, при получении ошибки `401 Unauthorized` на любой последующий запрос, необходимо обновить данный токен при помощи refresh-токена.
Для обновления токена необходимо выполнить POST запрос по следующему адресу:

```
{{baseUrl}}/api/Auth/RefreshToken
```

Тело запроса (application/json):

- `oldToken`: старый refresh-токен (строка, обязательный)

Успешный ответ (200 ОК):

- `accessToken` - JWT-токен доступа (строка, обязательный)
- `refreshToken` - refresh-токен (строка, обязательный)

Пример запроса:

```
{
    "oldToken": "9ekXNW+YZLhCBYrA4g0bjo2XgPUZOUm46..."
}
```

Пример ответа (200 ОК):

```
{
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVC...",
    "refreshToken": "o99L/9L3R83ZfXn10e5DpAnkf/5y42v1V..."
}
```

Ошибка (401): не удалось обновить токен

## Выход из системы

Для выхода из системы и обнуления всех access/refresh-токенов необходимо выполнить POST запрос по адресу:

```
{{baseUrl}}/api/Logout
```
