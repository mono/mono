thisdir = class/System.ServiceModel.Internals
SUBDIRS = 
include ../../build/rules.make

ifndef NO_MULTIPLE_APPDOMAINS
REFERENCE_SOURCES_FLAGS += -d:MONO_FEATURE_MULTIPLE_APPDOMAINS
endif

LIBRARY = System.ServiceModel.Internals.dll
LIB_REFS = System System.Core System.Xml
LIB_MCS_FLAGS = /unsafe $(REFERENCE_SOURCES_FLAGS)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = 

NO_TEST = yes

include ../../build/library.make
