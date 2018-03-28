using System;
using System.Collections.Generic;
using System.Data.Linq;

namespace System.Data.Linq.SqlClient {

    // Resolves references to columns/expressions defined in other scopes
    internal class SqlResolver {
        Visitor visitor;

        internal SqlResolver() {
            this.visitor = new Visitor();
        }

        internal SqlNode Resolve(SqlNode node) {
            return this.visitor.Visit(node);
        }

        private static string GetColumnName(SqlColumn c) {
#if DEBUG
            return c.Text;
#else
            return c.Name;
#endif
        }

        class Visitor : SqlScopedVisitor {
            SqlBubbler bubbler;

            internal Visitor() {
                this.bubbler = new SqlBubbler();
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                SqlColumnRef result = this.BubbleUp(cref);
                if (result == null) {
                    throw Error.ColumnReferencedIsNotInScope(GetColumnName(cref.Column));
                }
                return result;
            }

            private SqlColumnRef BubbleUp(SqlColumnRef cref) {
                for (Scope s = this.CurrentScope; s != null; s = s.ContainingScope) {
                    if (s.Source != null) {
                        SqlColumn found = this.bubbler.BubbleUp(cref.Column, s.Source);
                        if (found != null) {
                            if (found != cref.Column)
                                return new SqlColumnRef(found);
                            return cref;
                        }
                    }
                }
                return null;
            }
        }

        internal class SqlScopedVisitor : SqlVisitor {
            internal Scope CurrentScope;

            internal class Scope {
                SqlNode source;
                Scope containing;
                internal Scope(SqlNode source, Scope containing) {
                    this.source = source;
                    this.containing = containing;
                }
                internal SqlNode Source {
                    get { return this.source; }
                }
                internal Scope ContainingScope {
                    get { return this.containing; }
                }
            }

            internal SqlScopedVisitor() {
                this.CurrentScope = new Scope(null, null);
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss) {
                Scope save = this.CurrentScope;
                this.CurrentScope = new Scope(null, this.CurrentScope);
                base.VisitSubSelect(ss);
                this.CurrentScope = save;
                return ss;
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                select.From = (SqlSource)this.Visit(select.From);

                Scope save = this.CurrentScope;
                this.CurrentScope = new Scope(select.From, this.CurrentScope.ContainingScope);

                select.Where = this.VisitExpression(select.Where);
                for (int i = 0, n = select.GroupBy.Count; i < n; i++) {
                    select.GroupBy[i] = this.VisitExpression(select.GroupBy[i]);
                }
                select.Having = this.VisitExpression(select.Having);
                for (int i = 0, n = select.OrderBy.Count; i < n; i++) {
                    select.OrderBy[i].Expression = this.VisitExpression(select.OrderBy[i].Expression);
                }
                select.Top = this.VisitExpression(select.Top);
                select.Row = (SqlRow)this.Visit(select.Row);

                // selection must be able to see its own projection
                this.CurrentScope = new Scope(select, this.CurrentScope.ContainingScope);
                select.Selection = this.VisitExpression(select.Selection);

                this.CurrentScope = save;
                return select;
            }

            internal override SqlStatement VisitInsert(SqlInsert sin) {
                Scope save = this.CurrentScope;
                this.CurrentScope = new Scope(sin, this.CurrentScope.ContainingScope);
                base.VisitInsert(sin);
                this.CurrentScope = save;
                return sin;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate sup) {
                Scope save = this.CurrentScope;
                this.CurrentScope = new Scope(sup.Select, this.CurrentScope.ContainingScope);
                base.VisitUpdate(sup);
                this.CurrentScope = save;
                return sup;
            }

            internal override SqlStatement VisitDelete(SqlDelete sd) {
                Scope save = this.CurrentScope;
                this.CurrentScope = new Scope(sd, this.CurrentScope.ContainingScope);
                base.VisitDelete(sd);
                this.CurrentScope = save;
                return sd;
            }

            internal override SqlSource VisitJoin(SqlJoin join) {
                Scope save = this.CurrentScope;
                switch (join.JoinType) {
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.OuterApply: {
                        this.Visit(join.Left);
                        Scope tmp = new Scope(join.Left, this.CurrentScope.ContainingScope);
                        this.CurrentScope = new Scope(null, tmp);
                        this.Visit(join.Right);
                        Scope tmp2 = new Scope(join.Right, tmp);
                        this.CurrentScope = new Scope(null, tmp2);
                        this.Visit(join.Condition);
                        break;
                    }
                    default: {
                        this.Visit(join.Left);
                        this.Visit(join.Right);
                        this.CurrentScope = new Scope(null, new Scope(join.Right, new Scope(join.Left, this.CurrentScope.ContainingScope)));
                        this.Visit(join.Condition);
                        break;
                    }
                }
                this.CurrentScope = save;
                return join;
            }
        }

        // finds location of expression definition and re-projects that value all the
        // way to the outermost projection
        internal class SqlBubbler : SqlVisitor {
            SqlColumn match;
            SqlColumn found;

            internal SqlBubbler() {
            }

            internal SqlColumn BubbleUp(SqlColumn col, SqlNode source) {
                this.match = this.GetOriginatingColumn(col);
                this.found = null;
                this.Visit(source);
                return this.found;
            }

            internal SqlColumn GetOriginatingColumn(SqlColumn col) {
                SqlColumnRef cref = col.Expression as SqlColumnRef;
                if (cref != null) {
                    return this.GetOriginatingColumn(cref.Column);
                }
                return col;
            }

            internal override SqlRow VisitRow(SqlRow row) {
                foreach (SqlColumn c in row.Columns) {
                    if (this.RefersToColumn(c, this.match)) {
                        if (this.found != null) {
                            throw Error.ColumnIsDefinedInMultiplePlaces(GetColumnName(this.match));
                        }
                        this.found = c;
                        break;
                    }
                }
                return row;
            }

            internal override SqlTable VisitTable(SqlTable tab) {
                foreach (SqlColumn c in tab.Columns) {
                    if (c == this.match) {
                        if (this.found != null)
                            throw Error.ColumnIsDefinedInMultiplePlaces(GetColumnName(this.match));
                        this.found = c;
                        break;
                    }
                }
                return tab;
            }

            internal override SqlSource VisitJoin(SqlJoin join) {
                switch (join.JoinType) {
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.OuterApply: {
                        this.Visit(join.Left);
                        if (this.found == null) {
                            this.Visit(join.Right);
                        }
                        break;
                    }
                    default: {
                        this.Visit(join.Left);
                        this.Visit(join.Right);
                        break;
                    }
                }
                return join;
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc) {
                foreach (SqlColumn c in fc.Columns) {
                    if (c == this.match) {
                        if (this.found != null)
                            throw Error.ColumnIsDefinedInMultiplePlaces(GetColumnName(this.match));
                        this.found = c;
                        break;
                    }
                }
                return fc;
            }

            private void ForceLocal(SqlRow row, string name) {
                bool isLocal = false;
                // check to see if it already exists locally
                foreach (SqlColumn c in row.Columns) {
                    if (this.RefersToColumn(c, this.found)) {
                        this.found = c;
                        isLocal = true;
                        break;
                    }
                }
                if (!isLocal) {
                    // need to put this in the local projection list to bubble it up
                    SqlColumn c = new SqlColumn(found.ClrType, found.SqlType, name, this.found.MetaMember, new SqlColumnRef(this.found), row.SourceExpression);
                    row.Columns.Add(c);
                    this.found = c;
                }
            }

            private bool IsFoundInGroup(SqlSelect select) {
                // does the column happen to be listed in the group-by clause?
                foreach (SqlExpression exp in select.GroupBy) {               
                    if (this.RefersToColumn(exp, this.found) || this.RefersToColumn(exp, this.match)) {
                        return true;
                    }
                }
                return false;
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                // look in this projection
                this.Visit(select.Row);

                if (this.found == null) {
                    // look in upstream projections
                    this.Visit(select.From);

                    // bubble it up
                    if (this.found != null) {
                        if (select.IsDistinct && !match.IsConstantColumn) {
                            throw Error.ColumnIsNotAccessibleThroughDistinct(GetColumnName(this.match));
                        }
                        if (select.GroupBy.Count == 0 || this.IsFoundInGroup(select)) {
                            this.ForceLocal(select.Row, this.found.Name);
                        }
                        else {
                            // found it, but its hidden behind the group-by
                            throw Error.ColumnIsNotAccessibleThroughGroupBy(GetColumnName(this.match));
                        }
                    }
                }

                return select;
            }
        }
    }
}
