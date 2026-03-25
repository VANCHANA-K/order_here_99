# API Error Semantics

Every non-success API error returns the same JSON shape:

```json
{
  "errorCode": "ORDER_NOT_FOUND",
  "message": "Order not found",
  "traceId": "4f3d2c1b0a9e87654321fedcba098765"
}
```

Fields:

- `errorCode` — stable machine-readable code
- `message` — human-readable summary
- `traceId` — request correlation id for logs and troubleshooting

## Status Code Policy

- `400 Bad Request`
  - request payload is invalid
  - model binding or DTO validation fails
  - business input is invalid but the resource state is not in conflict
- `404 Not Found`
  - requested resource does not exist
  - unmatched route returns `ENDPOINT_NOT_FOUND`
- `409 Conflict`
  - request is valid, but current resource state prevents the action
- `500 Internal Server Error`
  - unexpected unhandled failure
  - internal exception details must not leak to clients

## Error Code Convention

- Use `SCREAMING_SNAKE_CASE`
- Prefer specific resource and action codes over generic ones
- Keep the same `errorCode` whether validation fails in model binding or handler logic
- Reserved generic fallback codes:
  - `INVALID_REQUEST`
  - `ENDPOINT_NOT_FOUND`
  - `UNEXPECTED_ERROR`

## Common Error Codes

Application-level examples:

- `ORDER_NOT_FOUND`
- `TABLE_NOT_FOUND`
- `QR_NOT_FOUND`
- `TABLE_ID_REQUIRED`
- `REQUEST_BODY_REQUIRED`
- `INVALID_JSON`
- `MENU_ITEM_ID_REQUIRED`
- `PRODUCT_NAME_REQUIRED`
- `UNIT_PRICE_INVALID`
- `EMPTY_ITEMS`
- `INVALID_QTY`
- `ITEM_UNAVAILABLE`
- `TABLE_CODE_ALREADY_EXISTS`
- `QR_TOKEN_ALREADY_EXISTS`
- `MENU_CODE_ALREADY_EXISTS`

Domain-level examples:

- `ORDER_NOT_OPEN`
- `ORDER_ALREADY_COMPLETED`
- `TABLE_CODE_REQUIRED`
- `TABLE_ALREADY_INACTIVE`
- `TABLE_ALREADY_ACTIVE`
- `TABLE_INACTIVE`
- `CURRENCY_MISMATCH`

## Response Consistency Rules

- All error responses use `ApiErrorResponse`
- Resource `404`s must use specific codes such as `ORDER_NOT_FOUND`, not generic `NOT_FOUND`
- Validation failures must not use `VALIDATION_ERROR`
- Model binding and handler validation should return the same `errorCode` for the same invalid condition
- `INVALID_REQUEST` exists only as a final fallback when no specific validation mapping applies
- Swagger should expose `ApiErrorResponse` plus examples for documented error responses
- Swagger should expose success examples for documented primary responses

## Request DTO Rule

Every new request DTO must implement the same validation pattern:

1. Add explicit annotations on the DTO in `src/QrFoodOrdering.Api/Contracts`.
2. Add or update a specific rule in `src/QrFoodOrdering.Api/Validation/ModelValidationErrorMapper.cs`.
3. Add at least one integration test for an invalid payload in `tests/QrFoodOrdering.IntegrationTests`.

Use specific validation semantics whenever possible:

- missing or empty value -> `*_REQUIRED`
- malformed type or conversion failure -> `*_INVALID`
- malformed body or wrong JSON shape -> `INVALID_JSON`
- use `INVALID_REQUEST` only when no specific mapping is possible

## Current Coverage

Integration tests currently verify:

- model binding validation errors
- request-body-required and invalid-JSON cases
- domain and application conflict errors
- resource not found errors
- unmatched route errors
- unexpected unhandled errors
- Swagger error examples in `/swagger/v1/swagger.json`
- Swagger success examples in `/swagger/v1/swagger.json`
