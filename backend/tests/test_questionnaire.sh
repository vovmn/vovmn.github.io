#!/bin/bash
set -e

BASE_URL="http://localhost:5000"
USERNAME="${TEST_USERNAME:?Set TEST_USERNAME to an admin-provisioned patient}"
PASSWORD="${TEST_PASSWORD:?Set TEST_PASSWORD}"
ASSIGNMENT_ID="${TEST_ASSIGNMENT_ID:?Set TEST_ASSIGNMENT_ID to a patient assignment UUID}"

echo "============================================"
echo "1. Входим под заранее созданным пациентом"
LOGIN=$(curl -s -X POST "$BASE_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d "{
    \"username\": \"$USERNAME\",
    \"password\": \"$PASSWORD\"
  }")

echo "$LOGIN" | jq .
ACCESS_TOKEN=$(echo "$LOGIN" | jq -r '.access_token')
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
echo "3. Начинаем назначенный опросник"
START=$(curl -s -X POST "$BASE_URL/api/questionnaire/assignments/$ASSIGNMENT_ID/start" \
  -H "Authorization: Bearer $ACCESS_TOKEN")
echo "$START" | jq .

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
