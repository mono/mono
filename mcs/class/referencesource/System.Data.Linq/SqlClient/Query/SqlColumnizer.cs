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
using System.Diagnostics;

namespace System.Data.Linq.SqlClient {

    // partions select expressions and common subexpressions into scalar and non-scalar pieces by 
    // wrapping scalar pieces floating column nodes.
    internal class SqlColumnizer {
        ColumnNominator nominator;
        ColumnDeclarer declarer;

        internal SqlColumnizer() {
            this.nominator = new ColumnNominator();
            this.declarer = new ColumnDeclarer();
        }

        internal SqlExpression ColumnizeSelection(SqlExpression selection) {
            return this.declarer.Declare(selection, this.nominator.Nominate(selection));
        }

        internal static bool CanBeColumn(SqlExpression expression) {
            return ColumnNominator.CanBeColumn(expression);
        }

        class ColumnDeclarer : SqlVisitor {
            HashSet<SqlExpression> candidates;

            internal ColumnDeclarer() {
            }

            internal SqlExpression Declare(SqlExpression expression, HashSet<SqlExpression> candidates) {
                this.candidates = candidates;
                return (SqlExpression)this.Visit(expression);
            }

            internal override SqlNode Visit(SqlNode node) {
                SqlExpression expr = node as SqlExpression;
                if (expr != null) {
                    if (this.candidates.Contains(expr)) {
                        if (expr.NodeType == SqlNodeType.Column ||
                            expr.NodeType == SqlNodeType.ColumnRef) {
                            return expr;
                        }
                        else {
                            return new SqlColumn(expr.ClrType, expr.SqlType, null, null, expr, expr.SourceExpression);
                        }
                    }
                }
                return base.Visit(node);
            }
        }

        class ColumnNominator : SqlVisitor {
            bool isBlocked;
            HashSet<SqlExpression> candidates;

            internal HashSet<SqlExpression> Nominate(SqlExpression expression) {
                this.candidates = new HashSet<SqlExpression>();
                this.isBlocked = false;
                this.Visit(expression);
                return this.candidates;
            }

            internal override SqlNode Visit(SqlNode node) {
                SqlExpression expression = node as SqlExpression;
                if (expression != null) {
                    bool saveIsBlocked = this.isBlocked;
                    this.isBlocked = false;
                    if (CanRecurseColumnize(expression)) {
                        base.Visit(expression);
                    }
                    if (!this.isBlocked) {
                        if (CanBeColumn(expression)) {
                            this.candidates.Add(expression);
                        }
                        else {
                            this.isBlocked = true;
                        }
                    }
                    this.isBlocked |= saveIsBlocked;
                }
                return node;
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c) {
                c.Expression = this.VisitExpression(c.Expression);
                for (int i = 0, n = c.Whens.Count; i < n; i++) {
                    // Don't walk down the match side. This can't be a column.
                    c.Whens[i].Value = this.VisitExpression(c.Whens[i].Value);
                }
                return c;
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase tc) {
                tc.Discriminator = this.VisitExpression(tc.Discriminator);
                for (int i = 0, n = tc.Whens.Count; i < n; i++) {
                    // Don't walk down the match side. This can't be a column.
                    tc.Whens[i].TypeBinding = this.VisitExpression(tc.Whens[i].TypeBinding);
                }
                return tc;
            }

            internal override SqlExpression VisitClientCase(SqlClientCase c) {
                c.Expression = this.VisitExpression(c.Expression);
                for (int i = 0, n = c.Whens.Count; i < n; i++) {
                    // Don't walk down the match side. This can't be a column.
                    c.Whens[i].Value = this.VisitExpression(c.Whens[i].Value);
                }
                return c;
            }

            private static bool CanRecurseColumnize(SqlExpression expr) {
                switch (expr.NodeType) {
                    case SqlNodeType.AliasRef:
                    case SqlNodeType.ColumnRef:
                    case SqlNodeType.Column:
                    case SqlNodeType.Multiset:
                    case SqlNodeType.Element:
                    case SqlNodeType.ScalarSubSelect:
                    case SqlNodeType.Exists:
                    case SqlNodeType.ClientQuery:
                    case SqlNodeType.SharedExpressionRef:
                    case SqlNodeType.Link:
                    case SqlNodeType.Nop:
                    case SqlNodeType.Value:
                    case SqlNodeType.Select:
                        return false;
                    default:
                        return true;
                }
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
            private static bool IsClientOnly(SqlExpression expr) {
                switch (expr.NodeType) {
                    case SqlNodeType.ClientCase:
                    case SqlNodeType.TypeCase:
                    case SqlNodeType.ClientArray:
                    case SqlNodeType.Grouping:
                    case SqlNodeType.DiscriminatedType:
                    case SqlNodeType.SharedExpression:
                    case SqlNodeType.SimpleExpression:
                    case SqlNodeType.AliasRef:
                    case SqlNodeType.Multiset:
                    case SqlNodeType.Element:
                    case SqlNodeType.ClientQuery:
                    case SqlNodeType.SharedExpressionRef:
                    case SqlNodeType.Link:
                    case SqlNodeType.Nop:
                        return true;
                    case SqlNodeType.OuterJoinedValue:
                        return IsClientOnly(((SqlUnary)expr).Operand);
                    default:
                        return false;
                }
            }

            internal static bool CanBeColumn(SqlExpression expression) {
                if (!IsClientOnly(expression)
                    && expression.NodeType != SqlNodeType.Column                            
                    && expression.SqlType.CanBeColumn) {

                    switch (expression.NodeType) {
                        case SqlNodeType.MethodCall:
                        case SqlNodeType.Member:
                        case SqlNodeType.New:
                            return PostBindDotNetConverter.CanConvert(expression);
                        default:
                            return true;
                    }
                }
                return false;
            }
        }
    }
}
