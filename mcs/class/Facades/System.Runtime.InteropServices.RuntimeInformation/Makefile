MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Runtime.InteropServices.RuntimeInformation
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Runtime.InteropServices.RuntimeInformation.dll

RESX_RESOURCE_STRING = ../../../../external/corefx/src/System.Runtime.InteropServices.RuntimeInformation/src/Resources/Strings.resx

KEYFILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /nowarn:1616,1699,436
LIB_REFS = System
LIB_MCS_FLAGS = $(SIGN_FLAGS) /unsafe

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make



