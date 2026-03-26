#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

TMP_FILE="$(mktemp)"
cleanup() {
  rm -f "$TMP_FILE"
}
trap cleanup EXIT

echo "Scanning repository for hard-coded secrets..."

rg -n -i --hidden \
  --glob '!**/bin/**' \
  --glob '!**/obj/**' \
  --glob '!**/.next/**' \
  --glob '!**/node_modules/**' \
  --glob '!**/dist/**' \
  --glob '!**/coverage/**' \
  --glob '!**/*.db' \
  --glob '!**/*.sqlite' \
  --glob '!**/*.Designer.cs' \
  --glob '!**/package-lock.json' \
  --glob '!**/.git/**' \
  --glob '!scripts/scan_no_hardcoded_secrets.sh' \
  '(BEGIN [A-Z ]*PRIVATE KEY|ghp_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|sk-[A-Za-z0-9_-]{20,}|xox[baprs]-[A-Za-z0-9-]{10,}|AKIA[0-9A-Z]{16}|aws_secret_access_key|(?:(?:api[_-]?key|client[_-]?secret|secret|password|passwd|pwd)\s*[:=]\s*["'"'"'][^"'"'"']{8,}["'"'"'])|(?:connectionstrings?(?::|__)?default\s*[:=]\s*["'"'"'][^"'"'"']*(?:password|pwd)=)|(?:Server=.*;Database=.*;(?:User Id|Uid)=.*;(?:Password|Pwd)=))' \
  . > "$TMP_FILE" || true

if [[ ! -s "$TMP_FILE" ]]; then
  echo "No hard-coded secret patterns found."
  exit 0
fi

echo "Potential hard-coded secrets detected:"
cat "$TMP_FILE"
exit 1
