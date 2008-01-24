
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
			public void DateNotInMilliSeconds ()
			{
				TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (50), 3, 15);
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
				TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (50), 3, 4, DayOfWeek.Sunday);
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

		[TestFixture]
		public class NonExceptional {

			[Test]
			public void EqualsObject ()
			{
				DateTime dt = new DateTime (1, 1, 1, 2, 0, 0, DateTimeKind.Unspecified);
				TimeZoneInfo.TransitionTime tt1 = TimeZoneInfo.TransitionTime.CreateFixedDateRule (dt, 1, 21);
				Assert.IsFalse (tt1.Equals (null), "null"); // found using Gendarme :)
				Assert.IsTrue (tt1.Equals (tt1), "self");
				TimeZoneInfo.TransitionTime tt2 = TimeZoneInfo.TransitionTime.CreateFixedDateRule (dt, 2, 12);
				Assert.IsFalse (tt2.Equals (tt1), "1!=2");
				Assert.IsFalse (tt1.Equals (tt2), "2!=1");
			}
		}
	}
}
#endif
