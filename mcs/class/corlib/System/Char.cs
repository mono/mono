//
// System.Char.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {
	
	public struct Char : IComparable, IFormattable {
		public const char MinValue = (char) 0;
		public const char MaxValue = (char) 0xffff;
		
		// VES needs to know about value.  public is workaround
		// so source will compile
		public byte value;

		public int CompareTo (object v)
		{
			if (!(v is System.Byte))
				throw new ArgumentException ("Value is not a System.Byte");

			return value - ((byte) v);
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Byte))
				return false;

			return ((byte) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public static double GetNumericValue (char c)
		{
			if ((c >= 48) && (c <= 57))
				return (double) (c - '0');
			return -1;
		}

		public static double GetNumericValue (string s, int index)
		{
			/* FIXME: implement me */
			return -1;
		}

		public static bool IsControl (char c)
		{
			return ((c > 1) && (c < 32));
		}

		public static bool IsDigit (char c)
		{
			return ((c >= '0') && (c <= '9'));
		}

		public static bool IsLetter (char c)
		{
			/*
			 * FIXME: This is broken, it should support
			 * the various categories in System.Globalization.UnicodeCategory
			 */
			return ((c >= 65) && (c <= 126));
		}
		
		public TypeCode GetTypeCode ()
		{
			return TypeCode.Byte;
		}

		public static char Parse (string s)
		{
			// TODO: Implement me
			return (char) 0;
		}

		public static char Parse (string s, IFormatProvider fp)
		{
			// TODO: Implement me
			return (char) 0;
		}

		public static char Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			return (char) 0;
		}

		private static char ToLower (char c)
		{
			// FIXME: make me unicode aware
			return (c >= 'A' && c <= 'Z') ? (char) (c + 33) : c;
		}

		private static char ToUpper (char c)
		{
			// FIXME: make me unicode aware
			return (char) ((c >= 'a' && c <= 'z') ? c - 33 : c);
		}

		public override string ToString ()
		{
			// TODO: Implement me

			return "";
		}

		public string ToString (IFormatProvider fp)
		{
			// TODO: Implement me.
			return "";
		}

		public string ToString (string format)
		{
			// TODO: Implement me.
			return "";
		}

		public string ToString (string format, IFormatProvider fp)
		{
			// TODO: Implement me.
			return "";
		}
	}
}
