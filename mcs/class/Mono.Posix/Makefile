thisdir = class/Mono.Posix
SUBDIRS = 
include ../../build/rules.make

ASSEMBLY = Mono.Posix.dll

TEST_MCS_FLAGS = /unsafe /nowarn:0219,0618
TEST_LIB_REFS = Mono.Posix System

include ../../build/msbuild.make

update-mappings:
	cp `pkg-config --variable=Sources create-native-map` Mono.Unix.Native
	cp `pkg-config --variable=Programs create-native-map` Mono.Unix.Native
	mono --debug Mono.Unix.Native/create-native-map.exe \
		--library=MonoPosixHelper \
		--rename-namespace Mono.Unix.Native=Mono.Posix \
		$(the_lib) Mono.Unix.Native/NativeConvert.generated

