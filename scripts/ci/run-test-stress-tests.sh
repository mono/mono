#!/bin/bash -e

${TESTCMD} --label=check-stress --timeout=12h make -C mono/tests check-stress
