thisdir = class/IBM.Data.DB2
SUBDIRS =
include ../../build/rules.make

LIBRARY = IBM.Data.DB2.dll
LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll \
	/r:System.Xml.dll /r:System.Data.dll
NO_TEST = yes

ifeq (net_2_0, $(PROFILE))
all-local install-local test-local run-test-local clean-local dist-local uninstall-local:
else
include ../../build/library.make
endif

