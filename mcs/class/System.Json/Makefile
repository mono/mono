thisdir = class/System.Json
SUBDIRS = 
include ../../build/rules.make

UPSTREAM_DIR = ../../../external/aspnetwebstack/src/System.Json/Properties
RESX_RESOURCES = $(UPSTREAM_DIR)/Resources.resx
RESOURCES = $(RESX_DIST:.resx=.resources)

LIBRARY = System.Json.dll
LIB_MCS_FLAGS = \
	        /r:System.dll \
	        /r:System.Xml.dll \
	        /r:System.Core.dll \
		/r:System.Runtime.Serialization.dll \
		/r:System.ServiceModel.Web.dll \
		$(RESX_RES:%=/resource:%)

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += /r:Microsoft.CSharp.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = $(RESX_RESOURCES)

VALID_PROFILE := $(filter 2 4, $(FRAMEWORK_VERSION_MAJOR))

ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Json.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make

$(the_lib): $(RESOURCES)

$(RESOURCES):  %.resources: %.resx
	$(RESGEN) $< || cp $@.prebuilt $@
