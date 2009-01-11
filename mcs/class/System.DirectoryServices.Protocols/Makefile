thisdir = class/System.DirectoryServices.Protocols
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.DirectoryServices.Protocols.dll
LIB_MCS_FLAGS = \
                /r:System.dll \
                /r:System.DirectoryServices.dll \
                /r:System.Xml.dll \

NO_TEST = yes

ifeq (2.0, $(FRAMEWORK_VERSION))
# This is a .NET 3.5 only assembly, but built during the 2.0 build
LIB_MCS_FLAGS += -d:NET_3_5
endif

include ../../build/library.make
