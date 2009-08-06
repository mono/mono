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

#if MICROSOFT_SCRIPTING_CORE || SILVERLIGHT
#if CODEPLEX_40
using ILGenerator = System.Linq.Expressions.Compiler.OffsetTrackingILGenerator;
#else
using ILGenerator = Microsoft.Linq.Expressions.Compiler.OffsetTrackingILGenerator;
#endif
#endif

#if CODEPLEX_40
using System;
#else
using System; using Microsoft;
#endif
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Reflection;
#if CODEPLEX_40
using System.Linq.Expressions;
#else
using Microsoft.Linq.Expressions;
#endif

#if CODEPLEX_40
namespace System.Runtime.CompilerServices {
#else
namespace Microsoft.Runtime.CompilerServices {
#endif
    /// <summary>
    /// Generates debug information for lambdas in an expression tree.
    /// </summary>
    public abstract class DebugInfoGenerator {
        /// <summary>
        /// Creates PDB symbol generator.
        /// </summary>
        /// <returns>PDB symbol generator.</returns>
        public static DebugInfoGenerator CreatePdbGenerator() {
            return new SymbolDocumentGenerator();
        }

        /// <summary>
        /// Marks a sequence point.
        /// </summary>
        /// <param name="method">The lambda being generated.</param>
        /// <param name="ilOffset">IL offset where to mark the sequence point.</param>
        /// <param name="sequencePoint">Debug informaton corresponding to the sequence point.</param>
        public abstract void MarkSequencePoint(LambdaExpression method, int ilOffset, DebugInfoExpression sequencePoint);

        internal virtual void MarkSequencePoint(LambdaExpression method, MethodBase methodBase, ILGenerator ilg, DebugInfoExpression sequencePoint) {
            MarkSequencePoint(method, ilg.ILOffset, sequencePoint);
        }

        internal virtual void SetLocalName(LocalBuilder localBuilder, string name) {
            // nop
        }
    }
}
