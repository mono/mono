#!/bin/bash -e

${TESTCMD} --label=check-stress --timeout=12h make -w -C mono/tests -k check-stress V=1

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
