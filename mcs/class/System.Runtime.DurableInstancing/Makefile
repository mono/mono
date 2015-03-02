thisdir = class/System.Runtime.DurableInstancing
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Runtime.DurableInstancing.dll
LIB_REFS = System System.Xml System.Runtime.Serialization System.Core System.Xml.Linq
LIB_MCS_FLAGS =

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = $(RESOURCE_FILES)

VALID_PROFILE := $(filter 2.0 4.0 4.5, $(FRAMEWORK_VERSION))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Runtime.DurableInstancing.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
endif

include ../../build/library.make
