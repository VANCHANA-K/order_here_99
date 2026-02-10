# Sprint 2 — Engineering Handoff

## System State
System is production-ready for core order flow.

## What Is Safe to Change
- UI / client behavior
- Observability tooling
- Performance tuning

## What Is NOT Safe Without Design Review
- Order lifecycle states
- Persistence model
- Transaction boundaries
- Idempotency logic

## Known Constraints
- In-memory idempotency store (non-distributed)
- SQLite (demo / local scale)

## Next Recommended Steps
- Replace idempotency store with distributed store (Redis)
- Add auth / real JWT validation
- Introduce rate limiting

