#!/bin/bash

TESTSUITEDIR=d:/cygwin/home/Administrator/mcs/mbas/testsuite
MBAS=d:/cygwin/home/Administrator/mcs/mbas/mbas.exe
MONO=
REFS=--reference=./TestUtils.dll

NUMTESTS=0
NUMOK=0
NUMFAIL=0

setup()
{	
	echo -n "Preparing ... "
	rm $TESTSUITEDIR/*.exe $TESTSUITEDIR/*.dll -f
	COMPILE_OUTPUT=$($MONO $MBAS --target=library TestUtils.vb)
	if [ $? -ne 0 ] ; then
		echo "ERROR: Can't Compile TestUtils.vb"
		exit
	fi
	echo "OK"
	echo "======================================"
}	

runtest()
{	
	((NUMTESTS++))
	echo "Testing : $1"
	echo -n "Compiling $2.vb ... "
	COMPILE_OUTPUT=$($MONO $MBAS $REFS $2'.vb')
	
	if [ $? -eq 0 ] ; then
		echo "OK"
		echo -n "Running $2.exe .... "
		if [ -z "$MONO" ] ; then
			TEST_OUTPUT=$($TESTSUITEDIR/$2'.exe')
		else
			TEST_OUTPUT=$($MONO $2'.exe')
		fi
		
		if [ "$TEST_OUTPUT" = "$3" ] ; then
			echo "OK"
			echo "======================================"
			((NUMOK++))
		else
			echo "FAILED!"
			echo "======================================"
			((NUMFAIL++))
		fi
	else
		echo "FAILED!"
		echo "======================================"
		((NUMFAIL++))
	fi
}

echo "Mono/mBas test suite started on $(date)"
echo "======================================"

setup

runtest "Parameter passing / Optional params / Sub-Function Call / Function return codes" "paramtest" "D9QEGF5FD+HDj50sLrT/SQ=="
runtest "Attributes / Properties" "attrtest" "2Q9TgBdW+1YeA8GGNBWmLg=="
runtest "Arrays" "arraytest" "nMEt9i1X6nZXZIfkjYwwdA=="
runtest "Enums / Structures" "enum_struct_test" "bE/VX3M3dUY4IMHr440VCw=="

echo "$NUMTESTS tests done"
echo "$NUMOK OK"
echo "$NUMFAIL failed"
