//---------------------------------------------------------------------
// <copyright file="Sentence.cs" company="Microsoft">
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
    /// Abstract base class for nodes in normal form expressions, e.g. Conjunctive Normal Form
    /// sentences.
    /// </summary>
    /// <typeparam name="T_Identifier">Type of expression leaf term identifiers.</typeparam>
    internal abstract class NormalFormNode<T_Identifier>
    {
        private readonly BoolExpr<T_Identifier> _expr;
        
        /// <summary>
        /// Initialize a new normal form node representing the given expression. Caller must
        /// ensure the expression is logically equivalent to the node.
        /// </summary>
        /// <param name="expr">Expression logically equivalent to this node.</param>
        protected NormalFormNode(BoolExpr<T_Identifier> expr) { _expr = expr.Simplify(); }

        /// <summary>
        /// Gets an expression that is logically equivalent to this node.
        /// </summary>
        internal BoolExpr<T_Identifier> Expr { get { return _expr; } }

        /// <summary>
        /// Utility method for delegation that return the expression corresponding to a given
        /// normal form node.
        /// </summary>
        /// <typeparam name="T_NormalFormNode">Type of node</typeparam>
        /// <param name="node">Node to examine.</param>
        /// <returns>Equivalent Boolean expression for the given node.</returns>
        protected static BoolExpr<T_Identifier> ExprSelector<T_NormalFormNode>(T_NormalFormNode node)
            where T_NormalFormNode : NormalFormNode<T_Identifier>
        {
            return node._expr;
        }
    }

    /// <summary>
    /// Abstract base class for normal form sentences (CNF and DNF)
    /// </summary>
    /// <typeparam name="T_Identifier">Type of expression leaf term identifiers.</typeparam>
    /// <typeparam name="T_Clause">Type of clauses in the sentence.</typeparam>
    internal abstract class Sentence<T_Identifier, T_Clause> : NormalFormNode<T_Identifier>
        where T_Clause : Clause<T_Identifier>, IEquatable<T_Clause>
    {
        private readonly Set<T_Clause> _clauses;

        /// <summary>
        /// Initialize a sentence given the appropriate sentence clauses. Produces
        /// an equivalent expression by composing the clause expressions using
        /// the given tree type.
        /// </summary>
        /// <param name="clauses">Sentence clauses</param>
        /// <param name="treeType">Tree type for sentence (and generated expression)</param>
        protected Sentence(Set<T_Clause> clauses, ExprType treeType)
            : base(ConvertClausesToExpr(clauses, treeType))
        {
            _clauses = clauses.AsReadOnly();
        }

        // Produces an expression equivalent to the given clauses by composing the clause
        // expressions using the given tree type.
        private static BoolExpr<T_Identifier> ConvertClausesToExpr(Set<T_Clause> clauses, ExprType treeType)
        {
            bool isAnd = ExprType.And == treeType;
            Debug.Assert(isAnd || ExprType.Or == treeType);

            IEnumerable<BoolExpr<T_Identifier>> clauseExpressions = 
                clauses.Select(new Func<T_Clause, BoolExpr<T_Identifier>>(ExprSelector));

            if (isAnd)
            {
                return new AndExpr<T_Identifier>(clauseExpressions);
            }
            else
            {
                return new OrExpr<T_Identifier>(clauseExpressions);
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Sentence{");
            builder.Append(_clauses);
            return builder.Append("}").ToString();
        }
    }

    /// <summary>
    /// Represents a sentence in disjunctive normal form, e.g.:
    /// 
    ///     Clause1 + Clause2 . ...
    /// 
    /// Where each DNF clause is of the form:
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
    /// <typeparam name="T_Identifier">Type of expression leaf term identifiers.</typeparam>
    internal sealed class DnfSentence<T_Identifier> : Sentence<T_Identifier, DnfClause<T_Identifier>>
    {
        // Initializes a new DNF sentence given its clauses.
        internal DnfSentence(Set<DnfClause<T_Identifier>> clauses)
            : base(clauses, ExprType.Or)
        {
        }
    }

    /// <summary>
    /// Represents a sentence in conjunctive normal form, e.g.:
    /// 
    ///     Clause1 . Clause2 . ...
    /// 
    /// Where each DNF clause is of the form:
    /// 
    ///     Literal1 + Literal2 + ...
    /// 
    /// Each literal is of the form:
    /// 
    ///     Term
    /// 
    /// or
    /// 
    ///     !Term    
    /// </summary>
    /// <typeparam name="T_Identifier">Type of expression leaf term identifiers.</typeparam>
    internal sealed class CnfSentence<T_Identifier> : Sentence<T_Identifier, CnfClause<T_Identifier>>
    {
        // Initializes a new CNF sentence given its clauses.
        internal CnfSentence(Set<CnfClause<T_Identifier>> clauses)
            : base(clauses, ExprType.And)
        {
        }
    }
}
