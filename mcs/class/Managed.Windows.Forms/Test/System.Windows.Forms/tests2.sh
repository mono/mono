#!/bin/bash

function usage {
if [ $# -eq 0 ]; then
	echo <<EOF '
	Usage:  '$0' [--nunit] [--prefix] [--monooption] [--test] all|Fixture

	--nunit		   : path to nunit, if you want to use a different one than the default 2.0
	--prefix	   : prefix to use to pass options to nunit. Default is /, newer nunits use - instead
	--monooption   : Options to pass on to mono, like --debug, --trace, etc.
	--test		   : Specific test to run, if the nunit you''re using supports it
	all            : run all tests
	Fixture        : Fixture is the name of the test you want to run. The MonoTests.System.Windows.Forms
					 namespace will be prepended automatically, so you don''t need to add it. You can
					 specify as many fixtures as you want, they will be run one after the other.

	Example:
		'$0' --debug --trace=N:MonoTests.System.Windows.Forms all
		Runs all tests with debug and trace flags, roughly equivalent to:
		"mono --debug --trace=N:MonoTests.System.Windows.Forms nunit.exe System.Windows.Forms_test_net_2_0.dll"
'
EOF
	exit 1
fi
}

cp ../../System.Windows.Forms_test_net_2_0.dll .

topdir=../../../..
NUNITCONSOLE=$topdir/class/lib/net_2_0/nunit-console.exe
MONO_PATH=$topdir/nunit20:$topdir/class/lib/net_2_0:.

opts=""
test=""
prefix="/"
ns="MonoTests."

for i in $@; do
	case $i in
		--prefix*)
			prefix=${i:9}
			shift
		;;
		--nunit*)
			NUNITCONSOLE="${i:8}/nunit-console.exe"
			MONO_PATH="${i:8}:."
			shift
		;;
		--test*)
			test="-run=${i:7}"
			shift
		;;
		-labels)
			NUNITCONSOLE="${NUNITCONSOLE} ${prefix}labels"
			shift
		;;
		-defns)
			ns="MonoTests.System.Windows.Forms."
			shift
		;;
		--*)
			opts="$opts $i"
			shift
	   ;;
	   *) continue ;;
	esac
done

if [ $# -eq 0 ]; then
	usage
	exit 1
fi


for i in $@; do
	case $i in
		all) fixture="" ;;
		*) fixture="${prefix}fixture:${ns}${i}" ;;
	esac
	echo "MONO_PATH=$MONO_PATH mono $opts ${NUNITCONSOLE} System.Windows.Forms_test_net_2_0.dll $fixture $test"
	MONO_PATH=$MONO_PATH mono $opts ${NUNITCONSOLE} System.Windows.Forms_test_net_2_0.dll $fixture $test
done
