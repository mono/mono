MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Runtime.Serialization.Xml
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Runtime.Serialization.Xml.dll

KEYFILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /nowarn:1616,1699
LIB_REFS = System System.Runtime.Serialization System.Xml Facades/System.Runtime.Serialization.Primitives
LIB_MCS_FLAGS = $(SIGN_FLAGS) 

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += /d:NO_CODEDOM
endif

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


