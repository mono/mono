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

#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif
using System.Runtime.InteropServices;
#if CODEPLEX_40
using System.Dynamic;
#else
using Microsoft.Scripting;
#endif
using System.Globalization;
using ComTypes = System.Runtime.InteropServices.ComTypes;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif
    internal sealed class ComTypeEnumDesc : ComTypeDesc {
        public override string ToString() {
            return String.Format(CultureInfo.CurrentCulture, "<enum '{0}'>", TypeName);
        }

        internal ComTypeEnumDesc(ComTypes.ITypeInfo typeInfo) :
            base(typeInfo) {
        }
    }
}

#endif
