// 
// ShlInstruction.cs:
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
    internal abstract class ShlInstruction : Instruction {
        private static Instruction _Int16, _Int32, _Int64, _UInt16, _UInt32, _UInt64;
        private static Instruction _Int16Lifted, _Int32Lifted, _Int64Lifted, _UInt16Lifted, _UInt32Lifted, _UInt64Lifted;

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        private ShlInstruction() {
        }

        internal sealed class ShlInt32 : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ScriptingRuntimeHelpers.Int32ToObject((Int32)l << (Int32)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class ShlInt16 : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int32)((Int16)l << (Int32)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class ShlInt64 : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int64)((Int64)l << (Int32)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class ShlUInt16 : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int32)((UInt16)l << (Int32)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class ShlUInt32 : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (UInt32)((UInt32)l << (Int32)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class ShlUInt64 : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (UInt64)((UInt64)l << (Int32)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class ShlInt32Lifted : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (Int32?)frame.Data[frame.StackIndex - 2];
                var r = (Int32?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int32?)(l << r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class ShlInt16Lifted : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (Int16?)frame.Data[frame.StackIndex - 2];
                var r = (Int32?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int32)(l << r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class ShlInt64Lifted : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (Int64?)frame.Data[frame.StackIndex - 2];
                var r = (Int32?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int64?)(l << r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class ShlUInt16Lifted : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (UInt16?)frame.Data[frame.StackIndex - 2];
                var r = (Int32?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int32?)(l << r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class ShlUInt32Lifted : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (UInt32?)frame.Data[frame.StackIndex - 2];
                var r = (Int32?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (UInt32?)(l << r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class ShlUInt64Lifted : ShlInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (UInt64?)frame.Data[frame.StackIndex - 2];
                var r = (Int32?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (UInt64?)(l << r);
                frame.StackIndex--;
                return 1;
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new ShlInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new ShlInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new ShlInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new ShlUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new ShlUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new ShlUInt64());

                default:
                    throw Assert.Unreachable;
            }
        }

        public static Instruction CreateLifted(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16Lifted ?? (_Int16Lifted = new ShlInt16Lifted());
                case TypeCode.Int32: return _Int32Lifted ?? (_Int32Lifted = new ShlInt32Lifted());
                case TypeCode.Int64: return _Int64Lifted ?? (_Int64Lifted = new ShlInt64Lifted());
                case TypeCode.UInt16: return _UInt16Lifted ?? (_UInt16Lifted = new ShlUInt16Lifted());
                case TypeCode.UInt32: return _UInt32Lifted ?? (_UInt32Lifted = new ShlUInt32Lifted());
                case TypeCode.UInt64: return _UInt64Lifted ?? (_UInt64Lifted = new ShlUInt64Lifted());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "Shl()";
        }
    }
}
