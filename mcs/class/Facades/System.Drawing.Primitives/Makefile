MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Drawing.Primitives
SUBDIRS =
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Drawing.Primitives.dll

KEY_FILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /keyfile:$(KEY_FILE) /nowarn:1616,1699
LIB_REFS = System
LIB_MCS_FLAGS = $(SIGN_FLAGS)

ifneq (2.1, $(FRAMEWORK_VERSION))
ifndef XAMMAC_4_5
LIB_REFS += System.Drawing
endif
endif

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make
