#!/bin/sh

SCRIPT_PATH="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
CSHARP=$SCRIPT_PATH/../lib/net_4_5/csharp.exe
MONO=$SCRIPT_PATH/../../../mono/mini/mono

export MONO_PATH=${MONO_PATH:-$SCRIPT_PATH/../lib/net_4_5}

TZ_FAILS=0
TZ_COUNT=0
FORMAT="%a %b %d %T %Y"

for tz in $(cd /usr/share/zoneinfo/; find * -type f -print); do
	TZ_COUNT=$(expr $TZ_COUNT + 1)
	SYS_DATETIME=$(date -ju -f "$FORMAT" "$(TZ=$tz date "+$FORMAT")" "+%s")
	CS_DATETIME=$(TZ=$tz $MONO $CSHARP -e '(int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;')
	DIFF=$(expr $SYS_DATETIME - $CS_DATETIME)
	if [ "$DIFF" -gt "5" ] || [ "$DIFF" -lt "-5" ]; then
		TZ_FAILS=$(expr $TZ_FAILS + 1)
		echo ""
		echo "DateTime.Now failed with timezone: $tz"
		echo "    System:       $(date -ju -f "%s" "$SYS_DATETIME" "+%Y-%m-%d %T")"
		echo "    DateTime.Now: $(date -ju -f "%s" "$CS_DATETIME" "+%Y-%m-%d %T")"
	fi
	echo ".\c"
done
echo ""
echo "DateTime.Now failed with $TZ_FAILS of $TZ_COUNT timezones."
