thisdir = class/Microsoft.Web.Atlas
include ../../build/rules.make

#RESOURCE_FILES = \
#	...

LIBRARY = Microsoft.Web.Atlas.dll
LIB_MCS_FLAGS = \
	-unsafe \
	-nowarn:649  	\
	-r:$(corlib)	 		\
	-r:System.dll 			\
	-r:System.Configuration.dll	\
	-r:System.Drawing.dll 		\
	-r:System.Data.dll		\
	-r:System.Web.dll		\
	-r:System.Web.Services.dll	\
	-r:System.Xml.dll		\
	-r:System.EnterpriseServices.dll \
	-r:System.Design.dll 		\
	$(RESOURCE_FILES:%=/resource:%)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:219 -nowarn:169

EXTRA_DISTFILES = $(RESOURCE_FILES)

include ../../build/library.make
