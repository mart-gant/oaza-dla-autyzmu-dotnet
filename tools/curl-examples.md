# Curl examples for Forum API

## Get all categories
curl -k "https://localhost:7115/api/v1/forum/categories"

## Get all topics
curl -k "https://localhost:7115/api/v1/forum/topics"

## Get topics by category (categoryId=1)
curl -k "https://localhost:7115/api/v1/forum/topics?categoryId=1"

## Create topic (requires Authorization header)
curl -k -X POST "https://localhost:7115/api/v1/forum/topics" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{"categoryId":1,"title":"Nowy temat testowy","content":"To jest treść testowa tematu."}'

## Create category (Admin)
curl -k -X POST "https://localhost:7115/api/v1/forum/categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <ADMIN_TOKEN>" \
  -d '{"name":"NowaKategoriaTest","description":"Opis testowy","sortOrder":10}'
