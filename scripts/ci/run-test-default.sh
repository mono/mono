#!/bin/bash -e

${TESTCMD} --label=mini --timeout=5m make -w -C mono/mini -k check check-seq-points EMIT_NUNIT=1
if [[ ${label} == w* ]]
then ${TESTCMD} --label=aot-test --skip;
else ${TESTCMD} --label=aot-test --timeout=30m make -w -C mono/tests -j4 -k test-aot
fi
${TESTCMD} --label=compile-bcl-tests --timeout=40m make -i -w -C runtime -j4 test
${TESTCMD} --label=compile-runtime-tests --timeout=40m make -w -C mono/tests -j4 tests
${TESTCMD} --label=runtime --timeout=160m make -w -C mono/tests -k test-wrench V=1 CI=1 CI_PR=${ghprbPullId}
${TESTCMD} --label=runtime-unit-tests --timeout=5m make -w -C mono/unit-tests -k check
${TESTCMD} --label=corlib --timeout=30m make -w -C mcs/class/corlib run-test
${TESTCMD} --label=corlib-xunit --timeout=5m make -w -C mcs/class/corlib run-xunit-test
${TESTCMD} --label=verify --timeout=15m make -w -C runtime mcs-compileall
${TESTCMD} --label=profiler --timeout=30m make -w -C mono/profiler -k check
${TESTCMD} --label=compiler --timeout=30m make -w -C mcs/tests run-test
${TESTCMD} --label=compiler-errors --timeout=30m make -w -C mcs/errors run-test
${TESTCMD} --label=System --timeout=10m bash -c "export MONO_TLS_PROVIDER=legacy && make -w -C mcs/class/System run-test"
if [[ ${label} == osx-* ]]; then ${TESTCMD} --label=System-btls --timeout=10m bash -c "export MONO_TLS_PROVIDER=btls && make -w -C mcs/class/System run-test"; fi
${TESTCMD} --label=System.XML --timeout=5m make -w -C mcs/class/System.XML run-test
${TESTCMD} --label=Mono.Security --timeout=5m make -w -C mcs/class/Mono.Security run-test
${TESTCMD} --label=System.Security --timeout=5m make -w -C mcs/class/System.Security run-test
if [[ ${label} == w* ]]
then ${TESTCMD} --label=System.Drawing --skip;
else ${TESTCMD} --label=System.Drawing --timeout=5m make -w -C mcs/class/System.Drawing run-test
fi
if [[ ${label} == osx-* ]] || [[ ${label} == w* ]]
then ${TESTCMD} --label=Windows.Forms --skip;
else
    if xvfb-run -a -- make -C mcs/class/System.Windows.Forms test-simple;
    then ${TESTCMD} --label=Windows.Forms --timeout=5m xvfb-run -a -- make -w -C mcs/class/System.Windows.Forms run-test
    else echo "The simple test failed (maybe because of missing X server), skipping test suite." && ${TESTCMD} --label=Windows.Forms --skip; fi
fi
${TESTCMD} --label=System.Data --timeout=5m make -w -C mcs/class/System.Data run-test
${TESTCMD} --label=System.Data-xunit --timeout=5m make -w -C mcs/class/System.Data run-xunit-test
if [[ ${label} == w* ]]; then ${TESTCMD} --label=Mono.Data.Sqlite --skip; else ${TESTCMD} --label=Mono.Data.Sqlite --timeout=5m make -w -C mcs/class/Mono.Data.Sqlite run-test; fi
${TESTCMD} --label=System.Data.OracleClient --timeout=5m make -w -C mcs/class/System.Data.OracleClient run-test;
${TESTCMD} --label=System.Design --timeout=5m make -w -C mcs/class/System.Design run-test;
${TESTCMD} --label=Mono.Posix --timeout=5m make -w -C mcs/class/Mono.Posix run-test
${TESTCMD} --label=System.Web --timeout=30m make -w -C mcs/class/System.Web run-test
${TESTCMD} --label=System.Web.Services --timeout=5m make -w -C mcs/class/System.Web.Services run-test
${TESTCMD} --label=System.Runtime.SFS --timeout=5m make -w -C mcs/class/System.Runtime.Serialization.Formatters.Soap run-test;
${TESTCMD} --label=System.Runtime.Remoting --timeout=5m make -w -C mcs/class/System.Runtime.Remoting run-test
${TESTCMD} --label=Cscompmgd --timeout=5m make -w -C mcs/class/Cscompmgd run-test;
${TESTCMD} --label=Commons.Xml.Relaxng --timeout=5m make -w -C mcs/class/Commons.Xml.Relaxng run-test;
${TESTCMD} --label=System.ServiceProcess --timeout=5m make -w -C mcs/class/System.ServiceProcess run-test
${TESTCMD} --label=I18N.CJK --timeout=5m make -w -C mcs/class/I18N/CJK run-test
${TESTCMD} --label=I18N.West --timeout=5m make -w -C mcs/class/I18N/West run-test
${TESTCMD} --label=I18N.MidEast --timeout=5m make -w -C mcs/class/I18N/MidEast run-test
${TESTCMD} --label=I18N.Rare --timeout=5m make -w -C mcs/class/I18N/Rare run-test
${TESTCMD} --label=I18N.Other --timeout=5m make -w -C mcs/class/I18N/Other run-test
${TESTCMD} --label=System.DirectoryServices --timeout=5m make -w -C mcs/class/System.DirectoryServices run-test
${TESTCMD} --label=Microsoft.Build.Engine --timeout=5m make -w -C mcs/class/Microsoft.Build.Engine run-test
${TESTCMD} --label=Microsoft.Build.Framework --timeout=5m make -w -C mcs/class/Microsoft.Build.Framework run-test
${TESTCMD} --label=Microsoft.Build.Tasks --timeout=5m make -w -C mcs/class/Microsoft.Build.Tasks run-test
${TESTCMD} --label=Microsoft.Build.Utilities --timeout=5m make -w -C mcs/class/Microsoft.Build.Utilities run-test
${TESTCMD} --label=Mono.C5 --timeout=5m make -w -C mcs/class/Mono.C5 run-test
${TESTCMD} --label=Mono.Options --timeout=5m make -w -C mcs/class/Mono.Options run-test
${TESTCMD} --label=Mono.Tasklets --timeout=5m make -w -C mcs/class/Mono.Tasklets run-test
${TESTCMD} --label=System.Configuration --timeout=5m make -w -C mcs/class/System.Configuration run-test
${TESTCMD} --label=System.Transactions --timeout=5m make -w -C mcs/class/System.Transactions run-test
${TESTCMD} --label=System.Web.Extensions --timeout=5m make -w -C mcs/class/System.Web.Extensions run-test
${TESTCMD} --label=System.Core --timeout=15m make -w -C mcs/class/System.Core run-test
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
${TESTCMD} --label=System.Numerics-xunit --timeout=5m make -w -C mcs/class/System.Numerics run-xunit-test
${TESTCMD} --label=System.Runtime.DurableInstancing --timeout=5m make -w -C mcs/class/System.Runtime.DurableInstancing run-test
${TESTCMD} --label=System.ServiceModel.Discovery --timeout=5m make -w -C mcs/class/System.ServiceModel.Discovery run-test
${TESTCMD} --label=System.Xaml --timeout=5m make -w -C mcs/class/System.Xaml run-test
${TESTCMD} --label=System.Net.Http --timeout=5m make -w -C mcs/class/System.Net.Http run-test
${TESTCMD} --label=System.Json --timeout=5m make -w -C mcs/class/System.Json run-test
${TESTCMD} --label=System.Threading.Tasks.Dataflow --timeout=5m make -w -C mcs/class/System.Threading.Tasks.Dataflow run-test
${TESTCMD} --label=Mono.Debugger.Soft --timeout=5m make -w -C mcs/class/Mono.Debugger.Soft run-test
${TESTCMD} --label=Microsoft.CSharp-xunit --timeout=5m make -w -C mcs/class/Microsoft.CSharp run-xunit-test
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
${TESTCMD} --label=System.IO.Compression --timeout=5m make -w -C mcs/class/System.IO.Compression run-test
if [[ ${label} == w* ]]; then ${TESTCMD} --label=symbolicate --skip; else ${TESTCMD} --label=symbolicate --timeout=60m make -w -C mcs/tools/mono-symbolicate check; fi
${TESTCMD} --label=monolinker --timeout=10m make -w -C mcs/tools/linker check
${TESTCMD} --label=csi --timeout=10m make -w -C mcs/packages run-test

if [[ $CI_TAGS == *'ms-test-suite'* ]]
then ${TESTCMD} --label=ms-test-suite --timeout=30m make -w -C acceptance-tests check-ms-test-suite
else ${TESTCMD} --label=ms-test-suite --skip;
fi
if [[ $CI_TAGS == *'apidiff'* ]]; then
    source ${MONO_REPO_ROOT}/scripts/ci/util.sh
    if ${TESTCMD} --label=apidiff --timeout=15m --fatal make -w -C mcs -j4 mono-api-diff
    then report_github_status "success" "API Diff" "No public API changes found." || true
    else report_github_status "error" "API Diff" "The public API changed." "$BUILD_URL/Public_API_Diff/" || true
    fi
else ${TESTCMD} --label=apidiff --skip
fi
rm -fr /tmp/jenkins-temp-aspnet*
