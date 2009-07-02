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

using System.Diagnostics;
#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif
using System.Runtime.InteropServices;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    internal sealed class CurrencyArgBuilder : SimpleArgBuilder {
        internal CurrencyArgBuilder(Type parameterType)
            : base(parameterType) {
            Debug.Assert(parameterType == typeof(CurrencyWrapper));
        }

        internal override Expression Marshal(Expression parameter) {
            // parameter.WrappedObject
            return Expression.Property(
                Helpers.Convert(base.Marshal(parameter), typeof(CurrencyWrapper)),
                "WrappedObject"
            );
        }

        internal override Expression MarshalToRef(Expression parameter) {
            // Decimal.ToOACurrency(parameter.WrappedObject)
            return Expression.Call(
                typeof(Decimal).GetMethod("ToOACurrency"),
                Marshal(parameter)
            );
        }

        internal override Expression UnmarshalFromRef(Expression value) {
            // Decimal.FromOACurrency(value)
            return base.UnmarshalFromRef(
                Expression.New(
                    typeof(CurrencyWrapper).GetConstructor(new Type[] { typeof(Decimal) }),
                    Expression.Call(
                        typeof(Decimal).GetMethod("FromOACurrency"),
                        value
                    )
                )
            );
        }
    }
}

#endif
