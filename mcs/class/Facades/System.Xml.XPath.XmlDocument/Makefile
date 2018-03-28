MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Xml.XPath.XmlDocument
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Xml.XPath.XmlDocument.dll

KEYFILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /nowarn:1616,1699
LIB_REFS = System System.Xml
LIB_MCS_FLAGS = $(SIGN_FLAGS)

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make
