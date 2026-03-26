#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
RULES_FILE="$ROOT_DIR/order_here_backend/docs/alert-rules.prometheus.yml"
PAYLOAD_FILE="$ROOT_DIR/order_here_backend/docs/alert-test-payload.json"

required_rules=(
  "OrderHereBackendHigh5xxErrorRateWarning"
  "OrderHereBackendHigh5xxErrorRateCritical"
  "OrderHereBackendReadinessFailing"
  "OrderHereBackendLivenessFailing"
)

for rule in "${required_rules[@]}"; do
  if ! grep -q "$rule" "$RULES_FILE"; then
    echo "Missing alert rule: $rule"
    exit 1
  fi
done

if [[ $# -lt 1 ]]; then
  echo "Usage: bash scripts/test_alert_webhook.sh <webhook-url>"
  exit 1
fi

WEBHOOK_URL="$1"

echo "Posting sample alert payload to webhook..."
status_code="$(
  curl \
    --silent \
    --show-error \
    --output /dev/null \
    --write-out "%{http_code}" \
    -X POST \
    -H "Content-Type: application/json" \
    --data @"$PAYLOAD_FILE" \
    "$WEBHOOK_URL"
)"

if [[ ! "$status_code" =~ ^2 ]]; then
  echo "Webhook did not acknowledge alert payload. HTTP $status_code"
  exit 1
fi

echo "Alert webhook acknowledged payload with HTTP $status_code"
