thisdir = class/WindowsBase
include ../../build/rules.make

LIBRARY = WindowsBase.dll

LIB_MCS_FLAGS = -unsafe -r:System -r:System.Xml
TEST_MCS_FLAGS = -unsafe -r:WindowsBase.dll -r:System.Xaml.dll

ifeq (2.0, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_0
endif
ifeq (4.0, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_4_0 -r:System.Xaml.dll
endif

include ../../build/library.make
