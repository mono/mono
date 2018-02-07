#!/usr/bin/env bash

set -ex

LLVM_REV=$1

URL=http://xamjenkinsartifact.blob.core.windows.net/build-package-osx-llvm/llvm-osx64-$LLVM_REV.tar.gz

wget -O tmp.tar.gz --show-progress $URL

rm -rf llvm-tmp
mkdir -p llvm-tmp
tar -xC llvm-tmp -f tmp.tar.gz
rm -rf ../out/ios-{llvm32,llvm64}
mkdir -p ../out/ios-llvm32 ../out/ios-llvm64
cp -r llvm-tmp/usr64/* ../out/ios-llvm64
cp -r llvm-tmp/usr32/* ../out/ios-llvm32
rm -rf llvm-tmp tmp.tar.gz
