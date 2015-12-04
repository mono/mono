//---------------------------------------------------------------------
// <copyright file="Clause.cs" company="Microsoft">
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
using System.Linq;

namespace System.Data.Common.Utils.Boolean
{
    /// <summary>
    /// Base class for clauses, which are (constrained) combinations of literals.
    /// </summary>
    /// <typeparam name="T_Identifier">Type of normal form literal.</typeparam>
    internal abstract class Clause<T_Identifier> : NormalFormNode<T_Identifier>
    {
        private readonly Set<Literal<T_Identifier>> _literals;
        private readonly int _hashCode;

        /// <summary>
        /// Initialize a new clause.
        /// </summary>
        /// <param name="literals">Literals contained in the clause.</param>
        /// <param name="treeType">Type of expression tree to produce from literals.</param>
        protected Clause(Set<Literal<T_Identifier>> literals, ExprType treeType)
            : base(ConvertLiteralsToExpr(literals, treeType))
        {
            _literals = literals.AsReadOnly();
            _hashCode = _literals.GetElementsHashCode();
        }

        /// <summary>
        /// Gets the literals contained in this clause.
        /// </summary>
        internal Set<Literal<T_Identifier>> Literals
        {
            get { return _literals; }
        }

        // Given a collection of literals and a tree type, returns an expression of the given type.
        private static BoolExpr<T_Identifier> ConvertLiteralsToExpr(Set<Literal<T_Identifier>> literals, ExprType treeType)
        {
            bool isAnd = ExprType.And == treeType;
            Debug.Assert(isAnd || ExprType.Or == treeType);

            IEnumerable<BoolExpr<T_Identifier>> literalExpressions = literals.Select(
                new Func<Literal<T_Identifier>, BoolExpr<T_Identifier>>(ConvertLiteralToExpression));

            if (isAnd)
            {
                return new AndExpr<T_Identifier>(literalExpressions);
            }
            else
            {
                return new OrExpr<T_Identifier>(literalExpressions);
            }
        }

        // Given a literal, returns its logical equivalent expression.
        private static BoolExpr<T_Identifier> ConvertLiteralToExpression(Literal<T_Identifier> literal)
        {
            return literal.Expr;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Clause{");
            builder.Append(_literals);
            return builder.Append("}").ToString();
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            Debug.Fail("call typed Equals");
            return base.Equals(obj);
        }
    }

    /// <summary>
    /// A DNF clause is of the form:
    /// 
    ///     Literal1 . Literal2 . ...
    /// 
    /// Each literal is of the form:
    /// 
    ///     Term
    /// 
    /// or
    /// 
    ///     !Term
    /// </summary>
    /// <typeparam name="T_Identifier">Type of normal form literal.</typeparam>
    internal sealed class DnfClause<T_Identifier> : Clause<T_Identifier>,
        IEquatable<DnfClause<T_Identifier>>
    {
        /// <summary>
        /// Initialize a DNF clause.
        /// </summary>
        /// <param name="literals">Literals in clause.</param>
        internal DnfClause(Set<Literal<T_Identifier>> literals)
            : base(literals, ExprType.And)
        {
        }

        public bool Equals(DnfClause<T_Identifier> other)
        {
            return null != other &&
                other.Literals.SetEquals(Literals);
        }
    }

    /// <summary>
    /// A CNF clause is of the form:
    /// 
    ///     Literal1 + Literal2 . ...
    /// 
    /// Each literal is of the form:
    /// 
    ///     Term
    /// 
    /// or
    /// 
    ///     !Term
    /// </summary>
    /// <typeparam name="T_Identifier">Type of normal form literal.</typeparam>
    internal sealed class CnfClause<T_Identifier> : Clause<T_Identifier>,
        IEquatable<CnfClause<T_Identifier>>
    {
        /// <summary>
        /// Initialize a CNF clause.
        /// </summary>
        /// <param name="literals">Literals in clause.</param>
        internal CnfClause(Set<Literal<T_Identifier>> literals)
            : base(literals, ExprType.Or)
        {
        }

        public bool Equals(CnfClause<T_Identifier> other)
        {
            return null != other &&
                other.Literals.SetEquals(Literals);
        }
    }
}
