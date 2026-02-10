# Sprint 2 — Day 6: Release Checklist

## Build & CI
- [ ] CI pipeline green
- [ ] No skipped tests
- [ ] Release build succeeds locally

## API Safety
- [ ] All endpoints return X-Trace-Id
- [ ] Error contract consistent
- [ ] No stack trace leakage

## Resilience
- [ ] CreateOrder idempotent
- [ ] AddItem idempotent
- [ ] Retry bounded
- [ ] Cancellation safe

## Observability
- [ ] TraceId correlated in logs
- [ ] OrderId visible in logs
- [ ] Errors searchable by code

## Config
- [ ] Production config present
- [ ] No debug-only flags enabled

## Go / No-Go
- [ ] All above checked → GO

