================================================================================
FILE 1 OF 3 — db_connection.conf
Oracle Instant Client environment setup and DB credentials.
Usage: source ~/db_connection.conf
Note: chmod 600 this file — contains credentials.
================================================================================

export ORACLE_HOME= #/path/to/oracle/instantclient
export LD_LIBRARY_PATH=$ORACLE_HOME
export PATH=$ORACLE_HOME:$PATH

# Fill in credentials before deploying
export DB_USER=      # schema username
export DB_PASS=      # password
export DB_CONN=      # host:port/service



================================================================================
FILE 2 OF 3 — run_reconciliation.sh
Runs the Oracle stored procedure run_main_reconciliation.
Scheduled via cron — see FILE 3.
================================================================================

#!/bin/bash
set -euo pipefail

CONF_FILE="${HOME}/db_connection.conf"

if [[ ! -f "$CONF_FILE" ]]; then
    echo "[ERROR] Config file not found: $CONF_FILE" >&2
    exit 1
fi

source "$CONF_FILE"

echo "[INFO] $(date '+%Y-%m-%d %H:%M:%S') — Starting reconciliation run"

# -s: silent mode (suppresses SQL*Plus banner)
"$ORACLE_HOME/sqlplus" -s "$DB_USER/$DB_PASS@$DB_CONN" <<EOF
EXEC run_main_reconciliation;
EXIT;
EOF

echo "[INFO] $(date '+%Y-%m-%d %H:%M:%S') — Reconciliation run completed"




================================================================================
FILE 3 OF 3 — crontab entry
Runs at 02:00 AM on the 27th, 28th, 30th, and 31st — covers month-end for all month lengths.
Requires: crontab -e  |  Verify: crontab -l  |  Logs: tail -f /recon_cron.log
Note: Replace /path/to/ with the actual script path on the server.
================================================================================
# m  h  dom          mon  dow  command
  0  2  27,28,30,31  *    *    /path/to/run_reconciliation.sh >> /recon_cron.log 2>&1
