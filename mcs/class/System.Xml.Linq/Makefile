thisdir = class/System.Xml.Linq
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Xml.Linq.dll
LIB_REFS = System System.Core System.Xml
LIB_MCS_FLAGS = -unsafe -nowarn:436

NO_SERIALIZATION_PROFILE := $(filter build, $(PROFILE))
ifndef NO_SERIALIZATION_PROFILE
LIB_REFS += System.Runtime.Serialization
LIB_MCS_FLAGS += -d:FEATURE_SERIALIZATION
endif

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:MONO_HYBRID_SYSTEM_XML
endif

TEST_LIB_REFS = System.Core System.Xml System
TEST_MCS_FLAGS =

XTEST_LIB_REFS = System.Core System.Xml System Facades/System.Reflection Facades/System.Threading.Tasks
XTEST_LIB_FLAGS = -unsafe

EXTRA_DISTFILES =

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Private.Xml.Linq/src/Resources/Strings.resx

include ../../build/library.make
