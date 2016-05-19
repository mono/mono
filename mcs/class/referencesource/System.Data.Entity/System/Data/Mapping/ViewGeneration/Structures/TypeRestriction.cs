//---------------------------------------------------------------------
// <copyright file="TypeRestriction.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Metadata.Edm;
using System.Linq;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    using DomainBoolExpr    = System.Data.Common.Utils.Boolean.BoolExpr<System.Data.Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;

    /// <summary>
    /// A class that denotes the boolean expression: "varType in values".
    /// See the comments in <see cref="MemberRestriction"/> for complete and incomplete restriction objects.
    /// </summary>
    internal class TypeRestriction : MemberRestriction
    {
        #region Constructors
        /// <summary>
        /// Creates an incomplete type restriction of the form "<paramref name="member"/> in <paramref name="values"/>".
        /// </summary>
        internal TypeRestriction(MemberPath member, IEnumerable<EdmType> values)
            : base(new MemberProjectedSlot(member), CreateTypeConstants(values))
        { }

        /// <summary>
        /// Creates an incomplete type restriction of the form "<paramref name="member"/> = <paramref name="value"/>".
        /// </summary>
        internal TypeRestriction(MemberPath member, Constant value)
            : base(new MemberProjectedSlot(member), value)
        {
            Debug.Assert(value is TypeConstant || value.IsNull(), "Type or NULL expected.");
        }

        /// <summary>
        /// Creates a complete type restriction of the form "<paramref name="slot"/> in <paramref name="domain"/>".
        /// </summary>
        internal TypeRestriction(MemberProjectedSlot slot, Domain domain)
            : base(slot, domain)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Requires: <see cref="MemberRestriction.IsComplete"/> is true.
        /// </summary>
        internal override DomainBoolExpr FixRange(Set<Constant> range, MemberDomainMap memberDomainMap)
        {
            Debug.Assert(IsComplete, "Ranges are fixed only for complete type restrictions.");
            IEnumerable<Constant> possibleValues = memberDomainMap.GetDomain(RestrictedMemberSlot.MemberPath);
            BoolLiteral newLiteral = new TypeRestriction(RestrictedMemberSlot, new Domain(range, possibleValues));
            return newLiteral.GetDomainBoolExpression(memberDomainMap);
        }

        internal override BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap)
        {
            MemberProjectedSlot newVar = (MemberProjectedSlot)this.RestrictedMemberSlot.RemapSlot(remap);
            return new TypeRestriction(newVar, this.Domain);
        }

        internal override MemberRestriction CreateCompleteMemberRestriction(IEnumerable<Constant> possibleValues)
        {
            Debug.Assert(!this.IsComplete, "CreateCompleteMemberRestriction must be called only for incomplete restrictions.");
            return new TypeRestriction(this.RestrictedMemberSlot, new Domain(this.Domain.Values, possibleValues));
        }

        internal override StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            // Add Cql of the form "(T.A IS OF (ONLY Person) OR .....)"

            // Important to enclose all the OR statements in parens.
            if (this.Domain.Count > 1)
            {
                builder.Append('(');
            }

            bool isFirst = true;
            foreach (Constant constant in this.Domain.Values)
            {
                TypeConstant typeConstant = constant as TypeConstant;
                Debug.Assert(typeConstant != null || constant.IsNull(), "Constants for type checks must be type constants or NULLs");

                if (isFirst == false)
                {
                    builder.Append(" OR ");
                }
                isFirst = false;
                if (Helper.IsRefType(this.RestrictedMemberSlot.MemberPath.EdmType))
                {
                    builder.Append("Deref(");
                    this.RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
                    builder.Append(')');
                }
                else
                {
                    // non-reference type
                    this.RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
                }
                if (constant.IsNull())
                {
                    builder.Append(" IS NULL");
                }
                else
                {
                    // type constant
                    builder.Append(" IS OF (ONLY ");
                    CqlWriter.AppendEscapedTypeName(builder, typeConstant.EdmType);
                    builder.Append(')');
                }
            }

            if (Domain.Count > 1)
            {
                builder.Append(')');
            }

            return builder;
        }

        internal override DbExpression AsCqt(DbExpression row, bool skipIsNotNull)
        {
            DbExpression cqt = this.RestrictedMemberSlot.MemberPath.AsCqt(row);

            if (Helper.IsRefType(this.RestrictedMemberSlot.MemberPath.EdmType))
            {
                cqt = cqt.Deref();
            }

            if (this.Domain.Count == 1)
            {
                // Single value
                cqt = cqt.IsOfOnly(TypeUsage.Create(((TypeConstant)this.Domain.Values.Single()).EdmType));
            }
            else
            {
                // Multiple values: build list of var IsOnOnly(t1), var = IsOnOnly(t1), ..., then OR them all.
                List<DbExpression> operands = this.Domain.Values.Select(t => (DbExpression)cqt.IsOfOnly(TypeUsage.Create(((TypeConstant)t).EdmType))).ToList();
                cqt = Helpers.BuildBalancedTreeInPlace(operands, (prev, next) => prev.Or(next));
            }

            return cqt;
        }

        internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            // Add user readable string of the form "T.A IS a (Person OR .....)"

            if (Helper.IsRefType(RestrictedMemberSlot.MemberPath.EdmType))
            {
                builder.Append("Deref(");
                RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
                builder.Append(')');
            }
            else
            {
                // non-reference type
                RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
            }

            if (Domain.Count > 1)
            {
                builder.Append(" is a (");
            }
            else
            {
                builder.Append(" is type ");
            }

            bool isFirst = true;
            foreach (Constant constant in Domain.Values)
            {
                TypeConstant typeConstant = constant as TypeConstant;
                Debug.Assert(typeConstant != null || constant.IsNull(), "Constants for type checks must be type constants or NULLs");

                if (isFirst == false)
                {
                    builder.Append(" OR ");
                }

                if (constant.IsNull())
                {
                    builder.Append(" NULL");
                }
                else
                {
                    CqlWriter.AppendEscapedTypeName(builder, typeConstant.EdmType);
                }

                isFirst = false;
            }

            if (Domain.Count > 1)
            {
                builder.Append(')');
            }
            return builder;
        }

        /// <summary>
        /// Given a list of <paramref name="types"/> (which can contain nulls), returns a corresponding list of <see cref="TypeConstant"/>s for those types.
        /// </summary>
        private static IEnumerable<Constant> CreateTypeConstants(IEnumerable<EdmType> types)
        {
            foreach (EdmType type in types)
            {
                if (type == null)
                {
                    yield return Constant.Null;
                }
                else
                {
                    yield return new TypeConstant(type);
                }
            }
        }
        #endregion

        #region String methods
        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append("type(");
            RestrictedMemberSlot.ToCompactString(builder);
            builder.Append(") IN (");
            StringUtil.ToCommaSeparatedStringSorted(builder, Domain.Values);
            builder.Append(")");
        }
        #endregion
    }
}
