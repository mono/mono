//---------------------------------------------------------------------
// <copyright file="BooleanProjectedSlot.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Mapping.ViewGeneration.Structures;
using System.Text;
using System.Diagnostics;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;

namespace System.Data.Mapping.ViewGeneration.CqlGeneration
{
    /// <summary>
    /// This class represents slots for expressions over boolean variables, e.g., _from0, _from1, etc
    /// </summary>
    internal sealed class BooleanProjectedSlot : ProjectedSlot
    {
        #region Constructor
        /// <summary>
        /// Creates a boolean slot for expression that comes from originalCellNum, i.e., 
        /// the value of the slot is <paramref name="expr"/> and the name is "_from{<paramref name="originalCellNum"/>}", e.g., _from2
        /// </summary>
        internal BooleanProjectedSlot(BoolExpression expr, CqlIdentifiers identifiers, int originalCellNum)
        {
            m_expr = expr;
            m_originalCell = new CellIdBoolean(identifiers, originalCellNum);

            Debug.Assert(!(expr.AsLiteral is CellIdBoolean) ||
                         BoolLiteral.EqualityComparer.Equals((CellIdBoolean)expr.AsLiteral, m_originalCell), "Cellid boolean for the slot and cell number disagree");
        }
        #endregion

        #region Fields
        /// <summary>
        /// The actual value of the slot - could be <see cref="CellIdBoolean"/>!
        /// </summary>
        private readonly BoolExpression m_expr;
        /// <summary>
        /// A boolean corresponding to the original cell number (_from0)
        /// </summary>
        private readonly CellIdBoolean m_originalCell;
        #endregion

        #region Methods
        /// <summary>
        /// Returns "_from0", "_from1" etc. <paramref name="outputMember"/> is ignored.
        /// </summary>
        internal override string GetCqlFieldAlias(MemberPath outputMember)
        {
            return m_originalCell.SlotName;
        }

        internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
        {
            if (m_expr.IsTrue || m_expr.IsFalse)
            {
                // No Case statement for TRUE and FALSE
                m_expr.AsEsql(builder, blockAlias);
            }
            else
            {
                // Produce "CASE WHEN boolExpr THEN True ELSE False END" in order to enforce the two-state boolean logic:
                // if boolExpr returns the boolean Unknown, it gets converted to boolean False.
                builder.Append("CASE WHEN ");
                m_expr.AsEsql(builder, blockAlias);
                builder.Append(" THEN True ELSE False END");
            }
            return builder;
        }

        internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
        {
            if (m_expr.IsTrue || m_expr.IsFalse)
            {
                return m_expr.AsCqt(row);
            }
            else
            {
                // Produce "CASE WHEN boolExpr THEN True ELSE False END" in order to enforce the two-state boolean logic:
                // if boolExpr returns the boolean Unknown, it gets converted to boolean False.
                return DbExpressionBuilder.Case(new DbExpression[] { m_expr.AsCqt(row) }, new DbExpression[] { DbExpressionBuilder.True }, DbExpressionBuilder.False);
            }
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            StringUtil.FormatStringBuilder(builder, "<{0}, ", m_originalCell.SlotName);
            m_expr.ToCompactString(builder);
            builder.Append('>');
        }
        #endregion
    }
}
