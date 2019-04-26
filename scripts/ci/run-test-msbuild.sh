#!/bin/bash -e

OLD_CWD=`pwd`
TESTCMD=`realpath ${TESTCMD}`

${TESTCMD} --label=make --timeout=30m make PROFILE=net_4_x

${TESTCMD} --label=remove-msbuild-temp-dir --timeout=1m rm -rf /tmp/xplat-master
${TESTCMD} --label=clone-msbuild --timeout=5m git clone https://github.com/mono/msbuild.git /tmp/xplat-master
cd /tmp/xplat-master
${TESTCMD} --label=checkout-xplat-master --timeout=5m git checkout xplat-master
mkdir /tmp/xplat-master/tmp-links
ln -T -s ${OLD_CWD}/runtime/mono-wrapper /tmp/xplat-master/tmp-links/mono

OLD_PATH=${PATH}
export PATH=/tmp/xplat-master/tmp-links/:${OLD_CWD}:${PATH}

# This fails for some reason due to weird bugs in the msbuild infra, and I can't suppress it using testcmd?
${TESTCMD} --label=compile-msbuild --timeout=15m ./eng/cibuild_bootstrapped_msbuild.sh --host_type mono --configuration Release --binaryLog --skip_tests /p:DisableNerdbankVersioning=true "/p:Projects=/tmp/xplat-master/src/MSBuild/MSBuild.csproj" /p:AssemblyVersion=15.1.0.0

${TESTCMD} --label=check-for-dll --timeout=1m test -s /tmp/xplat-master/artifacts/bin/MSBuild/*-MONO/net472/MSBuild.dll
