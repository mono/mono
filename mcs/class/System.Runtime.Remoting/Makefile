thisdir = class/System.Runtime.Remoting
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.Runtime.Remoting.dll

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Web.dll \
	/r:System.Runtime.Serialization.Formatters.Soap.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /r:System.Runtime.Remoting.dll

EXTRA_DISTFILES = Test/corba-example.cs Test/simple-example.cs

include ../../build/library.make
