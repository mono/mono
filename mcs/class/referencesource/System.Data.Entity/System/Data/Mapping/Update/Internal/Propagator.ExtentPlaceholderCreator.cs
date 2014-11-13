//---------------------------------------------------------------------
// <copyright file="Propagator.PlaceholderVisitor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Metadata.Edm;
    using System.Data.Spatial;
    using System.Diagnostics;

    internal partial class Propagator
    {
        /// <summary>
        /// Class generating default records for extents. Has a single external entry point, the 
        /// <see cref="CreatePlaceholder" /> static method.
        /// </summary>
        private class ExtentPlaceholderCreator
        {
            #region Constructors
            /// <summary>
            /// Constructs a new placeholder creator.
            /// </summary>
            /// <param name="parent">Context used to generate all elements of the placeholder.</param>
            private ExtentPlaceholderCreator(UpdateTranslator parent)
            {
                EntityUtil.CheckArgumentNull(parent, "parent");
                m_parent = parent;
            }
            #endregion

            #region Fields
            static private Dictionary<PrimitiveTypeKind, object> s_typeDefaultMap = InitializeTypeDefaultMap();
            private UpdateTranslator m_parent;
            #endregion

            #region Methods
            /// <summary>
            /// Initializes a map from primitive scalar types in the C-Space to default values
            /// used within the placeholder.
            /// </summary>
            private static Dictionary<PrimitiveTypeKind, object> InitializeTypeDefaultMap()
            {
                Dictionary<PrimitiveTypeKind, object> typeDefaultMap = new Dictionary<PrimitiveTypeKind, object>(
                    EqualityComparer<PrimitiveTypeKind>.Default);

                // Use CLR defaults for value types, arbitrary constants for reference types
                // (since these default to null)
                typeDefaultMap[PrimitiveTypeKind.Binary] = new Byte[0];
                typeDefaultMap[PrimitiveTypeKind.Boolean] = default(Boolean);
                typeDefaultMap[PrimitiveTypeKind.Byte] = default(Byte);
                typeDefaultMap[PrimitiveTypeKind.DateTime] = default(DateTime);
                typeDefaultMap[PrimitiveTypeKind.Time] = default(TimeSpan);
                typeDefaultMap[PrimitiveTypeKind.DateTimeOffset] = default(DateTimeOffset);
                typeDefaultMap[PrimitiveTypeKind.Decimal] = default(Decimal);
                typeDefaultMap[PrimitiveTypeKind.Double] = default(Double);
                typeDefaultMap[PrimitiveTypeKind.Guid] = default(Guid);
                typeDefaultMap[PrimitiveTypeKind.Int16] = default(Int16);
                typeDefaultMap[PrimitiveTypeKind.Int32] = default(Int32);
                typeDefaultMap[PrimitiveTypeKind.Int64] = default(Int64);
                typeDefaultMap[PrimitiveTypeKind.Single] = default(Single);
                typeDefaultMap[PrimitiveTypeKind.SByte] = default(SByte);
                typeDefaultMap[PrimitiveTypeKind.String] = String.Empty;

                typeDefaultMap[PrimitiveTypeKind.Geometry] = DbGeometry.FromText("POINT EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeometryPoint] = DbGeometry.FromText("POINT EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeometryLineString] = DbGeometry.FromText("LINESTRING EMPTY"); 
                typeDefaultMap[PrimitiveTypeKind.GeometryPolygon] = DbGeometry.FromText("POLYGON EMPTY"); 
                typeDefaultMap[PrimitiveTypeKind.GeometryMultiPoint] = DbGeometry.FromText("MULTIPOINT EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeometryMultiLineString] = DbGeometry.FromText("MULTILINESTRING EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeometryMultiPolygon] = DbGeometry.FromText("MULTIPOLYGON EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeometryCollection] = DbGeometry.FromText("GEOMETRYCOLLECTION EMPTY");

                typeDefaultMap[PrimitiveTypeKind.Geography] = DbGeography.FromText("POINT EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeographyPoint] = DbGeography.FromText("POINT EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeographyLineString] = DbGeography.FromText("LINESTRING EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeographyPolygon] = DbGeography.FromText("POLYGON EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeographyMultiPoint] = DbGeography.FromText("MULTIPOINT EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeographyMultiLineString] = DbGeography.FromText("MULTILINESTRING EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeographyMultiPolygon] = DbGeography.FromText("MULTIPOLYGON EMPTY");
                typeDefaultMap[PrimitiveTypeKind.GeographyCollection] = DbGeography.FromText("GEOMETRYCOLLECTION EMPTY");

#if DEBUG                
                foreach (object o in typeDefaultMap.Values)
                {
                    Debug.Assert(null != o, "DbConstantExpression instances do not support null values");
                }
#endif

                return typeDefaultMap;
            }

            /// <summary>
            /// Creates a record for an extent containing default values. Assumes the extent is either
            /// a relationship set or an entity set.
            /// </summary>
            /// <remarks>
            /// Each scalar value appearing in the record is a <see cref="DbConstantExpression" />. A placeholder is created by recursively
            /// building a record, so an entity record type will return a new record (<see cref="DbNewInstanceExpression" />)
            /// consisting of some recursively built record for each column in the type.
            /// </remarks>
            /// <param name="extent">Extent</param>
            /// <param name="parent">Command tree used to generate portions of the record</param>
            /// <returns>A default record for the </returns>
            internal static PropagatorResult CreatePlaceholder(EntitySetBase extent, UpdateTranslator parent)
            {
                EntityUtil.CheckArgumentNull(extent, "extent");

                ExtentPlaceholderCreator creator = new ExtentPlaceholderCreator(parent);

                AssociationSet associationSet = extent as AssociationSet;
                if (null != associationSet)
                {
                    return creator.CreateAssociationSetPlaceholder(associationSet);
                }

                EntitySet entitySet = extent as EntitySet;
                if (null != entitySet)
                {
                    return creator.CreateEntitySetPlaceholder(entitySet);
                }

                throw EntityUtil.NotSupported(System.Data.Entity.Strings.Update_UnsupportedExtentType(
                    extent.Name, extent.GetType().Name));
            }

            /// <summary>
            /// Specialization of <see cref="CreatePlaceholder" /> for an entity set extent.
            /// </summary>
            /// <param name="entitySet"></param>
            /// <returns></returns>
            private PropagatorResult CreateEntitySetPlaceholder(EntitySet entitySet)
            {
                EntityUtil.CheckArgumentNull(entitySet, "entitySet");
                ReadOnlyMetadataCollection<EdmProperty> members = entitySet.ElementType.Properties;
                PropagatorResult[] memberValues = new PropagatorResult[members.Count];

                for (int ordinal = 0; ordinal < members.Count; ordinal++)
                {
                    PropagatorResult memberValue = CreateMemberPlaceholder(members[ordinal]);
                    memberValues[ordinal] = memberValue;
                }

                PropagatorResult result = PropagatorResult.CreateStructuralValue(memberValues, entitySet.ElementType, false);

                return result;
            }

            /// <summary>
            /// Specialization of <see cref="CreatePlaceholder" /> for a relationship set extent.
            /// </summary>
            /// <param name="associationSet"></param>
            /// <returns></returns>
            private PropagatorResult CreateAssociationSetPlaceholder(AssociationSet associationSet)
            {
                Debug.Assert(null != associationSet, "Caller must verify parameters are not null");

                var endMetadata = associationSet.ElementType.AssociationEndMembers;
                PropagatorResult[] endReferenceValues = new PropagatorResult[endMetadata.Count];

                // Create a reference expression for each end in the relationship
                for (int endOrdinal = 0; endOrdinal < endMetadata.Count; endOrdinal++)
                {
                    var end = endMetadata[endOrdinal];
                    EntityType entityType = (EntityType)((RefType)end.TypeUsage.EdmType).ElementType;

                    // Retrieve key values for this end
                    PropagatorResult[] keyValues = new PropagatorResult[entityType.KeyMembers.Count];
                    for (int memberOrdinal = 0; memberOrdinal < entityType.KeyMembers.Count; memberOrdinal++)
                    {
                        EdmMember keyMember = entityType.KeyMembers[memberOrdinal];
                        PropagatorResult keyValue = CreateMemberPlaceholder(keyMember);
                        keyValues[memberOrdinal] = keyValue;
                    }

                    RowType endType = entityType.GetKeyRowType(m_parent.MetadataWorkspace);
                    PropagatorResult refKeys = PropagatorResult.CreateStructuralValue(keyValues, endType, false);

                    endReferenceValues[endOrdinal] = refKeys;
                }

                PropagatorResult result = PropagatorResult.CreateStructuralValue(endReferenceValues, associationSet.ElementType, false);
                return result;
            }

            /// <summary>
            /// Returns a placeholder for a specific metadata member.
            /// </summary>
            /// <param name="member">EdmMember for which to produce a placeholder.</param>
            /// <returns>Placeholder element for the given member.</returns>
            private PropagatorResult CreateMemberPlaceholder(EdmMember member)
            {
                EntityUtil.CheckArgumentNull(member, "member");

                return Visit(member);
            }

            #region Visitor implementation
            /// <summary>
            /// Given default values for children members, produces a new default expression for the requested (parent) member.
            /// </summary>
            /// <param name="node">Parent member</param>
            /// <returns>Default value for parent member</returns>
            internal PropagatorResult Visit(EdmMember node)
            {
                PropagatorResult result;
                TypeUsage nodeType = Helper.GetModelTypeUsage(node);

                if (Helper.IsScalarType(nodeType.EdmType))
                {
                    GetPropagatorResultForPrimitiveType(Helper.AsPrimitive(nodeType.EdmType), out result);
                }
                else
                {
                    // Construct a new 'complex type' (really any structural type) member.
                    StructuralType structuralType = (StructuralType)nodeType.EdmType;
                    IBaseList<EdmMember> members = TypeHelpers.GetAllStructuralMembers(structuralType);

                    PropagatorResult[] args = new PropagatorResult[members.Count];
                    for (int ordinal = 0; ordinal < members.Count; ordinal++)
                    //                    foreach (EdmMember member in members)
                    {
                        args[ordinal] = Visit(members[ordinal]);
                    }

                    result = PropagatorResult.CreateStructuralValue(args, structuralType, false);
                }

                return result;
            }

            // Find "sanctioned" default value
            private static void GetPropagatorResultForPrimitiveType(PrimitiveType primitiveType, out PropagatorResult result)
            {
                object value;
                PrimitiveTypeKind primitiveTypeKind = primitiveType.PrimitiveTypeKind;
                if (!s_typeDefaultMap.TryGetValue(primitiveTypeKind, out value))
                {
                    // If none exists, default to lowest common denominator for constants
                    value = default(byte);
                }

                // Return a new constant expression flagged as unknown since the value is only there for
                // show. (Not entirely for show, because null constraints may require a value for a record,
                // whether that record is a placeholder or not).
                result = PropagatorResult.CreateSimpleValue(PropagatorFlags.NoFlags, value);
            }

            #endregion
            #endregion
        }
    }
}
