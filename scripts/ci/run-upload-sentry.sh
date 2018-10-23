#!/bin/bash -e
export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

export MONO_SENTRY_ROOT=`dirname "${BASH_SOURCE[0]}/../../"`

if [[ ${CI_TAGS} == *'win-'* ]];
then
	echo "Skipping telemetry phase due to arch"
elif [[ -n "${MONO_SENTRY_URL}" ]]
then
	# Define MONO_SENTRY_URL and MONO_SENTRY_OS in environment
	${TESTCMD} --label=sentry-telemetry-upload --timeout=10m make -C mcs/tools/upload-to-sentry upload-crashes MONO_SENTRY_ROOT="$MONO_REPO_ROOT"
else
	echo "Skipping telemetry phase because URL missing"
fi
