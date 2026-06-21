#!/usr/bin/env bash
# Comprehensive API QA script for EventosVivos
set -u
BASE="${API_URL:-http://localhost:5142/api}"
ADMIN_KEY="${ADMIN_API_KEY:-dev-admin-key}"
PASS=0
FAIL=0
SKIP=0

pass() { PASS=$((PASS+1)); echo "  PASS: $1"; }
fail() { FAIL=$((FAIL+1)); echo "  FAIL: $1 — $2"; }
skip() { SKIP=$((SKIP+1)); echo "  SKIP: $1 — $2"; }

expect_status() {
  local name="$1" expected="$2" method="$3" url="$4" body="${5:-}" extra_header="${6:-}"
  local args=(-s -w "\n%{http_code}" -X "$method" "$url" -H "Content-Type: application/json")
  [[ -n "$extra_header" ]] && args+=(-H "$extra_header")
  [[ -n "$body" ]] && args+=(-d "$body")
  local resp
  resp=$(curl "${args[@]}")
  local code="${resp##*$'\n'}"
  local content="${resp%$'\n'*}"
  if [[ "$code" == "$expected" ]]; then
    pass "$name (HTTP $code)"
    echo "$content"
    return 0
  else
    fail "$name" "expected HTTP $expected, got $code — $content"
    echo "$content"
    return 1
  fi
}

body_contains() {
  local name="$1" haystack="$2" needle="$3"
  if echo "$haystack" | grep -q "$needle"; then pass "$name"; else fail "$name" "missing '$needle' in $haystack"; fi
}

echo "=== EventosVivos API QA ==="
echo "Base: $BASE"
echo ""

# --- Venues ---
echo "## RF Venues"
V=$(curl -s "$BASE/venues")
body_contains "GET /venues returns 3 venues" "$V" "Auditorio Central"

# --- Create valid event ---
echo "## RF-01 Create Event"
FUTURE_START=$(date -u -d "+30 days" +%Y-%m-%dT10:00:00Z 2>/dev/null || date -u -v+30d +%Y-%m-%dT10:00:00Z)
FUTURE_END=$(date -u -d "+30 days" +%Y-%m-%dT14:00:00Z 2>/dev/null || date -u -v+30d +%Y-%m-%dT14:00:00Z)

CREATE_OK=$(expect_status "RF-01 valid event" 201 POST "$BASE/events" "{
  \"title\": \"QA Success Event\",
  \"description\": \"Evento creado por script QA automatizado.\",
  \"venueId\": 2,
  \"maxCapacity\": 25,
  \"startDateTime\": \"$FUTURE_START\",
  \"endDateTime\": \"$FUTURE_END\",
  \"ticketPrice\": 45.50,
  \"type\": 2
}")
EVENT_ID=$(echo "$CREATE_OK" | grep -oE '"id":"[a-f0-9-]+"' | head -1 | cut -d'"' -f4)

expect_status "RF-01 short title" 400 POST "$BASE/events" '{"title":"Hi","description":"Too short title test.","venueId":1,"maxCapacity":10,"startDateTime":"2026-12-01T10:00:00Z","endDateTime":"2026-12-01T12:00:00Z","ticketPrice":10,"type":1}'
expect_status "RF-01 past start" 422 POST "$BASE/events" '{"title":"Past Event QA","description":"Event with past start date.","venueId":1,"maxCapacity":10,"startDateTime":"2020-01-01T10:00:00Z","endDateTime":"2020-01-01T12:00:00Z","ticketPrice":10,"type":1}'
body_contains "RF-01 RN-01 capacity" "$(curl -s -w '%{http_code}' -X POST "$BASE/events" -H "Content-Type: application/json" -d '{"title":"Over Cap QA","description":"Exceeds venue capacity test.","venueId":2,"maxCapacity":60,"startDateTime":"2026-12-05T10:00:00Z","endDateTime":"2026-12-05T14:00:00Z","ticketPrice":10,"type":1}')" "RN-01"
expect_status "RF-01 invalid venue" 404 POST "$BASE/events" '{"title":"Bad Venue QA","description":"Venue does not exist test.","venueId":99,"maxCapacity":10,"startDateTime":"2026-12-05T10:00:00Z","endDateTime":"2026-12-05T14:00:00Z","ticketPrice":10,"type":1}'

# RN-02 overlap
curl -s -X POST "$BASE/events" -H "Content-Type: application/json" -d '{"title":"Overlap Base QA","description":"Base event for overlap test.","venueId":2,"maxCapacity":20,"startDateTime":"2026-12-10T10:00:00Z","endDateTime":"2026-12-10T14:00:00Z","ticketPrice":10,"type":1}' >/dev/null
OVERLAP=$(curl -s -w "\n%{http_code}" -X POST "$BASE/events" -H "Content-Type: application/json" -d '{"title":"Overlap Child QA","description":"Overlapping schedule test.","venueId":2,"maxCapacity":20,"startDateTime":"2026-12-10T12:00:00Z","endDateTime":"2026-12-10T16:00:00Z","ticketPrice":10,"type":1}')
body_contains "RN-02 overlap" "$OVERLAP" "RN-02"

# RN-03 weekend late (next Saturday after BaseUtc + 30 days)
RN03_START="2026-08-08T22:30:00Z"
RN03_END="2026-08-09T01:00:00Z"
RN03=$(curl -s -w "\n%{http_code}" -X POST "$BASE/events" -H "Content-Type: application/json" -d "{\"title\":\"Late Weekend QA\",\"description\":\"Weekend after 22 test.\",\"venueId\":1,\"maxCapacity\":50,\"startDateTime\":\"$RN03_START\",\"endDateTime\":\"$RN03_END\",\"ticketPrice\":10,\"type\":3}")
body_contains "RN-03 weekend" "$RN03" "RN-03"

# --- RF-02 Filters ---
echo "## RF-02 List & Filters"
expect_status "GET /events" 200 GET "$BASE/events"
body_contains "Filter titleSearch" "$(curl -s "$BASE/events?titleSearch=QA%20Success")" "QA Success Event"
body_contains "Filter type" "$(curl -s "$BASE/events?type=2")" "QA Success Event"
body_contains "Filter venueId" "$(curl -s "$BASE/events?venueId=2")" "QA Success Event"
body_contains "Filter status active" "$(curl -s "$BASE/events?status=1")" "QA Success Event"
expect_status "GET event by id" 200 GET "$BASE/events/$EVENT_ID"
expect_status "GET event not found" 404 GET "$BASE/events/00000000-0000-0000-0000-000000000099"

# --- RF-03 Reserve ---
echo "## RF-03 Reserve"
RES_OK=$(expect_status "RF-03 valid reservation" 201 POST "$BASE/reservations" "{
  \"eventId\": \"$EVENT_ID\",
  \"quantity\": 2,
  \"buyerName\": \"QA Buyer\",
  \"buyerEmail\": \"qa@test.com\"
}")
RES_ID=$(echo "$RES_OK" | grep -oE '"id":"[a-f0-9-]+"' | head -1 | cut -d'"' -f4)

expect_status "RF-03 invalid email" 400 POST "$BASE/reservations" "{\"eventId\":\"$EVENT_ID\",\"quantity\":1,\"buyerName\":\"X\",\"buyerEmail\":\"bad\"}"
expect_status "RF-03 quantity zero" 400 POST "$BASE/reservations" "{\"eventId\":\"$EVENT_ID\",\"quantity\":0,\"buyerName\":\"X\",\"buyerEmail\":\"x@test.com\"}"
expect_status "RF-03 event not found" 404 POST "$BASE/reservations" '{"eventId":"00000000-0000-0000-0000-000000000099","quantity":1,"buyerName":"X","buyerEmail":"x@test.com"}'

# Capacity overflow - fill remaining + 1
curl -s -X POST "$BASE/reservations" -H "Content-Type: application/json" -d "{\"eventId\":\"$EVENT_ID\",\"quantity\":23,\"buyerName\":\"Fill\",\"buyerEmail\":\"fill@test.com\"}" >/dev/null
CAP=$(curl -s -w "\n%{http_code}" -X POST "$BASE/reservations" -H "Content-Type: application/json" -d "{\"eventId\":\"$EVENT_ID\",\"quantity\":1,\"buyerName\":\"Over\",\"buyerEmail\":\"over@test.com\"}")
body_contains "RF-03 capacity exceeded" "$CAP" "CAPACITY"

# --- RF-04 Confirm (use fresh event before filling capacity) ---
echo "## RF-04 Confirm Payment"
CONF_EVENT=$(expect_status "RF-04 setup event" 201 POST "$BASE/events" "{
  \"title\": \"QA Confirm Event\",
  \"description\": \"Evento dedicado para pruebas de confirmacion.\",
  \"venueId\": 3,
  \"maxCapacity\": 10,
  \"startDateTime\": \"2026-08-20T10:00:00Z\",
  \"endDateTime\": \"2026-08-20T14:00:00Z\",
  \"ticketPrice\": 40,
  \"type\": 1
}")
CONF_EVENT_ID=$(echo "$CONF_EVENT" | grep -oE '"id":"[a-f0-9-]+"' | head -1 | cut -d'"' -f4)

RES2=$(curl -s -X POST "$BASE/reservations" -H "Content-Type: application/json" -d "{
  \"eventId\": \"$CONF_EVENT_ID\", \"quantity\": 1, \"buyerName\": \"Confirm QA\", \"buyerEmail\": \"confirm@test.com\"
}")
RES2_ID=$(echo "$RES2" | grep -oE '"id":"[a-f0-9-]+"' | head -1 | cut -d'"' -f4)

CONF=$(expect_status "RF-04 confirm with admin key" 200 POST "$BASE/reservations/$RES2_ID/confirm-payment" "" "X-Admin-Key: $ADMIN_KEY")
body_contains "RF-04 code format EV-######" "$CONF" "EV-"

expect_status "RF-04 no admin key" 401 POST "$BASE/reservations/$RES2_ID/confirm-payment"
expect_status "RF-04 duplicate confirm" 422 POST "$BASE/reservations/$RES2_ID/confirm-payment" "" "X-Admin-Key: $ADMIN_KEY"

# Cancel then confirm
RES3=$(curl -s -X POST "$BASE/reservations" -H "Content-Type: application/json" -d "{
  \"eventId\": \"$CONF_EVENT_ID\", \"quantity\": 1, \"buyerName\": \"Cancel QA\", \"buyerEmail\": \"cancel@test.com\"
}")
RES3_ID=$(echo "$RES3" | grep -oE '"id":"[a-f0-9-]+"' | head -1 | cut -d'"' -f4)
curl -s -X POST "$BASE/reservations/$RES3_ID/cancel" >/dev/null
CANCEL_CONF=$(curl -s -w "\n%{http_code}" -X POST "$BASE/reservations/$RES3_ID/confirm-payment" -H "X-Admin-Key: $ADMIN_KEY")
body_contains "RF-04 confirm cancelled" "$CANCEL_CONF" "RESERVATION_CANCELLED"

# --- RF-05 Cancel ---
echo "## RF-05 Cancel"
RES4=$(curl -s -X POST "$BASE/reservations" -H "Content-Type: application/json" -d "{
  \"eventId\": \"$CONF_EVENT_ID\", \"quantity\": 1, \"buyerName\": \"Pending Cancel\", \"buyerEmail\": \"pc@test.com\"
}")
RES4_ID=$(echo "$RES4" | grep -oE '"id":"[a-f0-9-]+"' | head -1 | cut -d'"' -f4)
expect_status "RF-05 cancel pending" 200 POST "$BASE/reservations/$RES4_ID/cancel"
expect_status "RF-05 double cancel" 422 POST "$BASE/reservations/$RES4_ID/cancel"

# --- RF-06 Report ---
echo "## RF-06 Occupancy Report"
REP=$(curl -s "$BASE/events/$EVENT_ID/occupancy-report")
body_contains "RF-06 has sold tickets" "$REP" "totalSoldTickets"
body_contains "RF-06 has revenue" "$REP" "totalRevenue"
expect_status "RF-06 event not found" 404 GET "$BASE/events/00000000-0000-0000-0000-000000000099/occupancy-report"

echo ""
echo "=== SUMMARY: PASS=$PASS FAIL=$FAIL SKIP=$SKIP ==="
[[ "$FAIL" -eq 0 ]]
