//---------------------------------------------------------------------
// <copyright file="ClrPerspective.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Mapping;
    using System.Diagnostics;

    /// <summary>
    /// Internal helper class for query
    /// </summary>
    internal sealed class ClrPerspective : Perspective
    {
        private EntityContainer _defaultContainer;

        #region Constructors
        /// <summary>
        /// Creates a new instance of perspective class so that query can work
        /// ignorant of all spaces
        /// </summary>
        /// <param name="metadataWorkspace"></param>
        internal ClrPerspective(MetadataWorkspace metadataWorkspace)
            : base(metadataWorkspace, DataSpace.CSpace)
        {
        }
        #endregion //Constructors

        #region Methods
        /// <summary>
        /// Given a clrType attempt to return the corresponding target type from
        /// the worksapce
        /// </summary>
        /// <param name="clrType">The clr type to resolve</param>
        /// <param name="outTypeUsage">an out param for the typeUsage to be resolved to</param>
        /// <returns>true if a TypeUsage can be found for the target type</returns>
        internal bool TryGetType(Type clrType, out TypeUsage outTypeUsage)
        {
            return TryGetTypeByName(clrType.FullName, 
                                    false /*ignoreCase*/, 
                                    out outTypeUsage);
        }

        /// <summary>
        /// Given the type in the target space and the member name in the source space,
        /// get the corresponding member in the target space
        /// For e.g.  consider a Conceptual Type Foo with a member bar and a CLR type 
        /// XFoo with a member YBar. If one has a reference to Foo one can
        /// invoke GetMember(Foo,"YBar") to retrieve the member metadata for bar
        /// </summary>
        /// <param name="type">The type in the target perspective</param>
        /// <param name="memberName">the name of the member in the source perspective</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="outMember">returns the edmMember if a match is found</param>
        /// <returns>true if a match is found, otherwise false</returns>
        internal override bool TryGetMember(StructuralType type, String memberName, bool ignoreCase, out EdmMember outMember)
        {
            outMember = null;
            Map map = null;

            if (this.MetadataWorkspace.TryGetMap(type, DataSpace.OCSpace, out map))
            {
                ObjectTypeMapping objectTypeMap = map as ObjectTypeMapping;

                if (objectTypeMap!=null)
                {
                    ObjectMemberMapping objPropertyMapping = objectTypeMap.GetMemberMapForClrMember(memberName, ignoreCase);
                    if (null != objPropertyMapping)
                    {
                        outMember = objPropertyMapping.EdmMember;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Look up a type in the target data space based upon the fullName
        /// </summary>
        /// <param name="fullName">fullName</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="typeUsage">The type usage object to return</param>
        /// <returns>True if the retrieval succeeded</returns>
        internal override bool TryGetTypeByName(string fullName, bool ignoreCase, out TypeUsage typeUsage)
        {
            typeUsage = null;
            Map map = null;

            // From ClrPerspective, we should not allow anything from SSpace. So make sure that the CSpace type does not
            // have the Target attribute
            if (this.MetadataWorkspace.TryGetMap(fullName, DataSpace.OSpace, ignoreCase, DataSpace.OCSpace, out map))
            {
                // Check if it's primitive type, if so, then use the MetadataWorkspace to get the mapped primitive type
                if (map.EdmItem.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
                {
                    // Reassign the variable with the provider primitive type, then create the type usage
                    PrimitiveType primitiveType = this.MetadataWorkspace.GetMappedPrimitiveType(((PrimitiveType)map.EdmItem).PrimitiveTypeKind, DataSpace.CSpace);
                    if (primitiveType != null)
                    {
                        typeUsage = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(primitiveType.PrimitiveTypeKind);
                    }
                }
                else
                {
                    Debug.Assert(((GlobalItem)map.EdmItem).DataSpace == DataSpace.CSpace);
                    typeUsage = GetMappedTypeUsage(map);
                }
            }

            return (null != typeUsage);
        }

        /// <summary>
        /// get the default container
        /// </summary>
        /// <returns>The default container</returns>
        internal override EntityContainer GetDefaultContainer()
        {
            return _defaultContainer;
        }

        internal void SetDefaultContainer(string defaultContainerName)
        {
            EntityContainer container = null;
            if (!String.IsNullOrEmpty(defaultContainerName))
            {
                if (!MetadataWorkspace.TryGetEntityContainer(defaultContainerName, DataSpace.CSpace, out container))
                {
                    throw EntityUtil.InvalidDefaultContainerName("defaultContainerName", defaultContainerName);
                }
            }
            _defaultContainer = container;
        }

        /// <summary>
        /// Given a map, dereference the EdmItem, ensure that it is
        /// an EdmType and return a TypeUsage for the type, otherwise
        /// return null.
        /// </summary>
        /// <param name="map">The OC map to use to get the EdmType</param>
        /// <returns>A TypeUsage for the mapped EdmType or null if no EdmType was mapped</returns>
        private static TypeUsage GetMappedTypeUsage(Map map)
        {
            TypeUsage typeUsage = null;
            if (null != map)
            {
                MetadataItem item = map.EdmItem;
                EdmType edmItem = item as EdmType;
                if (null != item && edmItem!=null)
                {
                    typeUsage = TypeUsage.Create(edmItem);
                }
            }
            return typeUsage;
        }
        #endregion
    }
}
