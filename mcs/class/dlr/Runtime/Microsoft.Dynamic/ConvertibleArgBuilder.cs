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

using System.Globalization;
#if CODEPLEX_40
using System.Linq.Expressions;
using System.Dynamic.Utils;
#else
using Microsoft.Linq.Expressions;
using Microsoft.Scripting.Utils;
#endif

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif

    internal class ConvertibleArgBuilder : ArgBuilder {
        internal ConvertibleArgBuilder() {
        }

        internal override Expression Marshal(Expression parameter) {
            return Helpers.Convert(parameter, typeof(IConvertible));
        }

        internal override Expression MarshalToRef(Expression parameter) {
            //we are not supporting convertible InOut
            throw Assert.Unreachable;
        }
    }
}

#endif
