#!/bin/bash

#
# This script will download and install the dependencies needed for compiling
# mono on cygwin
#

# Check for required packages

commands="wget unzip automake autoconf libtool make bison"

failed=0
for i in $commands; do
	if ! which $i > /dev/null 2>&1; then 
		echo "You must have the '$i' package installed."
		failed=1
	fi
done

if [ $failed = 1 ]; then
	exit 1
fi

dir=cygwin-deps
mkdir -p $dir

echo -n "Downloading deps... "
if [ ! -f $dir/gettext-runtime-0.17-1.zip ]; then
	wget -P $dir http://ftp.gnome.org/pub/gnome/binaries/win32/dependencies/gettext-runtime-0.17-1.zip
fi
if [ ! -f $dir/libiconv-1.13-mingw32-dev.tar.gz ]; then
	wget -P $dir http://sourceforge.net/projects/mingw/files/MinGW/libiconv/libiconv-1.13/libiconv-1.13-mingw32-dev.tar.gz/download
fi
echo "done."

echo -n "Extracting to cygwin-deps/ ..."
(cd $dir && for i in *.zip; do unzip -oq $i || exit 1; done) || exit 1
# This is needed because windows can't use dll's without an x flag.
chmod a+x $dir/bin/*.dll
echo "done."

echo -n "Patching PC files... "
prefix=$PWD/$dir
find $dir -name "*.pc" > $dir/pc-files
for i in `cat $dir/pc-files`; do
	(sed -e "s,^prefix=.*,prefix=$prefix,g" < $i > $i.tmp && mv $i.tmp $i) || exit 1
done
rm -f $dir/pc-files
echo "done."

# Create an environment shell file
rm -f $dir/env.sh
echo "export PKG_CONFIG_PATH=\"$PWD/$dir/lib/pkgconfig:\$PKG_CONFIG\"" >> $dir/env.sh
echo "export PATH=\"$PWD/$dir/bin:\$PATH\"" >> $dir/env.sh

echo "Source $dir/env.sh into your environment using:"
echo ". $dir/env.sh"
echo "Then run mono's configure."
