#!/usr/bin/env bash

set -ex

LLVM_REV=$1
JENKINS_LANE=$2

URL=http://xamjenkinsartifact.blob.core.windows.net/$JENKINS_LANE/llvm-osx64-$LLVM_REV.tar.gz

curl --output tmp36.tar.gz $URL

rm -rf llvm-tmp36
mkdir -p llvm-tmp36
tar -xC llvm-tmp36 -f tmp36.tar.gz
rm -rf ../out/ios-llvm36-32
mkdir -p ../out/llvm-llvm36-32
cp -r llvm-tmp36/usr32/* ../out/llvm-llvm36-32
cp -r llvm-tmp36/usr64/bin/{llc,opt} ../out/llvm-llvm36-32/bin/
rm -rf llvm-tmp36 tmp36.tar.gz
cd ../out
ln -sf llvm-llvm36-32 ios-llvm36-32

