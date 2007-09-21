#!/bin/sh
#
# Does various checks for people that we can use to diagnose
# an end user installation
#
set `echo $PATH | sed 's/:/ /g'`

while test x$1 != x; do
    if test -x $1/mono; then
	if test x$monocmd = x; then
	   monocmd=$1/mono
        else
	   other_monos="$1/mono $other_monos"
	fi
    fi
    shift
done

echo Active Mono: $monocmd

if test "x$other_monos" != x; then
	echo "Other Mono executables: $other_monos"
fi


#
# Check that the pkg-config mono points to this mono
#
if pkg-config --modversion mono >& /dev/null; then 
        pkg_config_mono=`(cd \`pkg-config --variable prefix mono\`/bin; pwd)`/mono
	if test $pkg_config_mono != $monocmd; then
	    echo "Error: pkg-config Mono installation points to a different install"
	    echo "       than the Mono found:"
	    echo "       Mono on PATH: $monocmd"
	    echo "       Mono from pkg-config: $pkg_config_mono"
	    exit 1
	fi
else 
        echo "Warning: pkg-config could not find mono installed on this system"
fi

