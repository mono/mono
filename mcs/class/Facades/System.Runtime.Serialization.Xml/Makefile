MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Runtime.Serialization.Xml
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Runtime.Serialization.Xml.dll

KEY_FILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /keyfile:$(KEY_FILE) /nowarn:1616,1699
LIB_REFS = System.Runtime.Serialization System System.Xml Facades/System.Runtime.Serialization.Primitives
LIB_MCS_FLAGS = $(SIGN_FLAGS) /r:mscorlib

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += /d:NO_CODEDOM
endif

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


