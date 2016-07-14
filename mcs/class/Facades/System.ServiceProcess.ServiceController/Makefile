MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.ServiceProcess.ServiceController
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.ServiceProcess.ServiceController.dll

KEY_FILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /keyfile:$(KEY_FILE) /nowarn:1616,1699
LIB_REFS = System

ifneq (2.1, $(FRAMEWORK_VERSION))
ifndef XAMMAC_4_5
LIB_REFS += System.ServiceProcess
endif
endif

LIB_MCS_FLAGS = $(SIGN_FLAGS) /r:mscorlib

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


