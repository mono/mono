using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Provider;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    internal class SqlDuplicator {
        DuplicatingVisitor superDuper;

        internal SqlDuplicator()
            : this(true) {
        }

        internal SqlDuplicator(bool ignoreExternalRefs) {
            this.superDuper = new DuplicatingVisitor(ignoreExternalRefs);
        }

        internal static SqlNode Copy(SqlNode node) {
            if (node == null)
                return null;
            switch (node.NodeType) {
                case SqlNodeType.ColumnRef:
                case SqlNodeType.Value:
                case SqlNodeType.Parameter:
                case SqlNodeType.Variable:
                    return node;
                default:
                    return new SqlDuplicator().Duplicate(node);
            }
        }

        internal SqlNode Duplicate(SqlNode node) {
            return this.superDuper.Visit(node);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal class DuplicatingVisitor : SqlVisitor {
            Dictionary<SqlNode, SqlNode> nodeMap;
            bool ingoreExternalRefs;

            internal DuplicatingVisitor(bool ignoreExternalRefs) {
                this.ingoreExternalRefs = ignoreExternalRefs;
                this.nodeMap = new Dictionary<SqlNode, SqlNode>();
            }

            internal override SqlNode Visit(SqlNode node) {
                if (node == null) {
                    return null;
                }
                SqlNode result = null;
                if (this.nodeMap.TryGetValue(node, out result)) {
                    return result;
                }
                result = base.Visit(node);
                this.nodeMap[node] = result;
                return result;
            }
            internal override SqlExpression VisitDoNotVisit(SqlDoNotVisitExpression expr) {
                // duplicator can duplicate through a do-no-visit node
                return new SqlDoNotVisitExpression(this.VisitExpression(expr.Expression));
            }
            internal override SqlAlias VisitAlias(SqlAlias a) {
                SqlAlias n = new SqlAlias(a.Node);
                this.nodeMap[a] = n;
                n.Node = this.Visit(a.Node);
                n.Name = a.Name;
                return n;
            }
            internal override SqlExpression VisitAliasRef(SqlAliasRef aref) {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey(aref.Alias)) {
                    return aref;
                }
                return new SqlAliasRef((SqlAlias)this.Visit(aref.Alias));
            }
            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber) {
                List<SqlOrderExpression> orderBy = new List<SqlOrderExpression>();

                foreach (SqlOrderExpression expr in rowNumber.OrderBy) {
                    orderBy.Add(new SqlOrderExpression(expr.OrderType, (SqlExpression)this.Visit(expr.Expression)));
                }

                return new SqlRowNumber(rowNumber.ClrType, rowNumber.SqlType, orderBy, rowNumber.SourceExpression);
            }
            internal override SqlExpression VisitBinaryOperator(SqlBinary bo) {
                SqlExpression left = (SqlExpression)this.Visit(bo.Left);
                SqlExpression right = (SqlExpression)this.Visit(bo.Right);
                return new SqlBinary(bo.NodeType, bo.ClrType, bo.SqlType, left, right, bo.Method);
            }
            internal override SqlExpression VisitClientQuery(SqlClientQuery cq) {
                SqlSubSelect query = (SqlSubSelect) this.VisitExpression(cq.Query);
                SqlClientQuery nq = new SqlClientQuery(query);
                for (int i = 0, n = cq.Arguments.Count; i < n; i++) {
                    nq.Arguments.Add(this.VisitExpression(cq.Arguments[i]));
                }
                for (int i = 0, n = cq.Parameters.Count; i < n; i++) {
                    nq.Parameters.Add((SqlParameter)this.VisitExpression(cq.Parameters[i]));
                }
                return nq;
            }
            internal override SqlExpression VisitJoinedCollection(SqlJoinedCollection jc) {
                return new SqlJoinedCollection(jc.ClrType, jc.SqlType, this.VisitExpression(jc.Expression), this.VisitExpression(jc.Count), jc.SourceExpression);
            }
            internal override SqlExpression VisitClientArray(SqlClientArray scar) {
                SqlExpression[] exprs = new SqlExpression[scar.Expressions.Count];
                for (int i = 0, n = exprs.Length; i < n; i++) {
                    exprs[i] = this.VisitExpression(scar.Expressions[i]);
                }
                return new SqlClientArray(scar.ClrType, scar.SqlType, exprs, scar.SourceExpression);
            }
            internal override SqlExpression VisitTypeCase(SqlTypeCase tc) {
                SqlExpression disc = VisitExpression(tc.Discriminator);
                List<SqlTypeCaseWhen> whens = new List<SqlTypeCaseWhen>();
                foreach(SqlTypeCaseWhen when in tc.Whens) {
                    whens.Add(new SqlTypeCaseWhen(VisitExpression(when.Match), VisitExpression(when.TypeBinding)));
                }
                return new SqlTypeCase(tc.ClrType, tc.SqlType, tc.RowType, disc, whens, tc.SourceExpression);
            }
            internal override SqlExpression VisitNew(SqlNew sox) {
                SqlExpression[] args = new SqlExpression[sox.Args.Count];
                SqlMemberAssign[] bindings = new SqlMemberAssign[sox.Members.Count];
                for (int i = 0, n = args.Length; i < n; i++) {
                    args[i] = this.VisitExpression(sox.Args[i]);
                }
                for (int i = 0, n = bindings.Length; i < n; i++) {
                    bindings[i] = this.VisitMemberAssign(sox.Members[i]);
                }
                return new SqlNew(sox.MetaType, sox.SqlType, sox.Constructor, args, sox.ArgMembers, bindings, sox.SourceExpression);
            }
            internal override SqlNode VisitLink(SqlLink link) {
                SqlExpression[] exprs = new SqlExpression[link.KeyExpressions.Count];
                for (int i = 0, n = exprs.Length; i < n; i++) {
                    exprs[i] = this.VisitExpression(link.KeyExpressions[i]);
                }
                SqlLink newLink = new SqlLink(new object(), link.RowType, link.ClrType, link.SqlType, null, link.Member, exprs, null, link.SourceExpression);
                this.nodeMap[link] = newLink;
                // break the potential cyclic tree by visiting these after adding to the map
                newLink.Expression = this.VisitExpression(link.Expression);
                newLink.Expansion = this.VisitExpression(link.Expansion);
                return newLink;
            }
            internal override SqlExpression VisitColumn(SqlColumn col) {
                SqlColumn n = new SqlColumn(col.ClrType, col.SqlType, col.Name, col.MetaMember, null, col.SourceExpression);
                this.nodeMap[col] = n;
                n.Expression = this.VisitExpression(col.Expression);
                n.Alias = (SqlAlias)this.Visit(col.Alias);
                return n;
            }
            internal override SqlExpression VisitColumnRef(SqlColumnRef cref) {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey(cref.Column)) {
                    return cref;
                }
                return new SqlColumnRef((SqlColumn)this.Visit(cref.Column));
            }
            internal override SqlStatement VisitDelete(SqlDelete sd) {
                return new SqlDelete((SqlSelect)this.Visit(sd.Select), sd.SourceExpression);
            }
            internal override SqlExpression VisitElement(SqlSubSelect elem) {
                return this.VisitMultiset(elem);
            }
            internal override SqlExpression VisitExists(SqlSubSelect sqlExpr) {
                return new SqlSubSelect(sqlExpr.NodeType, sqlExpr.ClrType, sqlExpr.SqlType, (SqlSelect)this.Visit(sqlExpr.Select));
            }
            internal override SqlStatement VisitInsert(SqlInsert si) {
                SqlInsert n = new SqlInsert(si.Table, this.VisitExpression(si.Expression), si.SourceExpression);
                n.OutputKey = si.OutputKey;
                n.OutputToLocal = si.OutputToLocal;
                n.Row = this.VisitRow(si.Row);
                return n;
            }
            internal override SqlSource VisitJoin(SqlJoin join) {
                SqlSource left = this.VisitSource(join.Left);
                SqlSource right = this.VisitSource(join.Right);
                SqlExpression cond = (SqlExpression)this.Visit(join.Condition);
                return new SqlJoin(join.JoinType, left, right, cond, join.SourceExpression);
            }
            internal override SqlExpression VisitValue(SqlValue value) {
                return value;
            }
            internal override SqlNode VisitMember(SqlMember m) {
                return new SqlMember(m.ClrType, m.SqlType, (SqlExpression)this.Visit(m.Expression), m.Member);
            }
            internal override SqlMemberAssign VisitMemberAssign(SqlMemberAssign ma) {
                return new SqlMemberAssign(ma.Member, (SqlExpression)this.Visit(ma.Expression));
            }
            internal override SqlExpression VisitMultiset(SqlSubSelect sms) {
                return new SqlSubSelect(sms.NodeType, sms.ClrType, sms.SqlType, (SqlSelect)this.Visit(sms.Select));
            }
            internal override SqlExpression VisitParameter(SqlParameter p) {
                SqlParameter n = new SqlParameter(p.ClrType, p.SqlType, p.Name, p.SourceExpression);
                n.Direction = p.Direction;
                return n;
            }
            internal override SqlRow VisitRow(SqlRow row) {
                SqlRow nrow = new SqlRow(row.SourceExpression);
                foreach (SqlColumn c in row.Columns) {
                    nrow.Columns.Add((SqlColumn)this.Visit(c));
                }
                return nrow;
            }
            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
                return new SqlSubSelect(SqlNodeType.ScalarSubSelect, ss.ClrType, ss.SqlType, this.VisitSequence(ss.Select));
            }
            internal override SqlSelect VisitSelect(SqlSelect select) {
                SqlSource from = this.VisitSource(select.From);
                List<SqlExpression> gex = null;
                if (select.GroupBy.Count > 0) {
                    gex = new List<SqlExpression>(select.GroupBy.Count);
                    foreach (SqlExpression sqlExpr in select.GroupBy) {
                        gex.Add((SqlExpression)this.Visit(sqlExpr));
                    }
                }
                SqlExpression having = (SqlExpression)this.Visit(select.Having);
                List<SqlOrderExpression> lex = null;
                if (select.OrderBy.Count > 0) {
                    lex = new List<SqlOrderExpression>(select.OrderBy.Count);
                    foreach (SqlOrderExpression sox in select.OrderBy) {
                        SqlOrderExpression nsox = new SqlOrderExpression(sox.OrderType, (SqlExpression)this.Visit(sox.Expression));
                        lex.Add(nsox);
                    }
                }
                SqlExpression top = (SqlExpression)this.Visit(select.Top);
                SqlExpression where = (SqlExpression)this.Visit(select.Where);
                SqlRow row = (SqlRow)this.Visit(select.Row);
                SqlExpression selection = this.VisitExpression(select.Selection);

                SqlSelect n = new SqlSelect(selection, from, select.SourceExpression);
                if (gex != null)
                    n.GroupBy.AddRange(gex);
                n.Having = having;
                if (lex != null)
                    n.OrderBy.AddRange(lex);
                n.OrderingType = select.OrderingType;
                n.Row = row;
                n.Top = top;
                n.IsDistinct = select.IsDistinct;
                n.IsPercent = select.IsPercent;
                n.Where = where;
                n.DoNotOutput = select.DoNotOutput;
                return n;
            }
            internal override SqlTable VisitTable(SqlTable tab) {
                SqlTable nt = new SqlTable(tab.MetaTable, tab.RowType, tab.SqlRowType, tab.SourceExpression);
                this.nodeMap[tab] = nt;
                foreach (SqlColumn c in tab.Columns) {
                    nt.Columns.Add((SqlColumn)this.Visit(c));
                }
                return nt;
            }
            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq) {
                List<SqlExpression> args = new List<SqlExpression>(suq.Arguments.Count);
                foreach (SqlExpression expr in suq.Arguments) {
                    args.Add(this.VisitExpression(expr));
                }
                SqlExpression projection = this.VisitExpression(suq.Projection);
                SqlUserQuery n = new SqlUserQuery(suq.QueryText, projection, args, suq.SourceExpression);
                this.nodeMap[suq] = n;

                foreach (SqlUserColumn suc in suq.Columns) {
                    SqlUserColumn dupSuc = new SqlUserColumn(suc.ClrType, suc.SqlType, suc.Query, suc.Name, suc.IsRequired, suc.SourceExpression);
                    this.nodeMap[suc] = dupSuc;
                    n.Columns.Add(dupSuc);
                }

                return n;
            }
            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc) {
                List<SqlExpression> args = new List<SqlExpression>(spc.Arguments.Count);
                foreach (SqlExpression expr in spc.Arguments) {
                    args.Add(this.VisitExpression(expr));
                }
                SqlExpression projection = this.VisitExpression(spc.Projection);
                SqlStoredProcedureCall n = new SqlStoredProcedureCall(spc.Function, projection, args, spc.SourceExpression);
                this.nodeMap[spc] = n;
                foreach (SqlUserColumn suc in spc.Columns) {
                    n.Columns.Add((SqlUserColumn)this.Visit(suc));
                }
                return n;
            }
            internal override SqlExpression VisitUserColumn(SqlUserColumn suc) {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey(suc)) {
                    return suc;
                }
                return new SqlUserColumn(suc.ClrType, suc.SqlType, suc.Query, suc.Name, suc.IsRequired, suc.SourceExpression);
            }
            internal override SqlExpression VisitUserRow(SqlUserRow row) {
                return new SqlUserRow(row.RowType, row.SqlType, (SqlUserQuery)this.Visit(row.Query), row.SourceExpression);
            }
            internal override SqlExpression VisitTreat(SqlUnary t) {
                return new SqlUnary(SqlNodeType.Treat, t.ClrType, t.SqlType, (SqlExpression)this.Visit(t.Operand), t.SourceExpression);
            }
            internal override SqlExpression VisitUnaryOperator(SqlUnary uo) {
                return new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType, (SqlExpression)this.Visit(uo.Operand), uo.Method, uo.SourceExpression);
            }
            internal override SqlStatement VisitUpdate(SqlUpdate su) {
                SqlSelect ss = (SqlSelect)this.Visit(su.Select);
                List<SqlAssign> assignments = new List<SqlAssign>(su.Assignments.Count);
                foreach (SqlAssign sa in su.Assignments) {
                    assignments.Add((SqlAssign)this.Visit(sa));
                }
                return new SqlUpdate(ss, assignments, su.SourceExpression);
            }
            internal override SqlStatement VisitAssign(SqlAssign sa) {
                return new SqlAssign(this.VisitExpression(sa.LValue), this.VisitExpression(sa.RValue), sa.SourceExpression);
            }
            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c) {
                SqlExpression @else = this.VisitExpression(c.Else);
                SqlWhen[] whens = new SqlWhen[c.Whens.Count];
                for (int i = 0, n = whens.Length; i < n; i++) {
                    SqlWhen when = c.Whens[i];
                    whens[i] = new SqlWhen(this.VisitExpression(when.Match), this.VisitExpression(when.Value));
                }
                return new SqlSearchedCase(c.ClrType, whens, @else, c.SourceExpression);
            }
            internal override SqlExpression VisitClientCase(SqlClientCase c) {
                SqlExpression expr = this.VisitExpression(c.Expression);
                SqlClientWhen[] whens = new SqlClientWhen[c.Whens.Count];
                for (int i = 0, n = whens.Length; i < n; i++) {
                    SqlClientWhen when = c.Whens[i];
                    whens[i] = new SqlClientWhen(this.VisitExpression(when.Match), this.VisitExpression(when.Value));
                }
                return new SqlClientCase(c.ClrType, expr, whens, c.SourceExpression);
            }
            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c) {
                SqlExpression expr = this.VisitExpression(c.Expression);
                SqlWhen[] whens = new SqlWhen[c.Whens.Count];
                for (int i = 0, n = whens.Length; i < n; i++) {
                    SqlWhen when = c.Whens[i];
                    whens[i] = new SqlWhen(this.VisitExpression(when.Match), this.VisitExpression(when.Value));
                }
                return new SqlSimpleCase(c.ClrType, expr, whens, c.SourceExpression);
            }
            internal override SqlNode VisitUnion(SqlUnion su) {
                return new SqlUnion(this.Visit(su.Left), this.Visit(su.Right), su.All);
            }
            internal override SqlExpression VisitExprSet(SqlExprSet xs) {
                SqlExpression[] exprs = new SqlExpression[xs.Expressions.Count];
                for (int i = 0, n = exprs.Length; i < n; i++) {
                    exprs[i] = this.VisitExpression(xs.Expressions[i]);
                }
                return new SqlExprSet(xs.ClrType, exprs, xs.SourceExpression);
            }
            internal override SqlBlock VisitBlock(SqlBlock block) {
                SqlBlock nb = new SqlBlock(block.SourceExpression);
                foreach (SqlStatement stmt in block.Statements) {
                    nb.Statements.Add((SqlStatement)this.Visit(stmt));
                }
                return nb;
            }
            internal override SqlExpression VisitVariable(SqlVariable v) {
                return v;
            }
            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov) {
                SqlExpression hasValue = this.VisitExpression(sov.HasValue);
                SqlExpression value = this.VisitExpression(sov.Value);
                return new SqlOptionalValue(hasValue, value);
            }
            internal override SqlExpression VisitBetween(SqlBetween between) {
                SqlBetween nbet = new SqlBetween(
                    between.ClrType,
                    between.SqlType,
                    this.VisitExpression(between.Expression),
                    this.VisitExpression(between.Start),
                    this.VisitExpression(between.End),
                    between.SourceExpression
                    );
                return nbet;
            }
            internal override SqlExpression VisitIn(SqlIn sin) {
                SqlIn nin = new SqlIn(sin.ClrType, sin.SqlType, this.VisitExpression(sin.Expression), sin.Values, sin.SourceExpression);
                for (int i = 0, n = nin.Values.Count; i < n; i++) {
                    nin.Values[i] = this.VisitExpression(nin.Values[i]);
                }
                return nin;
            }
            internal override SqlExpression VisitLike(SqlLike like) {
                return new SqlLike(
                    like.ClrType, like.SqlType,
                    this.VisitExpression(like.Expression),
                    this.VisitExpression(like.Pattern),
                    this.VisitExpression(like.Escape),
                    like.SourceExpression
                    );
            }
            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc) {
                SqlExpression[] args = new SqlExpression[fc.Arguments.Count];
                for (int i = 0, n = fc.Arguments.Count; i < n; i++) {
                    args[i] = this.VisitExpression(fc.Arguments[i]);
                }
                return new SqlFunctionCall(fc.ClrType, fc.SqlType, fc.Name, args, fc.SourceExpression);
            }
            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc) {
                SqlExpression[] args = new SqlExpression[fc.Arguments.Count];
                for (int i = 0, n = fc.Arguments.Count; i < n; i++) {
                    args[i] = this.VisitExpression(fc.Arguments[i]);
                }
                SqlTableValuedFunctionCall nfc = new SqlTableValuedFunctionCall(fc.RowType, fc.ClrType, fc.SqlType, fc.Name, args, fc.SourceExpression);
                this.nodeMap[fc] = nfc;
                foreach (SqlColumn c in fc.Columns) {
                    nfc.Columns.Add((SqlColumn)this.Visit(c));
                }
                return nfc;
            }
            internal override SqlExpression VisitMethodCall(SqlMethodCall mc) {
                SqlExpression[] args = new SqlExpression[mc.Arguments.Count];
                for (int i = 0, n = mc.Arguments.Count; i < n; i++) {
                    args[i] = this.VisitExpression(mc.Arguments[i]);
                }
                return new SqlMethodCall(mc.ClrType, mc.SqlType, mc.Method, this.VisitExpression(mc.Object), args, mc.SourceExpression);
            }
            internal override SqlExpression VisitSharedExpression(SqlSharedExpression sub) {
                SqlSharedExpression n = new SqlSharedExpression(sub.Expression);
                this.nodeMap[sub] = n;
                n.Expression = this.VisitExpression(sub.Expression);
                return n;
            }
            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref) {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey(sref.SharedExpression)) {
                    return sref;
                }
                return new SqlSharedExpressionRef((SqlSharedExpression)this.Visit(sref.SharedExpression));
            }
            internal override SqlExpression VisitSimpleExpression(SqlSimpleExpression simple) {
                SqlSimpleExpression n = new SqlSimpleExpression(this.VisitExpression(simple.Expression));
                return n;
            }
            internal override SqlExpression VisitGrouping(SqlGrouping g) {
                SqlGrouping n = new SqlGrouping(g.ClrType, g.SqlType,
                    this.VisitExpression(g.Key), this.VisitExpression(g.Group), g.SourceExpression
                    );
                return n;
            }
            internal override SqlExpression VisitDiscriminatedType(SqlDiscriminatedType dt) {
                return new SqlDiscriminatedType(dt.SqlType, this.VisitExpression(dt.Discriminator), dt.TargetType, dt.SourceExpression);
            }
            internal override SqlExpression VisitLift(SqlLift lift) {
                return new SqlLift(lift.ClrType, this.VisitExpression(lift.Expression), lift.SourceExpression);
            }
            internal override SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof) {
                return new SqlDiscriminatorOf(this.VisitExpression(dof.Object), dof.ClrType, dof.SqlType, dof.SourceExpression);
            }
            internal override SqlNode VisitIncludeScope(SqlIncludeScope scope) {
                return new SqlIncludeScope(this.Visit(scope.Child), scope.SourceExpression);
            }
        }
    }
}
