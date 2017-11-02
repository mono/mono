MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Drawing.Primitives
SUBDIRS =
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Drawing.Primitives.dll

KEYFILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /nowarn:1616,1699
LIB_REFS = System
LIB_MCS_FLAGS = $(SIGN_FLAGS) $(EXTRA_LIB_MCS_FLAGS)

# xammac_net_4_5 and the testing_* profiles are special, they don't build
# System.Drawing.dll or equivalent neither in Mono nor in the XM repo.
# For now we embed the types directly here.
# TODO: find a better solution.
EMBEDDED_DRAWING_DEP := $(filter xammac_net_4_5 testing_aot_full testing_aot_hybrid, $(PROFILE))

ifndef EMBEDDED_DRAWING_DEP

# profiles which build a System.Drawing.dll in the repo
REPO_DRAWING_DEP := $(filter net_4_x orbis winaot unityaot unityjit unreal wasm, $(PROFILE))

ifdef REPO_DRAWING_DEP
LIB_REFS += System.Drawing
else
# When System.Drawing.dll is not built in the Mono repo but in
# the product repo like in the XI/XA case we need to pass in a reference
# to the assembly containing drawing types like Rectangle etc. from there.
# This needs to be passed in via EXTERNAL_FACADE_DRAWING_REFERENCE.
LIB_MCS_FLAGS += /r:$(EXTERNAL_FACADE_DRAWING_REFERENCE)
endif

endif

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make
