using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    internal class SqlDeflator {
        SqlValueDeflator vDeflator;
        SqlColumnDeflator cDeflator;
        SqlAliasDeflator aDeflator;
        SqlTopSelectDeflator tsDeflator;
        SqlDuplicateColumnDeflator dupColumnDeflator;

        internal SqlDeflator() {
            this.vDeflator = new SqlValueDeflator();
            this.cDeflator = new SqlColumnDeflator();
            this.aDeflator = new SqlAliasDeflator();
            this.tsDeflator = new SqlTopSelectDeflator();
            this.dupColumnDeflator = new SqlDuplicateColumnDeflator();
        }

        internal SqlNode Deflate(SqlNode node) {
            node = this.vDeflator.Visit(node);
            node = this.cDeflator.Visit(node);
            node = this.aDeflator.Visit(node);
            node = this.tsDeflator.Visit(node);
            node = this.dupColumnDeflator.Visit(node);
            return node;
        }

        // remove references to literal values
        class SqlValueDeflator : SqlVisitor {
            SelectionDeflator sDeflator;
            bool isTopLevel = true;

            internal SqlValueDeflator() {
                this.sDeflator = new SelectionDeflator();
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                if (this.isTopLevel) {
                    select.Selection = sDeflator.VisitExpression(select.Selection);
                }
                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss) {
                bool saveIsTopLevel = this.isTopLevel;
                try {
                    return base.VisitSubSelect(ss);
                }
                finally {
                    this.isTopLevel = saveIsTopLevel;
                }
            }

            class SelectionDeflator : SqlVisitor {
                internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                    SqlExpression literal = this.GetLiteralValue(cref);
                    if (literal != null) {
                        return literal;
                    }
                    return cref;
                }

                private SqlValue GetLiteralValue(SqlExpression expr) {
                    while (expr != null && expr.NodeType == SqlNodeType.ColumnRef) {
                        expr = ((SqlColumnRef)expr).Column.Expression;
                    }
                    return expr as SqlValue;
                }
            }
        }

        // remove unreferenced items in projection list
        class SqlColumnDeflator : SqlVisitor {
            Dictionary<SqlNode, SqlNode> referenceMap;
            bool isTopLevel;
            bool forceReferenceAll;
            SqlAggregateChecker aggregateChecker;

            internal SqlColumnDeflator() {
                this.referenceMap = new Dictionary<SqlNode, SqlNode>();
                this.aggregateChecker = new SqlAggregateChecker();
                this.isTopLevel = true;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                this.referenceMap[cref.Column] = cref.Column;
                return cref;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
                bool saveIsTopLevel = this.isTopLevel;
                this.isTopLevel = false;
                bool saveForceReferenceAll = this.forceReferenceAll;
                this.forceReferenceAll = true;
                try {
                    return base.VisitScalarSubSelect(ss);
                }
                finally {
                    this.isTopLevel = saveIsTopLevel;
                    this.forceReferenceAll = saveForceReferenceAll;
                }
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss) {
                bool saveIsTopLevel = this.isTopLevel;
                this.isTopLevel = false;
                try {
                    return base.VisitExists(ss);
                }
                finally {
                    this.isTopLevel = saveIsTopLevel;
                }
            }
            
            internal override SqlNode VisitUnion(SqlUnion su) {
                bool saveForceReferenceAll = this.forceReferenceAll;
                this.forceReferenceAll = true;
                su.Left = this.Visit(su.Left);
                su.Right = this.Visit(su.Right);
                this.forceReferenceAll = saveForceReferenceAll;
                return su;
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                bool saveForceReferenceAll = this.forceReferenceAll;
                this.forceReferenceAll = false;
                bool saveIsTopLevel = this.isTopLevel;

                try {
                    if (this.isTopLevel) {
                        // top-level projection references columns!
                        select.Selection = this.VisitExpression(select.Selection);
                    }
                    this.isTopLevel = false;

                    for (int i = select.Row.Columns.Count - 1; i >= 0; i--) {
                        SqlColumn c = select.Row.Columns[i];

                        bool safeToRemove =
                            !saveForceReferenceAll
                            && !this.referenceMap.ContainsKey(c)
                            // don't remove anything from a distinct select (except maybe a literal value) since it would change the meaning of the comparison
                            && !select.IsDistinct
                            // don't remove an aggregate expression that may be the only expression that forces the grouping (since it would change the cardinality of the results)
                            && !(select.GroupBy.Count == 0 && this.aggregateChecker.HasAggregates(c.Expression)); 

                        if (safeToRemove) {
                            select.Row.Columns.RemoveAt(i);
                        }
                        else {
                            this.VisitExpression(c.Expression);
                        }
                    }

                    select.Top = this.VisitExpression(select.Top);
                    for (int i = select.OrderBy.Count - 1; i >= 0; i--) {
                        select.OrderBy[i].Expression = this.VisitExpression(select.OrderBy[i].Expression);
                    }

                    select.Having = this.VisitExpression(select.Having);
                    for (int i = select.GroupBy.Count - 1; i >= 0; i--) {
                        select.GroupBy[i] = this.VisitExpression(select.GroupBy[i]);
                    }

                    select.Where = this.VisitExpression(select.Where);
                    select.From = this.VisitSource(select.From);
                }
                finally {
                    this.isTopLevel = saveIsTopLevel;
                    this.forceReferenceAll = saveForceReferenceAll;
                }

                return select;
            }

            internal override SqlSource VisitJoin(SqlJoin join) {
                join.Condition = this.VisitExpression(join.Condition);
                join.Right = this.VisitSource(join.Right);
                join.Left = this.VisitSource(join.Left);
                return join;
            }

            internal override SqlNode VisitLink(SqlLink link) {
                // don't visit expansion...
                for (int i = 0, n = link.KeyExpressions.Count; i < n; i++) {
                    link.KeyExpressions[i] = this.VisitExpression(link.KeyExpressions[i]);
                }
                return link;
            }
        }

        class SqlColumnEqualizer : SqlVisitor {
            Dictionary<SqlColumn, SqlColumn> map;

            internal SqlColumnEqualizer() {
            }

            internal void BuildEqivalenceMap(SqlSource scope) {
                this.map = new Dictionary<SqlColumn, SqlColumn>();
                this.Visit(scope);
            }

            internal bool AreEquivalent(SqlExpression e1, SqlExpression e2) {
                if (SqlComparer.AreEqual(e1, e2))
                    return true;

                SqlColumnRef cr1 = e1 as SqlColumnRef;
                SqlColumnRef cr2 = e2 as SqlColumnRef;
                 
                if (cr1 != null && cr2 != null) {
                    SqlColumn c1 = cr1.GetRootColumn();
                    SqlColumn c2 = cr2.GetRootColumn();
                    SqlColumn r;
                    return this.map.TryGetValue(c1, out r) && r == c2;
                }

                return false;
            }

            internal override SqlSource VisitJoin(SqlJoin join) {
                base.VisitJoin(join);
                if (join.Condition != null) {
                    this.CheckJoinCondition(join.Condition);
                }
                return join;
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                base.VisitSelect(select);
                if (select.Where != null) {
                    this.CheckJoinCondition(select.Where);
                }
                return select;
            }

            [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification="Microsoft: Cast is dependent on node type and casts do not happen unecessarily in a single code path.")]
            private void CheckJoinCondition(SqlExpression expr) {
                switch (expr.NodeType) {
                    case SqlNodeType.And: {
                        SqlBinary b = (SqlBinary)expr;
                        CheckJoinCondition(b.Left);
                        CheckJoinCondition(b.Right);
                        break;
                    }
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V: {
                        SqlBinary b = (SqlBinary)expr;
                        SqlColumnRef crLeft = b.Left as SqlColumnRef;
                        SqlColumnRef crRight = b.Right as SqlColumnRef;
                        if (crLeft != null && crRight != null) {
                            SqlColumn cLeft = crLeft.GetRootColumn();
                            SqlColumn cRight = crRight.GetRootColumn();
                            this.map[cLeft] = cRight;
                            this.map[cRight] = cLeft;
                        }
                        break;
                    }
                }
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss) {
                return ss;
            }
        }

        // remove redundant/trivial aliases
        class SqlAliasDeflator : SqlVisitor {
            Dictionary<SqlAlias, SqlAlias> removedMap;

            internal SqlAliasDeflator() {
                this.removedMap = new Dictionary<SqlAlias, SqlAlias>();
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref) {
                SqlAlias alias = aref.Alias;
                SqlAlias value;
                if (this.removedMap.TryGetValue(alias, out value)) {
                    throw Error.InvalidReferenceToRemovedAliasDuringDeflation();
                }
                return aref;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                if (cref.Column.Alias != null && this.removedMap.ContainsKey(cref.Column.Alias)) {
                    SqlColumnRef c = cref.Column.Expression as SqlColumnRef;
                    if (c != null) {
                        //The following code checks for cases where there are differences between the type returned
                        //by a ColumnRef and the column that refers to it.  This situation can occur when conversions
                        //are optimized out of the SQL node tree.  As mentioned in the SetClrType comments this is not 
                        //an operation that can have adverse effects and should only be used in limited cases, such as
                        //this one.
                        if (c.ClrType != cref.ClrType) {
                            c.SetClrType(cref.ClrType);
                            return this.VisitColumnRef(c);
                        }
                    }
                    return c;
                }
                return cref;
            }

            internal override SqlSource VisitSource(SqlSource node) {
                node = (SqlSource)this.Visit(node);
                SqlAlias alias = node as SqlAlias;
                if (alias != null) {
                    SqlSelect sel = alias.Node as SqlSelect;
                    if (sel != null && this.IsTrivialSelect(sel)) {
                        this.removedMap[alias] = alias;
                        node = sel.From;
                    }
                }
                return node;
            }

            internal override SqlSource VisitJoin(SqlJoin join) {
                base.VisitJoin(join);
                switch (join.JoinType) {
                    case SqlJoinType.Cross:
                    case SqlJoinType.Inner:
                        // reducing either side would effect cardinality of results
                        break;
                    case SqlJoinType.LeftOuter:
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.OuterApply:
                        // may reduce to left if no references to the right
                        if (this.HasEmptySource(join.Right)) {
                            SqlAlias a = (SqlAlias)join.Right;
                            this.removedMap[a] = a;
                            return join.Left;
                        }
                        break;
                }
                return join;
            }

            private bool IsTrivialSelect(SqlSelect select) {
                if (select.OrderBy.Count != 0 ||
                    select.GroupBy.Count != 0 ||
                    select.Having != null ||
                    select.Top != null ||
                    select.IsDistinct ||
                    select.Where != null)
                    return false;
                return this.HasTrivialSource(select.From) && this.HasTrivialProjection(select);
            }

            private bool HasTrivialSource(SqlSource node) {
                SqlJoin join = node as SqlJoin;
                if (join != null) {
                    return this.HasTrivialSource(join.Left) &&
                           this.HasTrivialSource(join.Right);
                }
                return node is SqlAlias;
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private bool HasTrivialProjection(SqlSelect select) {
                foreach (SqlColumn c in select.Row.Columns) {
                    if (c.Expression != null && c.Expression.NodeType != SqlNodeType.ColumnRef) {
                        return false;
                    }
                }
                return true;
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private bool HasEmptySource(SqlSource node) {
                SqlAlias alias = node as SqlAlias;
                if (alias == null) return false;
                SqlSelect sel = alias.Node as SqlSelect;
                if (sel == null) return false;
                return sel.Row.Columns.Count == 0 &&
                       sel.From == null &&
                       sel.Where == null &&
                       sel.GroupBy.Count == 0 &&
                       sel.Having == null &&
                       sel.OrderBy.Count == 0;
            }
        }

        // remove duplicate columns from order by and group by lists
        class SqlDuplicateColumnDeflator : SqlVisitor
        {
            SqlColumnEqualizer equalizer = new SqlColumnEqualizer();

            internal override SqlSelect VisitSelect(SqlSelect select) {
                select.From = this.VisitSource(select.From);
                select.Where = this.VisitExpression(select.Where);
                for (int i = 0, n = select.GroupBy.Count; i < n; i++)
                {
                    select.GroupBy[i] = this.VisitExpression(select.GroupBy[i]);
                }
                // remove duplicate group expressions
                for (int i = select.GroupBy.Count - 1; i >= 0; i--)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (SqlComparer.AreEqual(select.GroupBy[i], select.GroupBy[j]))
                        {
                            select.GroupBy.RemoveAt(i);
                            break;
                        }
                    }
                }
                select.Having = this.VisitExpression(select.Having);
                for (int i = 0, n = select.OrderBy.Count; i < n; i++)
                {
                    select.OrderBy[i].Expression = this.VisitExpression(select.OrderBy[i].Expression);
                }
                // remove duplicate order expressions
                if (select.OrderBy.Count > 0)
                {
                    this.equalizer.BuildEqivalenceMap(select.From);

                    for (int i = select.OrderBy.Count - 1; i >= 0; i--)
                    {
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (this.equalizer.AreEquivalent(select.OrderBy[i].Expression, select.OrderBy[j].Expression))
                            {
                                select.OrderBy.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
                select.Top = this.VisitExpression(select.Top);
                select.Row = (SqlRow)this.Visit(select.Row);
                select.Selection = this.VisitExpression(select.Selection);
                return select;
            }
        }

        // if the top level select is simply a reprojection of the subquery, then remove it,
        // pushing any distinct names down
        class SqlTopSelectDeflator : SqlVisitor {

            internal override SqlSelect VisitSelect(SqlSelect select) {
                if (IsTrivialSelect(select)) {
                    SqlSelect aselect = (SqlSelect)((SqlAlias)select.From).Node;
                    // build up a column map, so we can rewrite the top-level selection expression
                    Dictionary<SqlColumn, SqlColumnRef> map = new Dictionary<SqlColumn, SqlColumnRef>();
                    foreach (SqlColumn c in select.Row.Columns) {
                        SqlColumnRef cref = (SqlColumnRef)c.Expression;
                        map.Add(c, cref);
                        // push the interesting column names down (non null)
                        if (!string.IsNullOrEmpty(c.Name)) {
                            cref.Column.Name = c.Name;
                        }
                    }
                    aselect.Selection = new ColumnMapper(map).VisitExpression(select.Selection);
                    return aselect;
                }
                return select;
            }

            private bool IsTrivialSelect(SqlSelect select) {
                if (select.OrderBy.Count != 0 ||
                    select.GroupBy.Count != 0 ||
                    select.Having != null ||
                    select.Top != null ||
                    select.IsDistinct ||
                    select.Where != null)
                    return false;
                return this.HasTrivialSource(select.From) && this.HasTrivialProjection(select);
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private bool HasTrivialSource(SqlSource node) {
                SqlAlias alias = node as SqlAlias;
                if (alias == null) return false;
                return alias.Node is SqlSelect;
            }

            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
            private bool HasTrivialProjection(SqlSelect select) {
                foreach (SqlColumn c in select.Row.Columns) {
                    if (c.Expression != null && c.Expression.NodeType != SqlNodeType.ColumnRef) {
                        return false;
                    }
                }
                return true;
            }

            class ColumnMapper : SqlVisitor {
                Dictionary<SqlColumn, SqlColumnRef> map;
                internal ColumnMapper(Dictionary<SqlColumn, SqlColumnRef> map) {
                    this.map = map;
                }
                internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                    SqlColumnRef mapped;
                    if (this.map.TryGetValue(cref.Column, out mapped)) {
                        return mapped;
                    }
                    return cref;
                }
            }
        }
    }
}
