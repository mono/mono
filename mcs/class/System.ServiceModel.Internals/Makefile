thisdir = class/System.ServiceModel.Internals
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceModel.Internals.dll

LIB_MCS_FLAGS = \
		/unsafe \
	        /r:System.dll \
	        /r:System.Xml.dll 

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += /d:NET_3_0 	\
			/r:System.Configuration.dll \
			/r:System.Data.dll	\
			/r:System.Security.dll \
			/r:System.IdentityModel.dll \
			/r:System.IdentityModel.Selectors.dll \
			/r:System.Transactions.dll \
			/r:System.Messaging.dll \
			/r:System.Web.dll \
			/r:System.Web.Services.dll \
			/r:Mono.Security.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make

