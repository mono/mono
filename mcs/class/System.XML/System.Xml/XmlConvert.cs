//
// System.Xml.XmlConvert
//
// Authors:
//      Dwivedi, Ajay kumar (Adwiv@Yahoo.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Alan Tam Siu Lung (Tam@SiuLung.com)
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
			return ToDateTime(s, new string[] {
			  // dateTime
			  "yyyy-MM-ddTHH:mm:ss",
			  "yyyy-MM-ddTHH:mm:ss.f",
			  "yyyy-MM-ddTHH:mm:ss.ff",
			  "yyyy-MM-ddTHH:mm:ss.fff",
			  "yyyy-MM-ddTHH:mm:ss.ffff",
			  "yyyy-MM-ddTHH:mm:ss.fffff",
			  "yyyy-MM-ddTHH:mm:ss.ffffff",
			  "yyyy-MM-ddTHH:mm:ss.fffffff",
			  "yyyy-MM-ddTHH:mm:sszzz",
			  "yyyy-MM-ddTHH:mm:ss.fzzz",
			  "yyyy-MM-ddTHH:mm:ss.ffzzz",
			  "yyyy-MM-ddTHH:mm:ss.fffzzz",
			  "yyyy-MM-ddTHH:mm:ss.ffffzzz",
			  "yyyy-MM-ddTHH:mm:ss.fffffzzz",
			  "yyyy-MM-ddTHH:mm:ss.ffffffzzz",
			  "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
			  "yyyy-MM-ddTHH:mm:ssZ",
			  "yyyy-MM-ddTHH:mm:ss.fZ",
			  "yyyy-MM-ddTHH:mm:ss.ffZ",
			  "yyyy-MM-ddTHH:mm:ss.fffZ",
			  "yyyy-MM-ddTHH:mm:ss.ffffZ",
			  "yyyy-MM-ddTHH:mm:ss.fffffZ",
			  "yyyy-MM-ddTHH:mm:ss.ffffffZ",
			  "yyyy-MM-ddTHH:mm:ss.fffffffZ",
			  // time
			  "HH:mm:ss",
			  "HH:mm:ss.f",
			  "HH:mm:ss.ff",
			  "HH:mm:ss.fff",
			  "HH:mm:ss.ffff",
			  "HH:mm:ss.fffff",
			  "HH:mm:ss.ffffff",
			  "HH:mm:ss.fffffff",
			  "HH:mm:sszzz",
			  "HH:mm:ss.fzzz",
			  "HH:mm:ss.ffzzz",
			  "HH:mm:ss.fffzzz",
			  "HH:mm:ss.ffffzzz",
			  "HH:mm:ss.fffffzzz",
			  "HH:mm:ss.ffffffzzz",
			  "HH:mm:ss.fffffffzzz",
			  "HH:mm:ssZ",
			  "HH:mm:ss.fZ",
			  "HH:mm:ss.ffZ",
			  "HH:mm:ss.fffZ",
			  "HH:mm:ss.ffffZ",
			  "HH:mm:ss.fffffZ",
			  "HH:mm:ss.ffffffZ",
			  "HH:mm:ss.fffffffZ",
			  // date
			  "yyyy-MM-dd",
			  "yyyy-MM-ddzzz",
			  "yyyy-MM-ddZ",
			  // gYearMonth
			  "yyyy-MM",
			  "yyyy-MMzzz",
			  "yyyy-MMZ",
			  // gYear
			  "yyyy",
			  "yyyyzzz",
			  "yyyyZ",
			  // gMonthDay
			  "--MM-dd",
			  "--MM-ddzzz",
			  "--MM-ddZ",
			  // gDay
			  "---dd",
			  "---ddzzz",
			  "---ddZ",
			});
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
			if (s == "INF") return System.Double.PositiveInfinity;
			if (s == "-INF") return System.Double.NegativeInfinity;
			if (s == "NaN") return System.Double.NaN;
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
			if (s == "INF") return System.Single.PositiveInfinity;
			if (s == "-INF") return System.Single.NegativeInfinity;
			if (s == "NaN") return System.Single.NaN;
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
			if (value) return "true";
			return "false";
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
			StringBuilder builder = new StringBuilder();
			if (value.Ticks < 0) {
				builder.Append('-');
				value = value.Negate();
			}
			builder.Append('P');
			if (value.Days > 0) builder.Append(value.Days).Append('D');
			if (value.Days > 0 || value.Minutes > 0 || value.Seconds > 0 || value.Milliseconds > 0) {
				builder.Append('T');
				if (value.Hours > 0) builder.Append(value.Hours).Append('D');
				if (value.Minutes > 0) builder.Append(value.Minutes).Append('M');
				if (value.Seconds > 0 || value.Milliseconds > 0) {
					builder.Append(value.Seconds);
					if (value.Milliseconds > 0) builder.Append('.').Append(String.Format("{0:000}", value.Milliseconds));
					builder.Append('S');
				}
			}
			return builder.ToString();
		}

		public static string ToString(double value)
		{
			if (Double.IsNegativeInfinity(value)) return "-INF";
			if (Double.IsPositiveInfinity(value)) return "INF";
			if (Double.IsNaN(value)) return "INF";
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(float value)
		{
			if (Single.IsNegativeInfinity(value)) return "-INF";
			if (Single.IsPositiveInfinity(value)) return "INF";
			if (Single.IsNaN(value)) return "INF";
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
			return value.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
		}

		public static string ToString(DateTime value, string format)
		{
			return value.ToString(format, CultureInfo.InvariantCulture);
		}

		[MonoTODO]
		public static TimeSpan ToTimeSpan(string s)
		{
			return TimeSpan.Parse(s); // FIXME: Should Parse according to XML Schema spec
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
