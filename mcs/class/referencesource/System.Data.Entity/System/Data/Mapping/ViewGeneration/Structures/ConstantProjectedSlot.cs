//---------------------------------------------------------------------
// <copyright file="ConstantProjectedSlot.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    /// <summary>
    /// A constant that can be projected in a cell query.
    /// </summary>
    internal sealed class ConstantProjectedSlot : ProjectedSlot
    {
        #region Constructors
        /// <summary>
        /// Creates a slot with constant value being <paramref name="value"/>.
        /// </summary>
        internal ConstantProjectedSlot(Constant value, MemberPath memberPath)
        {
            Debug.Assert(value != null);
            Debug.Assert(value.IsNotNull() == false, "Cannot store NotNull in a slot - NotNull is only for conditions");
            m_constant = value;
            m_memberPath = memberPath;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The actual value.
        /// </summary>
        private readonly Constant m_constant;
        private readonly MemberPath m_memberPath;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the value stored in this constant.
        /// </summary>
        internal Constant CellConstant
        {
            get { return m_constant; }
        }

        #endregion

        #region Methods
        internal override ProjectedSlot DeepQualify(CqlBlock block)
        {
            return this; // Nothing to create
        }

        internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
        {
            return m_constant.AsEsql(builder, outputMember, blockAlias);
        }

        internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
        {
            return m_constant.AsCqt(row, outputMember);
        }

        protected override bool IsEqualTo(ProjectedSlot right)
        {
            ConstantProjectedSlot rightSlot = right as ConstantProjectedSlot;
            if (rightSlot == null)
            {
                return false;
            }
            return Constant.EqualityComparer.Equals(m_constant, rightSlot.m_constant);
        }

        protected override int GetHash()
        {
            return Constant.EqualityComparer.GetHashCode(m_constant);
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            m_constant.ToCompactString(builder);
        }
        #endregion
    }
}
