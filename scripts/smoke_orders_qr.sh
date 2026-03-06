#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5132}"
TIMEOUT_SECONDS="${TIMEOUT_SECONDS:-30}"

pass_count=0
fail_count=0
skip_count=0

CURRENT_BODY=""
CURRENT_STATUS=""

log() {
  printf '%s\n' "$*"
}

pass() {
  pass_count=$((pass_count + 1))
  printf '[PASS] %s\n' "$*"
}

fail() {
  fail_count=$((fail_count + 1))
  printf '[FAIL] %s\n' "$*"
}

skip() {
  skip_count=$((skip_count + 1))
  printf '[SKIP] %s\n' "$*"
}

require_cmd() {
  if ! command -v "$1" >/dev/null 2>&1; then
    log "Missing required command: $1"
    exit 2
  fi
}

api_call() {
  local method="$1"
  local path="$2"
  local body="${3:-}"
  shift 3 || true

  local url="${BASE_URL}${path}"
  local response

  if [[ -n "$body" ]]; then
    response=$(curl -sS -X "$method" "$url" \
      -H "Content-Type: application/json" \
      "$@" \
      --max-time "$TIMEOUT_SECONDS" \
      --data "$body" \
      -w $'\n%{http_code}')
  else
    response=$(curl -sS -X "$method" "$url" \
      "$@" \
      --max-time "$TIMEOUT_SECONDS" \
      -w $'\n%{http_code}')
  fi

  CURRENT_STATUS="${response##*$'\n'}"
  CURRENT_BODY="${response%$'\n'*}"
}

json_get() {
  local filter="$1"
  jq -r "$filter" <<<"$CURRENT_BODY"
}

ensure_2xx() {
  local test_name="$1"
  if [[ ! "$CURRENT_STATUS" =~ ^2 ]]; then
    fail "$test_name (http=$CURRENT_STATUS, body=$CURRENT_BODY)"
    return 1
  fi
  return 0
}

require_cmd curl
require_cmd jq

log "BASE_URL=$BASE_URL"

# Setup: active table
api_call "GET" "/api/v1/tables" ""
if ! ensure_2xx "Setup: GET /api/v1/tables"; then
  log "Smoke test stopped at setup."
  exit 1
fi

table_id=$(json_get '.[] | select(.status=="Active") | .id' | head -n1)

if [[ -z "$table_id" || "$table_id" == "null" ]]; then
  create_code="SMOKE-$(date +%s)"
  create_payload=$(jq -nc --arg code "$create_code" '{code: $code}')
  api_call "POST" "/api/v1/tables" "$create_payload"

  if ! ensure_2xx "Setup: POST /api/v1/tables"; then
    log "Smoke test stopped at setup."
    exit 1
  fi

  table_id=$(json_get '.id')
fi

if [[ -z "$table_id" || "$table_id" == "null" ]]; then
  fail "Setup: Could not determine active tableId"
  exit 1
fi

log "Using tableId=$table_id"

# Setup: menu
api_call "GET" "/api/v1/tables/${table_id}/menu" ""
if ! ensure_2xx "Setup: GET /api/v1/tables/{tableId}/menu"; then
  log "Smoke test stopped at setup."
  exit 1
fi

available_ids=()
while IFS= read -r id; do
  [[ -n "$id" && "$id" != "null" ]] && available_ids+=("$id")
done < <(json_get '.[] | select(.isAvailable==true) | .id')

unavailable_id=$(json_get '.[] | select(.isAvailable==false) | .id' | head -n1)

if [[ ${#available_ids[@]} -lt 2 ]]; then
  fail "Setup: Need at least 2 available menu items (found ${#available_ids[@]})"
  exit 1
fi

item1="${available_ids[0]}"
item2="${available_ids[1]}"

# Test 1: Happy Path
idem1="smoke-happy-$(date +%s)-$RANDOM"
payload_happy=$(jq -nc \
  --arg tableId "$table_id" \
  --arg i1 "$item1" \
  --arg i2 "$item2" \
  --arg key "$idem1" \
  '{tableId:$tableId, items:[{menuItemId:$i1, quantity:1},{menuItemId:$i2, quantity:1}], idempotencyKey:$key}')

api_call "POST" "/api/v1/orders/qr" "$payload_happy"
if ensure_2xx "Test 1 - create order request"; then
  order_id_t1=$(json_get '.orderId')
  status_t1=$(json_get '.status')

  if [[ -n "$order_id_t1" && "$order_id_t1" != "null" && "$status_t1" == "PENDING" ]]; then
    pass "Test 1 - Happy Path (orderId=$order_id_t1, status=$status_t1)"
  else
    fail "Test 1 - Happy Path (unexpected response body=$CURRENT_BODY)"
  fi
fi

# Test 2: Double Tap (same idempotency key, 3 requests)
idem2="smoke-doubletap-$(date +%s)-$RANDOM"
payload_double=$(jq -nc \
  --arg tableId "$table_id" \
  --arg i1 "$item1" \
  --arg i2 "$item2" \
  --arg key "$idem2" \
  '{tableId:$tableId, items:[{menuItemId:$i1, quantity:1},{menuItemId:$i2, quantity:2}], idempotencyKey:$key}')

ids=()
statuses=()
for n in 1 2 3; do
  api_call "POST" "/api/v1/orders/qr" "$payload_double"
  statuses+=("$CURRENT_STATUS")
  if [[ "$CURRENT_STATUS" =~ ^2 ]]; then
    ids+=("$(json_get '.orderId')")
  else
    ids+=("")
  fi
done

if [[ "${statuses[0]}" =~ ^2 && "${statuses[1]}" =~ ^2 && "${statuses[2]}" =~ ^2 && -n "${ids[0]}" && "${ids[0]}" == "${ids[1]}" && "${ids[1]}" == "${ids[2]}" ]]; then
  pass "Test 2 - Double Tap (same orderId returned for all 3 requests: ${ids[0]})"
else
  fail "Test 2 - Double Tap (statuses=${statuses[*]}, orderIds=${ids[*]})"
fi

# Test 3: Network Slow simulation (API-level)
idem3="smoke-slow-$(date +%s)-$RANDOM"
payload_slow=$(jq -nc \
  --arg tableId "$table_id" \
  --arg i1 "$item1" \
  --arg key "$idem3" \
  '{tableId:$tableId, items:[{menuItemId:$i1, quantity:1}], idempotencyKey:$key}')

start_s=$(date +%s)
response=$(curl -sS -X POST "${BASE_URL}/api/v1/orders/qr" \
  -H "Content-Type: application/json" \
  --limit-rate 8k \
  --max-time "$TIMEOUT_SECONDS" \
  --data "$payload_slow" \
  -w $'\n%{http_code}')
end_s=$(date +%s)
CURRENT_STATUS="${response##*$'\n'}"
CURRENT_BODY="${response%$'\n'*}"
elapsed_s=$((end_s - start_s))

if [[ "$CURRENT_STATUS" =~ ^2 ]]; then
  pass "Test 3 - Slow Network simulation (http=$CURRENT_STATUS, elapsed=${elapsed_s}s)"
else
  fail "Test 3 - Slow Network simulation (http=$CURRENT_STATUS, body=$CURRENT_BODY)"
fi

# Test 4: Invalid Item (ITEM_UNAVAILABLE)
if [[ -z "$unavailable_id" || "$unavailable_id" == "null" ]]; then
  skip "Test 4 - Invalid Item (no isAvailable=false menu item in current data)"
else
  idem4="smoke-unavailable-$(date +%s)-$RANDOM"
  payload_unavailable=$(jq -nc \
    --arg tableId "$table_id" \
    --arg unavailable "$unavailable_id" \
    --arg key "$idem4" \
    '{tableId:$tableId, items:[{menuItemId:$unavailable, quantity:1}], idempotencyKey:$key}')

  api_call "POST" "/api/v1/orders/qr" "$payload_unavailable"

  error_code=""
  if [[ -n "$CURRENT_BODY" ]]; then
    error_code=$(jq -r '.errorCode // ""' <<<"$CURRENT_BODY" 2>/dev/null || true)
  fi

  if [[ ! "$CURRENT_STATUS" =~ ^2 && "$error_code" == "ITEM_UNAVAILABLE" ]]; then
    pass "Test 4 - Invalid Item returns ITEM_UNAVAILABLE"
  else
    fail "Test 4 - Invalid Item (http=$CURRENT_STATUS, errorCode=$error_code, body=$CURRENT_BODY)"
  fi
fi

printf '\nSummary: pass=%d fail=%d skip=%d\n' "$pass_count" "$fail_count" "$skip_count"

if [[ "$fail_count" -gt 0 ]]; then
  exit 1
fi

exit 0
