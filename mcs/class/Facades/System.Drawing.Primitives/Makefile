MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Drawing.Primitives
SUBDIRS =
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Drawing.Primitives.dll

KEYFILE = ../../msfinal.pub
SIGN_FLAGS = /delaysign /nowarn:1616,1699
LIB_REFS = System
LIB_MCS_FLAGS = $(SIGN_FLAGS) $(EXTRA_LIB_MCS_FLAGS)

ifeq ($(PROFILE),monodroid)
# XA implements System.Drawing API inside Mono.Android
API_BIN_REFS := Mono.Android
LIB_REFS += Facades/System.Drawing.Common
endif

ifeq ($(PROFILE),monotouch)
API_BIN_REFS := Xamarin.iOS
endif

ifeq ($(PROFILE),monotouch_tv)
API_BIN_REFS := Xamarin.TVOS
endif

ifeq ($(PROFILE),monotouch_watch)
API_BIN_REFS := Xamarin.WatchOS
endif

ifeq ($(PROFILE),xammac)
API_BIN_REFS := Xamarin.Mac
endif

# profiles which build a System.Drawing.dll in the repo
REPO_DRAWING_DEP := $(filter net_4_x orbis testing_aot_full winaot unreal wasm, $(PROFILE))

ifdef REPO_DRAWING_DEP
LIB_REFS += System.Drawing
endif

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make
