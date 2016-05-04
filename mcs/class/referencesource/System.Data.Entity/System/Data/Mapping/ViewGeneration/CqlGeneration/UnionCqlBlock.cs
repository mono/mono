//---------------------------------------------------------------------
// <copyright file="UnionCqlBlock.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Diagnostics;

namespace System.Data.Mapping.ViewGeneration.CqlGeneration
{
    /// <summary>
    /// Represents Union nodes in the <see cref="CqlBlock"/> tree.
    /// </summary>
    internal sealed class UnionCqlBlock : CqlBlock
    {
        #region Constructor
        /// <summary>
        /// Creates a union block with SELECT (<paramref name="slotInfos"/>), FROM (<paramref name="children"/>), WHERE (true), AS (<paramref name="blockAliasNum"/>).
        /// </summary>
        internal UnionCqlBlock(SlotInfo[] slotInfos, List<CqlBlock> children, CqlIdentifiers identifiers, int blockAliasNum)
            : base(slotInfos, children, BoolExpression.True, identifiers, blockAliasNum)
        { }
        #endregion

        #region Methods
        internal override StringBuilder AsEsql(StringBuilder builder, bool isTopLevel, int indentLevel)
        {
            Debug.Assert(this.Children.Count > 0, "UnionCqlBlock: Children collection must not be empty");

            // Simply get the Cql versions of the children and add the union operator between them.
            bool isFirst = true;
            foreach (CqlBlock child in Children)
            {
                if (false == isFirst)
                {
                    StringUtil.IndentNewLine(builder, indentLevel + 1);
                    builder.Append(OpCellTreeNode.OpToEsql(CellTreeOpType.Union));
                }
                isFirst = false;

                builder.Append(" (");
                child.AsEsql(builder, isTopLevel, indentLevel + 1);
                builder.Append(')');
            }
            return builder;
        }

        internal override DbExpression AsCqt(bool isTopLevel)
        {
            Debug.Assert(this.Children.Count > 0, "UnionCqlBlock: Children collection must not be empty");
            DbExpression cqt = this.Children[0].AsCqt(isTopLevel);
            for (int i = 1; i < this.Children.Count; ++i)
            {
                cqt = cqt.UnionAll(this.Children[i].AsCqt(isTopLevel));
            }
            return cqt;
        }
        #endregion
    }
}
