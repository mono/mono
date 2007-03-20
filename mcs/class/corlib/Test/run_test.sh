#!/bin/sh

if [ $# -eq 0 ]; then
	echo "You should give a list of test names such as: "
	echo "$0 System.IO.FileTest System.Text.StringBuilderTest"
	echo "or"
	echo "$0 System.AllTests"
	echo "or"
	echo "$0 all"
	echo "and so on..."
	exit 1
fi

topdir=../../..
NUNITCONSOLE=$topdir/class/lib/net_2_0/nunit-console.exe
MONO_PATH=$topdir/nunit20:$topdir/class/lib:.

for i in $@; do
	if [ "$i" = "all" ]; then
		fixture=""
	else
		fixture="/fixture:MonoTests.${i}"
	fi
	MONO_PATH=$MONO_PATH \
		mono --debug ${NUNITCONSOLE} corlib_test.dll $fixture
done

