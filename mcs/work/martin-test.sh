#!/bin/bash

MONO=/work/gondor/INSTALL/bin/mono
PREFIX=/work/gondor/mono

${MONO} --debug ${PREFIX}/mcs/class/lib/net_2_0/compiler-tester.exe positive 'martin-test-*.cs' ${PREFIX}/mcs/gmcs/gmcs.exe known-issues martin.log

