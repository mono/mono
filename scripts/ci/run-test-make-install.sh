#!/bin/bash -e

${TESTCMD} --label=make-install --timeout=30m make -w install
mkdir -p ${MONO_REPO_ROOT}/tmp/
echo "class H{static void Main(){System.Console.WriteLine(\"ok\");}}" > ${MONO_REPO_ROOT}/tmp/hello.cs
export LD_LIBRARY_PATH=${MONO_REPO_ROOT}/tmp/monoprefix/lib:${LD_LIBRARY_PATH}
${TESTCMD} --label=check-prefix-mcs --timeout=1m ${MONO_REPO_ROOT}/tmp/monoprefix/bin/mcs /out:${MONO_REPO_ROOT}/tmp/hello.exe ${MONO_REPO_ROOT}/tmp/hello.cs 
${TESTCMD} --label=check-prefix-roslyn --timeout=1m ${MONO_REPO_ROOT}/tmp/monoprefix/bin/csc /out:${MONO_REPO_ROOT}/tmp/hello.exe ${MONO_REPO_ROOT}/tmp/hello.cs
${TESTCMD} --label=check-prefix-aot --timeout=1m ${MONO_REPO_ROOT}/tmp/monoprefix/bin/mono --aot ${MONO_REPO_ROOT}/tmp/hello.exe
${TESTCMD} --label=check-prefix-llvmaot --timeout=1m ${MONO_REPO_ROOT}/tmp/monoprefix/bin/mono --aot=llvm,llvm-path=/usr/lib/mono/llvm/bin ${MONO_REPO_ROOT}/tmp/hello.exe
${TESTCMD} --label=check-prefix-llvmjit --timeout=1m ${MONO_REPO_ROOT}/tmp/monoprefix/bin/mono --llvm ${MONO_REPO_ROOT}/tmp/hello.exe

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
