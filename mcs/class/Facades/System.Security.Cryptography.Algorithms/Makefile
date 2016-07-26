MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Security.Cryptography.Algorithms
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Security.Cryptography.Algorithms.dll

KEY_FILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /keyfile:$(KEY_FILE) /nowarn:1616,1699
LIB_REFS = System System.Core
LIB_MCS_FLAGS = $(SIGN_FLAGS)

ifneq (,$(findstring NETSTANDARD,$(PROFILE_MCS_FLAGS)))
LIB_MCS_FLAGS += /d:IN_FACADE
endif

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


