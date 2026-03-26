# Alert Rules

This backend now exposes health endpoints suitable for alerting:

- `GET /health/live`
- `GET /health/ready`

Recommended alert coverage:

1. Error rate
- alert on sustained `5xx` rate above threshold
- optional warning on unusually high `4xx` rate if it indicates bad client rollout or abuse

2. Health
- alert immediately when readiness fails
- alert immediately when liveness fails

3. Database availability
- readiness failures should be treated as a dependency incident because readiness is backed by the database health check

## Recommended thresholds

### 5xx error rate
- warning: `5xx` ratio > `2%` for `5m`
- critical: `5xx` ratio > `5%` for `5m`

### Readiness
- critical: readiness probe failing for `2m`

### Liveness
- critical: liveness probe failing for `2m`

## Metrics assumptions

The sample rules in [`alert-rules.prometheus.yml`](/Users/viic/Desktop/order_here/order_here_backend/docs/alert-rules.prometheus.yml) assume one of these metric sources exists:

- ingress / reverse proxy request metrics
- Prometheus ASP.NET Core HTTP metrics
- blackbox probe metrics for `/health/live` and `/health/ready`

If metric names differ in your environment, keep the thresholds and semantics but adapt the metric selectors.

## Alert semantics

- `liveness` means the process is alive and should usually trigger restart automation
- `readiness` means the app should receive traffic; failures usually indicate dependency or startup problems
- `5xx error rate` means requests are reaching the app but failing too often

## Routing recommendation

- warning alerts -> Slack / team channel
- critical alerts -> paging / on-call

## Probe targets

- liveness target: `/health/live`
- readiness target: `/health/ready`

## Notes

- `/health` remains a compatibility endpoint, but operational alerting should use `/health/live` and `/health/ready`
- readiness is database-backed through `DatabaseHealthCheck`

## Fire & Receive Test

Use the sample payload and webhook test script to verify alert delivery end-to-end:

```bash
cd /Users/viic/Desktop/order_here
bash scripts/test_alert_webhook.sh https://your-alert-receiver.example/webhook
```

Artifacts:

- sample payload: [`alert-test-payload.json`](/Users/viic/Desktop/order_here/order_here_backend/docs/alert-test-payload.json)
- webhook test script: [`/Users/viic/Desktop/order_here/scripts/test_alert_webhook.sh`](/Users/viic/Desktop/order_here/scripts/test_alert_webhook.sh)

The script:

- verifies required alert rules exist
- posts a firing readiness alert payload
- fails unless the receiver returns `2xx`

## Monitoring Stack Files

Sample monitoring stack files are included here:

- Prometheus config: [`monitoring/prometheus.yml`](/Users/viic/Desktop/order_here/order_here_backend/docs/monitoring/prometheus.yml)
- Blackbox config: [`monitoring/blackbox.yml`](/Users/viic/Desktop/order_here/order_here_backend/docs/monitoring/blackbox.yml)
- Alertmanager config: [`monitoring/alertmanager.yml`](/Users/viic/Desktop/order_here/order_here_backend/docs/monitoring/alertmanager.yml)
- Docker Compose stack: [`monitoring/docker-compose.monitoring.yml`](/Users/viic/Desktop/order_here/order_here_backend/docs/monitoring/docker-compose.monitoring.yml)

These files provide the exact external dependency config required for readiness/liveness alerts.

## End-to-End Alert Test

To verify a real alert path in a monitoring environment:

1. Start the monitoring stack from `docs/monitoring`
2. Induce a real failure, for example:
   - stop the backend process to trip liveness
   - make the database unavailable to trip readiness
3. Run:

```bash
cd /Users/viic/Desktop/order_here
bash scripts/test_alert_pipeline.sh OrderHereBackendReadinessFailing
```

This test:

- checks the rule is loaded in Prometheus
- waits until the alert is firing in Prometheus
- waits until the alert is visible in Alertmanager

Unlike the webhook replay test, this verifies the actual monitoring pipeline.
