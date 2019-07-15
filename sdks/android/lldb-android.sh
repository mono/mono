#!/bin/bash

show_usage()
{
	echo "Usage: lldb-android [-d <DEVICE>] <package>."
}

shell()
{
	adb -s "$DEVICE" shell "$@"
}

while getopts "d:" option ; do
	case "$option" in
		d)
			DEVICE=$OPTARG
			shift 2
			;;
	esac
done

if [ "$1" == "" ]; then
	show_usage
	exit 1
fi

PKG=$1

echo "Package: $PKG"

if [ "$DEVICE" = "" ]; then
	DEVICE_COUNT=`adb devices | grep 'device$' | wc -l`
	if [ $DEVICE_COUNT -eq 1 ]; then
		DEVICE=`adb devices | grep 'device$' | awk -F"\t+" '{print $1}'`
	fi
fi

if [ -z $DEVICE ]; then
	echo "Unable to find a device."
	exit 1
fi

echo "Device: $DEVICE"

for i in 1 2 3 4 5; do
	PID=$(shell "ps" | grep -E "\b$PKG\b$" | awk -F' +' '{print $2}')

	if [ "$PID" != "" ]; then
		break
	fi
	sleep 1
done

if [ "$PID" == "" ]; then
	echo "Can't find process pid."
	exit 1
fi

START_FILE=/tmp/lldb_commands.$DEVICE.$PID

echo "
platform select remote-android
platform connect connect://[$DEVICE]:6101
settings set auto-confirm true
settings set plugin.symbol-file.dwarf.comp-dir-symlink-paths /proc/self/cwd
process attach -p $PID
process handle -p true -n true -s false SIGPWR SIGXCPU SIGTTIN
p (void)monodroid_clear_lldb_wait()" > $START_FILE

lldb -s $START_FILE
