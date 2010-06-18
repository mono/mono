thisdir = class/System.Web.Extensions
SUBDIRS = Test
include ../../build/rules.make

LIBRARY = System.Web.Extensions.dll

RESOURCE_FILES= \
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjax.debug.js	\
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjax.js		\
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjaxTimer.debug.js	\
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjaxTimer.js		\
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjaxWebForms.debug.js\
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjaxWebForms.js

RESOURCE_FILES_DIST= \
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjax.debug.js	\
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjax.js		\
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjaxTimer.debug.js	\
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjaxTimer.js		\
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjaxWebForms.debug.js\
	../MicrosoftAjaxLibrary/System.Web.Extensions/3.5.0.0/3.5.21022.8/MicrosoftAjaxWebForms.js\
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjax.debug.js	\
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjax.js		\
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjaxTimer.debug.js	\
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjaxTimer.js		\
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjaxWebForms.debug.js\
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjaxWebForms.js \
	Resources/MicrosoftAjaxExtension.js \
	Resources/MicrosoftAjaxWebFormsExtension.js

NUNIT_RESOURCE_FILES= \
	Test/resources/Web.mono.config \
	$(wildcard Test/resources/*.as?x) \
	$(wildcard ../System.Web/Test/mainsoft/NunitWeb/NunitWeb/Resources/*.cs) \
	$(wildcard ../System.Web/Test/mainsoft/NunitWeb/NunitWeb/Resources/*.as?x) \
	$(wildcard ../System.Web/Test/mainsoft/NunitWeb/NunitWeb/Resources/*.master)

CLASSLIB_DIR = $(topdir)/class/lib/$(PROFILE)

STANDALONE_RUNNER_SUPPORT_ASSEMBLY = $(CLASSLIB_DIR)/standalone-runner-support.dll
STANDALONE_TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) $(PROFILE_MCS_FLAGS) -debug:full -r:$(STANDALONE_RUNNER_SUPPORT_ASSEMBLY) -r:System.Web.dll -r:System.Web.Extensions.dll -r:nunit.framework.dll
STANDALONE_TEST_ASSEMBLY = System.Web.Extensions_standalone_test_$(PROFILE).dll
STANDALONE_TEST_MAKEFRAG = $(depsdir)/$(STANDALONE_TEST_ASSEMBLY).makefrag

STANDALONE_TEST_RUNNER = ../System.Web/Test/tools/standalone-runner.exe
RUN_STANDALONE = $(TEST_RUNTIME) $(STANDALONE_TEST_RUNNER)
ifdef TESTNAME
RUN_STANDALONE += --test=$(TESTNAME)
endif

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
OTHER_LIB_MCS_FLAGS += -r:System.Web.ApplicationServices.dll
endif

LIB_MCS_FLAGS = \
	-unsafe \
	-define:NET_3_5			\
	-define:SYSTEM_WEB_EXTENSIONS	\
	-r:$(corlib)	 		\
	-r:System.dll 			\
	-r:System.Core.dll		\
	-r:System.Drawing.dll 		\
	-r:System.Data.dll		\
	-r:System.Data.Linq.dll		\
	-r:System.Xml.dll		\
	-r:System.Web.dll		\
	-r:System.Web.Services.dll	\
	-r:System.Configuration.dll	\
	-r:System.EnterpriseServices.dll \
	-r:System.ServiceModel.dll	\
	$(OTHER_LIB_MCS_FLAGS) 		\
	$(RESOURCE_FILES:%=/resource:%)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -doc:$(test_lib:.dll=.xml) -nowarn:219,169,1591 $(NUNIT_RESOURCE_FILES:%=/resource:%) -r:SystemWebTestShim.dll

EXTRA_DISTFILES = $(RESOURCE_FILES_DIST) $(NUNIT_RESOURCE_FILES) \
	Standalone.Web.Extensions_test.dll.sources

run-standalone-test: $(STANDALONE_TEST_MAKEFRAG) $(STANDALONE_RUNNER_SUPPORT_MAKEFRAG) $(STANDALONE_TEST_ASSEMBLY)
	$(MAKE) -C ../System.Web/ standalone-runner-support
	$(MAKE) -C ../System.Web/Test/tools/ STANDALONE_SUPPORT_BUILT=1 standalone-runner.exe
	$(RUN_STANDALONE) $(STANDALONE_TEST_ASSEMBLY)

$(STANDALONE_TEST_MAKEFRAG): System.Web.Extensions_standalone_test.dll.sources
	@echo Creating $@ ...
	@sed 's,^,$(STANDALONE_TEST_ASSEMBLY): ,' $< > $@

include $(STANDALONE_TEST_MAKEFRAG)

$(STANDALONE_TEST_ASSEMBLY): $(the_assembly) Test/standalone-tests/Consts.cs
	$(CSCOMPILE) $(STANDALONE_TEST_MCS_FLAGS) -out:$@ -target:library @System.Web.Extensions_standalone_test.dll.sources

Test/standalone-tests/Consts.cs: Test/standalone-tests/Consts.cs.in
	@sed 's,@SystemWebExtensionsClassDir@,$(shell pwd),' $< > $@

include ../../build/library.make

