#!/bin/sh

if [ $# -eq 0 ]; then
	echo "You should give a list of test names such as: "
	echo "$0 System.IO.FileTest System.Text.StringBuilderTest"
	echo "or"
	echo "$0 System.AllTests"
	echo "and so on..."
	exit 1
fi

topdir=../../..
NUNITCONSOLE=${topdir}/nunit/src/NUnitConsole/NUnitConsole_mono.exe
NUNIT_MONO_PATH=${topdir}/nunit/src/NUnitCore:.

for i in $@; do
	MONO_PATH=../../../nunit/src/NUnitCore:. \
		mono ${NUNITCONSOLE} MonoTests.${i},corlib_linux_test.dll
done

