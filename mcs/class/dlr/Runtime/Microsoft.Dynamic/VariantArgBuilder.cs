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
using System.Reflection;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    internal class VariantArgBuilder : SimpleArgBuilder {
        private readonly bool _isWrapper;

        internal VariantArgBuilder(Type parameterType)
            : base(parameterType) {

            _isWrapper = parameterType == typeof(VariantWrapper);
        }

        internal override Expression Marshal(Expression parameter) {
            parameter = base.Marshal(parameter);

            // parameter.WrappedObject
            if (_isWrapper) {
                parameter = Expression.Property(
                    Helpers.Convert(parameter, typeof(VariantWrapper)),
                    typeof(VariantWrapper).GetProperty("WrappedObject")
                );
            };

            return Helpers.Convert(parameter, typeof(object));
        }

        internal override Expression MarshalToRef(Expression parameter) {
            parameter = Marshal(parameter);

            // parameter == UnsafeMethods.GetVariantForObject(parameter);
            return Expression.Call(
                typeof(UnsafeMethods).GetMethod("GetVariantForObject", BindingFlags.Static | System.Reflection.BindingFlags.NonPublic),
                parameter
            );
        }


        internal override Expression UnmarshalFromRef(Expression value) {
            // value == IntPtr.Zero ? null : Marshal.GetObjectForNativeVariant(value);

            Expression unmarshal = Expression.Call(
                typeof(UnsafeMethods).GetMethod("GetObjectForVariant"),
                value
            );

            if (_isWrapper) {
                unmarshal = Expression.New(
                    typeof(VariantWrapper).GetConstructor(new Type[] { typeof(object) }),
                    unmarshal
                );
            };

            return base.UnmarshalFromRef(unmarshal);
        }
    }
}

#endif
