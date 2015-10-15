//---------------------------------------------------------------------
// <copyright file="RoleBoolean.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Denotes the fact that the key of the current tuple comes from a specific extent, or association role.
    /// </summary>
    internal sealed class RoleBoolean : TrueFalseLiteral
    {
        #region Constructor
        internal RoleBoolean(EntitySetBase extent)
        {
            m_metadataItem = extent;
        }
        internal RoleBoolean(AssociationSetEnd end)
        {
            m_metadataItem = end;
        }
        #endregion

        #region Fields
        private readonly MetadataItem m_metadataItem;
        #endregion

        #region BoolLiteral members
        /// <summary>
        /// Not supported in this class.
        /// </summary>
        internal override StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            Debug.Fail("Should not be called.");
            return null; // To keep the compiler happy
        }

        /// <summary>
        /// Not supported in this class.
        /// </summary>
        internal override DbExpression AsCqt(DbExpression row, bool skipIsNotNull)
        {
            Debug.Fail("Should not be called.");
            return null; // To keep the compiler happy
        }

        internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            AssociationSetEnd end = m_metadataItem as AssociationSetEnd;
            if (end != null)
            {
                builder.Append(Strings.ViewGen_AssociationSet_AsUserString(blockAlias, end.Name, end.ParentAssociationSet));
            }
            else
            {
                builder.Append(Strings.ViewGen_EntitySet_AsUserString(blockAlias, m_metadataItem.ToString()));
            }
            return builder;
        }

        internal override StringBuilder AsNegatedUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            AssociationSetEnd end = m_metadataItem as AssociationSetEnd;
            if (end != null)
            {
                builder.Append(Strings.ViewGen_AssociationSet_AsUserString_Negated(blockAlias, end.Name, end.ParentAssociationSet));
            }
            else
            {
                builder.Append(Strings.ViewGen_EntitySet_AsUserString_Negated(blockAlias, m_metadataItem.ToString()));
            }
            return builder;
        }

        internal override void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
        {
            throw new NotImplementedException();
        }

        protected override bool IsEqualTo(BoolLiteral right)
        {
            RoleBoolean rightBoolean = right as RoleBoolean;
            if (rightBoolean == null)
            {
                return false;
            }
            return m_metadataItem == rightBoolean.m_metadataItem;
        }

        public override int GetHashCode()
        {
            return m_metadataItem.GetHashCode();
        }

        internal override BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap)
        {
            return this;
        }
        #endregion

        #region Other Methods
        internal override void ToCompactString(StringBuilder builder)
        {
            AssociationSetEnd end = m_metadataItem as AssociationSetEnd;
            if (end != null)
            {
                builder.Append("InEnd:" + end.ParentAssociationSet + "_" + end.Name);
            }
            else
            {
                builder.Append("InSet:" + m_metadataItem.ToString());
            }
        }
        #endregion
    }
}
