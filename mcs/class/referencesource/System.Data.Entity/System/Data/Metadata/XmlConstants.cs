//---------------------------------------------------------------------
// <copyright file="XmlConstants.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       leil
// @backupOwner anpete
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class that contains all the constants for various schemas
    /// </summary>
    internal static class XmlConstants
    {
        // v3.5 of .net framework
        internal const string ModelNamespace_1 = "http://schemas.microsoft.com/ado/2006/04/edm";
        internal const string ModelNamespace_1_1 = "http://schemas.microsoft.com/ado/2007/05/edm";
        
        // v4 of .net framework
        internal const string ModelNamespace_2 = "http://schemas.microsoft.com/ado/2008/09/edm";
        
        // v4 next of .net framework
        internal const string ModelNamespace_3 = "http://schemas.microsoft.com/ado/2009/11/edm";
        
        internal const string ProviderManifestNamespace = "http://schemas.microsoft.com/ado/2006/04/edm/providermanifest";
        internal const string TargetNamespace_1 = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl";
        internal const string TargetNamespace_2 = "http://schemas.microsoft.com/ado/2009/02/edm/ssdl";
        internal const string TargetNamespace_3 = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
        internal const string CodeGenerationSchemaNamespace = "http://schemas.microsoft.com/ado/2006/04/codegeneration";
        internal const string EntityStoreSchemaGeneratorNamespace = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator";
        internal const string AnnotationNamespace = "http://schemas.microsoft.com/ado/2009/02/edm/annotation";

        internal const string Alias = "Alias";
        internal const string Provider = "Provider";
        internal const string ProviderManifestToken = "ProviderManifestToken";
        internal const string CSSpaceSchemaExtension = ".msl";
        internal const string CSpaceSchemaExtension = ".csdl";
        internal const string SSpaceSchemaExtension = ".ssdl";

        internal const double UndefinedVersion = 0.0;

        //Numeric Constant to represent V1 of CSDL schema
        internal const double EdmVersionForV1 = 1.0;
        //Numeric Constant to represent V1.1 of CSDL schema
        internal const double EdmVersionForV1_1 = 1.1;
        //Numeric Constant to represent V2.0 of CSDL schema
        internal const double EdmVersionForV2 = 2.0;
        //Numeric Constant to represent V3.0 of CSDL schema
        internal const double EdmVersionForV3 = 3.0;

        internal const double SchemaVersionLatest = EdmVersionForV3;
        internal const double StoreVersionForV1 = 1.0;
        internal const double StoreVersionForV2 = 2.0;
        internal const double StoreVersionForV3 = 3.0;



        #region CDM Schema Xml NodeNames
        // Const element names in the CDM schema xml
        internal const string Association = "Association";
        internal const string AssociationSet = "AssociationSet";
        internal const string ComplexType = "ComplexType";
        internal const string DefiningQuery = "DefiningQuery";
        internal const string DefiningExpression = "DefiningExpression";
        internal const string Documentation = "Documentation";
        internal const string DependentRole = "Dependent";
        internal const string End = "End";
        internal const string EntityType = "EntityType";
        internal const string EntityContainer = "EntityContainer";
        internal const string FunctionImport = "FunctionImport";
        internal const string Key = "Key";
        internal const string NavigationProperty = "NavigationProperty";
        internal const string OnDelete = "OnDelete";
        internal const string PrincipalRole = "Principal";
        internal const string Property = "Property";
        internal const string PropertyRef = "PropertyRef";
        internal const string ReferentialConstraint = "ReferentialConstraint";
        internal const string Role = "Role";
        internal const string Schema = "Schema";
        internal const string Summary = "Summary";
        internal const string LongDescription = "LongDescription";
        internal const string SampleValue = "SampleValue";
        internal const string EnumType = "EnumType";
        internal const string Member = "Member";
        internal const string ValueTerm = "ValueTerm";
        internal const string Annotations = "Annotations";
        internal const string ValueAnnotation = "ValueAnnotation";
        internal const string TypeAnnotation = "TypeAnnotation";

        internal const string Using = "Using";

        // constants used for codegen hints
        internal const string TypeAccess = "TypeAccess";
        internal const string MethodAccess = "MethodAccess";
        internal const string SetterAccess = "SetterAccess";
        internal const string GetterAccess = "GetterAccess";


        // const attribute names in the CDM schema XML
        internal const string Abstract = "Abstract";
        internal const string OpenType = "OpenType";
        internal const string Action = "Action";
        internal const string BaseType = "BaseType";
        internal const string EntitySet = "EntitySet";
        internal const string EntitySetPath = "EntitySetPath";
        internal const string Extends = "Extends";
        internal const string FromRole = "FromRole";
        internal const string Multiplicity = "Multiplicity";
        internal const string Name = "Name";
        internal const string Namespace = "Namespace";
        internal const string Table = "Table";
        internal const string ToRole = "ToRole";
        internal const string Relationship = "Relationship";
        internal const string ElementType = "ElementType";
        internal const string StoreGeneratedPattern = "StoreGeneratedPattern";
        internal const string IsFlags = "IsFlags";
        internal const string IsBindable = "IsBindable";
        internal const string IsSideEffecting = "IsSideEffecting";
        internal const string UnderlyingType = "UnderlyingType";
        internal const string Value = "Value";
        internal const string ContainsTarget = "ContainsTarget";

        // facet values
        internal const string Max = "Max";
        internal const string None = "None";
        internal const string Identity = "Identity";
        internal const string Computed = "Computed";
        internal const string Fixed = "Fixed";
        internal const string CollectionKind_None = "None";
        internal const string CollectionKind_List = "List";
        internal const string CollectionKind_Bag = "Bag";
        internal const string CollectionKind = "CollectionKind";
        internal const string In = "In";
        internal const string Out = "Out";
        internal const string InOut = "InOut";
        internal const string Variable = "Variable";

        // const attribute values in the CDM schema xml
        internal const string True = "true";
        internal const string False = "false";

        // xml constants used in provider manifest
        internal const string Function = "Function";
        internal const string ReturnType = "ReturnType";
        internal const string Parameter = "Parameter";
        internal const string Mode = "Mode";
        internal const string StoreFunctionName = "StoreFunctionName";

        internal const string ProviderManifestElement = "ProviderManifest";
        internal const string TypesElement = "Types";
        internal const string FunctionsElement = "Functions";
        internal const string TypeElement = "Type";
        internal const string FunctionElement = "Function";
        internal const string ScaleElement = "Scale";
        internal const string PrecisionElement = "Precision";
        internal const string MaxLengthElement = "MaxLength";
        internal const string FacetDescriptionsElement = "FacetDescriptions";
        internal const string UnicodeElement = "Unicode";
        internal const string FixedLengthElement = "FixedLength";
        internal const string ReturnTypeElement = "ReturnType";
        internal const string SridElement = "SRID";
        internal const string IsStrictElement = "IsStrict";
        internal const string TypeAttribute = "Type";

        internal const string MinimumAttribute = "Minimum";
        internal const string MaximumAttribute = "Maximum";
        internal const string NamespaceAttribute = "Namespace";
        internal const string DefaultValueAttribute = "DefaultValue";
        internal const string ConstantAttribute = "Constant";
        internal const string DestinationTypeAttribute = "DestinationType";
        internal const string PrimitiveTypeKindAttribute = "PrimitiveTypeKind";
        internal const string AggregateAttribute = "Aggregate";
        internal const string BuiltInAttribute = "BuiltIn";
        internal const string NameAttribute = "Name";
        internal const string IgnoreFacetsAttribute = "IgnoreFacets";
        internal const string NiladicFunction = "NiladicFunction";
        internal const string IsComposable = "IsComposable";
        internal const string CommandText = "CommandText";
        internal const string ParameterTypeSemantics = "ParameterTypeSemantics";
        internal const string CollectionType = "CollectionType";
        internal const string ReferenceType = "ReferenceType";
        internal const string RowType = "RowType";
        internal const string TypeRef = "TypeRef";
        internal const string UseStrongSpatialTypes = "UseStrongSpatialTypes";

        internal const string XmlCommentStartString = "<!--";
        internal const string XmlCommentEndString = "-->";


        #endregion // CDM Schema Xml NodeNames
    }
}