thisdir = class/Mono.Posix
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Posix.dll
# Don't warn about [Obsolete] members, as there are now *lots* of [Obsolete]
# members, generating volumes of output.
LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll /nowarn:0618,612
TEST_MCS_FLAGS = /r:Mono.Posix.dll /r:System.dll /nowarn:0219,0618

ifeq (net_4_0, $(PROFILE))
USE_BOOT_COMBILE = yes
LIBRARY_COMPILE = $(BOOT_COMPILE)
endif

include ../../build/library.make

update-mappings:
	cp `pkg-config --variable=Sources create-native-map` Mono.Unix.Native
	cp `pkg-config --variable=Programs create-native-map` Mono.Unix.Native
	mono --debug Mono.Unix.Native/create-native-map.exe \
		--library=MonoPosixHelper \
		--rename-namespace Mono.Unix.Native=Mono.Posix \
		$(the_lib) Mono.Unix.Native/NativeConvert.generated

