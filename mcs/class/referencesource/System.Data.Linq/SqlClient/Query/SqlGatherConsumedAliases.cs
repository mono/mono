using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Linq.SqlClient {
    /// <summary>
    /// Walk a tree and return the set of unique aliases it consumes.
    /// </summary>
    class SqlGatherConsumedAliases {
        internal static HashSet<SqlAlias> Gather(SqlNode node) {
            Gatherer g = new Gatherer();
            g.Visit(node);
            return g.Consumed;
        }

        private class Gatherer : SqlVisitor {
            internal HashSet<SqlAlias> Consumed = new HashSet<SqlAlias>();

            internal void VisitAliasConsumed(SqlAlias a) {
                Consumed.Add(a);
            }
            internal override SqlExpression VisitColumn(SqlColumn col) {
                VisitAliasConsumed(col.Alias);
                VisitExpression(col.Expression);
                return col;
            }
            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                VisitAliasConsumed(cref.Column.Alias);
                return cref;
            }
        }
    }
}
