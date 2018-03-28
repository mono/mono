using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Linq;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    internal class SqlNamer {
        Visitor visitor;

        internal SqlNamer() {
            this.visitor = new Visitor();
        }

        internal SqlNode AssignNames(SqlNode node) {
            return this.visitor.Visit(node);
        }

        class Visitor : SqlVisitor {
            int aliasCount;
            SqlAlias alias;
            bool makeUnique;
            bool useMappedNames;
            string lastName;

            internal Visitor() {
                this.makeUnique = true;
                this.useMappedNames = false;
            }

            internal string GetNextAlias() {
                return "t" + (aliasCount++);
            }

            internal override SqlAlias VisitAlias(SqlAlias sqlAlias) {
                SqlAlias save = this.alias;
                this.alias = sqlAlias;
                sqlAlias.Node = this.Visit(sqlAlias.Node);
                sqlAlias.Name = this.GetNextAlias();
                this.alias = save;
                return sqlAlias;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
                base.VisitScalarSubSelect(ss);
                if (ss.Select.Row.Columns.Count > 0) {
                    System.Diagnostics.Debug.Assert(ss != null && ss.Select != null && ss.Select.Row != null && ss.Select.Row.Columns.Count == 1);
                    // make sure these scalar subselects don't get redundantly named
                    ss.Select.Row.Columns[0].Name = "";
                }
                return ss;
            }

            internal override SqlStatement VisitInsert(SqlInsert insert) {
                bool saveMakeUnique = this.makeUnique;
                this.makeUnique = false;
                bool saveUseMappedNames = this.useMappedNames;
                this.useMappedNames = true;
                SqlStatement stmt = base.VisitInsert(insert);
                this.makeUnique = saveMakeUnique;
                this.useMappedNames = saveUseMappedNames;
                return stmt;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate update) {
                bool saveMakeUnique = this.makeUnique;
                this.makeUnique = false;
                bool saveUseMappedNames = this.useMappedNames;
                this.useMappedNames = true;
                SqlStatement stmt = base.VisitUpdate(update);
                this.makeUnique = saveMakeUnique;
                this.useMappedNames = saveUseMappedNames;
                return stmt;
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                select = base.VisitSelect(select);

                string[] names = new string[select.Row.Columns.Count];
                for (int i = 0, n = names.Length; i < n; i++) {
                    SqlColumn c = select.Row.Columns[i];
                    string name = c.Name;
                    if (name == null) {
                        name = SqlNamer.DiscoverName(c);
                    }
                    names[i] = name;
                    c.Name = null;
                }
                
                var reservedNames = this.GetColumnNames(select.OrderBy);

                for (int i = 0, n = select.Row.Columns.Count; i < n; i++) {
                    SqlColumn c = select.Row.Columns[i];
                    string rootName = names[i];
                    string name = rootName;
                    if (this.makeUnique) {
                        int iName = 1;
                        while (!this.IsUniqueName(select.Row.Columns, reservedNames, c, name)) {
                            iName++;
                            name = rootName + iName;
                        }
                    }
                    c.Name = name;
                    c.Ordinal = i;
                }

                return select;
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private bool IsUniqueName(List<SqlColumn> columns, ICollection<string> reservedNames, SqlColumn c, string name) {
                foreach (SqlColumn sc in columns) {
                    if (sc != c && string.Compare(sc.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                        return false;
                }

                if (!IsSimpleColumn(c, name)) {
                    return !reservedNames.Contains(name);
                }

                return true;
            }

            /// <summary>
            /// An expression is a simple reprojection if it's a column node whose expression is null, or 
            /// whose expression is a column whose name matches the name of the given name or where
            /// where the given name is null or empty.
            /// </summary>
            /// <param name="c"></param>
            /// <returns></returns>
            private static bool IsSimpleColumn(SqlColumn c, string name) {
                if (c.Expression != null) {
                    switch (c.Expression.NodeType) {
                        case SqlNodeType.ColumnRef:
                            var colRef = c.Expression as SqlColumnRef;
                            return String.IsNullOrEmpty(name) || string.Compare(name, colRef.Column.Name, StringComparison.OrdinalIgnoreCase) == 0;
                        default:
                            return false;
                    }
                }
                return true;
            }

            internal override SqlExpression VisitExpression(SqlExpression expr) {
                string saveLastName = this.lastName;
                this.lastName = null;
                try {
                    return (SqlExpression)this.Visit(expr);
                }
                finally {
                    this.lastName = saveLastName;
                }
            }

            private SqlExpression VisitNamedExpression(SqlExpression expr, string name) {
                string saveLastName = this.lastName;
                this.lastName = name;
                try {
                    return (SqlExpression)this.Visit(expr);
                }
                finally {
                    this.lastName = saveLastName;
                }
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                if (cref.Column.Name == null && this.lastName != null) {
                    cref.Column.Name = this.lastName;
                }
                return cref;
            }

            internal override SqlExpression VisitNew(SqlNew sox) {
                if (sox.Constructor != null) {
                    System.Reflection.ParameterInfo[] pis = sox.Constructor.GetParameters();
                    for (int i = 0, n = sox.Args.Count; i < n; i++) {
                        sox.Args[i] = this.VisitNamedExpression(sox.Args[i], pis[i].Name);
                    }
                }
                else {
                    for (int i = 0, n = sox.Args.Count; i < n; i++) {
                        sox.Args[i] = this.VisitExpression(sox.Args[i]);
                    }
                }
                foreach (SqlMemberAssign ma in sox.Members) {
                    string n = ma.Member.Name;
                    if (this.useMappedNames) {
                        n = sox.MetaType.GetDataMember(ma.Member).MappedName;
                    }
                    ma.Expression = this.VisitNamedExpression(ma.Expression, n);
                }
                return sox;
            }

            internal override SqlExpression VisitGrouping(SqlGrouping g) {
                g.Key = this.VisitNamedExpression(g.Key, "Key");
                g.Group = this.VisitNamedExpression(g.Group, "Group");
                return g;
            }

            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov) {
                sov.HasValue = this.VisitNamedExpression(sov.HasValue, "test");
                sov.Value = this.VisitExpression(sov.Value);
                return sov;
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc) {
                mc.Object = this.VisitExpression(mc.Object);
                System.Reflection.ParameterInfo[] pis = mc.Method.GetParameters();
                for (int i = 0, n = mc.Arguments.Count; i < n; i++) {
                    mc.Arguments[i] = this.VisitNamedExpression(mc.Arguments[i], pis[i].Name);
                }
                return mc;
            }


            ICollection<string> GetColumnNames(IEnumerable<SqlOrderExpression> orderList)
            {
                var visitor = new ColumnNameGatherer();

                foreach (var expr in orderList) {
                    visitor.Visit(expr.Expression);
                }

                return visitor.Names;
            }
        }

        internal static string DiscoverName(SqlExpression e) {
            if (e != null) {
                switch (e.NodeType) {
                    case SqlNodeType.Column:
                        return DiscoverName(((SqlColumn)e).Expression);
                    case SqlNodeType.ColumnRef:
                        SqlColumnRef cref = (SqlColumnRef)e;
                        if (cref.Column.Name != null) return cref.Column.Name;
                        return DiscoverName(cref.Column);
                    case SqlNodeType.ExprSet:
                        SqlExprSet eset = (SqlExprSet)e;
                        return DiscoverName(eset.Expressions[0]);
                }
            }
            return "value";
        }
        
        class ColumnNameGatherer : SqlVisitor {
            public HashSet<string> Names { get; set; }

            public ColumnNameGatherer()
                : base() {
                this.Names = new HashSet<string>();
            }

            internal override SqlExpression VisitColumn(SqlColumn col) {
                if (!String.IsNullOrEmpty(col.Name)) {
                    this.Names.Add(col.Name);
                }

                return base.VisitColumn(col);
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                Visit(cref.Column);

                return base.VisitColumnRef(cref);
            }
        }
    }
}
