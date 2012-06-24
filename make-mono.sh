#!/bin/bash
rm mono-win32-*.zip
export CPPFLAGS="-Os"
export LDFLAGS=""
export MAKEFLAGS="-j 4"
./build-mingw32.sh -d /usr/i586-mingw32msvc -m i586-mingw32msvc
cp mono-win32-*.zip /media/sf_sabertooth-docs

