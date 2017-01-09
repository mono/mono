#!/bin/bash -e

export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

${TESTCMD} --label=interpreter-regression --timeout=10m make -C mono/mini richeck
