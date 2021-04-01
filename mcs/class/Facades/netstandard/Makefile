MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/netstandard
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = netstandard.dll

KEYFILE = ../../Open.snk
LIBRARY_SNK = $(KEYFILE)
SIGN_FLAGS = /delaysign /nowarn:1616,1699,618 
LIB_REFS = System System.Xml System.Xml.Linq System.Core System.Numerics System.Net.Http \
System.IO.Compression System.ComponentModel.Composition System.IO.Compression.FileSystem

LIB_MCS_FLAGS = $(SIGN_FLAGS)

ifeq ($(PROFILE),build)
# for the build profile we include stubs for these types directly in netstandard.dll
LIB_MCS_FLAGS += -d:SYSTEM_WEB_IMPLEMENTATION
else ifeq ($(PROFILE),wasm)
LIB_MCS_FLAGS += -d:SYSTEM_WEB_IMPLEMENTATION -unsafe
LIB_REFS += System.Transactions System.Runtime.Serialization System.Data System.Data.DataSetExtensions
else ifeq ($(PROFILE),xammac_net_4_5)
LIB_REFS += System.Web.Services System.Transactions System.Runtime.Serialization System.Data System.Data.DataSetExtensions
else ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Web.Services System.Transactions System.Runtime.Serialization System.Data System.Data.DataSetExtensions
else
LIB_REFS += System.Web System.Transactions System.Runtime.Serialization System.Data System.Data.DataSetExtensions
endif

ifneq (,$(filter build net_4_x, $(PROFILE)))
# drawing types are inside System.Drawing.dll
LIB_REFS += System.Drawing
else
# drawing types are inside System.Drawing.Common.dll
LIB_REFS += Facades/System.Drawing.Common
endif

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


