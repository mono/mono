#!/bin/bash -e

printenv
${TESTCMD} --label=runtime --timeout=160m make -w -C mono/tests -k test-process-stress V=1 CI=1 CI_PR=${ghprbPullId}
