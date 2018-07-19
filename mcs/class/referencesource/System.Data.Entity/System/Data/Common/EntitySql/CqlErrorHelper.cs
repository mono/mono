//---------------------------------------------------------------------
// <copyright file="CqlQuery.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql
{
    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Globalization;

    /// <summary>
    /// Error reporting Helper
    /// </summary>
    internal static class CqlErrorHelper
    {
        /// <summary>
        /// Reports function overload resolution error.
        /// </summary>
        internal static void ReportFunctionOverloadError(AST.MethodExpr functionExpr, EdmFunction functionType, List<TypeUsage> argTypes)
        {
            string strDelim = "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(functionType.Name).Append("(");
            for (int i = 0 ; i < argTypes.Count ; i++)
            {
                sb.Append(strDelim);
                sb.Append(argTypes[i] != null ? argTypes[i].EdmType.FullName : "NULL");
                strDelim = ", ";
            }
            sb.Append(")");

            Func<object, object, object, string> formatString;
            if (TypeSemantics.IsAggregateFunction(functionType))
            {
                formatString = TypeHelpers.IsCanonicalFunction(functionType) ? 
                                            (Func<object, object, object, string>)System.Data.Entity.Strings.NoCanonicalAggrFunctionOverloadMatch :
                                            (Func<object, object, object, string>)System.Data.Entity.Strings.NoAggrFunctionOverloadMatch;
            }
            else
            {
                formatString = TypeHelpers.IsCanonicalFunction(functionType) ?
                                            (Func<object, object, object, string>)System.Data.Entity.Strings.NoCanonicalFunctionOverloadMatch :
                                            (Func<object, object, object, string>)System.Data.Entity.Strings.NoFunctionOverloadMatch;
            }

            throw EntityUtil.EntitySqlError(functionExpr.ErrCtx.CommandText,
                                 formatString(functionType.NamespaceName, functionType.Name, sb.ToString()),
                                 functionExpr.ErrCtx.InputPosition,
                                 System.Data.Entity.Strings.CtxFunction(functionType.Name),
                                 false /* loadContextInfoFromResource */);
        }
        
        /// <summary>
        /// provides error feedback for aliases already used in a given context
        /// </summary>
        /// <param name="aliasName"></param>
        /// <param name="errCtx"></param>
        /// <param name="contextMessage"></param>
        internal static void ReportAliasAlreadyUsedError( string aliasName, ErrorContext errCtx, string contextMessage )
        {
            throw EntityUtil.EntitySqlError(errCtx, String.Format(CultureInfo.InvariantCulture, "{0} {1}", System.Data.Entity.Strings.AliasNameAlreadyUsed(aliasName), contextMessage));
        }

        /// <summary>
        /// Reports incompatible type error
        /// </summary>
        /// <param name="errCtx"></param>
        /// <param name="leftType"></param>
        /// <param name="rightType"></param>
        internal static void ReportIncompatibleCommonType( ErrorContext errCtx, TypeUsage leftType, TypeUsage rightType )
        {
            //
            // 'navigate' through the type structure in order to find where the incompability is
            //
            ReportIncompatibleCommonType(errCtx, leftType, rightType, leftType, rightType);

            //
            // if we hit this point, throw the generic incompatible type error message
            //
            throw EntityUtil.EntitySqlError(errCtx, System.Data.Entity.Strings.ArgumentTypesAreIncompatible(leftType.Identity, rightType.Identity));
        }

        /// <summary>
        /// navigates through the type structure to find where the incompatibility happens
        /// </summary>
        /// <param name="errCtx"></param>
        /// <param name="rootLeftType"></param>
        /// <param name="rootRightType"></param>
        /// <param name="leftType"></param>
        /// <param name="rightType"></param>
        private static void ReportIncompatibleCommonType( ErrorContext errCtx, TypeUsage rootLeftType, TypeUsage rootRightType, TypeUsage leftType, TypeUsage rightType )
        {
            TypeUsage commonType = null;
            bool isRootType = (rootLeftType == leftType);
            string errorMessage = String.Empty;

            if (leftType.EdmType.BuiltInTypeKind != rightType.EdmType.BuiltInTypeKind)
            {
                throw EntityUtil.EntitySqlError(errCtx,
                                     System.Data.Entity.Strings.TypeKindMismatch(
                                                   GetReadableTypeKind(leftType), 
                                                   GetReadableTypeName(leftType),
                                                   GetReadableTypeKind(rightType), 
                                                   GetReadableTypeName(rightType)));
            }

            switch( leftType.EdmType.BuiltInTypeKind )
            {
                case BuiltInTypeKind.RowType:
                    RowType leftRow = (RowType)leftType.EdmType;
                    RowType rightRow = (RowType)rightType.EdmType;

                    if (leftRow.Members.Count != rightRow.Members.Count)
                    {
                        if (isRootType)
                        {
                            errorMessage = System.Data.Entity.Strings.InvalidRootRowType(
                                                         GetReadableTypeName(leftRow), 
                                                         GetReadableTypeName(rightRow));
                        }
                        else
                        {
                            errorMessage = System.Data.Entity.Strings.InvalidRowType(
                                                         GetReadableTypeName(leftRow),
                                                         GetReadableTypeName(rootLeftType), 
                                                         GetReadableTypeName(rightRow), 
                                                         GetReadableTypeName(rootRightType));
                        }
                        
                        throw EntityUtil.EntitySqlError(errCtx, errorMessage);
                    }

                    for (int i = 0 ; i < leftRow.Members.Count ; i++)
                    {
                        ReportIncompatibleCommonType(errCtx, rootLeftType, rootRightType, leftRow.Members[i].TypeUsage, rightRow.Members[i].TypeUsage);
                    }
                break;

                case BuiltInTypeKind.CollectionType:
                case BuiltInTypeKind.RefType:
                    ReportIncompatibleCommonType(errCtx, 
                                               rootLeftType, 
                                               rootRightType, 
                                               TypeHelpers.GetElementTypeUsage(leftType), 
                                               TypeHelpers.GetElementTypeUsage(rightType));
                    break;

                case BuiltInTypeKind.EntityType:
                    if (!TypeSemantics.TryGetCommonType(leftType, rightType, out commonType))
                    {
                        if (isRootType)
                        {
                            errorMessage = System.Data.Entity.Strings.InvalidEntityRootTypeArgument(
                                                         GetReadableTypeName(leftType), 
                                                         GetReadableTypeName(rightType));
                        }
                        else
                        {
                            errorMessage = System.Data.Entity.Strings.InvalidEntityTypeArgument(
                                                         GetReadableTypeName(leftType),
                                                         GetReadableTypeName(rootLeftType),
                                                         GetReadableTypeName(rightType),
                                                         GetReadableTypeName(rootRightType));
                        }
                        throw EntityUtil.EntitySqlError(errCtx, errorMessage);
                    }
                    break;

                case BuiltInTypeKind.ComplexType:
                    ComplexType leftComplex = (ComplexType)leftType.EdmType;
                    ComplexType rightComplex = (ComplexType)rightType.EdmType;
                    if (leftComplex.Members.Count != rightComplex.Members.Count)
                    {
                        if (isRootType)
                        {
                            errorMessage = System.Data.Entity.Strings.InvalidRootComplexType(
                                                         GetReadableTypeName(leftComplex),
                                                         GetReadableTypeName(rightComplex));
                        }
                        else
                        {
                            errorMessage = System.Data.Entity.Strings.InvalidComplexType(
                                                         GetReadableTypeName(leftComplex),
                                                         GetReadableTypeName(rootLeftType),
                                                         GetReadableTypeName(rightComplex),
                                                         GetReadableTypeName(rootRightType));
                        }
                        throw EntityUtil.EntitySqlError(errCtx, errorMessage);
                    }

                    for (int i = 0 ; i < leftComplex.Members.Count ; i++)
                    {
                        ReportIncompatibleCommonType(errCtx, 
                                                   rootLeftType, 
                                                   rootRightType, 
                                                   leftComplex.Members[i].TypeUsage, 
                                                   rightComplex.Members[i].TypeUsage);
                    }
                    break;

                default:
                    if (!TypeSemantics.TryGetCommonType(leftType, rightType, out commonType))
                    {
                        if (isRootType)
                        {
                            errorMessage = System.Data.Entity.Strings.InvalidPlaceholderRootTypeArgument(
                                                               GetReadableTypeKind(leftType),
                                                               GetReadableTypeName(leftType),
                                                               GetReadableTypeKind(rightType),
                                                               GetReadableTypeName(rightType));
                        }
                        else
                        {
                            errorMessage = System.Data.Entity.Strings.InvalidPlaceholderTypeArgument(
                                                               GetReadableTypeKind(leftType),
                                                               GetReadableTypeName(leftType),
                                                               GetReadableTypeName(rootLeftType),
                                                               GetReadableTypeKind(rightType),
                                                               GetReadableTypeName(rightType),
                                                               GetReadableTypeName(rootRightType));
                        }
                        throw EntityUtil.EntitySqlError(errCtx, errorMessage);
                    }
                    break;
            }
        }

        #region Private Type Name Helpers
        private static string GetReadableTypeName( TypeUsage type )
        {
            return GetReadableTypeName(type.EdmType);
        }

        private static string GetReadableTypeName( EdmType type )
        {
            if (type.BuiltInTypeKind == BuiltInTypeKind.RowType || 
                type.BuiltInTypeKind == BuiltInTypeKind.CollectionType ||
                type.BuiltInTypeKind == BuiltInTypeKind.RefType)
            {
                return type.Name;
            }
            return type.FullName;
        }

        private static string GetReadableTypeKind( TypeUsage type )
        {
            return GetReadableTypeKind(type.EdmType);
        }

        private static string GetReadableTypeKind( EdmType type )
        {
            string typeKindName = String.Empty;
            switch( type.BuiltInTypeKind)
            {
                case BuiltInTypeKind.RowType:
                    typeKindName = System.Data.Entity.Strings.LocalizedRow;
                    break;
                case BuiltInTypeKind.CollectionType:
                    typeKindName = System.Data.Entity.Strings.LocalizedCollection;
                    break;
                case BuiltInTypeKind.RefType:
                    typeKindName = System.Data.Entity.Strings.LocalizedReference;
                    break;
                case BuiltInTypeKind.EntityType:
                    typeKindName = System.Data.Entity.Strings.LocalizedEntity;
                    break;
                case BuiltInTypeKind.ComplexType:
                    typeKindName = System.Data.Entity.Strings.LocalizedComplex;
                    break;
                case BuiltInTypeKind.PrimitiveType:
                    typeKindName = System.Data.Entity.Strings.LocalizedPrimitive;
                    break;
                default:
                    typeKindName = type.BuiltInTypeKind.ToString();
                    break;
            }
            return typeKindName + " " + System.Data.Entity.Strings.LocalizedType;
        }
        #endregion
    }

}
