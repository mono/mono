//---------------------------------------------------------------------
// <copyright file="CaseStatementProjectedSlot.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Text;
using System.Collections.Generic;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    /// <summary>
    /// This class is just a wrapper over case statements so that we don't pollute the <see cref="CaseStatement"/> class itself.
    /// </summary>
    internal sealed class CaseStatementProjectedSlot : ProjectedSlot
    {
        #region Constructor
        /// <summary>
        /// Creates a slot for <paramref name="statement"/>.
        /// </summary>
        internal CaseStatementProjectedSlot(CaseStatement statement, IEnumerable<WithRelationship> withRelationships)
        {
            m_caseStatement = statement;
            m_withRelationships = withRelationships;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The actual case statement.
        /// </summary>
        private readonly CaseStatement m_caseStatement;
        private readonly IEnumerable<WithRelationship> m_withRelationships;
        #endregion

        #region Methods
        /// <summary>
        /// Creates new <see cref="ProjectedSlot"/> that is qualified with <paramref name="block"/>.CqlAlias.
        /// If current slot is composite (such as <see cref="CaseStatementProjectedSlot"/>, then this method recursively qualifies all parts
        /// and returns a new deeply qualified slot (as opposed to <see cref="CqlBlock.QualifySlotWithBlockAlias"/>).
        /// </summary>
        internal override ProjectedSlot DeepQualify(CqlBlock block)
        {
            CaseStatement newStatement = m_caseStatement.DeepQualify(block);
            return new CaseStatementProjectedSlot(newStatement, null);
        }

        internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
        {
            m_caseStatement.AsEsql(builder, m_withRelationships, blockAlias, indentLevel);
            return builder;
        }

        internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
        {
            return m_caseStatement.AsCqt(row, m_withRelationships);
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            m_caseStatement.ToCompactString(builder);
        }
        #endregion
    }
}
