//---------------------------------------------------------------------
// <copyright file="FragmentQuery.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
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
    internal class FragmentQuery : ITileQuery
    {
        private BoolExpression m_fromVariable; // optional
        private string m_label; // optional

        private HashSet<MemberPath> m_attributes;
        private BoolExpression m_condition;

        public HashSet<MemberPath> Attributes
        {
            get { return m_attributes; }
        }

        public BoolExpression Condition
        {
            get { return m_condition; }
        }

        public static FragmentQuery Create(BoolExpression fromVariable, CellQuery cellQuery)
        {
            BoolExpression whereClause = cellQuery.WhereClause;
            whereClause = whereClause.MakeCopy();
            whereClause.ExpensiveSimplify();
            return new FragmentQuery(null /*label*/, fromVariable, new HashSet<MemberPath>(cellQuery.GetProjectedMembers()), whereClause);
        }

        public static FragmentQuery Create(string label, RoleBoolean roleBoolean, CellQuery cellQuery)
        {
            BoolExpression whereClause = cellQuery.WhereClause.Create(roleBoolean);
            whereClause = BoolExpression.CreateAnd(whereClause, cellQuery.WhereClause);
            //return new FragmentQuery(label, null /* fromVariable */, new HashSet<MemberPath>(cellQuery.GetProjectedMembers()), whereClause);
            // don't need any attributes 
            whereClause = whereClause.MakeCopy();
            whereClause.ExpensiveSimplify();
            return new FragmentQuery(label, null /* fromVariable */, new HashSet<MemberPath>(), whereClause);
        }

        public static FragmentQuery Create(IEnumerable<MemberPath> attrs, BoolExpression whereClause)
        {
            return new FragmentQuery(null /* no name */, null /* no fromVariable*/, attrs, whereClause);
        }

        public static FragmentQuery Create(BoolExpression whereClause)
        {
            return new FragmentQuery(null /* no name */, null /* no fromVariable*/, new MemberPath[] { }, whereClause);
        }

        internal FragmentQuery(string label, BoolExpression fromVariable, IEnumerable<MemberPath> attrs, BoolExpression condition)
        {
            m_label = label;
            m_fromVariable = fromVariable;
            m_condition = condition;
            m_attributes = new HashSet<MemberPath>(attrs);
        }

        public BoolExpression FromVariable
        {
            get { return m_fromVariable; }
        }

        public string Description
        {
            get
            {
                string label = m_label;
                if (label == null && m_fromVariable != null)
                {
                    label = m_fromVariable.ToString();
                }
                return label;
            }
        }

        public override string ToString()
        {
            // attributes
            StringBuilder b = new StringBuilder();
            foreach (MemberPath value in this.Attributes)
            {
                if (b.Length > 0) { b.Append(','); }
                b.Append(value.ToString());
            }

            if (Description != null && Description != b.ToString())
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}: [{1} where {2}]", Description, b, this.Condition);
            }
            else
            {
                return String.Format(CultureInfo.InvariantCulture, "[{0} where {1}]", b, this.Condition);
            }
        }

        #region Static methods

        // creates a condition member=value
        internal static BoolExpression CreateMemberCondition(MemberPath path, Constant domainValue, MemberDomainMap domainMap)
        {
            if (domainValue is TypeConstant)
            {
                return BoolExpression.CreateLiteral(new TypeRestriction(new MemberProjectedSlot(path),
                                                    new Domain(domainValue, domainMap.GetDomain(path))), domainMap);
            }
            else
            {
                return BoolExpression.CreateLiteral(new ScalarRestriction(new MemberProjectedSlot(path),
                                                    new Domain(domainValue, domainMap.GetDomain(path))), domainMap);
            }
        }

        internal static IEqualityComparer<FragmentQuery> GetEqualityComparer(FragmentQueryProcessor qp)
        {
            return new FragmentQueryEqualityComparer(qp);
        }

        #endregion

        #region Equality Comparer
        // Two queries are "equal" if they project the same set of attributes
        // and their WHERE clauses are equivalent
        private class FragmentQueryEqualityComparer : IEqualityComparer<FragmentQuery>
        {
            FragmentQueryProcessor _qp;

            internal FragmentQueryEqualityComparer(FragmentQueryProcessor qp)
            {
                _qp = qp;
            }

            #region IEqualityComparer<FragmentQuery> Members

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCode", Justification = "Based on Bug VSTS Pioneer #433188: IsVisibleOutsideAssembly is wrong on generic instantiations.")]       
            public bool Equals(FragmentQuery x, FragmentQuery y)
            {
                if (!x.Attributes.SetEquals(y.Attributes))
                {
                    return false;
                }
                return _qp.IsEquivalentTo(x, y);
            }

            // Hashing a bit naive: it exploits syntactic properties,
            // i.e., some semantically equivalent queries may produce different hash codes
            // But that's fine for usage scenarios in QueryRewriter.cs 
            public int GetHashCode(FragmentQuery q)
            {
                int attrHashCode = 0;
                foreach (MemberPath member in q.Attributes)
                {
                    attrHashCode ^= MemberPath.EqualityComparer.GetHashCode(member);
                }
                int varHashCode = 0;
                int constHashCode = 0;
                foreach (MemberRestriction oneOf in q.Condition.MemberRestrictions)
                {
                    varHashCode ^= MemberPath.EqualityComparer.GetHashCode(oneOf.RestrictedMemberSlot.MemberPath);
                    foreach (Constant constant in oneOf.Domain.Values)
                    {
                        constHashCode ^= Constant.EqualityComparer.GetHashCode(constant);
                    }
                }
                return attrHashCode * 13 + varHashCode * 7 + constHashCode;
            }

            #endregion
        }
        #endregion

    }

}
