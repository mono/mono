// 
// GreaterThanOrEqualInstruction.cs:
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
    abstract class GreaterThanOrEqualInstruction : ComparisonInstruction {
        private static Instruction _SByte, _Int16, _Char, _Int32, _Int64, _Byte, _UInt16, _UInt32, _UInt64, _Single, _Double;
        private static Instruction _SByteLifted, _Int16Lifted, _CharLifted, _Int32Lifted, _Int64Lifted, _ByteLifted, _UInt16Lifted, _UInt32Lifted, _UInt64Lifted, _SingleLifted, _DoubleLifted;

        private GreaterThanOrEqualInstruction() {
        }

        protected override object DoNullComparison (object l, object r)
        {
            return LiftedToNull ? (object) null : (object) false;
        }

        internal sealed class GreaterThanOrEqualSByte : GreaterThanOrEqualInstruction {
            protected override object DoCalculate (object l, object r)
            {
                return (SByte)l >= (SByte)r;
            }
        }

        internal sealed class GreaterThanOrEqualInt16 : GreaterThanOrEqualInstruction {
            protected override object DoCalculate (object l, object r)
            {
                return (Int16)l >= (Int16)r;
            }
        }

        internal sealed class GreaterThanOrEqualChar : GreaterThanOrEqualInstruction {
            protected override object DoCalculate (object l, object r)
            {
                return (Char)l >= (Char)r;
            }
        }

        internal sealed class GreaterThanOrEqualInt32 : GreaterThanOrEqualInstruction {
            protected override object DoCalculate (object l, object r)
            {
                return (Int32)l >= (Int32)r;
            }
        }

        internal sealed class GreaterThanOrEqualInt64 : GreaterThanOrEqualInstruction {
            protected override object DoCalculate (object l, object r)
            {
                return (Int64)l >= (Int64)r;
            }
        }

        internal sealed class GreaterThanOrEqualByte : GreaterThanOrEqualInstruction {
            protected override object DoCalculate (object l, object r)
            {
                return (Byte)l >= (Byte)r;
            }
        }

        internal sealed class GreaterThanOrEqualUInt16 : GreaterThanOrEqualInstruction {
            protected override object DoCalculate (object l, object r)
            {
                return (UInt16)l >= (UInt16)r;
            }
        }

        internal sealed class GreaterThanOrEqualUInt32 : GreaterThanOrEqualInstruction {
            protected override object DoCalculate (object l, object r)
            {
                return (UInt32)l >= (UInt32)r;
            }
        }

        internal sealed class GreaterThanOrEqualUInt64 : GreaterThanOrEqualInstruction {
            protected override object DoCalculate (object l, object r)
            {
                return (UInt64)l >= (UInt64)r;
            }
        }

        internal sealed class GreaterThanOrEqualSingle : GreaterThanOrEqualInstruction {
            protected override object DoCalculate (object l, object r)
            {
                return (Single)l >= (Single)r;
            }
        }

        internal sealed class GreaterThanOrEqualDouble : GreaterThanOrEqualInstruction {
            protected override object DoCalculate (object l, object r)
            {
                return (Double)l >= (Double)r;
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.SByte: return _SByte ?? (_SByte = new GreaterThanOrEqualSByte());
                case TypeCode.Byte: return _Byte ?? (_Byte = new GreaterThanOrEqualByte());
                case TypeCode.Char: return _Char ?? (_Char = new GreaterThanOrEqualChar());
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new GreaterThanOrEqualInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new GreaterThanOrEqualInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new GreaterThanOrEqualInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new GreaterThanOrEqualUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new GreaterThanOrEqualUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new GreaterThanOrEqualUInt64());
                case TypeCode.Single: return _Single ?? (_Single = new GreaterThanOrEqualSingle());
                case TypeCode.Double: return _Double ?? (_Double = new GreaterThanOrEqualDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public static Instruction CreateLifted(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.SByte: return _SByteLifted ?? (_SByteLifted = new GreaterThanOrEqualSByte() { LiftedToNull = true });
                case TypeCode.Byte: return _ByteLifted ?? (_ByteLifted = new GreaterThanOrEqualByte() { LiftedToNull = true });
                case TypeCode.Char: return _CharLifted ?? (_CharLifted = new GreaterThanOrEqualChar() { LiftedToNull = true });
                case TypeCode.Int16: return _Int16Lifted ?? (_Int16Lifted = new GreaterThanOrEqualInt16() { LiftedToNull = true });
                case TypeCode.Int32: return _Int32Lifted ?? (_Int32Lifted = new GreaterThanOrEqualInt32() { LiftedToNull = true });
                case TypeCode.Int64: return _Int64Lifted ?? (_Int64Lifted = new GreaterThanOrEqualInt64() { LiftedToNull = true });
                case TypeCode.UInt16: return _UInt16Lifted ?? (_UInt16Lifted = new GreaterThanOrEqualUInt16() { LiftedToNull = true });
                case TypeCode.UInt32: return _UInt32Lifted ?? (_UInt32Lifted = new GreaterThanOrEqualUInt32() { LiftedToNull = true });
                case TypeCode.UInt64: return _UInt64Lifted ?? (_UInt64Lifted = new GreaterThanOrEqualUInt64() { LiftedToNull = true });
                case TypeCode.Single: return _SingleLifted ?? (_SingleLifted = new GreaterThanOrEqualSingle() { LiftedToNull = true });
                case TypeCode.Double: return _DoubleLifted ?? (_DoubleLifted = new GreaterThanOrEqualDouble() { LiftedToNull = true });

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "GreaterThanOrEqual()";
        }
    }
}
