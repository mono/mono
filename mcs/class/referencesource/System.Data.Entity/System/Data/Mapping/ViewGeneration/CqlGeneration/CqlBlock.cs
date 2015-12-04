//---------------------------------------------------------------------
// <copyright file="CqlBlock.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Linq;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Collections.Generic;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;

namespace System.Data.Mapping.ViewGeneration.CqlGeneration
{
    /// <summary>
    /// A class that holds an expression of the form "(SELECT .. FROM .. WHERE) AS alias".
    /// Essentially, it allows generating Cql query in a localized manner, i.e., all global decisions about nulls, constants,
    /// case statements, etc have already been made.
    /// </summary>
    internal abstract class CqlBlock : InternalBase
    {
        /// <summary>
        /// Initializes a <see cref="CqlBlock"/> with the SELECT (<paramref name="slotInfos"/>), FROM (<paramref name="children"/>), 
        /// WHERE (<paramref name="whereClause"/>), AS (<paramref name="blockAliasNum"/>).
        /// </summary>
        protected CqlBlock(SlotInfo[] slotInfos, List<CqlBlock> children, BoolExpression whereClause, CqlIdentifiers identifiers, int blockAliasNum)
        {
            m_slots = new ReadOnlyCollection<SlotInfo>(slotInfos);
            m_children = new ReadOnlyCollection<CqlBlock>(children);
            m_whereClause = whereClause;
            m_blockAlias = identifiers.GetBlockAlias(blockAliasNum);
        }

        #region Fields
        /// <summary>
        /// Essentially, SELECT. May be replaced with another collection after block construction.
        /// </summary>
        private ReadOnlyCollection<SlotInfo> m_slots;
        /// <summary>
        /// FROM inputs.
        /// </summary>
        private readonly ReadOnlyCollection<CqlBlock> m_children;
        /// <summary>
        /// WHERER.
        /// </summary>
        private readonly BoolExpression m_whereClause;
        /// <summary>
        /// Alias of the whole block for cql generation.
        /// </summary>
        private readonly string m_blockAlias;
        /// <summary>
        /// See <see cref="JoinTreeContext"/> for more info.
        /// </summary>
        private JoinTreeContext m_joinTreeContext;
        #endregion

        #region Properties
        /// <summary>
        /// Returns all the slots for this block (SELECT).
        /// </summary>
        internal ReadOnlyCollection<SlotInfo> Slots
        {
            get { return m_slots; }
            set { m_slots = value; }
        }

        /// <summary>
        /// Returns all the child (input) blocks of this block (FROM).
        /// </summary>
        protected ReadOnlyCollection<CqlBlock> Children
        {
            get { return m_children; }
        }

        /// <summary>
        /// Returns the where clause of this block (WHERE).
        /// </summary>
        protected BoolExpression WhereClause
        {
            get { return m_whereClause; }
        }

        /// <summary>
        /// Returns an alias for this block that can be used for "AS".
        /// </summary>
        internal string CqlAlias
        {
            get { return m_blockAlias; }
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Returns a string corresponding to the eSQL representation of this block (and its children below).
        /// </summary>
        internal abstract StringBuilder AsEsql(StringBuilder builder, bool isTopLevel, int indentLevel);

        /// <summary>
        /// Returns a string corresponding to the CQT representation of this block (and its children below).
        /// </summary>
        internal abstract DbExpression AsCqt(bool isTopLevel);
        #endregion

        #region Methods
        /// <summary>
        /// For the given <paramref name="slotNum"/> creates a <see cref="QualifiedSlot"/> qualified with <see cref="CqlAlias"/> of the current block:
        /// "<see cref="CqlAlias"/>.slot_alias"
        /// </summary>
        internal QualifiedSlot QualifySlotWithBlockAlias(int slotNum)
        {
            Debug.Assert(this.IsProjected(slotNum), StringUtil.FormatInvariant("Slot {0} that is to be qualified with the block alias is not projected in this block", slotNum));
            var slotInfo = m_slots[slotNum];
            return new QualifiedSlot(this, slotInfo.SlotValue);
        }

        internal ProjectedSlot SlotValue(int slotNum)
        {
            Debug.Assert(slotNum < m_slots.Count, "Slotnum too high");
            return m_slots[slotNum].SlotValue;
        }

        internal MemberPath MemberPath(int slotNum)
        {
            Debug.Assert(slotNum < m_slots.Count, "Slotnum too high");
            return m_slots[slotNum].OutputMember;
        }

        /// <summary>
        /// Returns true iff <paramref name="slotNum"/> is being projected by this block.
        /// </summary>
        internal bool IsProjected(int slotNum)
        {
            Debug.Assert(slotNum < m_slots.Count, "Slotnum too high");
            return m_slots[slotNum].IsProjected;
        }

        /// <summary>
        /// Generates "A, B, C, ..." for all the slots in the block.
        /// </summary>
        protected void GenerateProjectionEsql(StringBuilder builder, string blockAlias, bool addNewLineAfterEachSlot, int indentLevel, bool isTopLevel)
        {
            bool isFirst = true;
            foreach (SlotInfo slotInfo in Slots)
            {
                if (false == slotInfo.IsRequiredByParent)
                {
                    // Ignore slots that are not needed
                    continue;
                }
                if (isFirst == false)
                {
                    builder.Append(", ");
                }

                if (addNewLineAfterEachSlot)
                {
                    StringUtil.IndentNewLine(builder, indentLevel + 1);
                }

                slotInfo.AsEsql(builder, blockAlias, indentLevel);

                // Print the field alias for complex expressions that don't produce default alias.
                // Don't print alias for qualified fields as they reproduce their alias.
                // Don't print alias if it's a top level query using SELECT VALUE.
                if (!isTopLevel && (!(slotInfo.SlotValue is QualifiedSlot) || slotInfo.IsEnforcedNotNull))
                {
                    builder.Append(" AS ")
                           .Append(slotInfo.CqlFieldAlias);
                }
                isFirst = false;
            }
            if (addNewLineAfterEachSlot)
            {
                StringUtil.IndentNewLine(builder, indentLevel);
            }
        }

        /// <summary>
        /// Generates "NewRow(A, B, C, ...)" for all the slots in the block.
        /// If <paramref name="isTopLevel"/>=true then generates "A" for the only slot that is marked as <see cref="SlotInfo.IsRequiredByParent"/>.
        /// </summary>
        protected DbExpression GenerateProjectionCqt(DbExpression row, bool isTopLevel)
        {
            if (isTopLevel)
            {
                Debug.Assert(this.Slots.Where(slot => slot.IsRequiredByParent).Count() == 1, "Top level projection must project only one slot.");
                return this.Slots.Where(slot => slot.IsRequiredByParent).Single().AsCqt(row);
            }
            else
            {
                return DbExpressionBuilder.NewRow(
                    this.Slots.Where(slot => slot.IsRequiredByParent).Select(slot => new KeyValuePair<string, DbExpression>(slot.CqlFieldAlias, slot.AsCqt(row))));
            }
        }

        /// <summary>
        /// Initializes context positioning in the join tree that owns the <see cref="CqlBlock"/>.
        /// For more info see <see cref="JoinTreeContext"/>.
        /// </summary>
        internal void SetJoinTreeContext(IList<string> parentQualifiers, string leafQualifier)
        {
            Debug.Assert(m_joinTreeContext == null, "Join tree context is already set.");
            m_joinTreeContext = new JoinTreeContext(parentQualifiers, leafQualifier);
        }

        /// <summary>
        /// Searches the input <paramref name="row"/> for the property that represents the current <see cref="CqlBlock"/>.
        /// In all cases except JOIN, the <paramref name="row"/> is returned as is.
        /// In case of JOIN, <paramref name="row"/>.JoinVarX.JoinVarY...blockVar is returned.
        /// See <see cref="SetJoinTreeContext"/> for more info.
        /// </summary>
        internal DbExpression GetInput(DbExpression row)
        {
            return m_joinTreeContext != null ? m_joinTreeContext.FindInput(row) : row;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            for (int i = 0; i < m_slots.Count; i++)
            {
                StringUtil.FormatStringBuilder(builder, "{0}: ", i);
                m_slots[i].ToCompactString(builder);
                builder.Append(' ');
            }
            m_whereClause.ToCompactString(builder);
        }
        #endregion

        #region JoinTreeContext
        /// <summary>
        /// The class represents a position of a <see cref="CqlBlock"/> in a join tree.
        /// It is expected that the join tree is left-recursive (not balanced) and looks like this:
        /// 
        ///                     ___J___
        ///                    /       \
        ///                 L3/         \R3
        ///                  /           \
        ///               __J__           \
        ///              /     \           \
        ///           L2/       \R2         \
        ///            /         \           \
        ///          _J_          \           \
        ///         /   \          \           \
        ///      L1/     \R1        \           \
        ///       /       \          \           \
        /// CqlBlock1   CqlBlock2   CqlBlock3   CqlBlock4
        /// 
        /// Example of <see cref="JoinTreeContext"/>s for the <see cref="CqlBlock"/>s:
        /// block#   m_parentQualifiers   m_indexInParentQualifiers   m_leafQualifier    FindInput(row) = ...
        ///   1          (L2, L3)                    0                      L1             row.(L3.L2).L1
        ///   2          (L2, L3)                    0                      R1             row.(L3.L2).R1
        ///   3          (L2, L3)                    1                      R2             row.(L3).R2
        ///   4          (L2, L3)                    2                      R3             row.().R3
        /// 
        /// </summary>
        private sealed class JoinTreeContext
        {
            internal JoinTreeContext(IList<string> parentQualifiers, string leafQualifier)
            {
                Debug.Assert(parentQualifiers != null, "parentQualifiers != null");
                Debug.Assert(leafQualifier != null, "leafQualifier != null");

                m_parentQualifiers = parentQualifiers;
                m_indexInParentQualifiers = parentQualifiers.Count;
                m_leafQualifier = leafQualifier;
            }

            private readonly IList<string> m_parentQualifiers;
            private readonly int m_indexInParentQualifiers;
            private readonly string m_leafQualifier;

            internal DbExpression FindInput(DbExpression row)
            {
                DbExpression cqt = row;
                for (int i = m_parentQualifiers.Count - 1; i >= m_indexInParentQualifiers; --i)
                {
                    cqt = cqt.Property(m_parentQualifiers[i]);
                }
                return cqt.Property(m_leafQualifier);
            }
        }
        #endregion
    }
}
