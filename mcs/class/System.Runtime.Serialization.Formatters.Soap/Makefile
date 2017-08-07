thisdir = class/System.Runtime.Serialization.Formatters.Soap
SUBDIRS =
include ../../build/rules.make

# bet you can't say this ten times fast

LIBRARY = System.Runtime.Serialization.Formatters.Soap.dll
LIB_REFS = System.Xml
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS =

TEST_LIB_REFS = System
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169

EXTRA_DISTFILES = \
	README		\
	Sample.txt	\
	Test/Test.cmbx	\
	Test/Test.prjx

include ../../build/library.make
