thisdir = class/System.Data.Services.Client
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.Services.Client.dll

LIBRARY_SNK = ../mono.snk

LIB_MCS_FLAGS = \
	-d:NET_3_5	\
	-r:System.dll \
	-r:System.Core.dll \
	-r:System.Xml.Linq.dll \
	-r:System.Data.dll \
	-r:System.Xml.dll \
	-r:WindowsBase.dll \
	-warn:2

include ../../build/library.make
