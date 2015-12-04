using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {
    internal abstract class SqlVisitor {
        int nDepth;
 
        // Visit a SqlNode
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification="[....]: Cast is dependent on node type and casts do not happen unecessarily in a single code path.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal virtual SqlNode Visit(SqlNode node) {
            SqlNode result = null;
            if (node == null) {
                return null;
            }

            try {
                nDepth++;
                CheckRecursionDepth(500, nDepth);

                switch (node.NodeType) {
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                    case SqlNodeType.Negate:
                    case SqlNodeType.BitNot:
                    case SqlNodeType.IsNull:
                    case SqlNodeType.IsNotNull:
                    case SqlNodeType.Count:
                    case SqlNodeType.LongCount:
                    case SqlNodeType.Max:
                    case SqlNodeType.Min:
                    case SqlNodeType.Sum:
                    case SqlNodeType.Avg:
                    case SqlNodeType.Stddev:
                    case SqlNodeType.Convert:
                    case SqlNodeType.ValueOf:
                    case SqlNodeType.OuterJoinedValue:
                    case SqlNodeType.ClrLength:
                        result = this.VisitUnaryOperator((SqlUnary)node);
                        break;
                    case SqlNodeType.Lift:
                        result = this.VisitLift((SqlLift)node);
                        break;
                    case SqlNodeType.Add:
                    case SqlNodeType.Sub:
                    case SqlNodeType.Mul:
                    case SqlNodeType.Div:
                    case SqlNodeType.Mod:
                    case SqlNodeType.BitAnd:
                    case SqlNodeType.BitOr:
                    case SqlNodeType.BitXor:
                    case SqlNodeType.And:
                    case SqlNodeType.Or:
                    case SqlNodeType.GE:
                    case SqlNodeType.GT:
                    case SqlNodeType.LE:
                    case SqlNodeType.LT:
                    case SqlNodeType.EQ:
                    case SqlNodeType.NE:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.Concat:
                    case SqlNodeType.Coalesce:
                        result = this.VisitBinaryOperator((SqlBinary)node);
                        break;
                    case SqlNodeType.Between:
                        result = this.VisitBetween((SqlBetween)node);
                        break;
                    case SqlNodeType.In:
                        result = this.VisitIn((SqlIn)node);
                        break;
                    case SqlNodeType.Like:
                        result = this.VisitLike((SqlLike)node);
                        break;
                    case SqlNodeType.Treat:
                        result = this.VisitTreat((SqlUnary)node);
                        break;
                    case SqlNodeType.Alias:
                        result = this.VisitAlias((SqlAlias)node);
                        break;
                    case SqlNodeType.AliasRef:
                        result = this.VisitAliasRef((SqlAliasRef)node);
                        break;
                    case SqlNodeType.Member:
                        result = this.VisitMember((SqlMember)node);
                        break;
                    case SqlNodeType.Row:
                        result = this.VisitRow((SqlRow)node);
                        break;
                    case SqlNodeType.Column:
                        result = this.VisitColumn((SqlColumn)node);
                        break;
                    case SqlNodeType.ColumnRef:
                        result = this.VisitColumnRef((SqlColumnRef)node);
                        break;
                    case SqlNodeType.Table:
                        result = this.VisitTable((SqlTable)node);
                        break;
                    case SqlNodeType.UserQuery:
                        result = this.VisitUserQuery((SqlUserQuery)node);
                        break;
                    case SqlNodeType.StoredProcedureCall:
                        result = this.VisitStoredProcedureCall((SqlStoredProcedureCall)node);
                        break;
                    case SqlNodeType.UserRow:
                        result = this.VisitUserRow((SqlUserRow)node);
                        break;
                    case SqlNodeType.UserColumn:
                        result = this.VisitUserColumn((SqlUserColumn)node);
                        break;
                    case SqlNodeType.Multiset:
                    case SqlNodeType.ScalarSubSelect:
                    case SqlNodeType.Element:
                    case SqlNodeType.Exists:
                        result = this.VisitSubSelect((SqlSubSelect)node);
                        break;
                    case SqlNodeType.Join:
                        result = this.VisitJoin((SqlJoin)node);
                        break;
                    case SqlNodeType.Select:
                        result = this.VisitSelect((SqlSelect)node);
                        break;
                    case SqlNodeType.Parameter:
                        result = this.VisitParameter((SqlParameter)node);
                        break;
                    case SqlNodeType.New:
                        result = this.VisitNew((SqlNew)node);
                        break;
                    case SqlNodeType.Link:
                        result = this.VisitLink((SqlLink)node);
                        break;
                    case SqlNodeType.ClientQuery:
                        result = this.VisitClientQuery((SqlClientQuery)node);
                        break;
                    case SqlNodeType.JoinedCollection:
                        result = this.VisitJoinedCollection((SqlJoinedCollection)node);
                        break;
                    case SqlNodeType.Value:
                        result = this.VisitValue((SqlValue)node);
                        break;
                    case SqlNodeType.ClientArray:
                        result = this.VisitClientArray((SqlClientArray)node);
                        break;
                    case SqlNodeType.Insert:
                        result = this.VisitInsert((SqlInsert)node);
                        break;
                    case SqlNodeType.Update:
                        result = this.VisitUpdate((SqlUpdate)node);
                        break;
                    case SqlNodeType.Delete:
                        result = this.VisitDelete((SqlDelete)node);
                        break;
                    case SqlNodeType.MemberAssign:
                        result = this.VisitMemberAssign((SqlMemberAssign)node);
                        break;
                    case SqlNodeType.Assign:
                        result = this.VisitAssign((SqlAssign)node);
                        break;
                    case SqlNodeType.Block:
                        result = this.VisitBlock((SqlBlock)node);
                        break;
                    case SqlNodeType.SearchedCase:
                        result = this.VisitSearchedCase((SqlSearchedCase)node);
                        break;
                    case SqlNodeType.ClientCase:
                        result = this.VisitClientCase((SqlClientCase)node);
                        break;
                    case SqlNodeType.SimpleCase:
                        result = this.VisitSimpleCase((SqlSimpleCase)node);
                        break;
                    case SqlNodeType.TypeCase:
                        result = this.VisitTypeCase((SqlTypeCase)node);
                        break;
                    case SqlNodeType.Union:
                        result = this.VisitUnion((SqlUnion)node);
                        break;
                    case SqlNodeType.ExprSet:
                        result = this.VisitExprSet((SqlExprSet)node);
                        break;
                    case SqlNodeType.Variable:
                        result = this.VisitVariable((SqlVariable)node);
                        break;
                    case SqlNodeType.DoNotVisit:
                        result = this.VisitDoNotVisit((SqlDoNotVisitExpression)node);
                        break;
                    case SqlNodeType.OptionalValue:
                        result = this.VisitOptionalValue((SqlOptionalValue)node);
                        break;
                    case SqlNodeType.FunctionCall:
                        result = this.VisitFunctionCall((SqlFunctionCall)node);
                        break;
                    case SqlNodeType.TableValuedFunctionCall:
                        result = this.VisitTableValuedFunctionCall((SqlTableValuedFunctionCall)node);
                        break;
                    case SqlNodeType.MethodCall:
                        result = this.VisitMethodCall((SqlMethodCall)node);
                        break;
                    case SqlNodeType.Nop:
                        result = this.VisitNop((SqlNop)node);
                        break;
                    case SqlNodeType.SharedExpression:
                        result = this.VisitSharedExpression((SqlSharedExpression)node);
                        break;
                    case SqlNodeType.SharedExpressionRef:
                        result = this.VisitSharedExpressionRef((SqlSharedExpressionRef)node);
                        break;
                    case SqlNodeType.SimpleExpression:
                        result = this.VisitSimpleExpression((SqlSimpleExpression)node);
                        break;
                    case SqlNodeType.Grouping:
                        result = this.VisitGrouping((SqlGrouping)node);
                        break;
                    case SqlNodeType.DiscriminatedType:
                        result = this.VisitDiscriminatedType((SqlDiscriminatedType)node);
                        break;
                    case SqlNodeType.DiscriminatorOf:
                        result = this.VisitDiscriminatorOf((SqlDiscriminatorOf)node);
                        break;
                    case SqlNodeType.ClientParameter:
                        result = this.VisitClientParameter((SqlClientParameter)node);
                        break;
                    case SqlNodeType.RowNumber:
                        result = this.VisitRowNumber((SqlRowNumber)node);
                        break;
                    case SqlNodeType.IncludeScope:
                        result = this.VisitIncludeScope((SqlIncludeScope)node);
                        break;
                    default:
                        throw Error.UnexpectedNode(node);
                }
            } finally {
                this.nDepth--;
            }

            return result;
        }


        /// <summary>
        /// This method checks the recursion level to help diagnose/prevent
        /// infinite recursion in debug builds. Calls are ommitted in non debug builds.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification="Debug-only code.")]
        [Conditional("DEBUG")]
        internal static void CheckRecursionDepth(int maxLevel, int level) {           
            if (level > maxLevel) {
                System.Diagnostics.Debug.Assert(false);
                //**********************************************************************
                // EXCLUDING FROM LOCALIZATION.
                // Reason: This code only executes in DEBUG.
                throw new Exception("Infinite Descent?");
                //**********************************************************************
            }  
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        internal object Eval(SqlExpression expr) {
            if (expr.NodeType == SqlNodeType.Value) {
                return ((SqlValue)expr).Value;
            }
            throw Error.UnexpectedNode(expr.NodeType);
        }

        internal virtual SqlExpression VisitDoNotVisit(SqlDoNotVisitExpression expr) {
            return expr.Expression;
        }
        internal virtual SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber) {
            for (int i = 0, n = rowNumber.OrderBy.Count; i < n; i++) {
                rowNumber.OrderBy[i].Expression = this.VisitExpression(rowNumber.OrderBy[i].Expression);
            } 
            
            return rowNumber;
        }
        internal virtual SqlExpression VisitExpression(SqlExpression exp) {
            return (SqlExpression)this.Visit(exp);
        }
        internal virtual SqlSelect VisitSequence(SqlSelect sel) {
            return (SqlSelect)this.Visit(sel);
        }
        internal virtual SqlExpression VisitNop(SqlNop nop) {
            return nop;
        }
        internal virtual SqlExpression VisitLift(SqlLift lift) {
            lift.Expression = this.VisitExpression(lift.Expression);
            return lift;
        }
        internal virtual SqlExpression VisitUnaryOperator(SqlUnary uo) {
            uo.Operand = this.VisitExpression(uo.Operand);
            return uo;
        }
        internal virtual SqlExpression VisitBinaryOperator(SqlBinary bo) {
            bo.Left = this.VisitExpression(bo.Left);
            bo.Right = this.VisitExpression(bo.Right);
            return bo;
        }
        internal virtual SqlAlias VisitAlias(SqlAlias a) {
            a.Node = this.Visit(a.Node);
            return a;
        }
        internal virtual SqlExpression VisitAliasRef(SqlAliasRef aref) {
            return aref;
        }
        internal virtual SqlNode VisitMember(SqlMember m) {
            m.Expression = this.VisitExpression(m.Expression);
            return m;
        }
        internal virtual SqlExpression VisitCast(SqlUnary c) {
            c.Operand = this.VisitExpression(c.Operand);
            return c;
        }
        internal virtual SqlExpression VisitTreat(SqlUnary t) {
            t.Operand = this.VisitExpression(t.Operand);
            return t;
        }
        internal virtual SqlTable VisitTable(SqlTable tab) {
            return tab;
        }
        internal virtual SqlUserQuery VisitUserQuery(SqlUserQuery suq) {
            for (int i = 0, n = suq.Arguments.Count; i < n; i++) {
                suq.Arguments[i] = this.VisitExpression(suq.Arguments[i]);
            }
            suq.Projection = this.VisitExpression(suq.Projection);
            for (int i = 0, n = suq.Columns.Count; i < n; i++) {
                suq.Columns[i] = (SqlUserColumn) this.Visit(suq.Columns[i]);
            }
            return suq;
        }
        internal virtual SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc) {
            for (int i = 0, n = spc.Arguments.Count; i < n; i++) {
                spc.Arguments[i] = this.VisitExpression(spc.Arguments[i]);
            }
            spc.Projection = this.VisitExpression(spc.Projection);
            for (int i = 0, n = spc.Columns.Count; i < n; i++) {
                spc.Columns[i] = (SqlUserColumn) this.Visit(spc.Columns[i]);
            }
            return spc;
        }
        internal virtual SqlExpression VisitUserColumn(SqlUserColumn suc) {
            return suc;
        }
        internal virtual SqlExpression VisitUserRow(SqlUserRow row) {
            return row;
        }
        internal virtual SqlRow VisitRow(SqlRow row) {
            for (int i = 0, n = row.Columns.Count; i < n; i++) {
                row.Columns[i].Expression = this.VisitExpression(row.Columns[i].Expression);
            }
            return row;
        }
        internal virtual SqlExpression VisitNew(SqlNew sox) {
            for (int i = 0, n = sox.Args.Count; i < n; i++) {
                sox.Args[i] = this.VisitExpression(sox.Args[i]);
            }
            for (int i = 0, n = sox.Members.Count; i < n; i++) {
                sox.Members[i].Expression = this.VisitExpression(sox.Members[i].Expression);
            }
            return sox;
        }
        internal virtual SqlNode VisitLink(SqlLink link) {
            // Don't visit the link's Expansion
            for (int i = 0, n = link.KeyExpressions.Count; i < n; i++) {
                link.KeyExpressions[i] = this.VisitExpression(link.KeyExpressions[i]);
            }
            return link;
        }
        internal virtual SqlExpression VisitClientQuery(SqlClientQuery cq) {
            for (int i = 0, n = cq.Arguments.Count; i < n; i++) {
                cq.Arguments[i] = this.VisitExpression(cq.Arguments[i]);
            }
            return cq;
        }
        internal virtual SqlExpression VisitJoinedCollection(SqlJoinedCollection jc) {
            jc.Expression = this.VisitExpression(jc.Expression);
            jc.Count = this.VisitExpression(jc.Count);
            return jc;
        }
        internal virtual SqlExpression VisitClientArray(SqlClientArray scar) {
            for (int i = 0, n = scar.Expressions.Count; i < n; i++) {
                scar.Expressions[i] = this.VisitExpression(scar.Expressions[i]);
            }
            return scar;
        }
        internal virtual SqlExpression VisitClientParameter(SqlClientParameter cp) {
            return cp;
        }
        internal virtual SqlExpression VisitColumn(SqlColumn col) {
            col.Expression = this.VisitExpression(col.Expression);
            return col;
        }
        internal virtual SqlExpression VisitColumnRef(SqlColumnRef cref) {
            return cref;
        }
        internal virtual SqlExpression VisitParameter(SqlParameter p) {
            return p;
        }
        internal virtual SqlExpression VisitValue(SqlValue value) {
            return value;
        }
        internal virtual SqlExpression VisitSubSelect(SqlSubSelect ss) {
            switch(ss.NodeType) {
               case SqlNodeType.ScalarSubSelect: return this.VisitScalarSubSelect(ss);
               case SqlNodeType.Multiset: return this.VisitMultiset(ss);
               case SqlNodeType.Element: return this.VisitElement(ss);
               case SqlNodeType.Exists: return this.VisitExists(ss);
            }
            throw Error.UnexpectedNode(ss.NodeType);
        }
        internal virtual SqlExpression VisitScalarSubSelect(SqlSubSelect ss) {
            ss.Select = this.VisitSequence(ss.Select);
            return ss;
        }
        internal virtual SqlExpression VisitMultiset(SqlSubSelect sms) {
            sms.Select = this.VisitSequence(sms.Select);
            return sms;
        }
        internal virtual SqlExpression VisitElement(SqlSubSelect elem) {
            elem.Select = this.VisitSequence(elem.Select);
            return elem;
        }
        internal virtual SqlExpression VisitExists(SqlSubSelect sqlExpr) {
            sqlExpr.Select = this.VisitSequence(sqlExpr.Select);
            return sqlExpr;
        }
        internal virtual SqlSource VisitJoin(SqlJoin join) {
            join.Left = this.VisitSource(join.Left);
            join.Right = this.VisitSource(join.Right);
            join.Condition = this.VisitExpression(join.Condition);
            return join;
        }
        internal virtual SqlSource VisitSource(SqlSource source) {
            return (SqlSource) this.Visit(source);
        }
        internal virtual SqlSelect VisitSelectCore(SqlSelect select) {
            select.From = this.VisitSource(select.From);
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
            return select;
        }
        internal virtual SqlSelect VisitSelect(SqlSelect select) {
            select = this.VisitSelectCore(select);
            select.Selection = this.VisitExpression(select.Selection);
            return select;
        }
        internal virtual SqlStatement VisitInsert(SqlInsert insert) {
            insert.Table = (SqlTable)this.Visit(insert.Table);
            insert.Expression = this.VisitExpression(insert.Expression);
            insert.Row = (SqlRow)this.Visit(insert.Row);
            return insert;
        }
        internal virtual SqlStatement VisitUpdate(SqlUpdate update) {
            update.Select = this.VisitSequence(update.Select);
            for (int i = 0, n = update.Assignments.Count; i < n; i++) {
                update.Assignments[i] = (SqlAssign)this.Visit(update.Assignments[i]);
            }
            return update;
        }
        internal virtual SqlStatement VisitDelete(SqlDelete delete) {
            delete.Select = this.VisitSequence(delete.Select);
            return delete;
        }
        internal virtual SqlMemberAssign VisitMemberAssign(SqlMemberAssign ma) {
            ma.Expression = this.VisitExpression(ma.Expression);
            return ma;
        }
        internal virtual SqlStatement VisitAssign(SqlAssign sa) {
            sa.LValue = this.VisitExpression(sa.LValue);
            sa.RValue = this.VisitExpression(sa.RValue);
            return sa;
        }
        internal virtual SqlBlock VisitBlock(SqlBlock b) {
            for (int i = 0, n = b.Statements.Count; i < n; i++) {
                b.Statements[i] = (SqlStatement)this.Visit(b.Statements[i]);
            }
            return b;
        }
        internal virtual SqlExpression VisitSearchedCase(SqlSearchedCase c) {
            for (int i = 0, n = c.Whens.Count; i < n; i++) {
                SqlWhen when = c.Whens[i];
                when.Match = this.VisitExpression(when.Match);
                when.Value = this.VisitExpression(when.Value);
            }
            c.Else = this.VisitExpression(c.Else);
            return c;
        }
        internal virtual SqlExpression VisitClientCase(SqlClientCase c) {
            c.Expression = this.VisitExpression(c.Expression);
            for (int i = 0, n = c.Whens.Count; i < n; i++) {
                SqlClientWhen when = c.Whens[i];
                when.Match = this.VisitExpression(when.Match);
                when.Value = this.VisitExpression(when.Value);
            }
            return c;
        }
        internal virtual SqlExpression VisitSimpleCase(SqlSimpleCase c) {
            c.Expression = this.VisitExpression(c.Expression);
            for (int i = 0, n = c.Whens.Count; i < n; i++) {
                SqlWhen when = c.Whens[i];
                when.Match = this.VisitExpression(when.Match);
                when.Value = this.VisitExpression(when.Value);
            }
            return c;
        }
        internal virtual SqlExpression VisitTypeCase(SqlTypeCase tc) {
            tc.Discriminator = this.VisitExpression(tc.Discriminator);
            for (int i = 0, n = tc.Whens.Count; i < n; i++) {
                SqlTypeCaseWhen when = tc.Whens[i];
                when.Match = this.VisitExpression(when.Match);
                when.TypeBinding = this.VisitExpression(when.TypeBinding);
            }
            return tc;
        }
        internal virtual SqlNode VisitUnion(SqlUnion su) {
            su.Left = this.Visit(su.Left);
            su.Right = this.Visit(su.Right);
            return su;
        }
        internal virtual SqlExpression VisitExprSet(SqlExprSet xs) {
            for (int i = 0, n = xs.Expressions.Count; i < n; i++) {
                xs.Expressions[i] = this.VisitExpression(xs.Expressions[i]);
            }
            return xs;
        }
        internal virtual SqlExpression VisitVariable(SqlVariable v) {
            return v;
        }
        internal virtual SqlExpression VisitOptionalValue(SqlOptionalValue sov) {
            sov.HasValue = this.VisitExpression(sov.HasValue);
            sov.Value = this.VisitExpression(sov.Value);
            return sov;
        }
        internal virtual SqlExpression VisitBetween(SqlBetween between) {
            between.Expression = this.VisitExpression(between.Expression);
            between.Start = this.VisitExpression(between.Start);
            between.End = this.VisitExpression(between.End);
            return between;
        }
        internal virtual SqlExpression VisitIn(SqlIn sin) {
            sin.Expression = this.VisitExpression(sin.Expression);
            for (int i = 0, n = sin.Values.Count; i < n; i++) {
                sin.Values[i] = this.VisitExpression(sin.Values[i]);
            }
            return sin;
        }
        internal virtual SqlExpression VisitLike(SqlLike like) {
            like.Expression = this.VisitExpression(like.Expression);
            like.Pattern = this.VisitExpression(like.Pattern);
            like.Escape = this.VisitExpression(like.Escape);
            return like;
        }
        internal virtual SqlExpression VisitFunctionCall(SqlFunctionCall fc) {
            for (int i = 0, n = fc.Arguments.Count; i < n; i++) {
                fc.Arguments[i] = this.VisitExpression(fc.Arguments[i]);
            }
            return fc;
        }
        internal virtual SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc) {
            for (int i = 0, n = fc.Arguments.Count; i < n; i++) {
                fc.Arguments[i] = this.VisitExpression(fc.Arguments[i]);
            }
            return fc;
        }
        internal virtual SqlExpression VisitMethodCall(SqlMethodCall mc) {
            mc.Object = this.VisitExpression(mc.Object);
            for (int i = 0, n = mc.Arguments.Count; i < n; i++) {
                mc.Arguments[i] = this.VisitExpression(mc.Arguments[i]);
            }
            return mc;
        }
        internal virtual SqlExpression VisitSharedExpression(SqlSharedExpression shared) {
            shared.Expression = this.VisitExpression(shared.Expression);
            return shared;
        }
        internal virtual SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref) {
            return sref;
        }
        internal virtual SqlExpression VisitSimpleExpression(SqlSimpleExpression simple) {
            simple.Expression = this.VisitExpression(simple.Expression);
            return simple;
        }
        internal virtual SqlExpression VisitGrouping(SqlGrouping g) {
            g.Key = this.VisitExpression(g.Key);
            g.Group = this.VisitExpression(g.Group);
            return g;
        }
        internal virtual SqlExpression VisitDiscriminatedType(SqlDiscriminatedType dt) {
            dt.Discriminator = this.VisitExpression(dt.Discriminator);
            return dt;
        }
        internal virtual SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof) {
            dof.Object = this.VisitExpression(dof.Object);
            return dof;
        }
        internal virtual SqlNode VisitIncludeScope(SqlIncludeScope node) {
            node.Child = this.Visit(node.Child);
            return node;
        }

#if DEBUG
        int refersDepth;
#endif
        internal bool RefersToColumn(SqlExpression exp, SqlColumn col) {
#if DEBUG
            try {
                refersDepth++;
                System.Diagnostics.Debug.Assert(refersDepth < 20);
#endif
                if (exp != null) {
                    switch (exp.NodeType) {
                        case SqlNodeType.Column:
                            return exp == col || this.RefersToColumn(((SqlColumn)exp).Expression, col);
                        case SqlNodeType.ColumnRef:
                            SqlColumnRef cref = (SqlColumnRef)exp;
                            return cref.Column == col || this.RefersToColumn(cref.Column.Expression, col);
                        case SqlNodeType.ExprSet:
                            SqlExprSet set = (SqlExprSet)exp;
                            for (int i = 0, n = set.Expressions.Count; i < n; i++) {
                                if (this.RefersToColumn(set.Expressions[i], col)) {
                                    return true;
                                }
                            }
                            break;
                        case SqlNodeType.OuterJoinedValue:
                            return this.RefersToColumn(((SqlUnary)exp).Operand, col);
                    }
                }

                return false;
#if DEBUG
            }
            finally {
                refersDepth--;
            }
#endif
        }
    }
}
