#!/bin/bash -e

${TESTCMD} --label=bockbuild --timeout=180m --fatal ${MONO_REPO_ROOT}/scripts/mac-sdk-package.sh

# switch to using package Mono instead of system
export PATH=${MONO_REPO_ROOT}/external/bockbuild/stage/bin:$PATH

# Bundled MSBuild
cd ${MONO_REPO_ROOT}/external/bockbuild/builds/msbuild-15/
${TESTCMD} --label="msbuild-tests" --timeout=180m ./eng/cibuild_bootstrapped_msbuild.sh --host_type mono --configuration Release
zip ${MONO_REPO_ROOT}/msbuild-test-results.zip artifacts/2/TestResults/Release-MONO/* artifacts/2/log/Release-MONO/*.log

# Bundled LLVM
cd ${MONO_REPO_ROOT}/external/bockbuild/builds/mono
${TESTCMD} --label="runtime-tests-llvm" --timeout=240m make -C mono/tests -k test-wrench MONO_ENV_OPTIONS=--llvm V=1 CI=1 M=1
${TESTCMD} --label="corlib-tests-llvm" --timeout=60m make -C mcs/class/corlib run-test PLATFORM_TEST_HARNESS_EXCLUDES=NotOnMac,MacNotWorking,LLVMNotWorking, MONO_ENV_OPTIONS=--llvm

# Bundled libgdiplus
${TESTCMD} --label="System.Drawing" --timeout=60m make -C mcs/class/System.Drawing run-test
