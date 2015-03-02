thisdir = class/System.Runtime.Remoting
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.Runtime.Remoting.dll

LIB_REFS = System System.Web System.Xml System.Runtime.Serialization.Formatters.Soap
LIB_MCS_FLAGS = /r:$(corlib)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:618 /r:System.Runtime.Remoting.dll

TEST_MONO_PATH = .

EXTRA_DISTFILES = Test/corba-example.cs Test/simple-example.cs

include ../../build/library.make

