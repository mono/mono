thisdir = class/System.XML
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Xml.dll

TXT_RESOURCE_STRINGS = \
	../referencesource/System.Xml/System.Xml.txt \
	../referencesource/System.Data.SqlXml/System.Xml.Utils.txt
RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Private.Xml/src/Resources/Strings.resx

LIB_MCS_FLAGS = -nowarn:219,414,618,649,1717 -unsafe -d:ASYNC

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:DISABLE_XSLT_COMPILER -d:DISABLE_XSLT_SCRIPT,MONO_HYBRID_SYSTEM_XML
endif

TEST_LIB_REFS = System.Data System.Core
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169
XTEST_LIB_REFS = System System.Core System.Xml System.Xml.Linq Facades/System.Threading.Tasks System.Runtime.Serialization Facades/System.Text.Encoding.CodePages

ifneq (,$(filter build net_4_x, $(PROFILE)))
XTEST_LIB_REFS += System.Drawing
endif

LIBRARY_WARN_AS_ERROR = yes

LIB_REFS = System

ifndef MOBILE_PROFILE
API_BIN_REFS += System.Configuration
LIB_MCS_FLAGS += -d:CONFIGURATION_DEP
endif

nist_dom_files = \
	ITest.cs readme.txt util.cs

TEST_RESOURCE_FILES = \
	Test/System.Xml/nist_dom/files/noDTDXMLfile.xml \
	Test/System.Xml/nist_dom/files/otherDoc.xml \
	Test/System.Xml/nist_dom/files/staff.dtd \
	Test/System.Xml/nist_dom/files/staff.html \
	Test/System.Xml/nist_dom/files/staff.xml \
	Test/XmlFiles/xsl/current-in-select.ref \
	$(wildcard Test/XmlFiles/*.xsd) \
	$(wildcard Test/XmlFiles/*.xml) \
	$(wildcard Test/XmlFiles/*.dtd) \
	$(wildcard Test/XmlFiles/xsd/*.xml) \
	$(wildcard Test/XmlFiles/xsd/*.xsd) \
	$(wildcard Test/XmlFiles/xsl/*.xml) \
	$(wildcard Test/XmlFiles/xsl/*.xsl) \
	$(wildcard Test/XmlFiles/XsdValidation/*.xsd)

TEST_MCS_FLAGS += $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

EXTRA_DISTFILES = \
	$(wildcard System.Xml.Serialization/standalone_tests/*.cs) \
	$(wildcard System.Xml.Serialization/standalone_tests/*.output) \
	$(TEST_RESOURCE_FILES) \
	$(nist_dom_files:%=Test/System.Xml/nist_dom/%)

include ../../build/library.make
