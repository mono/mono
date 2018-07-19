using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Linq;

namespace System.Data.Linq.SqlClient {
    internal class SqlAliaser {
        Visitor visitor;

        internal SqlAliaser() {
            this.visitor = new Visitor();
        }

        internal SqlNode AssociateColumnsWithAliases(SqlNode node) {
            return this.visitor.Visit(node);
        }

        class Visitor : SqlVisitor {
            SqlAlias alias;

            internal Visitor() {
            }
            
            internal override SqlAlias VisitAlias(SqlAlias sqlAlias) {
                SqlAlias save = this.alias;
                this.alias = sqlAlias;
                sqlAlias.Node = this.Visit(sqlAlias.Node);
                this.alias = save;
                return sqlAlias;
            }
            
            internal override SqlRow VisitRow(SqlRow row) {
                foreach (SqlColumn c in row.Columns) {
                    c.Alias = alias;
                }
                return base.VisitRow(row);
            }
            internal override SqlTable VisitTable(SqlTable tab) {
                foreach (SqlColumn c in tab.Columns) {
                    c.Alias = alias;
                }
                return base.VisitTable(tab);
            }
            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc) {
                foreach (SqlColumn c in fc.Columns) {
                    c.Alias = this.alias;
                }
                return base.VisitTableValuedFunctionCall(fc);
            }
        }
    }
}
