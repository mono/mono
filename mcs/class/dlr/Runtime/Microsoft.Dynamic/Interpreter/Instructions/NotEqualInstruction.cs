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
    internal abstract class NotEqualInstruction : Instruction {
        // Perf: EqualityComparer<T> but is 3/2 to 2 times slower.
        private static Instruction _Reference, _Boolean, _SByte, _Int16, _Char, _Int32, _Int64, _Byte, _UInt16, _UInt32, _UInt64, _Single, _Double;
        private static Instruction _BooleanLifted, _SByteLifted, _Int16Lifted, _CharLifted, _Int32Lifted, _Int64Lifted,
            _ByteLifted, _UInt16Lifted, _UInt32Lifted, _UInt64Lifted, _SingleLifted, _DoubleLifted;

        public override int ConsumedStack { get { return 2; } }
        public override int ProducedStack { get { return 1; } }

        private NotEqualInstruction() {
        }

        public bool LiftedToNull { get; set; }

        internal sealed class NotEqualBoolean : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (Boolean)l != (Boolean)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class NotEqualSByte : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (SByte)l != (SByte)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class NotEqualInt16 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (Int16)l != (Int16)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class NotEqualChar : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (Char)l != (Char)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class NotEqualInt32 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (Int32)l != (Int32)r;

                frame.StackIndex--;
                return +1;            }
        }

        internal sealed class NotEqualInt64 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (Int64)l != (Int64)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class NotEqualByte : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (Byte)l != (Byte)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class NotEqualUInt16 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (UInt16)l != (UInt16)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class NotEqualUInt32 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (UInt32)l != (UInt32)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class NotEqualUInt64 : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (UInt64)l != (UInt64)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class NotEqualSingle : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (Single)l != (Single)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class NotEqualDouble : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                object l = frame.Data[frame.StackIndex - 2];
                object r = frame.Data[frame.StackIndex - 1];
                if (l == null || r == null)
                    frame.Data[frame.StackIndex - 2] = LiftedToNull ? (object) null : (object) l != r;
                else
                    frame.Data[frame.StackIndex - 2] = (Double)l != (Double)r;

                frame.StackIndex--;
                return +1;
            }
        }

        internal sealed class NotEqualReference : NotEqualInstruction {
            public override int Run(InterpretedFrame frame) {
                frame.Push(frame.Pop() != frame.Pop());
                return +1;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static Instruction Create(Type type) {
            // Boxed enums can be unboxed as their underlying types:
            switch ((type.IsEnum() ? Enum.GetUnderlyingType(type) : type).GetTypeCode()) {
                case TypeCode.Boolean: return _Boolean ?? (_Boolean = new NotEqualBoolean());
                case TypeCode.SByte: return _SByte ?? (_SByte = new NotEqualSByte());
                case TypeCode.Byte: return _Byte ?? (_Byte = new NotEqualByte());
                case TypeCode.Char: return _Char ?? (_Char = new NotEqualChar());
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new NotEqualInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new NotEqualInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new NotEqualInt64());

                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new NotEqualInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new NotEqualInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new NotEqualInt64());

                case TypeCode.Single: return _Single ?? (_Single = new NotEqualSingle());
                case TypeCode.Double: return _Double ?? (_Double = new NotEqualDouble());

                case TypeCode.Object:
                    if (!type.IsValueType()) {
                        return _Reference ?? (_Reference = new NotEqualReference());
                    }
                    // TODO: Nullable<T>
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();
            }
        }

        public static Instruction CreateLifted(Type type) {
            // Boxed enums can be unboxed as their underlying types:
            switch ((type.IsEnum() ? Enum.GetUnderlyingType(type) : type).GetTypeCode()) {
                case TypeCode.Boolean: return _BooleanLifted ?? (_BooleanLifted = new NotEqualBoolean() { LiftedToNull = true });
                case TypeCode.SByte: return _SByteLifted ?? (_SByteLifted = new NotEqualSByte() { LiftedToNull = true });
                case TypeCode.Byte: return _ByteLifted ?? (_ByteLifted = new NotEqualByte() { LiftedToNull = true });
                case TypeCode.Char: return _CharLifted ?? (_CharLifted = new NotEqualChar() { LiftedToNull = true });
                case TypeCode.Int16: return _Int16Lifted ?? (_Int16Lifted = new NotEqualInt16() { LiftedToNull = true });
                case TypeCode.Int32: return _Int32Lifted ?? (_Int32Lifted = new NotEqualInt32() { LiftedToNull = true });
                case TypeCode.Int64: return _Int64Lifted ?? (_Int64Lifted = new NotEqualInt64() { LiftedToNull = true });

                case TypeCode.UInt16: return _UInt16Lifted ?? (_UInt16Lifted = new NotEqualInt16() { LiftedToNull = true });
                case TypeCode.UInt32: return _UInt32Lifted ?? (_UInt32Lifted = new NotEqualInt32() { LiftedToNull = true });
                case TypeCode.UInt64: return _UInt64Lifted ?? (_UInt64Lifted = new NotEqualInt64() { LiftedToNull = true });

                case TypeCode.Single: return _SingleLifted ?? (_SingleLifted = new NotEqualSingle() { LiftedToNull = true });
                case TypeCode.Double: return _DoubleLifted ?? (_DoubleLifted = new NotEqualDouble() { LiftedToNull = true });

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "NotEqual()";
        }
    }
}

