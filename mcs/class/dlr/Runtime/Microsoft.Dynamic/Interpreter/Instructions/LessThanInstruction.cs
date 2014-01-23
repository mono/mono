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
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    public abstract class LessThanInstruction : Instruction {
        private static Instruction _SByte, _Int16, _Char, _Int32, _Int64, _Byte, _UInt16, _UInt32, _UInt64, _Single, _Double;

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        private LessThanInstruction() {
        }

        internal sealed class LessThanSByte : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                SByte right = (SByte)frame.Pop();
                frame.Push(((SByte)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanInt16 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Int16 right = (Int16)frame.Pop();
                frame.Push(((Int16)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanChar : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Char right = (Char)frame.Pop();
                frame.Push(((Char)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanInt32 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Int32 right = (Int32)frame.Pop();
                frame.Push(((Int32)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanInt64 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Int64 right = (Int64)frame.Pop();
                frame.Push(((Int64)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanByte : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Byte right = (Byte)frame.Pop();
                frame.Push(((Byte)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanUInt16 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                UInt16 right = (UInt16)frame.Pop();
                frame.Push(((UInt16)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanUInt32 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                UInt32 right = (UInt32)frame.Pop();
                frame.Push(((UInt32)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanUInt64 : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                UInt64 right = (UInt64)frame.Pop();
                frame.Push(((UInt64)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanSingle : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Single right = (Single)frame.Pop();
                frame.Push(((Single)frame.Pop()) < right);
                return +1;
            }
        }

        internal sealed class LessThanDouble : LessThanInstruction {
            public override int Run(InterpretedFrame frame) {
                Double right = (Double)frame.Pop();
                frame.Push(((Double)frame.Pop()) < right);
                return +1;
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.SByte: return _SByte ?? (_SByte = new LessThanSByte());
                case TypeCode.Byte: return _Byte ?? (_Byte = new LessThanByte());
                case TypeCode.Char: return _Char ?? (_Char = new LessThanChar());
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new LessThanInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new LessThanInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new LessThanInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new LessThanUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new LessThanUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new LessThanUInt64());
                case TypeCode.Single: return _Single ?? (_Single = new LessThanSingle());
                case TypeCode.Double: return _Double ?? (_Double = new LessThanDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "LessThan()";
        }
    }
}
