#!/usr/bin/env bash

set -ex

STAMP_FILE=$1
LLVM_REV=$2

URL=http://xamjenkinsartifact.blob.core.windows.net/build-package-osx-llvm/build/llvm-osx64-$LLVM_REV.tar.gz

wget -O tmp.tar.gz --show-progress $URL

mkdir -p ../out/llvm64
tar -xC ../out/llvm64 -f tmp.tar.gz
rm -f tmp.tar.gz

touch $STAMP_FILE



