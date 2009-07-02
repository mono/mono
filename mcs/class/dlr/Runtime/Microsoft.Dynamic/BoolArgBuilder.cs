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

using System.Runtime.InteropServices;
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
    internal sealed class BoolArgBuilder : SimpleArgBuilder {
        internal BoolArgBuilder(Type parameterType)
            : base(parameterType) {
            Debug.Assert(parameterType == typeof(bool));
        }

        internal override Expression MarshalToRef(Expression parameter) {
            // parameter  ? -1 : 0
            return Expression.Condition(
                Marshal(parameter),
                Expression.Constant((Int16)(-1)),
                Expression.Constant((Int16)0)
            );
        }

        internal override Expression UnmarshalFromRef(Expression value) {
            //parameter = temp != 0
            return base.UnmarshalFromRef(
                Expression.NotEqual(
                     value,
                     Expression.Constant((Int16)0)
                )
            );
        }
    }
}

#endif
