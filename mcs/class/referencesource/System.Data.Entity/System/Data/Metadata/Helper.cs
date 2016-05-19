//---------------------------------------------------------------------
// <copyright file="Helper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// Helper Class for EDM Metadata - this class contains all the helper methods
    /// which only accesses public methods/properties. The other partial class contains all 
    /// helper methods which just uses internal methods/properties. The reason why we 
    /// did this for allowing view gen to happen at compile time - all the helper
    /// methods that view gen or mapping uses are in this class. Rest of the
    /// methods are in this class
    /// </summary>
    internal static partial class Helper
    {
        #region Fields
        internal static readonly EdmMember[] EmptyArrayEdmProperty = new EdmMember[0];

        #endregion

        #region Methods
        /// <summary>
        /// The method wraps the GetAttribute method on XPathNavigator.
        /// The problem with using the method directly is that the 
        /// Get Attribute method does not differentiate the absence of an attribute and
        /// having an attribute with Empty string value. In both cases the value returned is an empty string.
        /// So in case of optional attributes, it becomes hard to distinguish the case whether the 
        /// xml contains the attribute with empty string or doesn't contain the attribute
        /// This method will return null if the attribute is not present and otherwise will return the
        /// attribute value.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="attributeName">name of the attribute</param>
        /// <returns></returns>
        static internal string GetAttributeValue(XPathNavigator nav, 
                                                 string attributeName)
        {
            //Clone the navigator so that there wont be any sideeffects on the passed in Navigator
            nav = nav.Clone();
            string attributeValue = null;
            if (nav.MoveToAttribute(attributeName, string.Empty))
            {
                attributeValue = nav.Value;
            }
            return attributeValue;
        }

        /// <summary>
        /// The method returns typed attribute value of the specified xml attribute.
        /// The method does not do any specific casting but uses the methods on XPathNavigator.
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="attributeName"></param>
        /// <param name="clrType"></param>
        /// <returns></returns>
        internal static object GetTypedAttributeValue(XPathNavigator nav, 
                                                     string attributeName,
                                                     Type clrType) 
        {
            //Clone the navigator so that there wont be any sideeffects on the passed in Navigator
            nav = nav.Clone();
            object attributeValue = null;
            if (nav.MoveToAttribute(attributeName, string.Empty))
            {
                attributeValue = nav.ValueAs(clrType);
            }
            return attributeValue;
        }
        
        /// <summary>
        /// Searches for Facet Description with the name specified. 
        /// </summary>
        /// <param name="facetCollection">Collection of facet description</param>
        /// <param name="facetName">name of the facet</param>
        /// <returns></returns>
        internal static FacetDescription GetFacet(IEnumerable<FacetDescription> facetCollection, string facetName)
        {
            foreach (FacetDescription facetDescription in facetCollection)
            {
                if (facetDescription.FacetName == facetName)
                {
                    return facetDescription;
                }
            }

            return null;
        }

        // requires: firstType is not null
        // effects: Returns true iff firstType is assignable from secondType
        internal static bool IsAssignableFrom(EdmType firstType, EdmType secondType)
        {
            Debug.Assert(firstType != null, "firstType should not be not null");
            if (secondType == null)
            {
                return false;
            }
            return firstType.Equals(secondType) || IsSubtypeOf(secondType, firstType);
        }

        // requires: firstType is not null
        // effects: if otherType is among the base types, return true, 
        // otherwise returns false.
        // when othertype is same as the current type, return false.
        internal static bool IsSubtypeOf(EdmType firstType, EdmType secondType)
        {
            Debug.Assert(firstType != null, "firstType should not be not null");
            if (secondType == null)
            {
                return false;
            }

            // walk up my type hierarchy list
            for (EdmType t = firstType.BaseType; t != null; t = t.BaseType)
            {
                if (t == secondType)
                    return true;
            }
            return false;
        }

        internal static IList GetAllStructuralMembers(EdmType edmType)
        {
            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.AssociationType:
                    return ((AssociationType)edmType).AssociationEndMembers;
                case BuiltInTypeKind.ComplexType:
                    return ((ComplexType)edmType).Properties;
                case BuiltInTypeKind.EntityType:
                    return ((EntityType)edmType).Properties;
                case BuiltInTypeKind.RowType:
                    return ((RowType)edmType).Properties;
                default:
                    return EmptyArrayEdmProperty;
            }
        }

        internal static AssociationEndMember GetEndThatShouldBeMappedToKey(AssociationType associationType)
        {
            //For 1:* and 1:0..1 associations, the end other than 1 i.e. either * or 0..1 ends need to be 
            //mapped to key columns
            if (associationType.AssociationEndMembers.Any( it =>
                it.RelationshipMultiplicity.Equals(RelationshipMultiplicity.One)))
            {
                {
                    return associationType.AssociationEndMembers.SingleOrDefault(it =>
                        ((it.RelationshipMultiplicity.Equals(RelationshipMultiplicity.Many))
                         || (it.RelationshipMultiplicity.Equals(RelationshipMultiplicity.ZeroOrOne))));
                }
            }
            //For 0..1:* associations, * end must be mapped to key.
            else if (associationType.AssociationEndMembers.Any(it => 
                (it.RelationshipMultiplicity.Equals(RelationshipMultiplicity.ZeroOrOne))))
            {
                {
                    return associationType.AssociationEndMembers.SingleOrDefault(it =>
                        ((it.RelationshipMultiplicity.Equals(RelationshipMultiplicity.Many))));
                }
            }                
            return null;
        }

        /// <summary>
        /// Creates a single comma delimited string given a list of strings
        /// </summary>
        /// <param name="stringList"></param>
        /// <returns></returns>
        internal static String GetCommaDelimitedString(IEnumerable<string> stringList)
        {
            Debug.Assert(stringList != null , "Expecting a non null list");
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string part in stringList)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                else
                {
                    first = false;
                }
                
                sb.Append(part);
            }
            return sb.ToString();
        }

        
        // effects: concatenates all given enumerations
        internal static IEnumerable<T> Concat<T>(params IEnumerable<T>[] sources)
        {
            foreach (IEnumerable<T> source in sources)
            {
                if (null != source)
                {
                    foreach (T element in source)
                    {
                        yield return element;
                    }
                }
            }
        }

        internal static void DisposeXmlReaders(IEnumerable<XmlReader> xmlReaders)
        {
            Debug.Assert(xmlReaders != null);

            foreach (XmlReader xmlReader in xmlReaders)
            {
                ((IDisposable)xmlReader).Dispose();
            }
        }

        #region IsXXXType Methods
        internal static bool IsStructuralType(EdmType type)
        {
            return (IsComplexType(type) || IsEntityType(type) || IsRelationshipType(type) || IsRowType(type));
        }

        internal static bool IsCollectionType(GlobalItem item)
        {
            return (BuiltInTypeKind.CollectionType == item.BuiltInTypeKind);
        }

        internal static bool IsEntityType(EdmType type)
        {
            return (BuiltInTypeKind.EntityType == type.BuiltInTypeKind);
        }

        internal static bool IsComplexType(EdmType type)
        {
            return (BuiltInTypeKind.ComplexType == type.BuiltInTypeKind);
        }

        internal static bool IsPrimitiveType(EdmType type)
        {
            return (BuiltInTypeKind.PrimitiveType == type.BuiltInTypeKind);
        }

        internal static bool IsRefType(GlobalItem item)
        {
            return (BuiltInTypeKind.RefType == item.BuiltInTypeKind);
        }

        internal static bool IsRowType(GlobalItem item)
        {
            return (BuiltInTypeKind.RowType == item.BuiltInTypeKind);
        }

        internal static bool IsAssociationType(EdmType type)
        {
            return (BuiltInTypeKind.AssociationType == type.BuiltInTypeKind);
        }

        internal static bool IsRelationshipType(EdmType type)
        {
            return (BuiltInTypeKind.AssociationType == type.BuiltInTypeKind);
        }

        internal static bool IsEdmProperty(EdmMember member)
        {
            return (BuiltInTypeKind.EdmProperty == member.BuiltInTypeKind);
        }

        internal static bool IsRelationshipEndMember(EdmMember member)
        {
            return (BuiltInTypeKind.AssociationEndMember == member.BuiltInTypeKind);
        }

        internal static bool IsAssociationEndMember(EdmMember member)
        {
            return (BuiltInTypeKind.AssociationEndMember == member.BuiltInTypeKind);
        }

        internal static bool IsNavigationProperty(EdmMember member)
        {
            return (BuiltInTypeKind.NavigationProperty == member.BuiltInTypeKind);
        }

        internal static bool IsEntityTypeBase(EdmType edmType)
        {
            return Helper.IsEntityType(edmType) ||
                   Helper.IsRelationshipType(edmType);
        }

        internal static bool IsTransientType(EdmType edmType)
        {
            return Helper.IsCollectionType(edmType) ||
                   Helper.IsRefType(edmType) ||
                   Helper.IsRowType(edmType);
        }

        internal static bool IsEntitySet(EntitySetBase entitySetBase)
        {
            return BuiltInTypeKind.EntitySet == entitySetBase.BuiltInTypeKind;
        }

        internal static bool IsRelationshipSet(EntitySetBase entitySetBase)
        {
            return BuiltInTypeKind.AssociationSet == entitySetBase.BuiltInTypeKind;
        }

        internal static bool IsEntityContainer(GlobalItem item)
        {
            return BuiltInTypeKind.EntityContainer == item.BuiltInTypeKind;
        }

        internal static bool IsEdmFunction(GlobalItem item)
        {
            return BuiltInTypeKind.EdmFunction == item.BuiltInTypeKind;
        }
               
        internal static string GetFileNameFromUri(Uri uri)
        {
            if ( uri == null )
                throw new ArgumentNullException("uri");
            if ( uri.IsFile )
                return uri.LocalPath;

            if ( uri.IsAbsoluteUri )
                return uri.AbsolutePath;

            throw new ArgumentException(System.Data.Entity.Strings.UnacceptableUri(uri),"uri");
        }

        internal static bool IsEnumType(EdmType edmType)
        {
            Debug.Assert(edmType != null, "edmType != null");
            return BuiltInTypeKind.EnumType == edmType.BuiltInTypeKind;
        }

        internal static bool IsUnboundedFacetValue(Facet facet)
        {
            return object.ReferenceEquals(facet.Value, EdmConstants.UnboundedValue);
        }

        internal static bool IsVariableFacetValue(Facet facet)
        {
            return object.ReferenceEquals(facet.Value, EdmConstants.VariableValue);
        }

        internal static bool IsScalarType(EdmType edmType)
        {
            return IsEnumType(edmType) || IsPrimitiveType(edmType);
        }

        internal static bool IsSpatialType(PrimitiveType type)
        {
            return IsGeographicType(type) || IsGeometricType(type);
        }

        internal static bool IsSpatialType(EdmType type, out bool isGeographic)
        {
            PrimitiveType pt = type as PrimitiveType;
            if (pt == null)
            {
                isGeographic = false;
                return false;
            }
            else
            {
                isGeographic = IsGeographicType(pt);
                return isGeographic || IsGeometricType(pt);
            }
        }

        internal static bool IsGeographicType(PrimitiveType type)
        {
            return IsGeographicTypeKind(type.PrimitiveTypeKind);
        }

        internal static bool AreSameSpatialUnionType(PrimitiveType firstType, PrimitiveType secondType)
        {
            // for the purposes of type checking all geographic types should be treated as if they were the Geography union type.
            if (Helper.IsGeographicTypeKind(firstType.PrimitiveTypeKind) && Helper.IsGeographicTypeKind(secondType.PrimitiveTypeKind))
            {
                return true;
            }

            // for the purposes of type checking all geometric types should be treated as if they were the Geometry union type.
            if (Helper.IsGeometricTypeKind(firstType.PrimitiveTypeKind) && Helper.IsGeometricTypeKind(secondType.PrimitiveTypeKind))
            {
                return true;
            }

            return false;
        }

        internal static bool IsGeographicTypeKind(PrimitiveTypeKind kind)
        {
            return kind == PrimitiveTypeKind.Geography || IsStrongGeographicTypeKind(kind);
        }

        internal static bool IsGeometricType(PrimitiveType type)
        {
            return IsGeometricTypeKind(type.PrimitiveTypeKind);
        }

        internal static bool IsGeometricTypeKind(PrimitiveTypeKind kind)
        {
            return kind == PrimitiveTypeKind.Geometry || IsStrongGeometricTypeKind(kind);
        }

        internal static bool IsStrongSpatialTypeKind(PrimitiveTypeKind kind)
        {
            return IsStrongGeometricTypeKind(kind) || IsStrongGeographicTypeKind(kind);
        }

        static bool IsStrongGeometricTypeKind(PrimitiveTypeKind kind)
        {
            return kind >= PrimitiveTypeKind.GeometryPoint && kind <= PrimitiveTypeKind.GeometryCollection;
        }

        static bool IsStrongGeographicTypeKind(PrimitiveTypeKind kind)
        {
            return kind >= PrimitiveTypeKind.GeographyPoint && kind <= PrimitiveTypeKind.GeographyCollection;
        }

        internal static bool IsSpatialType(TypeUsage type)
        {
            return (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType && IsSpatialType((PrimitiveType)type.EdmType));
        }

        internal static bool IsSpatialType(TypeUsage type, out PrimitiveTypeKind spatialType)
        {
            if (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                PrimitiveType primitiveType = (PrimitiveType)type.EdmType;
                if (IsGeographicTypeKind(primitiveType.PrimitiveTypeKind) || IsGeometricTypeKind(primitiveType.PrimitiveTypeKind))
                {
                    spatialType = primitiveType.PrimitiveTypeKind;
                    return true;
                }
            }

            spatialType = default(PrimitiveTypeKind);
            return false;
        }

        #endregion /// IsXXXType region

        /// <remarks>Performance of Enum.ToString() is slow and we use this value in building Identity</remarks>
        internal static string ToString(System.Data.ParameterDirection value)
        {
            switch (value)
            {
                case ParameterDirection.Input:
                    return "Input";
                case ParameterDirection.Output:
                    return "Output";
                case ParameterDirection.InputOutput:
                    return "InputOutput";
                case ParameterDirection.ReturnValue:
                    return "ReturnValue";
                default:
                    Debug.Assert(false, "which ParameterDirection.ToString() is missing?");
                    return value.ToString();
            }
        }

        /// <remarks>Performance of Enum.ToString() is slow and we use this value in building Identity</remarks>
        internal static string ToString(System.Data.Metadata.Edm.ParameterMode value)
        {
            switch (value)
            {
                case ParameterMode.In:
                    return EdmConstants.In;
                case ParameterMode.Out:
                    return EdmConstants.Out;
                case ParameterMode.InOut:
                    return EdmConstants.InOut;
                case ParameterMode.ReturnValue:
                    return "ReturnValue";
                default:
                    Debug.Assert(false, "which ParameterMode.ToString() is missing?");
                    return value.ToString();
            }
        }

        /// <summary>
        /// Verifies whether the given <paramref name="typeKind"/> is a valid underlying type for an enumeration type.
        /// </summary>
        /// <param name="typeKind">
        /// <see cref="PrimitiveTypeKind"/> to verifiy.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <paramref name="typeKind"/> is a valid underlying type for an enumeration type. Otherwise <c>false</c>.
        /// </returns>
        internal static bool IsSupportedEnumUnderlyingType(PrimitiveTypeKind typeKind)
        {
            return typeKind == PrimitiveTypeKind.Byte ||
                   typeKind == PrimitiveTypeKind.SByte ||
                   typeKind == PrimitiveTypeKind.Int16 ||
                   typeKind == PrimitiveTypeKind.Int32 ||
                   typeKind == PrimitiveTypeKind.Int64;
        }

        private static readonly Dictionary<PrimitiveTypeKind, long[]> _enumUnderlyingTypeRanges =
            new Dictionary<PrimitiveTypeKind, long[]>
            {
                { PrimitiveTypeKind.Byte,  new long[] { Byte.MinValue,  Byte.MaxValue  } },
                { PrimitiveTypeKind.SByte, new long[] { SByte.MinValue, SByte.MaxValue } },
                { PrimitiveTypeKind.Int16, new long[] { Int16.MinValue, Int16.MaxValue } },
                { PrimitiveTypeKind.Int32, new long[] { Int32.MinValue, Int32.MaxValue } },
                { PrimitiveTypeKind.Int64, new long[] { Int64.MinValue, Int64.MaxValue } },
            };

        /// <summary>
        /// Verifies whether a value of a member of an enumeration type is in range according to underlying type of the enumeration type.
        /// </summary>
        /// <param name="underlyingTypeKind">Underlying type of the enumeration type.</param>
        /// <param name="value">Value to check.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="value"/> is in range of the <paramref name="underlyingTypeKind"/>. <c>false</c> otherwise.
        /// </returns>
        internal static bool IsEnumMemberValueInRange(PrimitiveTypeKind underlyingTypeKind, long value)
        {
            Debug.Assert(IsSupportedEnumUnderlyingType(underlyingTypeKind), "Unsupported underlying type.");

            return value >= _enumUnderlyingTypeRanges[underlyingTypeKind][0] && value <= _enumUnderlyingTypeRanges[underlyingTypeKind][1];
        }

        /// <summary>
        /// Checks whether the <paramref name="type"/> is enum type and if this is the case returns its underlying type. Otherwise 
        /// returns <paramref name="type"/> after casting it to PrimitiveType.
        /// </summary>
        /// <param name="type">Type to convert to primitive type.</param>
        /// <returns>Underlying type if <paramref name="type"/> is enumeration type. Otherwise <paramref name="type"/> itself.</returns>
        /// <remarks>This method should be called only for primitive or enumeration types.</remarks>
        internal static PrimitiveType AsPrimitive(EdmType type)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(IsScalarType(type), "This method must not be called for types that are neither primitive nor enums.");

            return Helper.IsEnumType(type) ?
                GetUnderlyingEdmTypeForEnumType(type) : 
                (PrimitiveType)type;
        }

        /// <summary>
        /// Returns underlying EDM type of a given enum <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Enum type whose underlying EDM type needs to be returned. Must not be null.</param>
        /// <returns>The underlying EDM type of a given enum <paramref name="type"/>.</returns>
        internal static PrimitiveType GetUnderlyingEdmTypeForEnumType(EdmType type)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(IsEnumType(type), "This method can be called only for enums.");

            return ((EnumType)type).UnderlyingType;
        }

        internal static PrimitiveType GetSpatialNormalizedPrimitiveType(EdmType type)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(IsPrimitiveType(type), "This method can be called only for enums.");
            PrimitiveType primitiveType = (PrimitiveType)type;

            if (IsGeographicType(primitiveType) && primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Geography)
            {
                return PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Geography);
            }
            else if (IsGeometricType(primitiveType) && primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Geometry)
            {
                return PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Geometry);
            }
            else 
            {
                return primitiveType;
            }
        }

        #endregion
    }
}
