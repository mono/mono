thisdir = class/System.Security
SUBDIRS = 
include ../../build/rules.make

ifndef NO_MONO_SECURITY
MONO_SECURITY_DLL=$(the_libdir_base)/Mono.Security.dll
MONO_SECURITY=Mono.Security
endif

LIBRARY = System.Security.dll
LIB_REFS = secxml/System bare/System.Xml $(MONO_SECURITY)
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = \
	-nowarn:414,618 \
	-d:SECURITY_DEP,SYSTEM_SECURITY_DLL

LOCAL_MCS_FLAGS =

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:168,169,183,219,414

EXTRA_DISTFILES = \
	Test/System.Security.Cryptography.Xml/sample.pfx \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample1.xml \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample2.xml \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample3.xml \
	Test/System.Security.Cryptography.Pkcs/detached.data \
	Test/System.Security.Cryptography.Pkcs/detached.p7

RESX_RESOURCE_STRING = ../../../external/corefx/src/System.Security.Cryptography.Xml/src/Resources/Strings.resx

include ../../build/library.make

$(build_lib): $(secxml_libdir)/System.dll $(MONO_SECURITY_DLL)

$(secxml_libdir)/System.dll:
	(cd ../System; $(MAKE) $@)

$(the_libdir_base)/Mono.Security.dll:
	(cd ../Mono.Security; $(MAKE))

.NOTPARALLEL: $(secxml_libdir)/System.dll $(bare_libdir)/Mono.Security.dll
