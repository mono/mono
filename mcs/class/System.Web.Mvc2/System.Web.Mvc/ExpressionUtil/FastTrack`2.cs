/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class FastTrack<TModel, TValue> {

        private static Func<TModel, TValue> _identityFunc;

        private static readonly ConstMemberLookupCache _constMemberLookupCache = new ConstMemberLookupCache();
        private static readonly ModelMemberLookupCache _modelMemberLookupCache = new ModelMemberLookupCache();

        public static Func<TModel, TValue> GetFunc(ParameterExpression modelParameter, Expression body) {
            { // model => model
                if (modelParameter == body) {
                    return GetIdentityFunc();
                }
            }

            { // model => {const}
                ConstantExpression constantExpression = body as ConstantExpression;
                if (constantExpression != null) {
                    TValue value = (TValue)constantExpression.Value;
                    return _ => value;
                }
            }

            {
                MemberExpression memberExpression = body as MemberExpression;
                if (memberExpression != null) {
                    if (memberExpression.Expression == null) {
                        // model => {static member}
                        return GetModelMemberLookupFunc(memberExpression.Member, true /* isStatic */);
                    }
                    else if (memberExpression.Expression == modelParameter) {
                        // model => model.Member
                        return GetModelMemberLookupFunc(memberExpression.Member, false /* isStatic */);
                    }
                    else {
                        ConstantExpression constantExpression = memberExpression.Expression as ConstantExpression;
                        if (constantExpression != null) {
                            // model => {const}.Member, e.g. captured local variable in a foreach
                            return GetConstMemberLookupFunc(memberExpression.Member, constantExpression.Value);
                        }
                    }
                }
            }

            // don't know how to fast-track
            return null;
        }

        private static Func<TModel, TValue> GetIdentityFunc() {
            // don't need to worry about locking since all identity funcs are the same
            if (_identityFunc == null) {
                ParameterExpression modelParameter = Expression.Parameter(typeof(TModel), "model");
                Expression<Func<TModel, TValue>> identityLambda = Expression.Lambda<Func<TModel, TValue>>(modelParameter, modelParameter);
                _identityFunc = identityLambda.Compile();
            }

            return _identityFunc;
        }

        private static Func<TModel, TValue> GetModelMemberLookupFunc(MemberInfo member, bool isStatic) {
            return _modelMemberLookupCache.GetFunc(member, isStatic);
        }

        private static Func<TModel, TValue> GetConstMemberLookupFunc(MemberInfo member, object constValue) {
            Func<object, TValue> innerFunc = _constMemberLookupCache.GetFunc(member);
            return _ => innerFunc(constValue);
        }

        private sealed class ConstMemberLookupCache : ReaderWriterCache<MemberInfo, Func<object, TValue>> {
            private static Func<object, TValue> CreateFunc(MemberInfo member) {
                ParameterExpression constParam = Expression.Parameter(typeof(object), "constValue");
                // cast is necessary since the constant value comes in as a standard 'object'
                UnaryExpression castExpr = Expression.Convert(constParam, member.DeclaringType);
                MemberExpression memberExpr = Expression.MakeMemberAccess(castExpr, member);
                Expression<Func<object, TValue>> lambda = Expression.Lambda<Func<object, TValue>>(memberExpr, constParam);
                return lambda.Compile();
            }
            public Func<object, TValue> GetFunc(MemberInfo member) {
                return FetchOrCreateItem(member, () => CreateFunc(member));
            }
        }

        private sealed class ModelMemberLookupCache : ReaderWriterCache<MemberInfo, Func<TModel, TValue>> {
            private static Func<TModel, TValue> CreateFunc(MemberInfo member, bool isStatic) {
                ParameterExpression modelParam = Expression.Parameter(typeof(TModel), "model");
                MemberExpression memberExpr = Expression.MakeMemberAccess((!isStatic) ? modelParam : null, member);
                Expression<Func<TModel, TValue>> lambda = Expression.Lambda<Func<TModel, TValue>>(memberExpr, modelParam);
                return lambda.Compile();
            }
            public Func<TModel, TValue> GetFunc(MemberInfo member, bool isStatic) {
                return FetchOrCreateItem(member, () => CreateFunc(member, isStatic));
            }
        }

    }
}
