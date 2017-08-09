MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Text.Encoding.CodePages
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Text.Encoding.CodePages.dll

KEYFILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /nowarn:1616,1699
LIB_REFS = System
LIB_MCS_FLAGS = $(SIGN_FLAGS)

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


