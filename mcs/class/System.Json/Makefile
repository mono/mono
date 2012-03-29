thisdir = class/System.Json
SUBDIRS = 
include ../../build/rules.make

UPSTREAM_DIR = ../../../external/aspnetwebstack/src
RESX_RESOURCES = $(UPSTREAM_DIR)/System.Json/Properties/Resources.resx
RESOURCES = $(subst $(UPSTREAM_DIR),$(build_libdir),$(RESX_RESOURCES:.resx=.resources))

LIBRARY = System.Json.dll
LIB_MCS_FLAGS = \
		/d:ASPNETMVC \
	        /r:System.dll \
	        /r:System.Xml.dll \
	        /r:System.Core.dll \
		/r:System.Runtime.Serialization.dll \
		/r:System.ServiceModel.Web.dll \
		$(RESOURCES:%=/resource:%)

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

# Canned recipe which would be useful, but make doesn't run it for some reason...
#define run-resgen = 
#	mkdir -p $(dir $@)
#	$(RESGEN) $< $@
#endef

$(build_libdir)/System.Json/Properties/Resources.resources: $(UPSTREAM_DIR)/System.Json/Properties/Resources.resx
#	Doesn't work for some reason
#	$(run-resgen)
	mkdir -p $(dir $@)
	$(RESGEN) $< $@
