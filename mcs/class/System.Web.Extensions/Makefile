thisdir = class/System.Web.Extensions
SUBDIRS = Test
include ../../build/rules.make

LIBRARY = System.Web.Extensions.dll

RESOURCE_FILES= \
	..\MicrosoftAjaxLibrary\System.Web.Extensions\1.0.61025.0\MicrosoftAjax.debug.js	\
	..\MicrosoftAjaxLibrary\System.Web.Extensions\1.0.61025.0\MicrosoftAjax.js		\
	..\MicrosoftAjaxLibrary\System.Web.Extensions\1.0.61025.0\MicrosoftAjaxTimer.debug.js	\
	..\MicrosoftAjaxLibrary\System.Web.Extensions\1.0.61025.0\MicrosoftAjaxTimer.js		\
	..\MicrosoftAjaxLibrary\System.Web.Extensions\1.0.61025.0\MicrosoftAjaxWebForms.debug.js\
	..\MicrosoftAjaxLibrary\System.Web.Extensions\1.0.61025.0\MicrosoftAjaxWebForms.js

LIB_MCS_FLAGS = \
	-unsafe \
	-r:$(corlib)	 		\
	-r:System.dll 			\
	-r:System.Drawing.dll 		\
	-r:System.Data.dll		\
	-r:System.Xml.dll		\
	-r:System.Web.dll		\
	-r:System.Web.Services.dll	\
	-r:System.Configuration.dll	\
	-r:System.EnterpriseServices.dll \
	$(OTHER_LIB_MCS_FLAGS) \
	$(RESOURCE_FILES:%=/resource:%)

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -doc:$(test_lib:.dll=.xml)  -nowarn:219 -nowarn:169 $(NUNIT_RESOURCE_FILES:%=/resource:%)

EXTRA_DISTFILES = $(RESOURCE_FILES) 

include ../../build/library.make

