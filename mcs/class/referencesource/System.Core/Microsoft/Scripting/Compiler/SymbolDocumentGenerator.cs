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

#if SILVERLIGHT
using System.Core;
#endif

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Runtime.CompilerServices {
#if CLR2 || SILVERLIGHT
    using ILGenerator = OffsetTrackingILGenerator;
#endif

    /// <summary>
    /// Generator of PDB debugging information for expression trees.
    /// </summary>
    internal sealed class SymbolDocumentGenerator : DebugInfoGenerator {
        private Dictionary<SymbolDocumentInfo, ISymbolDocumentWriter> _symbolWriters;

        private ISymbolDocumentWriter GetSymbolWriter(MethodBuilder method, SymbolDocumentInfo document) {
            ISymbolDocumentWriter result;
            if (_symbolWriters == null) {
                _symbolWriters = new Dictionary<SymbolDocumentInfo, ISymbolDocumentWriter>();
            }

            if (!_symbolWriters.TryGetValue(document, out result)) {
                result = ((ModuleBuilder)method.Module).DefineDocument(document.FileName, document.Language, document.LanguageVendor, SymbolGuids.DocumentType_Text);
                _symbolWriters.Add(document, result);
            }

            return result;
        }

        internal override void MarkSequencePoint(LambdaExpression method, MethodBase methodBase, ILGenerator ilg, DebugInfoExpression sequencePoint) {
            MethodBuilder builder = methodBase as MethodBuilder;
            if (builder != null) {
                ilg.MarkSequencePoint(GetSymbolWriter(builder, sequencePoint.Document), sequencePoint.StartLine, sequencePoint.StartColumn, sequencePoint.EndLine, sequencePoint.EndColumn);
            }
        }

        public override void MarkSequencePoint(LambdaExpression method, int ilOffset, DebugInfoExpression sequencePoint) {
            throw Error.PdbGeneratorNeedsExpressionCompiler();
        }

        internal override void SetLocalName(LocalBuilder localBuilder, string name) {
            localBuilder.SetLocalSymInfo(name);
        }
    }
}
