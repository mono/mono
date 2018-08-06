#!/usr/bin/env bash

set -ex

LLVM_REV=$1
JENKINS_LANE=$2

URL=http://xamjenkinsartifact.blob.core.windows.net/$JENKINS_LANE/llvm-osx64-$LLVM_REV.tar.gz

curl --output tmp.tar.gz $URL

rm -rf llvm-tmp
mkdir -p llvm-tmp
tar -xC llvm-tmp -f tmp.tar.gz
rm -rf ../out/ios-llvm36-32
mkdir -p ../out/llvm-llvm36-32
cp -r llvm-tmp/usr32/* ../out/llvm-llvm36-32
rm -rf llvm-tmp tmp.tar.gz
cd ../out
ln -sf llvm-llvm36-32 ios-llvm36-32

