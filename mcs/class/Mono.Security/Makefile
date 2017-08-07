thisdir = class/Mono.Security
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Security.dll
LOCAL_MCS_FLAGS =
LIB_REFS = bare/System
KEYFILE = ../mono.pub
LIB_MCS_FLAGS = -unsafe -nowarn:1030,3009
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:169,219,618,672

CC_PROFILE := $(filter monotouch% xammac, $(PROFILE))
ifdef CC_PROFILE

BUILT_SOURCES = \
	../corlib/CommonCrypto/SHA224Managed.g.cs \
	../corlib/CommonCrypto/MD2Managed.g.cs \
	../corlib/CommonCrypto/MD4Managed.g.cs

../corlib/CommonCrypto/%.g.cs:
	$(MAKE) -C ../corlib/CommonCrypto

endif

include ../../build/library.make

$(build_lib): $(the_libdir_base)bare/System.dll

$(the_libdir_base)bare/System.dll:
	(cd ../System; $(MAKE) $@)

.NOTPARALLEL: $(the_libdir_base)bare/System.dll

EXTRA_DISTFILES = Mono.Security.Interface/README.md

#
# Update this comment to trigger a build in System
# +3
#
