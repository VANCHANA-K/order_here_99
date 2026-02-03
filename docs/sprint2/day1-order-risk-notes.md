# Sprint 2 — Day 1: Order Lifecycle & Risk Notes

viic@IVIC-MAC qr-food-ordering-platform % git rev-parse HEAD
33c2c11744a4a4a2372a0c9f7b0315d4c7191bd4

## Sprint 2 Hard Rules (Scope Lock)
- ❌ No new endpoint
- ❌ No schema change
- ❌ No new order state
- ❌ No feature add
- ❌ No refactor ที่ไม่จำเป็นต่อ hardening

## Purpose (Day 1)
Day 1 ไม่ได้สร้าง feature ใหม่
แต่ทำให้ baseline “เชื่อถือได้” และเห็น risk ของ order ชัดเจน

## Current Order Lifecycle (as-is)
> TODO: confirm from code today (read-only)

- Created
- Paid
- Closed
- Cancelled (allowed only before Paid)

## Idempotency Points (must harden)
- CreateOrder
  - Risk: retry/refresh/double submit creates duplicate order
  - Desired: same Idempotency-Key => same OrderId

## Failure Points (production-like)
- Retry from client/network
- Double submit (click twice)
- Race condition on status update
- Partial save (transaction not atomic)

## What we will NOT do in Sprint 2
- No new endpoints
- No domain rule changes
- No schema changes
- No new order states
