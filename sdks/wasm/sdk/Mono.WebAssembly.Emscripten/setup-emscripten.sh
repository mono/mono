#!/bin/sh

set -e

TARGET_DIR=$1
EMSCRIPTEN_VERSION=$2
PATCH_DIR=$3

echo "SETUP EMSCRIPTEN: $TARGET_DIR - $EMSCRIPTEN_VERSION - $PATCH_DIR!"

if [ -d $TARGET_DIR ]; then
    echo "Target directory exists, exiting."
    exit 0
fi

mkdir -p $TARGET_DIR
git clone https://github.com/juj/emsdk.git $TARGET_DIR
cd $TARGET_DIR && ./emsdk install $EMSCRIPTEN_VERSION
cd $TARGET_DIR && ./emsdk activate --embedded $EMSCRIPTEN_VERSION

cd $TARGET_DIR/upstream/emscripten && (patch -N -p1 < $PATCH_DIR/fix-emscripten-8511.diff; exit 0)
cd $TARGET_DIR/upstream/emscripten && (patch -N -p1 < $PATCH_DIR/emscripten-pr-8457.diff; exit 0)
