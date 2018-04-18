//---------------------------------------------------------------------
// <copyright file="ObjectStateManagerMetadata.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Data.Mapping;
using System.Diagnostics;

namespace System.Data.Objects
{

    internal struct EntitySetQualifiedType : IEqualityComparer<EntitySetQualifiedType>
    {
        internal static readonly IEqualityComparer<EntitySetQualifiedType> EqualityComparer = new EntitySetQualifiedType();        
        
        internal readonly Type ClrType;
        internal readonly EntitySet EntitySet;

        internal EntitySetQualifiedType(Type type, EntitySet set)
        {
            Debug.Assert(null != type, "null Type");
            Debug.Assert(null != set, "null EntitySet");
            Debug.Assert(null != set.EntityContainer, "null EntityContainer");
            Debug.Assert(null != set.EntityContainer.Name, "null EntityContainer.Name");
            ClrType = EntityUtil.GetEntityIdentityType(type);
            EntitySet = set;
        }

        public bool Equals(EntitySetQualifiedType x, EntitySetQualifiedType y)
        {
            return (Object.ReferenceEquals(x.ClrType, y.ClrType) &&
                    Object.ReferenceEquals(x.EntitySet, y.EntitySet));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2303", Justification="ClrType is not expected to be an Embedded Interop Type.")]
        public int GetHashCode(EntitySetQualifiedType obj)
        {
            return unchecked(obj.ClrType.GetHashCode() +
                             obj.EntitySet.Name.GetHashCode() +
                             obj.EntitySet.EntityContainer.Name.GetHashCode());
        }
    }

    internal sealed class StateManagerMemberMetadata
    {
        private readonly EdmProperty _clrProperty; // may be null if shadowState
        private readonly EdmProperty _edmProperty;
        private readonly bool _isPartOfKey;
        private readonly bool _isComplexType;

        internal StateManagerMemberMetadata(ObjectPropertyMapping memberMap, EdmProperty memberMetadata, bool isPartOfKey)
        {
            // if memberMap is null, then this is a shadowstate
            Debug.Assert(null != memberMap, "shadowstate not supported");
            Debug.Assert(null != memberMetadata, "CSpace should never be null");
            _clrProperty = memberMap.ClrProperty;
            _edmProperty = memberMetadata;
            _isPartOfKey = isPartOfKey;
            _isComplexType = (Helper.IsEntityType(_edmProperty.TypeUsage.EdmType) ||
                             Helper.IsComplexType(_edmProperty.TypeUsage.EdmType));
        }

        internal string CLayerName
        {
            get
            {
                return _edmProperty.Name;
            }
        }

        internal Type ClrType
        {
            get
            {
                Debug.Assert(null != _clrProperty, "shadowstate not supported");
                return _clrProperty.TypeUsage.EdmType.ClrType;
                //return ((null != _clrProperty)
                //    ? _clrProperty.TypeUsage.EdmType.ClrType
                //    : (Helper.IsComplexType(_edmProperty)
                //        ? typeof(DbDataRecord)
                //        : ((PrimitiveType)_edmProperty.TypeUsage.EdmType).ClrEquivalentType));
            }
        }
        internal bool IsComplex
        {
            get
            {
                return _isComplexType;
            }
        }
        internal EdmProperty CdmMetadata
        {
            get
            {
                return _edmProperty;
            }
        }
        internal EdmProperty ClrMetadata
        {
            get
            {
                Debug.Assert(null != _clrProperty, "shadowstate not supported");
                return _clrProperty;
            }
        }
        internal bool IsPartOfKey
        {
            get
            {
                return _isPartOfKey;
            }
        }
        public object GetValue(object userObject) // wrapp it in cacheentry
        {
            Debug.Assert(null != _clrProperty, "shadowstate not supported");
            object dataObject = LightweightCodeGenerator.GetValue(_clrProperty, userObject);
            return dataObject;
        }
        public void SetValue(object userObject, object value) // if record , unwrapp to object, use materializer in cacheentry
        {
            Debug.Assert(null != _clrProperty, "shadowstate not supported");
            if (DBNull.Value == value)
            {
                value = null;
            }
            if (IsComplex && value == null)
            {
                throw EntityUtil.NullableComplexTypesNotSupported(CLayerName);
            }
            LightweightCodeGenerator.SetValue(_clrProperty, userObject, value);
        }
    }

    internal sealed class StateManagerTypeMetadata
    {
        private readonly TypeUsage _typeUsage; // CSpace
        private readonly ObjectTypeMapping _ocObjectMap;
        private readonly StateManagerMemberMetadata[] _members;
        private readonly Dictionary<string, int> _objectNameToOrdinal;
        private readonly Dictionary<string, int> _cLayerNameToOrdinal;
        private readonly DataRecordInfo _recordInfo;

        internal StateManagerTypeMetadata(EdmType edmType, ObjectTypeMapping mapping)
        {
            Debug.Assert(null != edmType, "null EdmType");
            Debug.Assert(Helper.IsEntityType(edmType) ||
                         Helper.IsComplexType(edmType),
                         "not Complex or EntityType");
            Debug.Assert(Object.ReferenceEquals(mapping, null) ||
                         Object.ReferenceEquals(mapping.EdmType, edmType),
                         "different EdmType instance");

            _typeUsage = TypeUsage.Create(edmType);
            _recordInfo = new DataRecordInfo(_typeUsage);
            _ocObjectMap = mapping;

            ReadOnlyMetadataCollection<EdmProperty> members = TypeHelpers.GetProperties(edmType);
            _members = new StateManagerMemberMetadata[members.Count];
            _objectNameToOrdinal = new Dictionary<string, int>(members.Count);
            _cLayerNameToOrdinal = new Dictionary<string, int>(members.Count);

            ReadOnlyMetadataCollection<EdmMember> keyMembers = null;
            if (Helper.IsEntityType(edmType))
            {
                keyMembers = ((EntityType)edmType).KeyMembers;
            }

            for (int i = 0; i < _members.Length; ++i)
            {
                EdmProperty member = members[i];

                ObjectPropertyMapping memberMap = null;
                if (null != mapping)
                {
                    memberMap = mapping.GetPropertyMap(member.Name);
                    if (null != memberMap)
                    {
                        _objectNameToOrdinal.Add(memberMap.ClrProperty.Name, i); // olayer name
                    }
                }
                _cLayerNameToOrdinal.Add(member.Name, i); // clayer name

                // Determine whether this member is part of the identity of the entity.
                _members[i] = new StateManagerMemberMetadata(memberMap, member, ((null != keyMembers) && keyMembers.Contains(member)));
            }
        }

        internal TypeUsage CdmMetadata
        {
            get
            {
                return _typeUsage;
            }
        }
        internal DataRecordInfo DataRecordInfo
        {
            get { return _recordInfo; }
        }

        internal int FieldCount
        {
            get
            {
                return _members.Length;
            }
        }

        internal Type GetFieldType(int ordinal)
        {
            return Member(ordinal).ClrType;
        }

        internal StateManagerMemberMetadata Member(int ordinal)
        {
            if (unchecked((uint)ordinal < (uint)_members.Length))
            {
                return _members[ordinal];
            }
            throw EntityUtil.ArgumentOutOfRange("ordinal");
        }

        internal IEnumerable<StateManagerMemberMetadata> Members
        {
            get { return _members; }
        }

        internal string CLayerMemberName(int ordinal)
        {
            return Member(ordinal).CLayerName;
        }
        internal int GetOrdinalforOLayerMemberName(string name)
        {
            int ordinal;
            if (String.IsNullOrEmpty(name) || !_objectNameToOrdinal.TryGetValue(name, out ordinal))
            {
                ordinal = -1;
            }
            return ordinal;
        }
        internal int GetOrdinalforCLayerMemberName(string name)
        {
            int ordinal;
            if (String.IsNullOrEmpty(name) || !_cLayerNameToOrdinal.TryGetValue(name, out ordinal))
            {
                ordinal = -1;
            }
            return ordinal;
        }
        internal bool IsMemberPartofShadowState(int ordinal)
        {
            // 


            Debug.Assert(Member(ordinal) != null,
                "The only case where Member(ordinal) can be null is if the property is in shadow state.  " +
                "When shadow state support is added, this assert should never fire.");
            return (null == Member(ordinal).ClrMetadata);
        }
    }
}
