#!/bin/bash -e

# ${TESTCMD} --label=runtime --timeout=160m make -w -C mono/tests -k testllvmonlyinterp V=1

scripts/ci/run-upload-sentry.sh
