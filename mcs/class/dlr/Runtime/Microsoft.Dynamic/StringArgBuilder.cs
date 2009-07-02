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

    internal class StringArgBuilder : SimpleArgBuilder {
        private readonly bool _isWrapper;

        internal StringArgBuilder(Type parameterType)
            : base(parameterType) {

            Debug.Assert(parameterType == typeof(string) ||
                        parameterType == typeof(BStrWrapper));

            _isWrapper = parameterType == typeof(BStrWrapper);
        }

        internal override Expression Marshal(Expression parameter) {
            parameter = base.Marshal(parameter);

            // parameter.WrappedObject
            if (_isWrapper) {
                parameter = Expression.Property(
                    Helpers.Convert(parameter, typeof(BStrWrapper)),
                    typeof(BStrWrapper).GetProperty("WrappedObject")
                );
            };

            return parameter;
        }

        internal override Expression MarshalToRef(Expression parameter) {
            parameter = Marshal(parameter);


            // Marshal.StringToBSTR(parameter)
            return Expression.Call(
                typeof(Marshal).GetMethod("StringToBSTR"),
                parameter
            );
        }

        internal override Expression UnmarshalFromRef(Expression value) {
            // value == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(value);
            Expression unmarshal = Expression.Condition(
                Expression.Equal(value, Expression.Constant(IntPtr.Zero)),
                Expression.Constant(null, typeof(string)),   // default value
                Expression.Call(
                    typeof(Marshal).GetMethod("PtrToStringBSTR"),
                    value
                )
            );

            if (_isWrapper) {
                unmarshal = Expression.New(
                    typeof(BStrWrapper).GetConstructor(new Type[] { typeof(string) }),
                    unmarshal
                );
            };

            return base.UnmarshalFromRef(unmarshal);
        }
    }
}

#endif
