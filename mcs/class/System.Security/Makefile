thisdir = class/System.Security
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Security.dll
LIB_MCS_FLAGS = -nowarn:618 \
	-d:SECURITY_DEP \
	-r:$(corlib) -r:System.dll -r:System.Xml.dll \
	-r:Mono.Security.dll -nowarn:414

LOCAL_MCS_FLAGS = -lib:$(secxml_libdir) -lib:$(bare_libdir)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:168,169,183,219,414,1595

VALID_PROFILE := $(filter net_1_1 net_2_0 moonlight_raw net_4_0 net_4_5, $(PROFILE))

EXTRA_DISTFILES = \
	Test/System.Security.Cryptography.Xml/sample.pfx \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample1.xml \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample2.xml \
	Test/System.Security.Cryptography.Xml/EncryptedXmlSample3.xml \
	Test/System.Security.Cryptography.Pkcs/detached.data \
	Test/System.Security.Cryptography.Pkcs/detached.p7

include ../../build/library.make

$(build_lib): $(secxml_libdir)/System.dll $(the_libdir_base)/Mono.Security.dll

$(secxml_libdir)/System.dll:
	(cd ../System; $(MAKE) $@)

$(the_libdir_base)/Mono.Security.dll:
	(cd ../Mono.Security; $(MAKE))

.NOTPARALLEL: $(secxml_libdir)/System.dll $(bare_libdir)/Mono.Security.dll

# run the PKITS tests only if the data was installed/activated, otherwise ignore them
ifeq (net_2_0, $(PROFILE))
pkits_files := $(wildcard ../System/Test/System.Security.Cryptography.X509Certificates/pkits/hint)
ifndef pkits_files
TEST_HARNESS_EXCLUDES = -exclude:NotWorking,ValueAdd,CAS,InetAccess,PKITS
TEST_HARNESS_EXCLUDES_ONDOTNET = -exclude:NotDotNet,CAS,PKITS
endif
endif
