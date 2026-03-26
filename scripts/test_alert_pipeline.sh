#!/usr/bin/env bash

set -euo pipefail

PROMETHEUS_URL="${PROMETHEUS_URL:-http://localhost:9090}"
ALERTMANAGER_URL="${ALERTMANAGER_URL:-http://localhost:9093}"
ALERT_NAME="${1:-OrderHereBackendReadinessFailing}"
TIMEOUT_SECONDS="${TIMEOUT_SECONDS:-180}"
POLL_SECONDS="${POLL_SECONDS:-5}"

echo "Checking that Prometheus rule exists for $ALERT_NAME..."
rules_json="$(curl --silent --show-error "$PROMETHEUS_URL/api/v1/rules")"
if ! printf '%s' "$rules_json" | grep -q "\"name\":\"$ALERT_NAME\""; then
  echo "Alert rule $ALERT_NAME not loaded in Prometheus"
  exit 1
fi

echo "Polling Prometheus for firing alert: $ALERT_NAME"
deadline=$((SECONDS + TIMEOUT_SECONDS))
while (( SECONDS < deadline )); do
  alerts_json="$(curl --silent --show-error "$PROMETHEUS_URL/api/v1/alerts")"
  if printf '%s' "$alerts_json" | grep -q "\"alertname\":\"$ALERT_NAME\""; then
    echo "Alert is firing in Prometheus"
    break
  fi
  sleep "$POLL_SECONDS"
done

if (( SECONDS >= deadline )); then
  echo "Timed out waiting for alert to fire in Prometheus"
  exit 1
fi

echo "Polling Alertmanager for received alert: $ALERT_NAME"
deadline=$((SECONDS + TIMEOUT_SECONDS))
while (( SECONDS < deadline )); do
  groups_json="$(curl --silent --show-error "$ALERTMANAGER_URL/api/v2/alerts/groups")"
  if printf '%s' "$groups_json" | grep -q "\"alertname\":\"$ALERT_NAME\""; then
    echo "Alert received by Alertmanager"
    exit 0
  fi
  sleep "$POLL_SECONDS"
done

echo "Timed out waiting for alert to arrive in Alertmanager"
exit 1
