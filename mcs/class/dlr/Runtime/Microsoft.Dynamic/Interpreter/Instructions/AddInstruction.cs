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
using System.Diagnostics;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpreter {
    internal abstract class AddInstruction : ArithmeticInstruction {

        private static Instruction _Int16, _Int32, _Int64, _UInt16, _UInt32, _UInt64, _Single, _Double;

        private AddInstruction() {
        }

        internal sealed class AddInt32 : AddInstruction {
            protected override object Calculate (object l, object r)
            {
                return ScriptingRuntimeHelpers.Int32ToObject(unchecked((Int32)l + (Int32)r));
            }
        }

        internal sealed class AddInt16 : AddInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Int16)unchecked((Int16)l + (Int16)r);
            }
        }

        internal sealed class AddInt64 : AddInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Int64)unchecked((Int64)l + (Int64)r);
            }
        }

        internal sealed class AddUInt16 : AddInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt16)unchecked((UInt16)l + (UInt16)r);
            }
        }

        internal sealed class AddUInt32 : AddInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt32)unchecked((UInt32)l + (UInt32)r);
            }
        }

        internal sealed class AddUInt64 : AddInstruction {
            protected override object Calculate (object l, object r)
            {
                return (UInt64)unchecked((UInt64)l + (UInt64)r);
            }
        }

        internal sealed class AddSingle : AddInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Single)((Single)l + (Single)r);
            }
        }

        internal sealed class AddDouble : AddInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Double)l + (Double)r;
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new AddInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new AddInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new AddInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new AddUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new AddUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new AddUInt64());
                case TypeCode.Single: return _Single ?? (_Single = new AddSingle());
                case TypeCode.Double: return _Double ?? (_Double = new AddDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "Add()";
        }
    }

    internal abstract class AddOvfInstruction : ArithmeticInstruction {

        private static Instruction _Int16, _Int32, _Int64, _UInt16, _UInt32, _UInt64, _Single, _Double;

        private AddOvfInstruction() {
        }

        internal sealed class AddOvfInt32 : AddOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return ScriptingRuntimeHelpers.Int32ToObject(checked((Int32)l + (Int32)r));
            }
        }

        internal sealed class AddOvfInt16 : AddOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return checked((Int16)((Int16)l + (Int16)r));
            }
        }

        internal sealed class AddOvfInt64 : AddOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return checked((Int64)((Int64)l + (Int64)r));
            }
        }

        internal sealed class AddOvfUInt16 : AddOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return checked((UInt16)((UInt16)l + (UInt16)r));
            }
        }

        internal sealed class AddOvfUInt32 : AddOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return checked((UInt32)((UInt32)l + (UInt32)r));
            }
        }

        internal sealed class AddOvfUInt64 : AddOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return checked((UInt64)((UInt64)l + (UInt64)r));
            }
        }

        internal sealed class AddOvfSingle : AddOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Single)((Single)l + (Single)r);
            }
        }

        internal sealed class AddOvfDouble : AddOvfInstruction {
            protected override object Calculate (object l, object r)
            {
                return (Double)l + (Double)r;
            }
        }

        public static Instruction Create(Type type) {
            Debug.Assert(!type.IsEnum());
            switch (type.GetTypeCode()) {
                case TypeCode.Int16: return _Int16 ?? (_Int16 = new AddOvfInt16());
                case TypeCode.Int32: return _Int32 ?? (_Int32 = new AddOvfInt32());
                case TypeCode.Int64: return _Int64 ?? (_Int64 = new AddOvfInt64());
                case TypeCode.UInt16: return _UInt16 ?? (_UInt16 = new AddOvfUInt16());
                case TypeCode.UInt32: return _UInt32 ?? (_UInt32 = new AddOvfUInt32());
                case TypeCode.UInt64: return _UInt64 ?? (_UInt64 = new AddOvfUInt64());
                case TypeCode.Single: return _Single ?? (_Single = new AddOvfSingle());
                case TypeCode.Double: return _Double ?? (_Double = new AddOvfDouble());

                default:
                    throw Assert.Unreachable;
            }
        }

        public override string ToString() {
            return "AddOvf()";
        }
    }
}
