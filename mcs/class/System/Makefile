thisdir = class/System
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.dll

# yeah so. If we're doing a default build,
# System.Xml.dll doesn't exist yet, so we
# need to add in the /lib: to let mcs find it.
# Or at least, that's the only way I can make this
# work

ifeq ($(PROFILE),atomic)
SCARY_LIB=
else
SCARY_LIB=/lib:$(prefix)/lib
endif

LIB_MCS_FLAGS = $(SCARY_LIB) /r:System.Xml.dll /r:$(corlib)

EXTRA_DISTFILES = \
	System.Text.RegularExpressions/notes.txt	\
	System.ComponentModel.Design/Changelog		\
	Test/test-config-file


include ../../build/library.make
