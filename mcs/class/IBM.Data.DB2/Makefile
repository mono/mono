thisdir = class/IBM.Data.DB2
SUBDIRS =
include ../../build/rules.make

LIBRARY = IBM.Data.DB2.dll
LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll \
	/r:System.Xml.dll /r:System.Data.dll
NO_TEST = yes

EXTRA_DISTFILES = ibm.pub

ifeq (net_2_0, $(PROFILE))
all-local $(STD_TARGETS:=-local):
	@:
else
include ../../build/library.make
endif

