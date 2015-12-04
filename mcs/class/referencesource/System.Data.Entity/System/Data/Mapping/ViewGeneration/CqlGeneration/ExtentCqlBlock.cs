//---------------------------------------------------------------------
// <copyright file="ExtentCqlBlock.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Text;
using System.Collections.Generic;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Data.Metadata.Edm;

namespace System.Data.Mapping.ViewGeneration.CqlGeneration
{
    /// <summary>
    /// A class that represents leaf <see cref="CqlBlock"/>s in the <see cref="CqlBlock"/> tree.
    /// </summary>
    internal sealed class ExtentCqlBlock : CqlBlock
    {
        #region Constructors
        /// <summary>
        /// Creates an cql block representing the <paramref name="extent"/> (the FROM part).
        /// SELECT is given by <paramref name="slots"/>, WHERE by <paramref name="whereClause"/> and AS by <paramref name="blockAliasNum"/>.
        /// </summary>
        internal ExtentCqlBlock(EntitySetBase extent,
                                CellQuery.SelectDistinct selectDistinct,
                                SlotInfo[] slots,
                                BoolExpression whereClause,
                                CqlIdentifiers identifiers,
                                int blockAliasNum)
            : base(slots, EmptyChildren, whereClause, identifiers, blockAliasNum)
        {
            m_extent = extent;
            m_nodeTableAlias = identifiers.GetBlockAlias();
            m_selectDistinct = selectDistinct;
        }
        #endregion

        #region Fields
        private readonly EntitySetBase m_extent;
        private readonly string m_nodeTableAlias;
        private readonly CellQuery.SelectDistinct m_selectDistinct;
        private static readonly List<CqlBlock> EmptyChildren = new List<CqlBlock>();
        #endregion

        #region Methods
        internal override StringBuilder AsEsql(StringBuilder builder, bool isTopLevel, int indentLevel)
        {
            // The SELECT/DISTINCT part.
            StringUtil.IndentNewLine(builder, indentLevel);
            builder.Append("SELECT ");
            if (m_selectDistinct == CellQuery.SelectDistinct.Yes)
            {
                builder.Append("DISTINCT ");
            }
            GenerateProjectionEsql(builder, m_nodeTableAlias, true, indentLevel, isTopLevel);

            // Get the FROM part.
            builder.Append("FROM ");
            CqlWriter.AppendEscapedQualifiedName(builder, m_extent.EntityContainer.Name, m_extent.Name);
            builder.Append(" AS ").Append(m_nodeTableAlias);

            // Get the WHERE part only when the expression is not simply TRUE.
            if (!BoolExpression.EqualityComparer.Equals(this.WhereClause, BoolExpression.True))
            {
                StringUtil.IndentNewLine(builder, indentLevel);
                builder.Append("WHERE ");
                this.WhereClause.AsEsql(builder, m_nodeTableAlias);
            }

            return builder;
        }

        internal override DbExpression AsCqt(bool isTopLevel)
        {
            // Get the FROM part.
            DbExpression cqt = m_extent.Scan();

            // Get the WHERE part only when the expression is not simply TRUE.
            if (!BoolExpression.EqualityComparer.Equals(this.WhereClause, BoolExpression.True))
            {
                cqt = cqt.Where(row => this.WhereClause.AsCqt(row));
            }

            // The SELECT/DISTINCT part.
            cqt = cqt.Select(row => GenerateProjectionCqt(row, isTopLevel));
            if (m_selectDistinct == CellQuery.SelectDistinct.Yes)
            {
                cqt = cqt.Distinct();
            }

            return cqt;
        }
        #endregion
    }
}
