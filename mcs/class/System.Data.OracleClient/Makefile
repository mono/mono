thisdir = class/System.Data.OracleClient
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.OracleClient.dll
LIB_REFS = System System.Xml System.Data System.EnterpriseServices System.Drawing
LIB_MCS_FLAGS = /r:$(corlib)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /nowarn:618

EXTRA_DISTFILES = Test/*.cs

include ../../build/library.make
