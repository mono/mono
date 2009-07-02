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


#if !SILVERLIGHT

#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    internal class ConvertArgBuilder : SimpleArgBuilder {
        private readonly Type _marshalType;

        internal ConvertArgBuilder(Type parameterType, Type marshalType)
            : base(parameterType) {
            _marshalType = marshalType;
        }

        internal override Expression Marshal(Expression parameter) {
            parameter = base.Marshal(parameter);
            return Expression.Convert(parameter, _marshalType);
        }

        internal override Expression UnmarshalFromRef(Expression newValue) {
            return base.UnmarshalFromRef(Expression.Convert(newValue, ParameterType));
        }
    }
}

#endif
