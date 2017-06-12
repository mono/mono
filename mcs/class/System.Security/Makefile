thisdir = class/System.Security
SUBDIRS = 
include ../../build/rules.make

ASSEMBLY = System.Security.dll

ifndef NO_MONO_SECURITY
MONO_SECURITY_DLL=$(the_assemblydir_base)/Mono.Security.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:168,169,183,219,414

EXTRA_DISTFILES = \
	Test/System.Security.Cryptography.Xml/sample.pfx \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample1.xml \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample2.xml \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample3.xml \
	Test/System.Security.Cryptography.Pkcs/detached.data \
	Test/System.Security.Cryptography.Pkcs/detached.p7

RESX_RESOURCE_STRING = ../../../external/corefx/src/System.Security.Cryptography.Xml/src/Resources/Strings.resx

include ../../build/msbuild.make
