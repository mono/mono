//---------------------------------------------------------------------
// <copyright file="FragmentQueryProcessor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Data.Common.Utils;
using System.Data.Common.Utils.Boolean;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Globalization;

namespace System.Data.Mapping.ViewGeneration.QueryRewriting
{
    using BoolDomainConstraint = DomainConstraint<BoolLiteral, Constant>;

    internal class FragmentQueryProcessor : TileQueryProcessor<FragmentQuery>
    {
        private FragmentQueryKB _kb;

        public FragmentQueryProcessor(FragmentQueryKB kb)
        {
            _kb = kb;
        }

        internal static FragmentQueryProcessor Merge(FragmentQueryProcessor qp1, FragmentQueryProcessor qp2)
        {
            FragmentQueryKB mergedKB = new FragmentQueryKB();
            mergedKB.AddKnowledgeBase(qp1.KnowledgeBase);
            mergedKB.AddKnowledgeBase(qp2.KnowledgeBase);
            return new FragmentQueryProcessor(mergedKB);
        }

        internal FragmentQueryKB KnowledgeBase
        {
            get { return _kb; }
        }

        // resulting query contains an intersection of attributes
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCode", Justification = "Based on Bug VSTS Pioneer #433188: IsVisibleOutsideAssembly is wrong on generic instantiations.")]   
        internal override FragmentQuery Union(FragmentQuery q1, FragmentQuery q2)
        {
            HashSet<MemberPath> attributes = new HashSet<MemberPath>(q1.Attributes);
            attributes.IntersectWith(q2.Attributes);

            BoolExpression condition = BoolExpression.CreateOr(q1.Condition, q2.Condition);

            return FragmentQuery.Create(attributes, condition);
        }

        internal bool IsDisjointFrom(FragmentQuery q1, FragmentQuery q2)
        {
            return !IsSatisfiable(Intersect(q1, q2));
        }

        internal bool IsContainedIn(FragmentQuery q1, FragmentQuery q2)
        {
            return !IsSatisfiable(Difference(q1, q2));
        }

        internal bool IsEquivalentTo(FragmentQuery q1, FragmentQuery q2)
        {
            return IsContainedIn(q1, q2) && IsContainedIn(q2, q1);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCode", Justification = "Based on Bug VSTS Pioneer #433188: IsVisibleOutsideAssembly is wrong on generic instantiations.")]
        internal override FragmentQuery Intersect(FragmentQuery q1, FragmentQuery q2)
        {
            HashSet<MemberPath> attributes = new HashSet<MemberPath>(q1.Attributes);
            attributes.IntersectWith(q2.Attributes);

            BoolExpression condition = BoolExpression.CreateAnd(q1.Condition, q2.Condition);

            return FragmentQuery.Create(attributes, condition);
        }

        internal override FragmentQuery Difference(FragmentQuery qA, FragmentQuery qB)
        {
            return FragmentQuery.Create(qA.Attributes, BoolExpression.CreateAndNot(qA.Condition, qB.Condition));
        }

        internal override bool IsSatisfiable(FragmentQuery query)
        {
            return IsSatisfiable(query.Condition);
        }

        private bool IsSatisfiable(BoolExpression condition)
        {
            // instantiate conversion context for each check - gives better performance
            BoolExpression conditionUnderKB = condition.Create(
                new AndExpr<BoolDomainConstraint>(_kb.KbExpression, condition.Tree));
            var context = IdentifierService<BoolDomainConstraint>.Instance.CreateConversionContext();
            var converter = new Converter<BoolDomainConstraint>(conditionUnderKB.Tree, context);
            bool isSatisfiable = converter.Vertex.IsZero() == false;
            return isSatisfiable;
        }

        // creates "derived" views that may be helpful for answering the query
        // for example, view = SELECT ID WHERE B=2, query = SELECT ID,B WHERE B=2
        // Created derived view: SELECT ID,B WHERE B=2 by adding the attribute whose value is determined by the where clause to projected list
        internal override FragmentQuery CreateDerivedViewBySelectingConstantAttributes(FragmentQuery view)
        {
            HashSet<MemberPath> newProjectedAttributes = new HashSet<MemberPath>();
            // collect all variables from the view
            IEnumerable<DomainVariable<BoolLiteral, Constant>> variables = view.Condition.Variables;
            foreach (DomainVariable<BoolLiteral, Constant> var in variables)
            {
                MemberRestriction variableCondition = var.Identifier as MemberRestriction;
                if (variableCondition != null)
                {
                    // Is this attribute not already projected?
                    MemberPath conditionMember = variableCondition.RestrictedMemberSlot.MemberPath;
                    // Iterating through the variable domain var.Domain could be wasteful
                    // Instead, consider the actual condition values on the variable. Usually, they don't get repeated (if not, we could cache and check)
                    Domain conditionValues = variableCondition.Domain;

                    if ((false == view.Attributes.Contains(conditionMember))
                        && !(conditionValues.AllPossibleValues.Any(it => it.HasNotNull()))) //Don't add member to the projected list if the condition involves a 
                    {
                        foreach (Constant value in conditionValues.Values)
                        {
                            // construct constraint: X = value
                            DomainConstraint<BoolLiteral, Constant> constraint = new DomainConstraint<BoolLiteral, Constant>(var,
                                new Set<Constant>(new Constant[] { value }, Constant.EqualityComparer));
                            // is this constraint implied by the where clause?
                            BoolExpression exclusion = view.Condition.Create(
                                new AndExpr<DomainConstraint<BoolLiteral, Constant>>(view.Condition.Tree,
                                new NotExpr<DomainConstraint<BoolLiteral, Constant>>(new TermExpr<DomainConstraint<BoolLiteral, Constant>>(constraint))));
                            bool isImplied = false == IsSatisfiable(exclusion);
                            if (isImplied)
                            {
                                // add this variable to the projection, if it is used in the query
                                newProjectedAttributes.Add(conditionMember);
                            }
                        }
                    }
                }
            }
            if (newProjectedAttributes.Count > 0)
            {
                newProjectedAttributes.UnionWith(view.Attributes);
                FragmentQuery derivedView = new FragmentQuery(String.Format(CultureInfo.InvariantCulture, "project({0})", view.Description), view.FromVariable,
                                                              newProjectedAttributes, view.Condition);
                return derivedView;
            }
            return null;
        }

        public override string ToString()
        {
            return _kb.ToString();
        }

        #region Private class AttributeSetComparator

        private class AttributeSetComparator : IEqualityComparer<HashSet<MemberPath>>
        {
            internal static readonly AttributeSetComparator DefaultInstance = new AttributeSetComparator();

            #region IEqualityComparer<HashSet<MemberPath>> Members

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCode", Justification = "Based on Bug VSTS Pioneer #433188: IsVisibleOutsideAssembly is wrong on generic instantiations.")]
            public bool Equals(HashSet<MemberPath> x, HashSet<MemberPath> y)
            {
                return x.SetEquals(y);
            }

            public int GetHashCode(HashSet<MemberPath> attrs)
            {
                int hashCode = 123;
                foreach (MemberPath attr in attrs)
                {
                    hashCode += MemberPath.EqualityComparer.GetHashCode(attr) * 7;
                }
                return hashCode;
            }
            #endregion
        }

        #endregion
    }

}
