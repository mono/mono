#!/bin/bash

set -e

if [[ ! ${NACL_SDK_ROOT} || ! ${NACL_SDK_ROOT-_} ]]; then
  echo "Set NACL_SDK_ROOT to the location of your Native Client SDK"
  exit 1
fi


if [[ ${TARGET_BITSIZE} && ${TARGET_BITSIZE-_} ]]; then
  echo "Unset TARGET_BITSIZE before running this script"
  exit 1
fi

# 32-bit NaCl AOT cross-compiler
./nacl-mono.sh
# 64-bit NaCl AOT cross-compiler
./nacl64-mono.sh
# 32-bit NaCl Mono runtime (+JIT compiler)
TARGET_BITSIZE=32 ./nacl-runtime-mono.sh
# 64-bit NaCl Mono runtime (+JIT compiler)
TARGET_BITSIZE=64 ./nacl-runtime-mono.sh

