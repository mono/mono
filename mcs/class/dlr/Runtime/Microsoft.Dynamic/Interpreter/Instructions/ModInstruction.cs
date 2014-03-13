// 
// ModInstruction.cs:
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
    internal abstract class ModInstruction : ArithmeticInstruction {
        private static Instruction _Int16, _Int32, _Int64, _UInt16, _UInt32, _UInt64, _Single, _Double;

        private ModInstruction() {
        }

        internal sealed class ModInt32 : ModInstruction {
            protected override object Calculate (object l, object r)
            {
                return ScriptingRuntimeHelpers.Int32ToObject(unchecked((Int32)l % (Int32)r));
            }
        }

        internal sealed class ModInt16 : ModInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Int16)unchecked((Int16)l % (Int16)r);
            }
        }

        internal sealed class ModInt64 : ModInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Int64)unchecked((Int64)l % (Int64)r);
            }
        }

        internal sealed class ModUInt16 : ModInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt16)unchecked((UInt16)l % (UInt16)r);
            }
        }

        internal sealed class ModUInt32 : ModInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt32)unchecked((UInt32)l % (UInt32)r);
            }
        }

        internal sealed class ModUInt64 : ModInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt64)unchecked((UInt64)l % (UInt64)r);
            }
        }

        internal sealed class ModSingle : ModInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Single)((Single)l % (Single)r);
            }
        }

        internal sealed class ModDouble : ModInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Double)l % (Double)r;
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new ModInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new ModInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new ModInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new ModUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new ModUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new ModUInt64());
                case TypeCode.Single: return _Single ?? (_Single = new ModSingle());
                case TypeCode.Double: return _Double ?? (_Double = new ModDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "Mod()";
        }
    }
}
