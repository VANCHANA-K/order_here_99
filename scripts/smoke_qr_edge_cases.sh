#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5132}"
DB_PATH="${DB_PATH:-/Users/viic/Desktop/order_here/order_here_backend/src/QrFoodOrdering.Api/qrfood.dev.db}"
TIMEOUT_SECONDS="${TIMEOUT_SECONDS:-20}"

pass_count=0
fail_count=0
skip_count=0

CURRENT_BODY=""
CURRENT_STATUS=""

log() { printf '%s\n' "$*"; }
pass() { pass_count=$((pass_count + 1)); printf '[PASS] %s\n' "$*"; }
fail() { fail_count=$((fail_count + 1)); printf '[FAIL] %s\n' "$*"; }
skip() { skip_count=$((skip_count + 1)); printf '[SKIP] %s\n' "$*"; }

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
  local url="${BASE_URL}${path}"
  local response

  if [[ -n "$body" ]]; then
    response=$(curl -sS -X "$method" "$url" \
      -H "Content-Type: application/json" \
      --max-time "$TIMEOUT_SECONDS" \
      --data "$body" \
      -w $'\n%{http_code}')
  else
    response=$(curl -sS -X "$method" "$url" \
      --max-time "$TIMEOUT_SECONDS" \
      -w $'\n%{http_code}')
  fi

  CURRENT_STATUS="${response##*$'\n'}"
  CURRENT_BODY="${response%$'\n'*}"
}

json_get() { jq -r "$1" <<<"$CURRENT_BODY"; }

expect_error() {
  local test_name="$1"
  local expected_http="$2"
  local expected_code="$3"
  local actual_code

  if [[ -n "$CURRENT_BODY" ]]; then
    actual_code=$(jq -r '.errorCode // ""' <<<"$CURRENT_BODY" 2>/dev/null || true)
  else
    actual_code=""
  fi

  if [[ "$CURRENT_STATUS" == "$expected_http" && "$actual_code" == "$expected_code" ]]; then
    pass "$test_name"
  else
    fail "$test_name (http=$CURRENT_STATUS code=$actual_code body=$CURRENT_BODY)"
  fi
}

require_cmd curl
require_cmd jq
require_cmd sqlite3

if [[ ! -f "$DB_PATH" ]]; then
  log "DB file not found: $DB_PATH"
  exit 2
fi

log "BASE_URL=$BASE_URL"
log "DB_PATH=$DB_PATH"

# Setup: choose active table
api_call "GET" "/api/v1/tables" ""
if [[ ! "$CURRENT_STATUS" =~ ^2 ]]; then
  log "Setup failed: GET /api/v1/tables (http=$CURRENT_STATUS body=$CURRENT_BODY)"
  exit 1
fi

table_id=$(json_get '.[] | select(.status=="Active") | .id' | head -n1)
if [[ -z "$table_id" || "$table_id" == "null" ]]; then
  log "Setup failed: no active table"
  exit 1
fi

# Setup: generate QR
api_call "POST" "/api/v1/tables/${table_id}/qr" "{}"
if [[ ! "$CURRENT_STATUS" =~ ^2 ]]; then
  log "Setup failed: POST /api/v1/tables/{id}/qr (http=$CURRENT_STATUS body=$CURRENT_BODY)"
  exit 1
fi

token=$(json_get '.token')
if [[ -z "$token" || "$token" == "null" ]]; then
  log "Setup failed: could not read token"
  exit 1
fi

read -r orig_qr_active orig_qr_expires orig_qr_table_id < <(sqlite3 "$DB_PATH" "SELECT IsActive, ExpiresAtUtc, TableId FROM qr_codes WHERE Token='${token}' LIMIT 1;")
read -r orig_table_active < <(sqlite3 "$DB_PATH" "SELECT IsActive FROM tables WHERE Id='${table_id}' LIMIT 1;")

if [[ -z "${orig_qr_active:-}" || -z "${orig_qr_expires:-}" || -z "${orig_qr_table_id:-}" ]]; then
  log "Setup failed: QR row not found in sqlite"
  exit 1
fi

if [[ -z "${orig_table_active:-}" ]]; then
  log "Setup failed: table row not found in sqlite"
  exit 1
fi

restore_state() {
  sqlite3 "$DB_PATH" "UPDATE qr_codes SET IsActive=${orig_qr_active}, ExpiresAtUtc='${orig_qr_expires}', TableId='${orig_qr_table_id}' WHERE Token='${token}';"
  sqlite3 "$DB_PATH" "UPDATE tables SET IsActive=${orig_table_active} WHERE Id='${table_id}';"
}
trap restore_state EXIT

# Case 1: Valid QR
api_call "GET" "/api/v1/qr/${token}" ""
if [[ "$CURRENT_STATUS" =~ ^2 ]] && [[ "$(json_get '.tableId // ""')" == "$table_id" ]]; then
  pass "Valid QR returns table info"
else
  fail "Valid QR returns table info (http=$CURRENT_STATUS body=$CURRENT_BODY)"
fi

# Case 2: Expired QR
sqlite3 "$DB_PATH" "UPDATE qr_codes SET IsActive=1, ExpiresAtUtc='2020-01-01T00:00:00.0000000Z' WHERE Token='${token}';"
api_call "GET" "/api/v1/qr/${token}" ""
expect_error "Expired QR -> QR_EXPIRED" "409" "QR_EXPIRED"

# Case 3: Inactive QR
sqlite3 "$DB_PATH" "UPDATE qr_codes SET IsActive=0, ExpiresAtUtc='2030-01-01T00:00:00.0000000Z' WHERE Token='${token}';"
api_call "GET" "/api/v1/qr/${token}" ""
expect_error "Inactive QR -> QR_INACTIVE" "409" "QR_INACTIVE"

# Case 4: Table inactive
sqlite3 "$DB_PATH" "UPDATE qr_codes SET IsActive=1, ExpiresAtUtc='2030-01-01T00:00:00.0000000Z', TableId='${table_id}' WHERE Token='${token}';"
sqlite3 "$DB_PATH" "UPDATE tables SET IsActive=0 WHERE Id='${table_id}';"
api_call "GET" "/api/v1/qr/${token}" ""
expect_error "Table inactive -> TABLE_INACTIVE" "409" "TABLE_INACTIVE"
sqlite3 "$DB_PATH" "UPDATE tables SET IsActive=1 WHERE Id='${table_id}';"

# Case 5: Table not found
fake_table_id="00000000-0000-0000-0000-000000000099"
sqlite3 "$DB_PATH" "UPDATE qr_codes SET IsActive=1, ExpiresAtUtc='2030-01-01T00:00:00.0000000Z', TableId='${fake_table_id}' WHERE Token='${token}';"
api_call "GET" "/api/v1/qr/${token}" ""
expect_error "Table not found -> TABLE_NOT_FOUND" "404" "TABLE_NOT_FOUND"

# Case 6: Missing/invalid token format (whitespace)
api_call "GET" "/api/v1/qr/%20" ""
expect_error "Blank token -> QR_INVALID" "400" "QR_INVALID"

# Case 7: Random token not found
api_call "GET" "/api/v1/qr/not-found-${RANDOM}-${RANDOM}" ""
expect_error "Random token -> QR_NOT_FOUND" "404" "QR_NOT_FOUND"

printf '\nSummary: pass=%d fail=%d skip=%d\n' "$pass_count" "$fail_count" "$skip_count"

if [[ "$fail_count" -gt 0 ]]; then
  exit 1
fi

exit 0
