thisdir = class/System.Numerics
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Numerics.dll
LIB_REFS = System
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS = /unsafe
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)
XTEST_LIB_REFS = System Facades/System.Threading.Tasks Facades/System.Runtime.InteropServices.RuntimeInformation System.Core System.Numerics.Vectors Microsoft.CSharp
XTEST_LIB_FLAGS = -unsafe

RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Runtime.Numerics/src/Resources/Strings.resx \
	../../../external/corefx/src/System.Numerics.Vectors/src/Resources/Strings.resx

EXTRA_DISTFILES =

include ../../build/library.make
