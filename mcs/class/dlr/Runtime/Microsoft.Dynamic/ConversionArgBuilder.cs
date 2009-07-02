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

using System.Diagnostics;
#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif
using System.Runtime.CompilerServices;
#if !CODEPLEX_40
using Microsoft.Runtime.CompilerServices;
#endif

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

    internal class ConversionArgBuilder : ArgBuilder {
        private SimpleArgBuilder _innerBuilder;
        private Type _parameterType;

        internal ConversionArgBuilder(Type parameterType, SimpleArgBuilder innerBuilder) {
            _parameterType = parameterType;
            _innerBuilder = innerBuilder;
        }

        internal override Expression Marshal(Expression parameter) {
            return _innerBuilder.Marshal(Helpers.Convert(parameter, _parameterType));
        }

        internal override Expression MarshalToRef(Expression parameter) {
            //we are not supporting conversion InOut
            throw Assert.Unreachable;
        }
    }
}

#endif
