thisdir = class/System
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.dll

# yeah so. If we're doing a bootstrap build,
# System.Xml.dll doesn't exist yet, so we
# need to add in the /lib: to let mcs find it.
# Or at least, that's the only way I can make this
# work

ifeq ($(PROFILE),bootstrap)
SCARY_LIB=/lib:$(prefix)/lib
else
SCARY_LIB=
endif

LIB_MCS_FLAGS = $(SCARY_LIB) /r:System.Xml.dll /r:$(corlib)

EXTRA_DISTFILES = \
	System.Text.RegularExpressions/notes.txt	\
	System.ComponentModel.Design/Changelog		\
	Test/test-config-file				\
	Test/NUnit.Prefs				\
	Test/argument.txt				\
	Test/foo


include ../../build/library.make
