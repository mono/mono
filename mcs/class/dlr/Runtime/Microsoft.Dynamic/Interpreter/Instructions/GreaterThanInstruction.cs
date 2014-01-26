// 
// GreaterThanInstruction.cs:
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    public abstract class GreaterThanInstruction : Instruction {
        private static Instruction _SByte, _Int16, _Char, _Int32, _Int64, _Byte, _UInt16, _UInt32, _UInt64, _Single, _Double;
        private static Instruction _SByteLifted, _Int16Lifted, _CharLifted, _Int32Lifted, _Int64Lifted, _ByteLifted, _UInt16Lifted, _UInt32Lifted, _UInt64Lifted, _SingleLifted, _DoubleLifted;

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        private GreaterThanInstruction() {
        }

        public bool LiftedToNull { get; set; }

        internal sealed class GreaterThanSByte : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) false;
                else
                    frame.Data[frame.StackIndex - 2] = (SByte)l > (SByte)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class GreaterThanInt16 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) false;
                else
                    frame.Data[frame.StackIndex - 2] = (Int16)l > (Int16)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class GreaterThanChar : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) false;
                else
                    frame.Data[frame.StackIndex - 2] = (Char)l > (Char)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class GreaterThanInt32 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) false;
                else
                    frame.Data[frame.StackIndex - 2] = (Int32)l > (Int32)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class GreaterThanInt64 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) false;
                else
                    frame.Data[frame.StackIndex - 2] = (Int64)l > (Int64)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class GreaterThanByte : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) false;
                else
                    frame.Data[frame.StackIndex - 2] = (Byte)l > (Byte)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class GreaterThanUInt16 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) false;
                else
                    frame.Data[frame.StackIndex - 2] = (UInt16)l > (UInt16)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class GreaterThanUInt32 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) false;
                else
                    frame.Data[frame.StackIndex - 2] = (UInt32)l > (UInt32)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class GreaterThanUInt64 : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) false;
                else
                    frame.Data[frame.StackIndex - 2] = (UInt64)l > (UInt64)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class GreaterThanSingle : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) false;
                else
                    frame.Data[frame.StackIndex - 2] = (Single)l > (Single)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class GreaterThanDouble : GreaterThanInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) false;
                else
                    frame.Data[frame.StackIndex - 2] = (Double)l > (Double)r;

                frame.StackIndex--;
                return +1;
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.SByte: return _SByte ?? (_SByte = new GreaterThanSByte());
                case TypeCode.Byte: return _Byte ?? (_Byte = new GreaterThanByte());
                case TypeCode.Char: return _Char ?? (_Char = new GreaterThanChar());
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new GreaterThanInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new GreaterThanInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new GreaterThanInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new GreaterThanUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new GreaterThanUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new GreaterThanUInt64());
                case TypeCode.Single: return _Single ?? (_Single = new GreaterThanSingle());
                case TypeCode.Double: return _Double ?? (_Double = new GreaterThanDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public static Instruction CreateLifted(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.SByte: return _SByteLifted ?? (_SByteLifted = new GreaterThanSByte() { LiftedToNull = true });
                case TypeCode.Byte: return _ByteLifted ?? (_ByteLifted = new GreaterThanByte() { LiftedToNull = true });
                case TypeCode.Char: return _CharLifted ?? (_CharLifted = new GreaterThanChar() { LiftedToNull = true });
                case TypeCode.Int16: return _Int16Lifted ?? (_Int16Lifted = new GreaterThanInt16() { LiftedToNull = true });
                case TypeCode.Int32: return _Int32Lifted ?? (_Int32Lifted = new GreaterThanInt32() { LiftedToNull = true });
                case TypeCode.Int64: return _Int64Lifted ?? (_Int64Lifted = new GreaterThanInt64() { LiftedToNull = true });
                case TypeCode.UInt16: return _UInt16Lifted ?? (_UInt16Lifted = new GreaterThanUInt16() { LiftedToNull = true });
                case TypeCode.UInt32: return _UInt32Lifted ?? (_UInt32Lifted = new GreaterThanUInt32() { LiftedToNull = true });
                case TypeCode.UInt64: return _UInt64Lifted ?? (_UInt64Lifted = new GreaterThanUInt64() { LiftedToNull = true });
                case TypeCode.Single: return _SingleLifted ?? (_SingleLifted = new GreaterThanSingle() { LiftedToNull = true });
                case TypeCode.Double: return _DoubleLifted ?? (_DoubleLifted = new GreaterThanDouble() { LiftedToNull = true });

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "GreaterThan()";
        }
    }
}
