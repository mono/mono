thisdir = class/EntityFramework.SqlServer
SUBDIRS = 
include ../../build/rules.make

LIBRARY = EntityFramework.SqlServer.dll

System.Data.Entity.SqlServer.Properties.Resources.SqlServer.resources: ../../../external/entityframework/src/EntityFramework.SqlServer/Properties/Resources.SqlServer.resx
	$(RESGEN) "$<" "$@"


RESOURCES = \
	../../../external/entityframework/src/EntityFramework.SqlServer/Resources/System/Data/SqlClient/System.Data.Resources.SqlClient.SqlProviderServices.ProviderManifest.xml \
	../../../external/entityframework/src/EntityFramework.SqlServer/Resources/System/Data/SqlClient/System.Data.Resources.SqlClient.SqlProviderServices.StoreSchemaDefinition.ssdl \
	../../../external/entityframework/src/EntityFramework.SqlServer/Resources/System/Data/SqlClient/System.Data.Resources.SqlClient.SqlProviderServices.StoreSchemaDefinition_Sql8.ssdl \
	../../../external/entityframework/src/EntityFramework.SqlServer/Resources/System/Data/SqlClient/System.Data.Resources.SqlClient.SqlProviderServices.StoreSchemaDefinitionVersion3.ssdl \
	../../../external/entityframework/src/EntityFramework.SqlServer/Resources/System/Data/SqlClient/System.Data.Resources.SqlClient.SqlProviderServices.StoreSchemaDefinitionVersion3_Sql8.ssdl \
	../../../external/entityframework/src/EntityFramework.SqlServer/Resources/System/Data/SqlClient/System.Data.Resources.SqlClient.SqlProviderServices.StoreSchemaMapping.msl \
	../../../external/entityframework/src/EntityFramework.SqlServer/Resources/System/Data/SqlClient/System.Data.Resources.SqlClient.SqlProviderServices.StoreSchemaMappingVersion3.msl


LIB_MCS_FLAGS = -r:System.dll -r:System.Xml.dll -r:System.Core.dll -r:EntityFramework.dll -r:System.Data.dll \
		-keyfile:../ecma.pub -delaysign -warn:1 \
		-resource:System.Data.Entity.SqlServer.Properties.Resources.SqlServer.resources \
		$(RESOURCES:%=-resource:%)

include ../../build/library.make

$(the_lib): System.Data.Entity.SqlServer.Properties.Resources.SqlServer.resources
