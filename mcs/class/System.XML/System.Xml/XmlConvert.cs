//
// System.Xml.XmlConvert
//
// Authors:
//      Dwivedi, Ajay kumar (Adwiv@Yahoo.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Alan Tam Siu Lung (Tam@SiuLung.com)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
using System.IO;
using System.Text;
using System.Globalization;
using System.Xml.Schema;

namespace System.Xml {

	public class XmlConvert {

		const string encodedColon = "_x003A_";
		const NumberStyles floatStyle = NumberStyles.AllowCurrencySymbol |
			NumberStyles.AllowExponent | 
			NumberStyles.AllowDecimalPoint |
			NumberStyles.AllowLeadingSign;
		
		static readonly string [] datetimeFormats = {
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
		};
		
		public XmlConvert()
		{}

		private static string TryDecoding (string s)
		{
			if (s == null || s.Length < 6)
				return s;

			char c = '\uFFFF';
			try {
				c = (char) Int32.Parse (s.Substring (1, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
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

			if ((name [pos + 1] != 'X' && name [pos + 1] != 'x') || name [pos + 6] != '_')
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
			
			if (firstOnlyLetter)
				return !XmlChar.IsFirstNameChar (c);
			else
				return !XmlChar.IsNameChar (c);
		}

		private static string EncodeName (string name, bool nmtoken)
		{
			StringBuilder sb = new StringBuilder ();
			int length = name.Length;
			for (int i = 0; i < length; i++) {
				char c = name [i];
				if (IsInvalid (c, i == 0 && !nmtoken))
					sb.AppendFormat ("_x{0:X4}_", (int) c);
				else if (c == '_' && i + 6 < length && name [i+1] == 'x' && name [i + 6] == '_')
					sb.Append ("_x005F_");
				else
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
			s = s.Trim (XmlChar.WhitespaceChars);
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

		// LAMESPEC: It has been documented as public, but is marked as internal.
		internal static string ToBinHexString (byte [] buffer)
		{
			StringWriter w = new StringWriter ();
			WriteBinHex (buffer, 0, buffer.Length, w);
			return w.ToString ();
		}

		internal static void WriteBinHex (byte [] buffer, int index, int count, TextWriter w)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", index, "index must be non negative integer.");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", count, "count must be non negative integer.");
			if (buffer.Length < index + count)
				throw new ArgumentOutOfRangeException ("index and count must be smaller than the length of the buffer.");

			// Copied from XmlTextWriter.WriteBinHex ()
			int end = index + count;
			for (int i = index; i < end; i++) {
				int val = buffer [i];
				int high = val >> 4;
				int low = val & 15;
				if (high > 9)
					w.Write ((char) (high + 55));
				else
					w.Write ((char) (high + 0x30));
				if (low > 9)
					w.Write ((char) (low + 55));
				else
					w.Write ((char) (low + 0x30));
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

#if NET_2_0
		[Obsolete]
#endif
		public static DateTime ToDateTime (string s)
		{
			return ToDateTime (s, datetimeFormats);
		}
		
#if NET_2_0
		public static DateTime ToDateTime (string value, XmlDateTimeSerializationMode mode)
		{
			string modestr = null;
			switch (mode) {
			case XmlDateTimeSerializationMode.Local:
			case XmlDateTimeSerializationMode.RoundTripKind:
			default:
				return ToDateTime (value, "yyyy-MM-ddTHH:mm:ss.fffffffzzz");
			case XmlDateTimeSerializationMode.Utc:
				return ToDateTime (value, "yyyy-MM-ddTHH:mm:ss.fffffffZ").ToUniversalTime ();
			case XmlDateTimeSerializationMode.Unspecified:
				return ToDateTime (value, "yyyy-MM-ddTHH:mm:ss.fffffff");
			}
		}
#endif
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
			if (s == null)
				throw new ArgumentNullException();
			if (s == "INF")
				return Double.PositiveInfinity;
			if (s == "-INF")
				return Double.NegativeInfinity;
			if (s == "NaN")
				return Double.NaN;
			return Double.Parse (s, floatStyle, CultureInfo.InvariantCulture);
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
			if (s == null)
				throw new ArgumentNullException();
			if (s == "INF")
				return Single.PositiveInfinity;
			if (s == "-INF")
				return Single.NegativeInfinity;
			if (s == "NaN")
				return Single.NaN;
			return Single.Parse(s, floatStyle, CultureInfo.InvariantCulture);
		}

		public static string ToString(Guid value)
		{
			return value.ToString("D", CultureInfo.InvariantCulture);
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
			return value.ToString (CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static string ToString(UInt64 value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString (TimeSpan value)
		{
			StringBuilder builder = new StringBuilder ();
			if (value.Ticks < 0) {
				builder.Append ('-');
				value = value.Negate ();
			}
			builder.Append ('P');
			if (value.Days > 0)
				builder.Append (value.Days).Append ('D');
			if (value.Days > 0 || value.Hours > 0 || value.Minutes > 0 || value.Seconds > 0 || value.Milliseconds > 0) {
				builder.Append('T');
				if (value.Hours > 0)
					builder.Append (value.Hours).Append ('H');
				if (value.Minutes > 0) 
					builder.Append (value.Minutes).Append ('M');
				if (value.Seconds > 0 || value.Milliseconds > 0) {
					builder.Append (value.Seconds);
					if (value.Milliseconds > 0)
						builder.Append ('.').AppendFormat ("{0:000}", value.Milliseconds);
					builder.Append ('S');
				}
			}
			return builder.ToString ();
		}

		public static string ToString(double value)
		{
			if (Double.IsNegativeInfinity(value)) return "-INF";
			if (Double.IsPositiveInfinity(value)) return "INF";
			if (Double.IsNaN(value)) return "NaN";
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(float value)
		{
			if (Single.IsNegativeInfinity(value)) return "-INF";
			if (Single.IsPositiveInfinity(value)) return "INF";
			if (Single.IsNaN(value)) return "NaN";
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

#if NET_2_0
		[Obsolete]
#endif
		public static string ToString (DateTime value)
		{
			return value.ToString ("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
		}

#if NET_2_0
		public static string ToString (DateTime value, XmlDateTimeSerializationMode mode)
		{
			string modestr = null;
			switch (mode) {
			case XmlDateTimeSerializationMode.Local:
			case XmlDateTimeSerializationMode.RoundTripKind:
			default:
				return value.ToString (
					"yyyy-MM-ddTHH:mm:ss.fffffffzzz",
					CultureInfo.InvariantCulture);
				break;
			case XmlDateTimeSerializationMode.Utc:
				return value.ToUniversalTime ().ToString (
					"yyyy-MM-ddTHH:mm:ss.fffffffZ",
					CultureInfo.InvariantCulture);
				break;
			case XmlDateTimeSerializationMode.Unspecified:
				return value.ToString (
					"yyyy-MM-ddTHH:mm:ss.fffffff",
					CultureInfo.InvariantCulture);
				break;
			}
		}
#endif

		public static string ToString(DateTime value, string format)
		{
			return value.ToString(format, CultureInfo.InvariantCulture);
		}

		public static TimeSpan ToTimeSpan(string s)
		{
			if (s.Length == 0)
				throw new ArgumentException ("Invalid format string for duration schema datatype.");

			int start = 0;
			if (s [0] == '-')
				start = 1;
			bool minusValue = (start == 1);

			if (s [start] != 'P')
				throw new ArgumentException ("Invalid format string for duration schema datatype.");
			start++;

			int parseStep = 0;
			int days = 0;
			bool isTime = false;
			int hours = 0;
			int minutes = 0;
			int seconds = 0;

			bool error = false;

			int i = start;
			while (i < s.Length) {
				if (s [i] == 'T') {
					isTime = true;
					parseStep = 4;
					i++;
					start = i;
					continue;
				}
				for (; i < s.Length; i++) {
					if (!Char.IsDigit (s [i]))
						break;
				}
				int value = int.Parse (s.Substring (start, i - start), CultureInfo.InvariantCulture);
				switch (s [i]) {
				case 'Y':
					days += value * 365;
					if (parseStep > 0)
						error = true;
					else
						parseStep = 1;
					break;
				case 'M':
					if (parseStep < 2) {
						days += 365 * (value / 12) + 30 * (value % 12);
						parseStep = 2;
					} else if (isTime && parseStep < 6) {
						minutes = value;
						parseStep = 6;
					}
					else
						error = true;
					break;
				case 'D':
					days += value;
					if (parseStep > 2)
						error = true;
					else
						parseStep = 3;
					break;
				case 'H':
					hours = value;
					if (!isTime || parseStep > 4)
						error = true;
					else
						parseStep = 5;
					break;
				case 'S':
					seconds = value;
					if (!isTime || parseStep > 6)
						error = true;
					else
						parseStep = 7;
					break;
				default:
					error = true;
					break;
				}
				if (error)
					break;
				++i;
				start = i;
			}
			if (error)
				throw new ArgumentException ("Invalid format string for duration schema datatype.");
			TimeSpan ts = new TimeSpan (days, hours, minutes, seconds);
			return minusValue ? -ts : ts;
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

		public static string VerifyName (string name)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (!XmlChar.IsName (name))
				throw new XmlException("'" + name + "' is not a valid XML Name");
			return name;
			
		}

		public static string VerifyNCName (string ncname)
		{
			if (ncname == null)
				throw new ArgumentNullException("ncname");

			if (!XmlChar.IsNCName (ncname))
				throw new XmlException ("'" + ncname + "' is not a valid XML NCName");
			return ncname;
		}

#if NET_2_0
		public static string VerifyNMTOKEN (string name)
#else
		internal static string VerifyNMTOKEN (string name)
#endif
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (!XmlChar.IsNmToken (name))
				throw new XmlException("'" + name + "' is not a valid XML NMTOKEN");
			return name;
			
		}

		// It is documented as public method, but in fact it is not.
		internal static byte [] FromBinHexString (string s)
		{
			char [] chars = s.ToCharArray ();
			byte [] bytes = new byte [chars.Length / 2 + chars.Length % 2];
			FromBinHexString (chars, 0, chars.Length, bytes);
			return bytes;
		}

		internal static int FromBinHexString (char [] chars, int offset, int charLength, byte [] buffer)
		{
			int bufIndex = offset;
			for (int i = 0; i < charLength - 1; i += 2) {
				buffer [bufIndex] = (chars [i] > '9' ?
						(byte) (chars [i] - 'A' + 10) :
						(byte) (chars [i] - '0'));
				buffer [bufIndex] <<= 4;
				buffer [bufIndex] += chars [i + 1] > '9' ?
						(byte) (chars [i + 1] - 'A' + 10) : 
						(byte) (chars [i + 1] - '0');
				bufIndex++;
			}
			if (charLength %2 != 0)
				buffer [bufIndex++] = (byte)
					((chars [charLength - 1] > '9' ?
						(byte) (chars [charLength - 1] - 'A' + 10) :
						(byte) (chars [charLength - 1] - '0'))
					<< 4);

			return bufIndex - offset;
		}
	}
}
