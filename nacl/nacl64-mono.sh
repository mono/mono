#!/bin/bash

# nacl64-mono.sh
#
# usage:  nacl64-mono.sh
#
# this script builds a compiler for 64-bit NaCl code
# (installed in ./compiler folder)
#

TARGET_BITSIZE=64

source common.sh
source nacl-common.sh

readonly MONO_TRUNK_NACL=$(pwd)

readonly PACKAGE_NAME=nacl64-mono-build

readonly INSTALL_PATH=${NACL_SDK_USR}

CustomConfigureStep() {
  Banner "Configuring ${PACKAGE_NAME}"
  set +e
  cd ${PACKAGE_NAME}
  make distclean
  cd ${MONO_TRUNK_NACL}
  set -e
  Remove ${PACKAGE_NAME}
  MakeDir ${PACKAGE_NAME}
  cd ${PACKAGE_NAME}
  cp ../nacl64-mono-config-cache ../nacl64-mono-config-cache.temp
  if [ $HOST_BITSIZE = "64" ]; then
    ../../configure \
      CC='cc -m32' CXX='g++ -m32' \
      --host=i386-pc-linux \
      --build=amd64-pc-linux \
      --target=nacl64 \
      --prefix=${INSTALL_PATH} \
      --exec-prefix=${INSTALL_PATH} \
      --with-tls=pthread \
      --enable-nacl-codegen \
      --disable-mono-debugger \
      --disable-mcs-build \
      --with-sigaltstack=no \
      --with-sgen=no \
      --cache-file=../nacl64-mono-config-cache.temp
  else
    ../../configure \
      --target=nacl64 \
      --prefix=${INSTALL_PATH} \
      --exec-prefix=${INSTALL_PATH} \
      --with-tls=pthread \
      --enable-nacl-codegen \
      --disable-mono-debugger \
      --disable-mcs-build \
      --with-sigaltstack=no \
      --with-sgen=no \
      --cache-file=../nacl64-mono-config-cache.temp
  fi
  

  rm ../nacl64-mono-config-cache.temp
}

CustomPackageInstall() {
  CustomConfigureStep
  DefaultBuildStep
  DefaultInstallStep
}

CustomPackageInstall
exit 0
