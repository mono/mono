MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Data.SqlClient
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Data.SqlClient.dll

KEY_FILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /keyfile:$(KEY_FILE) /nowarn:1616,1699
LIB_REFS = System
LIB_MCS_FLAGS = $(SIGN_FLAGS) /r:mscorlib /r:System.Data.dll /r:System.Xml.dll

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


