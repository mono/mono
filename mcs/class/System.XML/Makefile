thisdir = class/System.XML
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Xml.dll

LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll
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
	Test/Microsoft.Test.csproj	\
	Test/Mono.Test.csproj		\
	Test/MonoMicro.Test.csproj	\
	Test/XmlFiles/xsd/xml.xsd	\
	Test/XmlFiles/xsd/ChangeLog	\
	Test/XmlFiles/xsl/empty.xsl	\
	Test/XmlFiles/xsl/ChangeLog	\
	Test/XmlFiles/ChangeLog		\
	Test/NUnit.Prefs

CLEAN_FILES = Test/XmlFiles/xsl/result.xml

include ../../build/library.make
