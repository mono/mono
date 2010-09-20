/*
 * System.TimeZoneInfo.AdjustmentRule
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

#if (INSIDE_CORLIB && (NET_4_0 || MOONLIGHT)) || (MOBILE && !INSIDE_CORLIB) || (NET_3_5 && !NET_4_0 && !BOOTSTRAP_NET_4_0)

using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System
{
	public sealed partial class TimeZoneInfo {
		[SerializableAttribute]
#if NET_4_0 || BOOTSTRAP_NET_4_0
		[TypeForwardedFrom (Consts.AssemblySystemCore_3_5)]
#elif MOONLIGHT
		[TypeForwardedFrom (Consts.AssemblySystem_Core)]
#endif
		public sealed class AdjustmentRule : IEquatable<TimeZoneInfo.AdjustmentRule>, ISerializable, IDeserializationCallback
		{
			DateTime dateEnd;
			public DateTime DateEnd {
				get { return dateEnd; }	
			}

			DateTime dateStart;
			public DateTime DateStart {
				get { return dateStart; }
			}

			TimeSpan daylightDelta;
			public TimeSpan DaylightDelta {
				get { return daylightDelta; }
			}

			TransitionTime daylightTransitionEnd;
			public TransitionTime DaylightTransitionEnd {
				get { return daylightTransitionEnd; }
			}

			TransitionTime daylightTransitionStart;
			public TransitionTime DaylightTransitionStart {
				get { return daylightTransitionStart; }
			}

			public static AdjustmentRule CreateAdjustmentRule (
				DateTime dateStart,
				DateTime dateEnd,
				TimeSpan daylightDelta,
				TransitionTime daylightTransitionStart,
				TransitionTime daylightTransitionEnd)
			{
				return new AdjustmentRule (dateStart, dateEnd, daylightDelta, daylightTransitionStart, daylightTransitionEnd);
			}

			private AdjustmentRule (
				DateTime dateStart,
				DateTime dateEnd,
				TimeSpan daylightDelta,
				TransitionTime daylightTransitionStart,
				TransitionTime daylightTransitionEnd)
			{
				if (dateStart.Kind != DateTimeKind.Unspecified || dateEnd.Kind != DateTimeKind.Unspecified)
					throw new ArgumentException ("the Kind property of dateStart or dateEnd parameter does not equal DateTimeKind.Unspecified");

				if (daylightTransitionStart == daylightTransitionEnd)
					throw new ArgumentException ("daylightTransitionStart parameter cannot equal daylightTransitionEnd parameter");

				if (dateStart.Ticks % TimeSpan.TicksPerDay != 0 || dateEnd.Ticks % TimeSpan.TicksPerDay != 0)
					throw new ArgumentException ("dateStart or dateEnd parameter includes a time of day value");

				if (dateEnd < dateStart)
					throw new ArgumentOutOfRangeException ("dateEnd is earlier than dateStart");

				if (daylightDelta > new TimeSpan (14, 0, 0) || daylightDelta < new TimeSpan (-14, 0, 0))
					throw new ArgumentOutOfRangeException ("daylightDelta is less than -14 or greater than 14 hours");

				if (daylightDelta.Ticks % TimeSpan.TicksPerSecond != 0)
					throw new ArgumentOutOfRangeException ("daylightDelta parameter does not represent a whole number of seconds");

				this.dateStart = dateStart;
				this.dateEnd = dateEnd;
				this.daylightDelta = daylightDelta;
				this.daylightTransitionStart = daylightTransitionStart;
				this.daylightTransitionEnd = daylightTransitionEnd;
			}

			public bool Equals (TimeZoneInfo.AdjustmentRule other)
			{
				return dateStart == other.dateStart &&
					dateEnd == other.dateEnd &&
					daylightDelta == other.daylightDelta && 
					daylightTransitionStart == other.daylightTransitionStart &&
					daylightTransitionEnd == other.daylightTransitionEnd;
			}

			public override int GetHashCode ()
			{
				return dateStart.GetHashCode () ^ 
					dateEnd.GetHashCode () ^
					daylightDelta.GetHashCode () ^
					daylightTransitionStart.GetHashCode () ^
					daylightTransitionEnd.GetHashCode ();
			}
					
#if NET_4_0
			void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
#else
			public void GetObjectData (SerializationInfo info, StreamingContext context)
#endif
			{
				throw new NotImplementedException ();
			}
#if NET_4_0
			void IDeserializationCallback.OnDeserialization (object sender)
#else
			public void OnDeserialization (object sender)
#endif
			{
				throw new NotImplementedException ();
			}
		}
	}
}

#endif
