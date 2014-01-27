// 
// OrInstruction.cs:
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
    internal abstract class OrInstruction : Instruction {
        private static Instruction _Int16, _Int32, _Int64, _UInt16, _UInt32, _UInt64, _Boolean;
        private static Instruction _Int16Lifted, _Int32Lifted, _Int64Lifted, _UInt16Lifted, _UInt32Lifted, _UInt64Lifted, _BooleanLifted;

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        private OrInstruction() {
        }

        internal sealed class OrInt32 : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = frame.Data[frame.StackIndex - 2];
                var r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = ScriptingRuntimeHelpers.Int32ToObject((Int32)l | (Int32)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrInt16 : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = frame.Data[frame.StackIndex - 2];
                var r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int16)((Int16)l | (Int16)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrInt64 : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = frame.Data[frame.StackIndex - 2];
                var r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int64)((Int64)l | (Int64)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrUInt16 : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = frame.Data[frame.StackIndex - 2];
                var r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (UInt16)((UInt16)l | (UInt16)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrUInt32 : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = frame.Data[frame.StackIndex - 2];
                var r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (UInt32)((UInt32)l | (UInt32)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrUInt64 : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = frame.Data[frame.StackIndex - 2];
                var r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (UInt64)((UInt64)l | (UInt64)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrBoolean : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = frame.Data[frame.StackIndex - 2];
                var r = frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Boolean)((Boolean)l | (Boolean)r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrInt32Lifted : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (Int32?)frame.Data[frame.StackIndex - 2];
                var r = (Int32?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int32?)(l | r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrInt16Lifted : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (Int16?)frame.Data[frame.StackIndex - 2];
                var r = (Int16?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int16?)(l | r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrInt64Lifted : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (Int64?)frame.Data[frame.StackIndex - 2];
                var r = (Int64?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Int64?)(l | r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrUInt16Lifted : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (UInt16?)frame.Data[frame.StackIndex - 2];
                var r = (UInt16?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (UInt16?)(l | r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrUInt32Lifted : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (UInt32?)frame.Data[frame.StackIndex - 2];
                var r = (UInt32?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (UInt32?)(l | r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrUInt64Lifted : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (UInt64?)frame.Data[frame.StackIndex - 2];
                var r = (UInt64?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (UInt64?)(l | r);
                frame.StackIndex--;
                return 1;
            }
        }

        internal sealed class OrBooleanLifted : OrInstruction {
            public override int Run(InterpretedFrame frame) {
                var l = (Boolean?)frame.Data[frame.StackIndex - 2];
                var r = (Boolean?)frame.Data[frame.StackIndex - 1];
                frame.Data[frame.StackIndex - 2] = (Boolean?)(l | r);
                frame.StackIndex--;
                return 1;
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new OrInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new OrInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new OrInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new OrUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new OrUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new OrUInt64());
                case TypeCode.Boolean: return _Boolean ?? (_Boolean = new OrBoolean());

                default:
                    throw Assert.Unreachable;
            }
        }

        public static Instruction CreateLifted(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16Lifted ?? (_Int16Lifted = new OrInt16Lifted());
                case TypeCode.Int32: return _Int32Lifted ?? (_Int32Lifted = new OrInt32Lifted());
                case TypeCode.Int64: return _Int64Lifted ?? (_Int64Lifted = new OrInt64Lifted());
                case TypeCode.UInt16: return _UInt16Lifted ?? (_UInt16Lifted = new OrUInt16Lifted());
                case TypeCode.UInt32: return _UInt32Lifted ?? (_UInt32Lifted = new OrUInt32Lifted());
                case TypeCode.UInt64: return _UInt64Lifted ?? (_UInt64Lifted = new OrUInt64Lifted());
                case TypeCode.Boolean: return _BooleanLifted ?? (_BooleanLifted = new OrBooleanLifted());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "Or()";
        }
    }
}
