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

LIB_MCS_FLAGS = $(SCARY_LIB) /unsafe /r:$(corlib) /r:System.dll /nowarn:0162 /nowarn:0618
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

CLEAN_FILES = Test/XmlFiles/xsl/result.xml System.Xml.XPath/Parser.cs System.Xml.Query/XQueryParser.cs

include ../../build/library.make
