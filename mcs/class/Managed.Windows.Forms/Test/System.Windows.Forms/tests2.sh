#!/bin/sh

if [ $# -eq 0 ]; then
	echo "You should give a list of test names such as: "
	echo "$0 System.Windows.Forms.ListViewItemTest"
	echo "or"
	echo "$0 all"
	exit 1
fi

cp ../../System.Windows.Forms_test_net_2_0.dll .

topdir=../../../..
NUNITCONSOLE=$topdir/class/lib/net_2_0/nunit-console.exe
MONO_PATH=$topdir/nunit20:$topdir/class/lib/net_2_0:.


for i in $@; do
	if [ "$i" = "all" ]; then
		fixture=""
	else
		fixture="/fixture:MonoTests.${i}"
	fi
	MONO_PATH=$MONO_PATH \
		mono --debug  ${NUNITCONSOLE} System.Windows.Forms_test_net_2_0.dll $fixture
done
