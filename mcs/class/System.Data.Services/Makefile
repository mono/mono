thisdir = class/System.Data.Services
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.Services.dll

LIBRARY_SNK = ../mono.snk
LIBRARY_PACKAGE = none

LIB_MCS_FLAGS = \
	-r:System.dll \
	-r:System.Core.dll \
	-r:System.ServiceModel.dll \
	-r:System.ServiceModel.Web.dll \
	-keyfile:$(LIBRARY_SNK)

include ../../build/library.make
