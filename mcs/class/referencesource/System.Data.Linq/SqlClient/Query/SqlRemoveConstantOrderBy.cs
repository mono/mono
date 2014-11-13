using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq;

    /// <summary>
    /// SQL doesn't allow constants in ORDER BY.
    /// 
    /// Worse, an integer constant greater than 0 is treated as ORDER BY ProjectionColumn[i] so the results
    /// can be unexpected.
    /// 
    /// The LINQ semantic for OrderBy(o=>constant) is for it to have no effect on the ordering. We enforce
    /// that semantic here by removing all constant columns from OrderBy.
    /// </summary>
    internal class SqlRemoveConstantOrderBy {

        private class Visitor : SqlVisitor {
            internal override SqlSelect VisitSelect(SqlSelect select) {
                int i = 0;
                List<SqlOrderExpression> orders = select.OrderBy;
                while (i < orders.Count) {
                    SqlExpression expr = orders[i].Expression;
                    while (expr.NodeType == SqlNodeType.DiscriminatedType) {
                        expr = ((SqlDiscriminatedType)expr).Discriminator;
                    }
                    switch (expr.NodeType) {
                        case SqlNodeType.Value:
                        case SqlNodeType.Parameter:
                            orders.RemoveAt(i);
                            break;
                        default:
                            ++i;
                            break;
                    }
                }
                return base.VisitSelect(select);
            }
        }

        /// <summary>
        /// Remove relative constants from OrderBy.
        /// </summary>
        internal static SqlNode Remove(SqlNode node) {
            return new Visitor().Visit(node);
        }
    }
}
