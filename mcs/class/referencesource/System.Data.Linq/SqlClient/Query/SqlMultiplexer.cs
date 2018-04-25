using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data.Linq;

namespace System.Data.Linq.SqlClient {

    // convert multiset & element expressions into separate queries

    internal class SqlMultiplexer {
        Visitor visitor;

        internal enum Options {
            None,
            EnableBigJoin
        }

        internal SqlMultiplexer(Options options, IEnumerable<System.Data.Linq.SqlClient.SqlParameter> parentParameters, SqlFactory sqlFactory) {
            this.visitor = new Visitor(options, parentParameters, sqlFactory);
        }

        internal SqlNode Multiplex(SqlNode node) {
            return this.visitor.Visit(node);
        }

        class Visitor : SqlVisitor {
            Options options;
            SqlFactory sql;
            SqlSelect outerSelect;
            bool hasBigJoin;
            bool canJoin;
            bool isTopLevel;
            IEnumerable<System.Data.Linq.SqlClient.SqlParameter> parentParameters;

            internal Visitor(Options options, IEnumerable<System.Data.Linq.SqlClient.SqlParameter> parentParameters, SqlFactory sqlFactory) {
                this.options = options;
                this.sql = sqlFactory;
                this.canJoin = true;
                this.isTopLevel = true;
                this.parentParameters = parentParameters;
            }

            internal override SqlExpression VisitMultiset(SqlSubSelect sms) {
                // allow one big-join per query?
                if ((this.options & Options.EnableBigJoin) != 0 &&
                    !this.hasBigJoin && this.canJoin && this.isTopLevel && this.outerSelect != null
                    && !MultisetChecker.HasMultiset(sms.Select.Selection) 
                    && BigJoinChecker.CanBigJoin(sms.Select)) {

                    sms.Select = this.VisitSelect(sms.Select);

                    SqlAlias alias = new SqlAlias(sms.Select);
                    SqlJoin join = new SqlJoin(SqlJoinType.OuterApply, this.outerSelect.From, alias, null, sms.SourceExpression);
                    this.outerSelect.From = join;
                    this.outerSelect.OrderingType = SqlOrderingType.Always;

                    // make joined expression
                    SqlExpression expr = (SqlExpression)SqlDuplicator.Copy(sms.Select.Selection);

                    // make count expression
                    SqlSelect copySelect = (SqlSelect)SqlDuplicator.Copy(sms.Select);
                    SqlAlias copyAlias = new SqlAlias(copySelect);
                    SqlSelect countSelect = new SqlSelect(sql.Unary(SqlNodeType.Count, null, sms.SourceExpression), copyAlias, sms.SourceExpression);
                    countSelect.OrderingType = SqlOrderingType.Never;
                    SqlExpression count = sql.SubSelect(SqlNodeType.ScalarSubSelect, countSelect);

                    // make joined collection
                    SqlJoinedCollection jc = new SqlJoinedCollection(sms.ClrType, sms.SqlType, expr, count, sms.SourceExpression);
                    this.hasBigJoin = true;
                    return jc;
                }
                else {
                    return QueryExtractor.Extract(sms, this.parentParameters);
                }
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem) {
                return QueryExtractor.Extract(elem, this.parentParameters);
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
                bool saveIsTopLevel = this.isTopLevel;
                this.isTopLevel = false;
                bool saveCanJoin = this.canJoin;
                this.canJoin = false;
                try {
                    return base.VisitScalarSubSelect(ss);
                }
                finally {
                    this.isTopLevel = saveIsTopLevel;
                    this.canJoin = saveCanJoin;
                }
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss) {
                bool saveIsTopLevel = this.isTopLevel;
                this.isTopLevel = false;
                bool saveCanJoin = this.canJoin;
                this.canJoin = false;
                try {
                    return base.VisitExists(ss);
                }
                finally {
                    this.isTopLevel = saveIsTopLevel;
                    this.canJoin = saveCanJoin;
                }
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                SqlSelect saveSelect = this.outerSelect;
                this.outerSelect = select;

                // big-joins may need to lift PK's out for default ordering, so don't allow big-join if we see these
                this.canJoin &= select.GroupBy.Count == 0 && select.Top == null && !select.IsDistinct;

                bool saveIsTopLevel = this.isTopLevel;
                this.isTopLevel = false;

                select = this.VisitSelectCore(select);

                this.isTopLevel = saveIsTopLevel;
                select.Selection = this.VisitExpression(select.Selection);

                this.isTopLevel = saveIsTopLevel;
                this.outerSelect = saveSelect;

                if (select.IsDistinct && HierarchyChecker.HasHierarchy(select.Selection)) {
                    // distinct across heirarchy is a NO-OP
                    select.IsDistinct = false;
                }
                return select;
            }

            internal override SqlNode VisitUnion(SqlUnion su) {
                this.canJoin = false;
                return base.VisitUnion(su);
            }

            internal override SqlExpression VisitClientCase(SqlClientCase c) {
                bool saveCanJoin = this.canJoin;
                this.canJoin = false;
                try {
                    return base.VisitClientCase(c);
                }
                finally {
                    this.canJoin = saveCanJoin;
                }
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c) {
                bool saveCanJoin = this.canJoin;
                this.canJoin = false;
                try {
                    return base.VisitSimpleCase(c);
                }
                finally {
                    this.canJoin = saveCanJoin;
                }
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c) {
                bool saveCanJoin = this.canJoin;
                this.canJoin = false;
                try {
                    return base.VisitSearchedCase(c);
                }
                finally {
                    this.canJoin = saveCanJoin;
                }
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase tc) {
                bool saveCanJoin = this.canJoin;
                this.canJoin = false;
                try {
                    return base.VisitTypeCase(tc);
                }
                finally {
                    this.canJoin = saveCanJoin;
                }
            }

            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov) {
                bool saveCanJoin = this.canJoin;
                this.canJoin = false;
                try {
                    return base.VisitOptionalValue(sov);
                }
                finally {
                    this.canJoin = saveCanJoin;
                }
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq) {
                this.canJoin = false;
                return base.VisitUserQuery(suq);
            }
        }
    }

    internal class QueryExtractor {

        internal static SqlClientQuery Extract(SqlSubSelect subquery, IEnumerable<System.Data.Linq.SqlClient.SqlParameter> parentParameters) {
            SqlClientQuery cq = new SqlClientQuery(subquery);
            if (parentParameters != null) {
                cq.Parameters.AddRange(parentParameters);
            }
            Visitor v = new Visitor(cq.Arguments, cq.Parameters);
            cq.Query = (SqlSubSelect)v.Visit(subquery);
            return cq;
        }

        class Visitor : SqlDuplicator.DuplicatingVisitor {
            List<SqlExpression> externals;
            List<SqlParameter> parameters;

            internal Visitor(List<SqlExpression> externals, List<SqlParameter> parameters)
                : base(true) {
                this.externals = externals;
                this.parameters = parameters;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                SqlExpression result = base.VisitColumnRef(cref);
                if (result == cref) { // must be external
                    return ExtractParameter(result);
                }
                return result;
            }

            internal override SqlExpression VisitUserColumn(SqlUserColumn suc) {
                SqlExpression result = base.VisitUserColumn(suc);
                if (result == suc) { // must be external
                    return ExtractParameter(result);
                }
                return result;
            }

            private SqlExpression ExtractParameter(SqlExpression expr) {
                Type clrType = expr.ClrType;
                if (expr.ClrType.IsValueType && !TypeSystem.IsNullableType(expr.ClrType)) {
                    clrType = typeof(Nullable<>).MakeGenericType(expr.ClrType);
                }
                this.externals.Add(expr);
                SqlParameter sp = new SqlParameter(clrType, expr.SqlType, "@x" + (this.parameters.Count + 1), expr.SourceExpression);
                this.parameters.Add(sp);
                return sp;
            }

            internal override SqlNode VisitLink(SqlLink link) {
                // Don't visit the Expression/Expansion for this link.
                // Any additional external refs in these expressions
                // should be ignored
                SqlExpression[] exprs = new SqlExpression[link.KeyExpressions.Count];
                for (int i = 0, n = exprs.Length; i < n; i++) {
                    exprs[i] = this.VisitExpression(link.KeyExpressions[i]);
                }
                return new SqlLink(new object(), link.RowType, link.ClrType, link.SqlType, null, link.Member, exprs, null, link.SourceExpression);
            }
        }
    }

    internal class HierarchyChecker {
        internal static bool HasHierarchy(SqlExpression expr) {
            Visitor v = new Visitor();
            v.Visit(expr);
            return v.foundHierarchy;
        }

        class Visitor : SqlVisitor {
            internal bool foundHierarchy;

            internal override SqlExpression VisitMultiset(SqlSubSelect sms) {
                this.foundHierarchy = true;
                return sms;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem) {
                this.foundHierarchy = true;
                return elem;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq) {
                this.foundHierarchy = true;
                return cq;
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss) {
                return ss;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
                return ss;
            }
        }
    }

    internal class MultisetChecker {
        internal static bool HasMultiset(SqlExpression expr) {
            Visitor v = new Visitor();
            v.Visit(expr);
            return v.foundMultiset;
        }

        class Visitor : SqlVisitor {
            internal bool foundMultiset;

            internal override SqlExpression VisitMultiset(SqlSubSelect sms) {
                this.foundMultiset = true;
                return sms;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem) {
                return elem;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq) {
                return cq;
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss) {
                return ss;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
                return ss;
            }
        }
    }

    internal class BigJoinChecker {
        internal static bool CanBigJoin(SqlSelect select) {
            Visitor v = new Visitor();
            v.Visit(select);
            return v.canBigJoin;
        }

        class Visitor : SqlVisitor {
            internal bool canBigJoin = true;

            internal override SqlExpression VisitMultiset(SqlSubSelect sms) {
                return sms;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem) {
                return elem;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq) {
                return cq;
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss) {
                return ss;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
                return ss;
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                // big-joins may need to lift PK's out for default ordering, so don't allow big-join if we see these
                this.canBigJoin &= select.GroupBy.Count == 0 && select.Top == null && !select.IsDistinct;
                if (!this.canBigJoin) {
                    return select;
                }
                return base.VisitSelect(select);
            }
        }
    }
}
