thisdir = class/System.Data.Services.Client
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.Services.Client.dll

LIB_MCS_FLAGS = \
	-d:NET_3_5	\
	-r:System.dll \
	-r:System.Core.dll \
	-r:System.Xml.Linq.dll \
	-r:System.Data.dll \
	-r:System.Xml.dll \
	-warn:2

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += -r:WindowsBase
endif

include ../../build/library.make
