//---------------------------------------------------------------------
// <copyright file="CaseStatement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Common.Utils;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Data.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Mapping.ViewGeneration.Structures
{
    /// <summary>
    /// A class to denote a case statement:
    /// CASE
    ///     WHEN condition1 THEN value1
    ///     WHEN condition2 THEN value2
    ///     ...
    /// END
    /// </summary>
    internal sealed class CaseStatement : InternalBase
    {
        #region Constructors
        /// <summary>
        /// Creates a case statement for the <paramref name="memberPath"/> with no clauses.
        /// </summary>
        internal CaseStatement(MemberPath memberPath)
        {
            m_memberPath = memberPath;
            m_clauses = new List<WhenThen>();
        }
        #endregion

        #region Fields
        /// <summary>
        /// The field.
        /// </summary>
        private readonly MemberPath m_memberPath;
        /// <summary>
        /// All the WHEN THENs.
        /// </summary>
        private List<WhenThen> m_clauses;
        /// <summary>
        /// Value for the else clause.
        /// </summary>
        private ProjectedSlot m_elseValue;
        private bool m_simplified = false;
        #endregion

        #region Properties
        internal MemberPath MemberPath
        {
            get { return m_memberPath; }
        }

        internal List<WhenThen> Clauses
        {
            get { return m_clauses; }
        }

        internal ProjectedSlot ElseValue
        {
            get { return m_elseValue; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Recursively qualifies all <see cref="ProjectedSlot"/>s and returns a new deeply qualified <see cref="CaseStatement"/>.
        /// </summary>
        internal CaseStatement DeepQualify(CqlBlock block)
        {
            // Go through the whenthens and else and make a new case statement with qualified slots as needed.
            CaseStatement result = new CaseStatement(m_memberPath);
            foreach (WhenThen whenThen in m_clauses)
            {
                WhenThen newClause = whenThen.ReplaceWithQualifiedSlot(block);
                result.m_clauses.Add(newClause);
            }
            if (m_elseValue != null)
            {
                result.m_elseValue = m_elseValue.DeepQualify(block);
            }
            result.m_simplified = m_simplified;
            return result;
        }

        /// <summary>
        /// Adds an expression of the form "WHEN <paramref name="condition"/> THEN <paramref name="value"/>".
        /// This operation is not allowed after the <see cref="Simplify"/> call.
        /// </summary>
        internal void AddWhenThen(BoolExpression condition, ProjectedSlot value)
        {
            Debug.Assert(!m_simplified, "Attempt to modify a simplified case statement");
            Debug.Assert(value != null);

            condition.ExpensiveSimplify();
            m_clauses.Add(new WhenThen(condition, value));
        }

        /// <summary>
        /// Returns true if the <see cref="CaseStatement"/> depends on (projects) its slot in THEN value or ELSE value.
        /// </summary>
        internal bool DependsOnMemberValue
        {
            get
            {
                if (m_elseValue is MemberProjectedSlot)
                {
                    Debug.Assert(m_memberPath.Equals(((MemberProjectedSlot)m_elseValue).MemberPath), "case statement slot (ELSE) must depend only on its own slot value");
                    return true;
                }
                foreach (WhenThen whenThen in m_clauses)
                {
                    if (whenThen.Value is MemberProjectedSlot)
                    {
                        Debug.Assert(m_memberPath.Equals(((MemberProjectedSlot)whenThen.Value).MemberPath), "case statement slot (THEN) must depend only on its own slot value");
                        return true;
                    }
                }
                return false;
            }
        }

        internal IEnumerable<EdmType> InstantiatedTypes
        {
            get
            {
                foreach (WhenThen whenThen in m_clauses)
                {
                    EdmType type;
                    if (TryGetInstantiatedType(whenThen.Value, out type))
                    {
                        yield return type;
                    }
                }
                EdmType elseType;
                if (TryGetInstantiatedType(m_elseValue, out elseType))
                {
                    yield return elseType;
                }
            }
        }

        private bool TryGetInstantiatedType(ProjectedSlot slot, out EdmType type)
        {
            type = null;
            ConstantProjectedSlot constantSlot = slot as ConstantProjectedSlot;
            if (constantSlot != null)
            {
                TypeConstant typeConstant = constantSlot.CellConstant as TypeConstant;
                if (typeConstant != null)
                {
                    type = typeConstant.EdmType;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Simplifies the <see cref="CaseStatement"/> so that unnecessary WHEN/THENs for nulls/undefined values are eliminated. 
        /// Also, adds an ELSE clause if possible.
        /// </summary>
        internal void Simplify()
        {
            if (m_simplified)
            {
                return;
            }

            List<CaseStatement.WhenThen> clauses = new List<CaseStatement.WhenThen>();
            // remove all WHEN clauses where the value gets set to "undefined"
            // We eliminate the last clause for now - we could determine the
            // "most complicated" WHEN clause and eliminate it
            bool eliminatedNullClauses = false;
            foreach (WhenThen clause in m_clauses)
            {
                ConstantProjectedSlot constantSlot = clause.Value as ConstantProjectedSlot;
                // If null or undefined, remove it
                if (constantSlot != null && (constantSlot.CellConstant.IsNull() || constantSlot.CellConstant.IsUndefined()))
                {
                    eliminatedNullClauses = true;
                }
                else
                {
                    clauses.Add(clause);
                    if (clause.Condition.IsTrue)
                    {
                        // none of subsequent case statements will be evaluated - ignore them
                        break;
                    }
                }
            }

            if (eliminatedNullClauses && clauses.Count == 0)
            {
                // There is nothing left -- we should add a null as the value
                m_elseValue = new ConstantProjectedSlot(Constant.Null, m_memberPath);
            }

            // If we eliminated some undefined or null clauses, we do not want an else clause
            if (clauses.Count > 0 && false == eliminatedNullClauses)
            {
                // turn the last WHEN clause into an ELSE
                int lastIndex = clauses.Count - 1;
                m_elseValue = clauses[lastIndex].Value;
                clauses.RemoveAt(lastIndex);
            }
            m_clauses = clauses;

            m_simplified = true;
        }

        /// <summary>
        /// Generates eSQL for the current <see cref="CaseStatement"/>.
        /// </summary>
        internal StringBuilder AsEsql(StringBuilder builder, IEnumerable<WithRelationship> withRelationships, string blockAlias, int indentLevel)
        {
            if (this.Clauses.Count == 0)
            {
                // This is just a single ELSE: no condition at all.
                Debug.Assert(this.ElseValue != null, "CASE statement with no WHEN/THENs must have ELSE.");
                CaseSlotValueAsEsql(builder, this.ElseValue, this.MemberPath, blockAlias, withRelationships, indentLevel);
                return builder;
            }

            // Generate the Case WHEN .. THEN ..., WHEN ... THEN ..., END
            builder.Append("CASE");
            foreach (CaseStatement.WhenThen clause in this.Clauses)
            {
                StringUtil.IndentNewLine(builder, indentLevel + 2);
                builder.Append("WHEN ");
                clause.Condition.AsEsql(builder, blockAlias);
                builder.Append(" THEN ");
                CaseSlotValueAsEsql(builder, clause.Value, this.MemberPath, blockAlias, withRelationships, indentLevel + 2);
            }

            if (this.ElseValue != null)
            {
                StringUtil.IndentNewLine(builder, indentLevel + 2);
                builder.Append("ELSE ");
                CaseSlotValueAsEsql(builder, this.ElseValue, this.MemberPath, blockAlias, withRelationships, indentLevel + 2);
            }
            StringUtil.IndentNewLine(builder, indentLevel + 1);
            builder.Append("END");
            return builder;
        }

        /// <summary>
        /// Generates CQT for the current <see cref="CaseStatement"/>.
        /// </summary>
        internal DbExpression AsCqt(DbExpression row, IEnumerable<WithRelationship> withRelationships)
        {
            // Generate the Case WHEN .. THEN ..., WHEN ... THEN ..., END
            List<DbExpression> conditions = new List<DbExpression>();
            List<DbExpression> values = new List<DbExpression>();
            foreach (CaseStatement.WhenThen clause in this.Clauses)
            {
                conditions.Add(clause.Condition.AsCqt(row));
                values.Add(CaseSlotValueAsCqt(row, clause.Value, this.MemberPath, withRelationships));
            }

            // Generate ELSE
            DbExpression elseValue = this.ElseValue != null ?
                CaseSlotValueAsCqt(row, this.ElseValue, this.MemberPath, withRelationships) :
                Constant.Null.AsCqt(row, this.MemberPath);

            if (this.Clauses.Count > 0)
            {
                return DbExpressionBuilder.Case(conditions, values, elseValue);
            }
            else
            {
                Debug.Assert(elseValue != null, "CASE statement with no WHEN/THENs must have ELSE.");
                return elseValue;
            }
        }

        private static StringBuilder CaseSlotValueAsEsql(StringBuilder builder, ProjectedSlot slot, MemberPath outputMember, string blockAlias, IEnumerable<WithRelationship> withRelationships, int indentLevel)
        {
            // We should never have THEN as a BooleanProjectedSlot.
            Debug.Assert(slot is MemberProjectedSlot || slot is QualifiedSlot || slot is ConstantProjectedSlot,
                         "Case statement THEN can only have constants or members.");
            slot.AsEsql(builder, outputMember, blockAlias, 1);
            WithRelationshipsClauseAsEsql(builder, withRelationships, blockAlias, indentLevel, slot);
            return builder;
        }

        private static void WithRelationshipsClauseAsEsql(StringBuilder builder, IEnumerable<WithRelationship> withRelationships, string blockAlias, int indentLevel, ProjectedSlot slot)
        {
            bool first = true;
            WithRelationshipsClauseAsCql(
                // emitWithRelationship action
                (withRelationship) =>
                {
                    if (first)
                    {
                        builder.Append(" WITH ");
                        first = false;
                    }
                    withRelationship.AsEsql(builder, blockAlias, indentLevel);
                },
                withRelationships,
                slot);
        }

        private static DbExpression CaseSlotValueAsCqt(DbExpression row, ProjectedSlot slot, MemberPath outputMember, IEnumerable<WithRelationship> withRelationships)
        {
            // We should never have THEN as a BooleanProjectedSlot.
            Debug.Assert(slot is MemberProjectedSlot || slot is QualifiedSlot || slot is ConstantProjectedSlot,
                         "Case statement THEN can only have constants or members.");
            DbExpression cqt = slot.AsCqt(row, outputMember);
            cqt = WithRelationshipsClauseAsCqt(row, cqt, withRelationships, slot);
            return cqt;
        }

        private static DbExpression WithRelationshipsClauseAsCqt(DbExpression row, DbExpression slotValueExpr, IEnumerable<WithRelationship> withRelationships, ProjectedSlot slot)
        {
            List<DbRelatedEntityRef> relatedEntityRefs = new List<DbRelatedEntityRef>();
            WithRelationshipsClauseAsCql(
                // emitWithRelationship action
                (withRelationship) =>
                {
                    relatedEntityRefs.Add(withRelationship.AsCqt(row));
                },
                withRelationships,
                slot);

            if (relatedEntityRefs.Count > 0)
            {
                DbNewInstanceExpression typeConstructor = slotValueExpr as DbNewInstanceExpression;
                Debug.Assert(typeConstructor != null && typeConstructor.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.EntityType,
                    "WITH RELATIONSHIP clauses should be specified for entity type constructors only.");
                return DbExpressionBuilder.CreateNewEntityWithRelationshipsExpression(
                    (EntityType)typeConstructor.ResultType.EdmType,
                    typeConstructor.Arguments,
                    relatedEntityRefs);
            }
            else
            {
                return slotValueExpr;
            }
        }

        private static void WithRelationshipsClauseAsCql(Action<WithRelationship> emitWithRelationship, IEnumerable<WithRelationship> withRelationships, ProjectedSlot slot)
        {
            if (withRelationships != null && withRelationships.Count() > 0)
            {
                ConstantProjectedSlot constantSlot = slot as ConstantProjectedSlot;
                Debug.Assert(constantSlot != null, "WITH RELATIONSHIP clauses should be specified for type constant slots only.");
                TypeConstant typeConstant = constantSlot.CellConstant as TypeConstant;
                Debug.Assert(typeConstant != null, "WITH RELATIONSHIP clauses should be there for type constants only.");
                EdmType fromType = typeConstant.EdmType;

                foreach (WithRelationship withRelationship in withRelationships)
                {
                    // Add With statement for the types that participate in the association.
                    if (withRelationship.FromEndEntityType.IsAssignableFrom(fromType))
                    {
                        emitWithRelationship(withRelationship);
                    }
                }
            }
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            builder.AppendLine("CASE");
            foreach (WhenThen clause in m_clauses)
            {
                builder.Append(" WHEN ");
                clause.Condition.ToCompactString(builder);
                builder.Append(" THEN ");
                clause.Value.ToCompactString(builder);
                builder.AppendLine();
            }
            if (m_elseValue != null)
            {
                builder.Append(" ELSE ");
                m_elseValue.ToCompactString(builder);
                builder.AppendLine();
            }
            builder.Append(" END AS ");
            m_memberPath.ToCompactString(builder);
        }
        #endregion

        /// <summary>
        /// A class that stores WHEN condition THEN value.
        /// </summary>
        internal sealed class WhenThen : InternalBase
        {
            #region Constructor
            /// <summary>
            /// Creates WHEN condition THEN value.
            /// </summary>
            internal WhenThen(BoolExpression condition, ProjectedSlot value)
            {
                m_condition = condition;
                m_value = value;
            }
            #endregion

            #region Fields
            private readonly BoolExpression m_condition;
            private readonly ProjectedSlot m_value;
            #endregion

            #region Properties
            /// <summary>
            /// Returns WHEN condition.
            /// </summary>
            internal BoolExpression Condition
            {
                get { return m_condition; }
            }

            /// <summary>
            /// Returns THEN value.
            /// </summary>
            internal ProjectedSlot Value
            {
                get { return m_value; }
            }
            #endregion

            #region String Methods
            internal WhenThen ReplaceWithQualifiedSlot(CqlBlock block)
            {
                // Change the THEN part
                ProjectedSlot newValue = m_value.DeepQualify(block);
                return new WhenThen(m_condition, newValue);
            }

            internal override void ToCompactString(StringBuilder builder)
            {
                builder.Append("WHEN ");
                m_condition.ToCompactString(builder);
                builder.Append("THEN ");
                m_value.ToCompactString(builder);
            }
            #endregion
        }
    }
}
