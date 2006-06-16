thisdir = class/System.Web.Services
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Services.dll
LIB_MCS_FLAGS = \
	-nowarn:649 -nowarn:169			\
	-r:$(corlib)				\
	-r:System.dll				\
	-r:System.EnterpriseServices.dll	\
	-r:System.Xml.dll			\
	-r:System.Web.dll

ifeq (net_2_0, $(PROFILE))
LIB_MCS_FLAGS += -r:System.Configuration.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:618

EXTRA_DISTFILES = Test/System.Web.Services.Description/test.wsdl

include ../../build/library.make
