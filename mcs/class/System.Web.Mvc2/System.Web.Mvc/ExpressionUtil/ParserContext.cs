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
    using System.Collections.Generic;

    internal class ParserContext {

        public static readonly ParameterExpression HoistedValuesParameter = Expression.Parameter(typeof(object[]), "hoistedValues");

        public ExpressionFingerprint Fingerprint;
        public readonly List<object> HoistedValues = new List<object>();
        public ParameterExpression ModelParameter;

    }
}
