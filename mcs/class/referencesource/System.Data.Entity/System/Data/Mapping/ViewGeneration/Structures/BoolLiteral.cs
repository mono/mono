//---------------------------------------------------------------------
// <copyright file="BoolLiteral.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Common.CommandTrees;
using System.Data.Common.Utils;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    using DomainConstraint  = System.Data.Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>;
    using DomainVariable    = System.Data.Common.Utils.Boolean.DomainVariable<BoolLiteral, Constant>;
    using DomainBoolExpr    = System.Data.Common.Utils.Boolean.BoolExpr<System.Data.Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;
    using DomainNotExpr     = System.Data.Common.Utils.Boolean.NotExpr <System.Data.Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;
    using DomainTermExpr    = System.Data.Common.Utils.Boolean.TermExpr<System.Data.Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;

    /// <summary>
    /// A class that ties up all the literals in boolean expressions.
    /// Conditions represented by <see cref="BoolLiteral"/>s need to be synchronized with <see cref="DomainConstraint"/>s,
    /// which may be modified upon calling <see cref="BoolExpression.ExpensiveSimplify"/>. This is what the method <see cref="BoolLiteral.FixRange"/> is used for.
    /// </summary>
    internal abstract class BoolLiteral : InternalBase
    {
        #region Fields
        internal static readonly IEqualityComparer<BoolLiteral> EqualityComparer = new BoolLiteralComparer();
        internal static readonly IEqualityComparer<BoolLiteral> EqualityIdentifierComparer = new IdentifierComparer();
        #endregion

        #region Static MakeTermExpression methods
        /// <summary>
        /// Creates a term expression of the form: "<paramref name="literal"/> in <paramref name="range"/> with all possible values being <paramref name="domain"/>".
        /// </summary>
        internal static DomainTermExpr MakeTermExpression(BoolLiteral literal, IEnumerable<Constant> domain, IEnumerable<Constant> range)
        {
            Set<Constant> domainSet = new Set<Constant>(domain, Constant.EqualityComparer);
            Set<Constant> rangeSet = new Set<Constant>(range, Constant.EqualityComparer);
            return MakeTermExpression(literal, domainSet, rangeSet);
        }

        /// <summary>
        /// Creates a term expression of the form: "<paramref name="literal"/> in <paramref name="range"/> with all possible values being <paramref name="domain"/>".
        /// </summary>
        internal static DomainTermExpr MakeTermExpression(BoolLiteral literal, Set<Constant> domain, Set<Constant> range)
        {
            domain.MakeReadOnly();
            range.MakeReadOnly();

            DomainVariable variable = new DomainVariable(literal, domain, EqualityIdentifierComparer);
            DomainConstraint constraint = new DomainConstraint(variable, range);
            DomainTermExpr result = new DomainTermExpr(EqualityComparer<DomainConstraint>.Default, constraint);
            return result;
        }
        #endregion

        #region Virtual methods
        /// <summary>
        /// Fixes the range of the literal using the new values provided in <paramref name="range"/> and returns a boolean expression corresponding to the new value.
        /// </summary>
        internal abstract DomainBoolExpr FixRange(Set<Constant> range, MemberDomainMap memberDomainMap);

        internal abstract DomainBoolExpr GetDomainBoolExpression(MemberDomainMap domainMap);

        /// <summary>
        /// See <see cref="BoolExpression.RemapBool"/>.
        /// </summary>
        internal abstract BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap);

        /// <summary>
        /// See <see cref="BoolExpression.GetRequiredSlots"/>.
        /// </summary>
        /// <param name="projectedSlotMap"></param>
        /// <param name="requiredSlots"></param>
        internal abstract void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots);

        /// <summary>
        /// See <see cref="BoolExpression.AsEsql"/>.
        /// </summary>
        internal abstract StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull);

        /// <summary>
        /// See <see cref="BoolExpression.AsCqt"/>.
        /// </summary>
        internal abstract DbExpression AsCqt(DbExpression row, bool skipIsNotNull);

        internal abstract StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull);

        internal abstract StringBuilder AsNegatedUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull);

        /// <summary>
        /// Checks if the identifier in this is the same as the one in <paramref name="right"/>.
        /// </summary>
        protected virtual bool IsIdentifierEqualTo(BoolLiteral right)
        {
            return IsEqualTo(right);
        }

        protected abstract bool IsEqualTo(BoolLiteral right);

        /// <summary>
        /// Get the hash code based on the identifier.
        /// </summary>
        protected virtual int GetIdentifierHash()
        {
            return GetHashCode();
        }
        #endregion

        #region Comparer class
        /// <summary>
        /// This class compares boolean expressions.
        /// </summary>
        private sealed class BoolLiteralComparer : IEqualityComparer<BoolLiteral>
        {
            public bool Equals(BoolLiteral left, BoolLiteral right)
            {
                // Quick check with references
                if (object.ReferenceEquals(left, right))
                {
                    // Gets the Null and Undefined case as well
                    return true;
                }
                // One of them is non-null at least
                if (left == null || right == null)
                {
                    return false;
                }
                // Both are non-null at this point
                return left.IsEqualTo(right);
            }

            public int GetHashCode(BoolLiteral literal)
            {
                return literal.GetHashCode();
            }
        }
        #endregion

        #region Identifier Comparer class
        /// <summary>
        /// This class compares just the identifier in boolean expressions.
        /// </summary>
        private sealed class IdentifierComparer : IEqualityComparer<BoolLiteral>
        {
            public bool Equals(BoolLiteral left, BoolLiteral right)
            {
                // Quick check with references
                if (object.ReferenceEquals(left, right))
                {
                    // Gets the Null and Undefined case as well
                    return true;
                }
                // One of them is non-null at least
                if (left == null || right == null)
                {
                    return false;
                }
                // Both are non-null at this point
                return left.IsIdentifierEqualTo(right);
            }

            public int GetHashCode(BoolLiteral literal)
            {
                return literal.GetIdentifierHash();
            }
        }
        #endregion
    }

    internal abstract class TrueFalseLiteral : BoolLiteral
    {
        internal override DomainBoolExpr GetDomainBoolExpression(MemberDomainMap domainMap)
        {
            // Essentially say that the variable can take values true or false and here its value is only true
            IEnumerable<Constant> actualValues = new Constant[] { new ScalarConstant(true) };
            IEnumerable<Constant> possibleValues = new Constant[] { new ScalarConstant(true), new ScalarConstant(false) };
            Set<Constant> variableDomain = new Set<Constant>(possibleValues, Constant.EqualityComparer).MakeReadOnly();
            Set<Constant> thisDomain = new Set<Constant>(actualValues, Constant.EqualityComparer).MakeReadOnly();

            DomainTermExpr result = MakeTermExpression(this, variableDomain, thisDomain);
            return result;
        }

        internal override DomainBoolExpr FixRange(Set<Constant> range, MemberDomainMap memberDomainMap)
        {
            Debug.Assert(range.Count == 1, "For BoolLiterals, there should be precisely one value - true or false");
            ScalarConstant scalar = (ScalarConstant)range.First();
            DomainBoolExpr expr = GetDomainBoolExpression(memberDomainMap);

            if ((bool)scalar.Value == false)
            {
                // The range of the variable was "inverted". Return a NOT of
                // the expression
                expr = new DomainNotExpr(expr);
            }
            return expr;
        }
    }
}
