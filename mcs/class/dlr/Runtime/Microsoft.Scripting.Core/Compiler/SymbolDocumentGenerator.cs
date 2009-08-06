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


#if MICROSOFT_SCRIPTING_CORE || SILVERLIGHT
#if CODEPLEX_40
using ILGenerator = System.Linq.Expressions.Compiler.OffsetTrackingILGenerator;
#else
using ILGenerator = Microsoft.Linq.Expressions.Compiler.OffsetTrackingILGenerator;
#endif
#endif

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
#if CODEPLEX_40
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
#else
using Microsoft.Linq.Expressions;
using Microsoft.Linq.Expressions.Compiler;
#endif
using System.Reflection;
using System.Reflection.Emit;

#if SILVERLIGHT
using System.Core;
#endif

#if CODEPLEX_40
namespace System.Runtime.CompilerServices {
#else
namespace Microsoft.Runtime.CompilerServices {
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
