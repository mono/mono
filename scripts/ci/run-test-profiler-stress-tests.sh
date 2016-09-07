#!/bin/bash -e

export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

${TESTCMD} --label=check-profiler-stress --timeout=20h make -C acceptance-tests check-profiler-stress
