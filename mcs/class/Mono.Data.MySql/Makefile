thisdir = class/Mono.Data.MySql
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.Data.MySql.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll /r:System.Data.dll
NO_TEST = yes

EXTRA_DISTFILES = \
	Test/MySqlTest.cs		\
	Test/TestDataAdapter.cs		\
	Test/TestMySqlExecuteReader.cs	\
	Test/TestMySqlExecuteScalar.cs	\
	Test/TestMySqlInsert.cs		\
	Test/TestParameters.cs

include ../../build/library.make
