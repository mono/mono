#!/bin/bash

set -u
set -e
# set -x

OLD_CWD=`pwd`
TESTCMD=`realpath ${TESTCMD}`

rm -rf /tmp/xplat-master || true
rm -rf /tmp/mono-from-source || true

${TESTCMD} --label=make-install --timeout=15m make install

OLD_PATH=${PATH}
export PATH=/tmp/mono-from-source/bin/:${OLD_PATH}

# hack: execution environment may not have certificates, which breaks nuget. roslyn and msbuild both need nuget.
# ${TESTCMD} --label=cert-sync --timeout=15m /tmp/mono-from-source/bin/cert-sync /etc/ssl/certs/ca-certificates.crt

# we bump the OS X version of msbuild by modifying msbuild.py, so we want to pull that revision hash out
#  and match it here so we are verifying the same revision mac users get
MSBUILD_REVISION=`grep -Po "(?<=revision \= \')[0-9a-f]+" packaging/MacSDK/msbuild.py`
echo Detected msbuild hash ${MSBUILD_REVISION} in MacSDK/msbuild.py

${TESTCMD} --label=clone-msbuild --timeout=5m git clone https://github.com/mono/msbuild.git /tmp/xplat-master
cd /tmp/xplat-master

XPLAT_MASTER_REVISION=`git rev-parse xplat-master`
echo Detected msbuild hash ${XPLAT_MASTER_REVISION} in mono/msbuild branch xplat-master

COMPARISON="\"${MSBUILD_REVISION}\" = \"${XPLAT_MASTER_REVISION}\""

# fixme: the xplat-master and macsdk hashes don't match right now
#${TESTCMD} --label=compare-hashes --timeout=5m test ${COMPARISON}

# hack: we can test against latest xplat-master but we don't want to do that on ci
#MSBUILD_REVISION=${XPLAT_MASTER_REVISION}

${TESTCMD} --label=checkout-xplat-master --timeout=5m git checkout ${MSBUILD_REVISION}

# DisableNerdbankVersioning: Nerdbank relies on libgit2, which relies on libssl1.0.0, which is extremely old and has been dropped from some distributions. Newer libssl is not binary compatible so fixing this will require an update to both libgit2 and nerdbank. libssl1.0.0 was removed for security reasons (AFAIK) so getting it back is a bad idea.
# CreateBootstrap: Building without this means some important configuration files will be missing
# Projects: Building regular MSBuild.csproj appears to work but also leaves out important files. Building the whole sln will compile the automated test suite and other tools which may fail for reasons that are unimportant to us.
# AssemblyVersion: Nerdbank is responsible for setting assembly versions, so without setting one here you may get errors due to a default version of 42.42.42.42. Suggested by Rainer Sigwald

${TESTCMD} --label=compile-msbuild --timeout=15m ./eng/cibuild_bootstrapped_msbuild.sh --host_type mono --configuration Release /p:DisableNerdbankVersioning=true /p:CreateBootstrap=true "/p:Projects=/tmp/xplat-master/src/MSBuild.Bootstrap/MSBuild.Bootstrap.csproj" /p:AssemblyVersion=15.1.0.0

# hack: this is useful for debugging some of the errors that will occur if there are assembly load bugs
#  in the mono runtime, issues with msbuild's compile-time dependencies, or runtime dependencies
#export MONO_LOG_MASK=asm
#export MONO_LOG_LEVEL=debug

MONO=/tmp/mono-from-source/bin/mono
# using artifacts/2/bin instead of artifacts/bin here is important because it is produced by later steps
#  in the build process. A build may appear to succeed by having a /bin which will hide problems and result
#  in confusing errors
DLL_PATH=`realpath /tmp/xplat-master/artifacts/2/bin/MSBuild.Bootstrap/*-MONO/net472/MSBuild.dll`

${TESTCMD} --label=check-for-dll --timeout=1m test -s ${DLL_PATH}

# BuildProjectReferences: Without doing this culevel will trigger build of mscorlib.dll etc which may
#  fail and is unnecessary given that make created its dependencies earlier.
${TESTCMD} --label=try-to-use-built-msbuild-to-build-culevel --timeout=1m ${MONO} ${DLL_PATH} ${OLD_CWD}/mcs/tools/culevel/culevel.csproj /p:BuildProjectReferences=false
 
${TESTCMD} --label=check-for-culevel-exe-1 --timeout=1m test -s ${OLD_CWD}/mcs/class/lib/net_4_x-*/culevel.exe

# install-mono-prefix below needs a 'msbuild' so we add the bootstrap msbuild folder into the PATH
#  in front of the folder we installed our prefix into. Suggested by Ankit Jain
export PATH=/tmp/xplat-master/artifacts/mono-msbuild:/tmp/mono-from-source/bin:${OLD_PATH}

# hack: this is fixed in latest xplat-master but we haven't bumped macsdk.
sed -i "s/ln -sfh Current/ln -sfn Current/g" mono/build/install.proj

${TESTCMD} --label=install-msbuild --timeout=2m ./install-mono-prefix.sh /tmp/mono-from-source/

rm -f mcs/class/lib/net_4_x-*/culevel.exe || true

export PATH=/tmp/mono-from-source/bin:${OLD_PATH}

# BuildProjectReferences: See above
# We are building using both msbuild-direct-from-outdir and msbuild-installed-to-prefix because
#  each scenario can reveal different bugs and configuration issues. We care about any failures in
#  either scenario.
${TESTCMD} --label=try-to-use-installed-msbuild-to-build-culevel --timeout=1m msbuild ${OLD_CWD}/mcs/tools/culevel/culevel.csproj /p:BuildProjectReferences=false

${TESTCMD} --label=check-for-culevel-exe-2 --timeout=1m test -s ${OLD_CWD}/mcs/class/lib/net_4_x-*/culevel.exe
