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
NUNITCONSOLE=$topdir/nunit20/nunit-console.exe
MONO_PATH=$topdir/nunit20:.

for i in $@; do
	MONO_PATH=$MONO_PATH \
		${NUNITCONSOLE} corlib_reference.dll /fixture:MonoTests.${i}
done

