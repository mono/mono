using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient {
    internal static class SqlNodeTypeOperators {

        /// <summary>
        /// Determines whether the given unary operator node type returns a value that 
        /// is predicate.
        /// </summary> 
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal static bool IsPredicateUnaryOperator(this SqlNodeType nodeType) {
            switch (nodeType) {
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                case SqlNodeType.IsNull:
                case SqlNodeType.IsNotNull:
                    return true;
                case SqlNodeType.Negate:
                case SqlNodeType.BitNot:
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
                    return false;
                default:
                    throw Error.UnexpectedNode(nodeType);
            }
        }

        /// <summary>
        /// Determines whether the given unary operator expects a predicate as input.
        /// </summary> 
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal static bool IsUnaryOperatorExpectingPredicateOperand(this SqlNodeType nodeType) {
            switch (nodeType) {
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                    return true;
                case SqlNodeType.IsNull:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.Negate:
                case SqlNodeType.BitNot:
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
                    return false;
                default:
                    throw Error.UnexpectedNode(nodeType);
            }
        }

        /// <summary>
        /// Determines whether the given binary operator node type returns a value that 
        /// is a predicate boolean.
        /// </summary> 
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal static bool IsPredicateBinaryOperator(this SqlNodeType nodeType) {
            switch (nodeType) {
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.LE:
                case SqlNodeType.LT:
                case SqlNodeType.EQ:
                case SqlNodeType.NE:
                case SqlNodeType.EQ2V:
                case SqlNodeType.NE2V:
                case SqlNodeType.And:
                case SqlNodeType.Or:
                    return true;
                case SqlNodeType.Add:
                case SqlNodeType.Sub:
                case SqlNodeType.Mul:
                case SqlNodeType.Div:
                case SqlNodeType.Mod:
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                case SqlNodeType.Concat:
                case SqlNodeType.Coalesce:
                    return false;
                default:
                    throw Error.UnexpectedNode(nodeType);
            }
        }
        /// <summary>
        /// Determines whether this operator is a binary comparison operator (i.e. >, =>, ==, etc)
        /// </summary> 
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal static bool IsComparisonOperator(this SqlNodeType nodeType)
        {
            switch (nodeType)
            {
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.LE:
                case SqlNodeType.LT:
                case SqlNodeType.EQ:
                case SqlNodeType.NE:
                case SqlNodeType.EQ2V:
                case SqlNodeType.NE2V:
                    return true;
                case SqlNodeType.And:
                case SqlNodeType.Or:
                case SqlNodeType.Add:
                case SqlNodeType.Sub:
                case SqlNodeType.Mul:
                case SqlNodeType.Div:
                case SqlNodeType.Mod:
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                case SqlNodeType.Concat:
                case SqlNodeType.Coalesce:
                    return false;
                default:
                    throw Error.UnexpectedNode(nodeType);
            }
        }

        /// <summary>
        /// Determines whether the given binary operator node type returns a value that 
        /// is a predicate boolean.
        /// </summary> 
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        internal static bool IsBinaryOperatorExpectingPredicateOperands(this SqlNodeType nodeType) {
            switch (nodeType) {
                case SqlNodeType.And:
                case SqlNodeType.Or:
                    return true;
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.LE:
                case SqlNodeType.LT:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.Add:
                case SqlNodeType.Sub:
                case SqlNodeType.Mul:
                case SqlNodeType.Div:
                case SqlNodeType.Mod:
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                case SqlNodeType.Concat:
                case SqlNodeType.Coalesce:
                    return false;
                default:
                    throw Error.UnexpectedNode(nodeType);
            }
        }

        /// <summary>
        /// Determines whether the given node requires support on the client for evaluation.
        /// For example, LINK nodes may be delay-executed only when the user requests the result.
        /// </summary>
        internal static bool IsClientAidedExpression(this SqlExpression expr) {
            switch (expr.NodeType) {
                case SqlNodeType.Link:
                case SqlNodeType.Element:
                case SqlNodeType.Multiset:
                case SqlNodeType.ClientQuery:
                case SqlNodeType.TypeCase:
                case SqlNodeType.New:
                    return true;
                default:
                    return false;
            };                              
        }
    }
}
