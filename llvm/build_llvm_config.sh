#!/bin/bash

llvm_config=$1
extra_libs="${@:2}"

use_llvm_config=1
llvm_host_win32=0
llvm_host_win32_wsl=0
llvm_host_win32_cygwin=0

function win32_format_path {
	local formatted_path=$1
	if  [[ $llvm_host_win32_wsl = 1 ]] && [[ $1 != "/mnt/"* ]]; then
		# if path is not starting with /mnt under WSL it could be a windows path, convert using wslpath.
		formatted_path="$(wslpath -a "$1")"
	elif [[ $llvm_host_win32_cygwin = 1 ]] && [[ $1 != "/cygdrive/"* ]]; then
		# if path is not starting with /cygdrive under CygWin it could be a windows path, convert using cygpath.
		formatted_path="$(cygpath -a "$1")"
	fi
	echo "$formatted_path"
}

if [[ $llvm_config = *".exe" ]]; then
	llvm_host_win32=1
	# llvm-config is a windows binary. Check if we are running CygWin or WSL since then we might still be able to run llvm-config.exe
	host_uname="$(uname -a)"
	case "$host_uname" in
		*Microsoft*)
			use_llvm_config=1
			llvm_host_win32_wsl=1
			;;
		CYGWIN*)
			use_llvm_config=1
			llvm_host_win32_cygwin=1
			;;
		*)
			use_llvm_config=0
	esac
fi

if [[ $llvm_host_win32 = 1 ]]; then
	llvm_config=$(win32_format_path "$llvm_config")
fi

if [[ $use_llvm_config = 1 ]]; then
	llvm_api_version=`$llvm_config --mono-api-version` || "0"
	with_llvm=`$llvm_config --prefix`
	llvm_config_cflags=`$llvm_config --cflags`
	llvm_system=`$llvm_config --system-libs`
	llvm_core_components=`$llvm_config --libs analysis core bitwriter`
	llvm_old_jit=`$llvm_config --libs mcjit jit 2>>/dev/null`
	llvm_new_jit=`$llvm_config --libs orcjit 2>>/dev/null`
	llvm_extra=`$llvm_config --libs $extra_libs`

	# When building for Windows subsystem using WSL or CygWin the path returned from llvm-config.exe
	# could be a Windows style path, make sure to format it into a path usable for WSL/CygWin.
	if [[ $llvm_host_win32 = 1 ]]; then
		with_llvm=$(win32_format_path "$with_llvm")
	fi
fi

# When cross compiling for Windows on system not capable of running Windows binaries, llvm-config.exe can't be used to query for
# LLVM parameters. In that scenario we will need to fallback to default values.
if [[ $llvm_host_win32 = 1 ]] && [[ $use_llvm_config = 0 ]]; then
	# Assume we have llvm-config sitting in llvm-install-root/bin directory, get llvm-install-root directory.
	with_llvm="$(dirname $llvm_config)"
	with_llvm="$(dirname $with_llvm)"
	llvm_config_path=$with_llvm/include/llvm/Config/llvm-config.h

	llvm_api_version=`awk '/MONO_API_VERSION/ { print $3 }' $llvm_config_path`
	llvm_config_cflags=
	llvm_system="-lshell32 -lpsapi -limagehlp"
	llvm_core_components="-lLLVMBitWriter -lLLVMAnalysis -lLLVMTarget -lLLVMMC -lLLVMCore -lLLVMSupport"
	llvm_old_jit="-lLLVMJIT -lLLVMCodeGen -lLLVMScalarOpts -lLLVMInstCombine -lLLVMTransformUtils -lLLVMipa -lLLVMAnalysis -lLLVMMCJIT -lLLVMTarget -lLLVMRuntimeDyld -lLLVMObject -lLLVMMCParser -lLLVMBitReader -lLLVMExecutionEngine -lLLVMMC -lLLVMCore -lLLVMSupport"
	llvm_new_jit=
	llvm_extra=
fi

if [[ $llvm_config_cflags = *"stdlib=libc++"* ]]; then
	llvm_libc_c="-stdlib=libc++"
	llvm_libc_link="-lc++"
else
	llvm_libc_c=""
	llvm_libc_link="-lstdc++"
fi

if [[ $llvm_host_win32 = 1 ]]; then
	host_cxxflag_additions="-std=gnu++11"
	host_cflag_additions="-DNDEBUG"
else
	host_cxxflag_additions="-std=c++11"
	host_cflag_additions=""
fi

# llvm-config --clfags adds warning and optimization flags we don't want
cflags_additions="-I$with_llvm/include -D__STDC_CONSTANT_MACROS -D__STDC_FORMAT_MACROS -D__STDC_LIMIT_MACROS -DLLVM_API_VERSION=$llvm_api_version $llvm_libc_c $host_cflag_additions"

cxxflag_additions="-fno-rtti -fexceptions $host_cxxflag_additions"

ldflags="-L$with_llvm/lib"

llvm_lib_components="$llvm_core_components $llvm_old_jit $llvm_new_jit $llvm_extra"

echo "LLVM_CFLAGS_INTERNAL=$cflags_additions"
echo "LLVM_CXXFLAGS_INTERNAL=$cflags_additions $cxxflag_additions"
echo "LLVM_LDFLAGS_INTERNAL=$ldflags"
echo "LLVM_LIBS_INTERNAL=$llvm_lib_components $ldflags $llvm_system $llvm_libc_link"
