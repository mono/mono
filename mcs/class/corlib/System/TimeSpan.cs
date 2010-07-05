//
// System.TimeSpan.cs
//
// Authors:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Duco Fijma
// (C) 2004 Andreas Nahr
// Copyright (C) 2004 Novell (http://www.novell.com)
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

using System.Text;
using System.Threading;
using System.Globalization;

namespace System
{
	[Serializable]
	[System.Runtime.InteropServices.ComVisible (true)]
	public struct TimeSpan : IComparable, IComparable<TimeSpan>, IEquatable <TimeSpan>
#if NET_4_0 || MOONLIGHT
				 , IFormattable
#endif
	{
#if MONOTOUCH
		static TimeSpan () {
			if (MonoTouchAOTHelper.FalseFlag) {
				var comparer = new System.Collections.Generic.GenericComparer <TimeSpan> ();
				var eqcomparer = new System.Collections.Generic.GenericEqualityComparer <TimeSpan> ();
			}
		}
#endif
		public static readonly TimeSpan MaxValue = new TimeSpan (long.MaxValue);
		public static readonly TimeSpan MinValue = new TimeSpan (long.MinValue);
		public static readonly TimeSpan Zero = new TimeSpan (0L);

		public const long TicksPerDay = 864000000000L;
		public const long TicksPerHour = 36000000000L;
		public const long TicksPerMillisecond = 10000L;
		public const long TicksPerMinute = 600000000L;
		public const long TicksPerSecond = 10000000L;

		private long _ticks;

		public TimeSpan (long ticks)
		{
			_ticks = ticks;
		}

		public TimeSpan (int hours, int minutes, int seconds)
		{
			CalculateTicks (0, hours, minutes, seconds, 0, true, out _ticks);
		}

		public TimeSpan (int days, int hours, int minutes, int seconds)
		{
			CalculateTicks (days, hours, minutes, seconds, 0, true, out _ticks);
		}

		public TimeSpan (int days, int hours, int minutes, int seconds, int milliseconds)
		{
			CalculateTicks (days, hours, minutes, seconds, milliseconds, true, out _ticks);
		}

		internal static bool CalculateTicks (int days, int hours, int minutes, int seconds, int milliseconds, bool throwExc, out long result)
		{
			// there's no overflow checks for hours, minutes, ...
			// so big hours/minutes values can overflow at some point and change expected values
			int hrssec = (hours * 3600); // break point at (Int32.MaxValue - 596523)
			int minsec = (minutes * 60);
			long t = ((long)(hrssec + minsec + seconds) * 1000L + (long)milliseconds);
			t *= 10000;

			result = 0;

			bool overflow = false;
			// days is problematic because it can overflow but that overflow can be 
			// "legal" (i.e. temporary) (e.g. if other parameters are negative) or 
			// illegal (e.g. sign change).
			if (days > 0) {
				long td = TicksPerDay * days;
				if (t < 0) {
					long ticks = t;
					t += td;
					// positive days -> total ticks should be lower
					overflow = (ticks > t);
				}
				else {
					t += td;
					// positive + positive != negative result
					overflow = (t < 0);
				}
			}
			else if (days < 0) {
				long td = TicksPerDay * days;
				if (t <= 0) {
					t += td;
					// negative + negative != positive result
					overflow = (t > 0);
				}
				else {
					long ticks = t;
					t += td;
					// negative days -> total ticks should be lower
					overflow = (t > ticks);
				}
			}

			if (overflow) {
				if (throwExc)
					throw new ArgumentOutOfRangeException (Locale.GetText ("The timespan is too big or too small."));
				return false;
			}

			result = t;
			return true;
		}

		public int Days {
			get {
				return (int) (_ticks / TicksPerDay);
			}
		}

		public int Hours {
			get {
				return (int) (_ticks % TicksPerDay / TicksPerHour);
			}
		}

		public int Milliseconds {
			get {
				return (int) (_ticks % TicksPerSecond / TicksPerMillisecond);
			}
		}

		public int Minutes {
			get {
				return (int) (_ticks % TicksPerHour / TicksPerMinute);
			}
		}

		public int Seconds {
			get {
				return (int) (_ticks % TicksPerMinute / TicksPerSecond);
			}
		}

		public long Ticks {
			get {
				return _ticks;
			}
		}

		public double TotalDays {
			get {
				return (double) _ticks / TicksPerDay;
			}
		}

		public double TotalHours {
			get {
				return (double) _ticks / TicksPerHour;
			}
		}

		public double TotalMilliseconds {
			get {
				return (double) _ticks  / TicksPerMillisecond;
			}
		}

		public double TotalMinutes {
			get {
				return (double) _ticks / TicksPerMinute;
			}
		}

		public double TotalSeconds {
			get {
				return (double) _ticks / TicksPerSecond;
			}
		}

		public TimeSpan Add (TimeSpan ts)
		{
			try {
				checked {
					return new TimeSpan (_ticks + ts.Ticks);
				}
			}
			catch (OverflowException) {
				throw new OverflowException (Locale.GetText ("Resulting timespan is too big."));
			}
		}

		public static int Compare (TimeSpan t1, TimeSpan t2)
		{
			if (t1._ticks < t2._ticks)
				return -1;
			if (t1._ticks > t2._ticks)
				return 1;
			return 0;
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if (!(value is TimeSpan)) {
				throw new ArgumentException (Locale.GetText ("Argument has to be a TimeSpan."), "value");
			}

			return Compare (this, (TimeSpan) value);
		}

		public int CompareTo (TimeSpan value)
		{
			return Compare (this, value);
		}

		public bool Equals (TimeSpan obj)
		{
			return obj._ticks == _ticks;
		}

		public TimeSpan Duration ()
		{
			try {
				checked {
					return new TimeSpan (Math.Abs (_ticks));
				}
			}
			catch (OverflowException) {
				throw new OverflowException (Locale.GetText (
					"This TimeSpan value is MinValue so you cannot get the duration."));
			}
		}

		public override bool Equals (object value)
		{
			if (!(value is TimeSpan))
				return false;

			return _ticks == ((TimeSpan) value)._ticks;
		}

		public static bool Equals (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks == t2._ticks;
		}

		public static TimeSpan FromDays (double value)
		{
			return From (value, TicksPerDay);
		}

		public static TimeSpan FromHours (double value)
		{
			return From (value, TicksPerHour);
		}

		public static TimeSpan FromMinutes (double value)
		{
			return From (value, TicksPerMinute);
		}

		public static TimeSpan FromSeconds (double value)
		{
			return From (value, TicksPerSecond);
		}

		public static TimeSpan FromMilliseconds (double value)
		{
			return From (value, TicksPerMillisecond);
		}

		private static TimeSpan From (double value, long tickMultiplicator) 
		{
			if (Double.IsNaN (value))
				throw new ArgumentException (Locale.GetText ("Value cannot be NaN."), "value");
			if (Double.IsNegativeInfinity (value) || Double.IsPositiveInfinity (value) ||
				(value < MinValue.Ticks) || (value > MaxValue.Ticks))
				throw new OverflowException (Locale.GetText ("Outside range [MinValue,MaxValue]"));

			try {
				value = (value * (tickMultiplicator / TicksPerMillisecond));

				checked {
					long val = (long) Math.Round(value);
					return new TimeSpan (val * TicksPerMillisecond);
				}
			}
			catch (OverflowException) {
				throw new OverflowException (Locale.GetText ("Resulting timespan is too big."));
			}
		}

		public static TimeSpan FromTicks (long value)
		{
			return new TimeSpan (value);
		}

		public override int GetHashCode ()
		{
			return _ticks.GetHashCode ();
		}

		public TimeSpan Negate ()
		{
			if (_ticks == MinValue._ticks)
				throw new OverflowException (Locale.GetText (
					"This TimeSpan value is MinValue and cannot be negated."));
			return new TimeSpan (-_ticks);
		}

		public static TimeSpan Parse (string s)
		{
			if (s == null) {
				throw new ArgumentNullException ("s");
			}

			TimeSpan result;
			Parser p = new Parser (s);
			p.Execute (false, out result);
			return result;
		}

		public static bool TryParse (string s, out TimeSpan result)
		{
			if (s == null) {
				result = TimeSpan.Zero;
				return false;
			}

			Parser p = new Parser (s);
			return p.Execute (true, out result);
		}

#if NET_4_0 || MOONLIGHT
		public static TimeSpan Parse (string s, IFormatProvider formatProvider)
		{
			if (s == null)
				throw new ArgumentNullException ("s");

			TimeSpan result;
			Parser p = new Parser (s, formatProvider);
			p.Execute (false, out result);
			return result;
		}

		public static bool TryParse (string s, IFormatProvider formatProvider, out TimeSpan result)
		{
			if (s == null || s.Length == 0) {
				result = TimeSpan.Zero;
				return false;
			}

			Parser p = new Parser (s, formatProvider);
			return p.Execute (true, out result);
		}

		public static TimeSpan ParseExact (string input, string format, IFormatProvider formatProvider)
		{
			if (format == null)
				throw new ArgumentNullException ("format");

			return ParseExact (input, new string [] { format }, formatProvider, TimeSpanStyles.None);
		}

		public static TimeSpan ParseExact (string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles)
		{
			if (format == null)
				throw new ArgumentNullException ("format");

			return ParseExact (input, new string [] { format }, formatProvider, styles);
		}

		public static TimeSpan ParseExact (string input, string [] formats, IFormatProvider formatProvider)
		{
			return ParseExact (input, formats, formatProvider, TimeSpanStyles.None);
		}

		public static TimeSpan ParseExact (string input, string [] formats, IFormatProvider formatProvider, TimeSpanStyles styles)
		{
			if (input == null)
				throw new ArgumentNullException ("input");
			if (formats == null)
				throw new ArgumentNullException ("formats");

			// All the errors found during the parsing process are reported as FormatException.
			TimeSpan result;
			if (!TryParseExact (input, formats, formatProvider, styles, out result))
				throw new FormatException ("Invalid format.");

			return result;
		}

		public static bool TryParseExact (string input, string format, IFormatProvider formatProvider, out TimeSpan result)
		{
			return TryParseExact (input, new string [] { format }, formatProvider, TimeSpanStyles.None, out result);
		}

		public static bool TryParseExact (string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles,
				out TimeSpan result)
		{
			return TryParseExact (input, new string [] { format }, formatProvider, styles, out result);
		}

		public static bool TryParseExact (string input, string [] formats, IFormatProvider formatProvider, out TimeSpan result)
		{
			return TryParseExact (input, formats, formatProvider, TimeSpanStyles.None, out result);
		}

		public static bool TryParseExact (string input, string [] formats, IFormatProvider formatProvider, TimeSpanStyles styles,
			out TimeSpan result)
		{
			result = TimeSpan.Zero;

			if (formats == null || formats.Length == 0)
				return false;

			Parser p = new Parser (input, formatProvider);
			p.Exact = true;

			foreach (string format in formats) {
				if (format == null || format.Length == 0)
					return false; // wrong format, return immediately.

				switch (format) {
					case "g":
						p.AllMembersRequired = false;
						p.CultureSensitive = true;
						p.UseColonAsDaySeparator = true;
						break;
					case "G":
						p.AllMembersRequired = true;
						p.CultureSensitive = true;
						p.UseColonAsDaySeparator = true;
						break;
					case "c":
						p.AllMembersRequired = false;
						p.CultureSensitive = false;
						p.UseColonAsDaySeparator = false;
						break;
					default:
						// Single letter formats other than the defined ones are not accepted.
						if (format.Length == 1)
							return false;
						// custom format
						if (p.ExecuteWithFormat (format, styles, true, out result))
							return true;
						continue;
				}

				if (p.Execute (true, out result))
					return true;
			}

			return false;
		}
#endif

		public TimeSpan Subtract (TimeSpan ts)
		{
			try {
				checked {
					return new TimeSpan (_ticks - ts.Ticks);
				}
			}
			catch (OverflowException) {
				throw new OverflowException (Locale.GetText ("Resulting timespan is too big."));
			}
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder (14);
			
			if (_ticks < 0)
				sb.Append ('-');

			// We need to take absolute values of all components.
			// Can't handle negative timespans by negating the TimeSpan
			// as a whole. This would lead to an overflow for the 
			// degenerate case "TimeSpan.MinValue.ToString()".
			if (Days != 0) {
				sb.Append (Math.Abs (Days));
				sb.Append ('.');
			}

			sb.Append (Math.Abs (Hours).ToString ("D2"));
			sb.Append (':');
			sb.Append (Math.Abs (Minutes).ToString ("D2"));
			sb.Append (':');
			sb.Append (Math.Abs (Seconds).ToString ("D2"));

			int fractional = (int) Math.Abs (_ticks % TicksPerSecond);
			if (fractional != 0) {
				sb.Append ('.');
				sb.Append (fractional.ToString ("D7"));
			}

			return sb.ToString ();
		}

#if NET_4_0 || MOONLIGHT
		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider formatProvider)
		{
			if (format == null || format.Length == 0 || format == "c" ||
					format == "t" || format == "T") // Default version
				return ToString ();

			if (format != "g" && format != "G")
				return ToStringCustom (format); // custom formats ignore culture/formatProvider

			NumberFormatInfo number_info = null;
			if (formatProvider != null)
				number_info = (NumberFormatInfo)formatProvider.GetFormat (typeof (NumberFormatInfo));
			if (number_info == null)
				number_info = Thread.CurrentThread.CurrentCulture.NumberFormat;

			string decimal_separator = number_info.NumberDecimalSeparator;
			int days, hours, minutes, seconds, milliseconds, fractional;

			days = Math.Abs (Days);
			hours = Math.Abs (Hours);
			minutes = Math.Abs (Minutes);
			seconds = Math.Abs (Seconds);
			milliseconds = Math.Abs (Milliseconds);
			fractional = (int) Math.Abs (_ticks % TicksPerSecond);

			// Set Capacity depending on whether it's long or shot format
			StringBuilder sb = new StringBuilder (format == "g" ? 16 : 32);
			if (_ticks < 0)
				sb.Append ('-');

			switch (format) {
				case "g": // short version
					if (days != 0) {
						sb.Append (days.ToString ());
						sb.Append (':');
					}
					sb.Append (hours.ToString ());
					sb.Append (':');
					sb.Append (minutes.ToString ("D2"));
					sb.Append (':');
					sb.Append (seconds.ToString ("D2"));
					if (milliseconds != 0) {
						sb.Append (decimal_separator);
						sb.Append (milliseconds.ToString ("D3"));
					}
					break;
				case "G": // long version
					sb.Append (days.ToString ("D1"));
					sb.Append (':');
					sb.Append (hours.ToString ("D2"));
					sb.Append (':');
					sb.Append (minutes.ToString ("D2"));
					sb.Append (':');
					sb.Append (seconds.ToString ("D2"));
					sb.Append (decimal_separator);
					sb.Append (fractional.ToString ("D7"));
					break;
			}

			return sb.ToString ();
		}

		string ToStringCustom (string format)
		{
			// Single char formats are not accepted.
			if (format.Length < 2)
				throw new FormatException ("The format is not recognized.");

			FormatParser parser = new FormatParser (format);
			FormatElement element;
			int value;

			StringBuilder sb = new StringBuilder (format.Length + 1);

			for (;;) {
				if (parser.AtEnd)
					break;

				element = parser.GetNextElement ();
				switch (element.Type) {
					case FormatElementType.Days:
						value = Math.Abs (Days);
						sb.Append (value.ToString ("D" + element.IntValue));
						break;
					case FormatElementType.Hours:
						value = Math.Abs (Hours);
						sb.Append (value.ToString ("D" + element.IntValue));
						break;
					case FormatElementType.Minutes:
						value = Math.Abs (Minutes);
						sb.Append (value.ToString ("D" + element.IntValue));
						break;
					case FormatElementType.Seconds:
						value = Math.Abs (Seconds);
						sb.Append (value.ToString ("D" + element.IntValue));
						break;
					case FormatElementType.Ticks:
						value = Math.Abs (Milliseconds);
						sb.Append (value.ToString ("D" + element.IntValue));
						break;
					case FormatElementType.TicksUppercase:
						value = Math.Abs (Milliseconds);
						if (value > 0) {
							int threshold = (int)Math.Pow (10, element.IntValue);
							while (value >= threshold)
								value /= 10;
							sb.Append (value.ToString ());
						}
						break;
					case FormatElementType.EscapedChar:
						sb.Append (element.CharValue);
						break;
					case FormatElementType.Literal:
						sb.Append (element.StringValue);
						break;
					default:
						throw new FormatException ("The format is not recognized.");
				}
			}

			return sb.ToString ();
		}
#endif

		public static TimeSpan operator + (TimeSpan t1, TimeSpan t2)
		{
			return t1.Add (t2);
		}

		public static bool operator == (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks == t2._ticks;
		}

		public static bool operator > (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks > t2._ticks;
		}

		public static bool operator >= (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks >= t2._ticks;
		}

		public static bool operator != (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks != t2._ticks;
		}

		public static bool operator < (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks < t2._ticks;
		}

		public static bool operator <= (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks <= t2._ticks;
		}

		public static TimeSpan operator - (TimeSpan t1, TimeSpan t2)
		{
			return t1.Subtract (t2);
		}

		public static TimeSpan operator - (TimeSpan t)
		{
			return t.Negate ();
		}

		public static TimeSpan operator + (TimeSpan t)
		{
			return t;
		}

		enum ParseError {
			None,
			Format,
			Overflow
		}

		// Class Parser implements parser for TimeSpan.Parse
		private class Parser
		{
			private string _src;
			private int _cur = 0;
			private int _length;
			ParseError parse_error;
#if NET_4_0 || MOONLIGHT
			bool parsed_ticks;
			NumberFormatInfo number_format;
			int parsed_numbers_count;
			bool parsed_days_separator;

			public bool Exact; // no fallback, strict pattern.
			public bool AllMembersRequired;
			public bool CultureSensitive = true;
			public bool UseColonAsDaySeparator = true;
#endif

			public Parser (string src)
			{
				_src = src;
				_length = _src.Length;
#if NET_4_0 || MOONLIGHT
				number_format = GetNumberFormatInfo (null);
#endif
			}

#if NET_4_0 || MOONLIGHT
			// Reset state data, so we can execute another parse over the input.
			void Reset ()
			{
				_cur = 0;
				parse_error = ParseError.None;
				parsed_ticks = parsed_days_separator = false;
				parsed_numbers_count = 0;
			}

			public Parser (string src, IFormatProvider formatProvider) :
				this (src)
			{
				number_format = GetNumberFormatInfo (formatProvider);
			}

			NumberFormatInfo GetNumberFormatInfo (IFormatProvider formatProvider)
			{
				NumberFormatInfo format = null;
				if (formatProvider != null)
					format = (NumberFormatInfo) formatProvider.GetFormat (typeof (NumberFormatInfo));
				if (format == null)
					format = Thread.CurrentThread.CurrentCulture.NumberFormat;

				return format;
			}
#endif
	
			public bool AtEnd {
				get {
					return _cur >= _length;
				}
			}

			// All "Parse" functions throw a FormatException on syntax error.
			// Their return value is semantic value of the item parsed.

			// Range checking is spread over three different places:
			// 1) When parsing "int" values, an exception is thrown immediately
			//    when the value parsed exceeds the maximum value for an int.
			// 2) An explicit check is built in that checks for hours > 23 and
			//    for minutes and seconds > 59.
			// 3) Throwing an exceptions for a final TimeSpan value > MaxValue
			//    or < MinValue is left to the TimeSpan constructor called.

			// Parse zero or more whitespace chars.
			private void ParseWhiteSpace ()
			{
				while (!AtEnd && Char.IsWhiteSpace (_src, _cur)) {
					_cur++;
				}
			}

			// Parse optional sign character.
			private bool ParseSign ()
			{
				bool res = false;

				if (!AtEnd && _src[_cur] == '-') {
					res = true;
					_cur++;
				}

				return res;
			}

#if NET_4_0 || MOONLIGHT
			// Used for custom formats parsing, where we may need to declare how
			// many digits we expect, as well as the maximum allowed.
			private int ParseIntExact (int digit_count, int max_digit_count)
			{
				long res = 0;
				int count = 0;

				// We can have more than one preceding zero here.
				while (!AtEnd && Char.IsDigit (_src, _cur)) {
					res = res * 10 + _src [_cur] - '0';
					if (res > Int32.MaxValue) {
						SetParseError (ParseError.Format);
						break;
					}
					_cur++;
					count++;
				}

				// digit_count = 1 means we can use up to maximum count,
				if (count == 0 || (digit_count > 1 && digit_count != count) ||
						count > max_digit_count)
					SetParseError (ParseError.Format);

				return (int)res;
			}
#endif

			// Parse simple int value
			private int ParseInt (bool optional)
			{
				if (optional && AtEnd)
					return 0;

				long res = 0;
				int count = 0;

				while (!AtEnd && Char.IsDigit (_src, _cur)) {
					res = res * 10 + _src[_cur] - '0';
					if (res > Int32.MaxValue) {
						SetParseError (ParseError.Overflow);
						break;
					}
					_cur++;
					count++;
				}

				if (!optional && (count == 0))
					SetParseError (ParseError.Format);
#if NET_4_0 || MOONLIGHT
				if (count > 0)
					parsed_numbers_count++;
#endif

				return (int)res;
			}

			// Parse optional dot
			private bool ParseOptDot ()
			{
				if (AtEnd)
					return false;

				if (_src[_cur] == '.') {
					_cur++;
					return true;
				}
				return false;
			}	

#if NET_4_0 || MOONLIGHT
			// This behaves pretty much like ParseOptDot, but we need to have it
			// as a separated routine for both days and decimal separators.
			private bool ParseOptDaysSeparator ()
			{
				if (AtEnd)
					return false;

				if (_src[_cur] == '.') {
					_cur++;
					parsed_days_separator = true;
					return true;
				}
				return false;
			}

			// Just as ParseOptDot, but for decimal separator
			private bool ParseOptDecimalSeparator ()
			{
				if (AtEnd)
					return false;

				// we may need to provide compatibility with old versions using '.'
				// for culture insensitve and non exact formats.
				if (!Exact || !CultureSensitive)
					if (_src [_cur] == '.') {
						_cur++;
						return true;
					}

				string decimal_separator = number_format.NumberDecimalSeparator;
				if (CultureSensitive && String.Compare (_src, _cur, decimal_separator, 0, decimal_separator.Length) == 0) {
					_cur += decimal_separator.Length;
					return true;
				}

				return false;
			}

			private bool ParseLiteral (string value)
			{
				if (!AtEnd && String.Compare (_src, _cur, value, 0, value.Length) == 0) {
					_cur += value.Length;
					return true;
				}

				return false;
			}

			private bool ParseChar (char c)
			{
				if (!AtEnd && _src [_cur] == c) {
					_cur++;
					return true;
				}

				return false;
			}
#endif

			private void ParseColon (bool optional)
			{
				if (!AtEnd) {
					if (_src[_cur] == ':')
						_cur++;
					else if (!optional)
						SetParseError (ParseError.Format);
				}
			}

			// Parse [1..7] digits, representing fractional seconds (ticks)
			// In 4.0 more than 7 digits will cause an OverflowException
			private long ParseTicks ()
			{
				long mag = 1000000;
				long res = 0;
				bool digitseen = false;
				
				while (mag > 0 && !AtEnd && Char.IsDigit (_src, _cur)) {
					res = res + (_src[_cur] - '0') * mag;
					_cur++;
					mag = mag / 10;
					digitseen = true;
				}

				if (!digitseen)
					SetParseError (ParseError.Format);
#if NET_4_0 || MOONLIGHT
				else if (!AtEnd && Char.IsDigit (_src, _cur))
					SetParseError (ParseError.Overflow);

				parsed_ticks = true;
#endif

				return res;
			}

#if NET_4_0 || MOONLIGHT
			// Used by custom formats parsing
			// digits_count = 0 for digits up to max_digits_count (optional), and other value to
			// force a precise number of digits.
			private long ParseTicksExact (int digits_count, int max_digits_count)
			{
				long mag = 1000000;
				long res = 0;
				int count = 0;

				while (mag > 0 && !AtEnd && Char.IsDigit (_src, _cur)) {
					res = res + (_src [_cur] - '0') * mag;
					_cur++;
					count++;
					mag = mag / 10;
				}

				if ((digits_count > 0 && count != digits_count) ||
						count > max_digits_count)
					SetParseError (ParseError.Format);

				return res;
			}
#endif

			void SetParseError (ParseError error)
			{
				// We preserve the very first error.
				if (parse_error != ParseError.None)
					return;

				parse_error = error;
			}

#if NET_4_0 || MOONLIGHT
			bool CheckParseSuccess (bool tryParse)
#else
			bool CheckParseSuccess (int hours, int minutes, int seconds, bool tryParse)
#endif
			{
				// We always report the first error, but for 2.0 we need to give a higher
				// precence to per-element overflow (as opposed to int32 overflow).
#if NET_4_0 || MOONLIGHT
				if (parse_error == ParseError.Overflow) {
#else
				if (parse_error == ParseError.Overflow || hours > 23 || minutes > 59 || seconds > 59) {
#endif
					if (tryParse)
						return false;
					throw new OverflowException (
						Locale.GetText ("Invalid time data."));
				}

				if (parse_error == ParseError.Format) {
					if (tryParse)
						return false;
					throw new FormatException (
						Locale.GetText ("Invalid format for TimeSpan.Parse."));
				}

				return true;
			}

#if NET_4_0 || MOONLIGHT
			// We are using a different parse approach in 4.0, due to some changes in the behaviour
			// of the parse routines.
			// The input string is documented as:
			// 	Parse [ws][-][dd.]hh:mm:ss[.ff][ws]
			//
			// There are some special cases as part of 4.0, however:
			// 1. ':' *can* be used as days separator, instead of '.', making valid the format 'dd:hh:mm:ss'
			// 2. A input in the format 'hh:mm:ss' will end up assigned as 'dd.hh:mm' if the first int has a value
			// exceeding the valid range for hours: 0-23.
			// 3. The decimal separator can be retrieved from the current culture, as well as keeping support
			// for the '.' value as part of keeping compatibility.
			//
			// So we take the approach to parse, if possible, 4 integers, and depending on both how many were
			// actually parsed and what separators were read, assign the values to days/hours/minutes/seconds.
			//
			public bool Execute (bool tryParse, out TimeSpan result)
			{
				bool sign;
				int value1, value2, value3, value4;
				int days, hours, minutes, seconds;
				long ticks = 0;

				result = TimeSpan.Zero;
				value1 = value2 = value3 = value4 = 0;
				days = hours = minutes = seconds = 0;

				Reset ();

				ParseWhiteSpace ();
				sign = ParseSign ();

				// Parse 4 integers, making only the first one non-optional.
				value1 = ParseInt (false);
				if (!ParseOptDaysSeparator ()) // Parse either day separator or colon
					ParseColon (false);
				int p = _cur;
				value2 = ParseInt (true);
				value3 = value4 = 0;
				if (p < _cur) {
					ParseColon (true);
					value3 = ParseInt (true);
					ParseColon (true);
					value4 = ParseInt (true);
				}

				// We know the precise separator for ticks, so there's no need to guess.
				if (ParseOptDecimalSeparator ())
					ticks = ParseTicks ();

				ParseWhiteSpace ();

				if (!AtEnd)
					SetParseError (ParseError.Format);

				if (Exact)
					// In Exact mode we cannot allow both ':' and '.' as day separator.
					if (UseColonAsDaySeparator && parsed_days_separator ||
						AllMembersRequired && (parsed_numbers_count < 4 || !parsed_ticks))
						SetParseError (ParseError.Format);

				switch (parsed_numbers_count) {
					case 1:
						days = value1;
						break;
					case 2: // Two elements are valid only if they are *exactly* in the format: 'hh:mm'
						if (parsed_days_separator)
							SetParseError (ParseError.Format);
						else {
							hours = value1;
							minutes = value2;
						}
						break;
					case 3: // Assign the first value to days if we parsed a day separator or the value
						// is not in the valid range for hours.
						if (parsed_days_separator || value1 > 23) {
							days = value1;
							hours = value2;
							minutes = value3;
						} else {
							hours = value1;
							minutes = value2;
							seconds = value3;
						}
						break;
					case 4: // We are either on 'dd.hh:mm:ss' or 'dd:hh:mm:ss'
						if (!UseColonAsDaySeparator && !parsed_days_separator)
							SetParseError (ParseError.Format);
						else {
							days = value1;
							hours = value2;
							minutes = value3;
							seconds = value4;
						}
						break;
				}

				if (hours > 23 || minutes > 59 || seconds > 59)
					SetParseError (ParseError.Overflow);

				if (!CheckParseSuccess (tryParse))
					return false;

				long t;
				if (!TimeSpan.CalculateTicks (days, hours, minutes, seconds, 0, false, out t))
					return false;

				try {
					t = checked ((sign) ? (-t - ticks) : (t + ticks));
				} catch (OverflowException) {
					if (tryParse)
						return false;
					throw;
				}

				result = new TimeSpan (t);
				return true;
			}
#else
			public bool Execute (bool tryParse, out TimeSpan result)
			{
				bool sign;
				int days;
				int hours = 0;
				int minutes;
				int seconds;
				long ticks;

				result = TimeSpan.Zero;

				// documented as...
				// Parse [ws][-][dd.]hh:mm:ss[.ff][ws]
				// ... but not entirely true as an lonely 
				// integer will be parsed as a number of days
				ParseWhiteSpace ();
				sign = ParseSign ();
				days = ParseInt (false);
				if (ParseOptDot ()) {
					hours = ParseInt (true);
				}
				else if (!AtEnd) {
					hours = days;
					days = 0;
				}
				ParseColon(false);
				int p = _cur;
				minutes = ParseInt (true);
				seconds = 0;
				if (p < _cur) {
					ParseColon (true);
					seconds = ParseInt (true);
				}

				if ( ParseOptDot () ) {
					ticks = ParseTicks ();
				}
				else {
					ticks = 0;
				}
				ParseWhiteSpace ();
	
				if (!AtEnd)
					SetParseError (ParseError.Format);

				if (!CheckParseSuccess (hours, minutes, seconds, tryParse))
					return false;

				long t;
				if (!TimeSpan.CalculateTicks (days, hours, minutes, seconds, 0, false, out t))
					return false;

				try {
					t = checked ((sign) ? (-t - ticks) : (t + ticks));
				} catch (OverflowException) {
					if (tryParse)
						return false;
					throw;
				}

				result = new TimeSpan (t);
				return true;
			}
#endif

#if NET_4_0 || MOONLIGHT
			public bool ExecuteWithFormat (string format, TimeSpanStyles style, bool tryParse, out TimeSpan result)
			{
				int days, hours, minutes, seconds;
				long ticks;
				FormatElement format_element;

				days = hours = minutes = seconds = -1;
				ticks = -1;
				result = TimeSpan.Zero;
				Reset ();

				FormatParser format_parser = new FormatParser (format);

				for (;;) {
					// We need to continue even if AtEnd == true, since we could have
					// a optional second element.
					if (parse_error != ParseError.None)
						break;
					if (format_parser.AtEnd)
						break;

					format_element = format_parser.GetNextElement ();
					switch (format_element.Type) {
						case FormatElementType.Days:
							if (days != -1)
								goto case FormatElementType.Error;
							days = ParseIntExact (format_element.IntValue, 8);
							break;
						case FormatElementType.Hours:
							if (hours != -1)
								goto case FormatElementType.Error;
							hours = ParseIntExact (format_element.IntValue, 2);
							break;
						case FormatElementType.Minutes:
							if (minutes != -1)
								goto case FormatElementType.Error;
							minutes = ParseIntExact (format_element.IntValue, 2);
							break;
						case FormatElementType.Seconds:
							if (seconds != -1)
								goto case FormatElementType.Error;
							seconds = ParseIntExact (format_element.IntValue, 2);
							break;
						case FormatElementType.Ticks:
							if (ticks != -1)
								goto case FormatElementType.Error;
							ticks = ParseTicksExact (format_element.IntValue,
									format_element.IntValue);
							break;
						case FormatElementType.TicksUppercase:
							// Similar to Milliseconds, but optional and the
							// number of F defines the max length, not the required one.
							if (ticks != -1)
								goto case FormatElementType.Error;
							ticks = ParseTicksExact (0, format_element.IntValue);
							break;
						case FormatElementType.Literal:
							if (!ParseLiteral (format_element.StringValue))
								SetParseError (ParseError.Format);
							break;
						case FormatElementType.EscapedChar:
							if (!ParseChar (format_element.CharValue))
								SetParseError (ParseError.Format);
							break;
						case FormatElementType.Error:
							SetParseError (ParseError.Format);
							break;
					}
				}

				if (days == -1)
					days = 0;
				if (hours == -1)
					hours = 0;
				if (minutes == -1)
					minutes = 0;
				if (seconds == -1)
					seconds = 0;
				if (ticks == -1)
					ticks = 0;

				if (!AtEnd || !format_parser.AtEnd)
					SetParseError (ParseError.Format);
				if (hours > 23 || minutes > 59 || seconds > 59)
					SetParseError (ParseError.Format);

				if (!CheckParseSuccess (tryParse))
					return false;

				long t;
				if (!TimeSpan.CalculateTicks (days, hours, minutes, seconds, 0, false, out t))
					return false;

				try {
					t = checked ((style == TimeSpanStyles.AssumeNegative) ? (-t - ticks) : (t + ticks));
				} catch (OverflowException) {
					if (tryParse)
						return false;
					throw;
				}

				result = new TimeSpan (t);
				return true;
			}
#endif
		}
#if NET_4_0 || MOONLIGHT
		enum FormatElementType 
		{
			Days,
			Hours,
			Minutes,
			Seconds,
			Ticks, // 'f'
			TicksUppercase, // 'F'
			Literal,
			EscapedChar,
			Error,
			End
		}

		struct FormatElement
		{
			public FormatElement (FormatElementType type)
			{
				Type = type;
				CharValue = (char)0;
				IntValue = 0;
				StringValue = null;
			}

			public FormatElementType Type;
			public char CharValue; // Used by EscapedChar
			public string StringValue; // Used by Literal
			public int IntValue; // Used by numerical elements.
		}

		class FormatParser 
		{
			int cur;
			string format;

			public FormatParser (string format)
			{
				this.format = format;
			}

			public bool AtEnd {
				get {
					return cur >= format.Length;
				}
			}

			public FormatElement GetNextElement ()
			{
				FormatElement element = new FormatElement ();

				if (AtEnd)
					return new FormatElement (FormatElementType.End);

				int count = 0;
				switch (format [cur]) {
					case 'd':
						count = ParseChar ('d');
						if (count > 8)
							return new FormatElement (FormatElementType.Error);
						element.Type = FormatElementType.Days;
						element.IntValue = count;
						break;
					case 'h':
						count = ParseChar ('h');
						if (count > 2)
							return new FormatElement (FormatElementType.Error);
						element.Type = FormatElementType.Hours;
						element.IntValue = count;
						break;
					case 'm':
						count = ParseChar ('m');
						if (count > 2)
							return new FormatElement (FormatElementType.Error);
						element.Type = FormatElementType.Minutes;
						element.IntValue = count;
						break;
					case 's':
						count = ParseChar ('s');
						if (count > 2)
							return new FormatElement (FormatElementType.Error);
						element.Type = FormatElementType.Seconds;
						element.IntValue = count;
						break;
					case 'f':
						count = ParseChar ('f');
						if (count > 7)
							return new FormatElement (FormatElementType.Error);
						element.Type = FormatElementType.Ticks;
						element.IntValue = count;
						break;
					case 'F':
						count = ParseChar ('F');
						if (count > 7)
							return new FormatElement (FormatElementType.Error);
						element.Type = FormatElementType.TicksUppercase;
						element.IntValue = count;
						break;
					case '%':
						cur++;
						if (AtEnd)
							return new FormatElement (FormatElementType.Error);
						if (format [cur] == 'd')
							goto case 'd';
						else if (format [cur] == 'h')
							goto case 'h';
						else if (format [cur] == 'm')
							goto case 'm';
						else if (format [cur] == 's')
							goto case 's';
						else if (format [cur] == 'f')
							goto case 'f';
						else if (format [cur] == 'F')
							goto case 'F';

						return new FormatElement (FormatElementType.Error);
					case '\'':
						string literal = ParseLiteral ();
						if (literal == null)
							return new FormatElement (FormatElementType.Error);
						element.Type = FormatElementType.Literal;
						element.StringValue = literal;
						break;
					case '\\':
						char escaped_char = ParseEscapedChar ();
						if ((int)escaped_char == 0)
							return new FormatElement (FormatElementType.Error);
						element.Type = FormatElementType.EscapedChar;
						element.CharValue = escaped_char;
						break;
					default:
						return new FormatElement (FormatElementType.Error);
				}

				return element;
			}

			int ParseChar (char c)
			{
				int count = 0;

				while (!AtEnd && format [cur] == c) {
					cur++;
					count++;
				}

				return count;
			}

			char ParseEscapedChar ()
			{
				if (AtEnd || format [cur] != '\\')
					return (char)0;

				cur++;
				if (AtEnd)
					return (char)0;

				return format [cur++];
			}

			string ParseLiteral ()
			{
				int start;
				int count = 0;

				if (AtEnd || format [cur] != '\'')
					return null;

				start = ++cur;
				while (!AtEnd && format [cur] != '\'') {
					cur++;
					count++;
				}

				if (!AtEnd && format [cur] == '\'') {
					cur++;
					return format.Substring (start, count);
				}

				return null;
			}
		}
#endif

	}
}
