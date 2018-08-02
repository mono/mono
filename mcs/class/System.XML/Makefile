thisdir = class/System.XML
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Xml.dll

TXT_RESOURCE_STRINGS = \
	../referencesource/System.Xml/System.Xml.txt \
	../referencesource/System.Data.SqlXml/System.Xml.Utils.txt
RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Private.Xml/src/Resources/Strings.resx

LIB_MCS_FLAGS = -nowarn:219,414,649,1717 -unsafe -d:ASYNC

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:AGCLR -d:DISABLE_XSLT_COMPILER -d:DISABLE_XSLT_SCRIPT,MONO_HYBRID_SYSTEM_XML -d:DISABLE_CAS_USE
endif

TEST_LIB_REFS = System.Data System.Core
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169
XTEST_LIB_REFS = System System.Core System.Xml System.Xml.Linq System.Drawing Facades/System.Threading.Tasks System.Runtime.Serialization Facades/System.Text.Encoding.CodePages

LIBRARY_WARN_AS_ERROR = yes

LIB_REFS = System

ifndef MOBILE_PROFILE
API_BIN_REFS += System.Configuration
LIB_MCS_FLAGS += -d:CONFIGURATION_DEP
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
