thisdir = class/System.Configuration.Install
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.Configuration.Install.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = Test/ChangeLog

include ../../build/library.make
