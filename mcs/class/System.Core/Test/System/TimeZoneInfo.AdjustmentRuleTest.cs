using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

#if NET_2_0
namespace MonoTests.System
{
	public class TimeZoneInfo_AdjustmentRuleTest
	{	
		[TestFixture]
		public class CreateAdjustmentRuleException
		{
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void DateTimeKindNotUnspecified ()
			{
				DateTime dateStart = new DateTime (2007,01,01, 0,0,0,DateTimeKind.Utc);
				DateTime dateEnd = new DateTime (2008,01,01);
				TimeZoneInfo.TransitionTime daylightTransitionStart = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 03, 11);
				TimeZoneInfo.TransitionTime daylightTransitionEnd = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 10, 11);
				TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (dateStart, dateEnd, new TimeSpan (1,0,0), daylightTransitionStart, daylightTransitionEnd);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void TransitionEndEqualStart ()
			{
				DateTime dateStart = new DateTime (2007,01,01);
				DateTime dateEnd = new DateTime (2008,01,01);
				TimeZoneInfo.TransitionTime daylightTransition = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 03, 11);
				TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (dateStart, dateEnd, new TimeSpan (1,0,0), daylightTransition, daylightTransition);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void DateIncludesTimeOfDay ()
			{
				DateTime dateStart = new DateTime (2007,01,01, 0,1,0);
				DateTime dateEnd = new DateTime (2008,01,01);
				TimeZoneInfo.TransitionTime daylightTransitionStart = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 03, 11);
				TimeZoneInfo.TransitionTime daylightTransitionEnd = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 10, 11);
				TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (dateStart, dateEnd, new TimeSpan (1,0,0), daylightTransitionStart, daylightTransitionEnd);
			}
		
			[Test]
			public void DatesMaxMinValid ()
			{
				try {
					TimeZoneInfo.TransitionTime daylightTransitionStart = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 03, 11);
					TimeZoneInfo.TransitionTime daylightTransitionEnd = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 10, 11);
					TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (DateTime.MinValue.Date, DateTime.MaxValue.Date, new TimeSpan (1,0,0), daylightTransitionStart, daylightTransitionEnd);
				} catch {
					Assert.Fail ("DateTime.MinValue and MaxValue are not valid...");
				}
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentOutOfRangeException))]
			public void EndBeforeStart ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					throw new ArgumentOutOfRangeException ();;
				DateTime dateStart = new DateTime (2007,01,01);
				DateTime dateEnd = new DateTime (2006,01,01);
				TimeZoneInfo.TransitionTime daylightTransitionStart = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 03, 11);
				TimeZoneInfo.TransitionTime daylightTransitionEnd = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 10, 11);
				TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (dateStart, dateEnd, new TimeSpan (1,0,0), daylightTransitionStart, daylightTransitionEnd);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentOutOfRangeException))]
			public void DeltaOutOfRange ()
			{
				DateTime dateStart = new DateTime (2007,01,01);
				DateTime dateEnd = new DateTime (2008,01,01);
				TimeZoneInfo.TransitionTime daylightTransitionStart = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 03, 11);
				TimeZoneInfo.TransitionTime daylightTransitionEnd = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 10, 11);
				TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (dateStart, dateEnd, new TimeSpan (14,1,0), daylightTransitionStart, daylightTransitionEnd);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentOutOfRangeException))]
			public void DeltaNotInSeconds ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					throw new ArgumentOutOfRangeException ();;
				DateTime dateStart = new DateTime (2007,01,01);
				DateTime dateEnd = new DateTime (2008,01,01);
				TimeZoneInfo.TransitionTime daylightTransitionStart = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 03, 11);
				TimeZoneInfo.TransitionTime daylightTransitionEnd = TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1,1,1,2,0,0), 10, 11);
				TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (dateStart, dateEnd, new TimeSpan (55), daylightTransitionStart, daylightTransitionEnd);
			}
		}
	
		[TestFixture]
		public class NonExceptional
		{
			[Test]
			public void Serialization_Deserialization ()
			{
				TimeZoneInfo.TransitionTime start = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,1,0,0), 3, 5, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime end = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,2,0,0), 10, 5, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (DateTime.MinValue.Date, DateTime.MaxValue.Date, new TimeSpan (1,0,0), start, end);
				MemoryStream stream = new MemoryStream ();
				BinaryFormatter formatter = new BinaryFormatter ();
				formatter.Serialize (stream, rule);
				stream.Position = 0;
				TimeZoneInfo.AdjustmentRule deserialized = (TimeZoneInfo.AdjustmentRule) formatter.Deserialize (stream);
				stream.Close ();
				stream.Dispose ();

				Assert.IsTrue (rule.Equals (deserialized));
			}
		}
	}	
}
#endif
