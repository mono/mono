thisdir = class/System.Threading.Tasks.Dataflow
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Threading.Tasks.Dataflow.dll

LIB_REFS += System.Core System
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS += -d:CONCURRENT_COLLECTIONS
XTEST_LIB_REFS = System System.Core System.Xml Facades/System.Threading.Tasks
USE_XTEST_REMOTE_EXECUTOR = YES
RESX_RESOURCE_STRING = \
	../../../external/corefx/src/System.Threading.Tasks.Dataflow/src/Resources/Strings.resx

TEST_MCS_FLAGS =
TEST_LIB_REFS = System.Core System

EXTRA_DISTFILES=README.md

include ../../build/library.make
