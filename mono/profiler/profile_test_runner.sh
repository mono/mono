#!/bin/sh
#
# Blackbox test for running test profilers. The success of the
# test depends on successfully running the .exe with loaded
# profiler. Either the C# program or the profiler can fail
# the test.
#

if [ $# -lt 3 ]; then
cat <<EOF
Usage: $0 MONO PROFILE-STRING PROGRAM [PROG-ARGS]
EOF
exit 1;
fi

MONO=${1}
PROFILE_STRING=${2}
PROGRAM=${3}
shift 3
PROGARGS="$*"

echo "Checking $PROGRAM with profiler $PROFILE_STRING ..."
$MONO --profile="$PROFILE_STRING" $PROGRAM $PROGARGS
