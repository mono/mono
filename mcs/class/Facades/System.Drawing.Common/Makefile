MCS_BUILD_DIR = ../../../build

thisdir = class/Facades/System.Drawing.Common
SUBDIRS =
include $(MCS_BUILD_DIR)/rules.make

LIBRARY_SUBDIR = Facades
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)/Facades

LIBRARY = System.Drawing.Common.dll

KEYFILE = ../../Open.snk
LIBRARY_SNK = $(KEYFILE)
SIGN_FLAGS = /delaysign /nowarn:1616,1699
LIB_REFS = System System.Core
LIB_MCS_FLAGS = $(SIGN_FLAGS) $(EXTRA_LIB_MCS_FLAGS)

SYSDRAWING_COREGRAPHICS_RESOURCE_FILES = \
	../../../../external/sysdrawing-coregraphics/Resources/Error.ico \
	../../../../external/sysdrawing-coregraphics/Resources/Information.ico \
	../../../../external/sysdrawing-coregraphics/Resources/Mono.ico \
	../../../../external/sysdrawing-coregraphics/Resources/Question.ico\
	../../../../external/sysdrawing-coregraphics/Resources/Shield.ico \
	../../../../external/sysdrawing-coregraphics/Resources/Warning.ico \
	../../../../external/sysdrawing-coregraphics/Resources/WinLogo.ico

ifeq ($(PROFILE),monotouch)
API_BIN_REFS := Xamarin.iOS
RESOURCE_FILES = $(SYSDRAWING_COREGRAPHICS_RESOURCE_FILES)
EXTRA_DISTFILES = \
	$(RESOURCE_FILES)
LIB_MCS_FLAGS += /unsafe -d:__IOS__ -d:__MOBILE__ -d:__UNIFIED__ -d:MONOTOUCH
endif

ifeq ($(PROFILE),xammac)
API_BIN_REFS := Xamarin.Mac
RESOURCE_FILES = $(SYSDRAWING_COREGRAPHICS_RESOURCE_FILES)
EXTRA_DISTFILES = \
	$(RESOURCE_FILES)
LIB_MCS_FLAGS += /unsafe -d:__UNIFIED__ -d:__MACOS__ -d:MONOMAC
endif

# profiles which build a System.Drawing.dll in the repo
REPO_DRAWING_DEP := $(filter net_4_x orbis winaot unreal testing_aot_full wasm, $(PROFILE))

ifdef REPO_DRAWING_DEP
LIB_REFS += System.Drawing
LIB_MCS_FLAGS += -d:MONO_HAS_SYSTEM_DRAWING
endif

PLATFORM_DEBUG_FLAGS =

NO_TEST = yes

include $(MCS_BUILD_DIR)/library.make
