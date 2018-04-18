using System;
using System.Collections.Generic;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq.Mapping;
    using System.Data.Linq.Provider;
    using System.Diagnostics.CodeAnalysis;

    // flatten object expressions into rows
    internal class SqlFlattener {
        Visitor visitor;

        internal SqlFlattener(SqlFactory sql, SqlColumnizer columnizer) {
            this.visitor = new Visitor(sql, columnizer);
        }

        internal SqlNode Flatten(SqlNode node) {
            node = this.visitor.Visit(node);
            return node;
        }

        class Visitor : SqlVisitor {
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Microsoft: part of our standard visitor pattern")]
            SqlFactory sql;
            SqlColumnizer columnizer;
            bool isTopLevel;
            Dictionary<SqlColumn, SqlColumn> map = new Dictionary<SqlColumn,SqlColumn>();

            [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily", Justification="Unknown reason.")]
            internal Visitor(SqlFactory sql, SqlColumnizer columnizer) {
                this.sql = sql;
                this.columnizer = columnizer;
                this.isTopLevel = true;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                SqlColumn mapped;
                if (this.map.TryGetValue(cref.Column, out mapped)) {
                    return new SqlColumnRef(mapped);
                }
                return cref;
            }

            internal override SqlSelect VisitSelectCore(SqlSelect select) {
                bool saveIsTopLevel = this.isTopLevel;
                this.isTopLevel = false;
                try {
                    return base.VisitSelectCore(select);
                }
                finally {
                    this.isTopLevel = saveIsTopLevel;
                }
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                select = base.VisitSelect(select);

                select.Selection = this.FlattenSelection(select.Row, false, select.Selection);

                if (select.GroupBy.Count > 0) {
                    this.FlattenGroupBy(select.GroupBy);
                }

                if (select.OrderBy.Count > 0) {
                    this.FlattenOrderBy(select.OrderBy);
                }

                if (!this.isTopLevel) {
                    select.Selection = new SqlNop(select.Selection.ClrType, select.Selection.SqlType, select.SourceExpression);
                }

                return select;
            }

            internal override SqlStatement VisitInsert(SqlInsert sin) {
                base.VisitInsert(sin);
                sin.Expression = this.FlattenSelection(sin.Row, true, sin.Expression);
                return sin;
            }

            private SqlExpression FlattenSelection(SqlRow row, bool isInput, SqlExpression selection) {
                selection = this.columnizer.ColumnizeSelection(selection);
                return new SelectionFlattener(row, this.map, isInput).VisitExpression(selection);
            }

            class SelectionFlattener : SqlVisitor {
                SqlRow row;
                Dictionary<SqlColumn, SqlColumn> map;
                bool isInput;
                bool isNew;

                internal SelectionFlattener(SqlRow row, Dictionary<SqlColumn, SqlColumn> map, bool isInput) {
                    this.row = row;
                    this.map = map;
                    this.isInput = isInput;
                }

                internal override SqlExpression VisitNew(SqlNew sox) {
                    this.isNew = true;
                    return base.VisitNew(sox);
                }

                internal override SqlExpression VisitColumn(SqlColumn col) {
                    SqlColumn c = this.FindColumn(this.row.Columns, col);
                    if (c == null && col.Expression != null && !this.isInput && (!this.isNew || (this.isNew && !col.Expression.IsConstantColumn))) {
                        c = this.FindColumnWithExpression(this.row.Columns, col.Expression);
                    }
                    if (c == null) {
                        this.row.Columns.Add(col);
                        c = col;
                    }
                    else if (c != col) {
                        // preserve expr-sets when folding expressions together
                        if (col.Expression.NodeType == SqlNodeType.ExprSet && c.Expression.NodeType != SqlNodeType.ExprSet) {
                            c.Expression = col.Expression;
                        }
                        this.map[col] = c;
                    }
                    return new SqlColumnRef(c);
                }

                internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                    SqlColumn c = this.FindColumn(this.row.Columns, cref.Column);
                    if (c == null) {
                        return MakeFlattenedColumn(cref, null);
                    }
                    else {
                        return new SqlColumnRef(c);
                    }
                }

                // ignore subquery in selection
                internal override SqlExpression VisitSubSelect(SqlSubSelect ss) {
                    return ss;
                }

                internal override SqlExpression VisitClientQuery(SqlClientQuery cq) {
                    return cq;
                }

                private SqlColumnRef MakeFlattenedColumn(SqlExpression expr, string name) {
                    SqlColumn c = (!this.isInput) ? this.FindColumnWithExpression(this.row.Columns, expr) : null;
                    if (c == null) {
                        c = new SqlColumn(expr.ClrType, expr.SqlType, name, null, expr, expr.SourceExpression);
                        this.row.Columns.Add(c);
                    }
                    return new SqlColumnRef(c);
                }


                private SqlColumn FindColumn(IEnumerable<SqlColumn> columns, SqlColumn col) {
                    foreach (SqlColumn c in columns) {
                        if (this.RefersToColumn(c, col)) {
                            return c;
                        }
                    }
                    return null;
                }

                private SqlColumn FindColumnWithExpression(IEnumerable<SqlColumn> columns, SqlExpression expr) {
                    foreach (SqlColumn c in columns) {
                        if (c == expr) {
                            return c;
                        }
                        if (SqlComparer.AreEqual(c.Expression, expr)) {
                            return c;
                        }
                    }
                    return null;
                }
            }

            private void FlattenGroupBy(List<SqlExpression> exprs) {
                List<SqlExpression> list = new List<SqlExpression>(exprs.Count);
                foreach (SqlExpression gex in exprs) {
                    if (TypeSystem.IsSequenceType(gex.ClrType)) {
                        throw Error.InvalidGroupByExpressionType(gex.ClrType.Name);
                    }
                    this.FlattenGroupByExpression(list, gex);
                }
                exprs.Clear();
                exprs.AddRange(list);
            }

            private void FlattenGroupByExpression(List<SqlExpression> exprs, SqlExpression expr) {
                SqlNew sn = expr as SqlNew;
                if (sn != null) {
                    foreach (SqlMemberAssign ma in sn.Members) {
                        this.FlattenGroupByExpression(exprs, ma.Expression);
                    }
                    foreach (SqlExpression arg in sn.Args) {
                        this.FlattenGroupByExpression(exprs, arg);
                    }
                }
                else if (expr.NodeType == SqlNodeType.TypeCase) {
                    SqlTypeCase tc = (SqlTypeCase)expr;
                    this.FlattenGroupByExpression(exprs, tc.Discriminator);
                    foreach (SqlTypeCaseWhen when in tc.Whens) {
                        this.FlattenGroupByExpression(exprs, when.TypeBinding);
                    }
                }
                else if (expr.NodeType == SqlNodeType.Link) {
                    SqlLink link = (SqlLink)expr;
                    if (link.Expansion != null) {
                        this.FlattenGroupByExpression(exprs, link.Expansion);
                    }
                    else {
                        foreach (SqlExpression key in link.KeyExpressions) {
                            this.FlattenGroupByExpression(exprs, key);
                        }
                    }
                }
                else if (expr.NodeType == SqlNodeType.OptionalValue) {
                    SqlOptionalValue sop = (SqlOptionalValue)expr;
                    this.FlattenGroupByExpression(exprs, sop.HasValue);
                    this.FlattenGroupByExpression(exprs, sop.Value);
                }
                else if (expr.NodeType == SqlNodeType.OuterJoinedValue) {
                    this.FlattenGroupByExpression(exprs, ((SqlUnary)expr).Operand);
                }
                else if (expr.NodeType == SqlNodeType.DiscriminatedType) {
                    SqlDiscriminatedType dt = (SqlDiscriminatedType)expr;
                    this.FlattenGroupByExpression(exprs, dt.Discriminator);
                }
                else {
                    // this expression should have been 'pushed-down' in SqlBinder, so we
                    // should only find column-references & expr-sets unless the expression could not
                    // be columnized (in which case it was a bad group-by expression.)
                    if (expr.NodeType != SqlNodeType.ColumnRef &&
                        expr.NodeType != SqlNodeType.ExprSet) {
                        if (!expr.SqlType.CanBeColumn) {
                            throw Error.InvalidGroupByExpressionType(expr.SqlType.ToQueryString());
                        }
                        throw Error.InvalidGroupByExpression();
                    }
                    exprs.Add(expr);
                }
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private void FlattenOrderBy(List<SqlOrderExpression> exprs) {
                foreach (SqlOrderExpression obex in exprs) {
                    if (!obex.Expression.SqlType.IsOrderable) {
                        if (obex.Expression.SqlType.CanBeColumn) {
                            throw Error.InvalidOrderByExpression(obex.Expression.SqlType.ToQueryString());
                        }
                        else {
                            throw Error.InvalidOrderByExpression(obex.Expression.ClrType.Name);
                        }
                    }
                }
            }
        }
    }
}
