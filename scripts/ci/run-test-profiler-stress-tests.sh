#!/bin/bash -e

${TESTCMD} --label=check-profiler-stress --timeout=24h make -C acceptance-tests check-profiler-stress
