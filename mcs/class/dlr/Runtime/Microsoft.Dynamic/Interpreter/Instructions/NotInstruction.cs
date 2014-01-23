﻿// 
// NotInstruction.cs:
//
// Authors: Marek Safar (marek.safar@gmail.com)
//     
// Copyright 2014 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//

using System;
using System.Diagnostics;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    internal abstract class NotInstruction : Instruction {
        private static Instruction _Int16, _Int32, _Int64, _UInt16, _UInt32, _UInt64, _Boolean;

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        private NotInstruction() {
        }

        internal sealed class NotBoolean : NotInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push((bool)frame.Pop() ? ScriptingRuntimeHelpers.False : ScriptingRuntimeHelpers.True);
                return 1;
            }
        }

        internal sealed class NotInt32 : NotInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = ScriptingRuntimeHelpers.Int32ToObject(~(Int32)v);
                return 1;
            }
        }

        internal sealed class NotInt16 : NotInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (Int16)(~(Int16)v);
                return 1;
            }
        }

        internal sealed class NotInt64 : NotInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (Int64)(~(Int64)v);
                return 1;
            }
        }

        internal sealed class NotUInt16 : NotInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (UInt64)(~(UInt64)v);
                return 1;
            }
        }

        internal sealed class NotUInt32 : NotInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (UInt32)(~(UInt32)v);
                return 1;
            }
        }

        internal sealed class NotUInt64 : NotInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (UInt64)(~(UInt64)v);
                return 1;
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new NotInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new NotInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new NotInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new NotUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new NotUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new NotUInt64());
                case TypeCode.Boolean: return _Boolean ?? (_Boolean = new NotBoolean());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "Not()";
        }
    }
}
