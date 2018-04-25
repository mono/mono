using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.Linq.Provider;
using System.Linq;
using System.Data.Linq.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// Determines whether an expression is simple or not.  
    /// Simple is a scalar expression that contains only functions, operators and column references
    /// </summary>
    internal static class SimpleExpression {
        internal static bool IsSimple(SqlExpression expr) {
            Visitor v = new Visitor();
            v.Visit(expr);
            return v.IsSimple;
        }

        class Visitor : SqlVisitor {
            bool isSimple = true;

            internal bool IsSimple {
                get { return this.isSimple; }
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            internal override SqlNode Visit(SqlNode node) {
                if (node == null) {
                    return null;
                }
                if (!this.isSimple) {
                    return node;
                }
                switch (node.NodeType) {
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                    case SqlNodeType.Negate:
                    case SqlNodeType.BitNot:
                    case SqlNodeType.IsNull:
                    case SqlNodeType.IsNotNull:
                    case SqlNodeType.ValueOf:
                    case SqlNodeType.OuterJoinedValue:
                    case SqlNodeType.ClrLength:
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
                    case SqlNodeType.Between:
                    case SqlNodeType.Concat:
                    case SqlNodeType.Convert:
                    case SqlNodeType.Treat:
                    case SqlNodeType.Member:
                    case SqlNodeType.TypeCase:
                    case SqlNodeType.SearchedCase:
                    case SqlNodeType.SimpleCase:
                    case SqlNodeType.Like:
                    case SqlNodeType.FunctionCall:
                    case SqlNodeType.ExprSet:
                    case SqlNodeType.OptionalValue:
                    case SqlNodeType.Parameter:
                    case SqlNodeType.ColumnRef:
                    case SqlNodeType.Value:
                    case SqlNodeType.Variable:
                        return base.Visit(node);
                    case SqlNodeType.Column:
                    case SqlNodeType.ClientCase:
                    case SqlNodeType.DiscriminatedType:
                    case SqlNodeType.Link:
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
                        this.isSimple = false;
                        return node;
                }
            }
        }
    }
}
