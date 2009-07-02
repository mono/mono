/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


#if !SILVERLIGHT // ComObject

using System.Collections.Generic;
#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif
using System.Runtime.InteropServices;
using System.Diagnostics;
#if CODEPLEX_40
using System.Dynamic.Utils;
#else
using Microsoft.Scripting.Utils;
#endif

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    internal class ErrorArgBuilder : SimpleArgBuilder {
        internal ErrorArgBuilder(Type parameterType)
            : base(parameterType) {

            Debug.Assert(parameterType == typeof(ErrorWrapper));
        }

        internal override Expression Marshal(Expression parameter) {
            // parameter.ErrorCode
            return Expression.Property(
                Helpers.Convert(base.Marshal(parameter), typeof(ErrorWrapper)),
                "ErrorCode"
            );
        }

        internal override Expression UnmarshalFromRef(Expression value) {
            // new ErrorWrapper(value)
            return base.UnmarshalFromRef(
                Expression.New(
                    typeof(ErrorWrapper).GetConstructor(new Type[] { typeof(int) }),
                    value
                )
            );
        }
    }
}

#endif
