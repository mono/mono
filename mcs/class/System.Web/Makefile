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
MONOWEB_DEP := Mono.Web.dll
MONOWEB_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(MONOWEB_DEP))
ifndef NO_SYSTEMCORE_DEP
SYSTEMCORE_DEP := System.Core.dll
SYSTEMCORE_DEP_FILE := $(wildcard ../lib/$(PROFILE)/$(SYSTEMCORE_DEP))
endif
CYCLIC_DEPS := $(WEBSERVICES_DEP) $(MONOWEB_DEP)
CYCLIC_DEP_FILES := $(WEBSERVICES_DEP_FILE) $(MONOWEB_DEP_FILE)
endif

RESOURCE_FILES_1= \
	resources/WebUIValidation.js

RESOURCE_FILES_2 = \
	resources/folder.gif \
	resources/file.gif \
	resources/computer.gif \
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
	resources/webform.js \
	resources/WebUIValidation_2.0.js \
	System.Web.UI.WebControls/GridView.js \
	System.Web.UI.WebControls/DetailsView.js \
	System.Web.UI.WebControls/TreeView.js \
	System.Web.UI.WebControls/Menu.js

OTHER_RES = $(RESOURCE_FILES_1)
TEST_RESOURCE_FILES = \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/Global.asax \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/Global.asax.cs \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/My.ashx \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/My.master \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/MyPage.aspx \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/MyPage.aspx.cs \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/MyPageWithMaster.aspx \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/Web.mono.config \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/Web.mono.config.1.1 \
	Test/mainsoft/NunitWebResources/menuclass.aspx \
	Test/mainsoft/NunitWebResources/FormView.aspx \
	Test/mainsoft/NunitWebResources/PostBackMenuTest.aspx \
	Test/mainsoft/NunitWebResources/PageWithStyleSheet.aspx \
	Test/mainsoft/NunitWebResources/PageWithTheme.aspx \
	Test/mainsoft/NunitWebResources/ResolveUrl.ascx \
	Test/mainsoft/NunitWebResources/ResolveUrl.aspx \
	Test/mainsoft/NunitWebResources/RunTimeSetTheme.aspx \
	Test/mainsoft/NunitWebResources/ReadOnlyPropertyBind.aspx \
	Test/mainsoft/NunitWebResources/ReadOnlyPropertyControl.ascx \
	Test/mainsoft/NunitWebResources/Theme1.skin \
	Test/mainsoft/NunitWebResources/Theme2.skin \
	Test/mainsoft/NunitWebResources/UrlProperty.aspx \
	Test/mainsoft/NunitWebResources/UrlProperty.ascx \
	Test/mainsoft/NunitWebResources/UrlProperty.ascx.cs \
	Test/mainsoft/NunitWebResources/Web.sitemap \
	Test/mainsoft/NunitWebResources/WizardTest.skin \
	Test/mainsoft/NunitWebResources/FooterTemplateTest.aspx \
	Test/mainsoft/NunitWebResources/DataGrid.aspx \
	Test/mainsoft/NunitWebResources/DetailsViewTemplates.aspx \
	Test/mainsoft/NunitWebResources/DetailsViewDataActions.aspx \
	Test/mainsoft/NunitWebResources/DetailsViewProperties1.aspx \
	Test/mainsoft/NunitWebResources/Bluehills.jpg \
	Test/mainsoft/NunitWebResources/FormViewTest1.aspx \
	Test/mainsoft/NunitWebResources/FormViewInsertEditDelete.aspx \
	Test/mainsoft/NunitWebResources/GridViewUpdate.aspx \
	Test/mainsoft/NunitWebResources/XMLDataSourceTest.xml \
	Test/mainsoft/NunitWebResources/XMLDataSourceTest.xsl \
	Test/mainsoft/NunitWebResources/XMLDataSourceTest1.aspx \
	Test/mainsoft/NunitWebResources/XMLDataSourceTest2.aspx \
	Test/mainsoft/NunitWebResources/XMLDataSourceTest3.aspx \
	Test/mainsoft/NunitWebResources/XMLDataSourceTest4.aspx \
	Test/mainsoft/NunitWebResources/LoginViewTest1.aspx \
	Test/mainsoft/NunitWebResources/WebControl.config \
	Test/mainsoft/NunitWebResources/WebLogin.config \
	Test/mainsoft/NunitWebResources/CallbackTest1.aspx \
	Test/mainsoft/NunitWebResources/CallbackTest2.aspx \
	Test/mainsoft/NunitWebResources/EventValidationTest2.aspx \
	Test/mainsoft/NunitWebResources/EventValidationTest1.aspx \
	Test/mainsoft/NunitWebResources/ClientScript.js \
	Test/mainsoft/NunitWebResources/EvalTest.aspx \
	Test/mainsoft/NunitWebResources/TemplateUserControl.ascx \
	Test/mainsoft/NunitWebResources/WebMapping.config \
	Test/mainsoft/NunitWebResources/Mapping.aspx \
	Test/mainsoft/NunitWebResources/Mapping1.aspx \
	Test/mainsoft/NunitWebResources/CrossPagePosting1.aspx \
	Test/mainsoft/NunitWebResources/CrossPagePosting2.aspx \
	Test/mainsoft/NunitWebResources/MyDerived.master \
	Test/mainsoft/NunitWebResources/MyPageWithDerivedMaster.aspx \
	Test/mainsoft/NunitWebResources/MasterTypeTest1.aspx \
	Test/mainsoft/NunitWebResources/MasterTypeTest2.aspx \
	Test/mainsoft/NunitWebResources/PageLifecycleTest.aspx \
	Test/mainsoft/NunitWebResources/PageValidationTest.aspx \
	Test/mainsoft/NunitWebResources/AsyncPage.aspx \
	Test/mainsoft/NunitWebResources/PageCultureTest.aspx \
	Test/mainsoft/NunitWebResources/adapters.browser \
	Test/mainsoft/NunitWebResources/NoEventValidation.aspx \
	Test/mainsoft/NunitWebResources/ListControlPage.aspx \
	Test/mainsoft/NunitWebResources/TextBoxTestlPage.aspx \
	Test/mainsoft/NunitWebResources/ClearErrorOnError.aspx \
	Test/mainsoft/NunitWebResources/RedirectOnError.aspx \
	Test/mainsoft/NunitWebResources/TestCapability.browser \
	Test/mainsoft/NunitWebResources/PageWithAdapter.aspx \
	Test/mainsoft/NunitWebResources/InvalidPropertyBind1.aspx \
	Test/mainsoft/NunitWebResources/InvalidPropertyBind2.aspx \
	Test/mainsoft/NunitWebResources/InvalidPropertyBind3.aspx \
	Test/mainsoft/NunitWebResources/InvalidPropertyBind4.aspx \
	Test/mainsoft/NunitWebResources/ValidPropertyBind1.aspx \
	Test/mainsoft/NunitWebResources/ValidPropertyBind2.aspx \
	Test/mainsoft/NunitWebResources/ValidPropertyBind3.aspx \
	Test/mainsoft/NunitWebResources/ValidPropertyBind4.aspx \
	Test/mainsoft/NunitWebResources/ValidPropertyBind5.aspx \
	Test/mainsoft/NunitWebResources/ReadWritePropertyControl.ascx \
	Test/mainsoft/MainsoftWebTest/nunitweb_config.xml \
	Test/mainsoft/NunitWebResources/TemplateControlParsingTest.aspx \
	Test/mainsoft/NunitWebResources/ContentPlaceHolderInTemplate.aspx \
	Test/mainsoft/NunitWebResources/ContentPlaceHolderInTemplate.master \
	Test/mainsoft/NunitWebResources/MissingMasterFile.aspx \
	Test/mainsoft/NunitWebResources/CustomSectionEmptyCollection.aspx \
	Test/mainsoft/NunitWebResources/NoDoubleOnInitOnRemoveAdd.aspx \
	Test/mainsoft/NunitWebResources/NoDoubleOnInitOnRemoveAdd.aspx.cs \
	Test/mainsoft/NunitWebResources/LoginDisplayRememberMe.aspx \
	Test/mainsoft/NunitWebResources/NoBindForMethodsWithBindInName.aspx \
	Test/mainsoft/NunitWebResources/LinkInHeadWithEmbeddedExpression.aspx

RESX_DIST =  resources/TranslationResources.resx
ifeq (net_2_0, $(PROFILE))
RESX_RES = $(RESX_DIST:.resx=.resources)
endif

NUNIT_RESOURCE_FILES = $(TEST_RESOURCE_FILES)
ifeq (net_2_0, $(PROFILE))
OTHER_RES += $(RESOURCE_FILES_2)
OTHER_LIB_MCS_FLAGS = -d:INSIDE_SYSTEM_WEB -nowarn:618 -r:System.Configuration.dll -r:Mono.Data.Sqlite.dll
endif

LIB_MCS_FLAGS = \
	-unsafe \
	-nowarn:612,618	\
	-r:$(corlib)	 		\
	-r:System.dll 			\
	-r:System.Drawing.dll 		\
	-r:System.Data.dll		\
	-r:System.Xml.dll		\
	-r:System.EnterpriseServices.dll \
	$(OTHER_LIB_MCS_FLAGS) \
	$(RESX_RES:%=/resource:%) \
	$(OTHER_RES:%=/resource:%)

ifneq ($(CYCLIC_DEPS:%=../lib/$(PROFILE)/%), $(CYCLIC_DEP_FILES))
NO_SIGN_ASSEMBLY = yes
NO_INSTALL = yes
all-local: System.Web/UplevelHelper.cs resources/TranslationResources.resources echo-warning
.PHONY: echo-warning

ifeq   (, $(strip $(CYCLIC_DEP_FILES)))
echo-warning:
	@echo "** Warning: System.Web.dll built without parts that depend on: $(CYCLIC_DEPS)"
endif
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -doc:$(test_lib:.dll=.xml)  -nowarn:219,169,1591 $(NUNIT_RESOURCE_FILES:%=/resource:%)
ifeq (net_2_0, $(PROFILE))
TEST_MCS_FLAGS += -r:System.Web.Extensions.dll
endif

EXTRA_DISTFILES = $(RESOURCE_FILES_2) $(RESOURCE_FILES_1) $(TEST_RESOURCE_FILES) UplevelHelperDefinitions.xml $(RESX_DIST) \
		  SQLiteProviders_DatabaseSchema.sql
BUILT_SOURCES = System.Web/UplevelHelper.cs 

include ../../build/library.make

ifdef WEBSERVICES_DEP_FILE
LIB_MCS_FLAGS += -define:WEBSERVICES_DEP -r:$(WEBSERVICES_DEP)
$(the_lib): $(WEBSERVICES_DEP_FILE)
endif

ifdef MONOWEB_DEP_FILE
LIB_MCS_FLAGS += -define:MONOWEB_DEP -r:$(MONOWEB_DEP)
$(the_lib): $(MONOWEB_DEP_FILE)
endif

ifdef SYSTEMCORE_DEP_FILE
LIB_MCS_FLAGS += -define:SYSTEMCORE_DEP -r:$(SYSTEMCORE_DEP)
$(the_lib): $(SYSTEMCORE_DEP_FILE)
endif

ifdef DEBUG
LIB_MCS_FLAGS += -define:DEBUG
endif

ifdef TRACE
LIB_MCS_FLAGS += -define:TRACE
endif

ifdef MONO_TRACE
LIB_MCS_FLAGS += -define:TRACE -define:MONO_TRACE
endif

ifdef DEVEL
LIB_MCS_FLAGS += -define:DEVEL
endif

$(the_lib): $(RESX_RES) $(RESOURCE_FILES2) $(RESOURCE_FILES_1)

$(RESX_RES): %.resources: %.resx
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`

System.Web/UplevelHelper.cs: UplevelHelperDefinitions.xml
	$(TEST_RUNTIME) $(topdir)/class/lib/net_1_1_bootstrap/culevel.exe -o $@ $^

run-aaa: test
	$(TEST_RUNTIME) $(TEST_HARNESS) $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_FLAGS) \
	/output:TestResult-$(PROFILE).log /include:aaa /xml:TestResult-$(PROFILE).xml $(test_assemblies) || ok=false; \
	(echo ''; cat TestResult-$(PROFILE).log) | sed '1,/^Tests run: /d'; \
	$$ok

run-nunitweb: test
	$(TEST_RUNTIME) $(TEST_HARNESS) $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_FLAGS) \
	/output:TestResult-$(PROFILE).log /include:NunitWeb /xml:TestResult-$(PROFILE).xml $(test_assemblies) || ok=false; \
	(echo ''; cat TestResult-$(PROFILE).log) | sed '1,/^Tests run: /d'; \
	$$ok

run-nunitweb-ondotnet: test
	$(TEST_HARNESS) $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_FLAGS) \
	/output:TestResult-ondotnet-$(PROFILE).log /include:NunitWeb /xml:TestResult-ondotnet-$(PROFILE).xml $(test_assemblies) || ok=false; \
	(echo ''; cat TestResult-ondotnet-$(PROFILE).log) | sed '1,/^Tests run: /d'; \
	$$ok

