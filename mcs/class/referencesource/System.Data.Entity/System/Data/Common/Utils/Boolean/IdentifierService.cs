//---------------------------------------------------------------------
// <copyright file="IdentifierService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Data.Common.Utils.Boolean
{
    /// <summary>
    /// Services related to different identifier types for Boolean expressions.
    /// </summary>
    internal abstract class IdentifierService<T_Identifier>
    {
        #region Static members
        internal static readonly IdentifierService<T_Identifier> Instance = GetIdentifierService();

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static IdentifierService<T_Identifier> GetIdentifierService()
        {
            Type identifierType = typeof(T_Identifier);
            if (identifierType.IsGenericType && 
                identifierType.GetGenericTypeDefinition() == typeof(DomainConstraint<,>))
            {
                // initialize a domain constraint literal service
                Type[] genericArguments = identifierType.GetGenericArguments();
                Type variableType = genericArguments[0];
                Type elementType = genericArguments[1];
                return (IdentifierService<T_Identifier>)Activator.CreateInstance(
                    typeof(DomainConstraintIdentifierService<,>).MakeGenericType(identifierType, variableType, elementType));
            }
            else
            {
                // initialize a generic literal service for all other identifier types
                return new GenericIdentifierService();
            }
        }
        #endregion

        #region Constructors
        private IdentifierService()
        {
        }
        #endregion

        #region Service methods
        /// <summary>
        /// Returns negation of the given literal.
        /// </summary>
        internal abstract Literal<T_Identifier> NegateLiteral(Literal<T_Identifier> literal);

        /// <summary>
        /// Creates a new conversion context.
        /// </summary>
        internal abstract ConversionContext<T_Identifier> CreateConversionContext();

        /// <summary>
        /// Performs local simplification appropriate to the current identifier.
        /// </summary>
        internal abstract BoolExpr<T_Identifier> LocalSimplify(BoolExpr<T_Identifier> expression);
        #endregion

        private class GenericIdentifierService : IdentifierService<T_Identifier>
        {
            internal override Literal<T_Identifier> NegateLiteral(Literal<T_Identifier> literal)
            {
                // just invert the sign
                return new Literal<T_Identifier>(literal.Term, !literal.IsTermPositive);
            }

            internal override ConversionContext<T_Identifier> CreateConversionContext()
            {
                return new GenericConversionContext<T_Identifier>();
            }

            internal override BoolExpr<T_Identifier> LocalSimplify(BoolExpr<T_Identifier> expression)
            {
                return expression.Accept(Simplifier<T_Identifier>.Instance);
            }
        }

        private class DomainConstraintIdentifierService<T_Variable, T_Element> : IdentifierService<DomainConstraint<T_Variable, T_Element>>
        {
            internal override Literal<DomainConstraint<T_Variable, T_Element>> NegateLiteral(Literal<DomainConstraint<T_Variable, T_Element>> literal)
            {
                // negate the literal by inverting the range, rather than changing the sign
                // of the literal
                TermExpr<DomainConstraint<T_Variable, T_Element>> term = new TermExpr<DomainConstraint<T_Variable, T_Element>>(
                    literal.Term.Identifier.InvertDomainConstraint());
                return new Literal<DomainConstraint<T_Variable, T_Element>>(term, literal.IsTermPositive);
            }

            internal override ConversionContext<DomainConstraint<T_Variable, T_Element>> CreateConversionContext()
            {
                return new DomainConstraintConversionContext<T_Variable, T_Element>();
            }

            internal override BoolExpr<DomainConstraint<T_Variable, T_Element>> LocalSimplify(BoolExpr<DomainConstraint<T_Variable, T_Element>> expression)
            {
                expression = NegationPusher.EliminateNot<T_Variable, T_Element>(expression);
                return expression.Accept(Simplifier<DomainConstraint<T_Variable, T_Element>>.Instance);
            }
        }
    }
}
