#!/bin/bash -e

OLD_CWD=`pwd`
TESTCMD=`realpath ${TESTCMD}`

${TESTCMD} --label=remove-mono-temp-dir --timeout=1m rm -rf /tmp/xplat-master
${TESTCMD} --label=autogen --timeout=15m ./autogen.sh --prefix=/tmp/mono-from-source
${TESTCMD} --label=make --timeout=30m make PROFILE=net_4_x -j 2
${TESTCMD} --label=make-install --timeout=15m make install

OLD_PATH=${PATH}
export PATH=/tmp/mono-from-source/bin/:${OLD_PATH}
${TESTCMD} --label=cert-sync --timeout=15m cert-sync /etc/ssl/certs/ca-certificates.crt

${TESTCMD} --label=remove-msbuild-temp-dir --timeout=1m rm -rf /tmp/xplat-master
${TESTCMD} --label=clone-msbuild --timeout=5m git clone https://github.com/mono/msbuild.git /tmp/xplat-master
cd /tmp/xplat-master
${TESTCMD} --label=checkout-xplat-master --timeout=5m git checkout xplat-master

${TESTCMD} --label=compile-msbuild --timeout=15m ./eng/cibuild_bootstrapped_msbuild.sh --host_type mono --configuration Release /p:DisableNerdbankVersioning=true "/p:Projects=/tmp/xplat-master/src/MSBuild.Bootstrap/MSBuild.Bootstrap.csproj" /p:AssemblyVersion=15.1.0.0

${TESTCMD} --label=check-for-dll --timeout=1m test -s /tmp/xplat-master/artifacts/bin/MSBuild/*-MONO/net472/MSBuild.dll
