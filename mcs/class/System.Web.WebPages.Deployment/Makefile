thisdir = class/System.Web.WebPages.Deployment
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.WebPages.Deployment.dll
LIBRARY_NAME = System.Web.WebPages.Deployment.dll

System.Web.WebPages.Deployment.Common.CommonResources.resources: ../../../external/aspnetwebstack/src/CommonResources.resx
	$(RESGEN) "$<" "$@"
	
System.Web.WebPages.Deployment.Resources.ConfigurationResources.resources: ../../../external/aspnetwebstack/src/System.Web.WebPages.Deployment/Resources/ConfigurationResources.resx
	$(RESGEN) "$<" "$@"

LIB_MCS_FLAGS = \
		/warn:1 \
		/keyfile:../winfx.pub -delaysign \
	        /r:System.dll \
	        /r:System.Core.dll \
	        /r:System.Configuration.dll \
	        /r:System.Web.dll \
	        /r:Microsoft.Web.Infrastructure.dll \
	        /d:ASPNETWEBPAGES \
		/resource:System.Web.WebPages.Deployment.Common.CommonResources.resources \
		/resource:System.Web.WebPages.Deployment.Resources.ConfigurationResources.resources

include ../../build/library.make

$(build_lib): System.Web.WebPages.Deployment.Common.CommonResources.resources \
		System.Web.WebPages.Deployment.Resources.ConfigurationResources.resources
