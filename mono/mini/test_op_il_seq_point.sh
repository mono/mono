#!/bin/bash

DEFAULT_PROFILE=$1
TEST_FILE=$2
USE_AOT=$3

TMP_FILE_PREFIX=$(basename $0).tmp
BASEDIR=$(dirname $0)

case "$(uname -s)" in
	CYGWIN*) PLATFORM_PATH_SEPARATOR=';';;
	*) PLATFORM_PATH_SEPARATOR=':';;
esac

MONO_PATH=$BASEDIR/../../mcs/class/lib/$DEFAULT_PROFILE$PLATFORM_PATH_SEPARATOR$BASEDIR
RUNTIME=$BASEDIR/../../runtime/mono-wrapper

trap "rm -rf ${TMP_FILE_PREFIX}*" EXIT

tmp_file () {
	mktemp ./${TMP_FILE_PREFIX}XXXXXX
}

clean_aot () {
	rm -rf *.exe.so *.exe.dylib *.exe.dylib.dSYM *.exe.dll
}

# The test compares the generated native code size between a compilation with and without seq points.
# In some architectures ie:amd64 when possible 32bit instructions and registers are used instead of 64bit ones.
# Using MONO_DEBUG=single-imm-size avoids 32bit optimizations thus mantaining the native code size between compilations.

get_compile_dump () {
	if [ -z $4 ]; then
		MONO_PATH=$1 $2 -v -v -O=all --compile-all=1 $3
	else
		clean_aot
		MONO_PATH=$1 $2 -v -v -O=all --aot $3
	fi | grep  '^Method .* (code length\|^0[0-9a-f]\+' | sed 's/0x[0-9a-fA-F]*/0x0/g'
}

if [ -z $USE_AOT ]; then
	echo "Checking unintended native code changes in $TEST_FILE without AOT"
else
	echo "Checking unintended native code changes in $TEST_FILE with AOT"
fi

TMP_FILE1=$(tmp_file)
TMP_FILE2=$(tmp_file)
echo "$(MONO_DEBUG=no-compact-seq-points,single-imm-size get_compile_dump $MONO_PATH $RUNTIME $TEST_FILE $USE_AOT)" >$TMP_FILE1
echo "$(MONO_DEBUG=single-imm-size get_compile_dump $MONO_PATH $RUNTIME $TEST_FILE $USE_AOT)" >$TMP_FILE2

LINE_DIFF_COUNT=$(diff -y --suppress-common-lines $TMP_FILE1 $TMP_FILE2 | grep '^' | wc -l)
LINE_DIFF_COUNT_REF=0

# Check if differences are due to JIT compiler beeing undeterminisc
if [ $LINE_DIFF_COUNT != 0 ]; then
	TMP_FILE1_REF=$(tmp_file)
	echo "$(MONO_DEBUG=no-compact-seq-points,single-imm-size get_compile_dump $MONO_PATH $RUNTIME $TEST_FILE $USE_AOT)" >$TMP_FILE1_REF
	LINE_DIFF_COUNT_REF=$(diff -y --suppress-common-lines $TMP_FILE1 $TMP_FILE1_REF| grep '^' | wc -l)
	if [ $LINE_DIFF_COUNT_REF != 0 ]
	then
		$TMP_FILE2=$TMP_FILE1_REF
	fi
fi

TMP_FILE=$(tmp_file)
TMP_METHOD_DIFF_BAD=$(tmp_file)

SDIFF_WIDTH=150
SDIFF_SEP_POS=$(expr $SDIFF_WIDTH / 2 - 1)

sdiff -w $SDIFF_WIDTH $TMP_FILE1 $TMP_FILE2 |
	expand -t 8                   | # Replacing tabs is required for the awk regex to work
	sed 's/^Method/@&/' > $TMP_FILE # Add @ before each method so awk can use it as record separator

# Use awk to print records that include differences
# Using @ as RS because BSD awk ignores all characters after the first one.
awk \
"{
	RS=\"@\";
	FS=\"\n\";
	if(NF<=2) {
		print \$0;
		print \"ERROR: Method native code not found.\"
		exit 1;
	}
	for(i=1; i<=NF; i++) {
		if (match(\$i, /^.{$SDIFF_SEP_POS}[^ ]/)) {
			print \$0;
			break;
		}
	}
}" $TMP_FILE > $TMP_METHOD_DIFF_BAD || FAIL_MESSAGE_FILE=$TMP_METHOD_DIFF_BAD

if [ $LINE_DIFF_COUNT_REF != 0 ]; then
	FAIL_MESSAGE_FILE=$(tmp_file)
	echo "Detected indeterministic JIT compilation on $TEST_FILE" >> $FAIL_MESSAGE_FILE
	echo "  $LINE_DIFF_COUNT lines differ between compilations." >> $FAIL_MESSAGE_FILE
	echo '  This is probably caused by a runtime optimization that is not deterministic' >> $FAIL_MESSAGE_FILE
	echo '' >> $FAIL_MESSAGE_FILE
	echo "$(cat $TMP_METHOD_DIFF_BAD)" >> $FAIL_MESSAGE_FILE
elif [ $LINE_DIFF_COUNT != 0 ]; then
	FAIL_MESSAGE_FILE=$(tmp_file)
	echo "Detected OP_IL_SEQ_POINT incompatibility on $TEST_FILE" >> $FAIL_MESSAGE_FILE
	echo "  $LINE_DIFF_COUNT lines differ when sequence points are enabled." >> $FAIL_MESSAGE_FILE
	echo '  This is probably caused by a runtime optimization that is not handling OP_IL_SEQ_POINT' >> $FAIL_MESSAGE_FILE
	echo '' >> $FAIL_MESSAGE_FILE
	echo "Without IL_OP_SEQ_POINT                                                         With IL_OP_SEQ_POINT" >> $FAIL_MESSAGE_FILE
	echo "$(cat $TMP_METHOD_DIFF_BAD)" >> $FAIL_MESSAGE_FILE
fi

TESTRESULT_FILE=TestResult-op_il_seq_point.tmp
echo -n "              <test-case name=\"MonoTests.op_il_seq_point.${TEST_FILE}${USE_AOT}\" executed=\"True\" time=\"0\" asserts=\"0\" success=\"" >> $TESTRESULT_FILE

if [ $FAIL_MESSAGE_FILE ]; then
	echo "False\">" >> $TESTRESULT_FILE
	echo "                <failure>" >> $TESTRESULT_FILE
	echo -n "                  <message><![CDATA[" >> $TESTRESULT_FILE
	echo "$(cat $FAIL_MESSAGE_FILE)" >> $TESTRESULT_FILE
	echo "]]></message>" >> $TESTRESULT_FILE
	echo "                  <stack-trace>" >> $TESTRESULT_FILE
	echo "                  </stack-trace>" >> $TESTRESULT_FILE
	echo "                </failure>" >> $TESTRESULT_FILE
	echo "              </test-case>" >> $TESTRESULT_FILE

	echo "$(cat $FAIL_MESSAGE_FILE)"
	exit 1
else
	echo "True\" />" >> $TESTRESULT_FILE
fi
