thisdir = class/System.Runtime.Serialization.Formatters.Soap
SUBDIRS =
include ../../build/rules.make

# bet you can't say this ten times fast

LIBRARY = System.Runtime.Serialization.Formatters.Soap.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.Xml.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = \
	README		\
	Sample.txt	\
	Test/Test.cmbx	\
	Test/Test.prjx

include ../../build/library.make
