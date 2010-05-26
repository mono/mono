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

    internal static class CachedExpressionCompiler {

        // This is the entry point to the cached expression tree compiler. The processor will perform a series of checks
        // and optimizations in order to return a fully-compiled func as quickly as possible to the caller. If the
        // input expression is particularly obscure, the system will fall back to a slow but correct compilation step.
        public static Func<TModel, TValue> Process<TModel, TValue>(Expression<Func<TModel, TValue>> lambdaExpression) {
            return Processor<TModel, TValue>.GetFunc(lambdaExpression);
        }

        private static class Processor<TModel, TValue> {

            private static readonly Cache _cache = new Cache();

            public static Func<TModel, TValue> GetFunc(Expression<Func<TModel, TValue>> lambdaExpression) {
                // look for common patterns that don't need to be fingerprinted
                Func<TModel, TValue> func = GetFuncFastTrack(lambdaExpression);
                if (func != null) {
                    return func;
                }

                // not a common pattern, so try fingerprinting (slower, but cached)
                func = GetFuncFingerprinted(lambdaExpression);
                if (func != null) {
                    return func;
                }

                // pattern not recognized by fingerprinting routine, so compile directly (slowest)
                return GetFuncSlow(lambdaExpression);
            }

            private static Func<TModel, TValue> GetFuncFastTrack(Expression<Func<TModel, TValue>> lambdaExpression) {
                ParameterExpression modelParameter = lambdaExpression.Parameters[0];
                Expression body = lambdaExpression.Body;

                return FastTrack<TModel, TValue>.GetFunc(modelParameter, body);
            }

            private static Func<TModel, TValue> GetFuncFingerprinted(Expression<Func<TModel, TValue>> lambdaExpression) {
                ParserContext context = ExpressionParser.Parse(lambdaExpression);
                if (context.Fingerprint == null) {
                    // fingerprinting failed
                    return null;
                }

                object[] hoistedValues = context.HoistedValues.ToArray();
                var del = _cache.GetDelegate(context);
                return model => del(model, hoistedValues);
            }

            private static Func<TModel, TValue> GetFuncSlow(Expression<Func<TModel, TValue>> lambdaExpression) {
                Func<TModel, TValue> del = lambdaExpression.Compile();
                return del;
            }

            private sealed class Cache : ReaderWriterCache<ExpressionFingerprint, CompiledExpressionDelegate<TModel, TValue>> {
                private static CompiledExpressionDelegate<TModel, TValue> CreateDelegate(ParserContext context) {
                    var bodyExpr = context.Fingerprint.ToExpression(context);
                    var lambdaExpr = Expression.Lambda<CompiledExpressionDelegate<TModel, TValue>>(bodyExpr, context.ModelParameter, ParserContext.HoistedValuesParameter);
                    var del = lambdaExpr.Compile();
                    return del;
                }
                public CompiledExpressionDelegate<TModel, TValue> GetDelegate(ParserContext context) {
                    return FetchOrCreateItem(context.Fingerprint, () => CreateDelegate(context));
                }
            }

        }

    }
}
