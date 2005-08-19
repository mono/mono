#!/bin/sh

if [ $# -eq 0 ]; then
	echo "You should give a list of test names such as: "
	echo "$0 System.Drawing.TestStringFormat"
	echo "or"
	echo "$0 all"
	exit 1
fi

cp ../../System.Drawing_test_default.dll .

topdir=../../../..
NUNITCONSOLE=$topdir/class/lib/default/nunit-console.exe
MONO_PATH=$topdir/nunit20:$topdir/class/lib/default:.


for i in $@; do
	if [ "$i" = "all" ]; then
		fixture=""
	else
		fixture="/fixture:MonoTests.${i}"
	fi
	MONO_PATH=$MONO_PATH \
		mono --debug ${NUNITCONSOLE} System.Drawing_test_default.dll $fixture
done



