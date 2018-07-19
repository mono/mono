thisdir = class/WindowsBase
include ../../build/rules.make

LIBRARY = WindowsBase.dll

LIB_REFS = System System.Xml
KEYFILE = ../winfx3.pub
LIB_MCS_FLAGS = -unsafe -nowarn:67,618
TEST_MCS_FLAGS = -unsafe
TEST_LIB_REFS = WindowsBase System System.Xml System.Core System.IO.Compression

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIB_REFS += System.Xaml
TEST_LIB_REFS += System.Xaml
endif

include ../../build/library.make
