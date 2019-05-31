#!/bin/bash -e

${TESTCMD} --label=make --timeout=30m make PROFILE=net_4_x ENABLE_COMPILER_SERVER=0

${TESTCMD} --label=verify-built-with-compiler-server-disabled --timeout=1m test -e mcs/class/lib/net_4_x-*/culevel.exe
${TESTCMD} --label=remove-culevel --timeout=1m rm mcs/class/lib/net_4_x-*/culevel.exe
${TESTCMD} --label=remove-log --timeout=1m rm -f mcs/build/compiler-server.log

${TESTCMD} --label=test-build-culevel-with-compiler-server --timeout=2m make PROFILE=net_4_x ENABLE_COMPILER_SERVER=1

${TESTCMD} --label=verify-build-produced-exe --timeout=1m test -s mcs/class/lib/net_4_x-*/culevel.exe
${TESTCMD} --label=verify-log-created --timeout=1m test -s mcs/build/compiler-server.log
${TESTCMD} --label=verify-csc-out-in-log --timeout=1m grep -- "\-out\:.*culevel.exe" mcs/build/compiler-server.log
${TESTCMD} --label=verify-return-code-0-in-log --timeout=1m grep -- "Return code: 0" mcs/build/compiler-server.log