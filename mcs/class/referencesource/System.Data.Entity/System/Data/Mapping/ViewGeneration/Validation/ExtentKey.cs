//---------------------------------------------------------------------
// <copyright file="ExtentKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------


using System.Data.Common.Utils;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data.Metadata.Edm;

namespace System.Data.Mapping.ViewGeneration.Structures
{

    // This class represents the key of  constraint on values that a relation slot may have
    internal class ExtentKey : InternalBase
    {

        #region Constructors
        // effects: Creates a key object for an extent (present in each MemberPath)
        // with the fields corresponding to keyFields
        internal ExtentKey(IEnumerable<MemberPath> keyFields)
        {
            m_keyFields = new List<MemberPath>(keyFields);
        }
        #endregion

        #region Fields
        // All the key fields in an entity set
        private List<MemberPath> m_keyFields;
        #endregion

        #region Properties
        internal IEnumerable<MemberPath> KeyFields
        {
            get { return m_keyFields; }
        }
        #endregion

        #region Methods
        // effects: Determines all the keys (unique and primary for
        // entityType) for entityType and returns a key. "prefix" gives the
        // path of the extent or end of a relationship in a relationship set
        // -- prefix is prepended to the entity's key fields to get the full memberpath
        internal static List<ExtentKey> GetKeysForEntityType(MemberPath prefix, EntityType entityType)
        {
            // CHANGE_[....]_MULTIPLE_KEYS: currently there is a single key only. Need to support
            // keys inside complex types + unique keys
            ExtentKey key = GetPrimaryKeyForEntityType(prefix, entityType);

            List<ExtentKey> keys = new List<ExtentKey>();
            keys.Add(key);
            return keys;
        }

        // effects: Returns the key for entityType prefixed with prefix (for
        // its memberPath)
        internal static ExtentKey GetPrimaryKeyForEntityType(MemberPath prefix, EntityType entityType)
        {
            List<MemberPath> keyFields = new List<MemberPath>();
            foreach (EdmMember keyMember in entityType.KeyMembers)
            {
                Debug.Assert(keyMember != null, "Bogus key member in metadata");
                keyFields.Add(new MemberPath(prefix, keyMember));
            }

            // Just have one key for now
            ExtentKey key = new ExtentKey(keyFields);
            return key;
        }

        // effects: Returns a key correspnding to all the fields in different
        // ends of relationtype prefixed with "prefix"
        internal static ExtentKey GetKeyForRelationType(MemberPath prefix, AssociationType relationType)
        {
            List<MemberPath> keyFields = new List<MemberPath>();

            foreach (AssociationEndMember endMember in relationType.AssociationEndMembers)
            {
                MemberPath endPrefix = new MemberPath(prefix, endMember);
                EntityType entityType = MetadataHelper.GetEntityTypeForEnd(endMember);
                ExtentKey primaryKey = GetPrimaryKeyForEntityType(endPrefix, entityType);
                keyFields.AddRange(primaryKey.KeyFields);
            }
            ExtentKey key = new ExtentKey(keyFields);
            return key;
        }

        internal string ToUserString()
        {
            string result = StringUtil.ToCommaSeparatedStringSorted(m_keyFields);
            return result;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            StringUtil.ToCommaSeparatedStringSorted(builder, m_keyFields);
        }
        #endregion
    }
}
