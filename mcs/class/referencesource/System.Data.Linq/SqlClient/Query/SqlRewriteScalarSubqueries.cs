using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data.Linq;

namespace System.Data.Linq.SqlClient {

    // converts correlated scalar subqueries into outer-applies
    // must be run after flattener.
    internal class SqlRewriteScalarSubqueries {
        Visitor visitor;

        internal SqlRewriteScalarSubqueries(SqlFactory sqlFactory) {
            this.visitor = new Visitor(sqlFactory);
        }

        internal SqlNode Rewrite(SqlNode node) {
            return this.visitor.Visit(node);
        }

        class Visitor : SqlVisitor {
            SqlFactory sql;
            SqlSelect currentSelect;
            SqlAggregateChecker aggregateChecker;

            internal Visitor(SqlFactory sqlFactory) {
                this.sql = sqlFactory;
                this.aggregateChecker = new SqlAggregateChecker();
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
                SqlSelect innerSelect = this.VisitSelect(ss.Select);
                if (!this.aggregateChecker.HasAggregates(innerSelect)) {
                    innerSelect.Top = this.sql.ValueFromObject(1, ss.SourceExpression);
                }
                innerSelect.OrderingType = SqlOrderingType.Blocked;
                SqlAlias alias = new SqlAlias(innerSelect);
                this.currentSelect.From = new SqlJoin(SqlJoinType.OuterApply, this.currentSelect.From, alias, null, ss.SourceExpression);
                return new SqlColumnRef(innerSelect.Row.Columns[0]);
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                SqlSelect save = this.currentSelect;
                try {
                    this.currentSelect = select;
                    return base.VisitSelect(select);
                }
                finally {
                    this.currentSelect = save;
                }
            }
        }
    }
}
