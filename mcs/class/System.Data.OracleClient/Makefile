thisdir = class/System.Data.OracleClient
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.OracleClient.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll \
	/r:System.Xml.dll /r:System.Data.dll \
	/r:System.EnterpriseServices.dll \
	/r:System.Drawing.dll \
	/nowarn:649 /nowarn:169
NO_TEST = yes
EXTRA_DISTFILES = Test/TestOracleClient.cs

include ../../build/library.make
