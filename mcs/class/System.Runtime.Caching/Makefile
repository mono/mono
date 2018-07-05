thisdir = class/System.Runtime.Caching
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Runtime.Caching.dll
LIB_REFS = System System.Data System.Configuration System.Core
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = -nowarn:414 -unsafe

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Runtime.Caching/src/Resources/Strings.resx

XTEST_LIB_REFS = System System.Core System.Xml Facades/System.Threading.Tasks

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = $(RESOURCE_FILES)

VALID_PROFILE := $(filter 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Runtime.Caching.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
