thisdir = class/System.Web.Services
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Services.dll
LIB_MCS_FLAGS = \
	/nowarn:0168 /nowarn:0162		\
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.EnterpriseServices.dll	\
	/r:System.Xml.dll			\
	/r:System.Web.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make
