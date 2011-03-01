thisdir = class/System.Data.Services
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.Services.dll

LIB_MCS_FLAGS = \
	-d:NET_3_5	\
	-r:System.dll \
	-r:System.Core.dll \
	-r:System.ServiceModel.dll \
	-r:System.ServiceModel.Web.dll \
	
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make
