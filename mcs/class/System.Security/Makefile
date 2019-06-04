thisdir = class/System.Security
SUBDIRS = 
include ../../build/rules.make

ifndef NO_MONO_SECURITY
MONO_SECURITY = Mono.Security
endif

LIBRARY = System.Security.dll
API_BIN_REFS := System.Numerics System.Core
LIB_REFS = $(MONO_SECURITY) System System.Xml
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = -unsafe -nowarn:414,618 -d:SECURITY_DEP

LOCAL_MCS_FLAGS =

TEST_RESOURCE_FILES = \
	Test/System.Security.Cryptography.Xml/sample.pfx \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample1.xml \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample2.xml \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample3.xml \
	Test/System.Security.Cryptography.Pkcs/detached.data \
	Test/System.Security.Cryptography.Pkcs/detached.p7

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:168,169,183,219,414 $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

XTEST_LIB_REFS = System.Core System Facades/System.Linq Facades/System.Threading.Tasks

EXTRA_DISTFILES = $(TEST_RESOURCE_FILES)

include ../../build/library.make
