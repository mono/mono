//---------------------------------------------------------------------
// <copyright file="EdmConstants.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    internal static class EdmConstants
    {
        // Namespace for all the system types
        internal const string EdmNamespace = "Edm";
        internal const string ClrPrimitiveTypeNamespace = "System";

        internal const string TransientNamespace = "Transient";

        // max number of primitive types
        internal const int NumPrimitiveTypes = (int)System.Data.Metadata.Edm.PrimitiveTypeKind.GeographyCollection + 1;

        // max number of primitive types
        internal const int NumBuiltInTypes = (int)BuiltInTypeKind.TypeUsage + 1;

        // MaxLength for the string types: Name, Namespace, Version
        internal const int MaxLength = 256;

        // Name of the built in types
        internal const string AssociationEnd = "AssociationEnd";
        internal const string AssociationSetType = "AssocationSetType";
        internal const string AssociationSetEndType = "AssociationSetEndType";
        internal const string AssociationType = "AssociationType";
        internal const string BaseEntitySetType = "BaseEntitySetType";
        internal const string CollectionType = "CollectionType";
        internal const string ComplexType = "ComplexType";
        internal const string DeleteAction = "DeleteAction";
        internal const string DeleteBehavior = "DeleteBehavior";
        internal const string Documentation = "Documentation";
        internal const string EdmType = "EdmType";
        internal const string ElementType = "ElementType";
        internal const string EntityContainerType = "EntityContainerType";
        internal const string EntitySetType = "EntitySetType";
        internal const string EntityType = "EntityType";
        internal const string EnumerationMember = "EnumMember";
        internal const string EnumerationType = "EnumType";
        internal const string Facet = "Facet";
        internal const string Function = "EdmFunction";
        internal const string FunctionParameter = "FunctionParameter";
        internal const string GlobalItem = "GlobalItem";
        internal const string ItemAttribute = "MetadataProperty";
        internal const string ItemType = "ItemType";
        internal const string Member = "EdmMember";
        internal const string NavigationProperty = "NavigationProperty";
        internal const string OperationBehavior = "OperationBehavior";
        internal const string OperationBehaviors = "OperationBehaviors";
        internal const string ParameterMode = "ParameterMode";
        internal const string PrimitiveType = "PrimitiveType";
        internal const string PrimitiveTypeKind = "PrimitiveTypeKind";
        internal const string Property = "EdmProperty";
        internal const string ProviderManifest = "ProviderManifest";
        internal const string ReferentialConstraint = "ReferentialConstraint";
        internal const string RefType = "RefType";
        internal const string RelationshipEnd = "RelationshipEnd";
        internal const string RelationshipMultiplicity = "RelationshipMultiplicity";
        internal const string RelationshipSet = "RelationshipSet";
        internal const string RelationshipType = "RelationshipType";
        internal const string ReturnParameter = "ReturnParameter";
        internal const string Role = "Role";
        internal const string RowType = "RowType";
        internal const string SimpleType = "SimpleType";
        internal const string StructuralType = "StructuralType";
        internal const string TypeUsage = "TypeUsage";

        //Enum value of date time kind
        internal const string Utc = "Utc";
        internal const string Unspecified = "Unspecified";
        internal const string Local = "Local";

        //Enum value of multiplicity kind
        internal const string One = "One";
        internal const string ZeroToOne = "ZeroToOne";
        internal const string Many = "Many";

        //Enum value of Parameter Mode 
        internal const string In = "In";
        internal const string Out = "Out";
        internal const string InOut = "InOut";

        //Enum value of DeleteAction Mode 
        internal const string None = "None";
        internal const string Cascade = "Cascade";
        internal const string Restrict = "Restrict";

        //Enum Value of CollectionKind
        internal const string NoneCollectionKind = "None";
        internal const string ListCollectionKind = "List";
        internal const string BagCollectionKind = "Bag";

        //Enum Value of MaxLength (max length can be a single enum value, or a positive integer)
        internal const string MaxMaxLength = "Max";

        //Enum Value of SRID (srid can be a single enum value, or a positive integer)
        internal const string VariableSrid = "Variable";

        // Members of the built in types
        internal const string AssociationSetEnds = "AssociationSetEnds";
        internal const string Child = "Child";
        internal const string DefaultValue = "DefaultValue";
        internal const string Ends = "Ends";
        internal const string EntitySet = "EntitySet";
        internal const string AssociationSet = "AssociationSet";
        internal const string EntitySets = "EntitySets";
        internal const string Facets = "Facets";
        internal const string FromProperties = "FromProperties";
        internal const string FromRole = "FromRole";
        internal const string IsParent = "IsParent";
        internal const string KeyMembers = "KeyMembers";
        internal const string Members = "Members";
        internal const string Mode = "Mode";
        internal const string Nullable = "Nullable";
        internal const string Parameters = "Parameters";
        internal const string Parent = "Parent";
        internal const string Properties = "Properties";
        internal const string ToProperties = "ToProperties";
        internal const string ToRole = "ToRole";
        internal const string ReferentialConstraints = "ReferentialConstraints";
        internal const string RelationshipTypeName = "RelationshipTypeName";
        internal const string ReturnType = "ReturnType";
        internal const string ToEndMemberName = "ToEndMemberName";
        internal const string CollectionKind = "CollectionKind";

        // Name of the primitive types
        internal const string Binary = "Binary";
        internal const string Boolean = "Boolean";
        internal const string Byte = "Byte";
        internal const string DateTime = "DateTime";
        internal const string Decimal = "Decimal";
        internal const string Double = "Double";
        internal const string Geometry = "Geometry";
        internal const string GeometryPoint = "GeometryPoint";
        internal const string GeometryLineString = "GeometryLineString";
        internal const string GeometryPolygon = "GeometryPolygon";
        internal const string GeometryMultiPoint = "GeometryMultiPoint";
        internal const string GeometryMultiLineString = "GeometryMultiLineString";
        internal const string GeometryMultiPolygon = "GeometryMultiPolygon";
        internal const string GeometryCollection = "GeometryCollection";
        internal const string Geography = "Geography";
        internal const string GeographyPoint = "GeographyPoint";
        internal const string GeographyLineString = "GeographyLineString";
        internal const string GeographyPolygon = "GeographyPolygon";
        internal const string GeographyMultiPoint = "GeographyMultiPoint";
        internal const string GeographyMultiLineString = "GeographyMultiLineString";
        internal const string GeographyMultiPolygon = "GeographyMultiPolygon";
        internal const string GeographyCollection = "GeographyCollection";
        internal const string Guid = "Guid";
        internal const string Single = "Single";
        internal const string SByte = "SByte";
        internal const string Int16 = "Int16";
        internal const string Int32 = "Int32";
        internal const string Int64 = "Int64";
        internal const string Money = "Money";
        internal const string Null = "Null";
        internal const string String = "String";
        internal const string DateTimeOffset = "DateTimeOffset";
        internal const string Time = "Time";
        internal const string UInt16 = "UInt16";
        internal const string UInt32 = "UInt32";
        internal const string UInt64 = "UInt64";
        internal const string Xml = "Xml";

        // Name of the system defined attributes on edm type
        internal const string Name = "Name";
        internal const string Namespace = "Namespace";
        internal const string Abstract = "Abstract";
        internal const string BaseType = "BaseType";
        internal const string Sealed = "Sealed";
        internal const string ItemAttributes = "MetadataProperties";
        internal const string Type = "Type";

        // Name of SSDL specifc attributes for SQL Gen
        internal const string Schema = "Schema";
        internal const string Table = "Table";

        // Name of the additional system defined attributes on item attribute
        internal const string FacetType = "FacetType";
        internal const string Value = "Value";

        // Name of the additional system defined attributes on enum types
        internal const string EnumMembers = "EnumMembers";



        //
        // Provider Manifest EdmFunction Attributes
        //
        internal const string BuiltInAttribute = "BuiltInAttribute";
        internal const string StoreFunctionNamespace = "StoreFunctionNamespace";
        internal const string ParameterTypeSemanticsAttribute = "ParameterTypeSemanticsAttribute";
        internal const string ParameterTypeSemantics = "ParameterTypeSemantics";
        internal const string NiladicFunctionAttribute = "NiladicFunctionAttribute";
        internal const string IsComposableFunctionAttribute = "IsComposable";
        internal const string CommandTextFunctionAttribyte = "CommandText";
        internal const string StoreFunctionNameAttribute = "StoreFunctionNameAttribute";

        /// <summary>
        /// Used to denote application home directory in a Web/ASP.NET context
        /// </summary>
        internal const string WebHomeSymbol = "~";

        // Name of Properties belonging to EDM's Documentation construct
        internal const string Summary = "Summary";
        internal const string LongDescription = "LongDescription";

        internal static readonly Unbounded UnboundedValue = Unbounded.Instance;
        internal class Unbounded
        {
            static readonly Unbounded _instance = new Unbounded();
            private Unbounded() { }
            static internal Unbounded Instance { get { return _instance; } }
            public override string ToString() { return MaxMaxLength; }
        }

        internal static readonly Variable VariableValue = Variable.Instance;
        internal class Variable
        {
            static readonly Variable _instance = new Variable();
            private Variable() { }
            static internal Variable Instance { get { return _instance; } }
            public override string ToString() { return VariableSrid; }
        }
    }
}
