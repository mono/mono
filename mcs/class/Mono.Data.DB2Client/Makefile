thisdir = class/Mono.Data.DB2Client
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.Data.DB2Client.dll
LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll \
	/r:System.Xml.dll /r:System.Data.dll
NO_TEST = yes

EXTRA_DISTFILES = \
	Test/TestDB2Conn/App.ico		\
	Test/TestDB2Conn/AssemblyInfo.cs	\
	Test/TestDB2Conn/ChangeLog		\
	Test/TestDB2Conn/TestDB2Conn.cs

include ../../build/library.make

