#!/bin/bash

llvm_config=$1
extra_libs="${@:2}"

llvm_api_version=`$llvm_config --mono-api-version` || "0"
with_llvm=`$llvm_config --prefix`

llvm_config_cflags=`$llvm_config --cflags`

if [[ $llvm_config_cflags = *"stdlib=libc++"* ]]; then
llvm_libc_c="-stdlib=libc++"
llvm_libc_link="-lc++"
else
llvm_libc_c=""
llvm_libc_link="-lstdc++"
fi

# llvm-config --clfags adds warning and optimization flags we don't want
shared_llvm_cflags="-I$with_llvm/include -D__STDC_CONSTANT_MACROS -D__STDC_FORMAT_MACROS -D__STDC_LIMIT_MACROS -DLLVM_API_VERSION=$llvm_api_version $llvm_libc_c"
cxxflag_additions="-std=c++11 -fno-rtti -fexceptions"

ldflags="-L$with_llvm/lib"

llvm_system=`$llvm_config --system-libs`



llvm_core_components=`$llvm_config --libs analysis core bitwriter` 
llvm_old_jit=`$llvm_config --libs mcjit jit 2>>/dev/null`
llvm_new_jit=`$llvm_config --libs orcjit 2>>/dev/null`
llvm_extra=`$llvm_config --libs $extra_libs`
llvm_lib_components="$llvm_core_components $llvm_old_jit $llvm_new_jit $llvm_extra"
     
echo "LLVM_CFLAGS_INTERNAL=$shared_llvm_cflags"
echo "LLVM_CXXFLAGS_INTERNAL=$shared_llvm_cflags $cxxflag_additions"
echo "LLVM_LDFLAGS_INTERNAL=$ldflags"
echo "LLVM_LIBS_INTERNAL=$llvm_lib_components $ldflags $llvm_system $llvm_libc_link"







 

