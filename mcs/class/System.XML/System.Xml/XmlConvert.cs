//
// System.Xml.XmlConvert
//
// Authors:
//      Dwivedi, Ajay kumar (Adwiv@Yahoo.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Text;
using System.Globalization;

namespace System.Xml {

	public class XmlConvert {

		static string encodedColon = "_x003A_";

		public XmlConvert()
		{}

		private static string TryDecoding (string s)
		{
			if (s == null || s.Length < 6)
				return s;

			char c = '\uFFFF';
			try {
				c = (char) Int32.Parse (s.Substring (1, 4), NumberStyles.HexNumber);
			} catch {
				return s [0] + DecodeName (s.Substring (1));
			}
			
			if (s.Length == 6)
				return c.ToString ();
			return c + DecodeName (s.Substring (6));
		}
		
		public static string DecodeName (string name)
		{
			if (name == null || name.Length == 0)
				return name;

			int pos = name.IndexOf ('_');
			if (pos == -1 || pos + 6 >= name.Length)
				return name;

			if (Char.ToUpper (name [pos + 1]) != 'X' || name [pos + 6] != '_')
				return name [0] + DecodeName (name.Substring (1));

			return name.Substring (0, pos) + TryDecoding (name.Substring (pos + 1));
		}

		public static string EncodeLocalName (string name)
		{
			string encoded = EncodeName (name);
			int pos = encoded.IndexOf (':');
			if (pos == -1)
				return encoded;
			return encoded.Replace (":", encodedColon);
		}

		internal static bool IsInvalid (char c, bool firstOnlyLetter)
		{
			if (c == ':') // Special case. allowed in EncodeName, but encoded in EncodeLocalName
				return false;
			
			if (firstOnlyLetter && !Char.IsLetter (c) && c != '_')
				return false;
			
			return !Char.IsLetterOrDigit (c);
		}

		private static string EncodeName (string name, bool nmtoken)
		{
			StringBuilder sb = new StringBuilder ();
			int length = name.Length;
			for (int i = 0; i < length; i++) {
				char c = name [i];
				if (c != '_' || i + 6 >= length) {
					bool firstOnlyLetter = (i == 0 && !nmtoken);
					if (IsInvalid (c, firstOnlyLetter)) {
						sb.AppendFormat ("_x{0:X4}_", (int) c);
						continue;
					}
				} else { 
					if (Char.ToUpper (name [i + 1]) == 'X' && name [i + 6] == '_') {
						string decoded = TryDecoding (name.Substring (i + 1, 6));
						if (decoded.Length == 1) {
							sb.AppendFormat ("_x{0:X4}_", (int) c);
							continue;
						}
					}
				}
				sb.Append (c);
			}
			return sb.ToString ();
		}

		public static string EncodeName (string name)
		{
			return EncodeName (name, false);
		}
		
		public static string EncodeNmToken(string name)
		{
			return EncodeName (name, true);
		}

		// {true, false, 1, 0}
		public static bool ToBoolean(string s)
		{
			s = s.Trim();
			switch(s)
			{
				case "1":
					return true;
				case "true":
					return true;
				case "0":
					return false;
				case "false":
					return false;
				default:
					throw new FormatException(s + " is not a valid boolean value");
			}
		}

		public static byte ToByte(string s)
		{
			return Byte.Parse(s, CultureInfo.InvariantCulture);
		}

		public static char ToChar(string s)
		{
			return Char.Parse(s);
		}

		public static DateTime ToDateTime(string s)
		{
			return DateTime.Parse(s);
		}

		public static DateTime ToDateTime(string s, string format)
		{
			DateTimeFormatInfo d = new DateTimeFormatInfo();
			d.FullDateTimePattern = format;
			return DateTime.Parse(s, d);
		}

		public static DateTime ToDateTime(string s, string[] formats)
		{
			DateTimeStyles style = DateTimeStyles.AllowLeadingWhite |
					       DateTimeStyles.AllowTrailingWhite;
			return DateTime.ParseExact (s, formats, DateTimeFormatInfo.InvariantInfo, style);
		}
		
		public static Decimal ToDecimal(string s)
		{
			return Decimal.Parse(s, CultureInfo.InvariantCulture);
		}
		
		public static double ToDouble(string s)
		{
			return Double.Parse(s, CultureInfo.InvariantCulture);
		}

		public static Guid ToGuid(string s)
		{
			return new Guid(s);
		}

		public static short ToInt16(string s)
		{
			return Int16.Parse(s, CultureInfo.InvariantCulture);
		}

		public static int ToInt32(string s)
		{
			return Int32.Parse(s, CultureInfo.InvariantCulture);
		}

		public static long ToInt64(string s)
		{
			return Int64.Parse(s, CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static SByte ToSByte(string s)
		{
			return SByte.Parse(s, CultureInfo.InvariantCulture);
		}

		public static float ToSingle(string s)
		{
			return Single.Parse(s, CultureInfo.InvariantCulture);
		}

		public static string ToString(Guid value)
		{
			return value.ToString("D",CultureInfo.InvariantCulture);
		}

		public static string ToString(int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(short value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(byte value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(long value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(char value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(bool value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static string ToString(SByte value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
		public static string ToString(Decimal value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static string ToString(UInt64 value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
		public static string ToString(TimeSpan value)
		{
			return value.ToString();
		}
		public static string ToString(double value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
		public static string ToString(float value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static string ToString(UInt32 value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static string ToString(UInt16 value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
		public static string ToString(DateTime value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}
		public static string ToString(DateTime value, string format)
		{
			return value.ToString(format, CultureInfo.InvariantCulture);
		}
		public static TimeSpan ToTimeSpan(string s)
		{
			return TimeSpan.Parse(s);
		}

		[CLSCompliant (false)]
		public static UInt16 ToUInt16(string s)
		{
			return UInt16.Parse(s, CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static UInt32 ToUInt32(string s)
		{
			return UInt32.Parse(s, CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static UInt64 ToUInt64(string s)
		{
			return UInt64.Parse(s, CultureInfo.InvariantCulture);
		}

		public static string VerifyName(string name)
		{
			Exception innerEx;
			if(name == null)
				throw new ArgumentNullException("name");

			if(XmlConstructs.IsValidName(name, out innerEx))
				return name;
			
			throw new XmlException("'"+name+"' is not a valid XML Name",null);
		}

		public static string VerifyNCName(string ncname)
		{
			Exception innerEx;
			if(ncname == null)
				throw new ArgumentNullException("ncname");

			if(XmlConstructs.IsValidName(ncname, out innerEx))
				return ncname;
			
			throw new XmlException("'"+ncname+"' is not a valid XML NCName",innerEx);
		}
	}
}
