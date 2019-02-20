#!/bin/bash -e


${TESTCMD} --label=System --timeout=10m make -w -C mcs/class/System run-test

rm -fr /tmp/jenkins-temp-aspnet*

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
