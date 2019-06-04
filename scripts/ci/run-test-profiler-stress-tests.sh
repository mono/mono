#!/bin/bash -e

${TESTCMD} --label=profiler --timeout=30m make -w -C mono/profiler -k check
${TESTCMD} --label=Mono.Profiler.Log-xunit --timeout=30m make -w -C mcs/class/Mono.Profiler.Log run-xunit-test
export MONO_BABYSITTER_EXTRA_XML=${MONO_REPO_ROOT}/acceptance-tests/profiler-stress/TestResult-profiler-stress.xml
${TESTCMD} --label=check-profiler-stress --timeout=24h make -C acceptance-tests check-profiler-stress

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
