//---------------------------------------------------------------------
// <copyright file="MemberProjectedSlot.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Data.Metadata.Edm;
using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Mapping.ViewGeneration.Utils;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    /// <summary>
    /// A wrapper around MemberPath that allows members to be marked as ProjectedSlots.
    /// </summary>
    internal sealed class MemberProjectedSlot : ProjectedSlot
    {
        #region Constructor
        /// <summary>
        /// Creates a projected slot that references the relevant celltree node.
        /// </summary>
        internal MemberProjectedSlot(MemberPath node)
        {
            m_memberPath = node;
        }
        #endregion

        #region Fields
        private readonly MemberPath m_memberPath;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the full metadata path from the root extent to this node, e.g., Person.Adrs.zip
        /// </summary>
        internal MemberPath MemberPath
        {
            get { return m_memberPath; }
        }
        #endregion

        #region Methods
        internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
        {
            TypeUsage outputMemberStoreTypeUsage;
            if (NeedToCastCqlValue(outputMember, out outputMemberStoreTypeUsage))
            {
                builder.Append("CAST(");
                m_memberPath.AsEsql(builder, blockAlias);
                builder.Append(" AS ");
                CqlWriter.AppendEscapedTypeName(builder, outputMemberStoreTypeUsage.EdmType);
                builder.Append(')');
            }
            else
            {
                m_memberPath.AsEsql(builder, blockAlias);
            }
            return builder;
        }

        internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
        {
            DbExpression cqt = m_memberPath.AsCqt(row);

            TypeUsage outputMemberTypeUsage;
            if (NeedToCastCqlValue(outputMember, out outputMemberTypeUsage))
            {
                cqt = cqt.CastTo(outputMemberTypeUsage);
            }

            return cqt;
        }

        /// <summary>
        /// True iff <see cref=" m_memberPath"/> and <paramref name="outputMember"/> types do not match,
        /// We assume that the mapping loader has already checked that the casts are ok and emitted warnings.
        /// </summary>
        private bool NeedToCastCqlValue(MemberPath outputMember, out TypeUsage outputMemberTypeUsage)
        {
            TypeUsage memberPathTypeUsage = Helper.GetModelTypeUsage(m_memberPath.LeafEdmMember);
            outputMemberTypeUsage = Helper.GetModelTypeUsage(outputMember.LeafEdmMember);
            return !memberPathTypeUsage.EdmType.Equals(outputMemberTypeUsage.EdmType);
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            m_memberPath.ToCompactString(builder);
        }

        internal string ToUserString()
        {
            return m_memberPath.PathToString(false);
        }

        protected override bool IsEqualTo(ProjectedSlot right)
        {
            MemberProjectedSlot rightSlot = right as MemberProjectedSlot;
            if (rightSlot == null)
            {
                return false;
            }
            // We want equality of the paths
            return MemberPath.EqualityComparer.Equals(m_memberPath, rightSlot.m_memberPath);
        }

        protected override int GetHash()
        {
            return MemberPath.EqualityComparer.GetHashCode(m_memberPath);
        }

        /// <summary>
        /// Given a slot and the new mapping, returns the corresponding new slot.
        /// </summary>
        internal MemberProjectedSlot RemapSlot(Dictionary<MemberPath, MemberPath> remap)
        {
            MemberPath remappedNode = null;
            if (remap.TryGetValue(MemberPath, out remappedNode))
            {
                return new MemberProjectedSlot(remappedNode);
            }
            else
            {
                return new MemberProjectedSlot(MemberPath);
            }
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Given the <paramref name="prefix"/>, determines the slots in <paramref name="slots"/> that correspond to the entity key for the entity set or the
        /// association set end. Returns the list of slots.  Returns null if even one of the key slots is not present in slots.
        /// </summary>
        /// <param name="prefix">corresponds to an entity set or an association end</param>
        internal static List<MemberProjectedSlot> GetKeySlots(IEnumerable<MemberProjectedSlot> slots, MemberPath prefix)
        {
            // Get the entity type of the hosted end or entity set
            EntitySet entitySet = prefix.EntitySet;
            Debug.Assert(entitySet != null, "Prefix must have associated entity set");

            List<ExtentKey> keys = ExtentKey.GetKeysForEntityType(prefix, entitySet.ElementType);
            Debug.Assert(keys.Count > 0, "No keys for entity?");
            Debug.Assert(keys.Count == 1, "Currently, we only support primary keys");
            // Get the slots for the key
            List<MemberProjectedSlot> keySlots = GetSlots(slots, keys[0].KeyFields);
            return keySlots;
        }

        /// <summary>
        /// Searches for members in <paramref name="slots"/> and returns the corresponding slots in the same order as present in
        /// <paramref name="members"/>. Returns null if even one member is not present in slots.
        /// </summary>
        internal static List<MemberProjectedSlot> GetSlots(IEnumerable<MemberProjectedSlot> slots, IEnumerable<MemberPath> members)
        {
            List<MemberProjectedSlot> result = new List<MemberProjectedSlot>();
            foreach (MemberPath member in members)
            {
                MemberProjectedSlot slot = GetSlotForMember(Helpers.AsSuperTypeList<MemberProjectedSlot, ProjectedSlot>(slots), member);
                if (slot == null)
                {
                    return null;
                }
                result.Add(slot);
            }
            return result;
        }

        /// <summary>
        /// Searches for <paramref name="member"/> in <paramref name="slots"/> and returns the corresponding slot. If none is found, returns null.
        /// </summary>
        internal static MemberProjectedSlot GetSlotForMember(IEnumerable<ProjectedSlot> slots, MemberPath member)
        {
            foreach (MemberProjectedSlot slot in slots)
            {
                if (MemberPath.EqualityComparer.Equals(slot.MemberPath, member))
                {
                    return slot;
                }
            }
            return null;
        }
        #endregion
    }
}
