thisdir = class/EntityFramework
SUBDIRS = 
include ../../build/rules.make

LIBRARY = EntityFramework.dll

System.Data.Entity.Properties.Resources.resources: ../../../external/entityframework/src/EntityFramework/Properties/Resources.resx
	$(RESGEN) "$<" "$@"
	
System.Data.Entity.Properties.Resources.Migrate.resources: ../../../external/entityframework/src/EntityFramework/Properties/Resources.Migrate.resx
	$(RESGEN) "$<" "$@"

System.Data.Entity.Properties.Resources.PowerShell.resources: ../../../external/entityframework/src/EntityFramework/Properties/Resources.PowerShell.resx
	$(RESGEN) "$<" "$@"
	
RESOURCES = ../../../external/entityframework/src/EntityFramework/Resources/System/Data/System.Data.Resources.DbProviderServices.ConceptualSchemaDefinition.csdl \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/System.Data.Resources.DbProviderServices.ConceptualSchemaDefinitionVersion3.csdl \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/System.Data.Resources.ProviderServices.ProviderManifest.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/EntityModel/System.Data.Resources.AnnotationSchema.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/EntityModel/System.Data.Resources.CodeGenerationSchema.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/EntityModel/System.Data.Resources.CSDLSchema_1.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/EntityModel/System.Data.Resources.CSDLSchema_1_1.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/EntityModel/System.Data.Resources.CSDLSchema_2.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/EntityModel/System.Data.Resources.CSDLSchema_3.xsd \
	System.Data.Resources.EntityStoreSchemaGenerator.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/EntityModel/System.Data.Resources.SSDLSchema.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/EntityModel/System.Data.Resources.SSDLSchema_2.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/EntityModel/System.Data.Resources.SSDLSchema_3.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/MappingSpecification/System.Data.Resources.CSMSL_1.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/MappingSpecification/System.Data.Resources.CSMSL_2.xsd \
	../../../external/entityframework/src/EntityFramework/Resources/System/Data/MappingSpecification/System.Data.Resources.CSMSL_3.xsd

LIB_MCS_FLAGS = -r:System.Data.dll -r:System.Transactions.dll -r:System.Xml.Linq.dll -r:System.Configuration.dll -r:System.ComponentModel.DataAnnotations.dll -r:System.Runtime.Serialization.dll \
		-r:System.dll -r:System.Xml.dll -r:System.Core.dll -r:Microsoft.CSharp.dll -keyfile:../ecma.pub -delaysign -warn:1 \
		-resource:System.Data.Entity.Properties.Resources.resources \
		-resource:System.Data.Entity.Properties.Resources.Migrate.resources \
		-resource:System.Data.Entity.Properties.Resources.PowerShell.resources \
		$(RESOURCES:%=-resource:%)

CLEAN_FILES = System.Data.Resources.EntityStoreSchemaGenerator.xsd

System.Data.Resources.EntityStoreSchemaGenerator.xsd:
	cp ../../../external/entityframework/src/EntityFramework/Resources/System/Data/EntityModel/system.data.resources.entitystoreschemagenerator.xsd System.Data.Resources.EntityStoreSchemaGenerator.xsd

include ../../build/library.make

$(the_lib): System.Data.Resources.EntityStoreSchemaGenerator.xsd \
    System.Data.Entity.Properties.Resources.resources \
	System.Data.Entity.Properties.Resources.Migrate.resources \
	System.Data.Entity.Properties.Resources.PowerShell.resources
