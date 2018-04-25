using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// Validates the integrity of super-SQL trees.
    /// </summary>
    internal class SqlSupersetValidator {

        List<SqlVisitor> validators = new List<SqlVisitor>();

        /// <summary>
        /// Add a validator to the collection of validators to run.
        /// </summary>
        internal void AddValidator(SqlVisitor validator) {
            this.validators.Add(validator);
        }

        /// <summary>
        /// Execute each current validator.
        /// </summary>
        internal void Validate(SqlNode node) {
            foreach (SqlVisitor validator in this.validators) {
                validator.Visit(node);
            }
        }
    }

    /// <summary>
    /// Column ClrType must agree with the expression that it points to.
    /// </summary>
    internal class ColumnTypeValidator : SqlVisitor {

        internal override SqlRow VisitRow(SqlRow row) {
            for (int i = 0, n = row.Columns.Count; i < n; i++) {
                SqlColumn col = row.Columns[i];
                SqlExpression expr = this.VisitExpression(col.Expression);
                if (expr != null) {
                    if (TypeSystem.GetNonNullableType(col.ClrType) != TypeSystem.GetNonNullableType(expr.ClrType)) {
                        throw Error.ColumnClrTypeDoesNotAgreeWithExpressionsClrType();
                    }
                }
            }
            return row;
        }
    }

    /// <summary>
    /// A validator that ensures literal values are reasonable.
    /// </summary>
    internal class LiteralValidator : SqlVisitor {

        internal override SqlExpression VisitValue(SqlValue value) {
            if (!value.IsClientSpecified 
                && value.ClrType.IsClass 
                && value.ClrType != typeof(string) 
                && value.ClrType != typeof(Type) 
                && value.Value != null) {
                throw Error.ClassLiteralsNotAllowed(value.ClrType);
            }
            return value;
        }

        internal override SqlExpression VisitBinaryOperator(SqlBinary bo) {
            bo.Left = this.VisitExpression(bo.Left);
            return bo;
        }
    }

    /// <summary>
    /// A validator which enforces rationalized boolean expressions.
    /// </summary>
    internal class ExpectRationalizedBooleans : SqlBooleanMismatchVisitor {

        internal ExpectRationalizedBooleans() {
        }

        internal override SqlExpression ConvertValueToPredicate(SqlExpression bitExpression) {
            throw Error.ExpectedPredicateFoundBit();
        }

        internal override SqlExpression ConvertPredicateToValue(SqlExpression predicateExpression) {
            throw Error.ExpectedBitFoundPredicate();
        }
    }

    /// <summary>
    /// A validator which enforces that no more SqlMethodCall nodes exist in the tree.
    /// </summary>
    internal class ExpectNoMethodCalls : SqlVisitor {

        internal override SqlExpression VisitMethodCall(SqlMethodCall mc) {
            // eventually we may support this type of stuff given the SQL CLR, but for now it is illegal
            throw Error.MethodHasNoSupportConversionToSql(mc.Method.Name);
        }

        // check everything except selection expressions (which will be client or ignored)
        internal override SqlSelect VisitSelect(SqlSelect select) {
            return this.VisitSelectCore(select);
        }
    }

    internal class ExpectNoFloatingColumns : SqlVisitor {
        internal override SqlRow VisitRow(SqlRow row) {
            foreach (SqlColumn c in row.Columns) {
                this.Visit(c.Expression);
            }
            return row;
        }
        internal override SqlTable VisitTable(SqlTable tab) {
            foreach (SqlColumn c in tab.Columns) {
                this.Visit(c.Expression);
            }
            return tab;
        }
        internal override SqlExpression VisitColumn(SqlColumn col) {
            throw Error.UnexpectedFloatingColumn();
        }
    }

    internal class ExpectNoAliasRefs : SqlVisitor {
        internal override SqlExpression VisitAliasRef(SqlAliasRef aref) {
            throw Error.UnexpectedNode(aref.NodeType);
        }
    }

    internal class ExpectNoSharedExpressions : SqlVisitor {
        internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared) {
            throw Error.UnexpectedSharedExpression();
        }
        internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref) {
            throw Error.UnexpectedSharedExpressionReference();
        }
    }

    /// <summary>
    /// Determines if there is a boolean NText/Text/Image comparison and if so throws an exception
    /// because this is not valid in SQLServer.
    /// </summary>
    internal class ValidateNoInvalidComparison : SqlVisitor {
  
        internal override SqlExpression VisitBinaryOperator(SqlBinary bo) {
            if (bo.NodeType == SqlNodeType.EQ || bo.NodeType == SqlNodeType.NE ||
                bo.NodeType == SqlNodeType.EQ2V || bo.NodeType == SqlNodeType.NE2V ||
                bo.NodeType == SqlNodeType.GT || bo.NodeType == SqlNodeType.GE ||
                bo.NodeType == SqlNodeType.LT || bo.NodeType == SqlNodeType.LE ) {
                if (!bo.Left.SqlType.SupportsComparison ||
                    !bo.Right.SqlType.SupportsComparison){
                    throw Error.UnhandledStringTypeComparison();
                }
            }
            bo.Left = this.VisitExpression(bo.Left);
            bo.Right = this.VisitExpression(bo.Right);
            return bo;
        }

    }
}
