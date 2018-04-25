using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data.Linq;

namespace System.Data.Linq.SqlClient {

    internal class SqlAggregateChecker {
        Visitor visitor;

        internal SqlAggregateChecker() {
            this.visitor = new Visitor();
        }

        internal bool HasAggregates(SqlNode node) {
            visitor.hasAggregates = false;
            visitor.Visit(node);
            return visitor.hasAggregates;
        }

        class Visitor : SqlVisitor {
            internal bool hasAggregates;

            internal Visitor() {
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss) {
                return ss;
            }
            internal override SqlSource VisitSource(SqlSource source) {
                return source;
            }
            internal override SqlExpression VisitUnaryOperator(SqlUnary uo) {
                switch (uo.NodeType) {
                    case SqlNodeType.Min:
                    case SqlNodeType.Max:
                    case SqlNodeType.Avg:
                    case SqlNodeType.Sum:
                    case SqlNodeType.Count:
                    case SqlNodeType.LongCount:
                        this.hasAggregates = true;
                        return uo;
                    default:
                        return base.VisitUnaryOperator(uo);
                }
            }
        }
    }
}
