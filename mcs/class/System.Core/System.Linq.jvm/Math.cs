//
// Math.cs
//
// (C) 2008 Mainsoft, Inc. (http://www.mainsoft.com)
// (C) 2008 db4objects, Inc. (http://www.db4o.com)
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
using System.Globalization;
using System.Linq.Expressions;

namespace System.Linq.jvm {
	class Math {

		public static object Evaluate (object a, object b, Type t, ExpressionType et)
		{
			TypeCode tc = Type.GetTypeCode (t);
			if (tc == TypeCode.Object) {
				if (!t.IsNullable ()) {
					throw new NotImplementedException (
						string.Format (
						"Expression with Node type {0} for type {1}",
						t.FullName,
						tc));

				}
				return EvaluateNullable (a, b, Type.GetTypeCode (t.GetGenericArguments () [0]), et);
			}
			return Evaluate (a, b, tc, et);
		}

		public static object EvaluateNullable (object a, object b, TypeCode tc, ExpressionType et)
		{
			object o = null;
			if (a == null || b == null) {
				if (tc != TypeCode.Boolean) {
					return null;
				}
				switch (et) {
				case ExpressionType.And:
					o = And (a, b);
					break;
				case ExpressionType.Or:
					o = Or (a, b);
					break;
				case ExpressionType.ExclusiveOr:
					o = ExclusiveOr (a, b);
					break;
				}
			} else {
				o = Evaluate (a, b, tc, et);
			}

			return Convert2Nullable (o, tc);

		}

		private static object ExclusiveOr (object a, object b)
		{
			if (a == null || b == null) {
				return null;
			}
			return (bool) a ^ (bool) b;
		}

		public static object Or (object a, object b)
		{
			if (a == null) {
				if (b == null || !((bool) b)) {
					return null;
				}
				return true;
			}

			if (b == null) {
				if (a == null || !((bool) a)) {
					return null;
				}
				return true;
			}

			return (bool) a || (bool) b;
		}

		public static object And (object a, object b)
		{
			if (a == null) {
				if (b == null || (bool) b) {
					return null;
				}
				return false;
			}

			if (b == null) {
				if (a == null || (bool) a) {
					return null;
				}
				return false;
			}

			return (bool) a && (bool) b;
		}

		private static object Convert2Nullable (object o, TypeCode tc)
		{
			if (o == null) {
				return null;
			}
			switch (tc) {
			case TypeCode.Char:
				return new Nullable<Char> ((Char) o);
			case TypeCode.Byte:
				return new Nullable<Byte> ((Byte) o);
			case TypeCode.Decimal:
				return new Nullable<Decimal> ((Decimal) o);
			case TypeCode.Double:
				return new Nullable<Double> ((Double) o);
			case TypeCode.Int16:
				return new Nullable<Int16> ((Int16) o);
			case TypeCode.Int32:
				return new Nullable<Int32> ((Int32) o);
			case TypeCode.Int64:
				return new Nullable<Int64> ((Int64) o);
			case TypeCode.UInt16:
				return new Nullable<UInt16> ((UInt16) o);
			case TypeCode.UInt32:
				return new Nullable<UInt32> ((UInt32) o);
			case TypeCode.SByte:
				return new Nullable<SByte> ((SByte) o);
			case TypeCode.Single:
				return new Nullable<Single> ((Single) o);
			case TypeCode.Boolean:
				return new Nullable<Boolean> ((Boolean) o);
			}

			throw new NotImplementedException ();
		}

		public static object Evaluate (object a, object b, TypeCode tc, ExpressionType et)
		{
			switch (tc) {
			case TypeCode.Boolean:
				return Evaluate (Convert.ToBoolean (a), Convert.ToBoolean (b), et);
			case TypeCode.Char:
				return Evaluate (Convert.ToChar (a), Convert.ToChar (b), et);
			case TypeCode.Byte:
				return unchecked ((Byte) Evaluate (Convert.ToByte (a), Convert.ToByte (b), et));
			case TypeCode.Decimal:
				return Evaluate (Convert.ToDecimal (a), Convert.ToDecimal (b), et);
			case TypeCode.Double:
				return Evaluate (Convert.ToDouble (a), Convert.ToDouble (b), et);
			case TypeCode.Int16:
				return unchecked ((Int16) Evaluate (Convert.ToInt16 (a), Convert.ToInt16 (b), et));
			case TypeCode.Int32:
				return Evaluate (Convert.ToInt32 (a), Convert.ToInt32 (b), et);
			case TypeCode.Int64:
				return Evaluate (Convert.ToInt64 (a), Convert.ToInt64 (b), et);
			case TypeCode.UInt16:
				return unchecked ((UInt16) Evaluate (Convert.ToUInt16 (a), Convert.ToUInt16 (b), et));
			case TypeCode.UInt32:
				return Evaluate (Convert.ToUInt32 (a), Convert.ToUInt32 (b), et);
			case TypeCode.UInt64:
				return Evaluate (Convert.ToUInt64 (a), Convert.ToUInt64 (b), et);
			case TypeCode.SByte:
				return unchecked ((SByte) Evaluate (Convert.ToSByte (a), Convert.ToSByte (b), et));
			case TypeCode.Single:
				return Evaluate (Convert.ToSingle (a), Convert.ToSingle (b), et);

			}

			throw new NotImplementedException ();
		}

		public static object NegateChecked (object a, TypeCode tc)
		{
			switch (tc) {
			case TypeCode.Char:
				return checked (-Convert.ToChar (a));
			case TypeCode.Byte:
				return checked (-Convert.ToByte (a));
			case TypeCode.Decimal:
				return checked (-Convert.ToDecimal (a));
			case TypeCode.Double:
				return checked (-Convert.ToDouble (a));
			case TypeCode.Int16:
				return checked (-Convert.ToInt16 (a));
			case TypeCode.Int32:
				return checked (-Convert.ToInt32 (a));
			case TypeCode.Int64:
				return checked (-Convert.ToInt64 (a));
			case TypeCode.UInt16:
				return checked (-Convert.ToUInt16 (a));
			case TypeCode.UInt32:
				return checked (-Convert.ToUInt32 (a));
			case TypeCode.SByte:
				return checked (-Convert.ToSByte (a));
			case TypeCode.Single:
				return checked (-Convert.ToSingle (a));
			}

			throw new NotImplementedException ();
		}

		static object CreateInstance (Type type, params object [] arguments)
		{
			return type.GetConstructor (
				(from argument in arguments select argument.GetType ()).ToArray ()).Invoke (arguments);
		}

		public static object ConvertToTypeChecked (object a, Type fromType, Type toType)
		{
			if (toType.IsNullable ())
				return a == null ? a : CreateInstance (toType,
					ConvertToTypeChecked (a, fromType.GetNotNullableType (), toType.GetNotNullableType ()));

			if (a == null) {
				if (!toType.IsValueType)
					return a;
				if (fromType.IsNullable ())
					throw new InvalidOperationException ("Nullable object must have a value");
			}

			if (IsType (toType, a)) {
				return a;
			}

			if (Expression.IsPrimitiveConversion (fromType, toType))
				return Convert.ChangeType (a, toType, CultureInfo.CurrentCulture);

			throw new NotImplementedException (
							string.Format ("No Convert defined for type {0} ", toType));
		}

		public static object ConvertToTypeUnchecked (object a, Type fromType, Type toType)
		{
			if (toType.IsNullable ())
				return a == null ? a : CreateInstance (toType,
					ConvertToTypeUnchecked (a, fromType.GetNotNullableType (), toType.GetNotNullableType ()));

			if (a == null) {
				if (!toType.IsValueType)
					return a;
				if (fromType.IsNullable ())
					throw new InvalidOperationException ("Nullable object must have a value");
			}

			if (IsType (toType, a))
				return a;

			if (Expression.IsPrimitiveConversion (fromType, toType))
				return Conversion.ConvertPrimitiveUnChecked (fromType, toType, a);

			throw new NotImplementedException (
							string.Format ("No Convert defined for type {0} ", toType));
		}

		public static bool IsType (Type t, Object o)
		{
			return t.IsInstanceOfType (o);
		}

		public static object Negate (object a, TypeCode tc)
		{
			switch (tc) {
			case TypeCode.Char:
				return unchecked (-Convert.ToChar (a));
			case TypeCode.Byte:
				return unchecked (-Convert.ToByte (a));
			case TypeCode.Decimal:
				return unchecked (-Convert.ToDecimal (a));
			case TypeCode.Double:
				return unchecked (-Convert.ToDouble (a));
			case TypeCode.Int16:
				return unchecked (-Convert.ToInt16 (a));
			case TypeCode.Int32:
				return unchecked (-Convert.ToInt32 (a));
			case TypeCode.Int64:
				return unchecked (-Convert.ToInt64 (a));
			case TypeCode.UInt16:
				return unchecked (-Convert.ToUInt16 (a));
			case TypeCode.UInt32:
				return unchecked (-Convert.ToUInt32 (a));
			case TypeCode.SByte:
				return unchecked (-Convert.ToSByte (a));
			case TypeCode.Single:
				return unchecked (-Convert.ToSingle (a));
			}

			throw new NotImplementedException ();
		}

		public static object RightShift (object a, int n, TypeCode tc)
		{
			switch (tc) {
			case TypeCode.Int16:
				return Convert.ToInt16 (a) >> n;
			case TypeCode.Int32:
				return Convert.ToInt32 (a) >> n;
			case TypeCode.Int64:
				return Convert.ToInt64 (a) >> n;
			case TypeCode.UInt16:
				return Convert.ToUInt16 (a) >> n;
			case TypeCode.UInt32:
				return Convert.ToUInt32 (a) >> n;
			case TypeCode.UInt64:
				return Convert.ToUInt64 (a) >> n;
			}

			throw new NotImplementedException ();
		}

		public static object LeftShift (object a, int n, TypeCode tc)
		{
			switch (tc) {
			case TypeCode.Int16:
				return Convert.ToInt16 (a) << n;
			case TypeCode.Int32:
				return Convert.ToInt32 (a) << n;
			case TypeCode.Int64:
				return Convert.ToInt64 (a) << n;
			case TypeCode.UInt16:
				return Convert.ToUInt16 (a) << n;
			case TypeCode.UInt32:
				return Convert.ToUInt32 (a) << n;
			case TypeCode.UInt64:
				return Convert.ToUInt64 (a) << n;
			}

			throw new NotImplementedException ();
		}

		private static Decimal Evaluate (Decimal a, Decimal b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.Add:
				return unchecked (a + b);
			case ExpressionType.AddChecked:
				return checked (a + b);
			case ExpressionType.Subtract:
				return unchecked (a - b);
			case ExpressionType.SubtractChecked:
				return checked (a - b);
			case ExpressionType.Multiply:
				return unchecked (a * b);
			case ExpressionType.MultiplyChecked:
				return checked (a * b);
			case ExpressionType.Divide:
				return a / b;
			case ExpressionType.Modulo:
				return a % b;

			}

			throw new NotImplementedException ();
		}

		private static Double Evaluate (Double a, Double b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.Add:
				return unchecked (a + b);
			case ExpressionType.AddChecked:
				return checked (a + b);
			case ExpressionType.Subtract:
				return unchecked (a - b);
			case ExpressionType.SubtractChecked:
				return checked (a - b);
			case ExpressionType.Multiply:
				return unchecked (a * b);
			case ExpressionType.MultiplyChecked:
				return checked (a * b);
			case ExpressionType.Divide:
				return a / b;
			case ExpressionType.Modulo:
				return a % b;
			case ExpressionType.Power:
				return System.Math.Pow (a, b);
			}

			throw new NotImplementedException ();

		}

		private static Int32 Evaluate (Int16 a, Int16 b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.Add:
				return unchecked (a + b);
			case ExpressionType.AddChecked:
				return checked (a + b);
			case ExpressionType.Subtract:
				return unchecked (a - b);
			case ExpressionType.SubtractChecked:
				return checked (a - b);
			case ExpressionType.Multiply:
				return unchecked (a * b);
			case ExpressionType.MultiplyChecked:
				return checked (a * b);
			case ExpressionType.Divide:
				return a / b;
			case ExpressionType.Modulo:
				return a % b;
			case ExpressionType.ExclusiveOr:
				return a ^ b;
			case ExpressionType.And:
				return a & b;
			case ExpressionType.Or:
				return a | b;
			}

			throw new NotImplementedException ();
		}

		private static Int32 Evaluate (Int32 a, Int32 b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.Add:
				return unchecked (a + b);
			case ExpressionType.AddChecked:
				return checked (a + b);
			case ExpressionType.Subtract:
				return unchecked (a - b);
			case ExpressionType.SubtractChecked:
				return checked (a - b);
			case ExpressionType.Multiply:
				return unchecked (a * b);
			case ExpressionType.MultiplyChecked:
				return checked (a * b);
			case ExpressionType.Divide:
				return a / b;
			case ExpressionType.Modulo:
				return a % b;
			case ExpressionType.ExclusiveOr:
				return a ^ b;
			case ExpressionType.And:
				return a & b;
			case ExpressionType.Or:
				return a | b;
			}

			throw new NotImplementedException ();
		}

		private static Int64 Evaluate (Int64 a, Int64 b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.Add:
				return unchecked (a + b);
			case ExpressionType.AddChecked:
				return checked (a + b);
			case ExpressionType.Subtract:
				return unchecked (a - b);
			case ExpressionType.SubtractChecked:
				return checked (a - b);
			case ExpressionType.Multiply:
				return unchecked (a * b);
			case ExpressionType.MultiplyChecked:
				return checked (a * b);
			case ExpressionType.Divide:
				return a / b;
			case ExpressionType.Modulo:
				return a % b;
			case ExpressionType.ExclusiveOr:
				return a ^ b;
			case ExpressionType.And:
				return a & b;
			case ExpressionType.Or:
				return a | b;
			}

			throw new NotImplementedException ();
		}

		private static Int32 Evaluate (UInt16 a, UInt16 b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.Add:
				return unchecked (a + b);
			case ExpressionType.AddChecked:
				return checked (a + b);
			case ExpressionType.Subtract:
				return unchecked (a - b);
			case ExpressionType.SubtractChecked:
				return checked ((UInt16) (a - b));
			case ExpressionType.Multiply:
				return unchecked (a * b);
			case ExpressionType.MultiplyChecked:
				return checked (a * b);
			case ExpressionType.Divide:
				return a / b;
			case ExpressionType.Modulo:
				return a % b;
			case ExpressionType.ExclusiveOr:
				return a ^ b;
			case ExpressionType.And:
				return a & b;
			case ExpressionType.Or:
				return a | b;
			}

			throw new NotImplementedException ();
		}

		private static UInt32 Evaluate (UInt32 a, UInt32 b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.Add:
				return unchecked (a + b);
			case ExpressionType.AddChecked:
				return checked (a + b);
			case ExpressionType.Subtract:
				return unchecked (a - b);
			case ExpressionType.SubtractChecked:
				return checked (a - b);
			case ExpressionType.Multiply:
				return unchecked (a * b);
			case ExpressionType.MultiplyChecked:
				return checked (a * b);
			case ExpressionType.Divide:
				return a / b;
			case ExpressionType.Modulo:
				return a % b;
			case ExpressionType.ExclusiveOr:
				return a ^ b;
			case ExpressionType.And:
				return a & b;
			case ExpressionType.Or:
				return a | b;
			}

			throw new NotImplementedException ();
		}

		private static UInt64 Evaluate (UInt64 a, UInt64 b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.Add:
				return unchecked (a + b);
			case ExpressionType.AddChecked:
				return checked (a + b);
			case ExpressionType.Subtract:
				return unchecked (a - b);
			case ExpressionType.SubtractChecked:
				return checked (a - b);
			case ExpressionType.Multiply:
				return unchecked (a * b);
			case ExpressionType.MultiplyChecked:
				return checked (a * b);
			case ExpressionType.Divide:
				return a / b;
			case ExpressionType.Modulo:
				return a % b;
			case ExpressionType.ExclusiveOr:
				return a ^ b;
			case ExpressionType.And:
				return a & b;
			case ExpressionType.Or:
				return a | b;
			}

			throw new NotImplementedException ();
		}

		private static object Evaluate (Char a, Char b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.ExclusiveOr:
				return a ^ b;
			case ExpressionType.And:
				return a & b;
			case ExpressionType.Or:
				return a | b;
			}

			throw new NotImplementedException ();
		}

		private static Int32 Evaluate (SByte a, SByte b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.Add:
				return unchecked (a + b);
			case ExpressionType.AddChecked:
				return checked (a + b);
			case ExpressionType.Subtract:
				return unchecked (a - b);
			case ExpressionType.SubtractChecked:
				return checked (a - b);
			case ExpressionType.Multiply:
				return unchecked (a * b);
			case ExpressionType.MultiplyChecked:
				return checked (a * b);
			case ExpressionType.Divide:
				return a / b;
			case ExpressionType.Modulo:
				return a % b;
			case ExpressionType.ExclusiveOr:
				return a ^ b;
			case ExpressionType.And:
				return a & b;
			case ExpressionType.Or:
				return a | b;
			}

			throw new NotImplementedException ();
		}

		private static Int32 Evaluate (Byte a, Byte b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.Add:
				return unchecked (a + b);
			case ExpressionType.AddChecked:
				return checked (a + b);
			case ExpressionType.Subtract:
				return unchecked (a - b);
			case ExpressionType.SubtractChecked:
				return checked (a - b);
			case ExpressionType.Multiply:
				return unchecked (a * b);
			case ExpressionType.MultiplyChecked:
				return checked (a * b);
			case ExpressionType.Divide:
				return a / b;
			case ExpressionType.Modulo:
				return a % b;
			case ExpressionType.ExclusiveOr:
				return a ^ b;
			case ExpressionType.And:
				return a & b;
			case ExpressionType.Or:
				return a | b;
			}

			throw new NotImplementedException ();
		}

		private static Single Evaluate (Single a, Single b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.Add:
				return unchecked (a + b);
			case ExpressionType.AddChecked:
				return checked (a + b);
			case ExpressionType.Subtract:
				return unchecked (a - b);
			case ExpressionType.SubtractChecked:
				return checked (a - b);
			case ExpressionType.Multiply:
				return unchecked (a * b);
			case ExpressionType.MultiplyChecked:
				return checked (a * b);
			case ExpressionType.Divide:
				return a / b;
			case ExpressionType.Modulo:
				return a % b;
			}

			throw new NotImplementedException ();
		}

		private static bool Evaluate (bool a, bool b, ExpressionType et)
		{
			switch (et) {
			case ExpressionType.ExclusiveOr:
				return a ^ b;
			case ExpressionType.And:
				return a & b;
			case ExpressionType.Or:
				return a | b;
			}

			throw new NotImplementedException ();
		}
	}
}
