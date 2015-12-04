//---------------------------------------------------------------------
// <copyright file="BoolExpr.cs" company="Microsoft">
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
    /// Base type for Boolean expressions. Boolean expressions are immutable,
    /// and value-comparable using Equals. Services include local simplification
    /// and normalization to Conjunctive and Disjunctive Normal Forms.
    /// </summary>
    /// <remarks>
    /// Comments use the following notation convention:
    /// 
    ///     "A . B" means "A and B"
    ///     "A + B" means "A or B"
    ///     "!A" means "not A"
    /// </remarks>
    /// <typeparam name="T_Identifier">The type of leaf term identifiers in this expression.</typeparam>
    internal abstract partial class BoolExpr<T_Identifier> : IEquatable<BoolExpr<T_Identifier>>
    {
        /// <summary>
        /// Gets an enumeration value indicating the type of the expression node.
        /// </summary>
        internal abstract ExprType ExprType { get; }

        /// <summary>
        /// Standard accept method invoking the appropriate method overload
        /// in the given visitor.
        /// </summary>
        /// <typeparam name="T_Return">T_Return is the return type for the visitor.</typeparam>
        /// <param name="visitor">Visitor implementation.</param>
        /// <returns>Value computed for this node.</returns>
        internal abstract T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor);

        /// <summary>
        /// Invokes the Simplifier visitor on this expression tree.
        /// Simplifications are purely local (see Simplifier class
        /// for details).
        /// </summary>
        internal BoolExpr<T_Identifier> Simplify()
        {
            return IdentifierService<T_Identifier>.Instance.LocalSimplify(this);
        }

        /// <summary>
        /// Expensive simplification that considers various permutations of the
        /// expression (including Decision Diagram, DNF, and CNF translations)
        /// </summary>
        internal BoolExpr<T_Identifier> ExpensiveSimplify(out Converter<T_Identifier> converter)
        {
            var context = IdentifierService<T_Identifier>.Instance.CreateConversionContext();
            converter = new Converter<T_Identifier>(this, context);

            // Check for valid/unsat constraints
            if (converter.Vertex.IsOne())
            {
                return TrueExpr<T_Identifier>.Value;
            }
            if (converter.Vertex.IsZero())
            {
                return FalseExpr<T_Identifier>.Value;
            }

            // Pick solution from the (unmodified) expression, its CNF and its DNF
            return ChooseCandidate(this, converter.Cnf.Expr, converter.Dnf.Expr);
        }

        private static BoolExpr<T_Identifier> ChooseCandidate(params BoolExpr<T_Identifier>[] candidates)
        {
            Debug.Assert(null != candidates && 1 < candidates.Length, "must be at least one to pick");

            int resultUniqueTermCount = default(int);
            int resultTermCount = default(int);
            BoolExpr<T_Identifier> result = null;

            foreach (var candidate in candidates)
            {
                // first do basic simplification
                var simplifiedCandidate = candidate.Simplify();

                // determine "interesting" properties of the expression
                int candidateUniqueTermCount = simplifiedCandidate.GetTerms().Distinct().Count();
                int candidateTermCount = simplifiedCandidate.CountTerms();

                // see if it's better than the current result best result
                if (null == result || // bootstrap
                    candidateUniqueTermCount < resultUniqueTermCount || // check if the candidate improves on # of terms
                    (candidateUniqueTermCount == resultUniqueTermCount && // in case of tie, choose based on total
                     candidateTermCount < resultTermCount))
                {
                    result = simplifiedCandidate;
                    resultUniqueTermCount = candidateUniqueTermCount;
                    resultTermCount = candidateTermCount;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all term expressions below this node.
        /// </summary>
        internal List<TermExpr<T_Identifier>> GetTerms()
        {
            return LeafVisitor<T_Identifier>.GetTerms(this);
        }

        /// <summary>
        /// Counts terms in this expression.
        /// </summary>
        internal int CountTerms()
        {
            return TermCounter<T_Identifier>.CountTerms(this);
        }

        /// <summary>
        /// Implicit cast from a value of type T to a TermExpr where
        /// TermExpr.Value is set to the given value.
        /// </summary>
        /// <param name="value">Value to wrap in term expression</param>
        /// <returns>Term expression</returns>
        public static implicit operator BoolExpr<T_Identifier>(T_Identifier value)
        {
            return new TermExpr<T_Identifier>(value);
        }

        /// <summary>
        /// Creates the negation of the current element. 
        /// </summary>
        internal virtual BoolExpr<T_Identifier> MakeNegated()
        {
            return new NotExpr<T_Identifier>(this);
        }

        public override string ToString()
        {
            return ExprType.ToString();
        }

        public bool Equals(BoolExpr<T_Identifier> other)
        {
            return null != other && ExprType == other.ExprType &&
                EquivalentTypeEquals(other);
        }

        protected abstract bool EquivalentTypeEquals(BoolExpr<T_Identifier> other);
    }

    /// <summary>
    /// Boolean expression that evaluates to true.
    /// </summary>
    /// <typeparam name="T_Identifier">The type of leaf term identifiers in this expression.</typeparam>
    internal sealed class TrueExpr<T_Identifier> : BoolExpr<T_Identifier>
    {
        private static readonly TrueExpr<T_Identifier> s_value = new TrueExpr<T_Identifier>();

        // private constructor so that we control existence of True instance
        private TrueExpr()
            : base()
        {
        }

        /// <summary>
        /// Gets the one instance of TrueExpr
        /// </summary>
        internal static TrueExpr<T_Identifier> Value { get { return s_value; } }

        internal override ExprType ExprType { get { return ExprType.True; } }

        internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
        {
            return visitor.VisitTrue(this);
        }

        internal override BoolExpr<T_Identifier> MakeNegated()
        {
            return FalseExpr<T_Identifier>.Value;
        }

        protected override bool EquivalentTypeEquals(BoolExpr<T_Identifier> other)
        {
            return object.ReferenceEquals(this, other);
        }
    }

    /// <summary>
    /// Boolean expression that evaluates to false.
    /// </summary>
    /// <typeparam name="T_Identifier">The type of leaf term identifiers in this expression.</typeparam>
    internal sealed class FalseExpr<T_Identifier> : BoolExpr<T_Identifier>
    {
        private static readonly FalseExpr<T_Identifier> s_value = new FalseExpr<T_Identifier>();

        // private constructor so that we control existence of False instance
        private FalseExpr()
            : base()
        {
        }

        /// <summary>
        /// Gets the one instance of FalseExpr
        /// </summary>
        internal static FalseExpr<T_Identifier> Value { get { return s_value; } }

        internal override ExprType ExprType { get { return ExprType.False; } }

        internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
        {
            return visitor.VisitFalse(this);
        }

        internal override BoolExpr<T_Identifier> MakeNegated()
        {
            return TrueExpr<T_Identifier>.Value;
        }

        protected override bool EquivalentTypeEquals(BoolExpr<T_Identifier> other)
        {
            return object.ReferenceEquals(this, other);
        }
    }

    /// <summary>
    /// A term is a leaf node in a Boolean expression. Its value (T/F) is undefined.
    /// </summary>
    /// <typeparam name="T_Identifier">The type of leaf term identifiers in this expression.</typeparam>
    internal sealed class TermExpr<T_Identifier> : BoolExpr<T_Identifier>, IEquatable<TermExpr<T_Identifier>>
    {
        private readonly T_Identifier _identifier;
        private readonly IEqualityComparer<T_Identifier> _comparer;

        /// <summary>
        /// Construct a term. 
        /// </summary>
        /// <param name="comparer">Value comparer to use when comparing two
        /// term expressions.</param>
        /// <param name="identifier">Identifier/tag for this term.</param>
        internal TermExpr(IEqualityComparer<T_Identifier> comparer, T_Identifier identifier)
            : base()
        {
            Debug.Assert(null != (object)identifier);
            _identifier = identifier;
            if (null == comparer) { _comparer = EqualityComparer<T_Identifier>.Default; }
            else { _comparer = comparer; }
        }
        internal TermExpr(T_Identifier identifier) : this(null, identifier) { }

        /// <summary>
        /// Gets identifier for this term. This value is used to determine whether
        /// two terms as equivalent.
        /// </summary>
        internal T_Identifier Identifier { get { return _identifier; } }

        internal override ExprType ExprType { get { return ExprType.Term; } }
        
        public override bool Equals(object obj)
        {
            Debug.Fail("use only typed equals");
            return this.Equals(obj as TermExpr<T_Identifier>);
        }

        public bool Equals(TermExpr<T_Identifier> other)
        {
            return _comparer.Equals(_identifier, other._identifier);
        }

        protected override bool EquivalentTypeEquals(BoolExpr<T_Identifier> other)
        {
            return _comparer.Equals(_identifier, ((TermExpr<T_Identifier>)other)._identifier);
        }
        
        public override int GetHashCode()
        {
            return _comparer.GetHashCode(_identifier);
        }
        
        public override string ToString()
        {
            return StringUtil.FormatInvariant("{0}", _identifier);
        }

        internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
        {
            return visitor.VisitTerm(this);
        }

        internal override BoolExpr<T_Identifier> MakeNegated()
        {
            Literal<T_Identifier> literal = new Literal<T_Identifier>(this, true);
            // leverage normalization code if it exists
            Literal<T_Identifier> negatedLiteral = literal.MakeNegated();
            if (negatedLiteral.IsTermPositive)
            {
                return negatedLiteral.Term;
            }
            else
            {
                return new NotExpr<T_Identifier>(negatedLiteral.Term);
            }
        }
    }

    /// <summary>
    /// Abstract base class for tree expressions (unary as in Not, n-ary
    /// as in And or Or). Duplicate elements are trimmed at construction
    /// time (algorithms applied to these trees rely on the assumption
    /// of uniform children).
    /// </summary>
    /// <typeparam name="T_Identifier">The type of leaf term identifiers in this expression.</typeparam>
    internal abstract class TreeExpr<T_Identifier> : BoolExpr<T_Identifier>
    {
        private readonly Set<BoolExpr<T_Identifier>> _children;
        private readonly int _hashCode;

        /// <summary>
        /// Initialize a new tree expression with the given children.
        /// </summary>
        /// <param name="children">Child expressions</param>
        protected TreeExpr(IEnumerable<BoolExpr<T_Identifier>> children)
            : base()
        {
            Debug.Assert(null != children);
            _children = new Set<BoolExpr<T_Identifier>>(children);
            _children.MakeReadOnly();
            _hashCode = _children.GetElementsHashCode();
        }

        /// <summary>
        /// Gets the children of this expression node.
        /// </summary>
        internal Set<BoolExpr<T_Identifier>> Children { get { return _children; } }
        
        public override bool Equals(object obj)
        {
            Debug.Fail("use only typed Equals");
            return base.Equals(obj as BoolExpr<T_Identifier>);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return StringUtil.FormatInvariant("{0}({1})", ExprType, _children);
        }

        protected override bool EquivalentTypeEquals(BoolExpr<T_Identifier> other)
        {
            return ((TreeExpr<T_Identifier>)other).Children.SetEquals(Children);
        }
    }

    /// <summary>
    /// A tree expression that evaluates to true iff. none of its children
    /// evaluate to false.
    /// </summary>
    /// <remarks>
    /// An And expression with no children is equivalent to True (this is an
    /// operational convenience because we assume an implicit True is along
    /// for the ride in every And expression)
    /// 
    ///     A . True iff. A
    /// </remarks>
    /// <typeparam name="T_Identifier">The type of leaf term identifiers in this expression.</typeparam>
    internal class AndExpr<T_Identifier> : TreeExpr<T_Identifier>
    {
        /// <summary>
        /// Initialize a new And expression with the given children.
        /// </summary>
        /// <param name="children">Child expressions</param>
        internal AndExpr(params BoolExpr<T_Identifier>[] children)
            : this((IEnumerable<BoolExpr<T_Identifier>>)children)
        {
        }

        /// <summary>
        /// Initialize a new And expression with the given children.
        /// </summary>
        /// <param name="children">Child expressions</param>
        internal AndExpr(IEnumerable<BoolExpr<T_Identifier>> children)
            : base(children)
        {
        }

        internal override ExprType ExprType { get { return ExprType.And; } }

        internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
        {
            return visitor.VisitAnd(this);
        }
    }

    /// <summary>
    /// A tree expression that evaluates to true iff. any of its children
    /// evaluates to true.
    /// </summary>
    /// <remarks>
    /// An Or expression with no children is equivalent to False (this is an
    /// operational convenience because we assume an implicit False is along
    /// for the ride in every Or expression)
    /// 
    ///     A + False iff. A
    /// </remarks>
    /// <typeparam name="T_Identifier">The type of leaf term identifiers in this expression.</typeparam>
    internal class OrExpr<T_Identifier> : TreeExpr<T_Identifier>
    {
        /// <summary>
        /// Initialize a new Or expression with the given children.
        /// </summary>
        /// <param name="children">Child expressions</param>
        internal OrExpr(params BoolExpr<T_Identifier>[] children)
            : this((IEnumerable<BoolExpr<T_Identifier>>)children)
        {
        }

        /// <summary>
        /// Initialize a new Or expression with the given children.
        /// </summary>
        /// <param name="children">Child expressions</param>
        internal OrExpr(IEnumerable<BoolExpr<T_Identifier>> children)
            : base(children)
        {
        }

        internal override ExprType ExprType { get { return ExprType.Or; } }

        internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
        {
            return visitor.VisitOr(this);
        }
    }

    /// <summary>
    /// A tree expression that evaluates to true iff. its (single) child evaluates to false.
    /// </summary>
    /// <typeparam name="T_Identifier">The type of leaf term identifiers in this expression.</typeparam>
    internal sealed class NotExpr<T_Identifier> : TreeExpr<T_Identifier>
    {
        /// <summary>
        /// Initialize a new Not expression with the given child.
        /// </summary>
        /// <param name="child"></param>
        internal NotExpr(BoolExpr<T_Identifier> child)
            : base(new BoolExpr<T_Identifier>[] { child })
        {
        }

        internal override ExprType ExprType { get { return ExprType.Not; } }

        internal BoolExpr<T_Identifier> Child { get { return Children.First(); } }

        internal override T_Return Accept<T_Return>(Visitor<T_Identifier, T_Return> visitor)
        {
            return visitor.VisitNot(this);
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "!{0}", Child);
        }

        internal override BoolExpr<T_Identifier> MakeNegated()
        {
            return this.Child;
        }
    }

    /// <summary>
    /// Enumeration of Boolean expression node types.
    /// </summary>
    internal enum ExprType
    {
        And, Not, Or, Term, True, False,
    }
}
