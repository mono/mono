thisdir = tools/macpack
SUBDIRS = 

RESOURCE_FILES = \
	LOADER \
	PLIST
include ../../build/rules.make

LOCAL_MCS_FLAGS = \
	-r:Mono.GetOptions \
	$(RESOURCE_FILES:%=/resource:%)

PROGRAM = macpack.exe

include ../../build/executable.make
