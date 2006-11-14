#!/bin/bash

PREFIX=/work/asgard
MONO=${PREFIX}/INSTALL/bin/mono

${MONO} --debug ${PREFIX}/mono/mcs/class/lib/net_2_0/compiler-tester.exe positive 'martin-test-*.cs' ${PREFIX}/mcs/gmcs/gmcs.exe known-issues martin.log

