//
// System.Int32.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {
	
	public struct Int32 : IComparable, IFormattable { //, IConvertible {
		private static Type Type = typeof (int);
		
		public const int MaxValue = 0x7fffffff;
		public const int MinValue = -2147483648;
		
		public int value;

		public int CompareTo (object v)
		{
			if (v == null || !(v is System.Int32))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Int32"));

			return value - (int) v;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Int32))
				return false;

			return ((int) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public static int Parse (string s)
		{
			return Parse (s, NumberStyles.Integer, null);
		}

		public static int Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		public static int Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}
		
		public static int Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return ToString ("G", null);
		}

		public string ToString (IFormatProvider fp)
		{
			return ToString ("G", fp);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider fp)
		{
			string fmt;
			NumberFormatInfo nfi;
			
			fmt = (format == null) ? "G" : format;
			
			if (fp == null)
				nfi = NumberFormatInfo.CurrentInfo;
			else {
				nfi = (NumberFormatInfo) fp.GetFormat (Type);
				
				if (nfi == null)
					nfi = NumberFormatInfo.CurrentInfo;
			}

			return IntegerFormatter.NumberToString (fmt, nfi, value);
		}

		// =========== IConvertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Int32;
		}
	}
}
