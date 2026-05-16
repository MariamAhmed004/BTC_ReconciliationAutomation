================================================================================
FILE 1 OF 3 — check_schedule.sh
Heartbeat script — runs every minute via cron (see FILE 3).
Reads run_time and day_of_month from system_configuration, then fires
run_main_reconciliation only when the current day and time match.
================================================================================

#!/bin/bash
set -euo pipefail

# Load Oracle environment and DB credentials
. ~/db_connection.conf

CURRENT_TIME=$(date +%H:%M)
CURRENT_DAY=$(date +%d)

# Pull the latest active schedule from the DB (run_time, day_of_month)
CONFIG=$($ORACLE_HOME/sqlplus -s "$DB_USER/$DB_PASS@$DB_CONN" <<EOF
SET HEADING OFF FEEDBACK OFF PAGESIZE 0 VERIFY OFF
SELECT run_time || ',' || day_of_month
FROM "system_configuration"
WHERE is_active = 'Y'
ORDER BY created_at DESC
FETCH FIRST 1 ROW ONLY;
EXIT;
EOF
)

RUN_TIME=$(echo $CONFIG | cut -d',' -f1)
DAY_OF_MONTH=$(echo $CONFIG | cut -d',' -f2)

# Cutoff: only run if scheduled time is at least this many minutes away
CUTOFF=30

NOW_SEC=$(date +%s)
RUN_SEC=$(date -d "$(date +%Y-%m-%d) $RUN_TIME" +%s)
DIFF=$(( (RUN_SEC - NOW_SEC) / 60 ))

if [[ "$CURRENT_DAY" == "$DAY_OF_MONTH" && "$CURRENT_TIME" == "$RUN_TIME" && $DIFF -ge $CUTOFF ]]; then
  echo "[INFO] $(date '+%Y-%m-%d %H:%M:%S') — Conditions met, running reconciliation"
  $ORACLE_HOME/sqlplus -s "$DB_USER/$DB_PASS@$DB_CONN" <<EOF
  EXEC run_main_reconciliation;
  EXIT;
EOF
else
  echo "[INFO] $(date '+%Y-%m-%d %H:%M:%S') — Conditions not met, skipping"
fi


================================================================================
FILE 2 OF 3 — db_connection.conf
Oracle Instant Client environment setup and DB credentials.
Usage: source ~/db_connection.conf
Note: chmod 600 this file — contains credentials.
================================================================================

export ORACLE_HOME=/home/maryamahmed004/oracle/instantclient_21_21
export LD_LIBRARY_PATH=$ORACLE_HOME
export PATH=$ORACLE_HOME:$PATH

export DB_USER=billing_mock
export DB_PASS=billing123
export DB_CONN=172.30.32.1:1521/XEPDB1


================================================================================
FILE 3 OF 3 — crontab entry
Runs check_schedule.sh every minute — the script itself decides whether to act.
Requires: crontab -e  |  Verify: crontab -l  |  Logs: tail -f /home/maryamahmed004/dynamic_cron.log
================================================================================

# m  h  dom  mon  dow  command
  *  *  *    *    *    /home/maryamahmed004/check_schedule.sh >> /home/maryamahmed004/dynamic_cron.log 2>&1
