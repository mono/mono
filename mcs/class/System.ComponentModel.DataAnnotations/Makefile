thisdir = class/System.ComponentModel.DataAnnotations
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.ComponentModel.DataAnnotations.dll
RESX_DIST = DataAnnotationsResources.resx
LIB_REFS = System System.Core System.Data System.Xml
LIB_MCS_FLAGS = -nowarn:414 \
		$(foreach r, $(RESOURCES), /resource:$(r),System.ComponentModel.DataAnnotations.Resources.$(notdir $(r)))

EXTRA_DISTFILES = $(RESX_DIST)
RESOURCES = $(RESX_DIST:.resx=.resources)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make

$(build_lib): $(RESOURCES)

$(RESOURCES): %.resources: %.resx
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
