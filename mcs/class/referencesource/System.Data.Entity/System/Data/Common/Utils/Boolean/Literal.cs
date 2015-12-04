//---------------------------------------------------------------------
// <copyright file="Literal.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace System.Data.Common.Utils.Boolean
{
    /// <summary>
    /// Represents a literal in a normal form expression of the form:
    /// 
    ///         Term
    /// 
    /// or
    /// 
    ///         !Term
    /// </summary>
    /// <typeparam name="T_Identifier"></typeparam>
    internal sealed class Literal<T_Identifier> : NormalFormNode<T_Identifier>,
        IEquatable<Literal<T_Identifier>>
    {
        private readonly TermExpr<T_Identifier> _term;
        private readonly bool _isTermPositive;

        /// <summary>
        /// Initialize a new literal.
        /// </summary>
        /// <param name="term">Term</param>
        /// <param name="isTermPositive">Sign of term</param>
        internal Literal(TermExpr<T_Identifier> term, bool isTermPositive)
            : base(isTermPositive ? (BoolExpr<T_Identifier>)term : (BoolExpr<T_Identifier>)new NotExpr<T_Identifier>(term))
        {
            Debug.Assert(null != term);
            _term = term;
            _isTermPositive = isTermPositive;
        }

        /// <summary>
        /// Gets literal term.
        /// </summary>
        internal TermExpr<T_Identifier> Term
        {
            get { return _term; }
        }

        /// <summary>
        /// Gets sign of term.
        /// </summary>
        internal bool IsTermPositive
        {
            get { return _isTermPositive; }
        }

        /// <summary>
        /// Creates a negated version of this literal.
        /// </summary>
        /// <returns>!this</returns>
        internal Literal<T_Identifier> MakeNegated()
        {
            return IdentifierService<T_Identifier>.Instance.NegateLiteral(this);
        }

        public override string ToString()
        {
            return StringUtil.FormatInvariant("{0}{1}",
                _isTermPositive ? String.Empty : "!",
                _term);
        }

        public override bool Equals(object obj)
        {
            Debug.Fail("use typed Equals");
            return Equals(obj as Literal<T_Identifier>);
        }

        public bool Equals(Literal<T_Identifier> other)
        {
            return null != other &&
                other._isTermPositive == _isTermPositive &&
                other._term.Equals(_term);
        }

        public override int GetHashCode()
        {
            return _term.GetHashCode();
        }
    }
}
