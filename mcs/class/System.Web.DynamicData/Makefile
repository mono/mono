thisdir = class/System.Web.DynamicData
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.DynamicData.dll
LIB_MCS_FLAGS = \
	        /r:System.dll \
	        /r:System.Core.dll \
	        /r:System.ComponentModel.DataAnnotations.dll \
		/r:System.Data.dll \
		/r:System.Data.Linq.dll \
	        /r:System.Drawing.dll \
	        /r:System.Web.dll \
	        /r:System.Web.Extensions.dll \
		/r:System.Web.Abstractions.dll \
		/r:System.Web.Routing.dll

ifeq (2.0, $(FRAMEWORK_VERSION))
# This is a .NET 3.5 only assembly, but built during the 2.0 build
LIB_MCS_FLAGS += -d:NET_3_5
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -r:System.ComponentModel.DataAnnotations.dll -r:System.Configuration.dll

EXTRA_DISTFILES =

ifneq (net_2_0, $(PROFILE))
LIBRARY_NAME = dummy-System.Web.DynamicData.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
