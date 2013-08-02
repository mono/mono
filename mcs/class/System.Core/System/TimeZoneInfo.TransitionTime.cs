/*
 * System.TimeZoneInfo.TransitionTime
 *
 * Author(s)
 * 	Stephane Delcroix <stephane@delcroix.org>
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

#if (INSIDE_CORLIB && NET_4_0) || (!INSIDE_CORLIB && (NET_3_5 && !NET_4_0 && !MOBILE))

using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System
{
	public sealed partial class TimeZoneInfo 
	{
		[SerializableAttribute]
#if MOBILE
	[TypeForwardedFrom (Consts.AssemblySystem_Core)]
#elif NET_4_0
	[TypeForwardedFrom (Consts.AssemblySystemCore_3_5)]
#endif
		public struct TransitionTime : IEquatable<TimeZoneInfo.TransitionTime>, ISerializable, IDeserializationCallback
		{
			DateTime timeOfDay;
			public DateTime TimeOfDay {
				get { return timeOfDay; }
			}

			int month;
			public int Month {
				get { return month; }
			}

			int day;
			public int Day {
				get { 
#if STRICT
					if (!isFixedDateRule)
						throw new Exception ("Day property is not valid for floating date rules");
#endif
					return day; 
				}
			}

			int week;
			public int Week {
				get { 
#if STRICT
					if (isFixedDateRule)
						throw new Exception ("Week property is not valid for fixed date rules");
#endif
		
					return week; 
				}
			}

			DayOfWeek dayOfWeek;
			public DayOfWeek DayOfWeek {
				get { 
#if STRICT
					if (isFixedDateRule)
						throw new Exception ("DayOfWeek property is not valid for fixed date rules");
#endif
	
					return dayOfWeek; 
				}
			}

			bool isFixedDateRule;
			public bool IsFixedDateRule {
				get { return isFixedDateRule; }
			}

			public static TransitionTime CreateFixedDateRule (
				DateTime timeOfDay, 
				int month, 
				int day)
			{
				return new TransitionTime (timeOfDay, month, day);
			}

			public static TransitionTime CreateFloatingDateRule (
				DateTime timeOfDay,
				int month,
				int week,
				DayOfWeek dayOfWeek)
			{
				return new TransitionTime (timeOfDay, month, week, dayOfWeek);
			}

			private TransitionTime (SerializationInfo info, StreamingContext context)
			{
				if (info == null)
					throw new ArgumentNullException ("info");
				timeOfDay = (DateTime) info.GetValue ("TimeOfDay", typeof (DateTime));
				month = (byte) info.GetValue ("Month", typeof (byte));
				week = (byte) info.GetValue ("Week", typeof (byte));
				day = (byte) info.GetValue ("Day", typeof (byte));
				dayOfWeek = (DayOfWeek) info.GetValue ("DayOfWeek", typeof (DayOfWeek));
				isFixedDateRule = (bool) info.GetValue ("IsFixedDateRule", typeof (bool));

				if (isFixedDateRule)
				{
					week = -1;
					dayOfWeek = (DayOfWeek) (-1);
				}
				if (!isFixedDateRule)			
					day = -1;
			}

			private TransitionTime (
				DateTime timeOfDay,
				int month,
				int day) : this (timeOfDay, month)
			{
				if (day < 1 || day > 31)
					throw new ArgumentOutOfRangeException ("day parameter is less than 1 or greater than 31");

				this.day = day;	
				this.isFixedDateRule = true;
			}

			private TransitionTime (
				DateTime timeOfDay,
				int month,
				int week,
				DayOfWeek dayOfWeek)  : this (timeOfDay, month)
			{
				if (week < 1 || week > 5)
					throw new ArgumentOutOfRangeException ("week parameter is less than 1 or greater than 5");

				if (dayOfWeek != DayOfWeek.Sunday &&
						dayOfWeek != DayOfWeek.Monday &&
						dayOfWeek != DayOfWeek.Tuesday &&
						dayOfWeek != DayOfWeek.Wednesday &&
						dayOfWeek != DayOfWeek.Thursday &&
						dayOfWeek != DayOfWeek.Friday &&
						dayOfWeek != DayOfWeek.Saturday)
					throw new ArgumentOutOfRangeException ("dayOfWeek parameter is not a member od DayOfWeek enumeration");

				this.week = week;
				this.dayOfWeek = dayOfWeek;
				this.isFixedDateRule = false;
			}

			private TransitionTime (
				DateTime timeOfDay,
				int month)
			{
				if (timeOfDay.Year != 1 || timeOfDay.Month != 1 || timeOfDay.Day != 1)
					throw new ArgumentException ("timeOfDay parameter has a non-default date component");

				if (timeOfDay.Kind != DateTimeKind.Unspecified)
					throw new ArgumentException ("timeOfDay parameter Kind's property is not DateTimeKind.Unspecified");

				if (timeOfDay.Ticks % TimeSpan.TicksPerMillisecond != 0)
					throw new ArgumentException ("timeOfDay parameter does not represent a whole number of milliseconds");

				if (month < 1 || month > 12)
					throw new ArgumentOutOfRangeException ("month parameter is less than 1 or greater than 12");
				
				this.timeOfDay = timeOfDay;
				this.month = month;

				this.week = -1;
				this.dayOfWeek = (System.DayOfWeek)(-1);
				this.day = -1;
				this.isFixedDateRule = false;
			}

			public static bool operator == (TransitionTime t1, TransitionTime t2)
			{
				return ( t1.day == t2.day &&
						t1.dayOfWeek == t2.dayOfWeek &&
						t1.isFixedDateRule == t2.isFixedDateRule &&
						t1.month == t2.month &&
						t1.timeOfDay == t2.timeOfDay &&
						t1.week == t2.week);	
			}

			public static bool operator != (TransitionTime t1, TransitionTime t2)
			{
				return !(t1 == t2);
			}


#if NET_4_0
			void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
#else
			public void GetObjectData (SerializationInfo info, StreamingContext context)
#endif
			{
				if (info == null)
					throw new ArgumentNullException ("info");
				info.AddValue ("TimeOfDay", TimeOfDay);
				info.AddValue ("Month", System.Convert.ToByte(Month));
				if (week > -1)
					info.AddValue ("Week", System.Convert.ToByte(week));
				else 
					info.AddValue ("Week", (byte) 1);
				if (day > -1)
					info.AddValue ("Day", System.Convert.ToByte(day));
				else
					info.AddValue ("Day", (byte) 1);
				if (dayOfWeek !=  ((System.DayOfWeek) (-1)))
					info.AddValue ("DayOfWeek", dayOfWeek);
				else
					info.AddValue ("DayOfWeek", DayOfWeek.Sunday);
				info.AddValue ("IsFixedDateRule", IsFixedDateRule);
			}	
	
			public override bool Equals (object obj)
			{
				if (obj is TransitionTime)
					return this == (TransitionTime) obj;
				return false;
			}

			public bool Equals (TimeZoneInfo.TransitionTime other)
			{
				return this == other;
			}

			public override int GetHashCode ()
			{
				return (day ^ (int)dayOfWeek ^ month ^ (int)timeOfDay.Ticks ^ week);
			}

#if NET_4_0
			void IDeserializationCallback.OnDeserialization (object sender)
#else
			public void OnDeserialization (object sender)
#endif
			{
				try {
					TimeZoneInfo.TransitionTime.Validate (timeOfDay, month, week, day, dayOfWeek, isFixedDateRule);
				} catch (ArgumentException ex) {
					throw new SerializationException ("invalid serialization data", ex);
				}
			}

			private static void Validate (DateTime timeOfDay, int month,int week, int day, DayOfWeek dayOfWeek, bool isFixedDateRule)
			{
				if (timeOfDay.Year != 1 || timeOfDay.Month != 1 || timeOfDay.Day != 1)
					throw new ArgumentException ("timeOfDay parameter has a non-default date component");

				if (timeOfDay.Kind != DateTimeKind.Unspecified)
					throw new ArgumentException ("timeOfDay parameter Kind's property is not DateTimeKind.Unspecified");

				if (timeOfDay.Ticks % TimeSpan.TicksPerMillisecond != 0)
					throw new ArgumentException ("timeOfDay parameter does not represent a whole number of milliseconds");

				if (day < 1 || day > 31) {
					if (!(!isFixedDateRule && day == -1))
						throw new ArgumentOutOfRangeException ("day parameter is less than 1 or greater than 31");
				}

				if (week < 1 || week > 5) {
					if (!(isFixedDateRule && week == -1))
						throw new ArgumentOutOfRangeException ("week parameter is less than 1 or greater than 5");
				}

				if (month < 1 || month > 12)
					throw new ArgumentOutOfRangeException ("month parameter is less than 1 or greater than 12");

				if (dayOfWeek != DayOfWeek.Sunday &&
						dayOfWeek != DayOfWeek.Monday &&
						dayOfWeek != DayOfWeek.Tuesday &&
						dayOfWeek != DayOfWeek.Wednesday &&
						dayOfWeek != DayOfWeek.Thursday &&
						dayOfWeek != DayOfWeek.Friday &&
						dayOfWeek != DayOfWeek.Saturday) {
					if (!(isFixedDateRule && dayOfWeek == (DayOfWeek) (-1)))
						throw new ArgumentOutOfRangeException ("dayOfWeek parameter is not a member od DayOfWeek enumeration");
				}
			}
		}
	}
}

#endif
