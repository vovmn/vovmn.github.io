#!/bin/bash
set -e

BASE_URL="http://localhost:5000"
USERNAME="testuser_$(date +%s)"
PASSWORD="TestPass123!"
EMAIL="${USERNAME}@example.com"
GENDER="M"  # или F

echo "============================================"
echo "1. Регистрируем пользователя с gender=$GENDER"
REGISTER=$(curl -s -X POST "$BASE_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d "{
    \"username\": \"$USERNAME\",
    \"email\": \"$EMAIL\",
    \"password\": \"$PASSWORD\",
    \"gender\": \"$GENDER\"
  }")

echo "$REGISTER" | jq .
ACCESS_TOKEN=$(echo "$REGISTER" | jq -r '.access_token')
if [ "$ACCESS_TOKEN" == "null" ]; then
  echo "Ошибка: не получен access_token"
  exit 1
fi
echo "Токен получен: ${ACCESS_TOKEN:0:30}..."

echo -e "\n============================================"
echo "2. Получаем список всех систем"
SYSTEMS=$(curl -s -X GET "$BASE_URL/api/questionnaire/systems" \
  -H "Authorization: Bearer $ACCESS_TOKEN")
echo "$SYSTEMS" | jq .
SYSTEM_CODE=$(echo "$SYSTEMS" | jq -r '.[0].code')  # первая попавшаяся, или явно зададим "nervous"
echo "Выбранная система: $SYSTEM_CODE"

echo -e "\n============================================"
echo "3. Начинаем опросник (назначаем систему пользователю)"
START=$(curl -s -X POST "$BASE_URL/api/questionnaire/start/$SYSTEM_CODE" \
  -H "Authorization: Bearer $ACCESS_TOKEN")
echo "$START" | jq .

ASSIGNMENT_ID=$(echo "$START" | jq -r '.assignment_id')
QUESTION_COUNT=$(echo "$START" | jq '.questions | length')
echo "Назначение создано: $ASSIGNMENT_ID"
echo "Количество вопросов: $QUESTION_COUNT"

echo -e "\n============================================"
echo "4. Проверяем структуру вопросов"
# Выведем первый вопрос для примера
echo "$START" | jq '.questions[0]'

echo -e "\n============================================"
echo "5. Получаем список 'моих назначений'"
MY_ASSIGN=$(curl -s -X GET "$BASE_URL/api/questionnaire/my-assignments" \
  -H "Authorization: Bearer $ACCESS_TOKEN")
echo "$MY_ASSIGN" | jq .
MY_COUNT=$(echo "$MY_ASSIGN" | jq 'length')
echo "Всего назначенных систем: $MY_COUNT"



echo -e "\n============================================"
echo "Тесты завершены успешно!"