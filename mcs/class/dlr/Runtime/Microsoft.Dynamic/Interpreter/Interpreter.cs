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

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.Scripting.Interpreter {
    /// <summary>
    /// A simple forth-style stack machine for executing Expression trees
    /// without the need to compile to IL and then invoke the JIT.  This trades
    /// off much faster compilation time for a slower execution performance.
    /// For code that is only run a small number of times this can be a 
    /// sweet spot.
    /// 
    /// The core loop in the interpreter is the RunInstructions method.
    /// </summary>
    internal sealed class Interpreter {
        internal static readonly object NoValue = new object();
        internal const int RethrowOnReturn = Int32.MaxValue;

        // zero: sync compilation
        // negative: default
        internal readonly int _compilationThreshold;

        private readonly int _localCount;
        private readonly HybridReferenceDictionary<LabelTarget, BranchLabel> _labelMapping;
        private readonly Dictionary<ParameterExpression, LocalVariable> _closureVariables;

        private readonly InstructionArray _instructions;
        internal readonly object[] _objects;
        internal readonly RuntimeLabel[] _labels;

        internal readonly string _name;
        private readonly ExceptionHandler[] _handlers;
        internal readonly DebugInfo[] _debugInfos;

        internal Interpreter(string name, LocalVariables locals, HybridReferenceDictionary<LabelTarget, BranchLabel> labelMapping,
            InstructionArray instructions, ExceptionHandler[] handlers, DebugInfo[] debugInfos, int compilationThreshold) {

            _name = name;
            _localCount = locals.LocalCount;
            _closureVariables = locals.ClosureVariables;

            _instructions = instructions;
            _objects = instructions.Objects;
            _labels = instructions.Labels;
            _labelMapping = labelMapping;

            _handlers = handlers;
            _debugInfos = debugInfos;
            _compilationThreshold = compilationThreshold;
        }

        internal int ClosureSize {
            get {
                if (_closureVariables == null) {
                    return 0;
                }
                return _closureVariables.Count;
            }
        }

        internal int LocalCount {
            get {
                return _localCount;
            }
        }

        internal bool CompileSynchronously {
            get { return _compilationThreshold <= 1; }
        }

        internal InstructionArray Instructions {
            get { return _instructions; }
        }

        internal Dictionary<ParameterExpression, LocalVariable> ClosureVariables {
            get { return _closureVariables; } 
        }

        internal HybridReferenceDictionary<LabelTarget, BranchLabel> LabelMapping {
            get { return _labelMapping; }
        }

        /// <summary>
        /// Runs instructions within the given frame.
        /// </summary>
        /// <remarks>
        /// Interpreted stack frames are linked via Parent reference so that each CLR frame of this method corresponds 
        /// to an interpreted stack frame in the chain. It is therefore possible to combine CLR stack traces with 
        /// interpreted stack traces by aligning interpreted frames to the frames of this method.
        /// Each group of subsequent frames of Run method corresponds to a single interpreted frame.
        /// </remarks>
        [SpecialName, MethodImpl(MethodImplOptions.NoInlining)]
        public void Run(InterpretedFrame frame) {
            while (true) {
                try {
                    var instructions = _instructions.Instructions;
                    int index = frame.InstructionIndex;
                    while (index < instructions.Length) {
                        index += instructions[index].Run(frame);
                        frame.InstructionIndex = index;
                    }
                    return;
                } catch (Exception exception) {
                    switch (HandleException(frame, exception)) {
                        case ExceptionHandlingResult.Rethrow: throw;
                        case ExceptionHandlingResult.Continue: continue;
                        case ExceptionHandlingResult.Return: return;
                    }
                }
            }
        }

        private ExceptionHandlingResult HandleException(InterpretedFrame frame, Exception exception) {
            frame.SaveTraceToException(exception);
            ExceptionHandler handler;
            frame.InstructionIndex += GotoHandler(frame, exception, out handler);

            if (handler == null || handler.IsFault) {
                // run finally/fault blocks:
                Run(frame);

                // a finally block can throw an exception caught by a handler, which cancels the previous exception:
                if (frame.InstructionIndex == RethrowOnReturn) {
                    return ExceptionHandlingResult.Rethrow;
                }
                return ExceptionHandlingResult.Return;
            }
            
#if FEATURE_THREAD
            // stay in the current catch so that ThreadAbortException is not rethrown by CLR:
            var abort = exception as ThreadAbortException;
            if (abort != null) {
                _anyAbortException = abort;
                frame.CurrentAbortHandler = handler;
            }
#endif
            while (true) {
                try {
                    var instructions = _instructions.Instructions;
                    int index = frame.InstructionIndex;

                    while (index < instructions.Length) {
                        var curInstr = instructions[index];                        

                        index += curInstr.Run(frame);
                        frame.InstructionIndex = index;
                        
                        if (curInstr is LeaveExceptionHandlerInstruction) {
                            // we've completed handling of this exception
                            return ExceptionHandlingResult.Continue;
                        }
                    }

                    if (frame.InstructionIndex == RethrowOnReturn) {
                        return ExceptionHandlingResult.Rethrow;
                    }

                    return ExceptionHandlingResult.Return;
                } catch (Exception nestedException) {                    
                    switch (HandleException(frame, nestedException)) {
                        case ExceptionHandlingResult.Rethrow: throw;
                        case ExceptionHandlingResult.Continue: continue;
                        case ExceptionHandlingResult.Return: return ExceptionHandlingResult.Return;
                        default: throw Assert.Unreachable;
                    }
                }
            }
        }

        enum ExceptionHandlingResult {
            Rethrow,
            Continue,
            Return
        }

#if FEATURE_THREAD
        // To get to the current AbortReason object on Thread.CurrentThread 
        // we need to use ExceptionState property of any ThreadAbortException instance.
        [ThreadStatic]
        private static ThreadAbortException _anyAbortException = null;

        internal static void AbortThreadIfRequested(InterpretedFrame frame, int targetLabelIndex) {
            var abortHandler = frame.CurrentAbortHandler;
            if (abortHandler != null && !abortHandler.IsInside(frame.Interpreter._labels[targetLabelIndex].Index)) {
                frame.CurrentAbortHandler = null;

                var currentThread = Thread.CurrentThread;
                if ((currentThread.ThreadState & System.Threading.ThreadState.AbortRequested) != 0) {
                    Debug.Assert(_anyAbortException != null);

#if FEATURE_EXCEPTION_STATE
                    // The current abort reason needs to be preserved.
                    currentThread.Abort(_anyAbortException.ExceptionState);
#else
                    currentThread.Abort();
#endif
                }
            }
        }
#else
        internal static void AbortThreadIfRequested(InterpretedFrame frame, int targetLabelIndex) {
            // nop
        }
#endif

        internal ExceptionHandler GetBestHandler(int instructionIndex, Type exceptionType) {
            ExceptionHandler best = null;
            foreach (var handler in _handlers) {
                if (handler.Matches(exceptionType, instructionIndex)) {
                    if (handler.IsBetterThan(best)) {
                        best = handler;
                    }
                }
            }
            return best;
        }

        internal int ReturnAndRethrowLabelIndex {
            get {
                // the last label is "return and rethrow" label:
                Debug.Assert(_labels[_labels.Length - 1].Index == RethrowOnReturn);
                return _labels.Length - 1;
            }
        }

        internal int GotoHandler(InterpretedFrame frame, object exception, out ExceptionHandler handler) {
            handler = GetBestHandler(frame.InstructionIndex, exception.GetType());
            if (handler == null) {
                return frame.Goto(ReturnAndRethrowLabelIndex, Interpreter.NoValue);
            } else {
                return frame.Goto(handler.LabelIndex, exception);
            }
        }
    }
}
