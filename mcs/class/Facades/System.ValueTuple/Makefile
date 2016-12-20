MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.ValueTuple
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.ValueTuple.dll

KEY_FILE = ../../Open.snk
LIBRARY_SNK = $(KEY_FILE)
SIGN_FLAGS = /delaysign /keyfile:$(KEY_FILE) /nowarn:1616,1699
LIB_REFS = 
LIB_MCS_FLAGS = $(SIGN_FLAGS)

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


