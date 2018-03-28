//---------------------------------------------------------------------
// <copyright file="EntityKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Runtime.Serialization;
using Edm = System.Data.Metadata.Edm;

namespace System.Data
{
    /// <summary>
    /// An identifier for an entity.
    /// </summary>
    [DebuggerDisplay("{ConcatKeyValue()}")]
    [Serializable]
    [DataContract(IsReference = true)]
    public sealed class EntityKey : IEquatable<EntityKey>
    {
        // The implementation of EntityKey is optimized for the following common cases:
        //      1) Keys constructed internally rather by the user - in particular, keys 
        //         created by the bridge on the round-trip from query.
        //      2) Single-valued (as opposed to composite) keys.
        // We accomplish this by maintaining two variables, at most one of which is non-null.
        // The first is of type object and in the case of a singleton key, is set to the
        // single key value.  The second is an object array and in the case of 
        // a composite key, is set to the list of key values.  If both variables are null,
        // the EntityKey is a temporary key.  Note that the key field names
        // are not stored - for composite keys, the values are stored in the order in which
        // metadata reports the corresponding key members.

        // The following 5 fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        private string _entitySetName;
        private string _entityContainerName;
        private object _singletonKeyValue;      // non-null for singleton keys
        private object[] _compositeKeyValues;   // non-null for composite keys
        private string[] _keyNames;             // key names that correspond to the key values
        private bool _isLocked;                 // determines if this key is lock from writing

        // Determines whether the key includes a byte[].
        // Not serialized for backwards compatibility.
        // This value is computed along with the _hashCode, which is also not serialized.
        [NonSerialized]
        private bool _containsByteArray;

        [NonSerialized]
        private EntityKeyMember[] _deserializedMembers;

        // The hash code is not serialized since it can be computed differently on the deserialized system.
        [NonSerialized]
        private int _hashCode;                  // computed as needed


        // Names for constant EntityKeys
        private const string s_NoEntitySetKey = "NoEntitySetKey.NoEntitySetKey";
        private const string s_EntityNotValidKey = "EntityNotValidKey.EntityNotValidKey";

        /// <summary>
        /// A singleton EntityKey by which a read-only entity is identified.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]     // Justification: these are internal so they cannot be modified publically
        public static readonly EntityKey NoEntitySetKey = new EntityKey(s_NoEntitySetKey);

        /// <summary>
        /// A singleton EntityKey identifying an entity resulted from a failed TREAT.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]     // Justification: these are internal so they cannot be modified publically
        public static readonly EntityKey EntityNotValidKey = new EntityKey(s_EntityNotValidKey);

        /// <summary>
        /// A dictionary of names so that singleton instances of names can be used
        /// </summary>
        private static Dictionary<string, string> _nameLookup = new Dictionary<string, string>();

        #region Public Constructors

        /// <summary>
        /// Constructs an empty EntityKey. For use during XmlSerialization.
        /// </summary>
        public EntityKey()
        {
            _isLocked = false;
        }

        /// <summary>
        /// Constructs an EntityKey with the given key values.
        /// </summary>
        /// <param name="qualifiedEntitySetName">The EntitySet name, qualified by the EntityContainer name, of the entity</param>
        /// <param name="entityKeyValues">The key-value pairs that identify the entity</param>
        public EntityKey(string qualifiedEntitySetName, IEnumerable<KeyValuePair<string, object>> entityKeyValues)
        {
            GetEntitySetName(qualifiedEntitySetName, out _entitySetName, out _entityContainerName);
            CheckKeyValues(entityKeyValues, out _keyNames, out _singletonKeyValue, out _compositeKeyValues);
            AssertCorrectState(null, false);
            _isLocked = true;
        }

        /// <summary>
        /// Constructs an EntityKey with the given key values.
        /// </summary>
        /// <param name="qualifiedEntitySetName">The EntitySet name, qualified by the EntityContainer name, of the entity</param>
        /// <param name="entityKeyValues">The key-value pairs that identify the entity</param>
        public EntityKey(string qualifiedEntitySetName, IEnumerable<EntityKeyMember> entityKeyValues)
        {
            GetEntitySetName(qualifiedEntitySetName, out _entitySetName, out _entityContainerName);
            EntityUtil.CheckArgumentNull(entityKeyValues, "entityKeyValues");
            CheckKeyValues(new KeyValueReader(entityKeyValues), out _keyNames, out _singletonKeyValue, out _compositeKeyValues);
            AssertCorrectState(null, false);
            _isLocked = true;
        }

        /// <summary>
        /// Constructs an EntityKey with the given single key name and value.
        /// </summary>
        /// <param name="qualifiedEntitySetName">The EntitySet name, qualified by the EntityContainer name, of the entity</param>
        /// <param name="keyName">The key name that identifies the entity</param>
        /// <param name="keyValue">The key value that identifies the entity</param>
        public EntityKey(string qualifiedEntitySetName, string keyName, object keyValue)
        {
            GetEntitySetName(qualifiedEntitySetName, out _entitySetName, out _entityContainerName);
            EntityUtil.CheckStringArgument(keyName, "keyName");
            EntityUtil.CheckArgumentNull(keyValue, "keyValue");

            _keyNames = new string[1];
            ValidateName(keyName);
            _keyNames[0] = keyName;
            _singletonKeyValue = keyValue;

            AssertCorrectState(null, false);
            _isLocked = true;
        }

        #endregion

        #region Internal Constructors

        /// <summary>
        /// Constructs an EntityKey from an IExtendedDataRecord representing the entity.
        /// </summary>
        /// <param name="entitySet">EntitySet of the entity</param>
        /// <param name="record">an IExtendedDataRecord that represents the entity</param>
        internal EntityKey(EntitySet entitySet, IExtendedDataRecord record)
        {
            Debug.Assert(entitySet != null, "entitySet is null");
            Debug.Assert(entitySet.Name != null, "entitySet.Name is null");
            Debug.Assert(entitySet.EntityContainer != null, "entitySet.EntityContainer is null");
            Debug.Assert(entitySet.EntityContainer.Name != null, "entitySet.EntityContainer.Name is null");
            Debug.Assert(record != null, "record is null");
            
            _entitySetName = entitySet.Name;
            _entityContainerName = entitySet.EntityContainer.Name;

            GetKeyValues(entitySet, record, out _keyNames, out _singletonKeyValue, out _compositeKeyValues);
            AssertCorrectState(entitySet, false);
            _isLocked = true;
        }

        /// <summary>
        /// Constructs an EntityKey from an IExtendedDataRecord representing the entity.
        /// </summary>
        /// <param name="entitySet">EntitySet of the entity</param>
        /// <param name="record">an IExtendedDataRecord that represents the entity</param>
        internal EntityKey(string qualifiedEntitySetName)
        {
            GetEntitySetName(qualifiedEntitySetName, out _entitySetName, out _entityContainerName);
            _isLocked = true;
        }

        /// <summary>
        /// Constructs a temporary EntityKey with the given EntitySet.
        /// Temporary keys do not store key field names
        /// </summary>
        /// <param name="entitySet">EntitySet of the entity</param>
        internal EntityKey(EntitySetBase entitySet)
        {
            EntityUtil.CheckArgumentNull(entitySet, "entitySet");
            Debug.Assert(entitySet.EntityContainer != null, "EntitySet.EntityContainer cannot be null.");

            _entitySetName = entitySet.Name;
            _entityContainerName = entitySet.EntityContainer.Name;

            AssertCorrectState(entitySet, true);
            _isLocked = true;
        }

        /// <summary>
        /// Constructor optimized for a singleton key.
        /// SQLBUDT 478655: Performance optimization: Does no integrity checking on the key value.
        /// SQLBUDT 523554: Performance optimization: Does no validate type of key members.
        /// </summary>
        /// <param name="entitySet">EntitySet of the entity</param>
        /// <param name="singletonKeyValue">The single value that composes the entity's key, assumed to contain the correct type.</param>
        internal EntityKey(EntitySetBase entitySet, object singletonKeyValue)
        {
            Debug.Assert(entitySet != null, "EntitySet cannot be null.");
            Debug.Assert(entitySet.EntityContainer != null, "EntitySet.EntityContainer cannot be null.");
            Debug.Assert(singletonKeyValue != null, "Singleton key value cannot be null.");
            _singletonKeyValue = singletonKeyValue;
            _entitySetName = entitySet.Name;
            _entityContainerName = entitySet.EntityContainer.Name;
            _keyNames = entitySet.ElementType.KeyMemberNames; // using EntitySetBase avoids an (EntityType) cast that EntitySet encoure
            AssertCorrectState(entitySet, false);
            _isLocked = true;
        }

        /// <summary>
        /// Constructor optimized for a composite key.
        /// SQLBUDT 478655: Performance optimization: Does no integrity checking on the key values.
        /// SQLBUDT 523554: Performance optimization: Does no validate type of key members.
        /// </summary>
        /// <param name="entitySet">EntitySet of the entity</param>
        /// <param name="compositeKeyValues">A list of the values (at least 2) that compose the entity's key, assumed to contain correct types.</param>
        internal EntityKey(EntitySetBase entitySet, object[] compositeKeyValues)
        {
            Debug.Assert(entitySet != null, "EntitySet cannot be null.");
            Debug.Assert(entitySet.EntityContainer != null, "EntitySet.EntityContainer cannot be null.");
            Debug.Assert(compositeKeyValues != null, "Composite key values cannot be null.");
            _compositeKeyValues = compositeKeyValues;
            _entitySetName = entitySet.Name;
            _entityContainerName = entitySet.EntityContainer.Name;
            _keyNames = entitySet.ElementType.KeyMemberNames; // using EntitySetBase avoids an (EntityType) cast that EntitySet encoure
            AssertCorrectState(entitySet, false);
            _isLocked = true;
        }

        #endregion

        /// <summary>
        /// Gets the EntitySet name identifying the entity set that contains the entity.
        /// </summary>
        [DataMember]
        public string EntitySetName
        {
            get
            {
                return _entitySetName;
            }
            set
            {
                ValidateWritable(_entitySetName);
                lock (_nameLookup)
                {
                    _entitySetName = EntityKey.LookupSingletonName(value);
                }
            }
        }

        /// <summary>
        /// Gets the EntityContainer name identifying the entity container that contains the entity.
        /// </summary>
        [DataMember]
        public string EntityContainerName
        {
            get
            {
                return _entityContainerName;
            }
            set
            {
                ValidateWritable(_entityContainerName);
                lock (_nameLookup)
                {
                    _entityContainerName = EntityKey.LookupSingletonName(value);
                }
            }
        }


        /// <summary>
        /// Gets the key values that identify the entity.
        /// </summary>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Required for this feature")]
        public EntityKeyMember[] EntityKeyValues
        {
            get
            {
                if (!IsTemporary)
                {
                    EntityKeyMember[] keyValues;
                    if (_singletonKeyValue != null)
                    {
                        keyValues = new EntityKeyMember[] { 
                                new EntityKeyMember(_keyNames[0], _singletonKeyValue) };
                    }
                    else
                    {
                        keyValues = new EntityKeyMember[_compositeKeyValues.Length];
                        for (int i = 0; i < _compositeKeyValues.Length; ++i)
                        {
                            keyValues[i] = new EntityKeyMember(_keyNames[i], _compositeKeyValues[i]);
                        }
                    }
                    return keyValues;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                ValidateWritable(_keyNames);
                if (value != null)
                {
                    if (!CheckKeyValues(new KeyValueReader(value), true, true, out _keyNames, out _singletonKeyValue, out _compositeKeyValues))
                    {
                        // If we did not retrieve values from the setter (i.e. encoded settings), we need to keep track of the 
                        // array instance because the array members will be set next.
                        _deserializedMembers = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this key is a temporary key.
        /// </summary>
        public bool IsTemporary
        {
            get
            {
                return (SingletonKeyValue == null) && (CompositeKeyValues == null);
            }
        }

        private object SingletonKeyValue
        {
            get
            {
                if (RequiresDeserialization)
                {
                    DeserializeMembers();
                }
                return _singletonKeyValue;
            }
        }

        private object[] CompositeKeyValues
        {
            get
            {
                if (RequiresDeserialization)
                {
                    DeserializeMembers();
                }
                return _compositeKeyValues;
            }
        }

        /// <summary>
        /// Gets the entity set for this entity key from the given metadata workspace, by
        /// entity container name and entity set name.
        /// </summary>
        /// <param name="metadataWorkspace">workspace in which to look up the entity set</param>
        /// <returns>the entity set from the given workspace for this entity key</returns>
        /// <exception cref="ArgumentException">the entity set could not be located in the workspace</exception>
        public EntitySet GetEntitySet(MetadataWorkspace metadataWorkspace)
        {
            EntityUtil.CheckArgumentNull(metadataWorkspace, "metadataWorkspace");
            if (String.IsNullOrEmpty(_entityContainerName) || String.IsNullOrEmpty(_entitySetName))
            {
                throw EntityUtil.MissingQualifiedEntitySetName();
            }

            // GetEntityContainer will throw if it cannot find the container

            // SQLBUDT 479443:  If this entity key was initially created using an entity set 
            // from a different workspace, look up the entity set in the new workspace.
            // Metadata will throw an ArgumentException if the entity set could not be found.

            return metadataWorkspace
                .GetEntityContainer(_entityContainerName, DataSpace.CSpace)
                .GetEntitySetByName(_entitySetName, false);
        }

        #region Equality/Hashing

        /// <summary>
        /// Compares this instance to a given key by their values.
        /// </summary>
        /// <param name="obj">the key to compare against this instance</param>
        /// <returns>true if this instance is equal to the given key, and false otherwise</returns>
        public override bool Equals(object obj)
        {
            return InternalEquals(this, obj as EntityKey, compareEntitySets: true);
        }

        /// <summary>
        /// Compares this instance to a given key by their values.
        /// </summary>
        /// <param name="other">the key to compare against this instance</param>
        /// <returns>true if this instance is equal to the given key, and false otherwise</returns>
        public bool Equals(EntityKey other)
        {
            return InternalEquals(this, other, compareEntitySets: true);
        }

        /// <summary>
        /// Returns a value-based hash code, to allow EntityKey to be used in hash tables.
        /// </summary>
        /// <returns>the hash value of this EntityKey</returns>
        public override int GetHashCode()
        {
            int hashCode = _hashCode;
            if (0 == hashCode)
            {
                _containsByteArray = false;

                if (RequiresDeserialization)
                {
                    DeserializeMembers();
                }

                if (_entitySetName != null)
                {
                    hashCode = _entitySetName.GetHashCode();
                }
                if (_entityContainerName != null)
                {
                    hashCode ^= _entityContainerName.GetHashCode();
                }

                // If the key is not temporary, determine a hash code based on the value(s) within the key.
                if (null != _singletonKeyValue)
                {
                    hashCode = AddHashValue(hashCode, _singletonKeyValue);
                }
                else if (null != _compositeKeyValues)
                {
                    for (int i = 0, n = _compositeKeyValues.Length; i < n; i++)
                    {
                        hashCode = AddHashValue(hashCode, _compositeKeyValues[i]);
                    }
                }
                else
                {
                    // If the key is temporary, use default hash code
                    hashCode = base.GetHashCode();
                }

                // cache the hash code if we are a locked or fully specified EntityKey
                if (_isLocked || (!String.IsNullOrEmpty(_entitySetName) &&
                                  !String.IsNullOrEmpty(_entityContainerName) &&
                                  (_singletonKeyValue != null || _compositeKeyValues != null)))
                {
                    _hashCode = hashCode;
                }
            }
            return hashCode;
        }

        private int AddHashValue(int hashCode, object keyValue)
        {
            byte[] byteArrayValue = keyValue as byte[];
            if (null != byteArrayValue)
            {
                hashCode ^= ByValueEqualityComparer.ComputeBinaryHashCode(byteArrayValue);
                _containsByteArray = true;
                return hashCode;
            }
            else
            {
                return hashCode ^ keyValue.GetHashCode();
            }
        }

        /// <summary>
        /// Compares two keys by their values.
        /// </summary>
        /// <param name="key1">a key to compare</param>
        /// <param name="key2">a key to compare</param>
        /// <returns>true if the two keys are equal, false otherwise</returns>
        public static bool operator ==(EntityKey key1, EntityKey key2)
        {
#if DEBUG
            if (((object)NoEntitySetKey == (object)key1) || ((object)EntityNotValidKey == (object)key1) ||
                ((object)NoEntitySetKey == (object)key2) || ((object)EntityNotValidKey == (object)key1)
                // || (null==(object)key1) || (null==(object)key2)) //To check for internal use of null==key
                )
            {
                Debug.Assert(typeof(EntityKey).Assembly != System.Reflection.Assembly.GetCallingAssembly(), "When comparing an EntityKey to one of the predefined types (EntityKey.NoEntitySetKey or EntityKey.EntityNotValidKey), use Object.ReferenceEquals()");
            }
#endif
            return InternalEquals(key1, key2, compareEntitySets: true);
        }

        /// <summary>
        /// Compares two keys by their values.
        /// </summary>
        /// <param name="key1">a key to compare</param>
        /// <param name="key2">a key to compare</param>
        /// <returns>true if the two keys are not equal, false otherwise</returns>
        public static bool operator !=(EntityKey key1, EntityKey key2)
        {
#if DEBUG
            if (((object)NoEntitySetKey == (object)key1) || ((object)EntityNotValidKey == (object)key1) ||
                ((object)NoEntitySetKey == (object)key2) || ((object)EntityNotValidKey == (object)key1))
            // || (null==(object)key1) || (null==(object)key2)) //To check for internal use of null==key
            {
                Debug.Assert(typeof(EntityKey).Assembly != System.Reflection.Assembly.GetCallingAssembly(), "When comparing an EntityKey to one of the predefined types (EntityKey.NoEntitySetKey or EntityKey.EntityNotValidKey), use Object.ReferenceEquals()");
            }
#endif
            return !InternalEquals(key1, key2, compareEntitySets: true);
        }

        /// <summary>
        /// Internal function to compare two keys by their values.
        /// </summary>
        /// <param name="key1">a key to compare</param>
        /// <param name="key2">a key to compare</param>
        /// <param name="compareEntitySets">Entity sets are not significant for conceptual null keys</param>
        /// <returns>true if the two keys are equal, false otherwise</returns>
        internal static bool InternalEquals(EntityKey key1, EntityKey key2, bool compareEntitySets)
        {
            // If both are null or refer to the same object, they're equal.
            if (object.ReferenceEquals(key1, key2))
            {
                return true;
            }

            // If exactly one is null (avoid calling EntityKey == operator overload), they're not equal.
            if (object.ReferenceEquals(key1, null) || object.ReferenceEquals(key2, null))
            {
                return false;
            }

            // If the hash codes differ, the keys are not equal.  Note that 
            // a key's hash code is cached after being computed for the first time, 
            // so this check will only incur the cost of computing a hash code 
            // at most once for a given key.

            // The primary caller is Dictionary<EntityKey,ObjectStateEntry>
            // at which point Equals is only called after HashCode was determined to be equal
            if ((key1.GetHashCode() != key2.GetHashCode() && compareEntitySets) ||
                key1._containsByteArray != key2._containsByteArray)
            {
                return false;
            }

            if (null != key1._singletonKeyValue)
            {
                if (key1._containsByteArray)
                {
                    // Compare the single value (if the second is null, false should be returned)
                    if (null == key2._singletonKeyValue)
                    {
                        return false;
                    }

                    // they are both byte[] because they have the same _containsByteArray value of true, and only a single value
                    if (!ByValueEqualityComparer.CompareBinaryValues((byte[])key1._singletonKeyValue, (byte[])key2._singletonKeyValue))
                    {
                        return false;
                    }
                }
                else
                {
                    // not a byte array
                    if (!key1._singletonKeyValue.Equals(key2._singletonKeyValue))
                    {
                        return false;
                    }
                }

                // Check key names
                if (!String.Equals(key1._keyNames[0], key2._keyNames[0]))
                {
                    return false;
                }
            }
            else
            {
                // If either key is temporary, they're not equal.  This is because
                // temporary keys are compared by CLR reference, and we've already
                // checked reference equality.
                // If the first key is a composite key and the second one isn't, they're not equal.
                if (null != key1._compositeKeyValues && null != key2._compositeKeyValues && key1._compositeKeyValues.Length == key2._compositeKeyValues.Length)
                {
                    if (key1._containsByteArray)
                    {
                        if (!CompositeValuesWithBinaryEqual(key1, key2))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!CompositeValuesEqual(key1, key2))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            if (compareEntitySets)
            {
                // Check metadata.
                if (!String.Equals(key1._entitySetName, key2._entitySetName) ||
                    !String.Equals(key1._entityContainerName, key2._entityContainerName))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool CompositeValuesWithBinaryEqual(EntityKey key1, EntityKey key2)
        {
            for (int i = 0; i < key1._compositeKeyValues.Length; ++i)
            {
                if (key1._keyNames[i].Equals(key2._keyNames[i]))
                {
                    if (!ByValueEqualityComparer.Default.Equals(key1._compositeKeyValues[i], key2._compositeKeyValues[i]))
                    {
                        return false;
                    }
                }
                // Key names might not be in the same order so try a slower approach that matches
                // key names between the keys.
                else if (!ValuesWithBinaryEqual(key1._keyNames[i], key1._compositeKeyValues[i], key2))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ValuesWithBinaryEqual(string keyName, object keyValue, EntityKey key2)
        {
            for (int i = 0; i < key2._keyNames.Length; i++)
            {
                if (String.Equals(keyName, key2._keyNames[i]))
                {
                    return ByValueEqualityComparer.Default.Equals(keyValue, key2._compositeKeyValues[i]);
                }
            }
            return false;
        }

        private static bool CompositeValuesEqual(EntityKey key1, EntityKey key2)
        {
            for (int i = 0; i < key1._compositeKeyValues.Length; ++i)
            {
                if (key1._keyNames[i].Equals(key2._keyNames[i]))
                {
                    if (!Object.Equals(key1._compositeKeyValues[i], key2._compositeKeyValues[i]))
                    {
                        return false;
                    }
                }
                // Key names might not be in the same order so try a slower approach that matches
                // key names between the keys.
                else if (!ValuesEqual(key1._keyNames[i], key1._compositeKeyValues[i], key2))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ValuesEqual(string keyName, object keyValue, EntityKey key2)
        {
            for (int i = 0; i < key2._keyNames.Length; i++)
            {
                if (String.Equals(keyName, key2._keyNames[i]))
                {
                    return Object.Equals(keyValue, key2._compositeKeyValues[i]);
                }
            }
            return false;
        }

        #endregion


        /// <summary>
        /// Returns an array of string/<see cref="DbExpression"/> pairs, one for each key value in this EntityKey,
        /// where the string is the key member name and the DbExpression is the value in this EntityKey
        /// for that key member, represented as a <see cref="DbConstantExpression"/> with the same result
        /// type as the key member.
        /// </summary>
        /// <param name="entitySet">The entity set to which this EntityKey refers; used to verify that this key has the required key members</param>
        /// <returns>The name -> expression mappings for the key member values represented by this EntityKey</returns>
        internal KeyValuePair<string, DbExpression>[] GetKeyValueExpressions(EntitySet entitySet)
        {
            Debug.Assert(!IsTemporary, "GetKeyValueExpressions doesn't make sense for temporary keys - they have no values.");
            Debug.Assert(entitySet != null, "GetEntitySet should not return null.");
            Debug.Assert(entitySet.Name == _entitySetName, "EntitySet returned from GetEntitySet has incorrect name.");
            int numKeyMembers = 0;
            if (!IsTemporary)
            {
                if (_singletonKeyValue != null)
                {
                    numKeyMembers = 1;
                }
                else
                {
                    numKeyMembers = _compositeKeyValues.Length;
                }
            }
            if (((EntitySetBase)entitySet).ElementType.KeyMembers.Count != numKeyMembers)
            {
                // If we found an entity set by name that's a different CLR reference 
                // than the one contained by this EntityKey, the two entity sets could
                // be incompatible.  The only error case we need to handle here is the
                // one where the number of key members differs; other error cases
                // will be handled by the command tree builder methods.

                // 


                throw EntityUtil.EntitySetDoesNotMatch("metadataWorkspace", TypeHelpers.GetFullName(entitySet));
            }

            // Iterate over the internal collection of string->object
            // key value pairs and create a list of string->constant
            // expression key value pairs.
            KeyValuePair<string, DbExpression>[] keyColumns;
            if (_singletonKeyValue != null)
            {
                EdmMember singletonKeyMember = ((EntitySetBase)entitySet).ElementType.KeyMembers[0];
                Debug.Assert(singletonKeyMember != null, "Metadata for singleton key member shouldn't be null.");
                keyColumns =
                    new[] { DbExpressionBuilder.Constant(Helper.GetModelTypeUsage(singletonKeyMember), _singletonKeyValue)
                            .As(singletonKeyMember.Name) };

            }
            else
            {
                keyColumns = new KeyValuePair<string, DbExpression>[_compositeKeyValues.Length];
                for (int i = 0; i < _compositeKeyValues.Length; ++i)
                {
                    Debug.Assert(_compositeKeyValues[i] != null, "Values within key-value pairs cannot be null.");

                    EdmMember keyMember = ((EntitySetBase)entitySet).ElementType.KeyMembers[i];
                    Debug.Assert(keyMember != null, "Metadata for key members shouldn't be null.");
                    keyColumns[i] = DbExpressionBuilder.Constant(Helper.GetModelTypeUsage(keyMember), _compositeKeyValues[i]).As(keyMember.Name);
                }
            }

            return keyColumns;
        }

        /// <summary>
        /// Returns a string representation of this EntityKey, for use in debugging.
        /// Note that the returned string contains potentially sensitive information
        /// (i.e., key values), and thus shouldn't be publicly exposed.
        /// </summary>
        internal string ConcatKeyValue()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("EntitySet=").Append(_entitySetName);
            if (!IsTemporary)
            {
                foreach (EntityKeyMember pair in EntityKeyValues)
                {
                    builder.Append(';');
                    builder.Append(pair.Key).Append("=").Append(pair.Value);
                }

            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns the appropriate value for the given key name. 
        /// </summary>
        internal object FindValueByName(string keyName)
        {
            Debug.Assert(!IsTemporary, "FindValueByName should not be called for temporary keys.");
            if (SingletonKeyValue != null)
            {
                Debug.Assert(_keyNames[0] == keyName, "For a singleton key, the given keyName must match.");
                return _singletonKeyValue;
            }
            else
            {
                object[] compositeKeyValues = CompositeKeyValues;
                for (int i = 0; i < compositeKeyValues.Length; i++)
                {
                    if (keyName == _keyNames[i])
                    {
                        return compositeKeyValues[i];
                    }
                }
                throw EntityUtil.ArgumentOutOfRange("keyName");
            }
        }

        internal static void GetEntitySetName(string qualifiedEntitySetName, out  string entitySet, out string container)
        {
            entitySet = null;
            container = null;
            EntityUtil.CheckStringArgument(qualifiedEntitySetName, "qualifiedEntitySetName");

            string[] result = qualifiedEntitySetName.Split('.');
            if (result.Length != 2)
            {
                throw EntityUtil.InvalidQualifiedEntitySetName();
            }

            container = result[0];
            entitySet = result[1];

            // both parts must be non-empty
            if (container == null || container.Length == 0 ||
                entitySet == null || entitySet.Length == 0)
            {
                throw EntityUtil.InvalidQualifiedEntitySetName();
            }

            ValidateName(container);
            ValidateName(entitySet);
        }

        internal static void ValidateName(string name)
        {
            if (!System.Data.EntityModel.SchemaObjectModel.Utils.ValidUndottedName(name))
            {
                throw EntityUtil.EntityKeyInvalidName(name);
            }
        }

        #region Key Value Assignment and Validation

        private static bool CheckKeyValues(IEnumerable<KeyValuePair<string, object>> entityKeyValues,
            out string[] keyNames, out object singletonKeyValue, out object[] compositeKeyValues)
        {
            return CheckKeyValues(entityKeyValues, false, false, out keyNames, out singletonKeyValue, out compositeKeyValues);
        }

        private static bool CheckKeyValues(IEnumerable<KeyValuePair<string, object>> entityKeyValues, bool allowNullKeys, bool tokenizeStrings,
            out string[] keyNames, out object singletonKeyValue, out object[] compositeKeyValues)
        {
            EntityUtil.CheckArgumentNull(entityKeyValues, "entityKeyValues");

            int numExpectedKeyValues;
            int numActualKeyValues = 0;

            keyNames = null;
            singletonKeyValue = null;
            compositeKeyValues = null;

            // Determine if we're a single or composite key.
            foreach (KeyValuePair<string, object> value in entityKeyValues)
            {
                numActualKeyValues++;
            }

            numExpectedKeyValues = numActualKeyValues;
            if (numExpectedKeyValues == 0)
            {
                if (!allowNullKeys)
                {
                    throw EntityUtil.EntityKeyMustHaveValues("entityKeyValues");
                }
            }
            else
            {
                keyNames = new string[numExpectedKeyValues];

                if (numExpectedKeyValues == 1)
                {
                    lock (_nameLookup)
                    {
                        foreach (KeyValuePair<string, object> keyValuePair in entityKeyValues)
                        {
                            if (EntityUtil.IsNull(keyValuePair.Value) || String.IsNullOrEmpty(keyValuePair.Key))
                            {
                                throw EntityUtil.NoNullsAllowedInKeyValuePairs("entityKeyValues");
                            }
                            ValidateName(keyValuePair.Key);
                            keyNames[0] = tokenizeStrings ? EntityKey.LookupSingletonName(keyValuePair.Key) : keyValuePair.Key;
                            singletonKeyValue = keyValuePair.Value;
                        }
                    }
                }
                else
                {
                    compositeKeyValues = new object[numExpectedKeyValues];

                    int i = 0;
                    lock (_nameLookup)
                    {
                        foreach (KeyValuePair<string, object> keyValuePair in entityKeyValues)
                        {
                            if (EntityUtil.IsNull(keyValuePair.Value) || String.IsNullOrEmpty(keyValuePair.Key))
                            {
                                throw EntityUtil.NoNullsAllowedInKeyValuePairs("entityKeyValues");
                            }
                            Debug.Assert(null == keyNames[i], "shouldn't have a name yet");
                            ValidateName(keyValuePair.Key);
                            keyNames[i] = tokenizeStrings ? EntityKey.LookupSingletonName(keyValuePair.Key) : keyValuePair.Key;
                            compositeKeyValues[i] = keyValuePair.Value;
                            i++;
                        }
                    }
                }
            }
            return numExpectedKeyValues > 0;
        }

        /// <summary>
        /// Validates the record parameter passed to the EntityKey constructor, 
        /// and converts the data into the form required by EntityKey.  For singleton keys, 
        /// this is a single object.  For composite keys, this is an object array.
        /// </summary>
        /// <param name="entitySet">the entity set metadata object which this key refers to</param>
        /// <param name="record">the parameter to validate</param>
        /// <param name="numExpectedKeyValues">the number of expected key-value pairs</param>
        /// <param name="argumentName">the name of the argument to use in exception messages</param>
        /// <param name="workspace">MetadataWorkspace used to resolve and validate enum keys.</param>
        /// <returns>the validated value(s) (for a composite key, an object array is returned)</returns>
        private static void GetKeyValues(EntitySet entitySet, IExtendedDataRecord record, 
            out string[] keyNames, out object singletonKeyValue, out object[] compositeKeyValues)
        {
            singletonKeyValue = null;
            compositeKeyValues = null;

            int numExpectedKeyValues = ((EntitySetBase)entitySet).ElementType.KeyMembers.Count;
            keyNames = ((EntitySetBase)entitySet).ElementType.KeyMemberNames;

            EntityType entityType = record.DataRecordInfo.RecordType.EdmType as EntityType;
            Debug.Assert(entityType != null, "Data record must be an entity.");

            // assert the type contained by this entity set matches the type contained by the data record
            Debug.Assert(entitySet != null && entitySet.ElementType.IsAssignableFrom(entityType), "Entity types do not match");
            Debug.Assert(numExpectedKeyValues > 0, "Should be expecting a positive number of key-values.");

            if (numExpectedKeyValues == 1)
            {
                // Optimize for a singleton key.

                EdmMember member = entityType.KeyMembers[0];
                singletonKeyValue = record[member.Name];
                if (EntityUtil.IsNull(singletonKeyValue))
                {
                    throw EntityUtil.NoNullsAllowedInKeyValuePairs("record");
                }
            }
            else
            {
                compositeKeyValues = new object[numExpectedKeyValues];
                // grab each key-field from the data record
                for (int i = 0; i < numExpectedKeyValues; ++i)
                {
                    EdmMember member = entityType.KeyMembers[i];
                    compositeKeyValues[i] = record[member.Name];
                    if (EntityUtil.IsNull(compositeKeyValues[i]))
                    {
                        throw EntityUtil.NoNullsAllowedInKeyValuePairs("record");
                    }
                }
            }
        }

        /// <summary>
        /// Verify that the types of the objects passed in to be used as keys actually match the types from the model.
        /// This error is also caught when the entity is materialized and when the key value is set, at which time it
        /// also throws ThrowSetInvalidValue().
        /// SQLBUDT 513838. This error is possible and should be caught at run time, not in an assertion.
        /// </summary>
        /// <param name="workspace">MetadataWorkspace used to resolve and validate types of enum keys.</param>
        /// <param name="entitySet">The EntitySet to validate against</param>
        internal void ValidateEntityKey(MetadataWorkspace workspace, EntitySet entitySet)
        {
            ValidateEntityKey(workspace, entitySet, false, null);
        }
        /// <summary>
        /// Verify that the types of the objects passed in to be used as keys actually match the types from the model.
        /// This error is also caught when the entity is materialized and when the key value is set, at which time it
        /// also throws ThrowSetInvalidValue().
        /// SQLBUDT 513838. This error is possible and should be caught at run time, not in an assertion.
        /// </summary>
        /// <param name="workspace">MetadataWorkspace used to resolve and validate types of enum keys.</param>
        /// <param name="entitySet">The EntitySet to validate against</param>
        /// <param name="isArgumentException">Wether to throw ArgumentException or InvalidOperationException.</param>
        /// <param name="argumentName">Name of the argument in case of ArgumentException.</param>
        internal void ValidateEntityKey(MetadataWorkspace workspace, EntitySet entitySet, bool isArgumentException, string argumentName)
        {
            if (entitySet != null)
            {
                ReadOnlyMetadataCollection<EdmMember> keyMembers = ((EntitySetBase)entitySet).ElementType.KeyMembers;
                if (_singletonKeyValue != null)
                {
                    // 1. Validate number of keys
                    if (keyMembers.Count != 1)
                    {
                        if (isArgumentException)
                        {
                            throw EntityUtil.IncorrectNumberOfKeyValuePairs(argumentName, entitySet.ElementType.FullName, keyMembers.Count, 1);
                        }
                        else
                        {
                            throw EntityUtil.IncorrectNumberOfKeyValuePairsInvalidOperation(entitySet.ElementType.FullName, keyMembers.Count, 1);
                        }
                    }

                    // 2. Validate type of key values
                    ValidateTypeOfKeyValue(workspace, keyMembers[0], _singletonKeyValue, isArgumentException, argumentName);

                    // 3. Validate key names
                    if (_keyNames[0] != keyMembers[0].Name)
                    {
                        if (isArgumentException)
                        {
                            throw EntityUtil.MissingKeyValue(argumentName, keyMembers[0].Name, entitySet.ElementType.FullName);
                        }
                        else
                        {
                            throw EntityUtil.MissingKeyValueInvalidOperation(keyMembers[0].Name, entitySet.ElementType.FullName);
                        }
                    }
                }
                else if (null != _compositeKeyValues)
                {
                    // 1. Validate number of keys
                    if (keyMembers.Count != _compositeKeyValues.Length)
                    {
                        if (isArgumentException)
                        {
                            throw EntityUtil.IncorrectNumberOfKeyValuePairs(argumentName, entitySet.ElementType.FullName, keyMembers.Count, _compositeKeyValues.Length);
                        }
                        else
                        {
                            throw EntityUtil.IncorrectNumberOfKeyValuePairsInvalidOperation(entitySet.ElementType.FullName, keyMembers.Count, _compositeKeyValues.Length);
                        }
                    }

                    for (int i = 0; i < _compositeKeyValues.Length; ++i)
                    {
                        EdmMember keyField = ((EntitySetBase)entitySet).ElementType.KeyMembers[i];
                        bool foundMember = false;
                        for (int j = 0; j < _compositeKeyValues.Length; ++j)
                        {
                            if (keyField.Name == _keyNames[j])
                            {
                                // 2. Validate type of key values
                                ValidateTypeOfKeyValue(workspace, keyField, _compositeKeyValues[j], isArgumentException, argumentName);

                                foundMember = true;
                                break;
                            }
                        }
                        // 3. Validate Key Name (if we found it or not)
                        if (!foundMember)
                        {
                            if (isArgumentException)
                            {
                                throw EntityUtil.MissingKeyValue(argumentName, keyField.Name, entitySet.ElementType.FullName);
                            }
                            else
                            {
                                throw EntityUtil.MissingKeyValueInvalidOperation(keyField.Name, entitySet.ElementType.FullName);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates whether type of the key matches the type of the key value.
        /// </summary>
        /// <param name="workspace">MetadataWorkspace used to resolve and validate types of enum keys.</param>
        /// <param name="keyMember">Edm key member.</param>
        /// <param name="keyValue">The value of the key.</param>
        /// <param name="isArgumentException">Whether to throw ArgumentException or InvalidOperation exception if validation fails.</param>
        /// <param name="argumentName">Name of the argument to be used for ArgumentExceptions.</param>
        private static void ValidateTypeOfKeyValue(MetadataWorkspace workspace, EdmMember keyMember, object keyValue, bool isArgumentException, string argumentName)
        {
            Debug.Assert(workspace != null, "workspace != null");
            Debug.Assert(keyMember != null, "keyMember != null");
            Debug.Assert(keyValue != null, "keyValue != null");
            Debug.Assert(Helper.IsScalarType(keyMember.TypeUsage.EdmType), "key member must be of a scalar type");

            EdmType keyMemberEdmType = keyMember.TypeUsage.EdmType;

            if (Helper.IsPrimitiveType(keyMemberEdmType))
            {
                Type entitySetKeyType = ((PrimitiveType)keyMemberEdmType).ClrEquivalentType;
                if (entitySetKeyType != keyValue.GetType())
                {
                    if (isArgumentException)
                    {
                        throw EntityUtil.IncorrectValueType(argumentName, keyMember.Name, entitySetKeyType.FullName, keyValue.GetType().FullName);
                    }
                    else
                    {
                        throw EntityUtil.IncorrectValueTypeInvalidOperation(keyMember.Name, entitySetKeyType.FullName, keyValue.GetType().FullName);
                    }
                }
            }
            else
            {
                Debug.Assert(Helper.IsEnumType(keyMember.TypeUsage.EdmType), "Enum type expected");

                EnumType expectedEnumType;
                if (workspace.TryGetObjectSpaceType((EnumType)keyMemberEdmType, out expectedEnumType))
                {
                    var expectedClrEnumType = ((ClrEnumType)expectedEnumType).ClrType;
                    if (expectedClrEnumType != keyValue.GetType())
                    {
                        if (isArgumentException)
                        {
                            throw EntityUtil.IncorrectValueType(argumentName, keyMember.Name, expectedClrEnumType.FullName, keyValue.GetType().FullName);
                        }
                        else
                        {
                            throw EntityUtil.IncorrectValueTypeInvalidOperation(keyMember.Name, expectedClrEnumType.FullName, keyValue.GetType().FullName);
                        }
                    }
                }
                else
                {
                    if (isArgumentException)
                    {
                        throw EntityUtil.NoCorrespondingOSpaceTypeForEnumKeyField(argumentName, keyMember.Name, keyMemberEdmType.FullName);
                    }
                    else
                    {
                        throw EntityUtil.NoCorrespondingOSpaceTypeForEnumKeyFieldInvalidOperation(keyMember.Name, keyMemberEdmType.FullName);
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that the "state" of the EntityKey is correct, by validating assumptions
        /// based on whether the key is a singleton, composite, or temporary.
        /// </summary>
        /// <param name="isTemporary">whether we expect this EntityKey to be marked temporary</param>
        [Conditional("DEBUG")]
        private void AssertCorrectState(EntitySetBase entitySet, bool isTemporary)
        {
            if (_singletonKeyValue != null)
            {
                Debug.Assert(!isTemporary, "Singleton keys should not be expected to be temporary.");
                Debug.Assert(_compositeKeyValues == null, "The EntityKey is marked as both a singleton key and a composite key - this is illegal.");
                if (entitySet != null)
                {
                    Debug.Assert(entitySet.ElementType.KeyMembers.Count == 1, "For a singleton key, the number of key fields must be exactly 1.");
                }
            }
            else if (_compositeKeyValues != null)
            {
                Debug.Assert(!isTemporary, "Composite keys should not be expected to be temporary.");
                if (entitySet != null)
                {
                    Debug.Assert(entitySet.ElementType.KeyMembers.Count > 1, "For a composite key, the number of key fields should be greater than 1.");
                    Debug.Assert(entitySet.ElementType.KeyMembers.Count == _compositeKeyValues.Length, "Incorrect number of values specified to composite key.");
                }
                for (int i = 0; i < _compositeKeyValues.Length; ++i)
                {
                    Debug.Assert(_compositeKeyValues[i] != null, "Values passed to a composite EntityKey cannot be null.");
                }
            }
            else if (!IsTemporary)
            {
                // one of our static keys
                Debug.Assert(!isTemporary, "Static keys should not be expected to be temporary.");
                Debug.Assert(this.EntityKeyValues == null, "The EntityKeyValues property for Static EntityKeys must return null.");
                Debug.Assert(this.EntityContainerName == null, "The EntityContainerName property for Static EntityKeys must return null.");
                Debug.Assert(this.EntitySetName != null, "The EntitySetName property for Static EntityKeys must not return null.");
            }
            else
            {
                Debug.Assert(isTemporary, "The EntityKey is marked as neither a singleton or composite.  Therefore, it should be expected to be temporary.");
                Debug.Assert(this.IsTemporary, "The EntityKey is marked as neither a singleton or composite.  Therefore it must be marked as temporary.");
                Debug.Assert(this.EntityKeyValues == null, "The EntityKeyValues property for temporary EntityKeys must return null.");
            }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [OnDeserializing]
        public void OnDeserializing(StreamingContext context)
        {
            if (RequiresDeserialization)
            {
                DeserializeMembers();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public void OnDeserialized(StreamingContext context)
        {
            lock (_nameLookup)
            {
                _entitySetName = LookupSingletonName(_entitySetName);
                _entityContainerName = LookupSingletonName(_entityContainerName);
                if (_keyNames != null)
                {
                    for (int i = 0; i < _keyNames.Length; i++)
                    {
                        _keyNames[i] = LookupSingletonName(_keyNames[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Dev Note: this must be called from within a _lock block on _nameLookup
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string LookupSingletonName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }
            if (_nameLookup.ContainsKey(name))
            {
                return _nameLookup[name];
            }
            _nameLookup.Add(name, name);
            return name;
        }

        private void ValidateWritable(object instance)
        {
            if (_isLocked || instance != null)
            {
                throw EntityUtil.CannotChangeEntityKey();
            }
        }

        private bool RequiresDeserialization
        {
            get { return _deserializedMembers != null; }
        }

        private void DeserializeMembers()
        {
            if (CheckKeyValues(new KeyValueReader(_deserializedMembers), true, true, out _keyNames, out _singletonKeyValue, out _compositeKeyValues))
            {
                // If we received values from the _deserializedMembers, then we do not need to track these any more
                _deserializedMembers = null;
            }
        }

        #endregion

        private class KeyValueReader : IEnumerable<KeyValuePair<string, object>>
        {
            IEnumerable<EntityKeyMember> _enumerator;

            public KeyValueReader(IEnumerable<EntityKeyMember> enumerator)
            {
                _enumerator = enumerator;
            }

            #region IEnumerable<KeyValuePair<string,object>> Members

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                foreach (EntityKeyMember pair in _enumerator)
                {
                    if (pair != null)
                    {
                        yield return new KeyValuePair<string, object>(pair.Key, pair.Value);
                    }
                }
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }
    }

    /// <summary>
    /// Information about a key that is part of an EntityKey.
    /// A key member contains the key name and value.
    /// </summary>
    [DataContract]
    [Serializable]
    public class EntityKeyMember
    {
        private string _keyName;
        private object _keyValue;

        /// <summary>
        /// Creates an empty EntityKeyMember. This constructor is used by serialization.
        /// </summary>
        public EntityKeyMember()
        {
        }

        /// <summary>
        /// Creates a new EntityKeyMember with the specified key name and value.
        /// </summary>
        /// <param name="keyName">The key name</param>
        /// <param name="keyValue">The key value</param>
        public EntityKeyMember(string keyName, object keyValue)
        {
            EntityUtil.CheckArgumentNull(keyName, "keyName");
            EntityUtil.CheckArgumentNull(keyValue, "keyValue");
            _keyName = keyName;
            _keyValue = keyValue;
        }

        /// <summary>
        /// The key name
        /// </summary>
        [DataMember]
        public string Key
        {
            get
            {
                return _keyName;
            }
            set
            {
                ValidateWritable(_keyName);
                EntityUtil.CheckArgumentNull(value, "value");
                _keyName = value;
            }
        }

        /// <summary>
        /// The key value
        /// </summary>
        [DataMember]
        public object Value
        {
            get
            {
                return _keyValue;
            }
            set
            {
                ValidateWritable(_keyValue);
                EntityUtil.CheckArgumentNull(value, "value");
                _keyValue = value;
            }
        }

        /// <summary>
        /// Returns a string representation of the EntityKeyMember
        /// </summary>
        /// <returns>A string representation of the EntityKeyMember</returns>
        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.CurrentCulture, "[{0}, {1}]", _keyName, _keyValue);
        }

        /// <summary>
        /// Ensures that the instance can be written to (value must be null)
        /// </summary>
        private void ValidateWritable(object instance)
        {
            if (instance != null)
            {
                throw EntityUtil.CannotChangeEntityKey();
            }
        }
    }
}

