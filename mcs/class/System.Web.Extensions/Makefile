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

EXTRA_DISTFILES = $(RESOURCE_FILES_DIST) $(NUNIT_RESOURCE_FILES)

include ../../build/library.make

