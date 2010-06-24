thisdir = class/System.Web
SUBDIRS = Test
include ../../build/rules.make

LIBRARY = System.Web.dll

ifneq (1, $(FRAMEWORK_VERSION_MAJOR))

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
TEST_APP_CODE_FILES = \
	Test/mainsoft/NunitWebResources/App_Code/EnumConverterControl.cs \
	Test/mainsoft/NunitWebResources/App_Code/MyContainer.cs \
	Test/mainsoft/NunitWebResources/App_Code/CustomCheckBoxColumn.cs

TEST_APP_GLOBALRESOURCES_FILES = \
	Test/mainsoft/NunitWebResources/App_GlobalResources/Common.resx \
	Test/mainsoft/NunitWebResources/App_GlobalResources/Common.fr-FR.resx \
	Test/mainsoft/NunitWebResources/App_GlobalResources/Resource1.resx

TEST_RESOURCE_FILES = \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/Global.asax \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/My.ashx \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/My.master \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/MyPage.aspx \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/MyPage.aspx.cs \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/MyPageWithMaster.aspx \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/Web.mono.config \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/Web.mono.config.1.1 \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/sub_map_01.sitemap \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/test_map_01.sitemap \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/test_map_02.sitemap \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/test_map_03.sitemap \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/test_map_04.sitemap \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/test_map_05.sitemap \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/test_map_06.sitemap \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/test_map_07.sitemap \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/test_map_08.sitemap \
	Test/mainsoft/NunitWeb/NunitWeb/Resources/test_map_09.sitemap \
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
	Test/mainsoft/NunitWebResources/DetailsViewTemplates_2.aspx \
	Test/mainsoft/NunitWebResources/DetailsViewTemplates_3.aspx \
	Test/mainsoft/NunitWebResources/DetailsViewDataActions.aspx \
	Test/mainsoft/NunitWebResources/DetailsViewProperties1.aspx \
	Test/mainsoft/NunitWebResources/Bluehills.jpg \
	Test/mainsoft/NunitWebResources/FormViewTest1.aspx \
	Test/mainsoft/NunitWebResources/FormViewTest1_2.aspx \
	Test/mainsoft/NunitWebResources/FormViewTest1_3.aspx \
	Test/mainsoft/NunitWebResources/FormViewTest1_4.aspx \
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
	Test/mainsoft/NunitWebResources/LinkInHeadWithEmbeddedExpression.aspx \
	Test/mainsoft/NunitWebResources/ExpressionInListControl.aspx \
	Test/mainsoft/NunitWebResources/ServerSideControlsInScriptBlock.aspx \
	Test/mainsoft/NunitWebResources/ServerControlInClientSideComment.aspx \
	Test/mainsoft/NunitWebResources/PreprocessorDirectivesInMarkup.aspx \
	Test/mainsoft/NunitWebResources/UnquotedAngleBrackets.aspx \
	Test/mainsoft/NunitWebResources/FullTagsInText.aspx \
	Test/mainsoft/NunitWebResources/TagsExpressionsAndCommentsInText.aspx \
	Test/mainsoft/NunitWebResources/NewlineInCodeExpression.aspx \
	Test/mainsoft/NunitWebResources/DuplicateControlsInClientComment.aspx \
	Test/mainsoft/NunitWebResources/TagsNestedInClientTag.aspx \
	Test/mainsoft/NunitWebResources/ConditionalClientComments.aspx \
	Test/mainsoft/NunitWebResources/OneLetterIdentifierInCodeRender.aspx \
	Test/mainsoft/NunitWebResources/GlobalResourcesLocalization.aspx \
	Test/mainsoft/NunitWebResources/TableSections_Bug551666.aspx \
	Test/mainsoft/NunitWebResources/TableSections_Bug551666.aspx.cs \
	Test/mainsoft/NunitWebResources/NestedParserFileText.aspx \
	Test/mainsoft/NunitWebResources/StateFormatter_CorrectConverter.aspx \
	Test/mainsoft/NunitWebResources/StateFormatter_CollectionConverter.aspx \
	Test/mainsoft/NunitWebResources/StateFormatter_CollectionConverter.aspx.cs \
	Test/mainsoft/NunitWebResources/ChangePasswordContainer_FindControl.aspx \
	Test/mainsoft/NunitWebResources/TagWithExpressionWithinAttribute.aspx \
	Test/mainsoft/NunitWebResources/CheckBoxList_Bug377703_1.aspx \
	Test/mainsoft/NunitWebResources/CheckBoxList_Bug377703_2.aspx \
	Test/mainsoft/NunitWebResources/CheckBoxList_Bug578770.aspx \
	Test/mainsoft/NunitWebResources/EnumConverter_Bug578586.aspx \
	Test/mainsoft/NunitWebResources/ButtonColor_Bug325489.aspx \
	Test/mainsoft/NunitWebResources/SqlDataSource_OnInit_Bug572781.aspx \
	Test/mainsoft/NunitWebResources/FormViewPagerVisibility.aspx \
	Test/mainsoft/NunitWebResources/OverridenControlsPropertyAndPostBack_Bug594238.aspx \
	Test/mainsoft/NunitWebResources/GlobalizationEncodingName.aspx \
	Test/mainsoft/NunitWebResources/CheckBoxField_Bug595568_0.aspx \
	Test/mainsoft/NunitWebResources/CheckBoxField_Bug595568_1.aspx \
	Test/mainsoft/NunitWebResources/CheckBoxField_Bug595568_2.aspx \
	Test/mainsoft/NunitWebResources/CheckBoxField_Bug595568_5.aspx \
	Test/mainsoft/NunitWebResources/CheckBoxField_Bug595568_6.aspx \
	Test/mainsoft/NunitWebResources/CheckBoxField_Bug595568_7.aspx \
	Test/mainsoft/NunitWebResources/GridView_Bug595567.aspx \
	Test/mainsoft/NunitWebResources/CheckBoxList_Bug600415.aspx

RESX_DIST =  resources/TranslationResources.resx
ifneq (1, $(FRAMEWORK_VERSION_MAJOR))
RESX_RES = $(RESX_DIST:.resx=.resources)
endif

NUNIT_RESOURCE_FILES = $(TEST_RESOURCE_FILES)
NUNIT_APP_CODE_FILES = $(TEST_APP_CODE_FILES)
NUNIT_APP_GLOBALRESOURCES_FILES = $(TEST_APP_GLOBALRESOURCES_FILES)

ifneq (1, $(FRAMEWORK_VERSION_MAJOR))
OTHER_RES += $(RESOURCE_FILES_2)
OTHER_LIB_MCS_FLAGS = -d:INSIDE_SYSTEM_WEB -nowarn:618 -r:System.Configuration.dll -r:Mono.Data.Sqlite.dll
endif

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
OTHER_LIB_MCS_FLAGS += -r:System.Web.ApplicationServices.dll
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
all-local: System.Web/UplevelHelper.cs resources/TranslationResources.resources echo-warning echo-warning-systemcore
.PHONY: echo-warning echo-warning-systemcore

ifeq   (, $(strip $(CYCLIC_DEP_FILES)))
echo-warning:
	@echo "** Warning: System.Web.dll built without parts that depend on: $(CYCLIC_DEPS)"
endif
endif

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
ifndef SYSTEMCORE_DEP_FILE
echo-warning-systemcore:
	@echo "** Warning: System.Web.dll built without parts that depend on $(SYSTEMCORE_DEP)"
endif
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -doc:$(test_lib:.dll=.xml) -nowarn:219,169,1591 -r:SystemWebTestShim.dll \
	$(NUNIT_RESOURCE_FILES:%=/resource:%) \
	$(foreach file,$(NUNIT_APP_CODE_FILES),$(shell echo $(file) | sed -e 's;\(.*\)/\(.*\);/resource:\1/\2,App_Code/\2 ;g')) \
	$(foreach file,$(NUNIT_APP_GLOBALRESOURCES_FILES),$(shell echo $(file) | sed -e 's;\(.*\)/\(.*\);/resource:\1/\2,App_GlobalResources/\2 ;g'))

ifeq (net_2_0, $(PROFILE))
TEST_MCS_FLAGS += -r:System.Web.Extensions.dll
endif

EXTRA_DISTFILES = \
	$(RESOURCE_FILES_2) \
	$(RESOURCE_FILES_1) \
	$(TEST_RESOURCE_FILES) \
	$(TEST_APP_CODE_FILES) \
	$(TEST_APP_GLOBALRESOURCES_FILES) \
	UplevelHelperDefinitions.xml \
	$(RESX_DIST) \
	SQLiteProviders_DatabaseSchema.sql \
	$(shell find Test/standalone-runner-support/ -name "*.cs" -type f -printf "'%p' ") \
	$(shell find Test/standalone-tests/ -name "*.cs" -type f -printf "'%p' " -o -name "*.cs.in" -type f -printf "'%p' ") \
	$(shell find Test/standalone/ -path '*/.svn' -prune -o -type f -printf "'%p' ") \
	$(shell find Test/tools/ -path '*/.svn' -prune -o -type f -printf "'%p' ") \
	System.Web_standalone_test.dll.sources \
	standalone-runner-support.dll.sources \
	ASPState.sql

BUILT_SOURCES = System.Web/UplevelHelper.cs 

include ../../build/library.make

ifdef WEBSERVICES_DEP_FILE
LIB_MCS_FLAGS += -define:WEBSERVICES_DEP -r:$(WEBSERVICES_DEP)
$(build_lib): $(WEBSERVICES_DEP_FILE)
endif

ifdef MONOWEB_DEP_FILE
LIB_MCS_FLAGS += -define:MONOWEB_DEP -r:$(MONOWEB_DEP)
$(build_lib): $(MONOWEB_DEP_FILE)
endif

ifdef SYSTEMCORE_DEP_FILE
LIB_MCS_FLAGS += -define:SYSTEMCORE_DEP -r:$(SYSTEMCORE_DEP)
$(build_lib): $(SYSTEMCORE_DEP_FILE)
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

ifdef PIPELINE_TIMER
LIB_MCS_FLAGS += -define:PIPELINE_TIMER
endif

ifdef DEVEL
LIB_MCS_FLAGS += -define:DEVEL
endif

CLASSLIB_DIR = $(topdir)/class/lib/$(PROFILE)

STANDALONE_RUNNER_SUPPORT_MCS_FLAGS = $(LIB_MCS_FLAGS) $(PROFILE_MCS_FLAGS) -d:STANDALONE_TEST -debug:full -r:System.Web.dll -r:nunit.framework.dll
STANDALONE_RUNNER_SUPPORT_ASSEMBLY = $(CLASSLIB_DIR)/standalone-runner-support.dll
STANDALONE_RUNNER_SUPPORT_MAKEFRAG = $(depsdir)/$(PROFILE)_standalone-runner-support.dll.makefrag

STANDALONE_TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) $(PROFILE_MCS_FLAGS) -debug:full -r:$(STANDALONE_RUNNER_SUPPORT_ASSEMBLY) -r:System.Web.dll -r:nunit.framework.dll
STANDALONE_TEST_ASSEMBLY = System.Web_standalone_test_$(PROFILE).dll
STANDALONE_TEST_MAKEFRAG = $(depsdir)/$(STANDALONE_TEST_ASSEMBLY).makefrag

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
STANDALONE_TEST_COMPILABLE_TESTS = \
	ApplicationPreStartMethods/test_01 \
	ApplicationPreStartMethods/test_02 \
	ApplicationPreStartMethods/test_03 \
	ApplicationPreStartMethods/test_04 \
	ApplicationPreStartMethods/test_05 \
	ApplicationPreStartMethods/test_06 \
	ApplicationPreStartMethods/test_07 \
	ApplicationPreStartMethods/test_08 \
	RegisterBuildProvider
endif

STANDALONE_TEST_RUNNER = Test/tools/standalone-runner.exe
RUN_STANDALONE = $(TEST_RUNTIME) $(STANDALONE_TEST_RUNNER)
ifdef TESTNAME
RUN_STANDALONE += --test=$(TESTNAME)
endif

$(build_lib): $(RESX_RES) $(RESOURCE_FILES_2) $(RESOURCE_FILES_1)

$(RESX_RES): %.resources: %.resx
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`

System.Web/UplevelHelper.cs: UplevelHelperDefinitions.xml
	$(TEST_RUNTIME) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/culevel.exe -o $@ $^

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

run-standalone-test: $(STANDALONE_TEST_MAKEFRAG) $(STANDALONE_RUNNER_SUPPORT_MAKEFRAG) $(STANDALONE_TEST_ASSEMBLY)
	$(MAKE) -C Test/tools/ STANDALONE_SUPPORT_BUILT=1 standalone-runner.exe
	$(RUN_STANDALONE) $(STANDALONE_TEST_ASSEMBLY)

standalone-runner-support: $(STANDALONE_RUNNER_SUPPORT_MAKEFRAG) $(STANDALONE_RUNNER_SUPPORT_ASSEMBLY)

$(STANDALONE_TEST_MAKEFRAG): System.Web_standalone_test.dll.sources
	@echo Creating $@ ...
	@sed 's,^,$(STANDALONE_TEST_ASSEMBLY): ,' $< > $@

$(STANDALONE_RUNNER_SUPPORT_MAKEFRAG): standalone-runner-support.dll.sources
	@echo Creating $@ ...
	@sed 's,^,$(STANDALONE_RUNNER_SUPPORT_ASSEMBLY): ,' $< > $@

include $(STANDALONE_TEST_MAKEFRAG)
include $(STANDALONE_RUNNER_SUPPORT_MAKEFRAG)

$(STANDALONE_TEST_ASSEMBLY): $(the_assembly) $(STANDALONE_RUNNER_SUPPORT_ASSEMBLY) Test/standalone-tests/Consts.cs standalone-compilable-tests
	$(CSCOMPILE) $(STANDALONE_TEST_MCS_FLAGS) -out:$@ -target:library @System.Web_standalone_test.dll.sources

standalone-compilable-tests:
ifdef STANDALONE_TEST_COMPILABLE_TESTS
	for d in $(STANDALONE_TEST_COMPILABLE_TESTS); do \
		$(MAKE) -C Test/standalone/$$d ; \
	done
endif

$(STANDALONE_RUNNER_SUPPORT_ASSEMBLY): 
	$(CSCOMPILE) $(STANDALONE_RUNNER_SUPPORT_MCS_FLAGS) -out:$(STANDALONE_RUNNER_SUPPORT_ASSEMBLY) -target:library @standalone-runner-support.dll.sources

Test/standalone-tests/Consts.cs: Test/standalone-tests/Consts.cs.in
	@sed 's,@SystemWebClassDir@,$(shell pwd),' $< > $@

clean-local:
ifdef STANDALONE_TEST_COMPILABLE_TESTS
	for d in $(STANDALONE_TEST_COMPILABLE_TESTS); do \
		$(MAKE) -C Test/standalone/$$d clean ; \
	done
endif
