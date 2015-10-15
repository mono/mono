//---------------------------------------------------------------------
// <copyright file="EntityTypeBase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
namespace System.Data.Metadata.Edm
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Represents the Entity Type
    /// </summary>
    public abstract class EntityTypeBase : StructuralType
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of Entity Type
        /// </summary>
        /// <param name="name">name of the entity type</param>
        /// <param name="namespaceName">namespace of the entity type</param>
        /// <param name="version">version of the entity type</param>
        /// <param name="dataSpace">dataSpace in which this edmtype belongs to</param>
        /// <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
        internal EntityTypeBase(string name, string namespaceName, DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
            _keyMembers = new ReadOnlyMetadataCollection<EdmMember>(new MetadataCollection<EdmMember>());
        }
        #endregion

        #region Fields
        private readonly ReadOnlyMetadataCollection<EdmMember> _keyMembers;
        private string[] _keyMemberNames;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the list of all the key members for this entity type
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EdmMember, true)]
        public ReadOnlyMetadataCollection<EdmMember> KeyMembers
        {
            get
            {
                // Since we allow entity types with no keys, we should first check if there are 
                // keys defined on the base class. If yes, then return the keys otherwise, return
                // the keys defined on this class
                if (this.BaseType != null && ((EntityTypeBase)this.BaseType).KeyMembers.Count != 0)
                {
                    Debug.Assert(_keyMembers.Count == 0, "Since the base type have keys, current type cannot have keys defined");
                    return ((EntityTypeBase)this.BaseType).KeyMembers;
                }
                else
                {
                    return _keyMembers;
                }
            }
        }

        /// <summary>
        /// Returns the list of the member names that form the key for this entity type
        /// Perf 

        internal string[] KeyMemberNames
        {
            get
            {
                String[] keyNames = _keyMemberNames;
                if (keyNames == null)
                {
                    keyNames = new string[this.KeyMembers.Count];
                    for (int i = 0; i < keyNames.Length; i++)
                    {
                        keyNames[i] = this.KeyMembers[i].Name;
                    }
                    _keyMemberNames = keyNames;
                }
                Debug.Assert(_keyMemberNames.Length == this.KeyMembers.Count, "This list is out of sync with the key members count. This property was called before all the keymembers were added");
                return _keyMemberNames;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Returns the list of all the key members for this entity type
        /// </summary>
        /// <exception cref="System.ArgumentNullException">if member argument is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the EntityType has a base type of another EntityTypeBase. In this case KeyMembers should be added to the base type</exception>
        /// <exception cref="System.InvalidOperationException">If the EntityType instance is in ReadOnly state</exception>
        internal void AddKeyMember(EdmMember member)
        {
            EntityUtil.GenericCheckArgumentNull(member, "member");
            Util.ThrowIfReadOnly(this);
            Debug.Assert(this.BaseType == null || ((EntityTypeBase)this.BaseType).KeyMembers.Count == 0,
                "Key cannot be added if there is a basetype with keys");

            if (!Members.Contains(member))
            {
                this.AddMember(member);
            }
            _keyMembers.Source.Add(member);
        }

        /// <summary>
        /// Makes this property readonly
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                _keyMembers.Source.SetReadOnly();
                base.SetReadOnly();
            }
        }

        /// <summary>
        /// Checks for each property to be non-null and then adds it to the member collection
        /// </summary>
        /// <param name="members">members for this type</param>
        /// <param name="entityType">the membersCollection to which the members should be added</param>
        internal static void CheckAndAddMembers(IEnumerable<EdmMember> members,
                                                EntityType entityType)
        {
            foreach (EdmMember member in members)
            {
                // Check for each property to be non-null
                if (null == member)
                    throw EntityUtil.CollectionParameterElementIsNull("members");

                // Add the property to the member collection
                entityType.AddMember(member);
            }
        }


        /// <summary>
        /// Checks for each key member to be non-null 
        /// also check for it to be present in the members collection
        /// and then adds it to the KeyMembers collection.
        /// 
        /// Throw if the key member is not already in the members 
        /// collection. Cannot do much other than that as the 
        /// Key members is just an Ienumerable of the names
        /// of the members.
        /// </summary>
        /// <param name="keyMembers">the list of keys (member names) to be added for the given type</param>
        internal void CheckAndAddKeyMembers(IEnumerable<String> keyMembers)
        {
            foreach (string keyMember in keyMembers)
            {
                // Check for each keymember to be non-null
                if (null == keyMember)
                {
                    throw EntityUtil.CollectionParameterElementIsNull("keyMembers");
                }
                // Check for whether the key exists in the members collection
                EdmMember member;
                if (!Members.TryGetValue(keyMember, false, out member))
                {
                    throw EntityUtil.Argument(System.Data.Entity.Strings.InvalidKeyMember(keyMember)); //--- to do, identify the right exception to throw here
                }
                // Add the key member to the key member collection 
                AddKeyMember(member);
            }
        }

        #endregion

    }
}
