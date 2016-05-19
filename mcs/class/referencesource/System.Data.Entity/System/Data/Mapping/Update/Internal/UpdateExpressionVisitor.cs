//---------------------------------------------------------------------
// <copyright file="UpdateExpressionVisitor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Mapping.Update.Internal
{
    using System.Data.Common.CommandTrees;

    /// <summary>
    /// Abstract implementation of node visitor that allows the specification of visit methods
    /// for different node types (VisitPre virtual methods) and evaluation of nodes with respect
    /// to the typed (TReturn) return values of their children.
    /// </summary>
    /// <remarks>
    /// This is not a general purpose class. It is tailored to the needs of the update pipeline.
    /// 
    /// All virtual methods throw NotSupportedException (must be explicitly overridden by each visitor).
    /// </remarks>
    /// <typeparam name="TReturn">Return type for the visitor</typeparam>
    internal abstract class UpdateExpressionVisitor<TReturn> : DbExpressionVisitor<TReturn>
    {
        /// <summary>
        /// Gets the name of this visitor for debugging and tracing purposes.
        /// </summary>
        protected abstract string VisitorName
        {
            get;
        }

        /// <summary>
        /// Utility method to generate an exception when unsupported node types are encountered.
        /// </summary>
        /// <param name="node">Unsupported node</param>
        /// <returns>Not supported exception</returns>
        protected NotSupportedException ConstructNotSupportedException(DbExpression node)
        {
            string nodeKind = null == node ? null :
                node.ExpressionKind.ToString();

            return EntityUtil.NotSupported(
                System.Data.Entity.Strings.Update_UnsupportedExpressionKind(nodeKind, VisitorName));
        }

        #region IExpressionVisitor<TReturn> Members
        public override TReturn Visit(DbExpression expression)
        {
            if (null != expression)
            {
                return expression.Accept(this);
            }
            else
            {
                throw ConstructNotSupportedException(expression);
            }
        }

        public override TReturn Visit(DbAndExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbApplyExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbArithmeticExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbCaseExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbCastExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbComparisonExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbConstantExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbCrossJoinExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbDerefExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbDistinctExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbElementExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbExceptExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbFilterExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbFunctionExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbLambdaExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbEntityRefExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbRefKeyExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbGroupByExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbIntersectExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbIsEmptyExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbIsNullExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbIsOfExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbJoinExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbLikeExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbLimitExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

#if METHOD_EXPRESSION
        public override TReturn Visit(MethodExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }
#endif

        public override TReturn Visit(DbNewInstanceExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbNotExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbNullExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbOfTypeExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbOrExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbParameterReferenceExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbProjectExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbPropertyExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbQuantifierExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbRefExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbRelationshipNavigationExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbSkipExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbSortExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbTreatExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbUnionAllExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbVariableReferenceExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }

        public override TReturn Visit(DbScanExpression expression)
        {
            throw ConstructNotSupportedException(expression);
        }
        #endregion
    }
}
