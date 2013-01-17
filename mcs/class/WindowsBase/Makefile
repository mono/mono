thisdir = class/WindowsBase
include ../../build/rules.make

LIBRARY = WindowsBase.dll

LIB_MCS_FLAGS = -unsafe -r:System.dll -r:System.Xml.dll
TEST_MCS_FLAGS = -unsafe -r:WindowsBase.dll -r:System.dll -r:System.Xml.dll -r:System.Core.dll

ifeq (2.0, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_0
endif
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += -r:System.Xaml.dll
TEST_MCS_FLAGS += -r:System.Xaml.dll
endif

include ../../build/library.make
