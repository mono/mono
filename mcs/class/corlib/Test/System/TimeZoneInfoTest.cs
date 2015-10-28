/*
 * TimeZoneInfo.Tests
 *
 * Author(s)
 * 	Stephane Delcroix <stephane@delcroix.org>
 *
 * Copyright 2011 Xamarin Inc.
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

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Reflection;
using System.Globalization;

using NUnit.Framework;
namespace MonoTests.System
{
	public class TimeZoneInfoTest
	{
		[TestFixture]
		public class PropertiesTests
		{
			[Test]
			public void GetLocal ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				TimeZoneInfo local = TimeZoneInfo.Local;
				Assert.IsNotNull (local);
				Assert.IsTrue (true);
			}

			[DllImport ("libc")]
			private static extern int readlink (string path, byte[] buffer, int buflen);

			[Test] // Covers #24958
			public void LocalId ()
			{
				byte[] buf = new byte [512];

				var path = "/etc/localtime";
				try {
					var ret = readlink (path, buf, buf.Length);
					if (ret == -1)
						return; // path is not a symbolic link, nothing to test
				} catch (DllNotFoundException e) {
					return;
				}

				Assert.IsTrue (TimeZoneInfo.Local.Id != "Local", "Local timezone id should not be \"Local\"");
			}
		}

		[TestFixture]
		public class CreateCustomTimezoneTests
		{
			[Test]
			[ExpectedException (typeof (ArgumentNullException))]
			public void IdIsNullException ()
			{
				TimeZoneInfo.CreateCustomTimeZone (null, new TimeSpan (0), null, null);	
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void IdIsEmptyString ()
			{
				TimeZoneInfo.CreateCustomTimeZone ("", new TimeSpan (0), null, null);	
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void OffsetIsNotMinutes ()
			{
				TimeZoneInfo.CreateCustomTimeZone ("mytimezone", new TimeSpan (0, 0, 55), null, null);	
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentOutOfRangeException))]
			public void OffsetTooBig ()
			{
				TimeZoneInfo.CreateCustomTimeZone ("mytimezone", new TimeSpan (14, 1, 0), null, null);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentOutOfRangeException))]
			public void OffsetTooSmall ()
			{
				TimeZoneInfo.CreateCustomTimeZone ("mytimezone", - new TimeSpan (14, 1, 0), null, null);
			}
		
		#if STRICT
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void IdLongerThan32 ()
			{
				TimeZoneInfo.CreateCustomTimeZone ("12345678901234567890123456789012345", new TimeSpan (0), null, null);	
			}	
		#endif
		
			[Test]
			[ExpectedException (typeof (InvalidTimeZoneException))]
			public void AdjustmentRulesOverlap ()
			{
				TimeZoneInfo.TransitionTime s1 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 3, 2, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime e1 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 10, 2, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule r1 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (2000,1,1), new DateTime (2005,1,1), new TimeSpan (1,0,0), s1, e1);
				TimeZoneInfo.TransitionTime s2 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 2, 2, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime e2 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 11, 2, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule r2 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (2004,1,1), new DateTime (2007,1,1), new TimeSpan (1,0,0), s2, e2);
				TimeZoneInfo.CreateCustomTimeZone ("mytimezone", new TimeSpan (6,0,0),null,null,null,new TimeZoneInfo.AdjustmentRule[] {r1, r2});
			}
		
			[Test]
			[ExpectedException (typeof (InvalidTimeZoneException))]
			public void RulesNotOrdered ()
			{
				TimeZoneInfo.TransitionTime s1 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 3, 2, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime e1 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 10, 2, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule r1 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (2000,1,1), new DateTime (2005,1,1), new TimeSpan (1,0,0), s1, e1);
				TimeZoneInfo.TransitionTime s2 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 2, 2, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime e2 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 11, 2, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule r2 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (2006,1,1), new DateTime (2007,1,1), new TimeSpan (1,0,0), s2, e2);
				TimeZoneInfo.CreateCustomTimeZone ("mytimezone", new TimeSpan (6,0,0),null,null,null,new TimeZoneInfo.AdjustmentRule[] {r2, r1});
			}
		
			[Test]
			[ExpectedException (typeof (InvalidTimeZoneException))]
			public void OffsetOutOfRange ()
			{
				TimeZoneInfo.TransitionTime startTransition = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 3, 2, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime endTransition = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 10, 2, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (2000,1,1), new DateTime (2005,1,1), new TimeSpan (3,0,0), startTransition, endTransition);
				TimeZoneInfo.CreateCustomTimeZone ("mytimezone", new TimeSpan (12,0,0),null,null,null,new TimeZoneInfo.AdjustmentRule[] {rule});
			}
		
			[Test]
			[ExpectedException (typeof (InvalidTimeZoneException))]
			public void NullRule ()
			{
				TimeZoneInfo.CreateCustomTimeZone ("mytimezone", new TimeSpan (12,0,0),null,null,null,new TimeZoneInfo.AdjustmentRule[] {null});
			}
		
			[Test]
			[ExpectedException (typeof (InvalidTimeZoneException))]
			public void MultiplesRulesForDate ()
			{
				TimeZoneInfo.TransitionTime s1 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 3, 2, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime e1 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 10, 2, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule r1 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (2000,1,1), new DateTime (2005,1,1), new TimeSpan (1,0,0), s1, e1);
				TimeZoneInfo.TransitionTime s2 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 2, 2, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime e2 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 11, 2, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule r2 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (2005,1,1), new DateTime (2007,1,1), new TimeSpan (1,0,0), s2, e2);
				TimeZoneInfo.CreateCustomTimeZone ("mytimezone", new TimeSpan (6,0,0),null,null,null,new TimeZoneInfo.AdjustmentRule[] {r1, r2});
			}

			[Test]
			public void SupportsDaylightSavingTime_NonEmptyAdjustmentRule ()
			{
				TimeZoneInfo.TransitionTime s1 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 3, 2, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime e1 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 10, 2, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule r1 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (2000,1,1), new DateTime (2005,1,1), new TimeSpan (1,0,0), s1, e1);
				TimeZoneInfo tz = TimeZoneInfo.CreateCustomTimeZone ("mytimezone", new TimeSpan (6,0,0),null,null,null,new TimeZoneInfo.AdjustmentRule[] {r1});
				Assert.IsTrue (tz.SupportsDaylightSavingTime);
			}

			[Test]
			public void SupportsDaylightSavingTime_EmptyAdjustmentRule ()
			{
				TimeZoneInfo tz = TimeZoneInfo.CreateCustomTimeZone ("mytimezone", new TimeSpan (6,0,0),null,null,null,null);
				Assert.IsFalse (tz.SupportsDaylightSavingTime);
			}

			[Test]
			public void SupportsDaylightSavingTime_NonEmptyAdjustmentRule_DisableDaylightSavingTime ()
			{
				TimeZoneInfo.TransitionTime s1 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 3, 2, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime e1 = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,4,0,0), 10, 2, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule r1 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (2000,1,1), new DateTime (2005,1,1), new TimeSpan (1,0,0), s1, e1);
				TimeZoneInfo tz = TimeZoneInfo.CreateCustomTimeZone ("mytimezone", new TimeSpan (6,0,0),null,null,null,new TimeZoneInfo.AdjustmentRule[] {r1}, true);
				Assert.IsFalse (tz.SupportsDaylightSavingTime);
			}

			[Test]
			public void SupportsDaylightSavingTime_EmptyAdjustmentRule_DisableDaylightSavingTime ()
			{
				TimeZoneInfo tz = TimeZoneInfo.CreateCustomTimeZone ("mytimezone", new TimeSpan (6,0,0),null,null,null,null,true);
				Assert.IsFalse (tz.SupportsDaylightSavingTime);
			}
		}
		
		[TestFixture]
		public class IsDaylightSavingTimeTests
		{
			TimeZoneInfo london;
		
			[SetUp]
			public void CreateTimeZones ()
			{
				TimeZoneInfo.TransitionTime start = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,1,0,0), 3, 5, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime end = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,2,0,0), 10, 5, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (DateTime.MinValue.Date, DateTime.MaxValue.Date, new TimeSpan (1,0,0), start, end);
				london = TimeZoneInfo.CreateCustomTimeZone ("Europe/London", new TimeSpan (0), "Europe/London", "British Standard Time", "British Summer Time", new TimeZoneInfo.AdjustmentRule [] {rule});
			}
		
			[Test]
			public void NoDSTInUTC ()
			{
				DateTime june01 = new DateTime (2007, 06, 01);
				Assert.IsFalse (TimeZoneInfo.Utc.IsDaylightSavingTime (june01));
			}
		
			[Test]
			public void DSTInLondon ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				DateTime june01 = new DateTime (2007, 06, 01);
				DateTime xmas = new DateTime (2007, 12, 25);
				Assert.IsTrue (london.IsDaylightSavingTime (june01), "June 01 is DST in London");
				Assert.IsFalse (london.IsDaylightSavingTime (xmas), "Xmas is not DST in London");
			}
		
			[Test]
			public void DSTTransisions ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				DateTime beforeDST = new DateTime (2007, 03, 25, 0, 59, 59, DateTimeKind.Unspecified);
				DateTime startDST = new DateTime (2007, 03, 25, 2, 0, 0, DateTimeKind.Unspecified);
				DateTime endDST = new DateTime (2007, 10, 28, 1, 59, 59, DateTimeKind.Unspecified);
				DateTime afterDST = new DateTime (2007, 10, 28, 2, 0, 0, DateTimeKind.Unspecified);
				Assert.IsFalse (london.IsDaylightSavingTime (beforeDST), "Just before DST");
				Assert.IsTrue (london.IsDaylightSavingTime (startDST), "the first seconds of DST");
				Assert.IsFalse (london.IsDaylightSavingTime (endDST), "The last seconds of DST");
				Assert.IsFalse (london.IsDaylightSavingTime (afterDST), "Just after DST");
			}
		
			[Test]
			public void DSTTransisionsUTC ()
			{
				DateTime beforeDST = new DateTime (2007, 03, 25, 0, 59, 59, DateTimeKind.Utc);
				DateTime startDST = new DateTime (2007, 03, 25, 1, 0, 0, DateTimeKind.Utc);
				DateTime endDST = new DateTime (2007, 10, 28, 0, 59, 59, DateTimeKind.Utc);
				DateTime afterDST = new DateTime (2007, 10, 28, 1, 0, 0, DateTimeKind.Utc);
				Assert.IsFalse (london.IsDaylightSavingTime (beforeDST), "Just before DST");
				Assert.IsTrue (london.IsDaylightSavingTime (startDST), "the first seconds of DST");
				Assert.IsTrue (london.IsDaylightSavingTime (endDST), "The last seconds of DST");
				Assert.IsFalse (london.IsDaylightSavingTime (afterDST), "Just after DST");
			}
		
		#if SLOW_TESTS
			[Test]
			public void MatchTimeZoneBehavior ()
			{
				TimeZone tzone = TimeZone.CurrentTimeZone;
				TimeZoneInfo local = TimeZoneInfo.Local;
				for (DateTime date = new DateTime (2007, 01, 01, 0, 0, 0, DateTimeKind.Local); date < new DateTime (2007, 12, 31, 23, 59, 59); date += new TimeSpan (0,1,0)) {
					date = DateTime.SpecifyKind (date, DateTimeKind.Local);
					if (local.IsInvalidTime (date))
						continue;
					Assert.IsTrue (tzone.IsDaylightSavingTime (date) == local.IsDaylightSavingTime (date));
				}
			}
		#endif
			[Test (Description="Description xambug #17155")]
			public void AdjustmentRuleAfterNewYears ()
			{
				TimeZoneInfo tz;
				if (Environment.OSVersion.Platform == PlatformID.Unix)
					tz = TimeZoneInfo.FindSystemTimeZoneById ("Pacific/Auckland"); // *nix
				else
					tz = TimeZoneInfo.FindSystemTimeZoneById ("New Zealand Standard Time"); // Windows

				// DST start: 9/29/2013 2:00:00 AM
				// DST end: 4/6/2014 3:00:00 AM
				DateTime dt = new DateTime (2014, 1, 9, 23, 0, 0, DateTimeKind.Utc);
				Assert.IsTrue (tz.IsDaylightSavingTime (dt), "#1.1");

				// DST start: 9/29/2014 2:00:00 AM
				// DST end: 4/6/2015 3:00:00 AM
				dt = new DateTime (2014, 6, 9, 23, 0, 0, DateTimeKind.Utc);
				Assert.IsFalse (tz.IsDaylightSavingTime (dt), "#2.1");

				// DST start: 9/29/2014 2:00:00 AM
				// DST end: 4/6/2015 3:00:00 AM
				dt = new DateTime (2014, 10, 9, 23, 0, 0, DateTimeKind.Utc);
				Assert.IsTrue (tz.IsDaylightSavingTime (dt), "#3.1");
			}

			[Test] //Covers #26008
			public void DSTWithFloatingDateRule ()
			{
				// Construct a custom time zone where daylight saving time starts on the
				// 2nd Sunday in March.
				var transitionToDaylight = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 0, 0), 3, 2, DayOfWeek.Sunday);
				var transitionToStandard = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 0, 0), 11, 1, DayOfWeek.Sunday);
				var adjustment = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (DateTime.MinValue.Date, DateTime.MaxValue.Date, new TimeSpan (1, 0, 0), transitionToDaylight, transitionToStandard);
				var timeZone = TimeZoneInfo.CreateCustomTimeZone ("BugCheck", new TimeSpan (-8, 0, 0), "Testing", "Testing Standard", "Testing Daylight", new TimeZoneInfo.AdjustmentRule [] { adjustment });
				// See if March 7, 2014 is listed as being during daylight saving time.
				// If it is DST, then the runtime has the bug that we are looking for.
				Assert.IsFalse (timeZone.IsDaylightSavingTime (new DateTime (2014, 3, 7, 12, 0, 0, DateTimeKind.Unspecified)));
			}

			[Test] //Covers #25050
			public void TestAthensDST ()
			{
				TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Athens");
				var date = new DateTime (2014, 3, 30 , 2, 0, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (2,0,0), tzi.GetUtcOffset (date));
			}
		}
		
		[TestFixture]
		public class ConvertTimeTests
		{
			TimeZoneInfo london;
		
			[SetUp]
			public void CreateTimeZones ()
			{
				TimeZoneInfo.TransitionTime start = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,1,0,0), 3, 5, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime end = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,2,0,0), 10, 5, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (DateTime.MinValue.Date, DateTime.MaxValue.Date, new TimeSpan (1,0,0), start, end);
				london = TimeZoneInfo.CreateCustomTimeZone ("Europe/London", new TimeSpan (0), "Europe/London", "British Standard Time", "British Summer Time", new TimeZoneInfo.AdjustmentRule [] {rule});
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void ConvertFromUtc_KindIsLocalException ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					throw new ArgumentException ();
				TimeZoneInfo.ConvertTimeFromUtc (new DateTime (2007, 5, 3, 11, 8, 0, DateTimeKind.Local), TimeZoneInfo.Local);	
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentNullException))]
			public void ConvertFromUtc_DestinationTimeZoneIsNullException ()
			{
				TimeZoneInfo.ConvertTimeFromUtc (new DateTime (2007, 5, 3, 11, 8, 0), null);		
			}
		
			[Test]
			public void ConvertFromUtc_DestinationIsUTC ()
			{
				DateTime now = DateTime.UtcNow;
				DateTime converted = TimeZoneInfo.ConvertTimeFromUtc (now, TimeZoneInfo.Utc);
				Assert.AreEqual (now, converted);
			}
			
			[Test]
			public void ConvertFromUTC_ConvertInWinter ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				DateTime utc = new DateTime (2007, 12, 25, 12, 0, 0);
				DateTime converted = TimeZoneInfo.ConvertTimeFromUtc (utc, london);
				Assert.AreEqual (utc, converted);
			}
		
			[Test]
			public void ConvertFromUtc_ConvertInSummer ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				DateTime utc = new DateTime (2007, 06, 01, 12, 0, 0);
				DateTime converted = TimeZoneInfo.ConvertTimeFromUtc (utc, london);
				Assert.AreEqual (utc + new TimeSpan (1,0,0), converted);
			}
		
			[Test]
			public void ConvertToUTC_KindIsUtc ()
			{
				DateTime now = DateTime.UtcNow;
				Assert.AreEqual (now.Kind, DateTimeKind.Utc);
				DateTime converted = TimeZoneInfo.ConvertTimeToUtc (now);
				Assert.AreEqual (now, converted);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void ConvertToUTC_KindIsUTCButSourceIsNot ()
			{
				TimeZoneInfo.ConvertTimeToUtc (new DateTime (2007, 5, 3, 12, 8, 0, DateTimeKind.Utc), london);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void ConvertToUTC_KindIsLocalButSourceIsNot ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					throw new ArgumentException ();
				TimeZoneInfo.ConvertTimeToUtc (new DateTime (2007, 5, 3, 12, 8, 0, DateTimeKind.Local), london);	
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentException))]
			public void ConvertToUTC_InvalidDate ()
			{
				TimeZoneInfo.ConvertTimeToUtc (new DateTime (2007, 3, 25, 1, 30, 0), london);
			}
		
			[Test]
			[ExpectedException (typeof (ArgumentNullException))]
			public void ConvertToUTC_SourceIsNull ()
			{
				TimeZoneInfo.ConvertTimeToUtc (new DateTime (2007, 5, 3, 12, 16, 0), null);
			}
		
		#if SLOW_TESTS
			[Test]
			public void ConvertToUtc_MatchDateTimeBehavior ()
			{
				for (DateTime date = new DateTime (2007, 01, 01, 0, 0, 0); date < new DateTime (2007, 12, 31, 23, 59, 59); date += new TimeSpan (0,1,0)) {
					Assert.AreEqual (TimeZoneInfo.ConvertTimeToUtc (date), date.ToUniversalTime ());
				}
			}
		#endif
		
			[Test]
			public void ConvertFromToUtc ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				DateTime utc = DateTime.UtcNow;
				Assert.AreEqual (utc.Kind, DateTimeKind.Utc);
				DateTime converted = TimeZoneInfo.ConvertTimeFromUtc (utc, london);
				Assert.AreEqual (converted.Kind, DateTimeKind.Unspecified);
				DateTime back = TimeZoneInfo.ConvertTimeToUtc (converted, london);
				Assert.AreEqual (back.Kind, DateTimeKind.Utc);
				Assert.AreEqual (utc, back);
		
			}

			[Test]
			public void ConvertTimeToUtc_Overflow ()
			{
				var res = TimeZoneInfo.ConvertTimeToUtc (new DateTime (0));
				Assert.AreEqual (res.Kind, DateTimeKind.Utc, "#1");

				res = TimeZoneInfo.ConvertTimeToUtc (DateTime.MaxValue);
				Assert.AreEqual (res.Kind, DateTimeKind.Utc, "#2");
			}

			[Test]
			public void ConvertFromToLocal ()
			{
				DateTime utc = DateTime.UtcNow;
				Assert.AreEqual(utc.Kind, DateTimeKind.Utc);
				DateTime converted = TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
				Assert.AreEqual(DateTimeKind.Local, converted.Kind);
				DateTime back = TimeZoneInfo.ConvertTimeToUtc(converted, TimeZoneInfo.Local);
				Assert.AreEqual(back.Kind, DateTimeKind.Utc);
				Assert.AreEqual(utc, back);
			}

			[Test]
			public void ConvertToTimeZone ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");

				TimeZoneInfo.ConvertTime (DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland"));
			}

			[Test]
			[ExpectedException (typeof (ArgumentNullException))]
			public void ConvertTime_DateTime_TimeZoneInfo_DestinationTimeZoneIsNull ()
			{
				TimeZoneInfo.ConvertTime (DateTime.Now, null);
			}

			[Test]
			public void ConvertTime_DateTime_TimeZoneInfo_DateTimeKindMatch ()
			{
				var sdt = new DateTime (2014, 1, 9, 23, 0, 0, DateTimeKind.Utc);
				var ddt = TimeZoneInfo.ConvertTime (sdt, TimeZoneInfo.Utc);
				Assert.AreEqual (ddt.Kind, sdt.Kind, "#1.1");
				Assert.AreEqual (ddt.Kind, DateTimeKind.Utc, "#1.2");
				
				sdt = new DateTime (2014, 1, 9, 23, 0, 0, DateTimeKind.Local);
				ddt = TimeZoneInfo.ConvertTime (sdt, TimeZoneInfo.Local);
				Assert.AreEqual (ddt.Kind, sdt.Kind, "#2.1");
				Assert.AreEqual (ddt.Kind, DateTimeKind.Local, "#2.2");

				sdt = new DateTime (2014, 1, 9, 23, 0, 0);
				ddt = TimeZoneInfo.ConvertTime (sdt, TimeZoneInfo.Local);
				Assert.AreEqual (ddt.Kind, sdt.Kind, "#3.1");
				Assert.AreEqual (ddt.Kind, DateTimeKind.Unspecified, "#3.2");
			}

			[Test]
			[ExpectedException (typeof (ArgumentNullException))]
			public void ConverTime_DateTime_TimeZoneInfo_TimeZoneInfo_SourceTimeZoneIsNull ()
			{
				TimeZoneInfo.ConvertTime (DateTime.Now, null, TimeZoneInfo.Local);
			}

			[Test]
			[ExpectedException (typeof (ArgumentNullException))]
			public void ConverTime_DateTime_TimeZoneInfo_TimeZoneInfo_DestinationTimeZoneIsNull ()
			{
				TimeZoneInfo.ConvertTime (DateTime.Now, TimeZoneInfo.Utc, null);
			}

			[Test (Description="Fix for xambug https://bugzilla.xamarin.com/show_bug.cgi?id=17155")]
			public void ConvertTime_AdjustmentRuleAfterNewYears ()
			{
				TimeZoneInfo tz;
				if (Environment.OSVersion.Platform == PlatformID.Unix)
					tz = TimeZoneInfo.FindSystemTimeZoneById ("Pacific/Auckland"); // *nix
				else
					tz = TimeZoneInfo.FindSystemTimeZoneById ("New Zealand Standard Time"); // Windows

				// DST start: 9/29/2013 2:00:00 AM
				// DST end: 4/6/2014 3:00:00 AM
				DateTime sdt = new DateTime (2014, 1, 9, 23, 0, 0, DateTimeKind.Utc);
				DateTime ddt = TimeZoneInfo.ConvertTime (sdt, tz);
				Assert.AreEqual (10, ddt.Day, "#1.1");
				Assert.AreEqual (1, ddt.Month, "#1.2");
				Assert.AreEqual (2014, ddt.Year, "#1.3");
				Assert.AreEqual (12, ddt.Hour, "#1.4");
				Assert.AreEqual (0, ddt.Minute, "#1.5");
				Assert.AreEqual (0, ddt.Second, "#1.6");
				
				// DST start: 9/29/2014 2:00:00 AM
				// DST end: 4/6/2015 3:00:00 AM
				sdt = new DateTime (2014, 6, 9, 23, 0, 0, DateTimeKind.Utc);
				ddt = TimeZoneInfo.ConvertTime (sdt, tz);
				Assert.AreEqual (10, ddt.Day, "#2.1");
				Assert.AreEqual (6, ddt.Month, "#2.2");
				Assert.AreEqual (2014, ddt.Year, "#2.3");
				Assert.AreEqual (11, ddt.Hour, "#2.4");
				Assert.AreEqual (0, ddt.Minute, "#2.5");
				Assert.AreEqual (0, ddt.Second, "#2.6");
				
				// DST start: 9/29/2014 2:00:00 AM
				// DST end: 4/6/2015 3:00:00 AM
				sdt = new DateTime (2014, 10, 9, 23, 0, 0, DateTimeKind.Utc);
				ddt = TimeZoneInfo.ConvertTime (sdt, tz);
				Assert.AreEqual (10, ddt.Day, "#3.1");
				Assert.AreEqual (10, ddt.Month, "#3.2");
				Assert.AreEqual (2014, ddt.Year, "#3.3");
				Assert.AreEqual (12, ddt.Hour, "#3.4");
				Assert.AreEqual (0, ddt.Minute, "#3.5");
				Assert.AreEqual (0, ddt.Second, "#3.6");
			}

			[Test (Description="Fix the bug https://bugzilla.xamarin.com/show_bug.cgi?id=1849")]
			public void ConvertTime_AjustmentConvertTimeWithSourceTimeZone () {
				
				TimeZoneInfo easternTimeZone;
				TimeZoneInfo pacificTimeZone;

				if (Environment.OSVersion.Platform == PlatformID.Unix) {
					// *nix
					easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById ("US/Eastern");
					pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById ("US/Pacific");	
				}
				else {
					// Windows
					easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById ("Eastern Standard Time");
					pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById ("Pacific Standard Time");
				}

				DateTime lastMidnight = new DateTime (new DateTime (2012, 06, 13).Ticks, DateTimeKind.Unspecified);
				DateTime lastMidnightAsEST = TimeZoneInfo.ConvertTime (lastMidnight, pacificTimeZone, easternTimeZone);
				DateTime lastMidnightAsPST = TimeZoneInfo.ConvertTime (lastMidnightAsEST, easternTimeZone, pacificTimeZone);
			
				// Last midnight in PST as EST should be 3AM
				DateTime expectedDate = new DateTime (2012, 06, 13, 3, 0, 0);

				Assert.AreEqual (expectedDate, lastMidnightAsEST);
				Assert.AreEqual (lastMidnight, lastMidnightAsPST);
			}
		}
		
		[TestFixture]
		public class IsInvalidTimeTests
		{
			TimeZoneInfo london;
		
			[SetUp]
			public void CreateTimeZones ()
			{
				TimeZoneInfo.TransitionTime start = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,1,0,0), 3, 5, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime end = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,2,0,0), 10, 5, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (DateTime.MinValue.Date, DateTime.MaxValue.Date, new TimeSpan (1,0,0), start, end);
				london = TimeZoneInfo.CreateCustomTimeZone ("Europe/London", new TimeSpan (0), "Europe/London", "British Standard Time", "British Summer Time", new TimeZoneInfo.AdjustmentRule [] {rule});
			}
		
		#if SLOW_TESTS
			[Test]
			public void UTCDate ()
			{
				for (DateTime date = new DateTime (2007, 01, 01, 0, 0, 0); date < new DateTime (2007, 12, 31, 23, 59, 59); date += new TimeSpan (0,1,0)) {
					date = DateTime.SpecifyKind (date, DateTimeKind.Utc);
					Assert.IsFalse (london.IsInvalidTime (date));
				}
			}
		#endif
			[Test]
			public void InvalidDates ()
			{
				Assert.IsFalse (london.IsInvalidTime (new DateTime (2007, 03, 25, 0, 59, 59)));
				Assert.IsTrue (london.IsInvalidTime (new DateTime (2007, 03, 25, 1, 0, 0)));
				Assert.IsTrue (london.IsInvalidTime (new DateTime (2007, 03, 25, 1, 59, 59)));
				Assert.IsFalse (london.IsInvalidTime (new DateTime (2007, 03, 25, 2, 0, 0)));
			}
		}
		
		[TestFixture]
		public class IsAmbiguousTimeTests
		{
			TimeZoneInfo london;
		
			[SetUp]
			public void CreateTimeZones ()
			{
				TimeZoneInfo.TransitionTime start = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,1,0,0), 3, 5, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime end = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,2,0,0), 10, 5, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (DateTime.MinValue.Date, DateTime.MaxValue.Date, new TimeSpan (1,0,0), start, end);
				london = TimeZoneInfo.CreateCustomTimeZone ("Europe/London", new TimeSpan (0), "Europe/London", "British Standard Time", "British Summer Time", new TimeZoneInfo.AdjustmentRule [] {rule});
			}
		
			[Test]
			public void AmbiguousDates ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				Assert.IsFalse (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 1, 0, 0)));
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 1, 0, 1)));
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 2, 0, 0)));
				Assert.IsFalse (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 2, 0, 1)));
			}
		
			[Test]
			public void AmbiguousUTCDates ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				Assert.IsFalse (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 0, 0, 0, DateTimeKind.Utc)));
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 0, 0, 1, DateTimeKind.Utc)));
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 0, 59, 59, DateTimeKind.Utc)));
				Assert.IsFalse (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 1, 0, 0, DateTimeKind.Utc)));
			}
		
		#if SLOW_TESTS
			[Test]
			public void AmbiguousInUTC ()
			{
				for (DateTime date = new DateTime (2007, 01, 01, 0, 0, 0); date < new DateTime (2007, 12, 31, 23, 59, 59); date += new TimeSpan (0,1,0)) {
					Assert.IsFalse (TimeZoneInfo.Utc.IsAmbiguousTime (date));
				}
			}
		#endif
		}
		
		[TestFixture]
		public class GetSystemTimeZonesTests
		{
			[Test]
			public void Identity ()
			{
				Assert.AreSame (TimeZoneInfo.GetSystemTimeZones (), TimeZoneInfo.GetSystemTimeZones ());
			}

			[Test]
			public void NotEmpty ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				global::System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> systemTZ = TimeZoneInfo.GetSystemTimeZones ();
				Assert.IsNotNull(systemTZ, "SystemTZ is null");
				Assert.IsFalse (systemTZ.Count == 0, "SystemTZ is empty");
			}
		
			[Test]
			public void ContainsBrussels ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				global::System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> systemTZ = TimeZoneInfo.GetSystemTimeZones ();
				foreach (TimeZoneInfo tz in systemTZ) {
					if (tz.Id == "Europe/Brussels")
						return;
				}
				Assert.Fail ("Europe/Brussels not found in SystemTZ");
			}

			[Test]
			public void ReflectionReturnsTheCorrectMethod ()
			{
				var method = (MethodInfo) typeof (TimeZoneInfo).GetMember ("GetSystemTimeZones", MemberTypes.Method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)[0];

				var timeZones = (global::System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo>) method.Invoke (null, null);
				Assert.IsTrue (timeZones.Count > 0, "GetSystemTimeZones should not return an empty collection.");
			}
		}
		
		[TestFixture]
		public class FindSystemTimeZoneByIdTests
		{
			[Test]
			[ExpectedException (typeof (ArgumentNullException))]
			public void NullId ()
			{
				TimeZoneInfo.FindSystemTimeZoneById (null);
			}
		
			[Test]
			[ExpectedException (typeof (TimeZoneNotFoundException))]
			public void NonSystemTimezone ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					throw new TimeZoneNotFoundException ();
				TimeZoneInfo.FindSystemTimeZoneById ("Neverland/The_Lagoon");
			}
		
			[Test]
			public void FindBrusselsTZ ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Brussels");
				Assert.IsNotNull (brussels);
			}
		
			[Test]
			public void OffsetIsCorrectInKinshasa ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				TimeZoneInfo kin = TimeZoneInfo.FindSystemTimeZoneById ("Africa/Kinshasa");
				Assert.AreEqual (new TimeSpan (1,0,0), kin.BaseUtcOffset, "BaseUtcOffset in Kinshasa is not +1h");
			}
		
			[Test]
			public void OffsetIsCorrectInBrussels ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Brussels");
				Assert.AreEqual (new TimeSpan (1,0,0), brussels.BaseUtcOffset, "BaseUtcOffset for Brussels is not +1h");
			}
		
			[Test]
			public void NoDSTInKinshasa ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				TimeZoneInfo kin = TimeZoneInfo.FindSystemTimeZoneById ("Africa/Kinshasa");
				Assert.IsFalse (kin.SupportsDaylightSavingTime);
			}
		
			[Test]
			public void BrusselsSupportsDST ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Brussels");
				Assert.IsTrue (brussels.SupportsDaylightSavingTime);
			}
		
			[Test]
			public void MelbourneSupportsDST ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				TimeZoneInfo melbourne = TimeZoneInfo.FindSystemTimeZoneById ("Australia/Melbourne");
				Assert.IsTrue (melbourne.SupportsDaylightSavingTime);
			}
		
			[Test]
			public void RomeAndVaticanSharesTime ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				TimeZoneInfo rome = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Rome");
				TimeZoneInfo vatican = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Vatican");
				Assert.IsTrue (rome.HasSameRules (vatican));
			}

			[Test]
			public void FindSystemTimeZoneById_Local_Roundtrip ()
			{
				Assert.AreEqual (TimeZoneInfo.Local.Id, TimeZoneInfo.FindSystemTimeZoneById (TimeZoneInfo.Local.Id).Id);
			}

			[Test]
			public void Test326 ()
			{
				DateTime utc = DateTime.UtcNow;
			        DateTime local = TimeZoneInfo.ConvertTime (utc, TimeZoneInfo.Utc, TimeZoneInfo.FindSystemTimeZoneById (TimeZoneInfo.Local.Id));
				Assert.AreEqual (local, utc + TimeZoneInfo.Local.GetUtcOffset (utc), "ConvertTime/Local");
			}
		
		#if SLOW_TESTS
			[Test]
			public void BrusselsAdjustments ()
			{
				TimeZoneInfo.TransitionTime start = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,2,0,0), 3, 5, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime end = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,3,0,0), 10, 5, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (DateTime.MinValue.Date, DateTime.MaxValue.Date, new TimeSpan (1,0,0), start, end);
				TimeZoneInfo brussels = TimeZoneInfo.CreateCustomTimeZone ("Europe/Brussels", new TimeSpan (1, 0, 0), "Europe/Brussels", "", "", new TimeZoneInfo.AdjustmentRule [] {rule});
		
				TimeZoneInfo brussels_sys = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Brussels");
		
				for (DateTime date = new DateTime (2006, 01, 01, 0, 0, 0, DateTimeKind.Local); date < new DateTime (2007, 12, 31, 23, 59, 59); date += new TimeSpan (0,30,0)) {
					Assert.AreEqual (brussels.GetUtcOffset (date), brussels_sys.GetUtcOffset (date));
					Assert.AreEqual (brussels.IsDaylightSavingTime (date), brussels_sys.IsDaylightSavingTime (date));
				}		
			}
		#endif

			[Test]
			public void SubminuteDSTOffsets ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ();

				var subMinuteDSTs = new string [] {
					"Europe/Dublin", // Europe/Dublin has a DST offset of 34 minutes and 39 seconds in 1916.
					"Europe/Amsterdam",
					"America/St_Johns",
					"Canada/Newfoundland",
					"Europe/Moscow",
					"Europe/Riga",
					"N/A", // testing that the test doesn't fail with inexistent TZs
				};
				foreach (var tz in subMinuteDSTs) {
					try {
						TimeZoneInfo.FindSystemTimeZoneById (tz);
					} catch (TimeZoneNotFoundException) {
						// ok;
					} catch (Exception ex) {
						Assert.Fail (string.Format ("Failed to load TZ {0}: {1}", tz, ex.ToString ()));
					}
				}
			}
		}
		
		[TestFixture]
		public class GetAmbiguousTimeOffsetsTests
		{
			[Test]
			[ExpectedException (typeof(ArgumentException))]
			public void DateIsNotAmbiguous ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					throw new ArgumentException ();
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Brussels");
				DateTime date = new DateTime (2007, 05, 11, 11, 40, 00);
				brussels.GetAmbiguousTimeOffsets (date);
			}
		
			[Test]
			public void AmbiguousOffsets ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ("Not running on Unix.");
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Brussels");
				DateTime date = new DateTime (2007, 10, 28, 2, 30, 00);
				Assert.IsTrue (brussels.IsAmbiguousTime (date));
				Assert.AreEqual (2, brussels.GetAmbiguousTimeOffsets (date).Length);
				Assert.AreEqual (new TimeSpan[] {new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0)}, brussels.GetAmbiguousTimeOffsets (date));
			}
		}

		[TestFixture]
		public class HasSameRulesTests
		{
			[Test]
			public void NullAdjustments () //bnc #391011
			{
				TimeZoneInfo utc = TimeZoneInfo.Utc;
				TimeZoneInfo custom = TimeZoneInfo.CreateCustomTimeZone ("Custom", new TimeSpan (0), "Custom", "Custom");
				Assert.IsTrue (utc.HasSameRules (custom));
			}
		}

		[TestFixture]
		public class SerializationTests
		{
			[Test]
			public void Serialization_Deserialization ()
			{
				TimeZoneInfo.TransitionTime start = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,1,0,0), 3, 5, DayOfWeek.Sunday);
				TimeZoneInfo.TransitionTime end = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1,1,1,2,0,0), 10, 5, DayOfWeek.Sunday);
				TimeZoneInfo.AdjustmentRule rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (DateTime.MinValue.Date, DateTime.MaxValue.Date, new TimeSpan (1,0,0), start, end);
				TimeZoneInfo london = TimeZoneInfo.CreateCustomTimeZone ("Europe/London", new TimeSpan (0), "Europe/London", "British Standard Time", "British Summer Time", new TimeZoneInfo.AdjustmentRule [] {rule});
				MemoryStream stream = new MemoryStream ();
				BinaryFormatter formatter = new BinaryFormatter ();
				formatter.Serialize (stream, london);
				stream.Position = 0;
				TimeZoneInfo deserialized = (TimeZoneInfo) formatter.Deserialize (stream);
				stream.Close ();
				stream.Dispose ();
				Assert.IsTrue (london.Equals (deserialized));
			}
		}

		[TestFixture]
		public class MultipleDaylightSavingTimeTests {
			private TimeZoneInfo cairo;
			private DateTime dst1Start;
			private DateTime dst1End;
			private DateTime dst2Start;
			private DateTime dst2End;

			private TimeSpan baseUtcOffset;
			private TimeSpan dstUtcOffset;
			private TimeSpan dstOffset;

			[SetUp]
			public void CreateTimeZones ()
			{
				/*
				From 1/1/2014 12:00:00 AM to 6/30/2014 12:00:00 AM
					Delta: 01:00:00
					Begins at 12:00 AM on 16 May
					Ends at 1:00 AM on 29 June
				From 7/1/2014 12:00:00 AM to 12/31/2014 12:00:00 AM
					Delta: 01:00:00
					Begins at 12:00 AM on 29 July
					Ends at 12:00 AM on 26 September
				*/
				dst1Start = new DateTime (2014, 5, 16);
				dst1End = new DateTime (2014, 6, 29);
				dst2Start = new DateTime (2014, 7, 29);
				dst2End = new DateTime (2014, 9, 26);

				baseUtcOffset = new TimeSpan (2, 0, 0);
				dstUtcOffset = new TimeSpan (3, 0, 0);
				dstOffset = dstUtcOffset - baseUtcOffset;

				var rule1 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (
					new DateTime (2014, 1, 1), new DateTime (2014, 6, 30), dstOffset,
					CreateFixedDateRule (dst1Start), CreateFixedDateRule (dst1End));

				var rule2 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (
					new DateTime (2014, 7, 1), new DateTime (2014, 12, 31), dstOffset,
					CreateFixedDateRule (dst2Start), CreateFixedDateRule (dst2End));

				cairo = TimeZoneInfo.CreateCustomTimeZone ("Africa/Cairo", baseUtcOffset, "Africa/Cairo", "EET", "EEST",
					new [] {rule1, rule2});
			}

			private static TimeZoneInfo.TransitionTime CreateFixedDateRule (DateTime dateTime)
			{
				var time = new DateTime (dateTime.Ticks - dateTime.Date.Ticks);
				return TimeZoneInfo.TransitionTime.CreateFixedDateRule (time, dateTime.Month, dateTime.Day);
			}

			[Test]
			public void GetUtcOffset_FromUTC ()
			{
				var d = dst1Start.Add (-baseUtcOffset);
				d = DateTime.SpecifyKind (d, DateTimeKind.Utc);
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));

				d = dst1End.Add (-baseUtcOffset-dstOffset);
				d = DateTime.SpecifyKind (d, DateTimeKind.Utc);
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));

				d = dst2Start.Add (-baseUtcOffset);
				d = DateTime.SpecifyKind (d, DateTimeKind.Utc);
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));

				d = dst2End.Add (-baseUtcOffset-dstOffset);
				d = DateTime.SpecifyKind (d, DateTimeKind.Utc);
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));
			}

			[Test]
			public void GetUtcOffset_FromLocal ()
			{
				var d = dst1Start.Add (-baseUtcOffset);
				d = DateTime.SpecifyKind (d, DateTimeKind.Utc);
				d = d.ToLocalTime ();
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));

				d = dst1End.Add (-baseUtcOffset-dstOffset);
				d = DateTime.SpecifyKind (d, DateTimeKind.Utc);
				d = d.ToLocalTime ();
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));

				d = dst2Start.Add (-baseUtcOffset);
				d = DateTime.SpecifyKind (d, DateTimeKind.Utc);
				d = d.ToLocalTime ();
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));

				d = dst2End.Add (-baseUtcOffset-dstOffset);
				d = DateTime.SpecifyKind (d, DateTimeKind.Utc);
				d = d.ToLocalTime ();
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));
			}

			[Test]
			public void GetUtcOffset_FromUnspecified ()
			{
				var d = dst1Start.Add (dstOffset);
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));

				d = dst1End.Add (-dstOffset);
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));

				d = dst2Start.Add (dstOffset);
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));

				d = dst2End.Add (-dstOffset);
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));
			}
		}

		[TestFixture]
		public class GetDaylightChanges
		{
			MethodInfo getChanges;

			[SetUp]
			public void Setup ()
			{
				var flags = BindingFlags.Instance | BindingFlags.NonPublic;
				getChanges = typeof (TimeZoneInfo).GetMethod ("GetDaylightChanges", flags);
			}

			[Test]
			public void TestSydneyDaylightChanges ()
			{
				TimeZoneInfo tz;
				if (Environment.OSVersion.Platform == PlatformID.Unix)
					tz = TimeZoneInfo.FindSystemTimeZoneById ("Australia/Sydney");
				else
					tz = TimeZoneInfo.FindSystemTimeZoneById ("W. Australia Standard Time");

				var changes = (DaylightTime) getChanges.Invoke (tz, new object [] {2014});

				Assert.AreEqual (new TimeSpan (1, 0, 0), changes.Delta);
				Assert.AreEqual (new DateTime (2014, 10, 5, 2, 0, 0), changes.Start);
				Assert.AreEqual (new DateTime (2014, 4, 6, 3, 0, 0), changes.End);
			}

			[Test]
			public void AllTimeZonesDaylightChanges ()
			{
				foreach (var tz in TimeZoneInfo.GetSystemTimeZones ()) {
					try {
						for (var year = 1950; year <= DateTime.Now.Year; year++)
							getChanges.Invoke (tz, new object [] {year} );
					} catch (Exception e) {
						Assert.Fail ("TimeZone " + tz.Id + " exception: " + e.ToString ()); 
					}
				}
			}
		}
	}
}
