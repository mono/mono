#!/bin/bash -e

export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh
export TEST_WITH_INTERPRETER=1

${TESTCMD} --label=interpreter-regression --timeout=10m make -C mono/mini richeck
${TESTCMD} --label=mixedmode-regression --timeout=10m make -C mono/mini mixedcheck
${TESTCMD} --label=fullaotmixed-regression --timeout=20m make -C mono/mini fullaotmixedcheck
${TESTCMD} --label=compile-runtime-tests --timeout=40m make -w -C mono/tests -j ${CI_CPU_COUNT} test
${TESTCMD} --label=runtime-interp --timeout=160m make -w -C mono/tests -k testinterp V=1
${TESTCMD} --label=corlib --timeout=160m make -w -C mcs/class/corlib run-test V=1
${TESTCMD} --label=System --timeout=160m bash -c "export MONO_TLS_PROVIDER=legacy && make -w -C mcs/class/System run-test V=1"
${TESTCMD} --label=System.Core --timeout=160m make -w -C mcs/class/System.Core run-test V=1
${TESTCMD} --label=mcs-tests --timeout=160m make -w -C mcs/tests run-test V=1;
${TESTCMD} --label=Mono.Debugger.Soft --timeout=5m make -w -C mcs/class/Mono.Debugger.Soft run-test V=1

run_full_test_suite=1

# Don't run full interpreter test suite on arm architectures during PR builds.
# NOTE, full test suite will still be run on none PR builds.
if [[ ${CI_TAGS} == *'pull-request'* ]] && [[ ${CI_TAGS} == *'arm'* ]]; then
	run_full_test_suite=0
fi

# Don't run full interpreter test suite on Windows during PR builds.
# NOTE, full test suite will still be run on none PR builds.
if [[ ${CI_TAGS} == *'pull-request'* ]] && [[ ${CI_TAGS} == *'win-'* ]]; then
	run_full_test_suite=0
fi

if [[ "$run_full_test_suite" -eq "1" ]]; then

	${TESTCMD} --label=corlib-xunit --timeout=60m make -w -C mcs/class/corlib run-xunit-test
	${TESTCMD} --label=System.XML --timeout=5m make -w -C mcs/class/System.XML run-test V=1
	${TESTCMD} --label=Mono.Security --timeout=30m make -w -C mcs/class/Mono.Security run-test V=1
	${TESTCMD} --label=System.Security --timeout=15m make -w -C mcs/class/System.Security run-test V=1
	${TESTCMD} --label=System.Security-xunit --timeout=5m make -w -C mcs/class/System.Security run-xunit-test V=1
	${TESTCMD} --label=System.Data --timeout=5m make -w -C mcs/class/System.Data run-test V=1
	${TESTCMD} --label=System.Data.OracleClient --timeout=5m make -w -C mcs/class/System.Data.OracleClient run-test V=1
	${TESTCMD} --label=System.Design --timeout=5m make -w -C mcs/class/System.Design run-test V=1
	${TESTCMD} --label=Mono.Posix --timeout=5m make -w -C mcs/class/Mono.Posix run-test V=1
	${TESTCMD} --label=System.Web --timeout=30m make -w -C mcs/class/System.Web run-test V=1
	${TESTCMD} --label=System.Web.Services --timeout=5m make -w -C mcs/class/System.Web.Services run-test V=1
	${TESTCMD} --label=System.Runtime.SFS --timeout=5m make -w -C mcs/class/System.Runtime.Serialization.Formatters.Soap run-test V=1
	${TESTCMD} --label=System.Runtime.Remoting --timeout=5m make -w -C mcs/class/System.Runtime.Remoting run-test V=1
	${TESTCMD} --label=Cscompmgd --timeout=5m make -w -C mcs/class/Cscompmgd run-test V=1
	${TESTCMD} --label=Commons.Xml.Relaxng --timeout=5m make -w -C mcs/class/Commons.Xml.Relaxng run-test V=1
	${TESTCMD} --label=System.ServiceProcess --timeout=5m make -w -C mcs/class/System.ServiceProcess run-test V=1
	${TESTCMD} --label=I18N.CJK --timeout=5m make -w -C mcs/class/I18N/CJK run-test V=1
	${TESTCMD} --label=I18N.West --timeout=5m make -w -C mcs/class/I18N/West run-test V=1
	${TESTCMD} --label=I18N.MidEast --timeout=5m make -w -C mcs/class/I18N/MidEast run-test V=1
	${TESTCMD} --label=I18N.Rare --timeout=5m make -w -C mcs/class/I18N/Rare run-test V=1
	${TESTCMD} --label=I18N.Other --timeout=5m make -w -C mcs/class/I18N/Other run-test V=1
	${TESTCMD} --label=System.DirectoryServices --timeout=5m make -w -C mcs/class/System.DirectoryServices run-test V=1
	${TESTCMD} --label=Microsoft.Build.Engine --timeout=5m make -w -C mcs/class/Microsoft.Build.Engine run-test V=1
	${TESTCMD} --label=Microsoft.Build.Framework --timeout=5m make -w -C mcs/class/Microsoft.Build.Framework run-test V=1
	${TESTCMD} --label=Microsoft.Build.Tasks --timeout=5m make -w -C mcs/class/Microsoft.Build.Tasks run-test V=1
	${TESTCMD} --label=Microsoft.Build.Utilities --timeout=5m make -w -C mcs/class/Microsoft.Build.Utilities run-test V=1
	${TESTCMD} --label=Mono.C5 --timeout=5m make -w -C mcs/class/Mono.C5 run-test V=1
	${TESTCMD} --label=Mono.Options --timeout=5m make -w -C mcs/class/Mono.Options run-test V=1
	${TESTCMD} --label=System.Configuration --timeout=5m make -w -C mcs/class/System.Configuration run-test V=1
	${TESTCMD} --label=System.Transactions --timeout=5m make -w -C mcs/class/System.Transactions run-test V=1
	${TESTCMD} --label=System.Web.Extensions --timeout=5m make -w -C mcs/class/System.Web.Extensions run-test V=1
	${TESTCMD} --label=System.Core-xunit --timeout=15m make -w -C mcs/class/System.Core run-xunit-test V=1
	${TESTCMD} --label=System.Xml.Linq --timeout=5m make -w -C mcs/class/System.Xml.Linq run-test V=1
	${TESTCMD} --label=System.Xml.Linq-xunit --timeout=15m make -w -C mcs/class/System.Xml.Linq run-xunit-test V=1
	${TESTCMD} --label=System.Data.DSE --timeout=5m make -w -C mcs/class/System.Data.DataSetExtensions run-test V=1
	${TESTCMD} --label=System.Web.Abstractions --timeout=5m make -w -C mcs/class/System.Web.Abstractions run-test V=1
	${TESTCMD} --label=System.Web.Routing --timeout=5m make -w -C mcs/class/System.Web.Routing run-test V=1
	${TESTCMD} --label=System.Runtime.Serialization --timeout=5m make -w -C mcs/class/System.Runtime.Serialization run-test V=1
	${TESTCMD} --label=System.Runtime.Serialization-xunit --timeout=5m make -w -C mcs/class/System.Runtime.Serialization run-xunit-test V=1
	${TESTCMD} --label=System.IdentityModel --timeout=5m make -w -C mcs/class/System.IdentityModel run-test V=1
	${TESTCMD} --label=System.ServiceModel --timeout=15m make -w -C mcs/class/System.ServiceModel run-test V=1
	${TESTCMD} --label=System.ServiceModel.Web --timeout=5m make -w -C mcs/class/System.ServiceModel.Web run-test V=1
	${TESTCMD} --label=System.Web.Extensions-standalone --timeout=5m make -w -C mcs/class/System.Web.Extensions run-standalone-test V=1
	${TESTCMD} --label=System.ComponentModel.DataAnnotations --timeout=5m make -w -C mcs/class/System.ComponentModel.DataAnnotations run-test V=1
	${TESTCMD} --label=System.ComponentModel.Composition-xunit --timeout=5m make -w -C mcs/class/System.ComponentModel.Composition.4.5 run-xunit-test V=1
	${TESTCMD} --label=Mono.CodeContracts --timeout=5m make -w -C mcs/class/Mono.CodeContracts run-test V=1
	${TESTCMD} --label=System.Runtime.Caching --timeout=5m make -w -C mcs/class/System.Runtime.Caching run-test V=1
	${TESTCMD} --label=System.Data.Services --timeout=5m make -w -C mcs/class/System.Data.Services run-test V=1
	${TESTCMD} --label=System.Web.DynamicData --timeout=5m make -w -C mcs/class/System.Web.DynamicData run-test V=1
	${TESTCMD} --label=Mono.CSharp --timeout=5m make -w -C mcs/class/Mono.CSharp run-test V=1
	${TESTCMD} --label=WindowsBase --timeout=5m make -w -C mcs/class/WindowsBase run-test V=1
	${TESTCMD} --label=System.Numerics --timeout=5m make -w -C mcs/class/System.Numerics run-test V=1
	${TESTCMD} --label=System.Numerics-xunit --timeout=20m make -w -C mcs/class/System.Numerics run-xunit-test V=1
	${TESTCMD} --label=System.Runtime.DurableInstancing --timeout=5m make -w -C mcs/class/System.Runtime.DurableInstancing run-test V=1
	${TESTCMD} --label=System.ServiceModel.Discovery --timeout=5m make -w -C mcs/class/System.ServiceModel.Discovery run-test V=1
	${TESTCMD} --label=System.Xaml --timeout=5m make -w -C mcs/class/System.Xaml run-test V=1
	${TESTCMD} --label=System.Net.Http --timeout=5m make -w -C mcs/class/System.Net.Http run-test V=1
	${TESTCMD} --label=System.Json --timeout=5m make -w -C mcs/class/System.Json run-test V=1
	${TESTCMD} --label=System.Json-xunit --timeout=5m make -w -C mcs/class/System.Json run-xunit-test V=1
	${TESTCMD} --label=System.Threading.Tasks.Dataflow --timeout=5m make -w -C mcs/class/System.Threading.Tasks.Dataflow run-test V=1
	${TESTCMD} --label=System.Threading.Tasks.Dataflow-xunit --timeout=5m make -w -C mcs/class/System.Threading.Tasks.Dataflow run-xunit-test V=1
	${TESTCMD} --label=System.Runtime.CompilerServices.Unsafe-xunit --timeout=5m make -w -C mcs/class/System.Runtime.CompilerServices.Unsafe run-xunit-test V=1
	${TESTCMD} --label=Microsoft.CSharp-xunit --timeout=5m make -w -C mcs/class/Microsoft.CSharp run-xunit-test V=1
	${TESTCMD} --label=Microsoft.Build --timeout=5m make -w -C mcs/class/Microsoft.Build run-test V=1
	${TESTCMD} --label=Microsoft.Build-12 --timeout=10m make -w -C mcs/class/Microsoft.Build run-test PROFILE=xbuild_12 V=1
	${TESTCMD} --label=Microsoft.Build.Engine-12 --timeout=60m make -w -C mcs/class/Microsoft.Build.Engine run-test PROFILE=xbuild_12 V=1
	${TESTCMD} --label=Microsoft.Build.Framework-12 --timeout=60m make -w -C mcs/class/Microsoft.Build.Framework run-test PROFILE=xbuild_12 V=1
	${TESTCMD} --label=Microsoft.Build.Tasks-12 --timeout=60m make -w -C mcs/class/Microsoft.Build.Tasks run-test PROFILE=xbuild_12 V=1
	${TESTCMD} --label=Microsoft.Build.Utilities-12 --timeout=60m make -w -C mcs/class/Microsoft.Build.Utilities run-test PROFILE=xbuild_12 V=1
	${TESTCMD} --label=Microsoft.Build-14 --timeout=60m make -w -C mcs/class/Microsoft.Build run-test PROFILE=xbuild_14 V=1
	${TESTCMD} --label=Microsoft.Build.Engine-14 --timeout=60m make -w -C mcs/class/Microsoft.Build.Engine run-test PROFILE=xbuild_14 V=1
	${TESTCMD} --label=Microsoft.Build.Framework-14 --timeout=60m make -w -C mcs/class/Microsoft.Build.Framework run-test PROFILE=xbuild_14 V=1
	${TESTCMD} --label=Microsoft.Build.Tasks-14 --timeout=60m make -w -C mcs/class/Microsoft.Build.Tasks run-test PROFILE=xbuild_14 V=1
	${TESTCMD} --label=Microsoft.Build.Utilities-14 --timeout=60m make -w -C mcs/class/Microsoft.Build.Utilities run-test PROFILE=xbuild_14 V=1
	${TESTCMD} --label=System.IO.Compression --timeout=5m make -w -C mcs/class/System.IO.Compression run-test V=1
fi

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
