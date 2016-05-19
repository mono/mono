using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This visitor searches for places where 'Predicate' is found but a 'Bit'
    /// was expected or vice versa. In response, it will call VisitBitExpectedPredicate
    /// and VisitPredicateExpectedBit.
    /// </summary>
    internal abstract class SqlBooleanMismatchVisitor : SqlVisitor {

        internal SqlBooleanMismatchVisitor() {
        }

        internal abstract SqlExpression ConvertValueToPredicate(SqlExpression valueExpression);
        internal abstract SqlExpression ConvertPredicateToValue(SqlExpression predicateExpression);

        internal override SqlSelect VisitSelect(SqlSelect select) {
            select.From = this.VisitSource(select.From);
            select.Where = this.VisitPredicate(select.Where);
            for (int i = 0, n = select.GroupBy.Count; i < n; i++) {
                select.GroupBy[i] = this.VisitExpression(select.GroupBy[i]);
            }
            select.Having = this.VisitPredicate(select.Having);
            for (int i = 0, n = select.OrderBy.Count; i < n; i++) {
                select.OrderBy[i].Expression = this.VisitExpression(select.OrderBy[i].Expression);
            }
            select.Top = this.VisitExpression(select.Top);
            select.Row = (SqlRow)this.Visit(select.Row);

            // don't visit selection
            //select.Selection = this.VisitExpression(select.Selection);

            return select;
        }

        internal override SqlSource VisitJoin(SqlJoin join) {
            join.Left = this.VisitSource(join.Left);
            join.Right = this.VisitSource(join.Right);
            join.Condition = this.VisitPredicate(join.Condition);
            return join;
        }

        internal override SqlExpression VisitUnaryOperator(SqlUnary uo) {
            if (uo.NodeType.IsUnaryOperatorExpectingPredicateOperand()) {
                uo.Operand = this.VisitPredicate(uo.Operand);
            } else {
                uo.Operand = this.VisitExpression(uo.Operand);
            }
            return uo;
        }

        internal override SqlExpression VisitBinaryOperator(SqlBinary bo) {
            if (bo.NodeType.IsBinaryOperatorExpectingPredicateOperands()) {
                bo.Left = this.VisitPredicate(bo.Left);
                bo.Right = this.VisitPredicate(bo.Right);
            } else {
                bo.Left = this.VisitExpression(bo.Left);
                bo.Right = this.VisitExpression(bo.Right);
            }

            return bo;
        }

        internal override SqlStatement VisitAssign(SqlAssign sa) {
            // L-Value of assign is never a 'Bit' nor a 'Predicate'.
            sa.LValue = this.VisitExpression(sa.LValue);
            sa.RValue = this.VisitExpression(sa.RValue);
            return sa;
        }

        internal override SqlExpression VisitSearchedCase(SqlSearchedCase c) {
            for (int i = 0, n = c.Whens.Count; i < n; i++) {
                SqlWhen when = c.Whens[i];
                when.Match = this.VisitPredicate(when.Match);
                when.Value = this.VisitExpression(when.Value);
            }
            c.Else = this.VisitExpression(c.Else);
            return c;
        }

        internal override SqlExpression VisitLift(SqlLift lift) {
            lift.Expression = base.VisitExpression(lift.Expression);
            return lift;
        }

        /// <summary>
        /// If an expression is type 'Bit' but a 'Predicate' is expected then 
        /// call 'VisitBitExpectedPredicate'.
        /// </summary>
        internal SqlExpression VisitPredicate(SqlExpression exp) {
            exp = (SqlExpression)base.Visit(exp);
            if (exp != null) {
                if (!IsPredicateExpression(exp)) {
                    exp = ConvertValueToPredicate(exp);
                }
            }
            return exp;
        }

        /// <summary>
        /// Any remaining calls to VisitExpression expect a 'Bit' when there's
        /// a boolean expression.
        /// </summary>
        internal override SqlExpression VisitExpression(SqlExpression exp) {
            exp = (SqlExpression)base.Visit(exp);
            if (exp != null) {
                if (IsPredicateExpression(exp)) {
                    exp = ConvertPredicateToValue(exp);
                }
            }
            return exp;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        private static bool IsPredicateExpression(SqlExpression exp) {
            switch (exp.NodeType) {
                case SqlNodeType.And:
                case SqlNodeType.Or:
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.LE:
                case SqlNodeType.LT:
                case SqlNodeType.Exists:
                case SqlNodeType.Between:
                case SqlNodeType.In:
                case SqlNodeType.Like:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.IsNull:
                    return true;
                case SqlNodeType.Lift:
                    return IsPredicateExpression(((SqlLift)exp).Expression);
                default:
                    return false;
            }
        }
    }
}
