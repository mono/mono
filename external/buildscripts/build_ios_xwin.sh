#!/bin/bash

function compile_vcproj ()
{
	sln=$1; shift
	config=$1; shift
	incremental=$1; shift
	proj=$1; shift
	opt=$@

	devenv=`echo "$VS100COMNTOOLS/../IDE/devenv.com" | sed 's.\\\\./.g'`
	devenv=`cygpath "$devenv"`

	if [[ ! -e $devenv ]] ; then echo "Can't find VS 10.0 install (through VS100COMNTOOLS)"; exit 1; fi 
	if [[ $incremental==1 ]] ; then buildCmd="/build"; else buildCmd="/rebuild"; fi
	if [[ -n $proj ]] ; then let params=/project $proj; fi

	echo "Building Mono crosscompiler targetting ARM"
	echo "\"$devenv\" \"$sln\" $buildCmd $config $params $opt"

	eval '"$devenv" "$sln" $buildCmd $config $params $opt'
	res=$?
	echo "result: $res"
	if [[ $res -ne 0 ]] ; then
		echo "Failed to: $devenv $sln $buildCmd $config $params $opt"; exit 1
	fi
}

f_dobuild=1

while [ $# -gt 0 ] ; do
	case "$1" in
		-*skipbuild) f_dobuild=0 ;;
	esac
	
	shift
done

if [[ $f_dobuild -eq 1 ]] ; then
	pushd mono/arch/arm

	bash ./dpiops.sh > arm_dpimacros.ht
	mv arm_dpimacros.ht arm_dpimacros.h

	bash ./fpaops.sh > arm_fpamacros.ht
	mv arm_fpamacros.ht arm_fpamacros.h

	bash ./vfpops.sh > arm_vfpmacros.ht
	mv arm_vfpmacros.ht arm_vfpmacros.h

	popd

	echo "arm_*macros generated successfully"
	root="`dirname \"$0\"`/../.."
	if [[ $UNITY_THISISABUILDMACHINE == 1 ]] ; then
		echo "On a buildserver, deleting $root/builds in order to have clean build\n"
		rm -rf "$root/builds"
	else
		echo "Not deleting $root/builds"
	fi

	compile_vcproj "$root/msvc/mono.sln" "Release_eglib_xarm" 1
fi
	
	
mkdir -p builds/crosscompiler/iphone
cp msvc/Win32-Release_eglib_xarm/mono.exe builds/crosscompiler/iphone/mono-xcompiler.exe

# make sure we have read/write permissions
chmod +rwx builds/crosscompiler/iphone/mono-xcompiler.exe

echo "iPhone cross compiler build done"
