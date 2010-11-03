thisdir = class/System.Web.Extensions_1.0
SUBDIRS = 
include ../../build/rules.make

NO_TEST = yes
LIBRARY = System.Web.Extensions_1.0.dll
LIBRARY_NAME = System.Web.Extensions.dll
LIBRARY_COMPAT = yes

RESOURCE_FILES= \
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjax.debug.js	\
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjax.js		\
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjaxTimer.debug.js	\
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjaxTimer.js		\
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjaxWebForms.debug.js\
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjaxWebForms.js

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
	../MicrosoftAjaxLibrary/System.Web.Extensions/1.0.61025.0/MicrosoftAjaxWebForms.js
	
LIB_MCS_FLAGS = \
	-unsafe \
	-define:SYSTEM_WEB_EXTENSIONS	\
	-r:$(corlib)	 		\
	-r:System.dll 			\
	-r:System.Core.dll		\
	-r:System.Drawing.dll 		\
	-r:System.Data.dll		\
	-r:System.Xml.dll		\
	-r:System.Web.dll		\
	-r:System.Web.Services.dll	\
	-r:System.Configuration.dll	\
	-r:System.EnterpriseServices.dll \
	$(OTHER_LIB_MCS_FLAGS) 		\
	$(RESOURCE_FILES:%=/resource:%)

EXTRA_DISTFILES = $(RESOURCE_FILES_DIST) 

include ../../build/library.make

