thisdir = class/System.Runtime.Remoting
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.Runtime.Remoting.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Web.dll \
	/r:System.Runtime.Serialization.Formatters.Soap.dll
NO_TEST = yes
EXTRA_DISTFILES = Test/corba-example.cs Test/simple-example.cs

include ../../build/library.make
