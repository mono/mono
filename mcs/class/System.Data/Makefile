thisdir = class/System.Data
SUBDIRS = 
include ../../build/rules.make

# I have *no idea* why I need to do this.

ifeq ($(PROFILE),bootstrap)
system = $(topdir)/class/lib/System.dll
else
system = System.dll
endif

LIBRARY = System.Data.dll
LIB_MCS_FLAGS = \
	/nowarn:0219 /nowarn:0168		\
	/r:$(corlib)				\
	/r:$(system)			 	\
	/r:System.Xml.dll 			\
	/r:System.EnterpriseServices.dll 	\
	/r:Mono.Data.Tds.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = \
	TODO					\
	Test/System.Data/region.xml		\
	Test/System.Data/store.xsd		\
	Test/System.Data/own_schema.xsd		\
	Test/System.Xml/2books.xml		\
	Test/System.Xml/region.xml		\
	Test/System.Xml/region.xsd		\
	Test/System.Xml/store.xsd		\
	Test/NUnit.Prefs

include ../../build/library.make
