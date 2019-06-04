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

ifneq (,$(filter build net_4_x, $(PROFILE)))
# drawing types are inside System.Drawing.dll
LIB_REFS += System.Drawing
else
# drawing types are inside System.Drawing.Common.dll
LIB_REFS += Facades/System.Drawing.Common
endif

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make
