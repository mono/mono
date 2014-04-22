// 
// MulInstruction.cs:
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
    internal abstract class MulInstruction : ArithmeticInstruction {

        private static Instruction _Int16, _Int32, _Int64, _UInt16, _UInt32, _UInt64, _Single, _Double;

        private MulInstruction() {
        }

        internal sealed class MulInt32 : MulInstruction {
            protected override object Calculate (object l, object r)
            {
                return ScriptingRuntimeHelpers.Int32ToObject(unchecked((Int32)l * (Int32)r));
            }
        }

        internal sealed class MulInt16 : MulInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Int16)unchecked((Int16)l * (Int16)r);
            }
        }

        internal sealed class MulInt64 : MulInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Int64)unchecked((Int64)l * (Int64)r);
            }
        }

        internal sealed class MulUInt16 : MulInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt16)unchecked((UInt16)l * (UInt16)r);
            }
        }

        internal sealed class MulUInt32 : MulInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt32)unchecked((UInt32)l * (UInt32)r);
            }
        }

        internal sealed class MulUInt64 : MulInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt64)unchecked((UInt64)l * (UInt64)r);
            }
        }

        internal sealed class MulSingle : MulInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Single)((Single)l * (Single)r);
            }
        }

        internal sealed class MulDouble : MulInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Double)l * (Double)r;
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new MulInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new MulInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new MulInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new MulUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new MulUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new MulUInt64());
                case TypeCode.Single: return _Single ?? (_Single = new MulSingle());
                case TypeCode.Double: return _Double ?? (_Double = new MulDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "Mul()";
        }
    }

    internal abstract class MulOvfInstruction : ArithmeticInstruction {

        private static Instruction _Int16, _Int32, _Int64, _UInt16, _UInt32, _UInt64, _Single, _Double;

        private MulOvfInstruction() {
        }

        internal sealed class MulOvfInt32 : MulOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return ScriptingRuntimeHelpers.Int32ToObject(checked((Int32)l * (Int32)r));
            }
        }

        internal sealed class MulOvfInt16 : MulOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return checked((Int16)((Int16)l * (Int16)r));
            }
        }

        internal sealed class MulOvfInt64 : MulOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return checked((Int64)((Int64)l * (Int64)r));
            }
        }

        internal sealed class MulOvfUInt16 : MulOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return checked((UInt16)((UInt16)l * (UInt16)r));
            }
        }

        internal sealed class MulOvfUInt32 : MulOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return checked((UInt32)((UInt32)l * (UInt32)r));
            }
        }

        internal sealed class MulOvfUInt64 : MulOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return checked((UInt64)((UInt64)l * (UInt64)r));
            }
        }

        internal sealed class MulOvfSingle : MulOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Single)((Single)l * (Single)r);
            }
        }

        internal sealed class MulOvfDouble : MulOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Double)l * (Double)r;
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new MulOvfInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new MulOvfInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new MulOvfInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new MulOvfUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new MulOvfUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new MulOvfUInt64());
                case TypeCode.Single: return _Single ?? (_Single = new MulOvfSingle());
                case TypeCode.Double: return _Double ?? (_Double = new MulOvfDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "MulOvf()";
        }
    }
}
