# LeadPilot AI — омниканальная CRM

**Коммерческая задача:** единая обработка входящих обращений, управление лидами и контролируемая помощь AI. Независимая портфолио-работа, не заказ клиента.

## Возможности

Организации и роли Owner/Admin/Manager; каналы Telegram, VK, MAX; inbox, назначение ответственного, история переписки, ручной ответ и аудит, карточки контактов, воронка лидов, статистика, отправка лида в amoCRM/Битрикс24. База знаний TXT/MD/PDF до 5 МБ; текстовый поиск работает без LLM. При наличии LLM_API_KEY создаются embeddings `text-embedding-3-small` (1536 измерений), поиск pgvector и RAG-предпросмотр через OpenAI API с указанием источников и ограничением числа запросов. Предпросмотр не отправляет сообщение; оператор явно нажимает «Отправить». Если ключ отсутствует, AI отвечает 503, не имитирует генерацию.

## Технологии, архитектура, структура

ASP.NET Core 10, EF Core, JWT, PostgreSQL + pgvector, Python/FastAPI (AI-service), Next.js 15, TypeScript, Tailwind CSS 4, Docker Compose. `backend/Model.cs`, `backend/Program.cs`, `backend/Integrations.cs`; `backend/Migrations`; `ai-service/main.py`, `ai-service/migrations/001_knowledge.sql`; `frontend/app`; `docs`. Организация определяется по JWT; пользовательские выборки ограничены её идентификатором. Токены каналов зашифрованы AES-GCM с ключом окружения. Входящие сообщения принимаются через webhook.

## Требования, запуск, миграции и seed

Docker Compose, PostgreSQL с pgvector, HTTPS reverse proxy для VPS. `cp .env.example .env`, смените все секреты. `docker compose up -d db`; `docker compose run --build --rm api --migrate`; `docker compose exec -T db psql -U leadpilot -d leadpilot < ai-service/migrations/001_knowledge.sql`; `docker compose up -d --build api ai web`. При DEMO_MODE=true миграция создаёт Студию Север, демо-пользователя и по диалогу от Telegram, VK и MAX. В production DEMO_MODE=false. Для локального запуска укажите `ConnectionStrings__Db`, `JWT_SECRET`, `ENCRYPTION_KEY`, `AI_INTERNAL_TOKEN`; backend `dotnet restore && dotnet build -c Release`; Python `python3 -m compileall -q .`; frontend `npm ci && npm run build`. Docker-образы: `docker compose build` при установленном Docker.

## Переменные и интеграции

`DB_PASSWORD`, `JWT_SECRET`, `ENCRYPTION_KEY`, `AI_INTERNAL_TOKEN`, `WEB_ORIGIN`, `WEB_PORT`, `DEMO_MODE`, `DEMO_EMAIL`, `DEMO_PASSWORD`, `LLM_API_KEY`, `VK_CONFIRMATION_CODE`, `AMO_DOMAIN`, `AMO_ACCESS_TOKEN`, `BITRIX_WEBHOOK_URL`; у backend также `ConnectionStrings__Db`, у AI `DATABASE_URL`. Настройте серверные webhook каналов `/api/webhooks/{channelId}`; `X-Telegram-Bot-Api-Secret-Token` для Telegram и `X-Channel-Secret` для других каналов. На VPS обеспечьте HTTPS и доступ к webhook, ограничьте доступ к БД/AI внутренней Docker-сетью. Секреты не сохраняются в UI после передачи.

## API

`POST /api/auth/register`, `/login`; `GET /api/organizations/me`, `/agents`, `/channels`, `/contacts`, `/conversations`, `/conversations/{id}/messages`, `/leads`, `/knowledge`, `/knowledge/search?q=`, `/analytics`, `/audit`; `PUT /api/organizations/settings`; `POST /api/agents`, `/channels`, `/conversations/{id}/messages`, `/leads`, `/knowledge`, `/ai/preview`, `/integrations/{system}/leads/{id}`; `PATCH /api/conversations/{id}`, `/leads/{id}`; внешний webhook `POST /api/webhooks/{channelId}`. Приватные API требуют Bearer JWT; списки контактов, диалогов, сообщений и лидов выдаются страницами по 50 элементов.
