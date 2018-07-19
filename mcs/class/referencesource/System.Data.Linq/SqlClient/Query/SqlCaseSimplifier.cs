using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Provider;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// SQL with CASE statements is harder to read. This visitor attempts to reduce CASE
    /// statements to equivalent (but easier to read) logic.
    /// </summary>
    internal class SqlCaseSimplifier {
        internal static SqlNode Simplify(SqlNode node, SqlFactory sql) {
            return new Visitor(sql).Visit(node);
        }
        class Visitor : SqlVisitor {
            SqlFactory sql;

            internal Visitor(SqlFactory sql) {
                this.sql = sql;
            }

            /// <summary>
            /// Replace equals and not equals:
            /// 
            /// | CASE XXX              |               CASE XXX                            CASE XXX             
            /// |   WHEN AAA THEN MMMM  | != RRRR  ===>    WHEN AAA THEN (MMMM != RRRR) ==>    WHEN AAA THEN true
            /// |   WHEN BBB THEN NNNN  |                  WHEN BBB THEN (NNNN != RRRR)        WHEN BBB THEN false
            /// |   etc.                |                  etc.                                etc.               
            /// |   ELSE OOOO           |                  ELSE (OOOO != RRRR)                 ELSE true
            /// | END                                   END                                 END
            /// 
            /// Where MMMM, NNNN and RRRR are constants. 
            /// </summary>
            internal override SqlExpression VisitBinaryOperator(SqlBinary bo) {
                switch (bo.NodeType) {
                    case SqlNodeType.EQ:
                    case SqlNodeType.NE:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.NE2V:
                        if (bo.Left.NodeType == SqlNodeType.SimpleCase && 
                            bo.Right.NodeType == SqlNodeType.Value && 
                            AreCaseWhenValuesConstant((SqlSimpleCase)bo.Left)) {
                            return this.DistributeOperatorIntoCase(bo.NodeType, (SqlSimpleCase)bo.Left, bo.Right);
                        } 
                        else if (bo.Right.NodeType == SqlNodeType.SimpleCase && 
                            bo.Left.NodeType==SqlNodeType.Value &&
                            AreCaseWhenValuesConstant((SqlSimpleCase)bo.Right)) {
                            return this.DistributeOperatorIntoCase(bo.NodeType, (SqlSimpleCase)bo.Right, bo.Left);
                        } 
                        break;
                }
                return base.VisitBinaryOperator(bo);
            }

            /// <summary>
            /// Checks to see if all SqlSimpleCase when values are of Value type.
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            internal bool AreCaseWhenValuesConstant(SqlSimpleCase sc) {
                foreach (SqlWhen when in sc.Whens) {
                    if (when.Value.NodeType != SqlNodeType.Value) {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// Helper for VisitBinaryOperator. Builds the new case with distributed valueds.
            /// </summary>
            private SqlExpression DistributeOperatorIntoCase(SqlNodeType nt, SqlSimpleCase sc, SqlExpression expr) {
                if (nt!=SqlNodeType.EQ && nt!=SqlNodeType.NE && nt!=SqlNodeType.EQ2V && nt!=SqlNodeType.NE2V)
                    throw Error.ArgumentOutOfRange("nt");
                object val = Eval(expr);
                List<SqlExpression> values = new List<SqlExpression>();
                List<SqlExpression> matches = new List<SqlExpression>();
                foreach(SqlWhen when in sc.Whens) {
                    matches.Add(when.Match);
                    object whenVal = Eval(when.Value);
                    bool eq = when.Value.SqlType.AreValuesEqual(whenVal, val);
                    values.Add(sql.ValueFromObject((nt==SqlNodeType.EQ || nt==SqlNodeType.EQ2V) == eq, false, sc.SourceExpression));
                }
                return this.VisitExpression(sql.Case(typeof(bool), sc.Expression, matches, values, sc.SourceExpression));
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c) {
                c.Expression = this.VisitExpression(c.Expression);
                int compareWhen = 0;

                // Find the ELSE if it exists.
                for (int i = 0, n = c.Whens.Count; i < n; i++) {
                    if (c.Whens[i].Match == null) {
                        compareWhen = i;
                        break;
                    }
                }

                c.Whens[compareWhen].Match = VisitExpression(c.Whens[compareWhen].Match);
                c.Whens[compareWhen].Value = VisitExpression(c.Whens[compareWhen].Value);

                // Compare each other when value to the compare when
                List<SqlWhen> newWhens = new List<SqlWhen>();
                bool allValuesLiteral = true;
                for (int i = 0, n = c.Whens.Count; i < n; i++) {
                    if (compareWhen != i) {
                        SqlWhen when = c.Whens[i];
                        when.Match = this.VisitExpression(when.Match);
                        when.Value = this.VisitExpression(when.Value);
                        if (!SqlComparer.AreEqual(c.Whens[compareWhen].Value, when.Value)) {
                            newWhens.Add(when);
                        }
                        allValuesLiteral = allValuesLiteral && when.Value.NodeType == SqlNodeType.Value;
                    }
                }
                newWhens.Add(c.Whens[compareWhen]);

                // Did everything reduce to a single CASE?
                SqlExpression rewrite = TryToConsolidateAllValueExpressions(newWhens.Count, c.Whens[compareWhen].Value);
                if (rewrite != null)
                    return rewrite;

                // Can it be a conjuction (or disjunction) of clauses?
                rewrite = TryToWriteAsSimpleBooleanExpression(c.ClrType, c.Expression, newWhens, allValuesLiteral);
                if (rewrite != null)
                    return rewrite;

                // Can any WHEN clauses be reduced to fall into the ELSE clause? 
                rewrite = TryToWriteAsReducedCase(c.ClrType, c.Expression, newWhens, c.Whens[compareWhen].Match, c.Whens.Count);
                if (rewrite != null)
                    return rewrite;

                return c;
            }

            /// <summary>
            /// When there is exactly one when clause in the CASE:
            /// 
            ///  CASE XXX
            ///    WHEN AAA THEN YYY        ===>        YYY
            ///  END
            /// 
            /// Then, just reduce it to the value.
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private SqlExpression TryToConsolidateAllValueExpressions(int valueCount, SqlExpression value) {
                if (valueCount == 1) {
                    return value;
                }
                return null;
            }

            /// <summary>
            /// For CASE statements which represent boolean values:
            /// 
            ///  CASE XXX
            ///    WHEN AAA THEN true        ===>        (XXX==AAA) || (XXX==BBB)
            ///    WHEN BBB THEN true
            ///    ELSE false
            ///    etc.
            ///  END
            ///
            /// Also,
            /// 
            ///  CASE XXX
            ///    WHEN AAA THEN false        ===>        (XXX!=AAA) && (XXX!=BBB)
            ///    WHEN BBB THEN false
            ///    ELSE true
            ///    etc.
            ///  END            
            ///   
            /// The reduce to a conjunction or disjunction of equality or inequality.
            /// The possibility of NULL in XXX is taken into account.
            /// </summary>
            private SqlExpression TryToWriteAsSimpleBooleanExpression(Type caseType, SqlExpression discriminator, List<SqlWhen> newWhens, bool allValuesLiteral) {
                SqlExpression rewrite = null;
                if (caseType == typeof(bool) && allValuesLiteral) {
                    bool? holdsNull = SqlExpressionNullability.CanBeNull(discriminator);
                    // The discriminator can't hold a NULL.
                    // In this case, we don't need the special fallback that CASE-ELSE gives.
                    // We can just construct a boolean operation.
                    bool? whenValue = null;
                    for (int i = 0; i < newWhens.Count; ++i) {
                        SqlValue lit = (SqlValue)newWhens[i].Value; // Must be SqlValue because of allValuesLiteral.
                        bool value = (bool)lit.Value; // Must be bool because of caseType==typeof(bool).
                        if (newWhens[i].Match != null) { // Skip the ELSE
                            if (value) {
                                rewrite = sql.OrAccumulate(rewrite, sql.Binary(SqlNodeType.EQ, discriminator, newWhens[i].Match));
                            }
                            else {
                                rewrite = sql.AndAccumulate(rewrite, sql.Binary(SqlNodeType.NE, discriminator, newWhens[i].Match));
                            }
                        }
                        else {
                            whenValue = value;
                        }
                    }
                    // If it could possibly hold null values.
                    if (holdsNull != false && whenValue != null) {
                        if (whenValue == true) {
                            rewrite = sql.OrAccumulate(rewrite, sql.Unary(SqlNodeType.IsNull, discriminator, discriminator.SourceExpression));
                        }
                        else {
                            rewrite = sql.AndAccumulate(rewrite, sql.Unary(SqlNodeType.IsNotNull, discriminator, discriminator.SourceExpression));
                        }
                    }

                }
                return rewrite;
            }


            /// <summary>
            /// Remove any WHEN clauses which have the same value as ELSE.
            /// 
            ///  CASE XXX                          CASE XXX
            ///    WHEN AAA THEN YYY        ===>     WHEN AAA THEN YYY
            ///    WHEN BBB THEN ZZZ                 WHEN CCC THEN YYY
            ///    WHEN CCC THEN YYY                 ELSE ZZZ
            ///    ELSE ZZZ                        END 
            ///  END
            /// 
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private SqlExpression TryToWriteAsReducedCase(Type caseType, SqlExpression discriminator, List<SqlWhen> newWhens, SqlExpression elseCandidate, int originalWhenCount) {
                if (newWhens.Count != originalWhenCount) {
                    // Some whens were the same as the comparand.
                    if (elseCandidate == null) {
                        // -and- the comparand is ELSE (value == null).
                        // In this case, simplify the CASE to eliminate everything equivalent to ELSE.
                        return new SqlSimpleCase(caseType, discriminator, newWhens, discriminator.SourceExpression);
                    }
                }
                return null;
            }
        }
    }
}
