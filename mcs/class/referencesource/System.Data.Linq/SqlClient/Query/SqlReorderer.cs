using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq.Mapping;
    using System.Data.Linq.Provider;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    
    // moves order-by clauses from sub-queries to outer-most or top selects
    // removes ordering in correlated sub-queries
    internal class SqlReorderer {
        TypeSystemProvider typeProvider;
        SqlFactory sql;

        internal SqlReorderer(TypeSystemProvider typeProvider, SqlFactory sqlFactory) {
            this.typeProvider = typeProvider;
            this.sql = sqlFactory;
        }

        internal SqlNode Reorder(SqlNode node) {
            return new Visitor(this.typeProvider, this.sql).Visit(node);
        }

        class Visitor : SqlVisitor {
            TypeSystemProvider typeProvider;
            bool topSelect = true;
            bool addPrimaryKeys;
            List<SqlOrderExpression> orders;
            List<SqlOrderExpression> rowNumberOrders;
            SqlSelect currentSelect;
            SqlFactory sql;
            SqlAggregateChecker aggregateChecker;

            internal Visitor(TypeSystemProvider typeProvider, SqlFactory sqlFactory) {
                this.orders = new List<SqlOrderExpression>();
                this.typeProvider = typeProvider;
                this.sql = sqlFactory;
                this.aggregateChecker = new SqlAggregateChecker();
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss) {
                List<SqlOrderExpression> save = this.orders;
                this.orders = new List<SqlOrderExpression>();
                base.VisitSubSelect(ss);
                this.orders = save;
                return ss;
            }

            private void PrependOrderExpressions(IEnumerable<SqlOrderExpression> exprs) {
                if (exprs != null) {
                    this.Orders.InsertRange(0, exprs);
                }
            }

            private List<SqlOrderExpression> Orders {
                get {
                    if (this.orders == null) {
                        this.orders = new List<SqlOrderExpression>();
                    }
                    return this.orders;
                }
            }


            internal override SqlSource VisitJoin(SqlJoin join) {
                this.Visit(join.Left);
                List<SqlOrderExpression> leftOrders = this.orders;
                this.orders = null;
                this.Visit(join.Right);
                this.PrependOrderExpressions(leftOrders);
                return join;
            }

            internal override SqlNode VisitUnion(SqlUnion su) {
                // ordering does not carry through a union
                this.orders = null;
                su.Left = this.Visit(su.Left);
                this.orders = null;
                su.Right = this.Visit(su.Right);
                this.orders = null;
                return su;
            }

            internal override SqlAlias VisitAlias(SqlAlias a) {

                SqlTable tab = a.Node as SqlTable;
                SqlTableValuedFunctionCall tvf = a.Node as SqlTableValuedFunctionCall;

                if (this.addPrimaryKeys && (tab != null || tvf != null)) {
                    List<SqlOrderExpression> list = new List<SqlOrderExpression>();

                    bool isTable = tab != null;
                    MetaType rowType = isTable ? tab.RowType : tvf.RowType;
                    foreach (MetaDataMember mm in rowType.IdentityMembers) {
                        string name = mm.MappedName;
                        SqlColumn col;
                        Expression sourceExpression;
                        List<SqlColumn> columns;

                        if (isTable) {
                            col = tab.Find(name);
                            sourceExpression = tab.SourceExpression;
                            columns = tab.Columns;
                        }
                        else {
                            col = tvf.Find(name);
                            sourceExpression = tvf.SourceExpression;
                            columns = tvf.Columns; 
                        }

                        if (col == null) {
                            col = new SqlColumn(mm.MemberAccessor.Type, typeProvider.From(mm.MemberAccessor.Type), name, mm, null, sourceExpression);
                            col.Alias = a;
                            columns.Add(col);
                        }
                        list.Add(new SqlOrderExpression(SqlOrderType.Ascending, new SqlColumnRef(col)));
                    }

                    this.PrependOrderExpressions(list);

                    return a;
                }
                else {
                    return base.VisitAlias(a);
                }
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                bool saveTop = this.topSelect;
                bool savePK = this.addPrimaryKeys;

                SqlSelect saveSelect = this.currentSelect;
                this.currentSelect = select;

                if (select.OrderingType == SqlOrderingType.Always) {
                    this.addPrimaryKeys = true;
                }

                this.topSelect = false;

                // can't forward ordering information through a group-by
                if (select.GroupBy.Count > 0) {
                    this.Visit(select.From);
                    this.orders = null;
                }
                else {
                    this.Visit(select.From);
                }

                if (select.OrderBy.Count > 0) {
                    this.PrependOrderExpressions(select.OrderBy);
                }

                List<SqlOrderExpression> save = this.orders;
                this.orders = null;
                this.rowNumberOrders = save; // lest orders be null when we need info

                /* do all the lower level stuff */
                select.Where = this.VisitExpression(select.Where);
                for (int i = 0, n = select.GroupBy.Count; i < n; i++) {
                    select.GroupBy[i] = this.VisitExpression(select.GroupBy[i]);
                }
                select.Having = this.VisitExpression(select.Having);
                for (int i = 0, n = select.OrderBy.Count; i < n; i++) {
                    select.OrderBy[i].Expression = this.VisitExpression(select.OrderBy[i].Expression);
                }
                select.Top = this.VisitExpression(select.Top);
                select.Selection = this.VisitExpression(select.Selection);
                select.Row = (SqlRow)this.Visit(select.Row);

                this.topSelect = saveTop;
                this.addPrimaryKeys = savePK;

                this.orders = save;

                // all ordering is blocked for this layer and above
                if (select.OrderingType == SqlOrderingType.Blocked) {
                    this.orders = null;
                }

                // rebuild orderby expressions, provided this select doesn't contain a SqlRowNumber
                // otherwise, replace the orderby with a reference to that column
                select.OrderBy.Clear();
                var rowNumberChecker = new SqlRowNumberChecker();

                if (rowNumberChecker.HasRowNumber(select) && rowNumberChecker.RowNumberColumn != null) {
                    select.Row.Columns.Remove(rowNumberChecker.RowNumberColumn);
                    this.PushDown(rowNumberChecker.RowNumberColumn);
                    this.Orders.Add(new SqlOrderExpression(SqlOrderType.Ascending, new SqlColumnRef(rowNumberChecker.RowNumberColumn)));
                } 
                if ((this.topSelect || select.Top != null) && select.OrderingType != SqlOrderingType.Never && this.orders != null) {
                    this.orders = new HashSet<SqlOrderExpression>(this.orders).ToList();
                    SqlDuplicator dup = new SqlDuplicator(true);
                    foreach (SqlOrderExpression sox in this.orders) {
                        select.OrderBy.Add(new SqlOrderExpression(sox.OrderType, (SqlExpression)dup.Duplicate(sox.Expression)));
                    }
                }
                this.currentSelect = saveSelect;

                return select;
            }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber) {
                if (rowNumber.OrderBy.Count > 0) return rowNumber;

                SqlDuplicator dup = new SqlDuplicator(true);
                List<SqlOrderExpression> orderBy = new List<SqlOrderExpression>();
                List<SqlOrderExpression> existingOrders = new List<SqlOrderExpression>();

                if (this.rowNumberOrders != null && this.rowNumberOrders.Count != 0) {
                    existingOrders = new List<SqlOrderExpression>(this.rowNumberOrders);
                }
                else if (this.orders != null) {
                    existingOrders = new List<SqlOrderExpression>(this.orders);
                }

                foreach (SqlOrderExpression expr in existingOrders) {
                    if (!expr.Expression.IsConstantColumn) {
                        orderBy.Add(expr);
                        if (this.rowNumberOrders != null) {
                            this.rowNumberOrders.Remove(expr);
                        }
                        if (this.orders != null) {
                            this.orders.Remove(expr);
                        }
                    }
                }

                rowNumber.OrderBy.Clear();

                if (orderBy.Count == 0) {
                    List<SqlColumn> columns = SqlGatherColumnsProduced.GatherColumns(this.currentSelect.From);

                    foreach (SqlColumn col in columns) {
                        if (col.Expression.SqlType.IsOrderable) {
                            orderBy.Add(new SqlOrderExpression(SqlOrderType.Ascending, new SqlColumnRef(col)));
                        }
                    }

                    if (orderBy.Count == 0) {
                        // insert simple column
                        SqlColumn col =
                            new SqlColumn(
                                "rowNumberOrder",
                                sql.Value(typeof(int), this.typeProvider.From(typeof(int)), 1, false, rowNumber.SourceExpression)
                            );
                        this.PushDown(col);
                        orderBy.Add(new SqlOrderExpression(SqlOrderType.Ascending, new SqlColumnRef(col)));
                    }
                }

                foreach (SqlOrderExpression sox in orderBy) {
                    rowNumber.OrderBy.Add(new SqlOrderExpression(sox.OrderType, (SqlExpression)dup.Duplicate(sox.Expression)));
                }

                return rowNumber;
            }

            private void PushDown(SqlColumn column) {
                SqlSelect select = new SqlSelect(new SqlNop(column.ClrType, column.SqlType, column.SourceExpression), this.currentSelect.From, this.currentSelect.SourceExpression);
                this.currentSelect.From = new SqlAlias(select);
                select.Row.Columns.Add(column);
            }
        }

        internal class SqlGatherColumnsProduced {
            static internal List<SqlColumn> GatherColumns(SqlSource source) {
                List<SqlColumn> columns = new List<SqlColumn>();
                new Visitor(columns).Visit(source);
                return columns;
            }
            class Visitor : SqlVisitor {
                List<SqlColumn> columns;
                internal Visitor(List<SqlColumn> columns) {
                    this.columns = columns;
                }
                internal override SqlSelect VisitSelect(SqlSelect select) {
                    foreach (SqlColumn c in select.Row.Columns) {
                        this.columns.Add(c);
                    }
                    return select;
                }
                internal override SqlNode VisitUnion(SqlUnion su) {
                    return su;
                }
            }
        }
    }
}
