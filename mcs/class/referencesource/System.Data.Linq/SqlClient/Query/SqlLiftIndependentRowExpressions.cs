using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// Find projection expressions on the right side of CROSS APPLY that do not 
    /// depend exclusively on right side productions and move them outside of the 
    /// CROSS APPLY by enclosing the CROSS APPLY with a new source.
    /// </summary>
    class SqlLiftIndependentRowExpressions {
        internal static SqlNode Lift(SqlNode node) {
            ColumnLifter cl = new ColumnLifter();
            node = cl.Visit(node);
            return node;
        }

        private class ColumnLifter : SqlVisitor {
            SelectScope expressionSink;
            SqlAggregateChecker aggregateChecker;

            internal ColumnLifter() {
                this.aggregateChecker = new SqlAggregateChecker();
            }

            class SelectScope {
                // Stack of projections lifted from the right to be pushed on the left.
                internal Stack<List<SqlColumn>> Lifted = new Stack<List<SqlColumn>>();
                internal IEnumerable<SqlAlias> LeftProduction;
                internal HashSet<SqlColumn> ReferencedExpressions = new HashSet<SqlColumn>();
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                SelectScope s = expressionSink;
                
                // Don't lift through a TOP.
                if (select.Top != null) {
                    expressionSink = null;
                }

                // Don't lift through a GROUP BY (or implicit GROUP BY).
                if (select.GroupBy.Count > 0 || this.aggregateChecker.HasAggregates(select)) {
                    expressionSink = null;
                }

                // Don't lift through DISTINCT
                if (select.IsDistinct) {
                    expressionSink = null;
                }

                if (expressionSink != null) {
                    List<SqlColumn> keep = new List<SqlColumn>();
                    List<SqlColumn> lift = new List<SqlColumn>();

                    foreach (SqlColumn sc in select.Row.Columns) {
                        bool referencesLeftsideAliases = SqlAliasesReferenced.ReferencesAny(sc.Expression, expressionSink.LeftProduction);
                        bool isLockedExpression = expressionSink.ReferencedExpressions.Contains(sc);
                        if (referencesLeftsideAliases && !isLockedExpression) {
                            lift.Add(sc);
                        } else {
                            keep.Add(sc);
                        }
                    }
                    select.Row.Columns.Clear();
                    select.Row.Columns.AddRange(keep);
                    if (lift.Count > 0) {
                        expressionSink.Lifted.Push(lift);
                    }
                }

                SqlSelect sel = base.VisitSelect(select);
                expressionSink = s;
                return sel;
            }
            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                if (expressionSink!=null) {
                    expressionSink.ReferencedExpressions.Add(cref.Column);
                }
                return cref;
            }
            internal override SqlSource VisitJoin(SqlJoin join) {
                if (join.JoinType == SqlJoinType.CrossApply) { 
                    // Visit the left side as usual.
                    join.Left = this.VisitSource(join.Left);

                    // Visit the condition as usual.
                    join.Condition = this.VisitExpression(join.Condition);

                    // Visit the right, with the expressionSink set.
                    SelectScope s = expressionSink;

                    expressionSink = new SelectScope();
                    expressionSink.LeftProduction = SqlGatherProducedAliases.Gather(join.Left);
                    join.Right = this.VisitSource(join.Right);

                    // Were liftable expressions found?
                    SqlSource newSource = join;
                    foreach (List<SqlColumn> cols in expressionSink.Lifted) {
                        newSource = PushSourceDown(newSource, cols);
                    }
                    expressionSink = s;
                    return newSource;
                }
                return base.VisitJoin(join);
            }
            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private SqlSource PushSourceDown(SqlSource sqlSource, List<SqlColumn> cols) {
                SqlSelect ns = new SqlSelect(new SqlNop(cols[0].ClrType, cols[0].SqlType, sqlSource.SourceExpression), sqlSource, sqlSource.SourceExpression);
                ns.Row.Columns.AddRange(cols);
                return new SqlAlias(ns);
            }
        }
    }
}
