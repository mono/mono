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


using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
#if CODEPLEX_40
using System.Dynamic.Utils;
#else
using Microsoft.Scripting.Utils;
#endif
using System.Runtime.CompilerServices;
#if !CODEPLEX_40
using Microsoft.Runtime.CompilerServices;
#endif


#if CODEPLEX_40
namespace System.Linq.Expressions.Compiler {
#else
namespace Microsoft.Linq.Expressions.Compiler {
#endif
    internal sealed class AnalyzedTree {
        internal readonly Dictionary<object, CompilerScope> Scopes = new Dictionary<object, CompilerScope>();
        internal readonly Dictionary<LambdaExpression, BoundConstants> Constants = new Dictionary<LambdaExpression, BoundConstants>();

        internal DebugInfoGenerator DebugInfoGenerator { get; set; }

        // Created by VariableBinder
        internal AnalyzedTree() {
        }
    }
}
