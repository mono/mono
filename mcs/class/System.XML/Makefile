thisdir = class/System.XML
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Xml.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

ifeq ($(PROFILE),atomic)
SCARY_LIB=
else
SCARY_LIB=/lib:$(prefix)/lib /noconfig
endif

lib_file := $(wildcard ../lib/$(PROFILE)/System.Xml.dll)
ifndef lib_file
# In the basic profile, System.Xml.dll is _not_ a dependency of mcs.exe.  So, don't use boot compilation.
# In other profiles, it _is_ a dependency of mcs.exe.  So, use boot compilation.
ifneq (basic, $(PROFILE))
USE_BOOT_COMPILE = yes
endif
endif

ifeq (net_2_0, $(PROFILE))
BOOTSTRAP_MCS = MONO_PATH="$(topdir)/class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_GMCS)
endif

ifdef USE_BOOT_COMPILE
LIBRARY_COMPILE = $(BOOT_COMPILE)
endif

LIB_MCS_FLAGS = $(SCARY_LIB) /unsafe /r:$(corlib) /r:System.dll /nowarn:0162 /nowarn:0618 /nowarn:0612
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = \
	Mono.System.XML.csproj		\
	Mono.System.XML.sln		\
	README				\
	System.Xml.Schema/BUGS-MS.txt	\
	System.Xml.Schema/BUGS.txt	\
	$(wildcard System.Xml.Serialization/standalone_tests/*.cs) \
	$(wildcard System.Xml.Serialization/standalone_tests/*.output) \
	System.Xml.XPath/Parser.jay	\
	System.Xml.Query/XQueryParser.jay	\
	System.Xml.Query/skeleton-2.0.cs	\
	Test/Microsoft.Test.csproj	\
	Test/Mono.Test.csproj		\
	Test/MonoMicro.Test.csproj	\
	Test/XmlFiles/xsd/xml.xsd	\
	Test/XmlFiles/xsd/ChangeLog	\
	Test/XmlFiles/xsl/empty.xsl	\
	Test/XmlFiles/xsl/ChangeLog	\
	Test/XmlFiles/ChangeLog

System.Xml.XPath/Parser.cs: System.Xml.XPath/Parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ct < $(topdir)/jay/skeleton.cs $< >$@

System.Xml.Query/XQueryParser.cs: System.Xml.Query/XQueryParser.jay System.Xml.Query/skeleton-2.0.cs
	$(topdir)/jay/jay -ct < System.Xml.Query/skeleton-2.0.cs $< >$@

BUILT_SOURCES = System.Xml.XPath/Parser.cs #System.Xml.Query/XQueryParser.cs
CLEAN_FILES = Test/XmlFiles/xsl/result.xml System.Xml.Query/XQueryParser.cs

include ../../build/library.make
