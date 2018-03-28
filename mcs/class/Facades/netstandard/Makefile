MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/netstandard
SUBDIRS = 
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = netstandard.dll

KEYFILE = ../../Open.snk
LIBRARY_SNK = $(KEYFILE)
SIGN_FLAGS = /delaysign /nowarn:1616,1699 /nowarn:618
LIB_REFS = System System.Xml System.Xml.Linq System.Runtime.Serialization System.Core System.Numerics System.Numerics.Vectors System.Net.Http System.Transactions \
System.IO.Compression System.Data System.ComponentModel.Composition System.IO.Compression.FileSystem Facades/System.Runtime.InteropServices.RuntimeInformation \
Facades/System.Security.Cryptography.Algorithms Facades/System.Globalization.Extensions Facades/System.Data.Common \
Facades/System.Diagnostics.StackTrace Facades/System.Runtime.Serialization.Xml Facades/System.Runtime.Serialization.Primitives \
Facades/System.Security.SecureString Facades/System.Threading.Overlapped Facades/System.Xml.XPath.XDocument

LIB_MCS_FLAGS = $(SIGN_FLAGS) $(EXTRA_LIB_MCS_FLAGS)

ifeq ($(PROFILE),xammac_net_4_5)
LIB_REFS += System.Web.Services
else ifeq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += System.Web.Services
else
LIB_REFS += System.Web
endif

# profiles which build a System.Drawing.dll in the repo
REPO_DRAWING_DEP := $(filter net_4_x orbis winaot unreal wasm, $(PROFILE))

ifdef REPO_DRAWING_DEP
LIB_REFS += System.Drawing
else

# xammac_net_4_5 and the testing_* profiles are special, they don't build
# System.Drawing.dll or equivalent neither in Mono nor in the XM repo.
# For now we typeforward the drawing types to the System.Drawing.Primitives.dll
# facade where they are embedded.
# TODO: find a better solution.
EMBEDDED_DRAWING_DEP := $(filter xammac_net_4_5 testing_aot_full testing_aot_hybrid, $(PROFILE))

ifdef EMBEDDED_DRAWING_DEP
LIB_REFS += Facades/System.Drawing.Primitives
else
# When System.Drawing.dll is not built in the Mono repo but in
# the product repo like in the XI/XA case we need to pass in a reference
# to the assembly containing drawing types like Rectangle etc. from there.
# This needs to be passed in via EXTERNAL_FACADE_DRAWING_REFERENCE.
LIB_MCS_FLAGS += /r:$(EXTERNAL_FACADE_DRAWING_REFERENCE)
endif

endif

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make


