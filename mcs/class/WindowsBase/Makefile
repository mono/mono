thisdir = class/WindowsBase
include ../../build/rules.make

LIBRARY = WindowsBase.dll

LIB_REFS = System System.Xml
LIB_MCS_FLAGS = -unsafe
TEST_MCS_FLAGS = -unsafe
TEST_LIB_REFS = WindowsBase System System.Xml System.Core System.IO.Compression

ifeq (2.0, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -d:NET_3_0
endif
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_REFS += System.Xaml
TEST_LIB_REFS += System.Xaml
endif

include ../../build/library.make
