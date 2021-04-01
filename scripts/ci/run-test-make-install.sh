#!/bin/bash -e

${TESTCMD} --label=make-install --timeout=30m make -w install
echo "class H { static void Main () { System.Console.WriteLine (\"Hello World: \" + System.DateTime.Now); } }" > ${MONO_REPO_ROOT}/tmp/hello.cs

MONO_PREFIX=${MONO_REPO_ROOT}/tmp/monoprefix
export DYLD_FALLBACK_LIBRARY_PATH=$MONO_PREFIX/lib:$DYLD_LIBRARY_FALLBACK_PATH
export LD_LIBRARY_PATH=$MONO_PREFIX/lib:$LD_LIBRARY_PATH
export PATH=$MONO_PREFIX/bin:$PATH

${TESTCMD} --label=check-prefix-mcs --timeout=1m mcs /out:${MONO_REPO_ROOT}/tmp/hello.exe ${MONO_REPO_ROOT}/tmp/hello.cs 
${TESTCMD} --label=check-prefix-roslyn --timeout=1m csc /out:${MONO_REPO_ROOT}/tmp/hello.exe ${MONO_REPO_ROOT}/tmp/hello.cs
${TESTCMD} --label=check-prefix-aot --timeout=1m mono --aot ${MONO_REPO_ROOT}/tmp/hello.exe
${TESTCMD} --label=check-prefix-llvmaot --timeout=1m mono --aot=llvm,llvm-path=${MONO_REPO_ROOT}/llvm/usr/bin ${MONO_REPO_ROOT}/tmp/hello.exe
${TESTCMD} --label=check-prefix-llvmjit --timeout=1m mono --llvm ${MONO_REPO_ROOT}/tmp/hello.exe
