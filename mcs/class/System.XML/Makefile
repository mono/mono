thisdir = class/System.XML
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Xml.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

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

LIB_MCS_FLAGS = -r:$(corlib) -r:System.dll -nowarn:0162,0618,0612,0642,1595
ifeq (net_1_1_java, $(PROFILE))
LIB_MCS_FLAGS += \
	-r:System.Xml.dll		\
	-r:rt.dll			\
	-r:J2SE.Helpers.dll		\
	-r:unresolved.dll		\
	/lib:../../class/lib/$(PROFILE)
endif
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169

nist_dom_files = \
	ChangeLog ITest.cs readme.txt util.cs \
	fundamental/ChangeLog \
	files/ChangeLog files/noDTDXMLfile.xml files/otherDoc.xml files/staff.dtd files/staff.html files/staff.xml

xmlfiles_files = \
	ChangeLog nested-included.dtd literal-data.xml nested-dtd-test.dtd nested-dtd-test.xml simple.xml \
	xsl/ChangeLog xsl/empty.xsl \
	XsdValidation/ChangeLog XsdValidation/1.xsd XsdValidation/2.xsd	XsdValidation/3.xsd XsdValidation/4.xsd \
	xsd/ChangeLog xsd/1.xsd xsd/2.xsd xsd/3.xsd xsd/4.xsd xsd/5.xsd xsd/6.xsd xsd/xml.xsd

EXTRA_DISTFILES = \
	Mono.System.XML.csproj		\
	Mono.System.XML.sln		\
	README				\
	System.Xml.Schema/BUGS-MS.txt	\
	System.Xml.Schema/BUGS.txt	\
	$(wildcard System.Xml.Serialization/standalone_tests/*.cs) \
	$(wildcard System.Xml.Serialization/standalone_tests/*.output) \
	System.Xml.XPath/Parser.jay	\
	Test/ChangeLog			\
	Test/Microsoft.Test.csproj	\
	Test/Mono.Test.csproj		\
	Test/MonoMicro.Test.csproj	\
	$(xmlfiles_files:%=Test/XmlFiles/%) \
	$(nist_dom_files:%=Test/System.Xml/nist_dom/%)

System.Xml.XPath/Parser.cs: System.Xml.XPath/Parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ct < $(topdir)/jay/skeleton.cs $< >$@

Mono.Xml.Xsl/PatternParser.jay: System.Xml.XPath/Parser.jay $(topdir)/jay/skeleton.cs
	sed "s/\%start Expr/\%start Pattern/" $< >$@

Mono.Xml.Xsl/PatternParser.cs: Mono.Xml.Xsl/PatternParser.jay $(topdir)/jay/skeleton.cs
	echo "#define XSLT_PATTERN" > $@
	$(topdir)/jay/jay -ct $< < $(topdir)/jay/skeleton.cs >>$@

Mono.Xml.Xsl/PatternTokenizer.cs: System.Xml.XPath/Tokenizer.cs
	echo "#define XSLT_PATTERN" > $@
	cat $< >>$@

BUILT_SOURCES = System.Xml.XPath/Parser.cs \
	Mono.Xml.Xsl/PatternParser.cs \
	Mono.Xml.Xsl/PatternTokenizer.cs

CLEAN_FILES = Test/XmlFiles/xsl/result.xml \
	System.Xml.XPath/Parser.cs \
	Mono.Xml.Xsl/PatternParser.cs \
	Mono.Xml.Xsl/PatternTokenizer.cs

include ../../build/library.make
