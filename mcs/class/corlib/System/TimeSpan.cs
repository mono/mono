//
// System.TimeSpan.cs
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (C) 2001 Duco Fijma
//

using System.Globalization;

namespace System {

[Serializable]
public struct TimeSpan :  IComparable  {

	private long _ticks;

	public TimeSpan (long value) { _ticks = value; }
	public TimeSpan (int hours, int minutes, int seconds) 
		: this(false, 0, hours, minutes, seconds, 0, 0) {}
	public TimeSpan (int days, int hours, int minutes, int seconds) 
		: this(false, days, hours, minutes, seconds, 0, 0) {}
	public TimeSpan (int days, int hours, int minutes, int seconds, int milliseconds)
		: this(false, days, hours, minutes, seconds, milliseconds, 0) {}

	internal TimeSpan (bool sign, int days, int hours, int minutes, int seconds, int milliseconds, long ticks)
	{
		checked {
			_ticks = TicksPerDay * days + 
				TicksPerHour * hours +
				TicksPerMinute * minutes +
				TicksPerSecond * seconds +
				TicksPerMillisecond * milliseconds +
				ticks;
			if ( sign ) {
				_ticks = -_ticks;
			}
		}
	}
	
	public static readonly TimeSpan MaxValue = new TimeSpan (long.MaxValue);
	public static readonly TimeSpan MinValue = new TimeSpan (long.MinValue);
	public const long TicksPerDay = 864000000000L;
	public const long TicksPerHour = 36000000000L;
	public const long TicksPerMillisecond = 10000L;
	public const long TicksPerMinute = 600000000L;
	public const long TicksPerSecond = 10000000L;
	public static readonly TimeSpan Zero = new TimeSpan (0L);

	public int Days
	{
		get {
			return (int) TotalDays;
		}
	}

	public int Hours
	{
		get {
			return (int) (_ticks % TicksPerDay / TicksPerHour);
		}
	}

	public int Milliseconds
	{
		get
		{
			return (int) (_ticks % TicksPerSecond / TicksPerMillisecond);
		}
	}

	public int Minutes
	{
		get
		{
			return (int) (_ticks % TicksPerHour / TicksPerMinute);
		}
	}

	public int Seconds
	{
		get
		{
			return (int) (_ticks % TicksPerMinute / TicksPerSecond);
		}
	}

	public long Ticks
	{ 
		get
		{
			return _ticks;
		}
	}

	public double TotalDays
	{
		get
		{
			return (double) _ticks / TicksPerDay;
		}
	}

	public double TotalHours
	{
		get
		{
			return (double) _ticks / TicksPerHour;
		}
	}

	public double TotalMilliseconds
	{
		get
		{
			return (double) _ticks  / TicksPerMillisecond;
		}
	}

	public double TotalMinutes
	{
		get {
			return (double) _ticks / TicksPerMinute;
		}
	}

	public double TotalSeconds
	{
		get {
			return (double) _ticks / TicksPerSecond;
		}
	}

	public TimeSpan Add (TimeSpan ts)
	{
		checked {
			return new TimeSpan (_ticks + ts.Ticks);
		}
	}

	public static int Compare (TimeSpan t1, TimeSpan t2)
	{
		if (t1._ticks < t2._ticks) {
			return -1;
		}
		else if (t1._ticks > t2._ticks) {
			return 1;
		}
		else {
			return 0;
		}
	}

	public int CompareTo (object value)
	{
		if (value == null )
			return 1;

		if (!(value is TimeSpan)) {
			throw new ArgumentException (Locale.GetText (
				"Argument of System.TimeSpan.CompareTo should be a TimeSpan"));
		}
	
		return Compare(this, (TimeSpan) value);
	}

	public TimeSpan Duration ()
	{
		checked {
			return new TimeSpan (Math.Abs (_ticks));
		}
	}

	public override bool Equals (object value)
	{
		if (!(value is TimeSpan)) {
			return false;
		}
		return Equals (this, (TimeSpan) value);
	}

	public static bool Equals (TimeSpan t1, TimeSpan t2)
	{
		return t1._ticks == t2._ticks;
	}

	// Implementing FromDays -> FromHours -> FromMinutes -> FromSeconds ->
	// FromMilliseconds as done here is probably not the most efficient
	// way. 
	public static TimeSpan FromDays (double value)
	{
		if (Double.IsNaN (value) || Double.IsNegativeInfinity (value)) {
			return MinValue;
		}

		if (Double.IsPositiveInfinity (value)) {
			return MaxValue;
		}

		return new TimeSpan ((int) value,0,0,0,0) + FromHours ((value - ((int) value)) * 24);
	}

	public static TimeSpan FromHours (double value)
	{
		if (Double.IsNaN (value) || Double.IsNegativeInfinity (value)) {
			return MinValue;
		}

		if (Double.IsPositiveInfinity (value)) {
			return MaxValue;
		}

		return new TimeSpan ((int) value,0,0) + FromMinutes ((value - ((int) value)) * 60);
	}

	public static TimeSpan FromMinutes (double value)
	{
		if (Double.IsNaN (value) || Double.IsNegativeInfinity (value)) {
			return MinValue;
		}

		if (Double.IsPositiveInfinity (value)) {
			return MaxValue;
		}

		return new TimeSpan (0, (int) value, 0) + FromSeconds((value - ((int) value)) * 60);
	}

	public static TimeSpan FromSeconds (double value)
	{
		if (Double.IsNaN (value) || Double.IsNegativeInfinity (value)) {
			return MinValue;
		}

		if (Double.IsPositiveInfinity (value)) {
			return MaxValue;
		}

		return new TimeSpan (0, 0, 0, (int) value) + FromMilliseconds((value - ((int) value)) * 1000);

	}

	public static TimeSpan FromMilliseconds (double value)
	{
		if (Double.IsNaN (value) || Double.IsNegativeInfinity (value)) {
			return MinValue;
		}

		if (Double.IsPositiveInfinity (value)) {
			return MaxValue;
		}

		return new TimeSpan (0, 0, 0, 0, (int) value);
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
               if (_ticks == long.MinValue)
                       throw new OverflowException ("This TimeSpan value is MinValue and cannot be negated.");
               return new TimeSpan (-_ticks);
	}

	public static TimeSpan Parse (string s)
	{
		if (s == null) {
			throw new ArgumentNullException (
				Locale.GetText ("null reference passed to TimeSpan.Parse"));
		}

		Parser p = new Parser (s); 
		return p.Execute ();
	}

	public TimeSpan Subtract (TimeSpan ts)
	{
		checked {
			return new TimeSpan (_ticks - ts.Ticks);
		}
	}

	public override string ToString ()
	{
		string res = "";	

		if (_ticks < 0) {
			res += "-";
		}

		// We need to take absolute values of all components.
		// Can't handle negative timespans by negating the TimeSpan
		// as a whole. This would lead to an overflow for the 
		// degenerate case "TimeSpan.MinValue.ToString()".
		if (Days != 0) {
			res += Math.Abs (Days) + "." ;
		}

		res += string.Format ("{0:D2}:{1:D2}:{2:D2}", Math.Abs(Hours), Math.Abs(Minutes), Math.Abs(Seconds));

		int fractional = (int) Math.Abs (_ticks % TicksPerSecond);
		if (fractional != 0) {
			res += string.Format (".{0:D7}", fractional);
		}
 
		return res;
	}

	public static TimeSpan operator + (TimeSpan t1, TimeSpan t2)
	{
		return t1.Add (t2);
	}

	public static bool operator == (TimeSpan t1, TimeSpan t2)
	{
		return Compare (t1, t2) == 0;
	}

	public static bool operator > (TimeSpan t1, TimeSpan t2)
	{
		return Compare (t1, t2) == 1;
	}

	public static bool operator >= (TimeSpan t1, TimeSpan t2)
	{
		return Compare (t1, t2) != -1;
	}

	public static bool operator != (TimeSpan t1, TimeSpan t2)
	{
		return Compare (t1, t2) != 0;
	}

	public static bool operator < (TimeSpan t1, TimeSpan t2)
	{
		return Compare (t1, t2) == -1;
	}

	public static bool operator <= (TimeSpan t1, TimeSpan t2)
	{
		return Compare (t1, t2) != 1;
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

// Class Parser implements simple parser for TimeSpan::Parse
internal class Parser {

	private string _src;
	private int _cur;
	private int _length;

	public Parser (string src)
	{
		_src = src;
		Reset ();
	}

	public void Reset ()
	{
		_cur = 0;
		_length = _src.Length;
	}

	public bool AtEnd
	{
		get {
			return _cur >= _length;
		}
	}

	private void ThrowFormatException() 
	{
		throw new FormatException (Locale.GetText ("Invalid format for TimeSpan.Parse"));
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
				res = res*10 + _src[_cur] - '0';
			}
			_cur++;
			count++;
		}
		
		if (count == 0) {
			ThrowFormatException ();
		}

		return res;
	}

	// Parse optional dot
	private bool ParseOptDot ()
	{
		if (AtEnd) {
			return false;
		}
		
		if (_src[_cur] == '.') {
			_cur++;
			return true;
		}
		else {
			return false;
		}
	}	
	
	// Parse NON-optional colon 
	private void ParseColon ()
	{
		if (!AtEnd && _src[_cur] == ':') {
			_cur++;
		}
		else {
			ThrowFormatException ();
		}
	}

	// Parse [1..7] digits, representing fractional seconds (ticks)
	private long ParseTicks ()
	{
		long mag = 1000000;
		long res = 0;
		bool digitseen = false;
		
		while ( mag > 0 && !AtEnd && Char.IsDigit (_src, _cur) ) {
			res = res + (_src[_cur] - '0') * mag;
			_cur++;
			mag = mag / 10;
			digitseen = true;
		}

		if (!digitseen) {
			ThrowFormatException ();
		}

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

		// Parse [ws][dd.]hh:mm:ss[.ff][ws]
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
		ParseColon();
		seconds = ParseInt ();
		if ( ParseOptDot () ) {
			ticks = ParseTicks ();
		}	
		else {
			ticks = 0;
		}
		ParseWhiteSpace ();

		if ( !AtEnd ) {
			ThrowFormatException ();
		}

		if ( hours > 23 || minutes > 59 || seconds > 59 ) {
			throw new OverflowException (Locale.GetText (
				"Value outside range in TimeSpan.Parse" ));
		}

		TimeSpan ts = new TimeSpan (sign, days, hours, minutes, seconds, 0, ticks);

		return ts;
	}	

}

} /* TimeSpan */
}


