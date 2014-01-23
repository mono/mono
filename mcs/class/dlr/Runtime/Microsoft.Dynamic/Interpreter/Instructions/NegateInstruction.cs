﻿// 
// NegateInstruction.cs:
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
    internal abstract class NegateInstruction : Instruction {
        private static Instruction _Int16, _Int32, _Int64, _UInt16, _UInt32, _Single, _Double;

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        private NegateInstruction() {
        }

        internal sealed class NegateInt32 : NegateInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = ScriptingRuntimeHelpers.Int32ToObject(unchecked(-(Int32)v));
                return 1;
            }
        }

        internal sealed class NegateInt16 : NegateInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (Int16)unchecked(-(Int16)v);
                return 1;
            }
        }

        internal sealed class NegateInt64 : NegateInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (Int64)unchecked(-(Int64)v);
                return 1;
            }
        }

        internal sealed class NegateUInt16 : NegateInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (UInt16)unchecked(-(UInt16)v);
                return 1;

            }
        }

        internal sealed class NegateUInt32 : NegateInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (UInt32)unchecked(-(UInt32)v);
                return 1;

            }
        }

        internal sealed class NegateSingle : NegateInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (Single)unchecked(-(Single)v);
                return 1;

            }
        }

        internal sealed class NegateDouble : NegateInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (Double)unchecked(-(Double)v);
                return 1;

            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new NegateInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new NegateInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new NegateInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new NegateUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new NegateUInt32());
                case TypeCode.Single: return _Single ?? (_Single = new NegateSingle());
                case TypeCode.Double: return _Double ?? (_Double = new NegateDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "Negate()";
        }
    }

    internal abstract class NegateOvfInstruction : Instruction {
        private static Instruction _Int16, _Int32, _Int64, _UInt16, _UInt32, _Single, _Double;

        public override int ConsumedStack { get { return 1; } }
        public override int ProducedStack { get { return 1; } }

        private NegateOvfInstruction() {
        }

        internal sealed class NegateOvfInt32 : NegateOvfInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = ScriptingRuntimeHelpers.Int32ToObject(checked(-(Int32)v));
                return 1;
            }
        }

        internal sealed class NegateOvfInt16 : NegateOvfInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (Int16)checked(-(Int16)v);
                return 1;
            }
        }

        internal sealed class NegateOvfInt64 : NegateOvfInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (Int64)checked(-(Int64)v);
                return 1;
            }
        }

        internal sealed class NegateOvfUInt16 : NegateOvfInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (UInt16)checked(-(UInt16)v);
                return 1;

            }
        }

        internal sealed class NegateOvfUInt32 : NegateOvfInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (UInt32)checked(-(UInt32)v);
                return 1;

            }
        }

        internal sealed class NegateOvfSingle : NegateOvfInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (Single)checked(-(Single)v);
                return 1;

            }
        }

        internal sealed class NegateOvfDouble : NegateOvfInstruction {
            public override int Run(InterpretedFrame frame) {
                object v = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 1] = (Double)checked(-(Double)v);
                return 1;

            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new NegateOvfInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new NegateOvfInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new NegateOvfInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new NegateOvfUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new NegateOvfUInt32());
                case TypeCode.Single: return _Single ?? (_Single = new NegateOvfSingle());
                case TypeCode.Double: return _Double ?? (_Double = new NegateOvfDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "NegateOvf()";
        }
    }
}
