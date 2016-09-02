#!/bin/bash -e

export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

${TESTCMD} --label=corlib --timeout=30m make -w -C mcs/class/corlib run-test
