thisdir = class/System.Runtime.Remoting
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.Runtime.Remoting.dll

LIB_REFS = System System.Xml System.Runtime.Serialization.Formatters.Soap
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS =

ifndef NO_SYSTEM_WEB_DEPENDENCY
LIB_REFS += System.Web
endif

TEST_MCS_FLAGS = -nowarn:618
TEST_LIB_REFS = System System.Xml System.Core

EXTRA_DISTFILES = Test/corba-example.cs Test/simple-example.cs

include ../../build/library.make

