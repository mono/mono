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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;

// Not needed in CLR 4 builds because we have the
// ILGenerator.ILOffset property.

#if CLR2 || SILVERLIGHT

#if CLR2
namespace Microsoft.Scripting.Ast.Compiler {
#else
namespace System.Linq.Expressions.Compiler {
#endif
    /// <summary>
    /// Wraps ILGenerator with code that tracks the current IL offset as instructions are emitted into the IL stream.
    /// </summary>
    internal sealed class OffsetTrackingILGenerator {
        private readonly ILGenerator _ilg;
        internal int _offset;

        internal int ILOffset { get { return _offset; } }

        internal OffsetTrackingILGenerator(ILGenerator ilg) {
            Debug.Assert(ilg != null);
            _ilg = ilg;
        }

        private void AdvanceOffset(OpCode opcode) {
            _offset += opcode.Size;
        }

        private void AdvanceOffsetWithLabel(OpCode opcode) {
            AdvanceOffset(opcode);
            if (OpCodes.TakesSingleByteArgument(opcode)) {
                _offset++;
            } else {
                _offset += 4;
            }
        }

        #region Simple Instructions

        internal void Emit(OpCode opcode) {
            _ilg.Emit(opcode);
            AdvanceOffset(opcode);
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, byte arg) {
            _ilg.Emit(opcode, arg);
            AdvanceOffset(opcode);
            _offset++;
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, sbyte arg) {
            _ilg.Emit(opcode, arg);
            AdvanceOffset(opcode);
            _offset++;
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, int arg) {
            _ilg.Emit(opcode, arg);
            AdvanceOffset(opcode);
            _offset += 4;
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, MethodInfo meth) {
            _ilg.Emit(opcode, meth);
            AdvanceOffset(opcode);
            _offset += 4;
            AssertOffsetMatches();
        }

        internal void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes) {
            _ilg.EmitCall(opcode, methodInfo, optionalParameterTypes);
            AdvanceOffset(opcode);
            _offset += 4;
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, ConstructorInfo con) {
            _ilg.Emit(opcode, con);
            AdvanceOffset(opcode);
            _offset += 4;
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, Type cls) {
            _ilg.Emit(opcode, cls);
            AdvanceOffset(opcode);
            _offset += 4;
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, long arg) {
            _ilg.Emit(opcode, arg);
            AdvanceOffset(opcode);
            _offset += 8;
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, float arg) {
            _ilg.Emit(opcode, arg);
            AdvanceOffset(opcode);
            _offset += 4;
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, double arg) {
            _ilg.Emit(opcode, arg);
            AdvanceOffset(opcode);
            _offset += 8;
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, Label label) {
            _ilg.Emit(opcode, label);
            AdvanceOffsetWithLabel(opcode);
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, Label[] labels) {
            _ilg.Emit(opcode, labels);
            AdvanceOffset(opcode);
            _offset += 4;
            for (int remaining = labels.Length * 4, i = 0; remaining > 0; remaining -= 4, i++) {
                _offset += 4;
            }
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, FieldInfo field) {
            _ilg.Emit(opcode, field);
            AdvanceOffset(opcode);
            _offset += 4;
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, String str) {
            _ilg.Emit(opcode, str);
            AdvanceOffset(opcode);
            _offset += 4;
            AssertOffsetMatches();
        }

        internal void Emit(OpCode opcode, LocalBuilder local) {
            _ilg.Emit(opcode, local);
            int tempVal = local.LocalIndex;
            if (opcode.Equals(OpCodes.Ldloc)) {
                switch (tempVal) {
                    case 0:
                        opcode = OpCodes.Ldloc_0;
                        break;
                    case 1:
                        opcode = OpCodes.Ldloc_1;
                        break;
                    case 2:
                        opcode = OpCodes.Ldloc_2;
                        break;
                    case 3:
                        opcode = OpCodes.Ldloc_3;
                        break;
                    default:
                        if (tempVal <= 255)
                            opcode = OpCodes.Ldloc_S;
                        break;
                }
            } else if (opcode.Equals(OpCodes.Stloc)) {
                switch (tempVal) {
                    case 0:
                        opcode = OpCodes.Stloc_0;
                        break;
                    case 1:
                        opcode = OpCodes.Stloc_1;
                        break;
                    case 2:
                        opcode = OpCodes.Stloc_2;
                        break;
                    case 3:
                        opcode = OpCodes.Stloc_3;
                        break;
                    default:
                        if (tempVal <= 255)
                            opcode = OpCodes.Stloc_S;
                        break;
                }
            } else if (opcode.Equals(OpCodes.Ldloca)) {
                if (tempVal <= 255)
                    opcode = OpCodes.Ldloca_S;
            }

            AdvanceOffset(opcode);

            if (opcode.OperandType == OperandType.InlineNone)
                return;
            else if (!OpCodes.TakesSingleByteArgument(opcode)) {
                _offset += 2;
            } else {
                _offset++;
            }
            AssertOffsetMatches();
        }

        #endregion

        #region Exception Handling
        
        private enum ExceptionState {
            Try = 0,
            Filter = 1,
            Catch = 2,
            Finally = 3,
            Fault = 4,
        }

        private Stack<ExceptionState> _exceptionState = new Stack<ExceptionState>();

        internal void BeginExceptionBlock() {
            _ilg.BeginExceptionBlock();
            _exceptionState.Push(ExceptionState.Try);
            AssertOffsetMatches();
        }

        internal void EndExceptionBlock() {
            _ilg.EndExceptionBlock();

            ExceptionState state = _exceptionState.Pop();
            if (state == ExceptionState.Catch) {
                AdvanceOffsetWithLabel(OpCodes.Leave);
            } else if (state == ExceptionState.Finally || state == ExceptionState.Fault) {
                AdvanceOffset(OpCodes.Endfinally);
            }

            AssertOffsetMatches();
        }

        internal void BeginExceptFilterBlock() {
            _ilg.BeginExceptFilterBlock();

            _exceptionState.Pop();
            _exceptionState.Push(ExceptionState.Filter);

            AssertOffsetMatches();
        }

        internal void BeginCatchBlock(Type exceptionType) {
            _ilg.BeginCatchBlock(exceptionType);

            ExceptionState state = _exceptionState.Pop();
            if (state == ExceptionState.Filter) {
                AdvanceOffset(OpCodes.Endfilter);
            } else {
                AdvanceOffsetWithLabel(OpCodes.Leave);
            }

            _exceptionState.Push(ExceptionState.Catch);

            AssertOffsetMatches();
        }

        internal void BeginFaultBlock() {
            _ilg.BeginFaultBlock();

            AdvanceOffsetWithLabel(OpCodes.Leave);
            _exceptionState.Pop();
            _exceptionState.Push(ExceptionState.Fault);

            AssertOffsetMatches();
        }

        internal void BeginFinallyBlock() {
            _ilg.BeginFinallyBlock();

            ExceptionState state = _exceptionState.Pop();
            if (state != ExceptionState.Try) {
                // leave for any preceeding catch clause
                AdvanceOffsetWithLabel(OpCodes.Leave);
            }

            // leave for try clause                                                  
            AdvanceOffsetWithLabel(OpCodes.Leave);
            _exceptionState.Push(ExceptionState.Finally);

            AssertOffsetMatches();
        }

        #endregion

        #region Labels and Locals

        internal Label DefineLabel() {
            return _ilg.DefineLabel();
        }

        internal void MarkLabel(Label loc) {
            _ilg.MarkLabel(loc);
        }

        internal LocalBuilder DeclareLocal(Type localType) {
            return _ilg.DeclareLocal(localType);
        }

        internal void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn) {
            _ilg.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
        }

        #endregion

        #region Assertions

#if STRESS_DEBUG
        private FieldInfo _ilgOffsetField;
        private bool _checkOffset = true;
#endif

        [Conditional("STRESS_DEBUG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void AssertOffsetMatches() {
#if STRESS_DEBUG
            if (!_checkOffset) {
                return;
            }

            int m_length = -1;
            try {
                if (_ilgOffsetField == null) {
                    _ilgOffsetField = typeof(ILGenerator).GetField("m_length", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                m_length = (int)_ilgOffsetField.GetValue(_ilg);
            } catch (Exception) {
                _checkOffset = false;
            }

            if (_checkOffset) {
                Debug.Assert(m_length == _offset);
            }
#endif
        }

        #endregion
    }
}

#endif
