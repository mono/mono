thisdir = class/System.Xml.Linq
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Xml.Linq.dll
LIB_REFS = System System.Core System.Xml System.Runtime.Serialization
LIB_MCS_FLAGS =

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:MONO_HYBRID_SYSTEM_XML
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

include ../../build/library.make
