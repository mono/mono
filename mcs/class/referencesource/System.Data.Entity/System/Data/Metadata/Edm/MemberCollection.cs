//---------------------------------------------------------------------
// <copyright file="MemberCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class representing a collection of member objects
    /// </summary>
    internal sealed class MemberCollection : MetadataCollection<EdmMember>
    {
        // This way this collection works is that it has storage for members on the current type and access to
        // members in the base types. As of that requirement, MemberCollection has a reference back to the declaring
        // type that owns this collection. Whenever MemberCollection is asked to do a look by name, it looks at the
        // current collection, if it doesn't find it, then it ask for it from the declaring type's base type's
        // MemberCollection. Because of this order, members in derived types hide members in the base type if they
        // have the same name. For look up by index, base type members have lower index then current type's members.
        // Add/Update/Remove operations on this collection is only allowed for members owned by this MemberCollection
        // and not allowed for members owned by MemberCollections in the base types. For example, if the caller tries
        // to remove a member by ordinal which is within the base type's member ordinal range, it throws an exception.
        // Hence, base type members are in a sense "readonly" to this MemberCollection. When enumerating all the
        // members, the enumeration starts from members in the root type in the inheritance chain. With this special
        // enumeration requirement, we have a specialized enumerator class for this MemberCollection. See the
        // Enumerator class for details on how it works.

        #region Constructors
        /// <summary>
        /// Default constructor for constructing an empty collection
        /// </summary>
        /// <param name="declaringType">The type that has this member collection</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the declaring type is null</exception>
        public MemberCollection(StructuralType declaringType)
            : this(declaringType, null)
        {
        }

        /// <summary>
        /// The constructor for constructing the collection with the given items
        /// </summary>
        /// <param name="declaringType">The type that has this member collection</param>
        /// <param name="items">The items to populate the collection</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the declaring type is null</exception>
        public MemberCollection(StructuralType declaringType, IEnumerable<EdmMember> items)
            : base(items)
        {
            Debug.Assert(declaringType != null, "This member collection must belong to a declaring type");
            _declaringType = declaringType;
        }
        #endregion

        #region Fields
        private StructuralType _declaringType;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the collection as a readonly collection
        /// </summary>
        public override System.Collections.ObjectModel.ReadOnlyCollection<EdmMember> AsReadOnly
        {
            get
            {
                return new System.Collections.ObjectModel.ReadOnlyCollection<EdmMember>(this);
            }
        }

        /// <summary>
        /// Gets the count on the number of items in the collection
        /// </summary>
        public override int Count
        {
            get
            {
                return GetBaseTypeMemberCount() + base.Count;
            }
        }

        /// <summary>
        /// Gets an item from the collection with the given index
        /// </summary>
        /// <param name="index">The index to search for</param>
        /// <returns>An item from the collection</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the index is out of the range for the Collection</exception>
        /// <exception cref="System.InvalidOperationException">Always thrown on setter</exception>
        public override EdmMember this[int index]
        {
            get
            {
                int relativeIndex = GetRelativeIndex(index);
                if (relativeIndex < 0)
                {
                    // This means baseTypeMemberCount must be non-zero, so we can safely cast the base type to StructuralType
                    return ((StructuralType)_declaringType.BaseType).Members[index];
                }

                return base[relativeIndex];
            }
            set
            {
                throw EntityUtil.OperationOnReadOnlyCollection();
            }
        }

        /// <summary>
        /// Gets an item from the collection with the given identity
        /// </summary>
        /// <param name="identity">The identity of the item to search for</param>
        /// <returns>An item from the collection</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if identity argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the Collection does not have an item with the given identity</exception>
        /// <exception cref="System.InvalidOperationException">Always thrown on setter</exception>
        public override EdmMember this[string identity]
        {
            get
            {
                return GetValue(identity, false);
            }
            set
            {
                throw EntityUtil.OperationOnReadOnlyCollection();
            }
        }

        /// <summary>
        /// Adds an item to the collection 
        /// </summary>
        /// <param name="member">The item to add to the list</param>
        /// <exception cref="System.ArgumentNullException">Thrown if member argument is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the member passed in or the collection itself instance is in ReadOnly state</exception>
        /// <exception cref="System.ArgumentException">Thrown if the member that is being added already belongs to another MemberCollection</exception>
        /// <exception cref="System.ArgumentException">Thrown if the MemberCollection already contains a member with the same identity</exception>
        public override void Add(EdmMember member)
        {
            // Make sure the member is valid for the add operation. 
            ValidateMemberForAdd(member, "member");
            
            base.Add(member);

            // Fix up the declaring type
            member.ChangeDeclaringTypeWithoutCollectionFixup(_declaringType);
        }

        /// <summary>
        /// Determines if this collection contains an item of the given identity
        /// </summary>
        /// <param name="identity">The identity of the item to check for</param>
        /// <returns>True if the collection contains the item with the given identity</returns>
        public override bool ContainsIdentity(string identity)
        {
            if (base.ContainsIdentity(identity))
            {
                return true;
            }

            // The item is not in this collection, check the base type member collection
            EdmType baseType = _declaringType.BaseType;
            if (baseType != null && ((StructuralType)baseType).Members.Contains(identity))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Find the index of an item
        /// </summary>
        /// <param name="item">The item whose index is to be looked for</param>
        /// <returns>The index of the found item, -1 if not found</returns>
        public override int IndexOf(EdmMember item)
        {
            // Try to get it from this collection, if found, then the relative index needs to be added with the number
            // of members in the base type to get the absolute index
            int relativeIndex = base.IndexOf(item);
            if (relativeIndex != -1)
            {
                return relativeIndex + GetBaseTypeMemberCount();
            }

            // Try to find it in the base type
            StructuralType baseType = _declaringType.BaseType as StructuralType;
            if (baseType != null)
            {
                return baseType.Members.IndexOf(item);
            }

            return -1;
        }

        /// <summary>
        /// Copies the items in this collection to an array
        /// </summary>
        /// <param name="array">The array to copy to</param>
        /// <param name="arrayIndex">The index in the array at which to start the copy</param>
        /// <exception cref="System.ArgumentNullException">Thrown if array argument is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the arrayIndex is less than zero</exception>
        /// <exception cref="System.ArgumentException">Thrown if the array argument passed in with respect to the arrayIndex passed in not big enough to hold the MemberCollection being copied</exception>
        public override void CopyTo(EdmMember[] array, int arrayIndex)
        {
            // Check on the array index
            if (arrayIndex < 0)
            {
                throw EntityUtil.ArgumentOutOfRange("arrayIndex");
            }

            // Check if the array together with the array index has enough room to copy
            int baseTypeMemberCount = GetBaseTypeMemberCount();
            if (base.Count + baseTypeMemberCount > array.Length - arrayIndex)
            {
                throw EntityUtil.Argument("arrayIndex");
            }

            // If the base type has any members, copy those first
            if (baseTypeMemberCount > 0)
            {
                ((StructuralType)_declaringType.BaseType).Members.CopyTo(array, arrayIndex);
            }

            base.CopyTo(array, arrayIndex + baseTypeMemberCount);
        }

        /// <summary>
        /// Gets an item from the collection with the given identity
        /// </summary>
        /// <param name="identity">The identity of the item to search for</param>
        /// <param name="ignoreCase">Whether case is ignore in the search</param>
        /// <param name="item">An item from the collection, null if the item is not found</param>
        /// <returns>True an item is retrieved</returns>
        /// <exception cref="System.ArgumentNullException">if identity argument is null</exception>
        public override bool TryGetValue(string identity, bool ignoreCase, out EdmMember item)
        {
            // See if it's in this collection
            if (!base.TryGetValue(identity, ignoreCase, out item))
            {
                // Now go to the parent type to find it
                EdmType baseType = _declaringType.BaseType;
                if (baseType != null)
                {
                    ((StructuralType)baseType).Members.TryGetValue(identity, ignoreCase, out item);
                }
            }

            return item != null;
        }

        /// <summary>
        /// Gets an itme with identity
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public override EdmMember GetValue(string identity, bool ignoreCase)
        {
            EdmMember item = null;

            if (!TryGetValue(identity, ignoreCase, out item))
            {
                throw EntityUtil.ItemInvalidIdentity(identity, "identity");
            }

            return item;
        }

        /// <summary>
        /// Get the declared only members of a particular type
        /// </summary>
        internal ReadOnlyMetadataCollection<T> GetDeclaredOnlyMembers<T>() where T : EdmMember
        {
            MetadataCollection<T> newCollection = new MetadataCollection<T>();
            for (int i = 0; i < base.Count; i++)
            {
                T member = base[i] as T;
                if (member != null)
                {
                    newCollection.Add(member);
                }
            }

            return newCollection.AsReadOnlyMetadataCollection();
        }

        /// <summary>
        /// Get the number of members the base type has.  If the base type is not a structural type or has no
        /// members, it returns 0
        /// </summary>
        /// <returns>The number of members in the base type</returns>
        private int GetBaseTypeMemberCount()
        {
            // The count of members is what in this collection plus base type's member collection
            StructuralType baseType = _declaringType.BaseType as StructuralType;
            if (baseType != null)
            {
                return baseType.Members.Count;
            }

            return 0;
        }

        /// <summary>
        /// Gets the index relative to this collection for the given index.  For an index to really refers to something in
        /// the base type, the return value is negative relative to this collection.  For an index refers to something in this
        /// collection, the return value is positive.  In both cases, it's simply (index) - (base type member count)
        /// </summary>
        /// <returns>The relative index</returns>
        private int GetRelativeIndex(int index)
        {
            int baseTypeMemberCount = GetBaseTypeMemberCount();
            int thisTypeMemberCount = base.Count;

            // Check if the index is in range
            if (index < 0 || index >= baseTypeMemberCount + thisTypeMemberCount)
            {
                throw EntityUtil.ArgumentOutOfRange("index");
            }

            return index - baseTypeMemberCount;
        }
        
        private void ValidateMemberForAdd(EdmMember member, string argumentName)
        {
            // Check to make sure the given member is not associated with another type
            EntityUtil.GenericCheckArgumentNull(member, argumentName);

            Debug.Assert(member.DeclaringType == null, string.Format(CultureInfo.CurrentCulture, "The member {0} already has a declaring type, it cannot be added to this collection.", argumentName));

            // Validate the item with the declaring type. 
            _declaringType.ValidateMemberForAdd(member);
        }

        #endregion
    }
}
