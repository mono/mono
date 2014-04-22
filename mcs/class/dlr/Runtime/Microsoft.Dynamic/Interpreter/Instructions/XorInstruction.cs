// 
// XorInstruction.cs:
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
    internal abstract class XorInstruction : ArithmeticInstruction {
        private static Instruction _Int16, _Int32, _Int64, _UInt16, _UInt32, _UInt64, _Boolean;
        private static Instruction _Int16Lifted, _Int32Lifted, _Int64Lifted, _UInt16Lifted, _UInt32Lifted, _UInt64Lifted, _BooleanLifted;

        private XorInstruction() {
        }

        internal sealed class XorInt32 : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return ScriptingRuntimeHelpers.Int32ToObject((Int32)l ^ (Int32)r);
            }
        }

        internal sealed class XorInt16 : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Int16)((Int16)l ^ (Int16)r);
            }
        }

        internal sealed class XorInt64 : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Int64)((Int64)l ^ (Int64)r);
            }
        }

        internal sealed class XorUInt16 : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt16)((UInt16)l ^ (UInt16)r);
            }
        }

        internal sealed class XorUInt32 : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return  (UInt32)((UInt32)l ^ (UInt32)r);
            }
        }

        internal sealed class XorUInt64 : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt64)((UInt64)l ^ (UInt64)r);
            }
        }

        internal sealed class XorBoolean : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Boolean)((Boolean)l ^ (Boolean)r);
            }
        }

        internal sealed class XorInt32Lifted : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Int32?)((Int32?)l ^ (Int32?)r);
            }
        }

        internal sealed class XorInt16Lifted : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return  (Int16?)((Int16?)l ^ (Int16?)r);
            }
        }

        internal sealed class XorInt64Lifted : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Int64?)((Int64?)l ^ (Int64?)r);
            }
        }

        internal sealed class XorUInt16Lifted : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt16?)((UInt16?)l ^ (UInt16?)r);
            }
        }

        internal sealed class XorUInt32Lifted : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt32?)((UInt32?)l ^ (UInt32?)r);
            }
        }

        internal sealed class XorUInt64Lifted : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt64?)((UInt64?)l ^ (UInt64?)r);
            }
        }

        internal sealed class XorBooleanLifted : XorInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Boolean?)((Boolean?)l ^ (Boolean?)r);
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new XorInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new XorInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new XorInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new XorUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new XorUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new XorUInt64());
                case TypeCode.Boolean: return _Boolean ?? (_Boolean = new XorBoolean());

                default:
                    throw Assert.Unreachable;
            }
        }

        public static Instruction CreateLifted(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16Lifted ?? (_Int16Lifted = new XorInt16Lifted());
                case TypeCode.Int32: return _Int32Lifted ?? (_Int32Lifted = new XorInt32Lifted());
                case TypeCode.Int64: return _Int64Lifted ?? (_Int64Lifted = new XorInt64Lifted());
                case TypeCode.UInt16: return _UInt16Lifted ?? (_UInt16Lifted = new XorUInt16Lifted());
                case TypeCode.UInt32: return _UInt32Lifted ?? (_UInt32Lifted = new XorUInt32Lifted());
                case TypeCode.UInt64: return _UInt64Lifted ?? (_UInt64Lifted = new XorUInt64Lifted());
                case TypeCode.Boolean: return _BooleanLifted ?? (_BooleanLifted = new XorBooleanLifted());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "Xor()";
        }
    }
}
