thisdir = tools/macpack
SUBDIRS =

RESOURCE_FILES = \
	LOADER \
	PLIST
include ../../build/rules.make

LOCAL_MCS_FLAGS = \
	/r:Mono.GetOptions.dll \
	$(RESOURCE_FILES:%=/resource:%)

PROGRAM = macpack.exe

EXTRA_DISTFILES = $(RESOURCE_FILES)

include ../../build/executable.make
