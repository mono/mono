thisdir = class/Mono.Xml.Ext
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Xml.Ext.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

ifeq ($(PROFILE),atomic)
SCARY_LIB=
else
SCARY_LIB=/lib:$(prefix)/lib /noconfig
endif

LIB_MCS_FLAGS = $(SCARY_LIB) /unsafe /r:$(corlib) /r:System.dll /r:System.Xml.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = \
	Mono.Xml.XPath2/XQueryParser.jay

Mono.Xml.XPath2/XQueryParser.cs: Mono.Xml.XPath2/XQueryParser.jay Mono.Xml.XPath2/skeleton-2.0.cs
	$(topdir)/jay/jay -ct < Mono.Xml.XPath2/skeleton-2.0.cs $< >$@

CLEAN_FILES = Mono.Xml.Ext.pdb Mono.Xml.XPath2/XQueryParser.cs y.output

include ../../build/library.make
