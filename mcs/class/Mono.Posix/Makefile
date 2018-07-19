thisdir = class/Mono.Posix
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Posix.dll
# Don't warn about [Obsolete] members, as there are now *lots* of [Obsolete]
# members, generating volumes of output.
LIB_REFS = System
KEYFILE = ../mono.pub
LIB_MCS_FLAGS = /unsafe /nowarn:0618,612
TEST_MCS_FLAGS = /unsafe /nowarn:0219,0618
TEST_LIB_REFS = Mono.Posix System

LIBRARY_COMPILE = $(BOOT_COMPILE)

include ../../build/library.make

update-mappings:
	cp `pkg-config --variable=Sources create-native-map` Mono.Unix.Native
	cp `pkg-config --variable=Programs create-native-map` Mono.Unix.Native
	mono --debug Mono.Unix.Native/create-native-map.exe \
		--library=MonoPosixHelper \
		--rename-namespace Mono.Unix.Native=Mono.Posix \
		$(the_lib) Mono.Unix.Native/NativeConvert.generated

