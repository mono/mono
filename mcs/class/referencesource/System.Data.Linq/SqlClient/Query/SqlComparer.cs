using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// Compare two trees for value equality. Implemented as a parallel visitor.
    /// </summary>
    internal class SqlComparer {

        internal SqlComparer() {
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "[....]: Cast is dependent on node type and casts do not happen unecessarily in a single code path.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal static bool AreEqual(SqlNode node1, SqlNode node2) {
            if (node1 == node2)
                return true;
            if (node1 == null || node2 == null)
                return false;           

            if (node1.NodeType == SqlNodeType.SimpleCase)
                node1 = UnwrapTrivialCaseExpression((SqlSimpleCase)node1);

            if (node2.NodeType == SqlNodeType.SimpleCase)
                node2 = UnwrapTrivialCaseExpression((SqlSimpleCase)node2);

            if (node1.NodeType != node2.NodeType) {
                // allow expression sets to compare against single expressions
                if (node1.NodeType == SqlNodeType.ExprSet) {
                    SqlExprSet eset = (SqlExprSet)node1;
                    for (int i = 0, n = eset.Expressions.Count; i < n; i++) {
                        if (AreEqual(eset.Expressions[i], node2))
                            return true;
                    }
                }
                else if (node2.NodeType == SqlNodeType.ExprSet) {
                    SqlExprSet eset = (SqlExprSet)node2;
                    for (int i = 0, n = eset.Expressions.Count; i < n; i++) {
                        if (AreEqual(node1, eset.Expressions[i]))
                            return true;
                    }
                }
                return false;
            }
            if (node1.Equals(node2))
                return true;

            switch (node1.NodeType) {
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                case SqlNodeType.Negate:
                case SqlNodeType.BitNot:
                case SqlNodeType.IsNull:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.Count:
                case SqlNodeType.Max:
                case SqlNodeType.Min:
                case SqlNodeType.Sum:
                case SqlNodeType.Avg:
                case SqlNodeType.Stddev:
                case SqlNodeType.ValueOf:
                case SqlNodeType.OuterJoinedValue:
                case SqlNodeType.ClrLength:
                    return AreEqual(((SqlUnary)node1).Operand, ((SqlUnary)node2).Operand);
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
                    SqlBinary firstNode = (SqlBinary)node1;
                    SqlBinary secondNode = (SqlBinary)node2;
                    return AreEqual(firstNode.Left, secondNode.Left)
                        && AreEqual(firstNode.Right, secondNode.Right);
                case SqlNodeType.Convert:
                case SqlNodeType.Treat: {
                        SqlUnary sun1 = (SqlUnary)node1;
                        SqlUnary sun2 = (SqlUnary)node2;
                        return sun1.ClrType == sun2.ClrType && sun1.SqlType == sun2.SqlType && AreEqual(sun1.Operand, sun2.Operand);
                    }
                case SqlNodeType.Between: {
                        SqlBetween b1 = (SqlBetween)node1;
                        SqlBetween b2 = (SqlBetween)node1;
                        return AreEqual(b1.Expression, b2.Expression) &&
                               AreEqual(b1.Start, b2.Start) &&
                               AreEqual(b1.End, b2.End);
                    }
                case SqlNodeType.Parameter:
                    return node1 == node2;
                case SqlNodeType.Alias:
                    return AreEqual(((SqlAlias)node1).Node, ((SqlAlias)node2).Node);
                case SqlNodeType.AliasRef:
                    return AreEqual(((SqlAliasRef)node1).Alias, ((SqlAliasRef)node2).Alias);
                case SqlNodeType.Column:
                    SqlColumn col1 = (SqlColumn)node1;
                    SqlColumn col2 = (SqlColumn)node2;
                    return col1 == col2 || (col1.Expression != null && col2.Expression != null && AreEqual(col1.Expression, col2.Expression));
                case SqlNodeType.Table:
                    return ((SqlTable)node1).MetaTable == ((SqlTable)node2).MetaTable;
                case SqlNodeType.Member:
                    return (((SqlMember)node1).Member == ((SqlMember)node2).Member) &&
                           AreEqual(((SqlMember)node1).Expression, ((SqlMember)node2).Expression);
                case SqlNodeType.ColumnRef:
                    SqlColumnRef cref1 = (SqlColumnRef)node1;
                    SqlColumnRef cref2 = (SqlColumnRef)node2;
                    return GetBaseColumn(cref1) == GetBaseColumn(cref2);
                case SqlNodeType.Value:
                    return Object.Equals(((SqlValue)node1).Value, ((SqlValue)node2).Value);
                case SqlNodeType.TypeCase: {
                        SqlTypeCase c1 = (SqlTypeCase)node1;
                        SqlTypeCase c2 = (SqlTypeCase)node2;
                        if (!AreEqual(c1.Discriminator, c2.Discriminator)) {
                            return false;
                        }
                        if (c1.Whens.Count != c2.Whens.Count) {
                            return false;
                        }
                        for (int i = 0, c = c1.Whens.Count; i < c; ++i) {
                            if (!AreEqual(c1.Whens[i].Match, c2.Whens[i].Match)) {
                                return false;
                            }
                            if (!AreEqual(c1.Whens[i].TypeBinding, c2.Whens[i].TypeBinding)) {
                                return false;
                            }
                        }

                        return true;
                    }
                case SqlNodeType.SearchedCase: {
                        SqlSearchedCase c1 = (SqlSearchedCase)node1;
                        SqlSearchedCase c2 = (SqlSearchedCase)node2;
                        if (c1.Whens.Count != c2.Whens.Count)
                            return false;
                        for (int i = 0, n = c1.Whens.Count; i < n; i++) {
                            if (!AreEqual(c1.Whens[i].Match, c2.Whens[i].Match) ||
                                !AreEqual(c1.Whens[i].Value, c2.Whens[i].Value))
                                return false;
                        }
                        return AreEqual(c1.Else, c2.Else);
                    }
                case SqlNodeType.ClientCase: {
                        SqlClientCase c1 = (SqlClientCase)node1;
                        SqlClientCase c2 = (SqlClientCase)node2;
                        if (c1.Whens.Count != c2.Whens.Count)
                            return false;
                        for (int i = 0, n = c1.Whens.Count; i < n; i++) {
                            if (!AreEqual(c1.Whens[i].Match, c2.Whens[i].Match) ||
                                !AreEqual(c1.Whens[i].Value, c2.Whens[i].Value))
                                return false;
                        }
                        return true;
                    }
                case SqlNodeType.DiscriminatedType: {
                    SqlDiscriminatedType dt1 = (SqlDiscriminatedType)node1;
                    SqlDiscriminatedType dt2 = (SqlDiscriminatedType)node2;
                    return AreEqual(dt1.Discriminator, dt2.Discriminator);
                }
                case SqlNodeType.SimpleCase: {
                        SqlSimpleCase c1 = (SqlSimpleCase)node1;
                        SqlSimpleCase c2 = (SqlSimpleCase)node2;
                        if (c1.Whens.Count != c2.Whens.Count)
                            return false;
                        for (int i = 0, n = c1.Whens.Count; i < n; i++) {
                            if (!AreEqual(c1.Whens[i].Match, c2.Whens[i].Match) ||
                                !AreEqual(c1.Whens[i].Value, c2.Whens[i].Value))
                                return false;
                        }
                        return true;
                    }
                case SqlNodeType.Like: {
                        SqlLike like1 = (SqlLike)node1;
                        SqlLike like2 = (SqlLike)node2;
                        return AreEqual(like1.Expression, like2.Expression) &&
                               AreEqual(like1.Pattern, like2.Pattern) &&
                               AreEqual(like1.Escape, like2.Escape);
                    }
                case SqlNodeType.Variable: {
                        SqlVariable v1 = (SqlVariable)node1;
                        SqlVariable v2 = (SqlVariable)node2;
                        return v1.Name == v2.Name;
                    }
                case SqlNodeType.FunctionCall: {
                        SqlFunctionCall f1 = (SqlFunctionCall)node1;
                        SqlFunctionCall f2 = (SqlFunctionCall)node2;
                        if (f1.Name != f2.Name)
                            return false;
                        if (f1.Arguments.Count != f2.Arguments.Count)
                            return false;
                        for (int i = 0, n = f1.Arguments.Count; i < n; i++) {
                            if (!AreEqual(f1.Arguments[i], f2.Arguments[i]))
                                return false;
                        }
                        return true;
                    }
                case SqlNodeType.Link: {
                        SqlLink l1 = (SqlLink)node1;
                        SqlLink l2 = (SqlLink)node2;
                        if (!MetaPosition.AreSameMember(l1.Member.Member, l2.Member.Member)) {
                            return false;
                        }
                        if (!AreEqual(l1.Expansion, l2.Expansion)) {
                            return false;
                        }
                        if (l1.KeyExpressions.Count != l2.KeyExpressions.Count) {
                            return false;
                        }
                        for (int i = 0, c = l1.KeyExpressions.Count; i < c; ++i) {
                            if (!AreEqual(l1.KeyExpressions[i], l2.KeyExpressions[i])) {
                                return false;
                            }
                        }
                        return true;
                    }
                case SqlNodeType.ExprSet:
                    SqlExprSet es1 = (SqlExprSet)node1;
                    SqlExprSet es2 = (SqlExprSet)node2;
                    if (es1.Expressions.Count != es2.Expressions.Count)
                        return false;
                    for(int i = 0, n = es1.Expressions.Count; i < n; i++) {
                        if (!AreEqual(es1.Expressions[i], es2.Expressions[i]))
                            return false;
                    }
                    return true;
                case SqlNodeType.OptionalValue:
                    SqlOptionalValue ov1 = (SqlOptionalValue)node1;
                    SqlOptionalValue ov2 = (SqlOptionalValue)node2;
                    return AreEqual(ov1.Value, ov2.Value);
                case SqlNodeType.Row:
                case SqlNodeType.UserQuery:
                case SqlNodeType.StoredProcedureCall:
                case SqlNodeType.UserRow:
                case SqlNodeType.UserColumn:
                case SqlNodeType.Multiset:
                case SqlNodeType.ScalarSubSelect:
                case SqlNodeType.Element:
                case SqlNodeType.Exists:
                case SqlNodeType.Join:
                case SqlNodeType.Select:
                case SqlNodeType.New:
                case SqlNodeType.ClientQuery:
                case SqlNodeType.ClientArray:
                case SqlNodeType.Insert:
                case SqlNodeType.Update:
                case SqlNodeType.Delete:
                case SqlNodeType.MemberAssign:
                case SqlNodeType.Assign:
                case SqlNodeType.Block:
                case SqlNodeType.Union:
                case SqlNodeType.DoNotVisit:
                case SqlNodeType.MethodCall:
                case SqlNodeType.Nop:
                default:
                    return false;
            }
        }

        private static SqlColumn GetBaseColumn(SqlColumnRef cref) {
            while (cref != null && cref.Column.Expression != null) {
                SqlColumnRef cr = cref.Column.Expression as SqlColumnRef;
                if (cr != null) {
                    cref = cr;
                    continue;
                }
                else {
                    break;
                }
            }
            return cref.Column;
        }

        private static SqlExpression UnwrapTrivialCaseExpression(SqlSimpleCase sc) {
            if (sc.Whens.Count != 1) {
                return sc;
            }
            if (!SqlComparer.AreEqual(sc.Expression, sc.Whens[0].Match)) {
                return sc;
            }
            SqlExpression result = sc.Whens[0].Value;
            if (result.NodeType == SqlNodeType.SimpleCase) {
                return UnwrapTrivialCaseExpression((SqlSimpleCase)result);
            }
            return result;
        }
    }
}
