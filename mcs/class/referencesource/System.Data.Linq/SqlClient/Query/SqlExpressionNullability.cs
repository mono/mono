using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Provider;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {

    internal static class SqlExpressionNullability {

        /// <summary>
        /// Determines whether the given expression may return a null result.
        /// </summary>
        /// <param name="expr">The expression to check.</param>
        /// <returns>null means that it couldn't be determined</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal static bool? CanBeNull(SqlExpression expr) {
            switch (expr.NodeType) {
                case SqlNodeType.ExprSet:
                    SqlExprSet exprSet = (SqlExprSet)expr;
                    return CanBeNull(exprSet.Expressions);

                case SqlNodeType.SimpleCase:                  
                    SqlSimpleCase sc = (SqlSimpleCase)expr;
                    return CanBeNull(sc.Whens.Select(w => w.Value));

                case SqlNodeType.Column:
                    SqlColumn col = (SqlColumn)expr;
                    if (col.MetaMember != null) {
                        return col.MetaMember.CanBeNull;
                    }
                    else if (col.Expression != null) {
                        return CanBeNull(col.Expression);
                    }
                    return null;  // Don't know.

                case SqlNodeType.ColumnRef:
                    SqlColumnRef cref = (SqlColumnRef)expr;
                    return CanBeNull(cref.Column);

                case SqlNodeType.Value:
                    return ((SqlValue)expr).Value == null;

                case SqlNodeType.New:
                case SqlNodeType.Multiset:
                case SqlNodeType.Grouping:
                case SqlNodeType.DiscriminatedType:
                case SqlNodeType.IsNotNull: // IsNull\IsNotNull always return true or false and can never return NULL.
                case SqlNodeType.IsNull:
                case SqlNodeType.Exists:
                    return false;

                case SqlNodeType.Add:
                case SqlNodeType.Sub:
                case SqlNodeType.Mul:
                case SqlNodeType.Div:
                case SqlNodeType.Mod:
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                case SqlNodeType.Concat: {
                    SqlBinary bop = (SqlBinary)expr;
                    bool? left = CanBeNull(bop.Left);
                    bool? right = CanBeNull(bop.Right);
                    return (left != false) || (right != false);
                }

                case SqlNodeType.Negate:
                case SqlNodeType.BitNot: {
                    SqlUnary uop = (SqlUnary)expr;
                    return CanBeNull(uop.Operand);
                }

                case SqlNodeType.Lift: {
                    SqlLift lift = (SqlLift)expr;
                    return CanBeNull(lift.Expression);
                }

                case SqlNodeType.OuterJoinedValue:
                    return true;

                default: 
                    return null; // Don't know.
            }
        }

        /// <summary>
        /// Used to determine nullability for a collection of expressions.
        ///   * If at least one of the expressions is nullable, the collection is nullable.
        ///   * If no expressions are nullable, but at least one is 'don't know', the collection is 'don't know'.
        ///   * Otherwise all expressions are non-nullable and the nullability is false.
        /// </summary>
        private static bool? CanBeNull(IEnumerable<SqlExpression> exprs) {
            bool hasAtleastOneUnknown = false;
            foreach(SqlExpression e in exprs) {
                bool? nullability = CanBeNull(e);

                // Even one expression that could return null means the
                // collection can return null.
                if (nullability == true) 
                    return true;

                // If there is one or more 'unknown' and no definitely nullable
                // results then the collection nullability is 'unknown'.
                if (nullability == null)
                    hasAtleastOneUnknown = true;
            }
            if (hasAtleastOneUnknown)
                return null;

            return false;
        }
    }
}
