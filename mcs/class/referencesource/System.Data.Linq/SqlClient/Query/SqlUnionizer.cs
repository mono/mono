using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {
    internal class SqlUnionizer {
        internal static SqlNode Unionize(SqlNode node) {
            return new Visitor().Visit(node);
        }

        class Visitor : SqlVisitor {
            internal override SqlSelect  VisitSelect(SqlSelect select) {
 	            base.VisitSelect(select);

                // enforce exact ordering of columns in union selects
                SqlUnion union = this.GetUnion(select.From);
                if (union != null) {
                    SqlSelect sleft = union.Left as SqlSelect;
                    SqlSelect sright = union.Right as SqlSelect;
                    if (sleft != null & sright != null) {
                        // preset ordinals to high values (so any unreachable column definition is ordered last)
                        for (int i = 0, n = sleft.Row.Columns.Count; i < n; i++) {
                            sleft.Row.Columns[i].Ordinal = select.Row.Columns.Count + i;
                        }
                        for (int i = 0, n = sright.Row.Columns.Count; i < n; i++) {
                            sright.Row.Columns[i].Ordinal = select.Row.Columns.Count + i;
                        }
                        // next assign ordinals to all direct columns in subselects
                        for (int i = 0, n = select.Row.Columns.Count; i < n; i++) {
                            SqlExprSet es = select.Row.Columns[i].Expression as SqlExprSet;
                            if (es != null) {
                                for (int e = 0, en = es.Expressions.Count; e < en; e++) {
                                    SqlColumnRef cr = es.Expressions[e] as SqlColumnRef;
                                    if (cr != null && e >= select.Row.Columns.Count) {
                                        cr.Column.Ordinal = i;
                                    }
                                }
                            }
                        }
                        // next sort columns in left & right subselects
                        Comparison<SqlColumn> comp = (x,y) => x.Ordinal - y.Ordinal;
                        sleft.Row.Columns.Sort(comp);
                        sright.Row.Columns.Sort(comp);
                    }
                }

                return select;
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private SqlUnion GetUnion(SqlSource source) {
                SqlAlias alias = source as SqlAlias;
                if (alias != null) {
                    SqlUnion union = alias.Node as SqlUnion;
                    if (union != null)
                        return union;
                }
                return null;
            }
        }
    }
}
