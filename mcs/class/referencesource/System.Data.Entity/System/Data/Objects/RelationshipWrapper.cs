//---------------------------------------------------------------------
// <copyright file="RelationshipWrapper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Metadata.Edm;

namespace System.Data.Objects
{
    internal sealed class RelationshipWrapper : IEquatable<RelationshipWrapper>
    {
        internal readonly AssociationSet AssociationSet;
        internal readonly EntityKey Key0;
        internal readonly EntityKey Key1;

        internal RelationshipWrapper(AssociationSet extent, EntityKey key)
        {
            Debug.Assert(null != extent, "null RelationshipWrapper");
            Debug.Assert(null != (object)key, "null key");

            this.AssociationSet = extent;
            this.Key0 = key;
            this.Key1 = key;
        }

        internal RelationshipWrapper(RelationshipWrapper wrapper, int ordinal, EntityKey key)
        {
            Debug.Assert(null != wrapper, "null RelationshipWrapper");
            Debug.Assert((uint)ordinal <= 1u, "ordinal out of range");
            Debug.Assert(null != (object)key, "null key2");

            this.AssociationSet = wrapper.AssociationSet;
            this.Key0 = (0 == ordinal) ? key : wrapper.Key0;
            this.Key1 = (0 == ordinal) ? wrapper.Key1 : key;
        }

        internal RelationshipWrapper(AssociationSet extent,
                                     KeyValuePair<string, EntityKey> roleAndKey1,
                                     KeyValuePair<string, EntityKey> roleAndKey2)
            : this(extent, roleAndKey1.Key, roleAndKey1.Value, roleAndKey2.Key, roleAndKey2.Value)
        {
        }

        internal RelationshipWrapper(AssociationSet extent,
                                     string role0, EntityKey key0,
                                     string role1, EntityKey key1)
        {
            Debug.Assert(null != extent, "null AssociationSet");
            Debug.Assert(null != (object)key0, "null key0");
            Debug.Assert(null != (object)key1, "null key1");

            AssociationSet = extent;
            Debug.Assert(extent.ElementType.AssociationEndMembers.Count == 2, "only 2 ends are supported");

            // this assert is explictly commented out to show that the two are similar but different
            // we should always use AssociationEndMembers, never CorrespondingAssociationEndMember
            //Debug.Assert(AssociationSet.AssociationSetEnds.Count == 2, "only 2 set ends supported");
            //Debug.Assert(extent.ElementType.AssociationEndMembers[0] == AssociationSet.AssociationSetEnds[0].CorrespondingAssociationEndMember, "should be same end member");
            //Debug.Assert(extent.ElementType.AssociationEndMembers[1] == AssociationSet.AssociationSetEnds[1].CorrespondingAssociationEndMember, "should be same end member");

            if (extent.ElementType.AssociationEndMembers[0].Name == role0)
            {
                Debug.Assert(extent.ElementType.AssociationEndMembers[1].Name == role1, "a)roleAndKey1 Name differs");
                Key0 = key0;
                Key1 = key1;
            }
            else
            {
                Debug.Assert(extent.ElementType.AssociationEndMembers[0].Name == role1, "b)roleAndKey1 Name differs");
                Debug.Assert(extent.ElementType.AssociationEndMembers[1].Name == role0, "b)roleAndKey0 Name differs");
                Key0 = key1;
                Key1 = key0;
            }
        }

        internal ReadOnlyMetadataCollection<AssociationEndMember> AssociationEndMembers
        {
            get { return this.AssociationSet.ElementType.AssociationEndMembers; }
        }

        internal AssociationEndMember GetAssociationEndMember(EntityKey key)
        {
            Debug.Assert(Key0 == key || Key1 == key, "didn't match a key");
            return AssociationEndMembers[(Key0 != key) ? 1 : 0];
        }

        internal EntityKey GetOtherEntityKey(EntityKey key)
        {
            return ((Key0 == key) ? Key1 : ((Key1 == key) ? Key0 : null));
        }

        internal EntityKey GetEntityKey(int ordinal)
        {
            switch (ordinal)
            {
                case 0:
                    return Key0;
                case 1:
                    return Key1;
                default:
                    throw EntityUtil.ArgumentOutOfRange("ordinal");
            }
        }

        public override int GetHashCode()
        {
            return this.AssociationSet.Name.GetHashCode() ^ (this.Key0.GetHashCode() + this.Key1.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RelationshipWrapper);
        }

        public bool Equals(RelationshipWrapper wrapper)
        {
            return (Object.ReferenceEquals(this, wrapper) ||
                    ((null != wrapper) &&
                     Object.ReferenceEquals(this.AssociationSet, wrapper.AssociationSet) &&
                     this.Key0.Equals(wrapper.Key0) &&
                     this.Key1.Equals(wrapper.Key1)));
        }
    }
}
