thisdir = class/System.Web
SUBDIRS = Test
include ../../build/rules.make

LIBRARY = System.Web.dll
LIB_MCS_FLAGS = \
	/nowarn:649  	\
	/r:$(corlib)	 		\
	/r:System.dll 			\
	/r:System.Drawing.dll 		\
	/r:System.Data.dll		\
	/r:System.Xml.dll		\
	/r:System.EnterpriseServices.dll
HAS_TEST = yes

include ../../build/library.make
