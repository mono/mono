MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Drawing.Common
SUBDIRS =
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Drawing.Common.dll

KEYFILE = ../../Open.snk
LIBRARY_SNK = $(KEYFILE)
SIGN_FLAGS = /delaysign /nowarn:1616,1699
LIB_REFS = System
LIB_MCS_FLAGS = $(SIGN_FLAGS) $(EXTRA_LIB_MCS_FLAGS) -d:FEATURE_TYPECONVERTER

ifneq (,$(filter build net_4_x, $(PROFILE)))
$(error This profile shouldn't build System.Drawing.Common.dll, it already has System.Drawing.dll)
endif

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make
