#!/bin/bash -e
export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

export MONO_SENTRY_ROOT=`dirname "${BASH_SOURCE[0]}/../../"`

if [[ ${CI_TAGS} == *'win-'* ]];
then
	echo "Skipping telemetry phase due to arch"
else
	export MONO_SENTRY_OS="${CI_TAGS}"
	export MONO_SENTRY_URL="https://baaf612e845d407eb5f415c338bb6df9@sentry.io/1314141"
	export MONO_SENTRY_ROOT="$MONO_REPO_ROOT"
	${TESTCMD} --label=sentry-telemetry-upload --timeout=10m make -C mcs/tools/upload-to-sentry upload-crashes
fi
