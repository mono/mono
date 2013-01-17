thisdir = class/System.Design
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Design.dll

LIBRARY_USE_INTERMEDIATE_FILE = yes

DRAWING_DESIGN_DEP := System.Drawing.Design.dll
DRAWING_DESIGN_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(DRAWING_DESIGN_DEP))
CYCLIC_DEPS := $(DRAWING_DESIGN_DEP)
CYCLIC_DEP_FILES := $(DRAWING_DESIGN_DEP_FILE)


LIB_MCS_FLAGS = -r:$(corlib) -r:System.dll -r:System.Xml.dll -r:System.Web.dll \
	-r:System.Windows.Forms.dll -r:System.Drawing.dll \
	-r:Accessibility.dll -r:System.Data.dll \
	-r:System.Configuration.dll
	
TEST_MCS_FLAGS = /r:System.dll -r:System.Drawing.dll -r:System.Windows.Forms

ifneq ($(CYCLIC_DEPS:%=../lib/$(PROFILE)/%), $(CYCLIC_DEP_FILES))
NO_SIGN_ASSEMBLY = yes
NO_INSTALL = yes
all-local: echo-warning
.PHONY: echo-warning

ifeq   (, $(strip $(CYCLIC_DEP_FILES)))
echo-warning:
	@echo "** Warning: System.Web.dll built without parts that depend on: $(CYCLIC_DEPS)"
endif
endif


include ../../build/library.make


ifdef DRAWING_DESIGN_DEP_FILE
LIB_MCS_FLAGS += -define:DRAWING_DESIGN_DEP -r:$(DRAWING_DESIGN_DEP)
$(build_lib): $(DRAWING_DESIGN_DEP_FILE)
endif
