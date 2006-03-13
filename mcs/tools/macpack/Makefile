thisdir = tools/macpack
SUBDIRS =

RESOURCE_FILES = \
	LOADER \
	PLIST
include ../../build/rules.make

LOCAL_MCS_FLAGS = \
	$(RESOURCE_FILES:%=/resource:%)

PROGRAM = macpack.exe

EXTRA_DISTFILES = $(RESOURCE_FILES)

include ../../build/executable.make
