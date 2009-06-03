thisdir = class/System.Security
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Security.dll
LIB_MCS_FLAGS = -nowarn:618 \
	-r:$(corlib) -r:System.dll -r:System.Xml.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:169,219,1595

ifneq (1, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += -r:Mono.Security.dll -nowarn:414
TEST_MCS_FLAGS += -nowarn:168,183,414
endif

EXTRA_DISTFILES = \
	Test/System.Security.Cryptography.Xml/sample.pfx \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample1.xml \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample2.xml \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample3.xml

include ../../build/library.make

# run the PKITS tests only if the data was installed/activated, otherwise ignore them
ifeq (net_2_0, $(PROFILE))
pkits_files := $(wildcard ../System/Test/System.Security.Cryptography.X509Certificates/pkits/hint)
ifndef pkits_files
TEST_HARNESS_EXCLUDES = -exclude:NotWorking,ValueAdd,CAS,InetAccess,PKITS
TEST_HARNESS_EXCLUDES_ONDOTNET = -exclude:NotDotNet,CAS,PKITS
endif
endif
