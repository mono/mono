thisdir = class/System.Web
SUBDIRS = Test
include ../../build/rules.make

RESOURCE_FILES = \
	System.Web.UI.WebControls/TreeView_Default_Collapse.gif \
	System.Web.UI.WebControls/TreeView_Default_Expand.gif \
	System.Web.UI.WebControls/TreeView_Default_NoExpand.gif

LIBRARY = System.Web.dll
LIB_MCS_FLAGS = \
	/nowarn:649  	\
	/r:$(corlib)	 		\
	/r:System.dll 			\
	/r:System.Drawing.dll 		\
	/r:System.Data.dll		\
	/r:System.Xml.dll		\
	/r:System.EnterpriseServices.dll \
	$(RESOURCE_FILES:%=/resource:%)

EXTRA_DISTFILES = $(RESOURCE_FILES)

include ../../build/library.make
