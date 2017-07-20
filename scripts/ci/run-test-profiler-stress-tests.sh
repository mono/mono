#!/bin/bash -e

export MONO_BABYSITTER_EXTRA_XML=${MONO_REPO_ROOT}/acceptance-tests/profiler-stress/TestResult-profiler-stress.xml

${TESTCMD} --label=check-profiler-stress --timeout=24h make -C acceptance-tests check-profiler-stress
