//---------------------------------------------------------------------
// <copyright file="EntityType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using System.Globalization;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// concrete Representation the Entity Type
    /// </summary>
    public class EntityType : EntityTypeBase
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of Entity Type
        /// </summary>
        /// <param name="name">name of the entity type</param>
        /// <param name="namespaceName">namespace of the entity type</param>
        /// <param name="version">version of the entity type</param>
        /// <param name="dataSpace">dataspace in which the EntityType belongs to</param>
        /// <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
        internal EntityType(string name, string namespaceName, DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
        }

        /// <param name="name">name of the entity type</param>
        /// <param name="namespaceName">namespace of the entity type</param>
        /// <param name="version">version of the entity type</param>
        /// <param name="dataSpace">dataspace in which the EntityType belongs to</param>
        /// <param name="members">members of the entity type [property and navigational property]</param>
        /// <param name="keyMemberNames">key members for the type</param>
        /// <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
        internal EntityType(string name,
                          string namespaceName,
                          DataSpace dataSpace,
                          IEnumerable<string> keyMemberNames,
                          IEnumerable<EdmMember> members)
            : base(name, namespaceName, dataSpace)
        {
            //--- first add the properties 
            if (null != members)
            {
                CheckAndAddMembers(members, this);
            }
            //--- second add the key members
            if (null != keyMemberNames)
            {
                //Validation should make sure that base type of this type does not have keymembers when this type has keymembers. 
                CheckAndAddKeyMembers(keyMemberNames);
            }
        }


        #endregion

        #region Fields
        /// <summary>cached dynamic method to construct a CLR instance</summary>
        private RefType _referenceType;
        private ReadOnlyMetadataCollection<EdmProperty> _properties;
        private RowType _keyRow;
        private Dictionary<EdmMember, string> _memberSql;
        private object _memberSqlLock = new object();
        #endregion

        #region Methods
        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.EntityType; } }

        /// <summary>
        /// Validates a EdmMember object to determine if it can be added to this type's 
        /// Members collection. If this method returns without throwing, it is assumed
        /// the member is valid. 
        /// </summary>
        /// <param name="member">The member to validate</param>
        /// <exception cref="System.ArgumentException">Thrown if the member is not a EdmProperty</exception>
        internal override void ValidateMemberForAdd(EdmMember member)
        {
            Debug.Assert(Helper.IsEdmProperty(member) || Helper.IsNavigationProperty(member),
                "Only members of type Property may be added to Entity types.");
        }

        /// <summary>
        /// Get SQL description of a member of this entity type.
        /// Requires: member must belong to this type
        /// </summary>
        /// <param name="member">Member for which to retrieve SQL</param>
        /// <param name="sql">Outputs SQL describing this member</param>
        /// <returns>Whether sql is cached for this member</returns>
        internal bool TryGetMemberSql(EdmMember member, out string sql)
        {
            Debug.Assert(Members.Contains(member));
            sql = null;
            return null != _memberSql && _memberSql.TryGetValue(member, out sql);
        }

        /// <summary>
        /// Sets SQL describing a member of this entity type.
        /// Requires: member must belong to this type
        /// </summary>
        /// <param name="member">Member for which to set SQL</param>
        /// <param name="sql">SQL describing this member</param>
        internal void SetMemberSql(EdmMember member, string sql)
        {
            Debug.Assert(Members.Contains(member));

            // initialize dictionary on first use
            lock (_memberSqlLock)
            {
                if (null == _memberSql)
                {
                    _memberSql = new Dictionary<EdmMember, string>();
                }

                _memberSql[member] = sql;
            }
        }
        #endregion

        #region Properties

        /// <summary>
        /// Returns the list of Navigation Properties for this entity type
        /// </summary>
        public ReadOnlyMetadataCollection<NavigationProperty> NavigationProperties
        {
            get
            {
                return new FilteredReadOnlyMetadataCollection<NavigationProperty, EdmMember>(
                    ((ReadOnlyMetadataCollection<EdmMember>)this.Members), Helper.IsNavigationProperty);
            }
        }

        /// <summary>
        /// Returns just the properties from the collection
        /// of members on this type
        /// </summary>
        public ReadOnlyMetadataCollection<EdmProperty> Properties
        {
            get
            {
                Debug.Assert(IsReadOnly, "this is a wrapper around this.Members, don't call it during metadata loading, only call it after the metadata is set to readonly");
                if (null == _properties)
                {
                    Interlocked.CompareExchange(ref _properties,
                        new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(
                            this.Members, Helper.IsEdmProperty), null);
                }
                return _properties;
            }
        }

        #endregion // Properties

        /// <summary>
        /// Returns the Reference type pointing to this entity type
        /// </summary>
        /// <returns></returns>
        public RefType GetReferenceType()
        {
            if (_referenceType == null)
            {
                Interlocked.CompareExchange<RefType>(ref _referenceType, new RefType(this), null);
            }
            return _referenceType;
        }

        internal RowType GetKeyRowType(MetadataWorkspace metadataWorkspace)
        {
            if (_keyRow == null)
            {
                List<EdmProperty> keyProperties = new List<EdmProperty>(KeyMembers.Count);
                foreach (EdmMember keyMember in KeyMembers)
                {
                    keyProperties.Add(new EdmProperty(keyMember.Name, Helper.GetModelTypeUsage(keyMember)));
                }
                Interlocked.CompareExchange<RowType>(ref _keyRow, new RowType(keyProperties), null);
            }
            return _keyRow;
        }

        /// <summary>
        /// Attempts to get the property name for the ----oication between the two given end
        /// names.  Note that this property may not exist if a navigation property is defined
        /// in one direction but not in the other.
        /// </summary>
        /// <param name="relationshipType">the relationship for which a nav property is required</param>
        /// <param name="fromName">the 'from' end of the association</param>
        /// <param name="toName">the 'to' end of the association</param>
        /// <param name="navigationProperty">the property name, or null if none was found</param>
        /// <returns>true if a property was found, false otherwise</returns>
        internal bool TryGetNavigationProperty(string relationshipType, string fromName, string toName, out NavigationProperty navigationProperty)
        {
            // This is a linear search but it's probably okay because the number of entries
            // is generally small and this method is only called to generate code during lighweight
            // code gen.
            foreach (NavigationProperty navProperty in NavigationProperties)
            {
                if (navProperty.RelationshipType.FullName == relationshipType &&
                    navProperty.FromEndMember.Name == fromName &&
                    navProperty.ToEndMember.Name == toName)
                {
                    navigationProperty = navProperty;
                    return true;
                }
            }
            navigationProperty = null;
            return false;
        }
    }

    internal sealed class ClrEntityType : EntityType
    {
        /// <summary>cached CLR type handle, allowing the Type reference to be GC'd</summary>
        private readonly System.RuntimeTypeHandle _type;

        /// <summary>cached dynamic method to construct a CLR instance</summary>
        private Delegate _constructor;

        private readonly string _cspaceTypeName;

        private readonly string _cspaceNamespaceName;

        private string _hash;

        /// <summary>
        /// Initializes a new instance of Complex Type with properties from the type.
        /// </summary>
        /// <param name="type">The CLR type to construct from</param>
        internal ClrEntityType(Type type, string cspaceNamespaceName, string cspaceTypeName)
            : base(EntityUtil.GenericCheckArgumentNull(type, "type").Name, type.Namespace ?? string.Empty,
            DataSpace.OSpace)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(cspaceNamespaceName) &&
                !String.IsNullOrEmpty(cspaceTypeName), "Mapping information must never be null");

            _type = type.TypeHandle;
            _cspaceNamespaceName = cspaceNamespaceName;
            _cspaceTypeName = cspaceNamespaceName + "." + cspaceTypeName;
            this.Abstract = type.IsAbstract;
        }

        /// <summary>cached dynamic method to construct a CLR instance</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Delegate Constructor
        {
            get { return _constructor; }
            set
            {
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _constructor, value, null);
            }
        }

        /// <summary>
        /// </summary>
        internal override System.Type ClrType
        {
            get { return Type.GetTypeFromHandle(_type); }
        }

        internal string CSpaceTypeName { get { return _cspaceTypeName; } }

        internal string CSpaceNamespaceName { get { return _cspaceNamespaceName; } }

        /// <summary>
        /// Gets a collision resistent (SHA256) hash of the information used to build
        /// a proxy for this type.  This hash is very, very unlikely to be the same for two
        /// proxies generated from the same CLR type but with different metadata, and is
        /// guarenteed to be the same for proxies generated from the same metadata.  This
        /// means that when EntityType comparison fails because of metadata eviction,
        /// the hash can be used to determine whether or not a proxy is of the correct type.
        /// </summary>
        internal string HashedDescription
        {
            get
            {
                if (_hash == null)
                {
                    Interlocked.CompareExchange(ref _hash, BuildEntityTypeHash(), null);
                }
                return _hash;
            }
        }

        /// <summary>
        /// Creates an SHA256 hash of a description of all the metadata relevant to the creation of a proxy type
        /// for this entity type.
        /// </summary>
        private string BuildEntityTypeHash()
        {
            var hash = System.Data.Common.Utils.MetadataHelper.CreateSHA256HashAlgorithm()
                .ComputeHash(Encoding.ASCII.GetBytes(BuildEntityTypeDescription()));

            // convert num bytes to num hex digits
            var builder = new StringBuilder(hash.Length * 2);
            foreach (byte bite in hash)
            {
                builder.Append(bite.ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates a description of all the metadata relevant to the creation of a proxy type
        /// for this entity type.
        /// </summary>
        private string BuildEntityTypeDescription()
        {
            var builder = new StringBuilder(512);
            Debug.Assert(ClrType != null, "Expecting non-null CLRType of o-space EntityType.");
            builder.Append("CLR:").Append(ClrType.FullName);
            builder.Append("Conceptual:").Append(CSpaceTypeName);

            var navProps = new SortedSet<string>();
            foreach (var navProperty in NavigationProperties)
            {
                navProps.Add(navProperty.Name + "*" +
                             navProperty.FromEndMember.Name + "*" +
                             navProperty.FromEndMember.RelationshipMultiplicity + "*" +
                             navProperty.ToEndMember.Name + "*" +
                             navProperty.ToEndMember.RelationshipMultiplicity + "*");
            }
            builder.Append("NavProps:");
            foreach (var navProp in navProps)
            {
                builder.Append(navProp);
            }

            var keys = new SortedSet<string>();
            foreach (var member in KeyMemberNames)
            {
                keys.Add(member);
            }
            builder.Append("Keys:");
            foreach (var key in keys)
            {
                builder.Append(key + "*");
            }

            var scalars = new SortedSet<string>();
            foreach (var member in Members)
            {
                if (!keys.Contains(member.Name))
                {
                    scalars.Add(member.Name + "*");
                }
            }
            builder.Append("Scalars:");
            foreach (var scalar in scalars)
            {
                builder.Append(scalar + "*");
            }

            return builder.ToString();
        }
    }
}
