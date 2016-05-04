//---------------------------------------------------------------------
// <copyright file="DbExpressionVisitor_TResultType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Metadata.Edm;

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// The expression visitor pattern abstract base class that should be implemented by visitors that return a result value of a specific type.
    /// </summary>
    /// <typeparam name="TResultType">The type of the result value produced by the visitor.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbExpressionVisitor<TResultType>
    {
        /// <summary>
        /// Called when an expression of an otherwise unrecognized type is encountered.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public abstract TResultType Visit(DbExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbAndExpression.
        /// </summary>
        /// <param name="expression">The DbAndExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbAndExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbApplyExpression.
        /// </summary>
        /// <param name="expression">The DbApplyExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbApplyExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbArithmeticExpression.
        /// </summary>
        /// <param name="expression">The DbArithmeticExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbArithmeticExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbCaseExpression.
        /// </summary>
        /// <param name="expression">The DbCaseExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbCaseExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbCastExpression.
        /// </summary>
        /// <param name="expression">The DbCastExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbCastExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbComparisonExpression.
        /// </summary>
        /// <param name="expression">The DbComparisonExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbComparisonExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbConstantExpression.
        /// </summary>
        /// <param name="expression">The DbConstantExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbConstantExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbCrossJoinExpression.
        /// </summary>
        /// <param name="expression">The DbCrossJoinExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbCrossJoinExpression expression);

        /// <summary>
        /// Visitor pattern method for DbDerefExpression.
        /// </summary>
        /// <param name="expression">The DbDerefExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbDerefExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbDistinctExpression.
        /// </summary>
        /// <param name="expression">The DbDistinctExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbDistinctExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbElementExpression.
        /// </summary>
        /// <param name="expression">The DbElementExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbElementExpression expression);
        
        /// <summary>
        /// Typed visitor pattern method for DbExceptExpression.
        /// </summary>
        /// <param name="expression">The DbExceptExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbExceptExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbFilterExpression.
        /// </summary>
        /// <param name="expression">The DbFilterExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbFilterExpression expression);

        /// <summary>
        /// Visitor pattern method for DbFunctionExpression
        /// </summary>
        /// <param name="expression">The DbFunctionExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbFunctionExpression expression);

        /// <summary>
        /// Visitor pattern method for DbEntityRefExpression.
        /// </summary>
        /// <param name="expression">The DbEntityRefExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbEntityRefExpression expression);

        /// <summary>
        /// Visitor pattern method for DbRefKeyExpression.
        /// </summary>
        /// <param name="expression">The DbRefKeyExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbRefKeyExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbGroupByExpression.
        /// </summary>
        /// <param name="expression">The DbGroupByExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbGroupByExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbIntersectExpression.
        /// </summary>
        /// <param name="expression">The DbIntersectExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbIntersectExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbIsEmptyExpression.
        /// </summary>
        /// <param name="expression">The DbIsEmptyExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbIsEmptyExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbIsNullExpression.
        /// </summary>
        /// <param name="expression">The DbIsNullExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbIsNullExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbIsOfExpression.
        /// </summary>
        /// <param name="expression">The DbIsOfExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbIsOfExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbJoinExpression.
        /// </summary>
        /// <param name="expression">The DbJoinExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbJoinExpression expression);

        /// <summary>
        /// Visitor pattern method for DbLambdaExpression.
        /// </summary>
        /// <param name="expression">The DbLambdaExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public virtual TResultType Visit(DbLambdaExpression expression)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// Visitor pattern method for DbLikeExpression.
        /// </summary>
        /// <param name="expression">The DbLikeExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbLikeExpression expression);

        /// <summary>
        /// Visitor pattern method for DbLimitExpression.
        /// </summary>
        /// <param name="expression">The DbLimitExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbLimitExpression expression);

#if METHOD_EXPRESSION
        /// <summary>
        /// Typed visitor pattern method for MethodExpression.
        /// </summary>
        /// <param name="expression">The Expression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(MethodExpression expression);
#endif

        /// <summary>
        /// Typed visitor pattern method for DbNewInstanceExpression.
        /// </summary>
        /// <param name="expression">The DbNewInstanceExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbNewInstanceExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbNotExpression.
        /// </summary>
        /// <param name="expression">The DbNotExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbNotExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbNullExpression.
        /// </summary>
        /// <param name="expression">The DbNullExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbNullExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbOfTypeExpression.
        /// </summary>
        /// <param name="expression">The DbOfTypeExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbOfTypeExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbOrExpression.
        /// </summary>
        /// <param name="expression">The DbOrExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbOrExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbParameterReferenceExpression.
        /// </summary>
        /// <param name="expression">The DbParameterReferenceExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbParameterReferenceExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbProjectExpression.
        /// </summary>
        /// <param name="expression">The DbProjectExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbProjectExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbPropertyExpression.
        /// </summary>
        /// <param name="expression">The DbPropertyExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbPropertyExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbQuantifierExpression.
        /// </summary>
        /// <param name="expression">The DbQuantifierExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbQuantifierExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbRefExpression.
        /// </summary>
        /// <param name="expression">The DbRefExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbRefExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbRelationshipNavigationExpression.
        /// </summary>
        /// <param name="expression">The DbRelationshipNavigationExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbRelationshipNavigationExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbScanExpression.
        /// </summary>
        /// <param name="expression">The DbScanExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbScanExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbSortExpression.
        /// </summary>
        /// <param name="expression">The DbSortExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbSortExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbSkipExpression.
        /// </summary>
        /// <param name="expression">The DbSkipExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbSkipExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbTreatExpression.
        /// </summary>
        /// <param name="expression">The DbTreatExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbTreatExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbUnionAllExpression.
        /// </summary>
        /// <param name="expression">The DbUnionAllExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbUnionAllExpression expression);

        /// <summary>
        /// Typed visitor pattern method for DbVariableReferenceExpression.
        /// </summary>
        /// <param name="expression">The DbVariableReferenceExpression that is being visited.</param>
        /// <returns>An instance of TResultType.</returns>
        public abstract TResultType Visit(DbVariableReferenceExpression expression);
    }
}
