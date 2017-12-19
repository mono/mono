#!/usr/bin/env bash

set -ex

STAMP_FILE=$1
LLVM_REV=$2

URL=http://xamjenkinsartifact.blob.core.windows.net/build-package-osx-llvm/llvm-osx64-$LLVM_REV.tar.gz

wget -O tmp.tar.gz --show-progress $URL

rm -rf llvm-tmp
mkdir -p llvm-tmp
tar -xC llvm-tmp -f tmp.tar.gz
rm -rf ../out/{llvm32,llvm64}
mkdir -p ../out/llvm32 ../out/llvm64
cp -r llvm-tmp/usr64/* ../out/llvm64
cp -r llvm-tmp/usr32/* ../out/llvm32
rm -rf llvm-tmp tmp.tar.gz

touch $STAMP_FILE



