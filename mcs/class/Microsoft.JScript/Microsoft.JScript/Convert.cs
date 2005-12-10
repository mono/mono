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
using System.Collections;

namespace Microsoft.JScript {

	public sealed class Convert {

		public static bool IsBadIndex (AST ast)
		{
			throw new NotImplementedException ();
		}

		internal static bool IsBoolean (object value)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);
			switch (tc) {
			case TypeCode.Boolean:
				return true;

			case TypeCode.Object:
				if (value is BooleanObject)
					return true;
				break;
			}

			return false;
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
				else if (value is GlobalScope)
					return "[object global]";
				else {
					Console.WriteLine ("value = {0} ({1})", value, value.GetType ());
					throw new NotImplementedException ();
				}
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

			case TypeCode.String:
				string str = ic.ToString (null);
				return str.Length != 0;

			case TypeCode.Object:
				return true;

			default:
				if (IsNumberTypeCode (tc)) {
					double num = ic.ToDouble (null);
					return !double.IsNaN (num) && (num != 0.0);
				}

				Console.WriteLine ("\nToBoolean: tc = {0}", tc);
				break;
			}
			throw new NotImplementedException ();
		}

		public static object ToForInObject (object value, VsaEngine engine)
		{
			if (value == null)
				throw new NullReferenceException ("value is null");
			
			return GlobalObject.Object.CreateInstance (value);
		}

		public static int ToInt32 (object value)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);

			switch (tc) {
			case TypeCode.Empty:
			case TypeCode.DBNull:
				return 0;

			case TypeCode.String:
				return (int) Math.Floor (GlobalObject.parseFloat (ic.ToString ()));

			default:
				if (IsFloatTypeCode (tc))
					return (int) Math.Floor ((double) value);
				else if (IsNumberTypeCode (tc))
					return (int) value;

				Console.WriteLine ("\nToInt32: value.GetType = {0}", value.GetType ());
				break;
			}
			throw new NotImplementedException ();
		}

		internal static uint ToUint16 (object value)
		{
			double val = Convert.ToNumber (value);
			if (Double.IsInfinity (val) || double.IsNaN (val))
				return 0;
			else
				return (uint) (val % 65536);
		}

		internal static uint ToUint32 (object value)
		{
			double val = Convert.ToNumber (value);
			if (Double.IsInfinity (val) || double.IsNaN (val))
				return 0;
			else
				return (uint) (val % 4294967296);
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

			case TypeCode.String:
				return ToNumber ((string) value);

			case TypeCode.Object:
				if (value is NumberObject)
					return ((NumberObject) value).value;
				else if (value is BooleanObject)
					return ((BooleanObject) value).value ? 1 : 0;
				else if (value is StringObject)
					return ToNumber (((StringObject) value).value);
				else if (value is DateObject)
					return ((DateObject) value).ms;
				else if (value is ArrayObject) {
					ArrayObject ary = (ArrayObject) value;
					Hashtable elems = ary.elems;
					uint n = (uint) ary.length;
					uint i = n - 1;
					if (elems.ContainsKey (i))
						return Convert.ToNumber (elems [i]);
				}
				return Double.NaN;

			default:
				if (IsFloatTypeCode (tc))
					return (double) value;
				else if (IsNumberTypeCode (tc))
					return ic.ToDouble (null);
				break;
			}

			Console.WriteLine ("\nToNumber: value.GetType = {0}", value.GetType ());
			throw new NotImplementedException ();
		}


		public static double ToNumber (string str)
		{
			if (str == "")
				return 0;

			if (str.IndexOfAny (new char [] { 'x', 'X' }) != -1)
				return GlobalObject.parseInt (str, null);
			else
				return GlobalObject.parseFloat (str);
		}


		public static object ToNativeArray (object value, RuntimeTypeHandle handle)
		{
			ArrayObject ary = null;
			if (value is ArrayObject)
				ary = (ArrayObject) value;
			else
				ary = Convert.ToArray (value);

			Hashtable elems = ary.elems;
			uint n = (uint) ary.length;
			object [] result = new object [n];
			for (uint i = 0; i < n; i++)
				result [i] = elems [i];

			return result;
		}

		private static ArrayObject ToArray (object value)
		{
			throw new Exception ("The method or operation is not implemented.");
		}


		public static object ToObject (object value, VsaEngine engine)
		{
			IConvertible ic = value as IConvertible;
			TypeCode tc = Convert.GetTypeCode (value, ic);

			switch (tc) {
			case TypeCode.DBNull:
			case TypeCode.Empty:
				throw new JScriptException (JSError.TypeMismatch, "value is null or undefined");
			case TypeCode.Boolean:
				return new BooleanObject (ic.ToBoolean (null));
			case TypeCode.String:
				return new StringObject (ic.ToString (null));
			case TypeCode.Object:
				return value;
			default:
				if (IsNumberTypeCode (tc))
					return new NumberObject (ic.ToDouble (null));

				Console.WriteLine ("\nToObject: value.GetType = {0}", value.GetType ());
				break;
			}
			throw new NotImplementedException ();
		}


		public static object ToObject2 (object value, VsaEngine engine)
		{
			return ToObject (value, engine);
		}

		internal static RegExpObject ToRegExp (object regExp)
		{
			if (regExp is RegExpObject)
				return (RegExpObject) regExp;
			else
				return RegExpConstructor.Ctr.Invoke (regExp);
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
				else if (value is ScriptObject) {
					ScriptObject obj_value = (ScriptObject) value;
					if (obj_value.HasMethod ("toString"))
						return (string) obj_value.CallMethod ("toString");
					else
						return (string) ObjectPrototype.smartToString ((JSObject) obj_value);
				}

				Console.WriteLine ("value.GetType = {0}", value.GetType ());
				throw new NotImplementedException ();

			default:
				if (IsNumberTypeCode (tc)) {
					double val = ic.ToDouble (null);
					return ToString (val);
				}

				Console.WriteLine ("tc = {0}", tc);
				throw new NotImplementedException ();
			}
		}

		public static string ToString (bool b)
		{
			return b ? "true" : "false";
		}

		public static string ToString (double d)
		{
			double exp = Math.Log10 (d);
			if (exp > -6 && exp < 1)
				return d.ToString ("0.##########", CultureInfo.InvariantCulture);
			else
				return d.ToString ("g21", CultureInfo.InvariantCulture);
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
