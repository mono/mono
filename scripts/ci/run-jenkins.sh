#!/bin/bash -e

TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

export TEST_HARNESS_VERBOSE=1

if [[ ${label} == 'osx-i386' ]]; then EXTRA_CONF_FLAGS="--with-libgdiplus=/Library/Frameworks/Mono.framework/Versions/Current/lib/libgdiplus.dylib --enable-nls=no --build=i386-apple-darwin11.2.0"; fi
if [[ ${label} == 'osx-amd64' ]]; then EXTRA_CONF_FLAGS="--with-libgdiplus=/Library/Frameworks/Mono.framework/Versions/Current/lib/libgdiplus.dylib --enable-nls=no"; fi
if [[ ${label} == 'w32' ]]; then PLATFORM=Win32; EXTRA_CONF_FLAGS="--host=i686-pc-mingw32"; fi
if [[ ${label} == 'w64' ]]; then PLATFORM=x64; EXTRA_CONF_FLAGS="--host=i686-pc-mingw32"; fi

if [[ ${label} == w* ]]; then
	export MONO_EXECUTABLE="`cygpath -u ${WORKSPACE}\\\msvc\\\\${PLATFORM}\\\bin\\\Release_SGen\\\mono-sgen.exe`"
	# without this dir in PATH the P/Invoke tests in mono/tests/ won't find the MSVC built libtest.dll
	export PATH="$PATH:`cygpath -u ${WORKSPACE}\\\msvc\\\\${PLATFORM}\\\bin\\\Release`"
fi

if [[ ${label} != w* ]] && [[ ${label} != 'debian-ppc64el' ]] && [[ ${label} != 'centos-s390x' ]];
    then
    EXTRA_CONF_FLAGS="$EXTRA_CONF_FLAGS --with-monodroid --with-monotouch --with-monotouch_watch --with-monotouch_tv --with-xammac --with-mobile_static"
    # only enable the mobile profiles and mobile_static on the main architectures
fi

${TESTCMD} --label=configure --timeout=60m --fatal ./autogen.sh $EXTRA_CONF_FLAGS
if [[ ${label} == w* ]];
    then
    ${TESTCMD} --label=make-msvc --timeout=60m --fatal /cygdrive/c/Program\ Files\ \(x86\)/MSBuild/12.0/Bin/MSBuild.exe /p:Platform=${PLATFORM} /p:Configuration=Release msvc/mono.sln
    ${TESTCMD} --label=make-msvc-sgen --timeout=60m --fatal /cygdrive/c/Program\ Files\ \(x86\)/MSBuild/12.0/Bin/MSBuild.exe /p:Platform=${PLATFORM} /p:Configuration=Release_SGen msvc/mono.sln
fi
${TESTCMD} --label=make --timeout=300m --fatal make -w V=1
if [[ -n "${ghprbPullId}" ]] && [[ ${label} == w* ]];
    then
    exit 0
    # we don't run the test suite on Windows PRs, we just ensure the build succeeds, so end here
fi
${TESTCMD} --label=mini --timeout=5m make -w -C mono/mini -k check
${TESTCMD} --label=runtime --timeout=120m make -w -C mono/tests -k test-wrench V=1 CI=1
${TESTCMD} --label=corlib --timeout=30m make -w -C mcs/class/corlib run-test
${TESTCMD} --label=verify --timeout=15m make -w -C runtime mcs-compileall
${TESTCMD} --label=profiler --timeout=30m make -w -C mono/profiler -k check
${TESTCMD} --label=compiler --timeout=30m make -w -C mcs/tests run-test
${TESTCMD} --label=compiler-errors --timeout=30m make -w -C mcs/errors run-test
${TESTCMD} --label=System --timeout=10m make -w -C mcs/class/System run-test
${TESTCMD} --label=System.XML --timeout=5m make -w -C mcs/class/System.XML run-test
${TESTCMD} --label=Mono.Security --timeout=5m make -w -C mcs/class/Mono.Security run-test
${TESTCMD} --label=System.Security --timeout=5m make -w -C mcs/class/System.Security run-test
${TESTCMD} --label=System.Drawing --timeout=5m make -w -C mcs/class/System.Drawing run-test
if [[ ${label} == osx-* ]]
then ${TESTCMD} --label=Windows.Forms --skip
else ${TESTCMD} --label=Windows.Forms --timeout=5m make -w -C mcs/class/System.Windows.Forms run-test
fi
${TESTCMD} --label=System.Data --timeout=5m make -w -C mcs/class/System.Data run-test
${TESTCMD} --label=System.Data.OracleClient --timeout=5m make -w -C mcs/class/System.Data.OracleClient run-test
${TESTCMD} --label=System.Design --timeout=5m make -w -C mcs/class/System.Design run-test
${TESTCMD} --label=Mono.Posix --timeout=5m make -w -C mcs/class/Mono.Posix run-test
${TESTCMD} --label=System.Web --timeout=30m make -w -C mcs/class/System.Web run-test
${TESTCMD} --label=System.Web.Services --timeout=5m make -w -C mcs/class/System.Web.Services run-test
${TESTCMD} --label=System.Runtime.SFS --timeout=5m make -w -C mcs/class/System.Runtime.Serialization.Formatters.Soap run-test
${TESTCMD} --label=System.Runtime.Remoting --timeout=5m make -w -C mcs/class/System.Runtime.Remoting run-test
${TESTCMD} --label=Cscompmgd --timeout=5m make -w -C mcs/class/Cscompmgd run-test
${TESTCMD} --label=Commons.Xml.Relaxng --timeout=5m make -w -C mcs/class/Commons.Xml.Relaxng run-test
${TESTCMD} --label=System.ServiceProcess --timeout=5m make -w -C mcs/class/System.ServiceProcess run-test
${TESTCMD} --label=I18N.CJK --timeout=5m make -w -C mcs/class/I18N/CJK run-test
${TESTCMD} --label=I18N.West --timeout=5m make -w -C mcs/class/I18N/West run-test
${TESTCMD} --label=I18N.MidEast --timeout=5m make -w -C mcs/class/I18N/MidEast run-test
${TESTCMD} --label=System.DirectoryServices --timeout=5m make -w -C mcs/class/System.DirectoryServices run-test
${TESTCMD} --label=Microsoft.Build.Engine --timeout=5m make -w -C mcs/class/Microsoft.Build.Engine run-test
${TESTCMD} --label=Microsoft.Build.Framework --timeout=5m make -w -C mcs/class/Microsoft.Build.Framework run-test
${TESTCMD} --label=Microsoft.Build.Tasks --timeout=5m make -w -C mcs/class/Microsoft.Build.Tasks run-test
${TESTCMD} --label=Microsoft.Build.Utilities --timeout=5m make -w -C mcs/class/Microsoft.Build.Utilities run-test
${TESTCMD} --label=Mono.C5 --timeout=5m make -w -C mcs/class/Mono.C5 run-test
${TESTCMD} --label=System.Configuration --timeout=5m make -w -C mcs/class/System.Configuration run-test
${TESTCMD} --label=System.Transactions --timeout=5m make -w -C mcs/class/System.Transactions run-test
${TESTCMD} --label=System.Web.Extensions --timeout=5m make -w -C mcs/class/System.Web.Extensions run-test
${TESTCMD} --label=System.Core --timeout=15m make -w -C mcs/class/System.Core run-test
${TESTCMD} --label=symbolicate --timeout=60m make -w -C mcs/tools/mono-symbolicate check
${TESTCMD} --label=System.Xml.Linq --timeout=5m make -w -C mcs/class/System.Xml.Linq run-test
${TESTCMD} --label=System.Data.DSE --timeout=5m make -w -C mcs/class/System.Data.DataSetExtensions run-test
${TESTCMD} --label=System.Web.Abstractions --timeout=5m make -w -C mcs/class/System.Web.Abstractions run-test
${TESTCMD} --label=System.Web.Routing --timeout=5m make -w -C mcs/class/System.Web.Routing run-test
${TESTCMD} --label=System.Runtime.Serialization --timeout=5m make -w -C mcs/class/System.Runtime.Serialization run-test
${TESTCMD} --label=System.IdentityModel --timeout=5m make -w -C mcs/class/System.IdentityModel run-test
${TESTCMD} --label=System.ServiceModel --timeout=15m make -w -C mcs/class/System.ServiceModel run-test
${TESTCMD} --label=System.ServiceModel.Web --timeout=5m make -w -C mcs/class/System.ServiceModel.Web run-test
${TESTCMD} --label=System.Web.Extensions-standalone --timeout=5m make -w -C mcs/class/System.Web.Extensions run-standalone-test
${TESTCMD} --label=System.ComponentModel.DataAnnotations --timeout=5m make -w -C mcs/class/System.ComponentModel.DataAnnotations run-test
${TESTCMD} --label=Mono.CodeContracts --timeout=5m make -w -C mcs/class/Mono.CodeContracts run-test
${TESTCMD} --label=System.Runtime.Caching --timeout=5m make -w -C mcs/class/System.Runtime.Caching run-test
${TESTCMD} --label=System.Data.Services --timeout=5m make -w -C mcs/class/System.Data.Services run-test
${TESTCMD} --label=System.Web.DynamicData --timeout=5m make -w -C mcs/class/System.Web.DynamicData run-test
${TESTCMD} --label=Mono.CSharp --timeout=5m make -w -C mcs/class/Mono.CSharp run-test
${TESTCMD} --label=WindowsBase --timeout=5m make -w -C mcs/class/WindowsBase run-test
${TESTCMD} --label=System.Numerics --timeout=5m make -w -C mcs/class/System.Numerics run-test
${TESTCMD} --label=System.Runtime.DurableInstancing --timeout=5m make -w -C mcs/class/System.Runtime.DurableInstancing run-test
${TESTCMD} --label=System.ServiceModel.Discovery --timeout=5m make -w -C mcs/class/System.ServiceModel.Discovery run-test
${TESTCMD} --label=System.Xaml --timeout=5m make -w -C mcs/class/System.Xaml run-test
${TESTCMD} --label=System.Net.Http --timeout=5m make -w -C mcs/class/System.Net.Http run-test
${TESTCMD} --label=System.Json --timeout=5m make -w -C mcs/class/System.Json run-test
${TESTCMD} --label=System.Threading.Tasks.Dataflow --timeout=5m make -w -C mcs/class/System.Threading.Tasks.Dataflow run-test
${TESTCMD} --label=Mono.Debugger.Soft --timeout=5m make -w -C mcs/class/Mono.Debugger.Soft run-test
${TESTCMD} --label=Microsoft.Build --timeout=5m make -w -C mcs/class/Microsoft.Build run-test
${TESTCMD} --label=monodoc --timeout=10m make -w -C mcs/tools/mdoc run-test
${TESTCMD} --label=Microsoft.Build-12 --timeout=10m make -w -C mcs/class/Microsoft.Build run-test PROFILE=xbuild_12
${TESTCMD} --label=Microsoft.Build.Engine-12 --timeout=60m make -w -C mcs/class/Microsoft.Build.Engine run-test PROFILE=xbuild_12
${TESTCMD} --label=Microsoft.Build.Framework-12 --timeout=60m make -w -C mcs/class/Microsoft.Build.Framework run-test PROFILE=xbuild_12
${TESTCMD} --label=Microsoft.Build.Tasks-12 --timeout=60m make -w -C mcs/class/Microsoft.Build.Tasks run-test PROFILE=xbuild_12
${TESTCMD} --label=Microsoft.Build.Utilities-12 --timeout=60m make -w -C mcs/class/Microsoft.Build.Utilities run-test PROFILE=xbuild_12
${TESTCMD} --label=Microsoft.Build-14 --timeout=60m make -w -C mcs/class/Microsoft.Build run-test PROFILE=xbuild_14
${TESTCMD} --label=Microsoft.Build.Engine-14 --timeout=60m make -w -C mcs/class/Microsoft.Build.Engine run-test PROFILE=xbuild_14
${TESTCMD} --label=Microsoft.Build.Framework-14 --timeout=60m make -w -C mcs/class/Microsoft.Build.Framework run-test PROFILE=xbuild_14
${TESTCMD} --label=Microsoft.Build.Tasks-14 --timeout=60m make -w -C mcs/class/Microsoft.Build.Tasks run-test PROFILE=xbuild_14
${TESTCMD} --label=Microsoft.Build.Utilities-14 --timeout=60m make -w -C mcs/class/Microsoft.Build.Utilities run-test PROFILE=xbuild_14
rm -fr /tmp/jenkins-temp-aspnet*
