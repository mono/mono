#!/bin/bash -e

${TESTCMD} --label=mcs-tests --timeout=30m make -w -C mcs/tests run-test
${TESTCMD} --label=mcs-errors --timeout=10m make -w -C mcs/errors run-test
${TESTCMD} --label=compile-bcl-tests --timeout=40m make -w -C runtime test

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
