/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CLR2
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Ast.Compiler;
#else
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Reflection;

namespace System.Runtime.CompilerServices {
#if CLR2 || SILVERLIGHT
    using ILGenerator = OffsetTrackingILGenerator;
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
