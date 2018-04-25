//---------------------------------------------------------------------
// <copyright file="NegationPusher.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Common.Utils.Boolean
{
    // Top-down push-down of negation in Boolean expressions.
    // - !(A or B) iff. !A and !B
    // - !(A and B) iff. !A or !B
    // - !!A iff. A
    // Uses two visitor classes: one to handle negated subtrees (essentially creates
    // an inverted tree) and one to handle non-negated subtrees (replicates until it
    // encounters NotExpr)
    internal static class NegationPusher
    {
        internal static BoolExpr<DomainConstraint<T_Variable, T_Element>> EliminateNot<T_Variable, T_Element>(BoolExpr<DomainConstraint<T_Variable, T_Element>> expression)
        {
            return expression.Accept(NonNegatedDomainConstraintTreeVisitor<T_Variable, T_Element>.Instance);
        }

        private class NonNegatedTreeVisitor<T_Identifier> : BasicVisitor<T_Identifier>
        {
            internal static readonly NonNegatedTreeVisitor<T_Identifier> Instance = new NonNegatedTreeVisitor<T_Identifier>();

            protected NonNegatedTreeVisitor()
            {
            }

            internal override BoolExpr<T_Identifier> VisitNot(NotExpr<T_Identifier> expression)
            {
                return expression.Child.Accept(NegatedTreeVisitor<T_Identifier>.Instance);
            }
        }

        private class NegatedTreeVisitor<T_Identifier> : Visitor<T_Identifier, BoolExpr<T_Identifier>>
        {
            internal static readonly NegatedTreeVisitor<T_Identifier> Instance = new NegatedTreeVisitor<T_Identifier>();

            protected NegatedTreeVisitor()
            {
            }

            internal override BoolExpr<T_Identifier> VisitTrue(TrueExpr<T_Identifier> expression)
            {
                return FalseExpr<T_Identifier>.Value;
            }

            internal override BoolExpr<T_Identifier> VisitFalse(FalseExpr<T_Identifier> expression)
            {
                return TrueExpr<T_Identifier>.Value;
            }

            internal override BoolExpr<T_Identifier> VisitTerm(TermExpr<T_Identifier> expression)
            {
                return new NotExpr<T_Identifier>(expression);
            }

            internal override BoolExpr<T_Identifier> VisitNot(NotExpr<T_Identifier> expression)
            {
                return expression.Child.Accept(NonNegatedTreeVisitor<T_Identifier>.Instance);
            }

            internal override BoolExpr<T_Identifier> VisitAnd(AndExpr<T_Identifier> expression)
            {
                return new OrExpr<T_Identifier>(expression.Children.Select(child => child.Accept(this)));                
            }

            internal override BoolExpr<T_Identifier> VisitOr(OrExpr<T_Identifier> expression)
            {
                return new AndExpr<T_Identifier>(expression.Children.Select(child => child.Accept(this)));
            }
        }

        private class NonNegatedDomainConstraintTreeVisitor<T_Variable, T_Element> : NonNegatedTreeVisitor<DomainConstraint<T_Variable, T_Element>>
        {
            internal new static readonly NonNegatedDomainConstraintTreeVisitor<T_Variable, T_Element> Instance = new NonNegatedDomainConstraintTreeVisitor<T_Variable, T_Element>();

            private NonNegatedDomainConstraintTreeVisitor()
            {
            }

            internal override BoolExpr<DomainConstraint<T_Variable, T_Element>> VisitNot(NotExpr<DomainConstraint<T_Variable, T_Element>> expression)
            {
                return expression.Child.Accept(NegatedDomainConstraintTreeVisitor<T_Variable, T_Element>.Instance);
            }
        }

        private class NegatedDomainConstraintTreeVisitor<T_Variable, T_Element> : NegatedTreeVisitor<DomainConstraint<T_Variable, T_Element>>
        {
            internal new static readonly NegatedDomainConstraintTreeVisitor<T_Variable, T_Element> Instance = new NegatedDomainConstraintTreeVisitor<T_Variable, T_Element>();

            private NegatedDomainConstraintTreeVisitor()
            {
            }

            internal override BoolExpr<DomainConstraint<T_Variable, T_Element>> VisitNot(NotExpr<DomainConstraint<T_Variable, T_Element>> expression)
            {
                return expression.Child.Accept(NonNegatedDomainConstraintTreeVisitor<T_Variable, T_Element>.Instance);
            }

            internal override BoolExpr<DomainConstraint<T_Variable, T_Element>> VisitTerm(TermExpr<DomainConstraint<T_Variable, T_Element>> expression)
            {
                return new TermExpr<DomainConstraint<T_Variable, T_Element>>(expression.Identifier.InvertDomainConstraint());
            }
        }
    }
}
