thisdir = class/System.Workflow.ComponentModel
include ../../build/rules.make
SUBDIRS =

LIBRARY_USE_INTERMEDIATE_FILE = yes

# Because System.Workflow.ComponentModel and System.Workflow.Runtime have a cyclic dependency, we need a two-pass build.
# 1st pass - build System.Workflow.ComponentModel without System.Workflow.Runtime reference
# 2nd pass - build System.Workflow.ComponentModel with System.Workflow.Runtime reference

LIBRARY = System.Workflow.ComponentModel.dll
RUNTIME_DEP := System.Workflow.Runtime.dll
RUNTIME_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(RUNTIME_DEP))
CYCLIC_DEPS := $(RUNTIME_DEP)
CYCLIC_DEP_FILES := $(RUNTIME_DEP_FILE)
WITHOUT_RUNTIME_DEP := $(depsdir)/$(PROFILE)_$(LIBRARY).without_runtime
WITHOUT_RUNTIME_FILE := $(wildcard $(WITHOUT_RUNTIME_DEP))

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll  /nowarn:114

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /r:System.Workflow.ComponentModel.dll  /r:System.Workflow.Activities.dll /r:System.Workflow.Runtime.dll

# This forces to recompile the library if the current version does not have the runtime dependency
ifneq   (, $(strip $(RUNTIME_DEP_FILE)))
ifneq   (, $(strip $(WITHOUT_RUNTIME_FILE)))

all-local: force-recompile

.PHONY: force-recompile
force-recompile:
	@rm -f $(depsdir)/$(PROFILE)_$(LIBRARY).*
	@rm -f $(WITHOUT_RUNTIME_DEP)
	@echo "** Forcing recompilation. The current library was built without parts that depend on: [$(RUNTIME_DEP_FILE)] and now can be built with those dependencies"
endif
endif

ifneq ($(CYCLIC_DEPS:%=../lib/$(PROFILE)/%), $(CYCLIC_DEP_FILES))
NO_SIGN_ASSEMBLY = yes
NO_INSTALL = yes
all-local: echo-warning

.PHONY: echo-warning

ifeq   (, $(strip $(CYCLIC_DEP_FILES)))
echo-warning:
	@echo "** Warning: System.Workflow.ComponentModel.dll built without parts that depend on: $(CYCLIC_DEPS)"
	@echo created >  $(WITHOUT_RUNTIME_DEP)
endif
endif

ifdef RUNTIME_DEP_FILE
LIB_MCS_FLAGS += -define:RUNTIME_DEP -r:$(RUNTIME_DEP)
endif

include ../../build/library.make

