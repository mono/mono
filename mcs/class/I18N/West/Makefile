thisdir = class/I18N/West
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = I18N.West.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes
LOCAL_MCS_FLAGS = /r:$(corlib) /r:I18N.dll /unsafe
#NO_TEST = yes

EXTRA_DISTFILES = $(wildcard *.ucm) \
	$(wildcard Test/texts/*.txt)

include ../../../build/library.make
