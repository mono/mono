//---------------------------------------------------------------------
// <copyright file="Emitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Diagnostics;
using System.Data.EntityModel.SchemaObjectModel;




namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class Emitter
    {
        #region Instance Fields
        private ClientApiGenerator _generator = null;
        #endregion

        #region Static Fields
        private static CodeExpression _nullExpression = null;
        private static CodeExpression _thisRef = null;

        /// <summary>Name of property used to get StorageContext from an Entity</summary>
        private const string EntityGetContextPropertyName = "Context";
        /// <summary>Name of property used to get StorageContext from a StorageSearcher</summary>
        protected const string SearcherGetContextPropertyName = "Context";
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="generator"></param>
        protected Emitter(ClientApiGenerator generator)
        {
            Generator = generator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected static CodeBinaryOperatorExpression EmitExpressionEqualsNull(CodeExpression expression)
        {
            return new CodeBinaryOperatorExpression(expression, CodeBinaryOperatorType.IdentityEquality, NullExpression);
        }

        protected static CodeBinaryOperatorExpression EmitExpressionDoesNotEqualNull(CodeExpression expression)
        {
            return new CodeBinaryOperatorExpression(expression, CodeBinaryOperatorType.IdentityInequality, NullExpression);
        }

        internal static CodeExpression EmitEnumMemberExpression(CodeTypeReference type, string member)
        {
            CodeTypeReferenceExpression typeref = new CodeTypeReferenceExpression(type);
            return new CodeFieldReferenceExpression(typeref, member);
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// 
        /// </summary>
        protected static CodeExpression ThisRef
        {
            get
            {
                if (_thisRef == null)
                    _thisRef = new CodeThisReferenceExpression();
                return _thisRef;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal ClientApiGenerator Generator
        {
            get
            {
                return _generator;
            }
            private set
            {
                _generator = value;
            }
        }

        protected TypeReference TypeReference
        {
            get
            {
                return _generator.TypeReference;
            }
        }

        protected AttributeEmitter AttributeEmitter
        {
            get { return _generator.AttributeEmitter; }
        }

        protected static CodeExpression NullExpression
        {
            get
            {
                if (_nullExpression == null)
                    _nullExpression = new CodePrimitiveExpression(null);

                return _nullExpression;

            }
        }

        #endregion
    }
}
