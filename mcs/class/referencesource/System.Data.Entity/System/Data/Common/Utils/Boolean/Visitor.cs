//---------------------------------------------------------------------
// <copyright file="Visitor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
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
    /// Abstract visitor class. All Boolean expression nodes know how to
    /// 'accept' a visitor, and delegate to the appropriate visitor method.
    /// For instance, AndExpr invokes Visitor.VisitAnd.
    /// </summary>
    /// <typeparam name="T_Identifier">Type of leaf term identifiers in expression.</typeparam>
    /// <typeparam name="T_Return">Return type for visit methods.</typeparam>
    internal abstract class Visitor<T_Identifier, T_Return>
    {
        internal abstract T_Return VisitTrue(TrueExpr<T_Identifier> expression);
        internal abstract T_Return VisitFalse(FalseExpr<T_Identifier> expression);
        internal abstract T_Return VisitTerm(TermExpr<T_Identifier> expression);
        internal abstract T_Return VisitNot(NotExpr<T_Identifier> expression);
        internal abstract T_Return VisitAnd(AndExpr<T_Identifier> expression);
        internal abstract T_Return VisitOr(OrExpr<T_Identifier> expression);
    }

    /// <summary>
    /// Basic visitor which reproduces the given expression tree.
    /// </summary>
    /// <typeparam name="T_Identifier">Type of leaf term identifiers in expression.</typeparam>
    internal abstract class BasicVisitor<T_Identifier> : Visitor<T_Identifier, BoolExpr<T_Identifier>>
    {
        internal override BoolExpr<T_Identifier> VisitFalse(FalseExpr<T_Identifier> expression) { return expression; }
        internal override BoolExpr<T_Identifier> VisitTrue(TrueExpr<T_Identifier> expression) { return expression; }
        internal override BoolExpr<T_Identifier> VisitTerm(TermExpr<T_Identifier> expression) { return expression; }
        internal override BoolExpr<T_Identifier> VisitNot(NotExpr<T_Identifier> expression) 
        { 
            return new NotExpr<T_Identifier>(expression.Child.Accept(this)); 
        }
        internal override BoolExpr<T_Identifier> VisitAnd(AndExpr<T_Identifier> expression) 
        { 
            return new AndExpr<T_Identifier>(AcceptChildren(expression.Children)); 
        }
        internal override BoolExpr<T_Identifier> VisitOr(OrExpr<T_Identifier> expression) 
        {
            return new OrExpr<T_Identifier>(AcceptChildren(expression.Children));
        }
        private IEnumerable<BoolExpr<T_Identifier>> AcceptChildren(IEnumerable<BoolExpr<T_Identifier>> children)
        {
            foreach (BoolExpr<T_Identifier> child in children) { yield return child.Accept(this); }
        }
    }

    internal class TermCounter<T_Identifier> : Visitor<T_Identifier, int>
    {
        static readonly TermCounter<T_Identifier> s_instance = new TermCounter<T_Identifier>();

        internal static int CountTerms(BoolExpr<T_Identifier> expression)
        {
            Debug.Assert(null != expression);
            return expression.Accept(s_instance);
        }

        internal override int VisitTrue(TrueExpr<T_Identifier> expression)
        {
            return 0;
        }

        internal override int VisitFalse(FalseExpr<T_Identifier> expression)
        {
            return 0;
        }

        internal override int VisitTerm(TermExpr<T_Identifier> expression)
        {
            return 1;
        }

        internal override int VisitNot(NotExpr<T_Identifier> expression)
        {
            return expression.Child.Accept(this);
        }

        internal override int VisitAnd(AndExpr<T_Identifier> expression)
        {
            return VisitTree(expression);
        }

        internal override int VisitOr(OrExpr<T_Identifier> expression)
        {
            return VisitTree(expression);
        }

        private int VisitTree(TreeExpr<T_Identifier> expression)
        {
            int sum = 0;
            foreach (var child in expression.Children)
            {
                sum += child.Accept(this);
            }
            return sum;
        }
    }

    /// <summary>
    /// A Visitor class that returns all the leaves in a boolean expression
    /// </summary>
    /// <typeparam name="T_Identifier">Type of leaf term identifiers in expression.</typeparam>
    internal class LeafVisitor<T_Identifier> : Visitor<T_Identifier, bool>
    {
        readonly List<TermExpr<T_Identifier>> _terms;

        private LeafVisitor()
        {
            _terms = new List<TermExpr<T_Identifier>>();
        }

        internal static List<TermExpr<T_Identifier>> GetTerms(BoolExpr<T_Identifier> expression)
        {
            Debug.Assert(null != expression, "expression must be given");
            LeafVisitor<T_Identifier> visitor = new LeafVisitor<T_Identifier>();
            expression.Accept(visitor);
            return visitor._terms;
        }

        internal static IEnumerable<T_Identifier> GetLeaves(BoolExpr<T_Identifier> expression) 
        {
            return GetTerms(expression).Select(term => term.Identifier);
        }

        internal override bool VisitTrue(TrueExpr<T_Identifier> expression)
        {
            return true;
        }

        internal override bool VisitFalse(FalseExpr<T_Identifier> expression)
        {
            return true;
        }

        internal override bool VisitTerm(TermExpr<T_Identifier> expression)
        {
            _terms.Add(expression);
            return true;
        }

        internal override bool VisitNot(NotExpr<T_Identifier> expression)
        {
            return expression.Child.Accept(this);
        }

        internal override bool VisitAnd(AndExpr<T_Identifier> expression)
        {
            return VisitTree(expression);
        }

        internal override bool VisitOr(OrExpr<T_Identifier> expression)
        {
            return VisitTree(expression);
        }

        private bool VisitTree(TreeExpr<T_Identifier> expression)
        {
            foreach (BoolExpr<T_Identifier> child in expression.Children)
            {
                child.Accept(this);
            }
            return true;
        }            
    }

    /// <summary>
    /// Rewrites the terms in a Boolean expression tree.
    /// </summary>
    /// <typeparam name="T_From">Term type for leaf nodes of input</typeparam>
    /// <typeparam name="T_To">Term type for leaf nodes of output</typeparam>
    internal class BooleanExpressionTermRewriter<T_From, T_To> : Visitor<T_From, BoolExpr<T_To>>
    {
        private readonly Func<TermExpr<T_From>, BoolExpr<T_To>> _translator;

        /// <summary>
        /// Initialize a new translator
        /// </summary>
        /// <param name="translator">Translator delegate; must not be null</param>
        internal BooleanExpressionTermRewriter(Func<TermExpr<T_From>, BoolExpr<T_To>> translator)
        {
            Debug.Assert(null != translator);
            _translator = translator;
        }

        internal override BoolExpr<T_To> VisitFalse(FalseExpr<T_From> expression)
        {
            return FalseExpr<T_To>.Value;
        }

        internal override BoolExpr<T_To> VisitTrue(TrueExpr<T_From> expression)
        {
            return TrueExpr<T_To>.Value;
        }

        internal override BoolExpr<T_To> VisitNot(NotExpr<T_From> expression)
        {
            return new NotExpr<T_To>(expression.Child.Accept(this));
        }

        internal override BoolExpr<T_To> VisitTerm(TermExpr<T_From> expression)
        {
            return _translator(expression);
        }

        internal override BoolExpr<T_To> VisitAnd(AndExpr<T_From> expression)
        {
            return new AndExpr<T_To>(VisitChildren(expression));
        }

        internal override BoolExpr<T_To> VisitOr(OrExpr<T_From> expression)
        {
            return new OrExpr<T_To>(VisitChildren(expression));
        }

        private IEnumerable<BoolExpr<T_To>> VisitChildren(TreeExpr<T_From> expression)
        {
            foreach (BoolExpr<T_From> child in expression.Children)
            {
                yield return child.Accept(this);
            }
        }
    }

    /// <summary>
    /// Converts a BoolExpr to a Vertex within a solver.
    /// </summary>
    internal class ToDecisionDiagramConverter<T_Identifier> : Visitor<T_Identifier, Vertex>
    {
        private readonly ConversionContext<T_Identifier> _context;

        private ToDecisionDiagramConverter(ConversionContext<T_Identifier> context)
        {
            Debug.Assert(null != context, "must provide a context");
            _context = context;
        }

        internal static Vertex TranslateToRobdd(BoolExpr<T_Identifier> expr, ConversionContext<T_Identifier> context)
        {
            Debug.Assert(null != expr, "must provide an expression");
            ToDecisionDiagramConverter<T_Identifier> converter =
                new ToDecisionDiagramConverter<T_Identifier>(context);
            return expr.Accept(converter);
        }

        internal override Vertex VisitTrue(TrueExpr<T_Identifier> expression)
        {
            return Vertex.One;
        }

        internal override Vertex VisitFalse(FalseExpr<T_Identifier> expression)
        {
            return Vertex.Zero;
        }

        internal override Vertex VisitTerm(TermExpr<T_Identifier> expression)
        {
            return _context.TranslateTermToVertex(expression);
        }

        internal override Vertex VisitNot(NotExpr<T_Identifier> expression)
        {
            return _context.Solver.Not(expression.Child.Accept(this));
        }

        internal override Vertex VisitAnd(AndExpr<T_Identifier> expression)
        {
            return _context.Solver.And(expression.Children.Select(child => child.Accept(this)));
        }

        internal override Vertex VisitOr(OrExpr<T_Identifier> expression)
        {
            return _context.Solver.Or(expression.Children.Select(child => child.Accept(this)));
        }
    }
}
