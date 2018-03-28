//---------------------------------------------------------------------
// <copyright file="MemberProjectionIndex.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
using System.Data.Common.Utils;
using System.Data.Common;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Diagnostics;
using System.Data.Metadata.Edm;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    /// <summary>
    /// Manages <see cref="MemberPath"/>s of the members of the types stored in an extent.
    /// This is a bi-directional dictionary of <see cref="MemberPath"/>s to integer indexes and back.
    /// </summary>
    internal sealed class MemberProjectionIndex : InternalBase
    {
        #region Fields
        private readonly Dictionary<MemberPath, int> m_indexMap;
        private readonly List<MemberPath> m_members;
        #endregion

        #region Constructor/Factory
        /// <summary>
        /// Recursively generates <see cref="MemberPath"/>s for the members of the types stored in the <paramref name="extent"/>.
        /// </summary>
        internal static MemberProjectionIndex Create(EntitySetBase extent, EdmItemCollection edmItemCollection)
        {
            // We generate the indices for the projected slots as we traverse the metadata.
            MemberProjectionIndex index = new MemberProjectionIndex();
            GatherPartialSignature(index, edmItemCollection, new MemberPath(extent), false); // need not only keys
            return index;
        }

        /// <summary>
        /// Creates an empty index.
        /// </summary>
        private MemberProjectionIndex()
        {
            m_indexMap = new Dictionary<MemberPath, int>(MemberPath.EqualityComparer);
            m_members = new List<MemberPath>();
        }
        #endregion

        #region Properties
        internal int Count
        {
            get { return m_members.Count; }
        }

        internal MemberPath this[int index]
        {
            get { return m_members[index]; }
        }

        /// <summary>
        /// Returns the indexes of the key slots corresponding to fields in this for which IsPartOfKey is true.
        /// </summary>
        internal IEnumerable<int> KeySlots
        {
            get
            {
                List<int> result = new List<int>();
                for (int slotNum = 0; slotNum < Count; slotNum++)
                {
                    // We pass for numboolslots since we know that this is not a
                    // bool slot
                    if (this.IsKeySlot(slotNum, 0))
                    {
                        result.Add(slotNum);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Returns an enumeration of all members
        /// </summary>
        internal IEnumerable<MemberPath> Members
        {
            get { return m_members; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a non-negative index of the <paramref name="member"/> if found, otherwise -1.
        /// </summary>
        internal int IndexOf(MemberPath member)
        {
            int index;
            if (m_indexMap.TryGetValue(member, out index))
            {
                return index;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// If an index already exists for member, this is a no-op. Else creates the next index available for member and returns it.
        /// </summary>
        internal int CreateIndex(MemberPath member)
        {
            int index;
            if (false == m_indexMap.TryGetValue(member, out index))
            {
                index = m_indexMap.Count;
                m_indexMap[member] = index;
                m_members.Add(member);
            }
            return index;
        }

        /// <summary>
        /// Given the <paramref name="slotNum"/>, returns the output member path that this slot contributes/corresponds to in the extent view.
        /// If the slot corresponds to one of the boolean variables, returns null.
        /// </summary>
        internal MemberPath GetMemberPath(int slotNum, int numBoolSlots)
        {
            MemberPath result = this.IsBoolSlot(slotNum, numBoolSlots) ? null : this[slotNum];
            return result;
        }

        /// <summary>
        /// Given the index of a boolean variable (e.g., of from1), returns the slot number for that boolean in this.
        /// </summary>
        internal int BoolIndexToSlot(int boolIndex, int numBoolSlots)
        {
            // Booleans appear after the regular slots
            Debug.Assert(boolIndex >= 0 && boolIndex < numBoolSlots, "No such boolean in this node");
            return this.Count + boolIndex;
        }

        /// <summary>
        /// Given the <paramref name="slotNum"/> corresponding to a boolean slot, returns the cell number that the cell corresponds to.
        /// </summary>
        internal int SlotToBoolIndex(int slotNum, int numBoolSlots)
        {
            Debug.Assert(slotNum < this.Count + numBoolSlots && slotNum >= this.Count, "No such boolean slot");
            return slotNum - this.Count;
        }

        /// <summary>
        /// Returns true if <paramref name="slotNum"/> corresponds to a key slot in the output extent view.
        /// </summary>
        internal bool IsKeySlot(int slotNum, int numBoolSlots)
        {
            Debug.Assert(slotNum < this.Count + numBoolSlots, "No such slot in tree");
            return slotNum < this.Count && this[slotNum].IsPartOfKey;
        }

        /// <summary>
        /// Returns true if <paramref name="slotNum"/> corresponds to a bool slot and not a regular field.
        /// </summary>
        internal bool IsBoolSlot(int slotNum, int numBoolSlots)
        {
            Debug.Assert(slotNum < this.Count + numBoolSlots, "Boolean slot does not exist in tree");
            return slotNum >= this.Count;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append('<');
            StringUtil.ToCommaSeparatedString(builder, m_members);
            builder.Append('>');
        }
        #endregion

        #region Signature construction
        /// <summary>
        /// Starting at the <paramref name="member"/>, recursively generates <see cref="MemberPath"/>s for the fields embedded in it.
        /// </summary>
        /// <param name="member">corresponds to a value of an Entity or Complex or Association type</param>
        /// <param name="needKeysOnly">indicates whether we need to only collect members that are keys</param>
        private static void GatherPartialSignature(MemberProjectionIndex index, EdmItemCollection edmItemCollection, MemberPath member, bool needKeysOnly)
        {
            EdmType memberType = member.EdmType;
            ComplexType complexTypemember = memberType as ComplexType;
            Debug.Assert(complexTypemember != null ||
                         memberType is EntityType || // for entity sets
                         memberType is AssociationType || // For association sets
                         memberType is RefType, // for association ends
                         "GatherPartialSignature can be called only for complex types, entity sets, association ends");

            if (memberType is ComplexType && needKeysOnly)
            {
                // Check if the complex type needs to be traversed or not. If not, just return 
                // from here. Else we need to continue to the code below. Right now, we do not
                // allow keys inside complex types
                return;
            }

            // Make sure that this member is in the slot map before any of its embedded objects.
            index.CreateIndex(member);

            // Consider each possible type value -- each type value conributes to a tuple in the result.
            // For that possible type, add all the type members into the signature.
            foreach (EdmType possibleType in MetadataHelper.GetTypeAndSubtypesOf(memberType, edmItemCollection, false /*includeAbstractTypes*/))
            {
                StructuralType possibleStructuralType = possibleType as StructuralType;
                Debug.Assert(possibleStructuralType != null, "Non-structural subtype?");

                GatherSignatureFromTypeStructuralMembers(index, edmItemCollection, member, possibleStructuralType, needKeysOnly);
            }
        }

        /// <summary>
        /// Given the <paramref name="member"/> and one of its <paramref name="possibleType"/>s, determine the attributes that are relevant
        /// for this <paramref name="possibleType"/> and return a <see cref="MemberPath"/> signature corresponding to the <paramref name="possibleType"/> and the attributes.
        /// If <paramref name="needKeysOnly"/>=true, collect the key fields only.
        /// </summary>
        /// <param name="possibleType">the <paramref name="member"/>'s type or one of its subtypes</param>
        private static void GatherSignatureFromTypeStructuralMembers(MemberProjectionIndex index,
                                                                     EdmItemCollection edmItemCollection,
                                                                     MemberPath member, 
                                                                     StructuralType possibleType, 
                                                                     bool needKeysOnly)
        {
            // For each child member of this type, collect all the relevant scalar fields
            foreach (EdmMember structuralMember in Helper.GetAllStructuralMembers(possibleType))
            {
                if (MetadataHelper.IsNonRefSimpleMember(structuralMember))
                {
                    if (!needKeysOnly || MetadataHelper.IsPartOfEntityTypeKey(structuralMember))
                    {
                        MemberPath nonStructuredMember = new MemberPath(member, structuralMember);
                        // Note: scalarMember's parent has already been added to the projectedSlotMap
                        index.CreateIndex(nonStructuredMember);
                    }
                }
                else
                {
                    Debug.Assert(structuralMember.TypeUsage.EdmType is ComplexType ||
                                 structuralMember.TypeUsage.EdmType is RefType, // for association ends
                                 "Only non-scalars expected - complex types, association ends");

                    

                    MemberPath structuredMember = new MemberPath(member, structuralMember);
                    GatherPartialSignature(index, 
                                           edmItemCollection, 
                                           structuredMember,
                                           // Only keys are required for entities referenced by association ends of an association.
                                           needKeysOnly || Helper.IsAssociationEndMember(structuralMember));
                }
            }
        }
        #endregion
    }
}
