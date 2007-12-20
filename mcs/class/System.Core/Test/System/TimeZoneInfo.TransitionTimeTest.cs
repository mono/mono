
using System;
using NUnit.Framework;

#if NET_2_0
namespace MonoTests.System
{
	public class TimeZoneInfo_TransitionTimeTest
	{	
		[TestFixture]
		public class CreateFixedDateRuleExceptions
		{
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void DateHasNonDefaultComponent ()
			{
				TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1, 1, 10, 2, 0, 0), 3, 15);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void KindNotUnspecified()
			{
				TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1, 1, 1, 2, 0, 0, DateTimeKind.Utc), 3, 15);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void DateNotInSeconds ()
			{
				TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1, 1, 1, 2, 0, 0, 77), 3, 15);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentOutOfRangeException))]
			public void MonthOutOfRange ()
			{
				TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1, 1, 1, 2, 0, 0), 13, 15);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentOutOfRangeException))]
			public void DayOutOfRange ()
			{
				TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1, 1, 1, 2, 0, 0), 3, -2);
			}
		}
		
		[TestFixture]
		public class CreateFloatingDateRuleExceptions
		{
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void DateHasNonDefaultComponent ()
			{
				TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 10, 2, 0, 0), 3, 4, DayOfWeek.Sunday);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void KindNotUnspecified()
			{
				TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 0, 0, DateTimeKind.Utc), 3, 4, DayOfWeek.Sunday);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void DateNotInSeconds ()
			{
				TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 0, 0, 77), 3, 4, DayOfWeek.Sunday);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentOutOfRangeException))]
			public void MonthOutOfRange ()
			{
				TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 0, 0), 13, 4, DayOfWeek.Sunday);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentOutOfRangeException))]
			public void WeekOutOfRange ()
			{
				TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 0, 0), 3, -2, DayOfWeek.Sunday);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentOutOfRangeException))]
			public void DayOfWeekOutOfRange ()
			{
				TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 0, 0), 3, 4, (DayOfWeek)12);
			}
		}
	}
}
#endif
