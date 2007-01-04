thisdir = class/System.Data.OracleClient
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.OracleClient.dll
ifeq (net_1_1_java, $(PROFILE))
LIB_MCS_FLAGS = \
    -r:rt.dll                \
    -r:J2SE.Helpers.dll            \
    /r:System.dll \
    /r:System.Xml.dll /r:../../class/lib/net_1_1_java/System.Data.dll \
    -nowarn:649 -nowarn:169 -nowarn:219 -nowarn:168 -nowarn:1595 \
    -r:$(corlib)                \
    -r:System.Xml.dll             \
    /r:System.Drawing.dll

else
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll \
	/r:System.Xml.dll /r:System.Data.dll \
	/r:System.EnterpriseServices.dll \
	/r:System.Drawing.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /nowarn:618

EXTRA_DISTFILES = Test/*.cs

include ../../build/library.make
