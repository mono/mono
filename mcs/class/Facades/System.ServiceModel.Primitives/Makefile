MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.ServiceModel.Primitives
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.ServiceModel.Primitives.dll

KEY_FILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /keyfile:$(KEY_FILE) /nowarn:1616,1699
LIB_REFS = System System.ServiceModel System.Xml Facades/System.Security.Cryptography.X509Certificates

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.IdentityModel
endif

LIB_MCS_FLAGS = $(SIGN_FLAGS) 

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


