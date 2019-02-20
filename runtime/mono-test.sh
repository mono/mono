#! /bin/sh -e

test_suite="$1"
test_argument_1="$2"
test_argument_2="$3"
xunit_results_path="$(pwd)/testResults.xml"

cd "$(dirname "$0")" || exit 1

r=$(pwd)
MONO_CFG_DIR="$r/_tmpinst/etc"
PATH="$r/_tmpinst/bin:$PATH"
MONO_EXECUTABLE=${MONO_EXECUTABLE:-"$r/mono-sgen"}
MONO_PATH="$r/net_4_x"
export MONO_CFG_DIR MONO_PATH MONO_EXECUTABLE PATH

sed "s,\$test_root_dir,$r,g" "$r/_tmpinst/etc/mono/config.tmpl" > "$r/_tmpinst/etc/mono/config"
sed "s,\$test_root_dir,$r,g" "$r/tests/runtime/tests-config.tmpl" > "$r/tests/runtime/tests-config"

chmod +x "${MONO_EXECUTABLE}"
echo "---------------------------------------------------------------------------------------"
"${MONO_EXECUTABLE}" --version
echo "---------------------------------------------------------------------------------------"

case "$(uname)" in
    "Linux")
        mkdir -p ~/.config/.mono/
        wget -qO- https://download.mono-project.com/test/new-certs.tgz| tar zx -C ~/.config/.mono/
        ;;
esac

if [ "$test_suite" = "--xunit" ]; then
    cd net_4_x || exit 1
    export MONO_PATH="$r/net_4_x/tests:$MONO_PATH"
    export REMOTE_EXECUTOR="$r/net_4_x/tests/RemoteExecutorConsoleApp.exe"
    case "$test_argument_1" in
        *"Mono.Profiler.Log"*)
            # necessary for the runtime to find libmono-profiler-log
            export LD_LIBRARY_PATH="$r:$LD_LIBRARY_PATH"
            export DYLD_LIBRARY_PATH="$r:$LD_LIBRARY_PATH"
            ;;
        *"System.Net.Http"*)
            export MONO_URI_DOTNETRELATIVEORABSOLUTE=true
            ;;
    esac
    case "$(uname)" in
        "Darwin")
            ADDITIONAL_TRAITS="-notrait category=nonosxtests"
            ;;
        "Linux")
            ADDITIONAL_TRAITS="-notrait category=nonlinuxtests"
            ;;
    esac

    "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" --debug "$r/net_4_x/xunit.console.exe" "$r/$test_argument_1" -noappdomain -noshadow -parallel none -xml "${xunit_results_path}" -notrait category=failing -notrait category=nonmonotests -notrait Benchmark=true -notrait category=outerloop $ADDITIONAL_TRAITS
    exit $?
fi

if [ "$test_suite" = "--nunit" ]; then
    cd net_4_x || exit 1
    export MONO_PATH="$r/net_4_x/tests:$MONO_PATH"
    export MONO_SYSTEMWEB_CACHEDEPENDENCY_SHARED_FSW=1
    case "$test_argument_1" in
        *"Microsoft.Build"*)
            export TESTING_MONO=a
            export MSBuildExtensionsPath="$r/net_4_x/tests/xbuild/extensions"
            export XBUILD_FRAMEWORK_FOLDERS_PATH="$r/net_4_x/tests/xbuild/frameworks"
            case "$test_argument_1" in
                "xbuild_12"*) export MONO_PATH="$r/xbuild_12:$r/xbuild_12/tests:$MONO_PATH" ;;
                "xbuild_14"*) export MONO_PATH="$r/xbuild_14:$r/xbuild_14/tests:$MONO_PATH" ;;
            esac
            ;;
        *"Mono.Messaging.RabbitMQ"*)
            export MONO_MESSAGING_PROVIDER=Mono.Messaging.RabbitMQ.RabbitMQMessagingProvider,Mono.Messaging.RabbitMQ
            ;;
        *"System.Windows.Forms"*)
            sudo apt install -y xvfb xauth
            XVFBRUN="xvfb-run -a --"
            ADDITIONAL_TEST_EXCLUDES="NotWithXvfb" # TODO: find out why this works on Jenkins?
            ;;
    esac
    case "$test_argument_2" in
        "--flaky-test-retries="*)
            export MONO_FLAKY_TEST_RETRIES=$(echo "$test_argument_2" | cut -d "=" -f2)
            ;;
    esac
    cp -f "$r/$test_argument_1.nunitlite.config" "$r/net_4_x/nunit-lite-console.exe.config"
    MONO_REGISTRY_PATH="$HOME/.mono/registry" MONO_TESTS_IN_PROGRESS="yes" $XVFBRUN "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" --debug "$r/net_4_x/nunit-lite-console.exe" "$r/$test_argument_1" -exclude=NotWorking,CAS,$ADDITIONAL_TEST_EXCLUDES -labels -format:xunit -result:"${xunit_results_path}"
    exit $?
fi

if [ "$test_suite" = "--verify" ]; then
    verifiable_files=$(find net_4_x -maxdepth 1 -name "*.dll" -or -name "*.exe" | grep -v System.Runtime.CompilerServices.Unsafe.dll | grep -v Xunit.NetCore.Extensions.dll)
    ok=true
    for asm in $verifiable_files; do
        echo "$asm"
        if [ ! -f "$asm" ]; then continue; fi
        if "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" --compile-all --verify-all --security=verifiable "$asm"; then
            echo "$asm verified OK"
        else
            echo "$asm verification failed"
            ok=false
        fi
    done;
    if [ "$ok" = "true" ]; then
        echo "<?xml version=\"1.0\" encoding=\"utf-8\"?><assemblies><assembly name=\"verify\" environment=\"Mono\" test-framework=\"custom\" run-date=\"$(date +%F)\" run-time=\"$(date +%T)\" total=\"1\" passed=\"1\" failed=\"0\" skipped=\"0\" errors=\"0\" time=\"0\"><collection total=\"1\" passed=\"1\" failed=\"0\" skipped=\"0\" name=\"Test collection for verify\" time=\"0\"><test name=\"verify.all\" type=\"verify\" method=\"all\" time=\"0\" result=\"Pass\"></test></collection></assembly></assemblies>" > "${xunit_results_path}";
        exit 0
    else
        echo "<?xml version=\"1.0\" encoding=\"utf-8\"?><assemblies><assembly name=\"verify\" environment=\"Mono\" test-framework=\"custom\" run-date=\"$(date +%F)\" run-time=\"$(date +%T)\" total=\"1\" passed=\"0\" failed=\"1\" skipped=\"0\" errors=\"0\" time=\"0\"><collection total=\"1\" passed=\"0\" failed=\"1\" skipped=\"0\" name=\"Test collection for verify\" time=\"0\"><test name=\"verify.all\" type=\"verify\" method=\"all\" time=\"0\" result=\"Fail\"><failure exception-type=\"VerifyException\"><message><![CDATA[Verifying framework assemblies failed. Check the log for more details.]]></message></failure></test></collection></assembly></assemblies>" > "${xunit_results_path}";
        exit 1
    fi
fi

if [ "$test_suite" = "--mcs" ]; then
    cd tests/mcs || exit 1
    "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" --verify-all compiler-tester.exe -mode:pos -files:v4 -compiler:"$r/net_4_x/mcs.exe" -reference-dir:"$r/net_4_x" -issues:known-issues-net_4_x -log:net_4_x.log -il:ver-il-net_4_x.xml -resultXml:"${xunit_results_path}" -compiler-options:"-d:NET_4_0;NET_4_5 -debug"
    exit $?
fi

if [ "$test_suite" = "--mcs-errors" ]; then
    cd tests/mcs-errors || exit 1
    "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" compiler-tester.exe -mode:neg -files:v4 -compiler:"$r/net_4_x/mcs.exe" -reference-dir:"$r/net_4_x" -issues:known-issues-net_4_x -log:net_4_x.log -resultXml:"${xunit_results_path}" -compiler-options:"-v --break-on-ice -d:NET_4_0;NET_4_5"
    exit $?
fi

if [ "$test_suite" = "--aot-test" ]; then
    failed=0
    passed=0
    failed_tests=""
    profile="$r/net_4_x"
    tmpfile=$(mktemp -t mono_aot_outputXXXXXX) || exit 1
    rm -f "test-aot-*.stdout" "test-aot-*.stderr" "${xunit_results_path}.cases.xml"
    for assembly in "$profile"/*.dll; do
        asm_name=$(basename "$assembly")
        echo "... $asm_name"
        for conf in "|regular" "--gc=boehm|boehm"; do
            name=$(echo $conf | cut -d\| -f 2)
            params=$(echo $conf | cut -d\| -f 1)
            test_name="${asm_name}|${name}"
            echo "  $test_name"
            if "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" $params --aot=outfile="$tmpfile" "$assembly" > "test-aot-${name}-${asm_name}.stdout" 2> "test-aot-${name}-${asm_name}.stderr"
            then
                passed=$((passed + 1))
                resultstring="Pass"
            else \
                failed=$((failed + 1))
                failed_tests="${failed_tests} $test_name"
                resultstring="Fail"
            fi
            echo "<test name=\"aot-test.$name.$asm_name\" type=\"aot-test.$name\" method=\"$asm_name\" time=\"0\" result=\"$resultstring\">" >> "${xunit_results_path}.cases.xml"
            if [ "$resultstring" = "Fail" ]; then
                echo "<failure exception-type=\"AotTestException\"><message><![CDATA[
                    STDOUT:
                    $(cat "test-aot-${name}-${asm_name}.stdout")
                    STDERR:
                    $(cat "test-aot-${name}-${asm_name}.stderr")]]></message><stack-trace></stack-trace></failure>" >> "${xunit_results_path}.cases.xml"; fi
            echo "</test>" >> "${xunit_results_path}.cases.xml"
        done
    done
    echo "<?xml version=\"1.0\" encoding=\"utf-8\"?>\
    <assemblies>\
        <assembly name=\"aot-test\" environment=\"Mono\" test-framework=\"custom\" run-date=\"$(date +%F)\" run-time=\"$(date +%T)\" total=\"$((passed + failed))\" passed=\"$passed\" failed=\"$failed\" skipped=\"0\" errors=\"0\" time=\"0\">\
            <collection total=\"$((passed + failed))\" passed=\"$passed\" failed=\"$failed\" skipped=\"0\" name=\"Test collection for aot-test\" time=\"0\">\
                $(cat "${xunit_results_path}.cases.xml")
            </collection>\
        </assembly>\
    </assemblies>" > "${xunit_results_path}";
    rm "$tmpfile"
    echo "${passed} test(s) passed. ${failed} test(s) did not pass."
    if [ "${failed}" != 0 ]; then
        echo ""
        echo "Failed tests:"
        echo ""
        for i in ${failed_tests}; do
            echo "${i}";
        done
        exit 1
    fi
    exit 0
fi

if [ "$test_suite" = "--mini" ]; then
    cd tests/mini || exit 1

    "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" --regression ./*.exe > regressiontests.out 2>&1
    cat regressiontests.out
    if grep -q "100% pass" regressiontests.out; then
        resultstring="Pass"
        failurescount=0
        successcount=1
    else
        resultstring="Fail"
        failurescount=1
        successcount=0
    fi
    echo "<?xml version=\"1.0\" encoding=\"utf-8\"?>\
        <assemblies>\
            <assembly name=\"mini.regression-tests\" environment=\"Mono\" test-framework=\"custom\" run-date=\"$(date +%F)\" run-time=\"$(date +%T)\" total=\"1\" passed=\"$successcount\" failed=\"$failurescount\" skipped=\"0\" errors=\"0\" time=\"0\">\
                <collection total=\"1\" passed=\"$successcount\" failed=\"$failurescount\" skipped=\"0\" name=\"Test collection for mini.regression-tests\" time=\"0\">\
                    <test name=\"mini.regression-tests.all\" type=\"mini.regression-tests\" method=\"all\" time=\"0\" result=\"$resultstring\">" > "${xunit_results_path}"
                    if [ "$resultstring" = "Fail" ]; then echo "<failure exception-type=\"MiniRegressionTestsException\"><message><![CDATA[$(cat regressiontests.out)]]></message><stack-trace></stack-trace></failure>" >> "${xunit_results_path}"; fi
                echo "</test>
                </collection>\
            </assembly>\
        </assemblies>" >> "${xunit_results_path}";
    exit $failurescount
fi

if [ "$test_suite" = "--symbolicate" ]; then
    cd tests/symbolicate || exit 1

    "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" --aot 2>&1 | grep -q "AOT compilation is not supported" && echo "No AOT support, skipping tests." && exit 0

    ok=true
    for config in without-aot with-aot with-aot-msym; do
        OUT_DIR="$config"
        MSYM_DIR="$OUT_DIR/msymdir"
        STACKTRACE_FILE="$OUT_DIR/stacktrace.out"
        SYMBOLICATE_RAW_FILE="$OUT_DIR/symbolicate_raw.out"
        SYMBOLICATE_RESULT_FILE="$OUT_DIR/symbolicate.result"
        SYMBOLICATE_EXPECTED_FILE=symbolicate.expected

        echo "Checking StackTraceDumper.exe in configuration $config..."
        rm -rf "$OUT_DIR"
        mkdir -p "$OUT_DIR"
        mkdir -p "$MSYM_DIR"

        cp StackTraceDumper.exe "$OUT_DIR"
        cp StackTraceDumper.pdb "$OUT_DIR"

        # store symbols
        "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" "$r/net_4_x/mono-symbolicate.exe" store-symbols "$MSYM_DIR" "$OUT_DIR"
        "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" "$r/net_4_x/mono-symbolicate.exe" store-symbols "$MSYM_DIR" "$r/net_4_x"

        if [ "$config" = "with-aot" ]; then "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" -O=-inline --aot "$OUT_DIR/StackTraceDumper.exe"; fi
        if [ "$config" = "with-aot-msym" ]; then "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" -O=-inline --aot=msym-dir="$MSYM_DIR" "$OUT_DIR/StackTraceDumper.exe"; fi

        # check diff
        "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" -O=-inline StackTraceDumper.exe > "$STACKTRACE_FILE"
        "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" "$r/net_4_x/mono-symbolicate.exe" "$MSYM_DIR" "$STACKTRACE_FILE" > "$SYMBOLICATE_RAW_FILE"
        tr "\\\\" "/" < "$SYMBOLICATE_RAW_FILE" | sed "s,) .* in .*/mcs/,) in mcs/," | sed "s,) .* in .*/external/,) in external/," | sed '/\[MVID\]/d' | sed '/\[AOTID\]/d' > "$SYMBOLICATE_RESULT_FILE"

        DIFF=$(diff -up "$SYMBOLICATE_EXPECTED_FILE" "$SYMBOLICATE_RESULT_FILE")
        if [ ! -z "$DIFF" ]; then
            echo "ERROR: Symbolicate tests failed."
            echo "If $SYMBOLICATE_RESULT_FILE is correct copy it to $SYMBOLICATE_EXPECTED_FILE."
            echo "Otherwise runtime sequence points need to be fixed."
            echo ""
            echo "$DIFF"
            ok=false
        else
            echo "Success."
        fi
    done

    if [ "$ok" = "true" ]; then
        echo "<?xml version=\"1.0\" encoding=\"utf-8\"?><assemblies><assembly name=\"symbolicate\" environment=\"Mono\" test-framework=\"custom\" run-date=\"$(date +%F)\" run-time=\"$(date +%T)\" total=\"1\" passed=\"1\" failed=\"0\" skipped=\"0\" errors=\"0\" time=\"0\"><collection total=\"1\" passed=\"1\" failed=\"0\" skipped=\"0\" name=\"Test collection for symbolicate\" time=\"0\"><test name=\"symbolicate.all\" type=\"symbolicate\" method=\"all\" time=\"0\" result=\"Pass\"></test></collection></assembly></assemblies>" > "${xunit_results_path}";
        exit 0
    else
        echo "<?xml version=\"1.0\" encoding=\"utf-8\"?><assemblies><assembly name=\"symbolicate\" environment=\"Mono\" test-framework=\"custom\" run-date=\"$(date +%F)\" run-time=\"$(date +%T)\" total=\"1\" passed=\"0\" failed=\"1\" skipped=\"0\" errors=\"0\" time=\"0\"><collection total=\"1\" passed=\"0\" failed=\"1\" skipped=\"0\" name=\"Test collection for symbolicate\" time=\"0\"><test name=\"symbolicate.all\" type=\"symbolicate\" method=\"all\" time=\"0\" result=\"Fail\"><failure exception-type=\"SymbolicateException\"><message><![CDATA[Symbolicate tests failed. Check the log for more details.]]></message></failure></test></collection></assembly></assemblies>" > "${xunit_results_path}";
        exit 1
    fi

fi

if [ "$test_suite" = "--csi" ]; then
    cd tests/csi || error 1
    echo "Console.WriteLine (\"hello world: \" + DateTime.Now)" > csi-test.csx

    ok=true
    "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" csi.exe csi-test.csx > csi-test-output.txt || ok=false
    cat csi-test-output.txt && grep -q "hello world" csi-test-output.txt || ok=false

    if [ "$ok" = "true" ]; then
        echo "<?xml version=\"1.0\" encoding=\"utf-8\"?><assemblies><assembly name=\"csi\" environment=\"Mono\" test-framework=\"custom\" run-date=\"$(date +%F)\" run-time=\"$(date +%T)\" total=\"1\" passed=\"1\" failed=\"0\" skipped=\"0\" errors=\"0\" time=\"0\"><collection total=\"1\" passed=\"1\" failed=\"0\" skipped=\"0\" name=\"Test collection for csi\" time=\"0\"><test name=\"csi.all\" type=\"csi\" method=\"all\" time=\"0\" result=\"Pass\"></test></collection></assembly></assemblies>" > "${xunit_results_path}";
        exit 0
    else
        echo "<?xml version=\"1.0\" encoding=\"utf-8\"?><assemblies><assembly name=\"csi\" environment=\"Mono\" test-framework=\"custom\" run-date=\"$(date +%F)\" run-time=\"$(date +%T)\" total=\"1\" passed=\"0\" failed=\"1\" skipped=\"0\" errors=\"0\" time=\"0\"><collection total=\"1\" passed=\"0\" failed=\"1\" skipped=\"0\" name=\"Test collection for csi\" time=\"0\"><test name=\"csi.all\" type=\"csi\" method=\"all\" time=\"0\" result=\"Fail\"><failure exception-type=\"CsiException\"><message><![CDATA[csi.exe tests failed. Check the log for more details.]]></message></failure></test></collection></assembly></assemblies>" > "${xunit_results_path}";
        exit 1
    fi

fi

if [ "$test_suite" = "--profiler" ]; then
    cd tests/profiler || exit 1

    chmod +x "$r/mprof-report"
    perl ptestrunner.pl out-of-tree xunit "${xunit_results_path}"
    exit $?
fi

if [ "$test_suite" = "--runtime" ]; then
    cd tests/runtime || exit 1

    # TODO: only ported runtest-managed for now
    "${MONO_EXECUTABLE}" --config "$r/_tmpinst/etc/mono/config" --debug test-runner.exe --verbose --xunit "${xunit_results_path}" --config tests-config --runtime "${MONO_EXECUTABLE}" --mono-path "$r/net_4_x" -j a --testsuite-name "runtime" --timeout 300 --disabled "$DISABLED_TESTS" --input-file runtime-test-list.txt
    exit $?
fi
