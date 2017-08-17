thisdir = class/System.Runtime.DurableInstancing
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Runtime.DurableInstancing.dll
LIB_REFS = System System.Xml System.Core System.Xml.Linq System.Transactions System.ServiceModel.Internals System.Runtime.Serialization
LIB_MCS_FLAGS =

ifdef MOBILE_PROFILE
KEYFILE = ../silverlight.pub
else
KEYFILE = ../winfx.pub
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = $(RESOURCE_FILES)

ifneq (4.5, $(FRAMEWORK_VERSION))
LIBRARY_NAME = dummy-System.Runtime.DurableInstancing.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
endif

include ../../build/library.make
