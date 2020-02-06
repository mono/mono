#!/usr/bin/env bash

set -x

if [ $# -ne 2 ]; then
	echo "Usage: $0 <roslyn_out_dir> <roslyn version>"
	exit 1
fi

MONO_DIR=$PWD
ROSLYN_OUT_DIR=$1
PACKAGES_TMP=$(mktemp -d)
PACKAGE_NAME="Microsoft.Net.Compilers.Toolset"
PACKAGE_VERSION=$2

NUPKG_PATH=$PACKAGES_TMP/${PACKAGE_NAME}.${PACKAGE_VERSION}.nupkg
NUPKG_URL="https://dotnet.myget.org/F/roslyn/api/v2/package/$PACKAGE_NAME/$PACKAGE_VERSION"

if command -v curl > /dev/null; then
	curl -f -L $NUPKG_URL -o $NUPKG_PATH || exit 1
else
	wget $NUPKG_URL -O $NUPKG_PATH || exit 1
fi

unzip -q $NUPKG_PATH -d $PACKAGES_TMP || exit 1

ROSLYN_BINARIES_DIR=${PACKAGES_TMP}/tasks/net472
cp $MONO_DIR/csi.fixed.rsp $ROSLYN_BINARIES_DIR/csi.rsp

FILES=`find $ROSLYN_BINARIES_DIR -type f -d 1`
test -z "$FILES" && exit 1

mkdir -p $ROSLYN_OUT_DIR
chmod -x $FILES || exit 1
cp $FILES $ROSLYN_OUT_DIR || exit 1

echo $PACKAGE_VERSION > $ROSLYN_OUT_DIR/version.txt

rm -Rf $PACKAGES_TMP
