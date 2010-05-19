//
// ConstantExpression.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//   Miguel de Icaza (miguel@novell.com)
//
// Some code is based on the Mono C# compiler:
//   Marek Safar (marek.safar@seznam.cz)
//   Martin Baulig (martin@ximian.com)
//
// (C) 2001-2008 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	public sealed class ConstantExpression : Expression {
		object value;

		public object Value {
			get { return value; }
		}

		internal ConstantExpression (object value, Type type)
			: base (ExpressionType.Constant, type)
		{
			this.value = value;
		}

		internal override void Emit (EmitContext ec)
		{
			if (Type.IsNullable ()) {
				EmitNullableConstant (ec, Type, value);
				return;
			}

			EmitConstant (ec, Type, value);
		}

		void EmitNullableConstant (EmitContext ec, Type type, object value)
		{
			if (value == null) {
				var ig = ec.ig;
				var local = ig.DeclareLocal (type);
				ig.Emit (OpCodes.Ldloca, local);
				ig.Emit (OpCodes.Initobj, type);
				ig.Emit (OpCodes.Ldloc, local);
			} else {
				EmitConstant (ec, type.GetFirstGenericArgument (), value);
				ec.EmitNullableNew (type);
			}
		}

		void EmitConstant (EmitContext ec, Type type, object value)
		{
			var ig = ec.ig;

			switch (Type.GetTypeCode (type)){
			case TypeCode.Byte:
				ig.Emit (OpCodes.Ldc_I4, (int) ((byte)value));
				return;

			case TypeCode.SByte:
				ig.Emit (OpCodes.Ldc_I4, (int) ((sbyte)value));
				return;

			case TypeCode.Int16:
				ig.Emit (OpCodes.Ldc_I4, (int) ((short)value));
				return;

			case TypeCode.UInt16:
				ig.Emit (OpCodes.Ldc_I4, (int) ((ushort)value));
				return;

			case TypeCode.Int32:
				ig.Emit (OpCodes.Ldc_I4, (int) value);
				return;

			case TypeCode.UInt32:
				ig.Emit (OpCodes.Ldc_I4, unchecked ((int) ((uint)Value)));
				return;

			case TypeCode.Int64:
				ig.Emit (OpCodes.Ldc_I8, (long) value);
				return;

			case TypeCode.UInt64:
				ig.Emit (OpCodes.Ldc_I8, unchecked ((long) ((ulong)value)));
				return;

			case TypeCode.Boolean:
				if ((bool) Value)
					ig.Emit (OpCodes.Ldc_I4_1);
				else
					ec.ig.Emit (OpCodes.Ldc_I4_0);
				return;

			case TypeCode.Char:
				ig.Emit (OpCodes.Ldc_I4, (int) ((char) value));
				return;

			case TypeCode.Single:
				ig.Emit (OpCodes.Ldc_R4, (float) value);
				return;

			case TypeCode.Double:
				ig.Emit (OpCodes.Ldc_R8, (double) value);
				return;

			case TypeCode.Decimal: {
				Decimal v = (decimal) value;
				int [] words = Decimal.GetBits (v);
				int power = (words [3] >> 16) & 0xff;
				Type ti = typeof (int);

				if (power == 0 && v <= int.MaxValue && v >= int.MinValue) {
					ig.Emit (OpCodes.Ldc_I4, (int) v);

					ig.Emit (OpCodes.Newobj, typeof (Decimal).GetConstructor (new Type [1] { ti }));
					return;
				}
				ig.Emit (OpCodes.Ldc_I4, words [0]);
				ig.Emit (OpCodes.Ldc_I4, words [1]);
				ig.Emit (OpCodes.Ldc_I4, words [2]);
				// sign
				ig.Emit (OpCodes.Ldc_I4, words [3] >> 31);

				// power
				ig.Emit (OpCodes.Ldc_I4, power);

				ig.Emit (OpCodes.Newobj, typeof (Decimal).GetConstructor (new Type [5] { ti, ti, ti, typeof(bool), typeof(byte) }));
				return;
			}

			case TypeCode.DateTime: {
				var date = (DateTime) value;
				var local = ig.DeclareLocal (typeof (DateTime));

				ig.Emit (OpCodes.Ldloca, local);
				ig.Emit (OpCodes.Ldc_I8, date.Ticks);
				ig.Emit (OpCodes.Ldc_I4, (int) date.Kind);
				ig.Emit (OpCodes.Call, typeof (DateTime).GetConstructor (new [] { typeof (long), typeof (DateTimeKind) }));
				ig.Emit (OpCodes.Ldloc, local);

				return;
			}

			case TypeCode.DBNull:
				ig.Emit (OpCodes.Ldsfld, typeof (DBNull).GetField ("Value", BindingFlags.Public | BindingFlags.Static));
				return;

			case TypeCode.String:
				EmitIfNotNull (ec, c => c.ig.Emit (OpCodes.Ldstr, (string) value));
				return;

			case TypeCode.Object:
				EmitIfNotNull (ec, c => c.EmitReadGlobal (value));
				return;
			}

			throw new NotImplementedException (String.Format ("No support for constants of type {0} yet", Type));
		}

		void EmitIfNotNull (EmitContext ec, Action<EmitContext> emit)
		{
			if (value == null) {
				ec.ig.Emit (OpCodes.Ldnull);
				return;
			}

			emit (ec);
		}
	}
}
