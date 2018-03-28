thisdir = class/System.Data.Services.Client
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.Services.Client.dll

RESOURCE_DEFS = System.Data.Services.Client,Client/System.Data.Services.Client.txt

LIB_REFS = System System.Core System.Xml.Linq System.Data System.Xml
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS = \
	-d:NET_3_5	\
	-warn:2

ifndef NO_WINDOWS_BASE
ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_REFS += WindowsBase
endif
endif

include ../../build/library.make

EXTRA_DISTFILES = Client/System.Data.Services.Client.txt

