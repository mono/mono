/*
 * System.DateTimeUtils
 *
 *  Copyright (C) 2007 Novell, Inc (http://www.novell.com) 
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Globalization;
using System.Text;

namespace System {
	internal static class DateTimeUtils {
		public static int CountRepeat (string fmt, int p, char c)
		{
			int l = fmt.Length;
			int i = p + 1;
			while ((i < l) && (fmt [i] == c)) 
				i++;
			
			return i - p;
		}

		public static unsafe void ZeroPad (StringBuilder output, int digits, int len)
		{
			// more than enough for an int
			char* buffer = stackalloc char [16];
			int pos = 16;
			
			do {
				buffer [-- pos] = (char) ('0' + digits % 10);
				digits /= 10;
				len --;
			} while (digits > 0);
			
			while (len -- > 0)
				buffer [-- pos] = '0';
			
			output.Append (new string (buffer, pos, 16 - pos));
		}

		static int ParseQuotedString (string fmt, int pos, StringBuilder output)
		{
			// pos == position of " or '
			
			int len = fmt.Length;
			int start = pos;
			char quoteChar = fmt [pos++];
			
			while (pos < len) {
				char ch = fmt [pos++];
				
				if (ch == quoteChar)
					return pos - start;
				
				if (ch == '\\') {
					// C-Style escape
					if (pos >= len)
						throw new FormatException("Un-ended quote");
	
					if (output != null)
						output.Append (fmt [pos++]);
				} else {
					if (output != null)
						output.Append (ch);
				}
			}

			throw new FormatException("Un-ended quote");
		}

		public static string GetStandardPattern (char format, DateTimeFormatInfo dfi, out bool useutc, out bool use_invariant)
		{
			return GetStandardPattern (format, dfi, out useutc, out use_invariant, false);
		}

		public static string GetStandardPattern (char format, DateTimeFormatInfo dfi, out bool useutc, out bool use_invariant, bool date_time_offset)
		{
			String pattern;

			useutc = false;
			use_invariant = false;

			switch (format)
			{
			case 'd':
				pattern = dfi.ShortDatePattern;
				break;
			case 'D':
				pattern = dfi.LongDatePattern;
				break;
			case 'f':
				pattern = dfi.LongDatePattern + " " + dfi.ShortTimePattern;
				break;
			case 'F':
				pattern = dfi.FullDateTimePattern;
				break;
			case 'g':
				pattern = dfi.ShortDatePattern + " " + dfi.ShortTimePattern;
				break;
			case 'G':
				pattern = dfi.ShortDatePattern + " " + dfi.LongTimePattern;
				break;
			case 'm':
			case 'M':
				pattern = dfi.MonthDayPattern;
				break;
			case 'o':
			case 'O':
				pattern = dfi.RoundtripPattern;
				use_invariant = true;
				break;
			case 'r':
			case 'R':
				pattern = dfi.RFC1123Pattern;
				if (date_time_offset)
					useutc = true;
				use_invariant = true;
				break;
			case 's':
				pattern = dfi.SortableDateTimePattern;
				use_invariant = true;
				break;
			case 't':
				pattern = dfi.ShortTimePattern;
				break;
			case 'T':
				pattern = dfi.LongTimePattern;
				break;
			case 'u':
				pattern = dfi.UniversalSortableDateTimePattern;
				if (date_time_offset)
					useutc = true;
				use_invariant = true;
				break;
			case 'U':
				if (date_time_offset)
					pattern = null;
				else {
//					pattern = dfi.LongDatePattern + " " + dfi.LongTimePattern;
					pattern = dfi.FullDateTimePattern;
					useutc = true;
				}
				break;
			case 'y':
			case 'Y':
				pattern = dfi.YearMonthPattern;
				break;
			default:
				pattern = null;
				break;
//				throw new FormatException (String.Format ("Invalid format pattern: '{0}'", format));
			}

			return pattern;
		}

		public static string ToString (DateTime dt, string format, DateTimeFormatInfo dfi)
		{
			return ToString (dt, null, format, dfi);
		}

		public static string ToString (DateTime dt, TimeSpan? utc_offset, string format, DateTimeFormatInfo dfi)
		{
			// the length of the format is usually a good guess of the number
			// of chars in the result. Might save us a few bytes sometimes
			// Add + 10 for cases like mmmm dddd
			StringBuilder result = new StringBuilder (format.Length + 10);

			// For some cases, the output should not use culture dependent calendar
			DateTimeFormatInfo inv = DateTimeFormatInfo.InvariantInfo;
			if (format == inv.RFC1123Pattern)
				dfi = inv;
			else if (format == inv.UniversalSortableDateTimePattern)
				dfi = inv;

			int i = 0;
			bool saw_day_specifier = false;

			while (i < format.Length) {
				int tokLen;
				bool omitZeros = false;
				char ch = format [i];

				switch (ch) {

				//
				// Time Formats
				//
				case 'h':
					// hour, [1, 12]
					tokLen = DateTimeUtils.CountRepeat (format, i, ch);

					int hr = dt.Hour % 12;
					if (hr == 0)
						hr = 12;

					DateTimeUtils.ZeroPad (result, hr, tokLen == 1 ? 1 : 2);
					break;
				case 'H':
					// hour, [0, 23]
					tokLen = DateTimeUtils.CountRepeat (format, i, ch);
					DateTimeUtils.ZeroPad (result, dt.Hour, tokLen == 1 ? 1 : 2);
					break;
				case 'm':
					// minute, [0, 59]
					tokLen = DateTimeUtils.CountRepeat (format, i, ch);
					DateTimeUtils.ZeroPad (result, dt.Minute, tokLen == 1 ? 1 : 2);
					break;
				case 's':
					// second [0, 29]
					tokLen = DateTimeUtils.CountRepeat (format, i, ch);
					DateTimeUtils.ZeroPad (result, dt.Second, tokLen == 1 ? 1 : 2);
					break;
				case 'F':
					omitZeros = true;
					goto case 'f';
				case 'f':
					// fraction of second, to same number of
					// digits as there are f's

					tokLen = DateTimeUtils.CountRepeat (format, i, ch);
					if (tokLen > 7)
						throw new FormatException ("Invalid Format String");

					int dec = (int)((long)(dt.Ticks % TimeSpan.TicksPerSecond) / (long) Math.Pow (10, 7 - tokLen));
					int startLen = result.Length;
					DateTimeUtils.ZeroPad (result, dec, tokLen);

					if (omitZeros) {
						while (result.Length > startLen && result [result.Length - 1] == '0')
							result.Length--;
						// when the value was 0, then trim even preceding '.' (!) It is fixed character.
						if (dec == 0 && startLen > 0 && result [startLen - 1] == '.')
							result.Length--;
					}

					break;
				case 't':
					// AM/PM. t == first char, tt+ == full
					tokLen = DateTimeUtils.CountRepeat (format, i, ch);
					string desig = dt.Hour < 12 ? dfi.AMDesignator : dfi.PMDesignator;

					if (tokLen == 1) {
						if (desig.Length >= 1)
							result.Append (desig [0]);
					}
					else
						result.Append (desig);

					break;
				case 'z':
					// timezone. t = +/-h; tt = +/-hh; ttt+=+/-hh:mm
					tokLen = DateTimeUtils.CountRepeat (format, i, ch);
					TimeSpan offset = 
						utc_offset ?? 
						TimeZone.CurrentTimeZone.GetUtcOffset (dt);

					if (offset.Ticks >= 0)
						result.Append ('+');
					else
						result.Append ('-');

					switch (tokLen) {
					case 1:
						result.Append (Math.Abs (offset.Hours));
						break;
					case 2:
						result.Append (Math.Abs (offset.Hours).ToString ("00"));
						break;
					default:
						result.Append (Math.Abs (offset.Hours).ToString ("00"));
						result.Append (':');
						result.Append (Math.Abs (offset.Minutes).ToString ("00"));
						break;
					}
					break;
				case 'K': // 'Z' (UTC) or zzz (Local)
					tokLen = 1;

					if (utc_offset != null || dt.Kind == DateTimeKind.Local) {
						offset = utc_offset ?? TimeZone.CurrentTimeZone.GetUtcOffset (dt);
						if (offset.Ticks >= 0)
							result.Append ('+');
						else
							result.Append ('-');
						result.Append (Math.Abs (offset.Hours).ToString ("00"));
						result.Append (':');
						result.Append (Math.Abs (offset.Minutes).ToString ("00"));
					} else if (dt.Kind == DateTimeKind.Utc)
						result.Append ('Z');
					break;
				//
				// Date tokens
				//
				case 'd':
					// day. d(d?) = day of month (leading 0 if two d's)
					// ddd = three leter day of week
					// dddd+ full day-of-week
					tokLen = DateTimeUtils.CountRepeat (format, i, ch);

					if (tokLen <= 2)
						DateTimeUtils.ZeroPad (result, dfi.Calendar.GetDayOfMonth (dt), tokLen == 1 ? 1 : 2);
					else if (tokLen == 3)
						result.Append (dfi.GetAbbreviatedDayName (dfi.Calendar.GetDayOfWeek (dt)));
					else
						result.Append (dfi.GetDayName (dfi.Calendar.GetDayOfWeek (dt)));

					saw_day_specifier = true;
					break;
				case 'M':
					// Month.m(m?) = month # (with leading 0 if two mm)
					// mmm = 3 letter name
					// mmmm+ = full name
					tokLen = DateTimeUtils.CountRepeat (format, i, ch);
					int month = dfi.Calendar.GetMonth(dt);
					if (tokLen <= 2)
						DateTimeUtils.ZeroPad (result, month, tokLen);
					else if (tokLen == 3)
						result.Append (dfi.GetAbbreviatedMonthName (month));
					else {
						// Handles MMMM dd format
						if (!saw_day_specifier) {
							for (int ii = i + 1; ii < format.Length; ++ii) {
								ch = format [ii];
								if (ch == 'd') {
									saw_day_specifier = true;
									break;
								}

								if (ch == '\'' || ch == '"') {
									ii += ParseQuotedString (format, ii, null) - 1;
								}
							}
						}

						// NOTE: .NET ignores quoted 'd' and reads it as day specifier but I think 
						// that's wrong
						result.Append (saw_day_specifier ? dfi.GetMonthGenitiveName (month) : dfi.GetMonthName (month));
					}

					break;
				case 'y':
					// Year. y(y?) = two digit year, with leading 0 if yy
					// yyy+ full year with leading zeros if needed.
					tokLen = DateTimeUtils.CountRepeat (format, i, ch);

					if (tokLen <= 2)
						DateTimeUtils.ZeroPad (result, dfi.Calendar.GetYear (dt) % 100, tokLen);
					else
						DateTimeUtils.ZeroPad (result, dfi.Calendar.GetYear (dt), tokLen);
					break;

				case 'g':
					// Era name
					tokLen = DateTimeUtils.CountRepeat (format, i, ch);
					result.Append (dfi.GetEraName (dfi.Calendar.GetEra (dt)));
					break;

				//
				// Other
				//
				case ':':
					result.Append (dfi.TimeSeparator);
					tokLen = 1;
					break;
				case '/':
					result.Append (dfi.DateSeparator);
					tokLen = 1;
					break;
				case '\'': case '"':
					tokLen = ParseQuotedString (format, i, result);
					break;
				case '%':
					if (i >= format.Length - 1)
						throw new FormatException ("% at end of date time string");
					if (format [i + 1] == '%')
						throw new FormatException ("%% in date string");

					// Look for the next char
					tokLen = 1;
					break;
				case '\\':
					// C-Style escape
					if (i >= format.Length - 1)
						throw new FormatException ("\\ at end of date time string");

					result.Append (format [i + 1]);
					tokLen = 2;

					break;
				default:
					// catch all
					result.Append (ch);
					tokLen = 1;
					break;
				}
				i += tokLen;
			}
			return result.ToString ();
		}
	
	}
}	
