thisdir = class/System.Web
SUBDIRS = Test
include ../../build/rules.make

LIBRARY = System.Web.dll
LIB_MCS_FLAGS = \
	/nowarn:0168 /nowarn:0162	\
	/r:$(corlib)	 		\
	/r:System.dll 			\
	/r:System.Drawing.dll 		\
	/r:System.Xml.dll
NO_TEST = yes

EXTRA_DISTFILES = \
	System.Web/NOTES		\
	System.Web/TODO			\
	System.Web.UI/TODO		\
	System.Web.UI.WebControls/TODO	\

include ../../build/library.make
