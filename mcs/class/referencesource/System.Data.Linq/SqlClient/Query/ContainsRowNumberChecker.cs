using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Linq.SqlClient {
    internal class SqlRowNumberChecker {
        Visitor rowNumberVisitor;

        internal SqlRowNumberChecker() {
            this.rowNumberVisitor = new Visitor();
        }

        internal bool HasRowNumber(SqlNode node) {
            this.rowNumberVisitor.Visit(node);
            return rowNumberVisitor.HasRowNumber;
        }

        internal bool HasRowNumber(SqlRow row) {
            foreach (SqlColumn column in row.Columns) {
                if (this.HasRowNumber(column)) {
                    return true;
                }
            }
            return false;
        }

        internal SqlColumn RowNumberColumn {
            get {
                return rowNumberVisitor.HasRowNumber ? rowNumberVisitor.CurrentColumn : null;
            }
        }

        private class Visitor: SqlVisitor {
            bool hasRowNumber = false;

            public bool HasRowNumber {
                get { return hasRowNumber; }
            }

            public SqlColumn CurrentColumn { private set; get; }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber) {
                this.hasRowNumber = true;
                return rowNumber;
            }

            // shortcuts
            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
                return ss;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss) {
                return ss;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                for (int i = 0, n = row.Columns.Count; i < n; i++) {
                    row.Columns[i].Expression = this.VisitExpression(row.Columns[i].Expression);
                    if (this.hasRowNumber) {
                        this.CurrentColumn = row.Columns[i];
                        break;
                    }
                }
                return row;
            }
            internal override SqlSelect VisitSelect(SqlSelect select) {
                this.Visit(select.Row);
                this.Visit(select.Where);

                return select;
            }
        }
    }
}
