//
// System.TimeSpan.cs
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 Duco Fijma
// (C) 2004 Andreas Nahr
//

using System.Text;

namespace System
{
	[Serializable]
	public struct TimeSpan : IComparable
	{
		public static readonly TimeSpan MaxValue = new TimeSpan (long.MaxValue);
		public static readonly TimeSpan MinValue = new TimeSpan (long.MinValue);
		public static readonly TimeSpan Zero = new TimeSpan (0L);

		public const long TicksPerDay = 864000000000L;
		public const long TicksPerHour = 36000000000L;
		public const long TicksPerMillisecond = 10000L;
		public const long TicksPerMinute = 600000000L;
		public const long TicksPerSecond = 10000000L;

		private long _ticks;

		public TimeSpan (long value)
		{
			_ticks = value;
		}

		public TimeSpan (int hours, int minutes, int seconds)
			: this (0, hours, minutes, seconds, 0)
		{
		}

		public TimeSpan (int days, int hours, int minutes, int seconds)
			: this (days, hours, minutes, seconds, 0)
		{
		}

		public TimeSpan (int days, int hours, int minutes, int seconds, int milliseconds)
		{
			try {
				checked {
					_ticks = TicksPerDay * days + 
						TicksPerHour * hours +
						TicksPerMinute * minutes +
						TicksPerSecond * seconds +
						TicksPerMillisecond * milliseconds;
				}
			}
			catch {
				throw new ArgumentOutOfRangeException (Locale.GetText ("The timespan is too big or too small."));
			}
		}

		private TimeSpan (bool sign, int days, int hours, int minutes, int seconds, long ticks)
		{
			try {
				checked {
					_ticks = TicksPerDay * days + 
						TicksPerHour * hours +
						TicksPerMinute * minutes +
						TicksPerSecond * seconds +
						ticks;
					if ( sign ) {
						_ticks = -_ticks;
					}
				}
			}
			catch {
				throw new ArgumentOutOfRangeException (Locale.GetText ("The timespan is too big or too small."));
			}
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
			catch {
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

		public TimeSpan Duration ()
		{
			try {
				checked {
					return new TimeSpan (Math.Abs (_ticks));
				}
			}
			catch {
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
			return FromMilliseconds (value * (TicksPerDay / TicksPerMillisecond));
		}

		public static TimeSpan FromHours (double value)
		{
			return FromMilliseconds (value * (TicksPerHour / TicksPerMillisecond));
		}

		public static TimeSpan FromMinutes (double value)
		{
			return FromMilliseconds (value * (TicksPerMinute / TicksPerMillisecond));
		}

		public static TimeSpan FromSeconds (double value)
		{
			return FromMilliseconds (value * (TicksPerSecond / TicksPerMillisecond));
		}

		public static TimeSpan FromMilliseconds (double value)
		{
			if (Double.IsNaN (value))
				throw new ArgumentException (Locale.GetText ("Value cannot be NaN."), "value");
			if (Double.IsNegativeInfinity (value))
				return MinValue;
			if (Double.IsPositiveInfinity (value))
				return MaxValue;

			try {
				checked {
					long val = (long) Math.Round(value);
					return new TimeSpan (val * TicksPerMillisecond);
				}
			}
			catch {
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

			Parser p = new Parser (s);
			return p.Execute ();
		}

		public TimeSpan Subtract (TimeSpan ts)
		{
			try {
				checked {
					return new TimeSpan (_ticks - ts.Ticks);
				}
			}
			catch {
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

			sb.Append (IntegerFormatter.FormatDecimal (Math.Abs (Hours), 2, 4));
			sb.Append (':');
			sb.Append (IntegerFormatter.FormatDecimal (Math.Abs (Minutes), 2, 4));
			sb.Append (':');
			sb.Append (IntegerFormatter.FormatDecimal (Math.Abs (Seconds), 2, 4));

			int fractional = (int) Math.Abs (_ticks % TicksPerSecond);
			if (fractional != 0) {
				sb.Append ('.');
				sb.Append (IntegerFormatter.FormatDecimal (Math.Abs (fractional), 7, 4));
			}

			return sb.ToString ();
		}

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

		// Class Parser implements parser for TimeSpan.Parse
		private class Parser
		{
			private string _src;
			private int _cur = 0;
			private int _length;

			public Parser (string src)
			{
				_src = src;
				_length = _src.Length;
			}
	
			public bool AtEnd {
				get {
					return _cur >= _length;
				}
			}

			private void ThrowFormatException ()
			{
				throw new FormatException (Locale.GetText ("Invalid format for TimeSpan.Parse."));
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

			// Parse simple int value
			private int ParseInt ()
			{
				int res = 0;
				int count = 0;

				while (!AtEnd && Char.IsDigit (_src, _cur)) {
					checked {
						res = res * 10 + _src[_cur] - '0';
					}
					_cur++;
					count++;
				}

				if (count == 0)
					ThrowFormatException ();

				return res;
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

			// Parse NON-optional colon
			private void ParseColon ()
			{
				if (!AtEnd && _src[_cur] == ':')
					_cur++;
				else 
					ThrowFormatException ();
			}

			// Parse [1..7] digits, representing fractional seconds (ticks)
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
					ThrowFormatException ();

				return res;
			}

			public TimeSpan Execute ()
			{
				bool sign;
				int days;
				int hours;
				int minutes;
				int seconds;
				long ticks;

				// Parse [ws][-][dd.]hh:mm:ss[.ff][ws]
				ParseWhiteSpace ();
				sign = ParseSign ();
				days = ParseInt ();
				if (ParseOptDot ()) {
					hours = ParseInt ();
				}
				else {
					hours = days;
					days = 0;
				}
				ParseColon();
				minutes = ParseInt ();
				ParseColon ();
				seconds = ParseInt ();
				if ( ParseOptDot () ) {
					ticks = ParseTicks ();
				}
				else {
					ticks = 0;
				}
				ParseWhiteSpace ();
	
				if (!AtEnd)
					ThrowFormatException ();

				if (hours > 23 || minutes > 59 || seconds > 59) {
					throw new OverflowException (Locale.GetText (
						"Invalid time data."));
				}

				TimeSpan ts = new TimeSpan (sign, days, hours, minutes, seconds, ticks);

				return ts;
			}
		}
	}
}
