//---------------------------------------------------------------------
// <copyright file="SlotInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Text;
using System.Diagnostics;

namespace System.Data.Mapping.ViewGeneration.CqlGeneration
{
    /// <summary>
    /// A class that keeps track of slot information in a <see cref="CqlBlock"/>.
    /// </summary>
    internal sealed class SlotInfo : InternalBase
    {
        #region Constructor
        /// <summary>
        /// Creates a <see cref="SlotInfo"/> for a <see cref="CqlBlock"/> X with information about whether this slot is needed by X's parent
        /// (<paramref name="isRequiredByParent"/>), whether X projects it (<paramref name="isProjected"/>) along with the slot value (<paramref name="slotValue"/>) and 
        /// the output member path (<paramref name="outputMember"/> (for regular/non-boolean slots) for the slot.
        /// </summary>
        internal SlotInfo(bool isRequiredByParent, bool isProjected, ProjectedSlot slotValue, MemberPath outputMember)
            : this(isRequiredByParent, isProjected, slotValue, outputMember, false /* enforceNotNull */)
        { }

        /// <summary>
        /// Creates a <see cref="SlotInfo"/> for a <see cref="CqlBlock"/> X with information about whether this slot is needed by X's parent
        /// (<paramref name="isRequiredByParent"/>), whether X projects it (<paramref name="isProjected"/>) along with the slot value (<paramref name="slotValue"/>) and 
        /// the output member path (<paramref name="outputMember"/> (for regular/non-boolean slots) for the slot.
        /// </summary>
        /// <param name="enforceNotNull">We need to ensure that _from variables are never null since view generation uses 2-valued boolean logic.
        /// If <paramref name="enforceNotNull"/>=true, the generated Cql adds a condition (AND <paramref name="slotValue"/> NOT NULL).
        /// This flag is used only for boolean slots.</param>
        internal SlotInfo(bool isRequiredByParent, bool isProjected, ProjectedSlot slotValue, MemberPath outputMember, bool enforceNotNull)
        {
            m_isRequiredByParent = isRequiredByParent;
            m_isProjected = isProjected;
            m_slotValue = slotValue;
            m_outputMember = outputMember;
            m_enforceNotNull = enforceNotNull;
            Debug.Assert(false == m_isRequiredByParent || m_slotValue != null, "Required slots cannot be null");
            Debug.Assert(m_slotValue is QualifiedSlot ||
                         (m_slotValue == null && m_outputMember == null) || // unused boolean slot
                         (m_slotValue is BooleanProjectedSlot) == (m_outputMember == null),
                         "If slot is boolean slot, there is no member path for it and vice-versa");
        }
        #endregion

        #region Fields
        /// <summary>
        /// If slot is required by the parent. Can be reset to false in <see cref="ResetIsRequiredByParent"/> method.
        /// </summary>
        private bool m_isRequiredByParent;
        /// <summary>
        /// If the node is capable of projecting this slot.
        /// </summary>
        private readonly bool m_isProjected;
        /// <summary>
        /// The slot represented by this <see cref="SlotInfo"/>.
        /// </summary>
        private readonly ProjectedSlot m_slotValue;
        /// <summary>
        /// The output member path of this slot.
        /// </summary>
        private readonly MemberPath m_outputMember;
        /// <summary>
        /// Whether to add AND NOT NULL to Cql.
        /// </summary>
        private readonly bool m_enforceNotNull;
        #endregion

        #region Properties
        /// <summary>
        /// Returns true iff this slot is required by the <see cref="CqlBlock"/>'s parent.
        /// Can be reset to false by calling <see cref="ResetIsRequiredByParent"/> method.
        /// </summary>
        internal bool IsRequiredByParent
        {
            get { return m_isRequiredByParent; }
        }

        /// <summary>
        /// Returns true iff this slot is projected by this <see cref="CqlBlock"/>.
        /// </summary>
        internal bool IsProjected
        {
            get { return m_isProjected; }
        }

        /// <summary>
        /// Returns the output memberpath of this slot
        /// </summary>
        internal MemberPath OutputMember
        {
            get { return m_outputMember; }
        }

        /// <summary>
        /// Returns the slot value corresponfing to this object.
        /// </summary>
        internal ProjectedSlot SlotValue
        {
            get { return m_slotValue; }
        }

        /// <summary>
        /// Returns the Cql alias for this slot, e.g., "CPerson1_Pid", "_from0", etc
        /// </summary>
        internal string CqlFieldAlias
        {
            get
            {
                return m_slotValue != null ? m_slotValue.GetCqlFieldAlias(m_outputMember) : null;
            }
        }

        /// <summary>
        /// Returns true if Cql generated for the slot needs to have an extra AND IS NOT NULL condition.
        /// </summary>
        internal bool IsEnforcedNotNull
        {
            get { return m_enforceNotNull; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the <see cref="IsRequiredByParent"/> to false.
        /// Note we don't have a setter because we don't want people to set this field to true after the object has been created.
        /// </summary>
        internal void ResetIsRequiredByParent()
        {
            m_isRequiredByParent = false;
        }

        /// <summary>
        /// Generates eSQL representation of the slot. For different slots, the result is different, e.g., "_from0", "CPerson1.pid", "TREAT(....)".
        /// </summary>
        internal StringBuilder AsEsql(StringBuilder builder, string blockAlias, int indentLevel)
        {
            if (m_enforceNotNull)
            {
                builder.Append('(');
                m_slotValue.AsEsql(builder, m_outputMember, blockAlias, indentLevel);
                builder.Append(" AND ");
                m_slotValue.AsEsql(builder, m_outputMember, blockAlias, indentLevel);
                builder.Append(" IS NOT NULL)");
            }
            else
            {
                m_slotValue.AsEsql(builder, m_outputMember, blockAlias, indentLevel);
            }
            return builder;
        }

        /// <summary>
        /// Generates CQT representation of the slot.
        /// </summary>
        internal DbExpression AsCqt(DbExpression row)
        {
            DbExpression cqt = m_slotValue.AsCqt(row, m_outputMember);
            if (m_enforceNotNull)
            {
                cqt = cqt.And(cqt.IsNull().Not());
            }
            return cqt;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            if (m_slotValue != null)
            {
                builder.Append(CqlFieldAlias);
            }
        }
        #endregion
    }
}
