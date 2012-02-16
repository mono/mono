thisdir = class/IBM.Data.DB2
SUBDIRS =
include ../../build/rules.make

LIBRARY = IBM.Data.DB2.dll
LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll \
	/r:System.Xml.dll /r:System.Data.dll -warn:1
NO_TEST = yes

EXTRA_DISTFILES = ibm.pub

include ../../build/library.make

