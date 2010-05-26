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

    internal static class ExpressionParser {

        public static ParserContext Parse<TModel, TValue>(Expression<Func<TModel, TValue>> expression) {
            ParserContext context = new ParserContext() {
                ModelParameter = expression.Parameters[0]
            };

            Expression body = expression.Body;
            context.Fingerprint = ExpressionFingerprint.Create(body, context);
            return context;
        }

    }
}
