thisdir = class/System.Runtime.Remoting
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.Runtime.Remoting.dll

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Web.dll \
	/r:System.Xml.dll /r:System.Runtime.Serialization.Formatters.Soap.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:618 /r:System.Runtime.Remoting.dll

TEST_MONO_PATH = .

EXTRA_DISTFILES = Test/corba-example.cs Test/simple-example.cs

include ../../build/library.make

update-http: get_mono_http
	./get_mono_http
