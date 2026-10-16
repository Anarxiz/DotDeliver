# DotDeliver

Учебный pet-проект — backend для сервиса доставки еды на ASP.NET Core. Сделан, чтобы попрактиковаться в построении бэкенда уровня production-ready: чистая архитектура, CQRS, аутентификация, тестирование и контейнеризация.

## Возможности

- Регистрация и вход по JWT, смена пароля, обновление профиля
- Ролевая авторизация (`User` / `Administrator`) — создание/изменение категорий и блюд доступно только администраторам
- CRUD по категориям, блюдам и адресам доставки
- Оформление заказа с проверкой доступности блюд и принадлежности адреса пользователю
- Изменение статуса заказа администратором
- Rate limiting на `login`/`register` (защита от перебора)
- Output caching для списков категорий и блюд с инвалидацией по тегам при изменении данных
- Health checks (проверка доступности PostgreSQL) — `/health`
- Структурированное логирование (Serilog: консоль + файлы с ежедневной ротацией)
- Интерактивная документация API (Scalar / OpenAPI) с поддержкой Bearer-авторизации прямо в UI

## Технологии

ASP.NET Core 9 (Minimal API) · C# 13 · PostgreSQL · Entity Framework Core · Mediator (CQRS) · FluentValidation · JWT Bearer · Serilog · Docker / Docker Compose · xUnit · Testcontainers

## Архитектура

Проект разбит на слои с чёткой инверсией зависимостей:

- **Web.Domain** — сущности, enum'ы, общий `Result`/`Result<T>` для обработки ошибок без исключений
- **Web.Application** — бизнес-логика: CQRS-команды/запросы (Mediator), DTO, валидаторы, сервисы, интерфейсы репозиториев
- **Web.Infrastructure** — доступ к данным: EF Core, `DbContext`, конфигурации сущностей, репозитории, миграции
- **Web.API** — HTTP-слой: minimal API эндпоинты, аутентификация/авторизация, middleware (rate limiting, output caching, exception handling), OpenAPI
- **Web.API.Tests** — интеграционные тесты (`WebApplicationFactory` + Testcontainers c реальным PostgreSQL)

Ошибки бизнес-логики не кидаются как исключения, а возвращаются через `Result`/`Result<T>` и конвертируются в `ProblemDetails` (RFC 9457) на уровне эндпоинтов.

## Как запустить

### Через Docker Compose (проще всего)

```bash
docker compose up --build
```

Поднимет API (`http://localhost:8080`) и PostgreSQL. Секрет для JWT можно переопределить переменной окружения `JWT_SECRET_KEY` (по умолчанию используется плейсхолдер).

### Локально (.NET SDK)

Нужен .NET 9 SDK и PostgreSQL.

```bash
cp Web.API/appsettingsExample.json Web.API/appsettings.json
# поправить ConnectionStrings:ShopDbContext под свою БД

dotnet ef database update -p Web.Infrastructure -s Web.API
dotnet run --project Web.API
```

После запуска в Development-режиме доступна интерактивная документация API: `/scalar/v1`.

## Тесты

```bash
dotnet test
```

Интеграционные тесты поднимают настоящий PostgreSQL в Docker через Testcontainers — для их запуска нужен работающий Docker.

Для отчёта о покрытии кода:

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:coveragereport -reporttypes:Html
```
