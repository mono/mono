//
// System.TimeSpan.cs
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (C) 2001 Duco Fijma

namespace System {
	
	public struct TimeSpan :  IComparable  {
		private long ticks;

		// Ctors

		public TimeSpan (long value) { ticks = value; }
		public TimeSpan (int hours, int minutes, int seconds) 
			: this(0, hours, minutes, seconds, 0) {}
		public TimeSpan (int days, int hours, int minutes, int seconds) 
			: this(days, hours, minutes, seconds, 0) {}
		public TimeSpan (int days, int hours, int minutes, int seconds, int milliseconds)
		{
			ticks = TicksPerDay * days + 
				TicksPerHour * hours +
				TicksPerMinute * minutes +
				TicksPerSecond * seconds +
				TicksPerMillisecond * milliseconds;
		}
		
		// Fields

		public static readonly TimeSpan MaxValue = new TimeSpan (long.MaxValue);
		public static readonly TimeSpan MinValue = new TimeSpan (long.MinValue);
		public const long TicksPerDay = 864000000000L;
		public const long TicksPerHour = 36000000000L;
		public const long TicksPerMillisecond = 10000L;
		public const long TicksPerMinute = 600000000L;
		public const long TicksPerSecond = 10000000L;
		public static readonly TimeSpan Zero = new TimeSpan (0L);

		// Properties

		public int Days
		{
			get {
				return (int) TotalDays;
			}
		}

		public int Hours
		{
			get {
				return (int) (ticks % TicksPerDay / TicksPerHour);
			}
		}

		public int Milliseconds
		{
			get
			{
				return (int) (ticks % TicksPerSecond / TicksPerMillisecond);
			}
		}

		public int Minutes
		{
			get
			{
				return (int) (ticks % TicksPerHour / TicksPerMinute);
			}
		}

		public int Seconds
		{
			get
			{
				return (int) (ticks % TicksPerMinute / TicksPerSecond);
			}
		}

		public long Ticks
		{ 
			get
			{
				return ticks;
			}
		}

		public double TotalDays
		{
			get
			{
				return (double) ticks / TicksPerDay;
			}
		}

		public double TotalHours
		{
			get
			{
				return (double) ticks / TicksPerHour;
			}
		}

		public double TotalMilliseconds
		{
			get
			{
				return (double) ticks  / TicksPerMillisecond;
			}
		}

		public double TotalMinutes
		{
			get {
				return (double) ticks / TicksPerMinute;
			}
		}

		public double TotalSeconds
		{
			get {
				return (double) ticks / TicksPerSecond;
			}
		}

		// Methods

		public TimeSpan Add (TimeSpan ts)
		{
			checked {
				return new TimeSpan (ticks+ts.Ticks);
			}
		}

		public static int Compare (TimeSpan t1, TimeSpan t2)
		{
			if (t1.ticks < t2.ticks) {
				return -1;
			}
			else if (t1.ticks > t2.ticks) {
				return 1;
			}
			else {
				return 0;
			}
		}

		public int CompareTo (object value)
		{
			
			if (value == null ) {
				return 1;
			}

			if (!(value is TimeSpan)) {
				throw new ArgumentException("Argument of System.TimeSpan.CompareTo should be a TimeSpan");
			}
		
			return Compare(this, (TimeSpan) value);
		}

		// Overflow issue for Duration is like for Negate.
		public TimeSpan Duration ()
		{
			checked {
				return new TimeSpan (Math.Abs (ticks));
			}
		}

		// TODO: Consider implementing this version
		// in terms of Equals(TimeSPan, TimeSpan)
		public override bool Equals (object value)
		{
			if (value == null || !(value is TimeSpan)) {
				return false;
			}
			return ticks == ((TimeSpan) (value)).ticks;
		}

		public static new bool Equals(TimeSpan t1, TimeSpan t2)
		{
			return t1.Equals (t2);
		}

		// Implementing FromDays -> FromHours -> FromMinutes -> FromSeconds ->
		// FromMilliseconds as done here is probably not the most efficient
		// way. 

		public static TimeSpan FromDays (double value)
		{
			if (Double.IsNaN(value) || Double.IsNegativeInfinity(value)) {
				return MinValue;
			}

			if (Double.IsPositiveInfinity(value)) {
				return MaxValue;
			}

			return new TimeSpan((int) value,0,0,0,0) + FromHours ((value - ((int) value)) * 24);
		}

		public static TimeSpan FromHours (double value)
		{
			if (Double.IsNaN(value) || Double.IsNegativeInfinity(value)) {
				return MinValue;
			}

			if (Double.IsPositiveInfinity(value)) {
				return MaxValue;
			}

			return new TimeSpan ((int) value,0,0) + FromMinutes ((value - ((int) value)) * 60);
		}

		public static TimeSpan FromMinutes(double value)
		{
			if (Double.IsNaN(value) || Double.IsNegativeInfinity(value)) {
				return MinValue;
			}

			if (Double.IsPositiveInfinity(value)) {
				return MaxValue;
			}

			return new TimeSpan (0,(int) value,0) + FromSeconds((value - ((int) value)) * 60);
		}

		public static TimeSpan FromSeconds(double value)
		{
			if (Double.IsNaN(value) || Double.IsNegativeInfinity(value)) {
				return MinValue;
			}

			if (Double.IsPositiveInfinity(value)) {
				return MaxValue;
			}

			return new TimeSpan (0,0,0,(int) value,((int) ((value - ((int) value)) * 1000)));

		}

		public static TimeSpan FromTicks (long value)
		{
			return new TimeSpan (value);
		}

		public override int GetHashCode()
		{
			return ticks.GetHashCode();
		}

		// TODO: It makes sense that Negate can throw an overflow
		// exception (if negating MinValue). Is this specified
		// somewhere?
		public TimeSpan Negate()
		{
			checked {
				return new TimeSpan(- ticks);
			}
		}

		// TODO: implement
		public static TimeSpan Parse(string s) 
		{
			return Zero;
		}

		public TimeSpan Subtract(TimeSpan ts)
		{
			checked {
				return new TimeSpan(ticks - ts.Ticks);
			}
		}

		public override string ToString()
		{
			string res = "";	

			if (ticks < 0) {
				res += "-";
			}

			// We need to take absolute values of all components.
			// Can't handle negative timespans by negating the TimeSpan
			// as a whole. This would lead to an overflow for the 
			// degenerate case "TimeSpan.MinValue.ToString()".

			if (Days != 0) {
				res += Math.Abs(Days) + "." ;
			}

			res += string.Format("{0:00}:{1:00}:{2:00}", Math.Abs(Hours), Math.Abs(Minutes), Math.Abs(Seconds));

			int fractional = (int) Math.Abs(ticks % TicksPerSecond);
			if (fractional != 0) {
				res += string.Format(".{0:0000000}", fractional);
			}
 
			return res;
		}

		public static TimeSpan operator +(TimeSpan t1, TimeSpan t2)
		{
			return t1.Add(t2);
		}

		public static bool operator ==(TimeSpan t1, TimeSpan t2)
		{
			return Compare(t1, t2) == 0;
		}

		public static bool operator >(TimeSpan t1, TimeSpan t2)
		{
			return Compare(t1, t2) == 1;
		}

		public static bool operator >=(TimeSpan t1, TimeSpan t2)
		{
			return Compare(t1, t2) != -1;
		}

		public static bool operator !=(TimeSpan t1, TimeSpan t2)
		{
			return Compare(t1, t2) != 0;
		}

		public static bool operator <(TimeSpan t1, TimeSpan t2)
		{
			return Compare(t1, t2) == -1;
		}

		public static bool operator <=(TimeSpan t1, TimeSpan t2)
		{
			return Compare(t1, t2) != 1;
		}

		public static TimeSpan operator -(TimeSpan t1, TimeSpan t2)
		{
			return t1.Subtract(t2);
		}

		public static TimeSpan operator -(TimeSpan t)
		{
			return t.Negate();
		}

		public static TimeSpan operator +(TimeSpan t)
		{
			return t;
		}
	}
}

