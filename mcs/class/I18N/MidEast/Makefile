thisdir = class/I18N/MidEast
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = I18N.MidEast.dll
LIB_REFS = I18N
KEYFILE = ../../mono.pub
LOCAL_MCS_FLAGS = /unsafe
TEST_LIB_REFS = I18N

TEST_RESOURCE_FILES = $(wildcard Test/texts/*.txt)
TEST_MCS_FLAGS = $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

EXTRA_DISTFILES = $(wildcard *.ucm) \
	$(TEST_RESOURCE_FILES)

include ../../../build/library.make
