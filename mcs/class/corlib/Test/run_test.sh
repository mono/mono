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
NUNITCONSOLE=${topdir}/class/lib/NUnitConsole_mono.exe

for i in $@; do
	MONO_PATH=../../../class/lib:. \
		mono ${NUNITCONSOLE} MonoTests.${i},corlib_linux_test.dll
done

