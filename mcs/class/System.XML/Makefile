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

TXT_RESOURCE_STRINGS = \
	../referencesource/System.Xml/System.Xml.txt \
	../referencesource/System.Data.SqlXml/System.Xml.Utils.txt

LIB_MCS_FLAGS = -nowarn:219,414,649,1717 -unsafe -d:ASYNC

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:AGCLR -d:DISABLE_XSLT_COMPILER -d:DISABLE_XSLT_SCRIPT,MONO_HYBRID_SYSTEM_XML -d:DISABLE_CAS_USE
endif

TEST_LIB_REFS = System.Data System.Core
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169

ifneq (bare/,$(intermediate))
LIB_REFS += secxml/System 
ifndef MOBILE_PROFILE
LIB_REFS += System.Configuration
LIB_MCS_FLAGS += -d:CONFIGURATION_DEP
endif
else
LIB_REFS += $(intermediate)System
endif

nist_dom_files = \
	ITest.cs readme.txt util.cs \
	files/noDTDXMLfile.xml files/otherDoc.xml files/staff.dtd files/staff.html files/staff.xml

xmlfiles_files = \
	nested-included.dtd literal-data.xml nested-dtd-test.dtd nested-dtd-test.xml simple.xml \
	xsl/empty.xsl \
	XsdValidation/1.xsd XsdValidation/2.xsd	XsdValidation/3.xsd XsdValidation/4.xsd \
	xsd/1.xsd xsd/2.xsd xsd/3.xsd xsd/4.xsd xsd/5.xsd xsd/6.xsd xsd/xml.xsd xsd/81360.xsd xsd/81360inc1.xsd xsd/81360inc2.xsd

EXTRA_DISTFILES = \
	$(wildcard System.Xml.Serialization/standalone_tests/*.cs) \
	$(wildcard System.Xml.Serialization/standalone_tests/*.output) \
	Test/XmlFiles/76102.xml		\
	Test/XmlFiles/79683.dtd		\
	Test/XmlFiles/496192.xml	\
	Test/XmlFiles/*.xsd	\
	$(wildcard Test/XmlFiles/xsd/*.xml) \
	$(wildcard Test/XmlFiles/xsd/*.xsd) \
	$(wildcard Test/XmlFiles/xsl/*.xml) \
	$(wildcard Test/XmlFiles/xsl/*.xsl) \
	Test/XmlFiles/xsl/current-in-select.ref \
	$(xmlfiles_files:%=Test/XmlFiles/%) \
	$(nist_dom_files:%=Test/System.Xml/nist_dom/%)

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

ifndef MOBILE_PROFILE
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
