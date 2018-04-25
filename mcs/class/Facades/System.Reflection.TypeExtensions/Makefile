MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Reflection.TypeExtensions
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Reflection.TypeExtensions.dll

KEYFILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /nowarn:1616,1699
LIB_REFS = System
LIB_MCS_FLAGS = $(SIGN_FLAGS) 

RESX_RESOURCE_STRING = \
	../../../../external/corefx/src/System.Reflection.TypeExtensions/src/Resources/Strings.resx

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


