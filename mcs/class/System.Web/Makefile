thisdir = class/System.Web
SUBDIRS = Test
include ../../build/rules.make

RESOURCE_FILES = \
	resources/arrow_minus.gif \
	resources/arrow_noexpand.gif \
	resources/arrow_plus.gif \
	resources/box_full.gif \
	resources/box_empty.gif \
	resources/box_minus.gif \
	resources/box_noexpand.gif \
	resources/box_plus.gif \
	resources/contact.gif \
	resources/dot_empty.gif \
	resources/dot_full.gif \
	resources/dots.gif \
	resources/inbox.gif \
	resources/star_empty.gif \
	resources/star_full.gif \
	resources/warning.gif \
	resources/TreeView_noexpand.gif \
	resources/TreeView_dash.gif \
	resources/TreeView_dashminus.gif \
	resources/TreeView_dashplus.gif \
	resources/TreeView_i.gif \
	resources/TreeView_l.gif \
	resources/TreeView_lminus.gif \
	resources/TreeView_lplus.gif \
	resources/TreeView_minus.gif \
	resources/TreeView_plus.gif \
	resources/TreeView_r.gif \
	resources/TreeView_rminus.gif \
	resources/TreeView_rplus.gif \
	resources/TreeView_t.gif \
	resources/TreeView_tminus.gif \
	resources/TreeView_tplus.gif \
	resources/callback.js \
	System.Web.UI.WebControls/TreeView.js \
	System.Web.UI.WebControls/Menu.js

ifeq (net_2_0, $(PROFILE))
OTHER_RES = $(RESOURCE_FILES)
endif

LIBRARY = System.Web.dll
LIB_MCS_FLAGS = \
	/nowarn:649  	\
	/r:$(corlib)	 		\
	/r:System.dll 			\
	/r:System.Drawing.dll 		\
	/r:System.Data.dll		\
	/r:System.Xml.dll		\
	/r:System.EnterpriseServices.dll \
	$(OTHER_RES:%=/resource:%)

EXTRA_DISTFILES = $(RESOURCE_FILES)

include ../../build/library.make
