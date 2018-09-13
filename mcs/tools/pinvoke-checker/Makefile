thisdir = tools/pinvoke-checker
SUBDIRS =
include ../../build/rules.make

PROGRAM = pinvoke-checker.exe
NO_INSTALL = yes

API = $(filter basic build, $(PROFILE))
ifdef API
# It can be run using system .net during boostrap
TARGET_NET_REFERENCE = v4.6
# Trick to make it work during boostrap where it has to run with system
# assemblies not the ones in lib/basic folder
PROGRAM_USE_INTERMEDIATE_FILE = 1
endif

LIB_REFS = System Mono.Cecil

ifdef MCS_MODE
LIB_REFS += Mono.Cecil.Mdb
endif

include ../../build/executable.make

IOS_BUILD_OUT = ../../../sdks/out/ios-sim64-release
MAC_BUILD_OUT = ../../../../../builds/install/mac64

MARTIN_MONOTOUCH_ARGS = \
	--runtime=$(IOS_BUILD_OUT)/lib/libmonosgen-2.0.1.dylib \
	--dylib=System.Security.Cryptography.Native.Apple:$(IOS_BUILD_OUT)/lib/libmono-apple-crypto.0.dylib \
	../../class/lib/monotouch/System.dll

MARTIN_LINUX_ARGS = \
	--runtime=../../mono/mini/.libs/libmonosgen-2.0.so.1 \
	--dylib=System.Security.Cryptography.Native.OpenSsl:../../../mono/btls/build-shared/libmono-btls-shared.so \
	--dylib=libmono-btls-shared:../../../mono/btls/build-shared/libmono-btls-shared.so \
	--dylib=System.Native:../../../mono/metadata/.libs/libmono-system-native.a \
	../../class/lib/net_4_x/System.dll

MARTIN_XAMMAC_ARGS = \
	--runtime=$(MAC_BUILD_OUT)/lib/libmonosgen-2.0.1.dylib \
	--dylib=System.Security.Cryptography.Native.Apple:$(MAC_BUILD_OUT)/lib/libmono-apple-crypto.0.dylib \
	../../class/lib/xammac/System.dll

martin-monotouch::
	$(MAKE) V=1 PROFILE=basic
	mono --debug ../../class/lib/basic/pinvoke-checker.exe $(MARTIN_MONOTOUCH_ARGS)

martin-linux::
	$(MAKE) V=1 PROFILE=basic
	mono --debug ../../class/lib/basic/pinvoke-checker.exe $(MARTIN_LINUX_ARGS)

martin-xammac::
	$(MAKE) V=1 PROFILE=basic
	mono --debug ../../class/lib/basic/pinvoke-checker.exe $(MARTIN_XAMMAC_ARGS)

