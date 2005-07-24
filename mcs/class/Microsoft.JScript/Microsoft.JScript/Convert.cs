//
// Convert.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
// (C) 2005, Novell Inc, (http://novell.com)
//

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
using System.Diagnostics;
using Microsoft.JScript.Vsa;
using System.Globalization;

namespace Microsoft.JScript {

	public sealed class Convert {

		public static bool IsBadIndex (AST ast)
		{
			throw new NotImplementedException ();
		}

		internal static bool IsNumber (object value)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);

			switch (tc) {
			case TypeCode.Char:
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Single:
			case TypeCode.Double:
				return true;

			case TypeCode.Object:
				if (value is NumberObject)
					return true;
				break;
			}

			return false;
		}

		internal static bool IsString (object value)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);

			switch (tc) {
			case TypeCode.String:
				return true;

			case TypeCode.Object:
				if (value is StringObject)
					return true;
				break;
			}

			return false;
		}

		internal static bool IsNumberTypeCode (TypeCode tc)
		{
			switch (tc) {
			case TypeCode.Byte:
			case TypeCode.Char:
			case TypeCode.Decimal:
			case TypeCode.Double:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.SByte:
			case TypeCode.Single:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				return true;
			default:
				return false;
			}
		}

		internal static bool IsFloatTypeCode (TypeCode tc)
		{
			switch (tc) {
			case TypeCode.Decimal:
			case TypeCode.Double:
			case TypeCode.Single:
				return true;
			default:
				return false;
			}
		}

		public static double CheckIfDoubleIsInteger (double d)
		{
			if (d == Math.Round (d))
				return d;
			throw new NotImplementedException ();
		}


		public static Single CheckIfSingleIsInteger (Single s)
		{
			if (s == Math.Round (s))
				return s;
			throw new NotImplementedException ();
		}


		public static object Coerce (object value, object type)
		{
			throw new NotImplementedException ();
		}


		public static object CoerceT (object value, Type t, bool explicitOK)
		{
			throw new NotImplementedException ();
		}


		public static object Coerce2 (object value, TypeCode target,
						  bool truncationPermitted)
		{
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static void ThrowTypeMismatch (object val)
		{
			throw new NotImplementedException ();
		}

		internal static object ToPrimitive (object value, Type hint)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);
			switch (tc) {
			case TypeCode.Object:
				if (value is JSObject)
					return ((JSObject) value).GetDefaultValue (hint);
				else
					throw new NotImplementedException ();
			default:
				return value;
			}
		}

		public static bool ToBoolean (double d)
		{
			return Convert.ToBoolean (d, true);
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static bool ToBoolean (object value)
		{
			return Convert.ToBoolean (value, true);
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static bool ToBoolean (object value, bool explicitConversion)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);

			switch (tc) {
			case TypeCode.Empty:
			case TypeCode.DBNull:
				return false;

			case TypeCode.Boolean:
				return ic.ToBoolean (null);

			case TypeCode.Char:
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Single:
			case TypeCode.Double:
				double num = ic.ToDouble (null);
				return !double.IsNaN (num) && (num != 0.0);

			case TypeCode.String:
				string str = ic.ToString (null);
				return str.Length != 0;

			case TypeCode.Object:
				return true;

			default:
				Console.WriteLine ("\nToBoolean: tc = {0}", tc);
				break;
			}
			throw new NotImplementedException ();
		}

		public static object ToForInObject (object value, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}

		public static int ToInt32 (object value)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);

			switch (tc) {
			case TypeCode.Empty:
			case TypeCode.DBNull:
				return 0;

			case TypeCode.Char:
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.Int16:
			case TypeCode.Int32:
				return (int) value;

			case TypeCode.Single:
			case TypeCode.Double:
				return (int) Math.Floor ((double) value);

			case TypeCode.String:
				return (int) Math.Floor (GlobalObject.parseFloat (ic.ToString ()));

			default:
				Console.WriteLine ("\nToInt32: value.GetType = {0}", value.GetType ());
				break;
			}
			throw new NotImplementedException ();
		}

		internal static int ToUint16 (object value)
		{
			double val = Convert.ToNumber (value);
			if (Double.IsInfinity (val) || double.IsNaN (val))
				return 0;
			else
				return (int) val % 65536;
		}

		public static double ToNumber (object value)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);

			switch (tc) {
			case TypeCode.Empty:
				return Double.NaN;

			case TypeCode.DBNull:
				return 0;

			case TypeCode.Boolean:
				if (ic.ToBoolean (null))
					return 1;
				return 0;

			case TypeCode.Char:
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.Int16:
			case TypeCode.Int32:
				return ic.ToDouble (null);

			case TypeCode.Single:
			case TypeCode.Double:
				return (double) value;

			case TypeCode.String:
				return GlobalObject.parseFloat (value);

			case TypeCode.Object:
				if (value is NumberObject)
					return ((NumberObject) value).value;
				break;
			}

			Console.WriteLine ("\nToNumber: value.GetType = {0}", value.GetType ());
			throw new NotImplementedException ();
		}


		public static double ToNumber (string str)
		{
			return GlobalObject.parseFloat (str);
		}


		public static object ToNativeArray (object value, RuntimeTypeHandle handle)
		{
			throw new NotImplementedException ();
		}


		public static object ToObject (object value, VsaEngine engine)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);

			switch (tc) {
			case TypeCode.String:
				return new StringObject (ic.ToString (null));
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Char:
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.Int16:
			case TypeCode.Int32:
				return new NumberObject (ic.ToDouble (null));
			case TypeCode.Object:
				return value;
			default:
				Console.WriteLine ("\nToObject: value.GetType = {0}", value.GetType ());
				break;
			}
			throw new NotImplementedException ();
		}


		public static object ToObject2 (object value, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		internal static string ToString (object obj)
		{
			return Convert.ToString (obj, true);
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static string ToString (object value, bool explicitOK)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);

			switch (tc) {
			case TypeCode.Empty:
				return "undefined";

			case TypeCode.DBNull:
				return "null";

			case TypeCode.Boolean:
				bool r = (bool) value;
				if (r)
					return "true";
				else
					return "false";

			case TypeCode.Char:
				return ic.ToInt16 (null).ToString ();

			case TypeCode.String:
				return ic.ToString (null);

			case TypeCode.Object:
				if (value is StringObject)
					return ((StringObject) value).value;
				else if (value is Closure)
					return FunctionPrototype.toString (((Closure) value).func);
				else if (value is ScriptObject)
					return (string) ((ScriptObject) value).CallMethod ("toString");

				Console.WriteLine ("value.GetType = {0}", value.GetType ());
				throw new NotImplementedException ();

			default:
				if (IsNumberTypeCode (tc))
					return ic.ToString (CultureInfo.InvariantCulture);

				Console.WriteLine ("tc = {0}", tc);
				throw new NotImplementedException ();
			}
		}


		internal static RegExpObject ToRegExp (object regExp)
		{
			if (regExp is RegExpObject)
				return (RegExpObject) regExp;
			else
				return RegExpConstructor.Ctr.Invoke (regExp);
		}

		public static string ToString (bool b)
		{
			return b ? "true" : "false";
		}


		public static string ToString (double d)
		{
			IConvertible ic = d as IConvertible;
			return ic.ToString (null);
		}

		//
		// Utility methods
		//
		internal static TypeCode GetTypeCode (object obj, IConvertible ic)
		{
			if (obj == null)
				return TypeCode.Empty;
			else if (ic == null)
				return TypeCode.Object;
			else
				return ic.GetTypeCode ();
		}
	}
}
