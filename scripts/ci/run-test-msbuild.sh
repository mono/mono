#!/bin/bash

set -u
set -e
# set -x

OLD_CWD=`pwd`
TESTCMD=`realpath ${TESTCMD}`

${TESTCMD} --label=remove-mono-temp-dir --timeout=1m rm -rf /tmp/xplat-master
${TESTCMD} --label=autogen --timeout=15m ./autogen.sh --prefix=/tmp/mono-from-source
${TESTCMD} --label=make --timeout=30m make PROFILE=net_4_x -j 2
${TESTCMD} --label=make-install --timeout=15m make install

OLD_PATH=${PATH}
export PATH=/tmp/mono-from-source/bin/:${OLD_PATH}
${TESTCMD} --label=cert-sync --timeout=15m cert-sync /etc/ssl/certs/ca-certificates.crt

rm -rf /tmp/xplat-master || true

MSBUILD_REVISION=`grep -Po "(?<=revision \= \')[0-9a-f]+" packaging/MacSDK/msbuild.py`
echo Detected msbuild hash ${MSBUILD_REVISION} in MacSDK/msbuild.py

${TESTCMD} --label=clone-msbuild --timeout=5m git clone https://github.com/mono/msbuild.git /tmp/xplat-master
cd /tmp/xplat-master

# fixme: we need to bump macsdk
MSBUILD_REVISION=xplat-master

${TESTCMD} --label=checkout-xplat-master --timeout=5m git checkout ${MSBUILD_REVISION}

${TESTCMD} --label=compile-msbuild --timeout=15m ./eng/cibuild_bootstrapped_msbuild.sh --host_type mono --configuration Release /p:DisableNerdbankVersioning=true /p:CreateBootstrap=true "/p:Projects=/tmp/xplat-master/src/MSBuild.Bootstrap/MSBuild.Bootstrap.csproj" /p:AssemblyVersion=15.1.0.0

MONO=/tmp/mono-from-source/bin/mono
DLL_PATH=`realpath /tmp/xplat-master/artifacts/2/bin/MSBuild.Bootstrap/*-MONO/net472/MSBuild.dll`

${TESTCMD} --label=check-for-dll --timeout=1m test -s ${DLL_PATH}

export PATH=/tmp/xplat-master/artifacts/mono-msbuild:/tmp/mono-from-source/bin:${OLD_PATH}

# fixme: this also requires a bump
sed -i "s/ln -sfh Current/ln -sfn Current/g" ./install-mono-prefix.sh

${TESTCMD} --label=install-msbuild --timeout=2m ./install-mono-prefix.sh /tmp/mono-from-source/

rm -f mcs/class/lib/net_4_x-*/culevel.exe || true

export PATH=/tmp/mono-from-source/bin:${OLD_PATH}

${TESTCMD} --label=try-to-use-installed-msbuild-to-build-culevel --timeout=1m msbuild ${OLD_CWD}/mcs/tools/culevel/culevel.csproj /p:BuildProjectReferences=false

${TESTCMD} --label=check-for-culevel-exe --timeout=1m test -s ${OLD_CWD}/mcs/class/lib/net_4_x-*/culevel.exe
