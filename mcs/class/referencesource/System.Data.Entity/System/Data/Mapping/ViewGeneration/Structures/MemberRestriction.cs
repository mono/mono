//---------------------------------------------------------------------
// <copyright file="MemberRestriction.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common.Utils;
using System.Data.Entity;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    using DomainBoolExpr    = System.Data.Common.Utils.Boolean.BoolExpr<System.Data.Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;
    using DomainTermExpr    = System.Data.Common.Utils.Boolean.TermExpr<System.Data.Common.Utils.Boolean.DomainConstraint<BoolLiteral, Constant>>;

    /// <summary>
    /// An abstract class that denotes the boolean expression: "var in values".
    /// An object of this type can be complete or incomplete. 
    /// An incomplete object is one whose domain was not created with all possible values.
    /// Incomplete objects have a limited set of methods that can be called.
    /// </summary>
    internal abstract class MemberRestriction : BoolLiteral
    {
        #region Constructors
        /// <summary>
        /// Creates an incomplete member restriction with the meaning "<paramref name="slot"/> = <paramref name="value"/>".
        /// "Partial" means that the <see cref="Domain"/> in this restriction is partial - hence the operations on the restriction are limited.
        /// </summary>
        protected MemberRestriction(MemberProjectedSlot slot, Constant value)
            : this(slot, new Constant[] { value })
        { }

        /// <summary>
        /// Creates an incomplete member restriction with the meaning "<paramref name="slot"/> in <paramref name="values"/>".
        /// </summary>
        protected MemberRestriction(MemberProjectedSlot slot, IEnumerable<Constant> values)
        {
            m_restrictedMemberSlot = slot;
            m_domain = new Domain(values, values);
        }

        /// <summary>
        /// Creates a complete member restriction with the meaning "<paramref name="slot"/> in <paramref name="domain"/>".
        /// </summary>
        protected MemberRestriction(MemberProjectedSlot slot, Domain domain)
        {
            m_restrictedMemberSlot = slot;
            m_domain = domain;
            m_isComplete = true;
            Debug.Assert(m_domain.Count != 0, "If you want a boolean that evaluates to false, " +
                         "use the ConstantBool abstraction");
        }

        /// <summary>
        /// Creates a complete member restriction with the meaning "<paramref name="slot"/> in <paramref name="values"/>".
        /// </summary>
        /// <param name="possibleValues">all the values that the <paramref name="slot"/> can take</param>
        protected MemberRestriction(MemberProjectedSlot slot, IEnumerable<Constant> values, IEnumerable<Constant> possibleValues)
            : this(slot, new Domain(values, possibleValues))
        {
            Debug.Assert(possibleValues != null);
        }
        #endregion

        #region Fields
        private readonly MemberProjectedSlot m_restrictedMemberSlot;
        private readonly Domain m_domain;
        private readonly bool m_isComplete;
        #endregion

        #region Properties
        internal bool IsComplete
        {
            get { return m_isComplete; }
        }

        /// <summary>
        /// Returns the variable in the member restriction.
        /// </summary>
        internal MemberProjectedSlot RestrictedMemberSlot
        {
            get { return m_restrictedMemberSlot; }
        }

        /// <summary>
        /// Returns the values that <see cref="RestrictedMemberSlot"/> is being checked for.
        /// </summary>
        internal Domain Domain
        {
            get { return m_domain; }
        }
        #endregion

        #region BoolLiteral Members
        /// <summary>
        /// Returns a boolean expression that is domain-aware and ready for optimizations etc.
        /// </summary>
        /// <param name="domainMap">Maps members to the values that each member can take;
        /// it can be null in which case the possible and actual values are the same.</param>
        internal override DomainBoolExpr GetDomainBoolExpression(MemberDomainMap domainMap)
        {
            // Get the variable name from the slot's memberpath and the possible domain values from the slot
            DomainTermExpr result;
            if (domainMap != null)
            {
                // Look up the domain from the domainMap
                IEnumerable<Constant> domain = domainMap.GetDomain(m_restrictedMemberSlot.MemberPath);
                result = MakeTermExpression(this, domain, m_domain.Values);
            }
            else
            {
                result = MakeTermExpression(this, m_domain.AllPossibleValues, m_domain.Values);
            }
            return result;
        }

        /// <summary>
        /// Creates a complete member restriction based on the existing restriction with possible values for the domain being given by <paramref name="possibleValues"/>.
        /// </summary>
        internal abstract MemberRestriction CreateCompleteMemberRestriction(IEnumerable<Constant> possibleValues);

        /// <summary>
        /// See <see cref="BoolLiteral.GetRequiredSlots"/>.
        /// </summary>
        internal override void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
        {
            // Simply get the slot for the variable var in "var in values"
            MemberPath member = RestrictedMemberSlot.MemberPath;
            int slotNum = projectedSlotMap.IndexOf(member);
            requiredSlots[slotNum] = true;
        }

        /// <summary>
        /// See <see cref="BoolLiteral.IsEqualTo"/>. Member restriction can be incomplete for this operation. 
        /// </summary>
        protected override bool IsEqualTo(BoolLiteral right)
        {
            MemberRestriction rightRestriction= right as MemberRestriction;
            if (rightRestriction == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, rightRestriction))
            {
                return true;
            }
            if (false == MemberProjectedSlot.EqualityComparer.Equals(m_restrictedMemberSlot, rightRestriction.m_restrictedMemberSlot))
            {
                return false;
            }

            return m_domain.IsEqualTo(rightRestriction.m_domain);
        }

        /// <summary>
        /// Member restriction can be incomplete for this operation. 
        /// </summary>
        public override int GetHashCode()
        {
            int result = MemberProjectedSlot.EqualityComparer.GetHashCode(m_restrictedMemberSlot);
            result ^= m_domain.GetHash();
            return result;
        }

        /// <summary>
        /// See <see cref="BoolLiteral.IsIdentifierEqualTo"/>. Member restriction can be incomplete for this operation. 
        /// </summary>
        protected override bool IsIdentifierEqualTo(BoolLiteral right)
        {
            MemberRestriction rightOneOfConst = right as MemberRestriction;
            if (rightOneOfConst == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, rightOneOfConst))
            {
                return true;
            }
            return MemberProjectedSlot.EqualityComparer.Equals(m_restrictedMemberSlot, rightOneOfConst.m_restrictedMemberSlot);
        }

        /// <summary>
        /// See <see cref="BoolLiteral.GetIdentifierHash"/>. Member restriction can be incomplete for this operation. 
        /// </summary>
        protected override int GetIdentifierHash()
        {
            int result = MemberProjectedSlot.EqualityComparer.GetHashCode(m_restrictedMemberSlot);
            return result;
        }
        #endregion

        #region Other Methods
        /// <summary>
        /// Converts this to a user-understandable string.
        /// </summary>
        /// <param name="invertOutput">indicates whether the text needs to say "x in .." or "x in NOT ..."  (i.e., the latter if <paramref name="invertOutput"/> is true)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void ToUserString(bool invertOutput, StringBuilder builder, MetadataWorkspace workspace)
        {
            // If there is a negated cell constant, get the inversion of the domain
            NegatedConstant negatedConstant = null;
            foreach (Constant constant in Domain.Values)
            {
                negatedConstant = constant as NegatedConstant;
                if (negatedConstant != null)
                {
                    break;
                }
            }

            Set<Constant> constants;
            if (negatedConstant != null)
            {
                // Invert the domain and invert "invertOutput"
                invertOutput = !invertOutput;
                // Add all the values to negatedConstant's values to get the
                // final set of constants
                constants = new Set<Constant>(negatedConstant.Elements, Constant.EqualityComparer);
                foreach (Constant constant in Domain.Values)
                {
                    if (!(constant is NegatedConstant))
                    {
                        Debug.Assert(constants.Contains(constant), "Domain of negated constant does not have positive constant");
                        constants.Remove(constant);
                    }
                }
            }
            else
            {
                constants = new Set<Constant>(Domain.Values, Constant.EqualityComparer);
            }

            // Determine the resource to use
            Debug.Assert(constants.Count > 0, "one of const is false?");
            bool isNull = constants.Count == 1 && constants.Single().IsNull();
            bool isTypeConstant = this is TypeRestriction;

            Func<object, string> resourceName0 = null;
            Func<object, object, string> resourceName1 = null;

            if (invertOutput)
            {
                if (isNull)
                {
                    resourceName0 = isTypeConstant ? (Func<object, string>)Strings.ViewGen_OneOfConst_IsNonNullable : (Func<object, string>)Strings.ViewGen_OneOfConst_MustBeNonNullable;
                }
                else if (constants.Count == 1)
                {
                    resourceName1 = isTypeConstant ? (Func<object, object, string>)Strings.ViewGen_OneOfConst_IsNotEqualTo : (Func<object, object, string>)Strings.ViewGen_OneOfConst_MustNotBeEqualTo;
                }
                else
                {
                    resourceName1 = isTypeConstant ? (Func<object, object, string>)Strings.ViewGen_OneOfConst_IsNotOneOf : (Func<object, object, string>)Strings.ViewGen_OneOfConst_MustNotBeOneOf;
                }
            }
            else
            {
                if (isNull)
                {
                    resourceName0 = isTypeConstant ? (Func<object, string>)Strings.ViewGen_OneOfConst_MustBeNull : (Func<object, string>)Strings.ViewGen_OneOfConst_MustBeNull;
                }
                else if (constants.Count == 1)
                {
                    resourceName1 = isTypeConstant ? (Func<object, object, string>)Strings.ViewGen_OneOfConst_IsEqualTo : (Func<object, object, string>)Strings.ViewGen_OneOfConst_MustBeEqualTo;
                }
                else
                {
                    resourceName1 = isTypeConstant ? (Func<object, object, string>)Strings.ViewGen_OneOfConst_IsOneOf : (Func<object, object, string>)Strings.ViewGen_OneOfConst_MustBeOneOf;
                }
            }

            // Get the constants
            StringBuilder constantBuilder = new StringBuilder();
            Constant.ConstantsToUserString(constantBuilder, constants);

            Debug.Assert((resourceName0 == null) != (resourceName1 == null),
                         "Both resources must not have been set or be null");
            string variableName = m_restrictedMemberSlot.MemberPath.PathToString(false);
            if (isTypeConstant)
            {
                variableName = "TypeOf(" + variableName + ")";
            }

            if (resourceName0 != null)
            {
                builder.Append(resourceName0(variableName));
            }
            else
            {
                builder.Append(resourceName1(variableName, constantBuilder.ToString()));
            }

            if (invertOutput && isTypeConstant)
            {
                InvertOutputStringForTypeConstant(builder, constants, workspace);
            }
        }

        /// <summary>
        /// Modifies builder to contain a message for inverting the typeConstants, i.e., NOT(p in Person) becomes p in Customer.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void InvertOutputStringForTypeConstant(StringBuilder builder, Set<Constant> constants, MetadataWorkspace workspace)
        {
            // Get all the types that this type can take (i.e., all
            // subtypes - the types present in this
            StringBuilder typeBuilder = new StringBuilder();
            Set<EdmType> allTypes = new Set<EdmType>();

            // Get all types
            EdmType memberType = RestrictedMemberSlot.MemberPath.EdmType;
            foreach (EdmType type in MetadataHelper.GetTypeAndSubtypesOf(memberType, workspace, false))
            {
                allTypes.Add(type);
            }

            // Get the types in this
            Set<EdmType> oneOfTypes = new Set<EdmType>();
            foreach (Constant constant in constants)
            {
                TypeConstant typeConstant = (TypeConstant)constant;
                oneOfTypes.Add(typeConstant.EdmType);
            }

            // Get the difference
            allTypes.Subtract(oneOfTypes);
            bool isFirst = true;
            foreach (EdmType type in allTypes)
            {
                if (isFirst == false)
                {
                    typeBuilder.Append(System.Data.Entity.Strings.ViewGen_CommaBlank);
                }
                isFirst = false;
                typeBuilder.Append(type.Name);
            }
            builder.Append(Strings.ViewGen_OneOfConst_IsOneOfTypes(typeBuilder.ToString()));
        }

        internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            return AsEsql(builder, blockAlias, skipIsNotNull);
        }

        internal override StringBuilder AsNegatedUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            builder.Append("NOT(");
            builder = AsUserString(builder, blockAlias, skipIsNotNull);
            builder.Append(")");
            return builder;
        }
        #endregion
    }
}
