#!/bin/bash -e

${TESTCMD} --label=check-stress --timeout=12h make -w -C mono/tests -k check-stress V=1 CI=1
