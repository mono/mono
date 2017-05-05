MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/netstandard
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = netstandard.dll

KEY_FILE = ../../Open.snk
LIBRARY_SNK = $(KEY_FILE)
SIGN_FLAGS = /delaysign /keyfile:$(KEY_FILE) /nowarn:1616,1699 /nowarn:618
LIB_REFS = System System.Xml System.Xml.Linq System.Runtime.Serialization System.Core System.Numerics System.Net.Http System.Transactions \
System.IO.Compression System.Data System.ComponentModel.Composition System.IO.Compression.FileSystem \
Facades/System.Security.Cryptography.Algorithms

ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Web.Services
else
LIB_REFS += System.Web System.Drawing
endif

LIB_MCS_FLAGS = $(SIGN_FLAGS) $(EXTRA_LIB_MCS_FLAGS)

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


