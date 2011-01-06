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
			NumberStyles.AllowLeadingSign |
			NumberStyles.AllowLeadingWhite |
			NumberStyles.AllowTrailingWhite;
		
		const NumberStyles integerStyle = NumberStyles.Integer |
			NumberStyles.AllowLeadingWhite |
			NumberStyles.AllowTrailingWhite;

		static readonly string [] datetimeFormats = {
		  // dateTime
#if NET_2_0
		  "yyyy-MM-ddTHH:mm:sszzz",
		  "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz",
		  "yyyy-MM-ddTHH:mm:ssZ",
		  "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ",
		  "yyyy-MM-ddTHH:mm:ss",
		  "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
		  "HH:mm:ss",
		  "HH:mm:ss.FFFFFFF",
		  "HH:mm:sszzz",
		  "HH:mm:ss.FFFFFFFzzz",
		  "HH:mm:ssZ",
		  "HH:mm:ss.FFFFFFFZ",
#else // it is not required in trunk but should make it easy to backport...
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
		  "yyyy-MM-ddTHH:mm:ss",
		  "yyyy-MM-ddTHH:mm:ss.f",
		  "yyyy-MM-ddTHH:mm:ss.ff",
		  "yyyy-MM-ddTHH:mm:ss.fff",
		  "yyyy-MM-ddTHH:mm:ss.ffff",
		  "yyyy-MM-ddTHH:mm:ss.fffff",
		  "yyyy-MM-ddTHH:mm:ss.ffffff",
		  "yyyy-MM-ddTHH:mm:ss.fffffff",
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
#endif
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

#if NET_2_0
		static readonly string [] defaultDateTimeFormats = new string [] {
			"yyyy-MM-ddTHH:mm:ss", // dateTime(1)
			"yyyy-MM-ddTHH:mm:ss.FFFFFFF", // dateTime(2)
			"yyyy-MM-dd", // date
			"HH:mm:ss", // time
			"yyyy-MM", // gYearMonth
			"yyyy", // gYear
			"--MM-dd", // gMonthDay
			"---dd", // gDay
			};

		static readonly string [] roundtripDateTimeFormats;
		static readonly string [] localDateTimeFormats;
		static readonly string [] utcDateTimeFormats;
		static readonly string [] unspecifiedDateTimeFormats;

		static XmlConvert ()
		{
			int l = defaultDateTimeFormats.Length;
			roundtripDateTimeFormats = new string [l];
			localDateTimeFormats = new string [l];
			utcDateTimeFormats = new string [l * 3];
			unspecifiedDateTimeFormats = new string [l * 4];
			for (int i = 0; i < l; i++) {
				string s = defaultDateTimeFormats [i];
				localDateTimeFormats [i] = s + (s [s.Length - 1] == 's' || s [s.Length - 1] == 'F' ? "zzz" : String.Empty);
				roundtripDateTimeFormats [i] = s + 'K';
				utcDateTimeFormats [i * 3] = s;
				utcDateTimeFormats [i * 3 + 1] = s + 'Z';
				utcDateTimeFormats [i * 3 + 2] = s + "zzz";
				unspecifiedDateTimeFormats [i * 4] = s;
				unspecifiedDateTimeFormats [i * 4 + 1] = localDateTimeFormats [i];
				unspecifiedDateTimeFormats [i * 4 + 2] = roundtripDateTimeFormats [i];
				unspecifiedDateTimeFormats [i * 4 + 3] = utcDateTimeFormats [i];
			}
		}
#endif
		static DateTimeStyles _defaultStyle = DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite;
		
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
			if (name == null)
				return name;

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
			if (name == null || name.Length == 0)
				return name;

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
		
		public static string EncodeNmToken (string name)
		{
			if (name == String.Empty)
				throw new XmlException ("Invalid NmToken: ''");
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
			if (index < 0) {
				throw new ArgumentOutOfRangeException (
#if !NET_2_1
					"index", index,
#endif
					"index must be non negative integer.");
			}
			if (count < 0) {
				throw new ArgumentOutOfRangeException (
#if !NET_2_1
					"count", count,
#endif
					"count must be non negative integer.");
			}
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
			return Byte.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
		}

		public static char ToChar(string s)
		{
#if !NET_2_1
			return Char.Parse(s);
#else
			if (s == null)
				throw new ArgumentNullException ("s");

			if (s.Length != 1)
				throw new FormatException ("String contain more than one char");

			return s [0];
#endif
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
			DateTime dt;
			switch (mode) {
			case XmlDateTimeSerializationMode.Local:
				dt = ToDateTime (value, localDateTimeFormats);
				return new DateTime (dt.Ticks, DateTimeKind.Local);
			case XmlDateTimeSerializationMode.RoundtripKind:
				return ToDateTime (value, roundtripDateTimeFormats, _defaultStyle | DateTimeStyles.RoundtripKind);
			case XmlDateTimeSerializationMode.Utc:
				dt = ToDateTime (value, utcDateTimeFormats);
				return new DateTime (dt.Ticks, DateTimeKind.Utc);
			case XmlDateTimeSerializationMode.Unspecified:
				return ToDateTime (value, unspecifiedDateTimeFormats);
			default:
				return ToDateTime (value, defaultDateTimeFormats);
			}
		}
#endif
		public static DateTime ToDateTime(string s, string format)
		{
			//DateTimeFormatInfo d = new DateTimeFormatInfo();
			//d.FullDateTimePattern = format;
			//return DateTime.Parse(s, d);
			DateTimeStyles style = DateTimeStyles.AllowLeadingWhite |
					       DateTimeStyles.AllowTrailingWhite;			
			return DateTime.ParseExact (s, format, DateTimeFormatInfo.InvariantInfo, style);
		}

		public static DateTime ToDateTime(string s, string[] formats)
		{
			return ToDateTime (s, formats, _defaultStyle);			
		}

		private static DateTime ToDateTime (string s, string [] formats, DateTimeStyles style) 
		{
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

			float f = TryParseStringFloatConstants (s);
			if (f != 0)
				return f;

			return Double.Parse (s, floatStyle, CultureInfo.InvariantCulture);
		}

		static float TryParseStringFloatConstants (string s)
		{
			int sidx = 0;
			while (sidx < s.Length && Char.IsWhiteSpace (s [sidx]))
				sidx++;
			if (sidx == s.Length)
				throw new FormatException ();
			int sEndPos = s.Length - 1;
			while (Char.IsWhiteSpace (s [sEndPos]))
				sEndPos--;

			if (TryParseStringConstant ("NaN", s, sidx, sEndPos))
				return Single.NaN;
			if (TryParseStringConstant ("INF", s, sidx, sEndPos))
				return Single.PositiveInfinity;
			if (TryParseStringConstant ("-INF", s, sidx, sEndPos))
				return Single.NegativeInfinity;
			// Handle these here because Single.Parse("Infinity") is invalid while XmlConvert.ToSingle("Infinity") is valid.
			if (TryParseStringConstant ("Infinity", s, sidx, sEndPos))
				return Single.PositiveInfinity;
			if (TryParseStringConstant ("-Infinity", s, sidx, sEndPos))
				return Single.NegativeInfinity;
			return 0;
		}

		static bool TryParseStringConstant (string format, string s, int start, int end)
		{
			return end - start + 1 == format.Length && String.CompareOrdinal (format, 0, s, start, format.Length) == 0;
		}

		public static Guid ToGuid (string s)
		{
			try {
				return new Guid(s);
			} catch (FormatException ex) {
				throw new FormatException (String.Format ("Invalid Guid input '{0}'", ex.InnerException));
			}
		}

		public static short ToInt16(string s)
		{
			return Int16.Parse (s, integerStyle, CultureInfo.InvariantCulture);
		}

		public static int ToInt32(string s)
		{
			return Int32.Parse (s, integerStyle, CultureInfo.InvariantCulture);
		}

		public static long ToInt64(string s)
		{
			return Int64.Parse (s, integerStyle, CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static SByte ToSByte(string s)
		{
			return SByte.Parse(s, integerStyle, CultureInfo.InvariantCulture);
		}

		public static float ToSingle(string s)
		{
			if (s == null)
				throw new ArgumentNullException();

			float f = TryParseStringFloatConstants (s);
			if (f != 0)
				return f;

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
			if (value == TimeSpan.Zero)
				return "PT0S";

			StringBuilder builder = new StringBuilder ();
			if (value.Ticks < 0) {
				if (value == TimeSpan.MinValue)
					return "-P10675199DT2H48M5.4775808S";  // There's one fewer tick on the positive side, so we cannot Negate this value; just hard-code it
				builder.Append ('-');
				value = value.Negate ();
			}
			builder.Append ('P');
			if (value.Days > 0)
				builder.Append (value.Days).Append ('D');
			long ticks = value.Ticks % TimeSpan.TicksPerMillisecond;
			if (value.Days > 0 || value.Hours > 0 || value.Minutes > 0 || value.Seconds > 0 || value.Milliseconds > 0 || ticks > 0) {
				builder.Append('T');
				if (value.Hours > 0)
					builder.Append (value.Hours).Append ('H');
				if (value.Minutes > 0) 
					builder.Append (value.Minutes).Append ('M');
				if (value.Seconds > 0 || value.Milliseconds > 0 || ticks > 0) {
					builder.Append (value.Seconds);
					bool trimZero = true;
					if (ticks > 0)
						builder.Append ('.').AppendFormat ("{0:0000000}", value.Ticks % TimeSpan.TicksPerSecond);
					else if (value.Milliseconds > 0)
						builder.Append ('.').AppendFormat ("{0:000}", value.Milliseconds);
					else
						trimZero = false;
					if (trimZero)
						while (builder [builder.Length - 1] == '0')
							builder.Remove (builder.Length - 1, 1);

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
			return value.ToString("R", CultureInfo.InvariantCulture);
		}

		public static string ToString(float value)
		{
			if (Single.IsNegativeInfinity(value)) return "-INF";
			if (Single.IsPositiveInfinity(value)) return "INF";
			if (Single.IsNaN(value)) return "NaN";
			return value.ToString("R", CultureInfo.InvariantCulture);
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
			// Unlike usual DateTime formatting, it preserves
			// MaxValue/MinValue as is.
			switch (mode) {
			case XmlDateTimeSerializationMode.Local:
				return (value == DateTime.MinValue ? DateTime.MinValue : value == DateTime.MaxValue ? value : value.ToLocalTime ()).ToString (
					"yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz",
					CultureInfo.InvariantCulture);
			case XmlDateTimeSerializationMode.RoundtripKind:
				return value.ToString (
					"yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
					CultureInfo.InvariantCulture);
			default:
				return value.ToString (
					"yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz",
					CultureInfo.InvariantCulture);
			case XmlDateTimeSerializationMode.Utc:
				return (value == DateTime.MinValue ? DateTime.MinValue : value == DateTime.MaxValue ? value : value.ToUniversalTime ()).ToString (
					"yyyy-MM-ddTHH:mm:ss.FFFFFFFZ",
					CultureInfo.InvariantCulture);
			case XmlDateTimeSerializationMode.Unspecified:
				return value.ToString (
					"yyyy-MM-ddTHH:mm:ss.FFFFFFF",
					CultureInfo.InvariantCulture);
			}
		}
#endif

		public static string ToString(DateTime value, string format)
		{
			return value.ToString(format, CultureInfo.InvariantCulture);
		}

		public static TimeSpan ToTimeSpan(string s)
		{
			s = s.Trim (XmlChar.WhitespaceChars);
			if (s.Length == 0)
				throw new FormatException ("Invalid format string for duration schema datatype.");

			int start = 0;
			if (s [0] == '-')
				start = 1;
			bool minusValue = (start == 1);

			if (s [start] != 'P')
				throw new FormatException ("Invalid format string for duration schema datatype.");
			start++;

			int parseStep = 0;
			int days = 0;
			bool isTime = false;
			int hours = 0;
			int minutes = 0;
			int seconds = 0;
			long ticks = 0;
			int parsedDigits = 0;

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
				for (; i < s.Length; i++)
					if (s [i] < '0' || '9' < s [i])
						break;
				if (parseStep == 7)
					parsedDigits = i - start;
				int value = int.Parse (s.Substring (start, i - start), CultureInfo.InvariantCulture);
				if (parseStep == 7) {
					// adjust to 7 digits so that it makes sense as millisecond digits
					for (; parsedDigits > 7; parsedDigits--)
						value /= 10;
					for (; parsedDigits < 7; parsedDigits++)
						value *= 10;
				}
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
					if (parseStep == 7)
						ticks = value;
					else
						seconds = value;
					if (!isTime || parseStep > 7)
						error = true;
					else
						parseStep = 8;
					break;
				case '.':
					if (parseStep > 7)
						error = true;
					seconds = value;
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
				throw new FormatException ("Invalid format string for duration schema datatype.");
			TimeSpan ts = new TimeSpan (days, hours, minutes, seconds);
			if (minusValue)
				return TimeSpan.FromTicks (- (ts.Ticks + ticks));
			else
				return TimeSpan.FromTicks (ts.Ticks + ticks);
		}

		[CLSCompliant (false)]
		public static UInt16 ToUInt16(string s)
		{
			return UInt16.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static UInt32 ToUInt32(string s)
		{
			return UInt32.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
		}

		[CLSCompliant (false)]
		public static UInt64 ToUInt64(string s)
		{
			return UInt64.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);
		}

		public static string VerifyName (string name)
		{
			if (name == null || name.Length == 0)
				throw new ArgumentNullException("name");

			if (!XmlChar.IsName (name))
				throw new XmlException("'" + name + "' is not a valid XML Name");
			return name;
			
		}

		public static string VerifyNCName (string ncname)
		{
			if (ncname == null || ncname.Length == 0)
				throw new ArgumentNullException("ncname");

			if (!XmlChar.IsNCName (ncname))
				throw new XmlException ("'" + ncname + "' is not a valid XML NCName");
			return ncname;
		}

#if NET_2_0
		public static string VerifyTOKEN (string name)
#else
		internal static string VerifyTOKEN (string name)
#endif
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (name.Length == 0)
				return name;

			if (XmlChar.IsWhitespace (name [0]) ||
				XmlChar.IsWhitespace (name [name.Length - 1]))
				throw new XmlException ("Whitespace characters (#xA, #xD, #x9, #x20) are not allowed as leading or trailing whitespaces of xs:token.");

			for (int i = 0; i < name.Length; i++)
				if (XmlChar.IsWhitespace (name [i]) && name [i] != ' ')
				throw new XmlException ("Either #xA, #xD or #x9 are not allowed inside xs:token.");

			return name;
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

#if NET_2_0 // actually NET_3_5
#if !TARGET_JVM

		public static DateTimeOffset ToDateTimeOffset (string s)
		{
			return ToDateTimeOffset (s, datetimeFormats);
		}

		public static DateTimeOffset ToDateTimeOffset (string s, string format)
		{
			return DateTimeOffset.ParseExact (s, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
		}

		public static DateTimeOffset ToDateTimeOffset (string s, string [] formats)
		{
			DateTimeStyles style = DateTimeStyles.AllowLeadingWhite |
					       DateTimeStyles.AllowTrailingWhite |
					       DateTimeStyles.AssumeUniversal;
			return DateTimeOffset.ParseExact (s, formats, CultureInfo.InvariantCulture, style);
		}

		public static string ToString (DateTimeOffset value)
		{
			return ToString (value, "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz");
		}

		public static string ToString (DateTimeOffset value, string format)
		{
			return value.ToString (format, CultureInfo.InvariantCulture);
		}
#endif

		// it is used only from 2.1 System.Xml.Serialization.dll from
		// MS Silverlight SDK. We don't use it so far.
		internal static Uri ToUri (string s)
		{
			return new Uri (s, UriKind.RelativeOrAbsolute);
		}

#endif
	}
}
