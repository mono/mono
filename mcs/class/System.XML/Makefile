thisdir = class/System.XML
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Xml.dll

lib_file := $(wildcard ../lib/$(PROFILE)/System.Xml.dll)
ifndef lib_file
USE_BOOT_COMPILE = yes
endif

ifdef USE_BOOT_COMPILE
LIBRARY_COMPILE = $(BOOT_COMPILE)
endif

PROFILE_ANY_MOBILE := $(filter monotouch monotouch_runtime monodroid xammac mobile mobile_static, $(PROFILE))

LIB_MCS_FLAGS = -r:$(corlib) -r:System.dll -nowarn:0618,0612,0642
ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -unsafe -d:AGCLR -d:NET_2_1_HACK
endif
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169 -r:System.Data.dll -r:System.Core.dll

ifndef PROFILE_ANY_MOBILE
FINAL_MCS_FLAGS = -r:System.Configuration.dll -d:CONFIGURATION_DEP
endif

ifneq (bare/,$(intermediate))
LIB_MCS_FLAGS += $(FINAL_MCS_FLAGS)
endif

LOCAL_MCS_FLAGS += -lib:$(bare_libdir)

nist_dom_files = \
	ChangeLog ITest.cs readme.txt util.cs \
	fundamental/ChangeLog \
	files/ChangeLog files/noDTDXMLfile.xml files/otherDoc.xml files/staff.dtd files/staff.html files/staff.xml

xmlfiles_files = \
	ChangeLog nested-included.dtd literal-data.xml nested-dtd-test.dtd nested-dtd-test.xml simple.xml \
	xsl/ChangeLog xsl/empty.xsl \
	XsdValidation/ChangeLog XsdValidation/1.xsd XsdValidation/2.xsd	XsdValidation/3.xsd XsdValidation/4.xsd \
	xsd/ChangeLog xsd/1.xsd xsd/2.xsd xsd/3.xsd xsd/4.xsd xsd/5.xsd xsd/6.xsd xsd/xml.xsd xsd/81360.xsd xsd/81360inc1.xsd xsd/81360inc2.xsd

EXTRA_DISTFILES = \
	System.Xml.Schema/BUGS-MS.txt	\
	System.Xml.Schema/BUGS.txt	\
	$(wildcard System.Xml.Serialization/standalone_tests/*.cs) \
	$(wildcard System.Xml.Serialization/standalone_tests/*.output) \
	System.Xml.XPath/Parser.jay	\
	Test/ChangeLog			\
	Test/Microsoft.Test.csproj	\
	Test/Mono.Test.csproj		\
	Test/MonoMicro.Test.csproj	\
	Test/XmlFiles/76102.xml		\
	Test/XmlFiles/79683.dtd		\
	Test/XmlFiles/496192.xml	\
	Test/XmlFiles/*.xsd	\
	$(wildcard Test/XmlFiles/xsd/*.xml) \
	$(wildcard Test/XmlFiles/xsd/*.xsd) \
	$(wildcard Test/XmlFiles/xsl/*.xml) \
	$(wildcard Test/XmlFiles/xsl/*.xsl) \
	Test/XmlFiles/xsl/current-in-select.ref \
	Test/XmlFiles/xsl/ChangeLog \
	$(xmlfiles_files:%=Test/XmlFiles/%) \
	$(nist_dom_files:%=Test/System.Xml/nist_dom/%)

System.Xml.XPath/$(PROFILE)_Parser.cs: System.Xml.XPath/Parser.jay $(topdir)/jay/skeleton.cs
	(cd System.Xml.XPath; $(topdir)/../jay/jay -ct < $(topdir)/../jay/skeleton.cs Parser.jay > $(PROFILE)_Parser.cs)

Mono.Xml.Xsl/$(PROFILE)_PatternParser.jay: System.Xml.XPath/Parser.jay $(topdir)/jay/skeleton.cs
	sed "s/\%start Expr/\%start Pattern/" $< >$@

Mono.Xml.Xsl/$(PROFILE)_PatternParser.cs: Mono.Xml.Xsl/$(PROFILE)_PatternParser.jay $(topdir)/jay/skeleton.cs
	echo "#define XSLT_PATTERN" > $@
	(cd Mono.Xml.Xsl; $(topdir)/../jay/jay -ct $(PROFILE)_PatternParser.jay < $(topdir)/../jay/skeleton.cs >> $(PROFILE)_PatternParser.cs)

Mono.Xml.Xsl/$(PROFILE)_PatternTokenizer.cs: System.Xml.XPath/Tokenizer.cs
	echo "#define XSLT_PATTERN" > $@
	cat $< >>$@

ifneq (moonlight_raw, $(PROFILE))
BUILT_SOURCES = System.Xml.XPath/$(PROFILE)_Parser.cs \
	Mono.Xml.Xsl/$(PROFILE)_PatternParser.cs \
	Mono.Xml.Xsl/$(PROFILE)_PatternTokenizer.cs

CLEAN_FILES = Test/XmlFiles/xsl/result.xml \
	System.Xml.XPath/$(PROFILE)_Parser.cs \
	Mono.Xml.Xsl/$(PROFILE)_PatternParser.cs \
	Mono.Xml.Xsl/$(PROFILE)_PatternTokenizer.cs
endif

include ../../build/library.make

ifeq (bare/,$(intermediate))
build-bare:
else
$(bare_libdir)/System.Xml.dll:
	$(MAKE) intermediate=bare/ $(bare_libdir)/System.Xml.dll
endif

system_xml_library_deps := 				\
	$(bare_libdir)/System.dll			\
	$(bare_libdir)/System.Xml.dll

ifndef PROFILE_ANY_MOBILE
system_xml_library_deps += $(the_libdir_base)System.Configuration.dll
endif

artifacts = $(system_xml_library_deps)	\
	$(the_libdir_base)System.Configuration.dll

.NOTPARALLEL: $(artifacts)

CLEAN_FILES = $(bare_libdir)/System.Xml.dll $(bare_libdir)/System.Xml.dll.pdb
$(the_libdir_base)System.Xml.dll: $(system_xml_library_deps) 
$(bare_libdir)/System.dll:
	(cd ../System; $(MAKE) $@)
$(bare_libdir)/System.Xml.dll: $(bare_libdir)/System.dll 

$(the_libdir_base)System.Configuration.dll:
	(cd ../System.Configuration; $(MAKE) $@)


ifndef intermediate
ifneq ($(PROFILE),basic)
csproj-local:
	$(MAKE) csproj-local intermediate=bare/
endif
endif
