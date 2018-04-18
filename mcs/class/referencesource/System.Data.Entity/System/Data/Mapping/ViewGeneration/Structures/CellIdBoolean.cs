//---------------------------------------------------------------------
// <copyright file="CellIdBoolean.cs" company="Microsoft">
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
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Mapping.ViewGeneration.CqlGeneration;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Wraps from0, from1, etc. boolean fields that identify the source of tuples (# of respective cell query) in the view statements.
    /// </summary>
    internal class CellIdBoolean : TrueFalseLiteral
    {
        #region Constructor
        /// <summary>
        /// Creates a boolean expression for the variable name specified by <paramref name="index"/>, e.g., 0 results in from0, 1 into from1.
        /// </summary>
        internal CellIdBoolean(CqlIdentifiers identifiers, int index)
        {
            Debug.Assert(index >= 0);
            m_index = index;
            m_slotName = identifiers.GetFromVariable(index);
        }
        #endregion

        #region Fields
        /// <summary>
        /// e.g., from0, from1.
        /// </summary>
        private readonly int m_index;
        private readonly string m_slotName;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the slotName corresponding to this, ie., _from0 etc.
        /// </summary>
        internal string SlotName
        {
            get { return m_slotName; }
        }
        #endregion

        #region BoolLiteral members
        internal override StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            // Get e.g., T2._from1 using the table alias
            string qualifiedName = CqlWriter.GetQualifiedName(blockAlias, SlotName);
            builder.Append(qualifiedName);
            return builder;
        }

        internal override DbExpression AsCqt(DbExpression row, bool skipIsNotNull)
        {
            // Get e.g., row._from1
            return row.Property(SlotName);
        }

        internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            return AsEsql(builder, blockAlias, skipIsNotNull);
        }

        internal override StringBuilder AsNegatedUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            builder.Append("NOT(");
            builder = AsUserString(builder, blockAlias, skipIsNotNull);
            builder.Append(")");
            return builder;
        }

        internal override void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
        {
            // The slot corresponding to from1, etc
            int numBoolSlots = requiredSlots.Length - projectedSlotMap.Count;
            int slotNum = projectedSlotMap.BoolIndexToSlot(m_index, numBoolSlots);
            requiredSlots[slotNum] = true;
        }

        protected override bool IsEqualTo(BoolLiteral right)
        {
            CellIdBoolean rightBoolean = right as CellIdBoolean;
            if (rightBoolean == null)
            {
                return false;
            }
            return m_index == rightBoolean.m_index;
        }

        public override int GetHashCode()
        {
            return m_index.GetHashCode();
        }

        internal override BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap)
        {
            return this;
        }
        #endregion

        #region Other Methods
        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append(SlotName);
        }
        #endregion
    }
}
