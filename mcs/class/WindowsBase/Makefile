thisdir = class/WindowsBase
include ../../build/rules.make

LIBRARY = WindowsBase.dll

LIB_MCS_FLAGS = -unsafe -r:System -r:System.Xml
TEST_MCS_FLAGS = -unsafe -r:WindowsBase.dll

ifeq (2.0, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_0
endif

include ../../build/library.make
