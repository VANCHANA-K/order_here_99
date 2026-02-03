# Sprint 2 — Regression Checklist (Day 1 Draft)

## Smoke (Sprint 1 baseline)
- [ ] Staff: Create table
- [ ] Staff: Generate QR
- [ ] Customer: Resolve QR (menu + table)
- [ ] Customer: Create order -> status PENDING
- [ ] Kitchen: Get orders by status
- [ ] Kitchen: Update order status

## Retry / Duplicate (Sprint 2 focus)
- [ ] CreateOrder retry (same Idempotency-Key) => MUST return same OrderId
- [ ] CreateOrder retry (different Idempotency-Key) => MUST create new OrderId
- [ ] Double-click submit => no duplicate effect
- [ ] AddItem retry => no duplicate effect

## Error & Safety
- [ ] Invalid payload (qty=0) => INVALID_REQUEST (400)
- [ ] Domain rule violation => DOMAIN_RULE_VIOLATION (400)
- [ ] Not found => RESOURCE_NOT_FOUND (404)
- [ ] Concurrency conflict => CONCURRENCY_CONFLICT (409)
- [ ] Unexpected => INTERNAL_ERROR (500) and no stack trace leak

## Observability
- [ ] Every response has X-Trace-Id
- [ ] TraceId can be found in server logs
- [ ] TraceId can be matched in audit log (if enabled)

## Expected Outputs
- All checks pass without manual workaround
- No duplicate data
- Error responses are predictable (code/message/traceId)
