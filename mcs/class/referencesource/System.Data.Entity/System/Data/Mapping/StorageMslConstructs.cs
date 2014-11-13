//---------------------------------------------------------------------
// <copyright file="StorageMslConstructs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Mapping {
    /// <summary>
    /// Defines all the string constrcuts defined in CS MSL specification
    /// </summary>
    internal static class StorageMslConstructs {
        #region Fields
        internal const string NamespaceUriV1 = "urn:schemas-microsoft-com:windows:storage:mapping:CS";
        internal const string NamespaceUriV2 = "http://schemas.microsoft.com/ado/2008/09/mapping/cs";
        internal const string NamespaceUriV3 = "http://schemas.microsoft.com/ado/2009/11/mapping/cs";
        internal const double MappingVersionV1 = 1.0;
        internal const double MappingVersionV2 = 2.0;
        internal const double MappingVersionV3 = 3.0;
        internal const string MappingElement = "Mapping";
        internal const string GenerateUpdateViews = "GenerateUpdateViews";
        internal const string MappingSpaceAttribute = "Space";
        internal const string EntityContainerMappingElement = "EntityContainerMapping";
        internal const string CdmEntityContainerAttribute = "CdmEntityContainer";
        internal const string StorageEntityContainerAttribute = "StorageEntityContainer";
        internal const string AliasElement = "Alias";
        internal const string AliasKeyAttribute = "Key";
        internal const string AliasValueAttribute = "Value";
        internal const string EntitySetMappingElement = "EntitySetMapping";
        internal const string EntitySetMappingNameAttribute = "Name";
        internal const string EntitySetMappingTypeNameAttribute = "TypeName";
        internal const string EntitySetMappingStoreEntitySetAttribute = "StoreEntitySet";
        internal const string EntityTypeMappingElement = "EntityTypeMapping";
        internal const string QueryViewElement = "QueryView";
        internal const string EntityTypeMappingTypeNameAttribute = "TypeName";
        internal const string EntityTypeMappingStoreEntitySetAttribute = "StoreEntitySet";
        internal const string AssociationSetMappingElement = "AssociationSetMapping";
        internal const string AssociationSetMappingNameAttribute = "Name";
        internal const string AssociationSetMappingTypeNameAttribute = "TypeName";
        internal const string AssociationSetMappingStoreEntitySetAttribute = "StoreEntitySet";
        internal const string EndPropertyMappingElement = "EndProperty";
        internal const string EndPropertyMappingNameAttribute = "Name";
        internal const string CompositionSetMappingNameAttribute = "Name";
        internal const string CompositionSetMappingTypeNameAttribute = "TypeName";
        internal const string CompositionSetMappingStoreEntitySetAttribute = "StoreEntitySet";
        internal const string FunctionImportMappingElement = "FunctionImportMapping";
        internal const string FunctionImportMappingFunctionNameAttribute = "FunctionName";
        internal const string FunctionImportMappingFunctionImportNameAttribute = "FunctionImportName";
        internal const string CompositionSetParentEndName = "Parent";
        internal const string CompositionSetChildEndName = "Child";
        internal const string MappingFragmentElement = "MappingFragment";
        internal const string MappingFragmentStoreEntitySetAttribute = "StoreEntitySet";
        internal const string MappingFragmentMakeColumnsDistinctAttribute = "MakeColumnsDistinct";
        internal const string ScalarPropertyElement = "ScalarProperty";
        internal const string ScalarPropertyNameAttribute = "Name";
        internal const string ScalarPropertyColumnNameAttribute = "ColumnName";
        internal const string ScalarPropertyValueAttribute = "Value";
        internal const string ComplexPropertyElement = "ComplexProperty";
        internal const string AssociationEndElement = "AssociationEnd";
        internal const string ComplexPropertyNameAttribute = "Name";
        internal const string ComplexPropertyTypeNameAttribute = "TypeName";
        internal const string ComplexPropertyIsPartialAttribute = "IsPartial";
        internal const string ComplexTypeMappingElement = "ComplexTypeMapping";
        internal const string ComplexTypeMappingTypeNameAttribute = "TypeName";
        internal const string ConditionElement = "Condition";
        internal const string ConditionNameAttribute = "Name";
        internal const string ConditionValueAttribute = "Value";
        internal const string ConditionColumnNameAttribute = "ColumnName";
        internal const string ConditionIsNullAttribute = "IsNull";
        internal const string CollectionPropertyNameAttribute = "Name";
        internal const string CollectionPropertyIsPartialAttribute = "IsPartial";
        internal const string ResourceXsdNameV1 = "System.Data.Resources.CSMSL_1.xsd";
        internal const string ResourceXsdNameV2 = "System.Data.Resources.CSMSL_2.xsd";
        internal const string ResourceXsdNameV3 = "System.Data.Resources.CSMSL_3.xsd";
        internal const string IsTypeOf = "IsTypeOf(";
        internal const string IsTypeOfTerminal = ")";
        internal const string IsTypeOfOnly = "IsTypeOfOnly(";
        internal const string IsTypeOfOnlyTerminal = ")";
        internal const string ModificationFunctionMappingElement = "ModificationFunctionMapping";
        internal const string DeleteFunctionElement = "DeleteFunction";
        internal const string InsertFunctionElement = "InsertFunction";
        internal const string UpdateFunctionElement = "UpdateFunction";
        internal const string FunctionNameAttribute = "FunctionName";
        internal const string RowsAffectedParameterAttribute = "RowsAffectedParameter";
        internal const string ParameterNameAttribute = "ParameterName";
        internal const string ParameterVersionAttribute = "Version";
        internal const string ParameterVersionAttributeCurrentValue = "Current";
        internal const string AssociationSetAttribute = "AssociationSet";
        internal const string FromAttribute = "From";
        internal const string ToAttribute = "To";
        internal const string ResultBindingElement = "ResultBinding";
        internal const string ResultBindingPropertyNameAttribute = "Name";
        internal const string ResultBindingColumnNameAttribute = "ColumnName";
        internal const char TypeNameSperator = ';';
        internal const char IdentitySeperator = ':';
        internal const string EntityViewGenerationTypeName = "Edm_EntityMappingGeneratedViews.ViewsForBaseEntitySets";
        internal const string FunctionImportMappingResultMapping = "ResultMapping";
        #endregion
    }
}
