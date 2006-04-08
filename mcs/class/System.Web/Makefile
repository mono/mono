thisdir = class/System.Web
SUBDIRS = Test
include ../../build/rules.make

LIBRARY = System.Web.dll

ifeq (net_2_0, $(PROFILE))

LIBRARY_USE_INTERMEDIATE_FILE = yes

# Because System.Web.dll and System.Web.Services.dll have a cyclic dependency, we need a two-pass build.
# 1st pass - build System.Web.dll without System.Web.Services.dll reference
# 2nd pass - build System.Web.dll with System.Web.Services.dll reference

WEBSERVICES_DEP := System.Web.Services.dll
WEBSERVICES_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(WEBSERVICES_DEP))
CYCLIC_DEPS := $(WEBSERVICES_DEP)
CYCLIC_DEP_FILES := $(WEBSERVICES_DEP_FILE)
endif

RESOURCE_FILES_1= \
	resources/WebUIValidation.js

RESOURCE_FILES_2 = \
	resources/arrow_minus.gif \
	resources/arrow_noexpand.gif \
	resources/arrow_plus.gif \
	resources/arrow_up.gif \
	resources/arrow_down.gif \
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
	resources/transparent.gif \
	resources/callback.js \
	resources/webform.js \
	System.Web.UI.WebControls/GridView.js \
	System.Web.UI.WebControls/TreeView.js \
	System.Web.UI.WebControls/Menu.js

OTHER_RES = $(RESOURCE_FILES_1)
ifeq (net_2_0, $(PROFILE))
OTHER_RES += $(RESOURCE_FILES_2)
OTHER_LIB_MCS_FLAGS = -nowarn:618 -r:System.Configuration.dll
endif


LIB_MCS_FLAGS = \
	-unsafe \
	-nowarn:612,618,649	\
	-r:$(corlib)	 		\
	-r:System.dll 			\
	-r:System.Drawing.dll 		\
	-r:System.Data.dll		\
	-r:System.Xml.dll		\
	-r:System.EnterpriseServices.dll \
	$(OTHER_LIB_MCS_FLAGS) \
	$(OTHER_RES:%=/resource:%)

ifneq ($(CYCLIC_DEPS:%=../lib/$(PROFILE)/%), $(CYCLIC_DEP_FILES))
NO_SIGN_ASSEMBLY = yes
NO_INSTALL = yes
all-local: echo-warning
.PHONY: echo-warning

ifeq   (, $(strip $(CYCLIC_DEP_FILES)))
echo-warning:
	@echo "** Warning: System.Web.dll built without parts that depend on: $(CYCLIC_DEPS)"
endif
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:219 -nowarn:169

EXTRA_DISTFILES = $(RESOURCE_FILES_2) $(RESOURCE_FILES_1)

include ../../build/library.make

ifdef WEBSERVICES_DEP_FILE
LIB_MCS_FLAGS += -define:WEBSERVICES_DEP -r:$(WEBSERVICES_DEP)
$(the_lib): $(WEBSERVICES_DEP_FILE)
endif
