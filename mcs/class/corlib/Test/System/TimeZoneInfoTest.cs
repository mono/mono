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
using System.Linq;
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
		static FieldInfo localField;
		static FieldInfo cachedDataField;
		static object localFieldObj;

		public static string MapTimeZoneId (string id)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				return id;
			else {
				switch (id) {
				case "Pacific/Auckland":
					return "New Zealand Standard Time";
				case "Europe/Athens":
					return "GTB Standard Time";
				case "Europe/Chisinau":
					return "E. Europe Standard Time";
				case "America/New_York":
					return "Eastern Standard Time";
				case "America/Chicago":
				case "US/Central":
					return "Central Standard Time";
				case "America/Los_Angeles":
					return "Pacific Standard Time";
				case "Australia/Sydney":
				case "Australia/Melbourne":
					return "AUS Eastern Standard Time";
				case "Europe/Brussels":
				case "Europe/Copenhagen":
				case "Europe/Paris":
				case "Europe/Madrid":
					return "Romance Standard Time";
				case "Africa/Kinshasa":
					return "W. Central Africa Standard Time";
				case "Europe/Rome":
				case "Europe/Vatican":
				case "Europe/Vienna":
				case "Europe/Berlin":
				case "Europe/Luxembourg":
				case "Europe/Malta":
				case "Europe/Monaco":
				case "Europe/Amsterdam":
				case "Europe/Oslo":
				case "Europe/San_Marino":
					return "W. Europe Standard Time";
				case "America/Toronto":
					return "Eastern Standard Time";
				case "Asia/Tehran":
					return "Iran Standard Time";
				case "Europe/Guernsey":
				case "Europe/Dublin":
				case "Europe/Isle_of_Man":
				case "Europe/Jersey":
				case "Europe/Lisbon":
				case "Europe/London":
					return "GMT Standard Time";
				case "America/Havana":
					return "Cuba Standard Time";
				case "America/Anchorage":
					return "Alaskan Standard Time";
				case "Atlantic/Azores":
					return "Azores Standard Time";
				case "Asia/Jerusalem":
					return "Israel Standard Time";
				case "Asia/Amman":
					return "Jordan Standard Time";
				case "Europe/Tirane":
				case "Europe/Warsaw":
					return "Central European Standard Time";
				case "Europe/Sofia":
				case "Europe/Tallinn":
				case "Europe/Riga":
				case "Europe/Vilnius":
				case "Europe/Kiev":
					return "FLE Standard Time";
				case "Europe/Prague":
				case "Europe/Budapest":
				case "Europe/Bratislava":
					return "Central Europe Standard Time";
				default:
					Assert.Fail ($"No mapping defined for zone id '{id}'");
					return null;
				}
			}
		}

		public static void SetLocal (TimeZoneInfo val)
		{
			if (localField == null) {
#if MOBILE
					localField = typeof (TimeZoneInfo).GetField ("local",
							BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
#else
					cachedDataField = typeof (TimeZoneInfo).GetField ("s_cachedData",
							BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);

					localField = cachedDataField.FieldType.GetField ("_localTimeZone",
						BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
#endif
			}

			if (cachedDataField != null)
				localFieldObj = cachedDataField.GetValue (null);

			localField.SetValue (localFieldObj, val);
		}

		[TestFixture]
		public class PropertiesTests
		{
			[Test]
			public void GetLocal ()
			{
				TimeZoneInfo local = TimeZoneInfo.Local;
				Assert.IsNotNull (local);
				Assert.IsTrue (true);
			}

			[DllImport ("libc")]
			private static extern int readlink (string path, byte[] buffer, int buflen);

			[Test] // Covers #24958
			public void LocalId ()
			{
#if !MONOTOUCH && !XAMMAC
				byte[] buf = new byte [512];

				var path = "/etc/localtime";
				try {
					var ret = readlink (path, buf, buf.Length);
					if (ret == -1)
						return; // path is not a symbolic link, nothing to test
				} catch (DllNotFoundException e) {
					return;
				}
#endif
				
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
		[Category ("NotWasm")]
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
				DateTime june01 = new DateTime (2007, 06, 01);
				DateTime xmas = new DateTime (2007, 12, 25);
				Assert.IsTrue (london.IsDaylightSavingTime (june01), "June 01 is DST in London");
				Assert.IsFalse (london.IsDaylightSavingTime (xmas), "Xmas is not DST in London");
			}
		
			[Test]
			[Category ("MobileNotWorking")]
			public void DSTTransitions ()
			{
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
			public void DSTTransitionsUTC ()
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
				TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Pacific/Auckland"));
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
				TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Athens"));
				var date = new DateTime (2014, 3, 30 , 2, 0, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (2,0,0), tzi.GetUtcOffset (date));
			}

			[Test]
			public void TestAthensDST_InDSTDelta ()
			{
				// In .NET/.Net Core GetUtcOffset() returns the BaseUtcOffset for times within the hour
				// lost when DST starts and IsDaylightSavingTime() returns false for datetime and true for datetimeoffset

				TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Athens"));

				var date = new DateTime (2014, 3, 30 , 2, 0, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (2, 0, 0), tzi.GetUtcOffset (date));
				Assert.IsFalse (tzi.IsDaylightSavingTime (new DateTimeOffset (date, tzi.GetUtcOffset (date))));

				date = new DateTime (2014, 3, 30 , 3, 0, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (2, 0, 0), tzi.GetUtcOffset (date));
				Assert.IsTrue (tzi.IsDaylightSavingTime (new DateTimeOffset (date, tzi.GetUtcOffset (date))));

				date = new DateTime (2014, 3, 30 , 3, 1, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (2, 0, 0), tzi.GetUtcOffset (date));
				Assert.IsTrue (tzi.IsDaylightSavingTime (new DateTimeOffset (date, tzi.GetUtcOffset (date))));

				date = new DateTime (2014, 3, 30 , 3, 59, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (2, 0, 0), tzi.GetUtcOffset (date));
				Assert.IsTrue (tzi.IsDaylightSavingTime (new DateTimeOffset (date, tzi.GetUtcOffset (date))));

				date = new DateTime (2014, 3, 30 , 4, 0, 0);
				Assert.IsTrue (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (3, 0, 0), tzi.GetUtcOffset (date));
				Assert.IsTrue (tzi.IsDaylightSavingTime (new DateTimeOffset (date, tzi.GetUtcOffset (date))));
			}

			[Test] //Covers #41349
			public void TestIsDST_DateTimeOffset ()
			{
				TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Athens"));
				var date = new DateTime (2014, 3, 30 , 2, 0, 0);
				var offset = tzi.GetUtcOffset (date);
				var dateOffset = new DateTimeOffset (date, offset);
				Assert.IsFalse (tzi.IsDaylightSavingTime (dateOffset));

				date = new DateTime (2014, 3, 30 , 3, 0, 0);
				offset = tzi.GetUtcOffset (date);
				dateOffset = new DateTimeOffset (date, offset);
				Assert.IsTrue (tzi.IsDaylightSavingTime (dateOffset));
			}

			// https://github.com/mono/mono/issues/16742
			[Test]
			public void Bug_16472 ()
			{
				var parsedTime = DateTime.Parse ("1948-02-19T23:00:00Z", CultureInfo.InvariantCulture);
				var newTime = TimeZoneInfo.ConvertTime (parsedTime, TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Rome")));
				Assert.AreEqual (1948, newTime.Year);
			}

			// https://github.com/mono/mono/issues/9664
			[Test]
			public void Bug_9664 ()
			{
				TimeZoneInfo tzi;
				try {
					tzi = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("US/Central"));
				} catch (TimeZoneNotFoundException e) {
					Assert.Ignore ("Timezone US/Central not found.");
					return;
				}
				var date = new DateTime (2019, 3, 9, 21, 0, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (-6, 0, 0), tzi.GetUtcOffset (date));

				date = new DateTime (2019, 3, 10, 2, 0, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (-6, 0, 0), tzi.GetUtcOffset (date));

				date = new DateTime (2019, 3, 10, 2, 30, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (-6, 0, 0), tzi.GetUtcOffset (date));

				date = new DateTime (2019, 3, 10, 3, 0, 0);
				Assert.IsTrue (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (-5, 0, 0), tzi.GetUtcOffset (date));

#if !WINAOT // https://github.com/mono/mono/issues/15439
				tzi = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Vatican"));
				date = new DateTime (2018, 10, 28, 2, 15, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (1, 0, 0), tzi.GetUtcOffset (date));

				tzi = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Asia/Tehran"));
				date = new DateTime (2018, 9, 21, 23, 15, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (3, 30, 0), tzi.GetUtcOffset (date));

				// for Greenwitch Mean Time (Guernsey)
				tzi = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Guernsey"));
				date = new DateTime (2019, 10, 27, 1, 15, 0);
				Assert.IsFalse (tzi.IsDaylightSavingTime (date));
				Assert.AreEqual (new TimeSpan (0, 0, 0), tzi.GetUtcOffset (date));
#endif
			}

			[Test]
			public void Bug_16395 ()
			{
				// Cuba, Havana (Cuba Standard Time):    Jumps ahead at 12:00 AM on 3/8/2020 to 1:00 AM
				CheckJumpingIntoDST ("America/Havana",
									new DateTime (2020, 3, 8, 0, 0, 0), new DateTime (2020, 3, 8, 0, 30, 0), new DateTime (2020, 3, 8, 1, 0, 0), 
									new TimeSpan (-5, 0, 0), new TimeSpan (-4, 0, 0));

				// US, Kansas City, MO (US Central Time):    Jumps ahead at 2:00 AM on 3/8/2020 to 3:00 AM
				CheckJumpingIntoDST ("America/Chicago",
									new DateTime (2020, 3, 8, 2, 0, 0), new DateTime (2020, 3, 8, 2, 30, 0), new DateTime (2020, 3, 8, 3, 0, 0),
									new TimeSpan (-6, 0, 0), new TimeSpan (-5, 0, 0));

				// Anchorage, AK (Alaska Time):    Jumps ahead at 2:00 AM on 3/8/2020 to 3:00 AM
				CheckJumpingIntoDST ("America/Anchorage",
									new DateTime (2020, 3, 8, 2, 0, 0), new DateTime (2020, 3, 8, 2, 30, 0), new DateTime (2020, 3, 8, 3, 0, 0),
									new TimeSpan (-9, 0, 0), new TimeSpan (-8, 0, 0));

				// Azores ST (Ponta Delgada, Portugal):    Jumps ahead at 12:00 AM on 3/29/2020 to 1:00 AM
				CheckJumpingIntoDST ("Atlantic/Azores",
									new DateTime (2020, 3, 29, 0, 0, 0), new DateTime (2020, 3, 29, 0, 30, 0), new DateTime (2020, 3, 29, 1, 0, 0),
									new TimeSpan (-1, 0, 0), new TimeSpan (0, 0, 0));
									
				// Iran, Tehran (Iran ST):    Jumps ahead at 12:00 AM on 3/21/2020 to 1:00 AM
				CheckJumpingIntoDST ("Asia/Tehran",
									new DateTime (2020, 3, 21, 0, 0, 0), new DateTime (2020, 3, 21, 0, 30, 0), new DateTime (2020, 3, 21, 1, 0, 0),
									new TimeSpan (3, 30, 0), new TimeSpan (4, 30, 0));
									
				// Israel, Jerusalem (Israel ST):    Jumps ahead at 2:00 AM on 3/27/2020 to 3:00 AM
				CheckJumpingIntoDST ("Asia/Jerusalem",
									new DateTime (2020, 3, 27, 2, 0, 0), new DateTime (2020, 3, 27, 2, 30, 0), new DateTime (2020, 3, 27, 3, 0, 0),
									new TimeSpan (2, 0, 0), new TimeSpan (3, 0, 0));

				// Jordan, Amman (Eastern European ST):    Jumps ahead at 12:00 AM on 3/27/2020 to 1:00 AM
				CheckJumpingIntoDST ("Asia/Amman",
									new DateTime (2020, 3, 27, 0, 0, 0), new DateTime (2020, 3, 27, 0, 30, 0), new DateTime (2020, 3, 27, 1, 0, 0),
									new TimeSpan (2, 0, 0), new TimeSpan (3, 0, 0));

				// Albania, Tirana (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Tirane",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Austria, Vienna (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Vienna",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Belgium, Brussels (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Brussels",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Bulgaria, Sofia (Eastern European ST):    Jumps ahead at 3:00 AM on 3/29/2020 to 4:00 AM
				CheckJumpingIntoDST ("Europe/Sofia",
									new DateTime (2020, 3, 29, 3, 0, 0), new DateTime (2020, 3, 29, 3, 30, 0), new DateTime (2020, 3, 29, 4, 0, 0),
									new TimeSpan (2, 0, 0), new TimeSpan (3, 0, 0));

				// Czechia, Prague (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Prague",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Denmark, Copenhagen (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Copenhagen",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Estonia, Tallinn (Eastern European ST):    Jumps ahead at 3:00 AM on 3/29/2020 to 4:00 AM
				CheckJumpingIntoDST ("Europe/Tallinn",
									new DateTime (2020, 3, 29, 3, 0, 0), new DateTime (2020, 3, 29, 3, 30, 0), new DateTime (2020, 3, 29, 4, 0, 0),
									new TimeSpan (2, 0, 0), new TimeSpan (3, 0, 0));

				// France, Paris (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Paris",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Germany, Berlin (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Berlin",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Greece, Athens (Eastern European ST):    Jumps ahead at 3:00 AM on 3/29/2020 to 4:00 AM
				CheckJumpingIntoDST ("Europe/Athens",
									new DateTime (2020, 3, 29, 3, 0, 0), new DateTime (2020, 3, 29, 3, 30, 0), new DateTime (2020, 3, 29, 4, 0, 0),
									new TimeSpan (2, 0, 0), new TimeSpan (3, 0, 0));

				// Guernsey (UK)    Jumps ahead at 1:00 AM on 3/29/2020 to 2:00 AM
				CheckJumpingIntoDST ("Europe/Guernsey",
									new DateTime (2020, 3, 29, 1, 0, 0), new DateTime (2020, 3, 29, 1, 30, 0), new DateTime (2020, 3, 29, 2, 0, 0),
									new TimeSpan (0, 0, 0), new TimeSpan (1, 0, 0));

				// Holy See, Vatican City (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Vatican",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Hungary, Budapest (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Budapest",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// // Ireland, Dublin (Greenwich Mean Time -> Irish Standard Time):    Jumps ahead at 1:00 AM on 3/29/2020 to 2:00 AM
				// CheckJumpingIntoDST ("Europe/Dublin",
				// 					new DateTime (2020, 3, 29, 1, 0, 0), new DateTime (2020, 3, 29, 1, 30, 0), new DateTime (2020, 3, 29, 2, 0, 0),
				// 					new TimeSpan (0, 0, 0), new TimeSpan (1, 0, 0));

				// UK, Douglas, Isle of Man (GMT+1:00):    Jumps ahead at 1:00 AM on 3/29/2020 to 2:00 AM
				CheckJumpingIntoDST ("Europe/Isle_of_Man",
									new DateTime (2020, 3, 29, 1, 0, 0), new DateTime (2020, 3, 29, 1, 30, 0), new DateTime (2020, 3, 29, 2, 0, 0),
									new TimeSpan (0, 0, 0), new TimeSpan (1, 0, 0));

				// Italy, Rome (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Rome",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Jersey (UK):   Jumps ahead at 1:00 AM on 3/29/2020 to 2:00 AM
				CheckJumpingIntoDST ("Europe/Jersey",
									new DateTime (2020, 3, 29, 1, 0, 0), new DateTime (2020, 3, 29, 1, 30, 0), new DateTime (2020, 3, 29, 2, 0, 0),
									new TimeSpan (0, 0, 0), new TimeSpan (1, 0, 0));

				// Latvia, Riga (Eastern European ST):    Jumps ahead at 3:00 AM on 3/29/2020 to 4:00 AM
				CheckJumpingIntoDST ("Europe/Riga",
									new DateTime (2020, 3, 29, 3, 0, 0), new DateTime (2020, 3, 29, 3, 30, 0), new DateTime (2020, 3, 29, 4, 0, 0),
									new TimeSpan (2, 0, 0), new TimeSpan (3, 0, 0));

				// Lithuania, Vilnius (Eastern European ST):    Jumps ahead at 3:00 AM on 3/29/2020 to 4:00 AM
				CheckJumpingIntoDST ("Europe/Vilnius",
									new DateTime (2020, 3, 29, 3, 0, 0), new DateTime (2020, 3, 29, 3, 30, 0), new DateTime (2020, 3, 29, 4, 0, 0),
									new TimeSpan (2, 0, 0), new TimeSpan (3, 0, 0));

				// Luxembourg, Luxembourg (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Luxembourg",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Malta, Valletta (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Malta",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Moldova, Chişinău (Eastern European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Chisinau",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (2, 0, 0), new TimeSpan (3, 0, 0));

				// Monaco, Monaco (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Monaco",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Netherlands, Amsterdam (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Amsterdam",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Norway, Oslo (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Oslo",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Poland, Warsaw (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Warsaw",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Portugal, Lisbon (Western European ST):    Jumps ahead at 1:00 AM on 3/29/2020 to 2:00 AM
				CheckJumpingIntoDST ("Europe/Lisbon",
									new DateTime (2020, 3, 29, 1, 0, 0), new DateTime (2020, 3, 29, 1, 30, 0), new DateTime (2020, 3, 29, 2, 0, 0),
									new TimeSpan (0, 0, 0), new TimeSpan (1, 0, 0));

				// San Marino, San Marino (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/San_Marino",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Slovakia, Bratislava (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Bratislava",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Spain, Madrid (Central European ST):    Jumps ahead at 2:00 AM on 3/29/2020 to 3:00 AM
				CheckJumpingIntoDST ("Europe/Madrid",
									new DateTime (2020, 3, 29, 2, 0, 0), new DateTime (2020, 3, 29, 2, 30, 0), new DateTime (2020, 3, 29, 3, 0, 0),
									new TimeSpan (1, 0, 0), new TimeSpan (2, 0, 0));

				// Ukraine, Kiev (Eastern European ST):    Jumps ahead at 3:00 AM on 3/29/2020 to 4:00 AM
				CheckJumpingIntoDST ("Europe/Kiev",
									new DateTime (2020, 3, 29, 3, 0, 0), new DateTime (2020, 3, 29, 3, 30, 0), new DateTime (2020, 3, 29, 4, 0, 0),
									new TimeSpan (2, 0, 0), new TimeSpan (3, 0, 0));

				// United Kingdom, London (British ST):    Jumps ahead at 1:00 AM on 3/29/2020 to 2:00 AM
				CheckJumpingIntoDST ("Europe/London",
									new DateTime (2020, 3, 29, 1, 0, 0), new DateTime (2020, 3, 29, 1, 30, 0), new DateTime (2020, 3, 29, 2, 0, 0),
									new TimeSpan (0, 0, 0), new TimeSpan (1, 0, 0));
			}

			void CheckJumpingIntoDST (string tzId, DateTime dstDeltaStart, DateTime inDstDelta, DateTime dstDeltaEnd, TimeSpan baseOffset, TimeSpan dstOffset)
			{
				var tzi = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId (tzId));
				Assert.IsFalse (tzi.IsDaylightSavingTime (dstDeltaStart), $"{tzId}: #1");
				Assert.AreEqual (baseOffset, tzi.GetUtcOffset (dstDeltaStart), $"{tzId}: #2");

				Assert.IsFalse (tzi.IsDaylightSavingTime (inDstDelta), $"{tzId}: #3");
				Assert.AreEqual (baseOffset, tzi.GetUtcOffset (inDstDelta), $"{tzId}: #4");

				Assert.IsTrue (tzi.IsDaylightSavingTime (dstDeltaEnd), $"{tzId}: #5");
				Assert.AreEqual (dstOffset, tzi.GetUtcOffset (dstDeltaEnd), $"{tzId}: #6");
			}
		}

		[TestFixture]
		[Category ("NotWasm")]
		public class ConvertTimeTests_LocalUtc : ConvertTimeTests
		{
			static TimeZoneInfo oldLocal;

			[SetUp]
			public void SetLocal ()
			{
				base.CreateTimeZones ();

				oldLocal = TimeZoneInfo.Local;
				TimeZoneInfoTest.SetLocal (TimeZoneInfo.GetSystemTimeZones().First(t => t.BaseUtcOffset == TimeSpan.Zero));
			}

			[TearDown]
			public void RestoreLocal ()
			{
				TimeZoneInfoTest.SetLocal (oldLocal);
			}
		}

		[TestFixture]
		[Category ("NotWasm")]
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
				DateTime utc = new DateTime (2007, 12, 25, 12, 0, 0);
				DateTime converted = TimeZoneInfo.ConvertTimeFromUtc (utc, london);
				Assert.AreEqual (utc, converted);
			}
		
			[Test]
			public void ConvertFromUtc_ConvertInSummer ()
			{
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
			public void ConvertFromToUtc_Utc ()
			{
				DateTime utc = DateTime.UtcNow;
				Assert.AreEqual (utc.Kind, DateTimeKind.Utc);
				DateTime converted = TimeZoneInfo.ConvertTimeFromUtc (utc, TimeZoneInfo.Utc);
				Assert.AreEqual (DateTimeKind.Utc, converted.Kind);
				DateTime back = TimeZoneInfo.ConvertTimeToUtc (converted, TimeZoneInfo.Utc);
				Assert.AreEqual (back.Kind, DateTimeKind.Utc);
				Assert.AreEqual (utc, back);
			}

			[Test]
			public void ConvertFromToLocal ()
			{
				DateTime utc = DateTime.UtcNow;
				Assert.AreEqual (utc.Kind, DateTimeKind.Utc);
				DateTime converted = TimeZoneInfo.ConvertTimeFromUtc (utc, TimeZoneInfo.Local);
				var expectedKind = (TimeZoneInfo.Local == TimeZoneInfo.Utc)? DateTimeKind.Utc : DateTimeKind.Local;
				Assert.AreEqual (expectedKind, converted.Kind);
				DateTime back = TimeZoneInfo.ConvertTimeToUtc (converted, TimeZoneInfo.Local);
				Assert.AreEqual (back.Kind, DateTimeKind.Utc);
				Assert.AreEqual (utc, back);
			}

			[Test]
			public void ConvertToTimeZone ()
			{
				TimeZoneInfo.ConvertTime (DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Pacific/Auckland")));
			}

			[Test]
			[ExpectedException (typeof (ArgumentNullException))]
			public void ConvertTime_DateTime_TimeZoneInfo_DestinationTimeZoneIsNull ()
			{
				TimeZoneInfo.ConvertTime (DateTime.Now, null);
			}

			[Test]
			[Category ("MobileNotWorking")]
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
				var expectedKind = (TimeZoneInfo.Local == TimeZoneInfo.Utc)? DateTimeKind.Utc : DateTimeKind.Local;
				Assert.AreEqual (expectedKind,  ddt.Kind, "#3.1");
				Assert.AreEqual (DateTimeKind.Unspecified, sdt.Kind, "#3.2");
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
				TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Pacific/Auckland"));

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
				
				TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("America/New_York"));
				TimeZoneInfo pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("America/Los_Angeles"));

				DateTime lastMidnight = new DateTime (new DateTime (2012, 06, 13).Ticks, DateTimeKind.Unspecified);
				DateTime lastMidnightAsEST = TimeZoneInfo.ConvertTime (lastMidnight, pacificTimeZone, easternTimeZone);
				DateTime lastMidnightAsPST = TimeZoneInfo.ConvertTime (lastMidnightAsEST, easternTimeZone, pacificTimeZone);
			
				// Last midnight in PST as EST should be 3AM
				DateTime expectedDate = new DateTime (2012, 06, 13, 3, 0, 0);

				Assert.AreEqual (expectedDate, lastMidnightAsEST);
				Assert.AreEqual (lastMidnight, lastMidnightAsPST);
			}

			[Test]
			public void ConvertTimeBySystemTimeZoneId_UtcId ()
			{
				DateTime localTime = TimeZoneInfo.ConvertTime (DateTime.UtcNow, TimeZoneInfo.Utc, TimeZoneInfo.Local);

				TimeZoneInfo.ConvertTimeBySystemTimeZoneId (DateTime.UtcNow, TimeZoneInfo.Utc.Id, TimeZoneInfo.Local.Id);
			}
		}
		
		[TestFixture]
		[Category ("NotWasm")]
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
		[Category ("NotWasm")]
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
			[Category ("MobileNotWorking")]
			public void AmbiguousDates ()
			{
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 1, 0, 0)));
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 1, 0, 1)));
				Assert.IsFalse (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 2, 0, 0)));
				Assert.IsFalse (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 2, 0, 1)));
			}
		
			[Test]
			[Category ("MobileNotWorking")]
			public void AmbiguousUTCDates ()
			{
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 0, 0, 0, DateTimeKind.Utc)));
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 0, 0, 1, DateTimeKind.Utc)));
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 0, 59, 59, DateTimeKind.Utc)));
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 1, 0, 0, DateTimeKind.Utc)));
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
		[Category ("NotWasm")]
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
				global::System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> systemTZ = TimeZoneInfo.GetSystemTimeZones ();
				Assert.IsNotNull(systemTZ, "SystemTZ is null");
				Assert.IsFalse (systemTZ.Count == 0, "SystemTZ is empty");
			}
		
			[Test]
			public void ContainsBrussels ()
			{
				global::System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> systemTZ = TimeZoneInfo.GetSystemTimeZones ();
				foreach (TimeZoneInfo tz in systemTZ) {
					if (tz.Id == MapTimeZoneId ("Europe/Brussels"))
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

#if !MOBILE
			[Test]
			public void WindowsRegistryTimezoneWithParentheses ()
			{
				var memberInfos = typeof (TimeZoneInfo).GetMember ("TrimSpecial", MemberTypes.Method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

				if (memberInfos.Length == 0)
					Assert.Ignore ("TrimSpecial method not found");

				var name = ((MethodInfo)memberInfos[0]).Invoke (null, new object [] { " <--->  Central Standard Time (Mexico)   ||<<>>" });
				Assert.AreEqual (name, "Central Standard Time (Mexico)", "#1");
			}
#endif
		}
		
		[TestFixture]
		[Category ("NotWasm")]
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
				TimeZoneInfo.FindSystemTimeZoneById ("Neverland/The_Lagoon");
			}
		
			[Test]
			public void FindBrusselsTZ ()
			{
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Brussels"));
				Assert.IsNotNull (brussels);
			}
		
			[Test]
			public void OffsetIsCorrectInKinshasa ()
			{
				TimeZoneInfo kin = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Africa/Kinshasa"));
				Assert.AreEqual (new TimeSpan (1,0,0), kin.BaseUtcOffset, "BaseUtcOffset in Kinshasa is not +1h");
			}
		
			[Test]
			public void OffsetIsCorrectInBrussels ()
			{
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Brussels"));
				Assert.AreEqual (new TimeSpan (1,0,0), brussels.BaseUtcOffset, "BaseUtcOffset for Brussels is not +1h");
			}
		
			[Test]
			[Category ("MobileNotWorking")]
			[Category ("NotOnWindows")]
			public void DSTInKinshasa ()
			{
				TimeZoneInfo kin = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Africa/Kinshasa"));
				Assert.IsTrue (kin.SupportsDaylightSavingTime);
			}
		
			[Test]
			public void BrusselsSupportsDST ()
			{
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Brussels"));
				Assert.IsTrue (brussels.SupportsDaylightSavingTime);
			}
		
			[Test]
			public void MelbourneSupportsDST ()
			{
				TimeZoneInfo melbourne = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Australia/Melbourne"));
				Assert.IsTrue (melbourne.SupportsDaylightSavingTime);
			}
		
			[Test]
			public void RomeAndVaticanSharesTime ()
			{
				TimeZoneInfo rome = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Rome"));
				TimeZoneInfo vatican = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Vatican"));
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
			public void FindIsraelStandardTime ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Win32NT)
					Assert.Ignore ("Only applies to Windows.");

				TimeZoneInfo.FindSystemTimeZoneById ("Israel Standard Time");
			}

			[Test]
			public void SubminuteDSTOffsets ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					Assert.Ignore ();

				var subMinuteDSTs = new string [] {
					"Europe/Dublin", // Europe/Dublin has a DST offset of 34 minutes and 39 seconds in 1916.
					"Europe/Amsterdam",
					"America/St_Johns",
					"Europe/Moscow",
					"Europe/Riga",
				};
				foreach (var tz in subMinuteDSTs) {
					TimeZoneInfo.FindSystemTimeZoneById (tz);
				}
			}

			[Test]
			[ExpectedException (typeof (TimeZoneNotFoundException))]
			public void InvalidName ()
			{
				TimeZoneInfo.FindSystemTimeZoneById ("N/A");
			}
		}
		
		[TestFixture]
		[Category ("NotWasm")]
		public class GetAmbiguousTimeOffsetsTests
		{
			[Test]
			[ExpectedException (typeof(ArgumentException))]
			public void DateIsNotAmbiguous ()
			{
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Brussels"));
				DateTime date = new DateTime (2007, 05, 11, 11, 40, 00);
				brussels.GetAmbiguousTimeOffsets (date);
			}
		
			[Test]
			public void AmbiguousOffsets ()
			{
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Brussels"));
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
			[Category ("MobileNotWorking")]
			public void GetUtcOffset_FromUnspecified ()
			{
				var d = dst1Start.Add (dstOffset);
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));

				d = dst1End.Add (-dstOffset);
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,1,0, 1))));

				d = dst2Start.Add (dstOffset);
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d));
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0, 1))));

				d = dst2End.Add (-dstOffset);
				Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,0,0,-1))));
				Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset (d.Add (new TimeSpan(0,1,0, 1))));
			}

		  [Test]
		  [Category ("MobileNotWorking")]
		  public void  GetUtcOffset_FromDateTimeOffset ()
		  {
			  DateTimeOffset offset;

			  offset = new DateTimeOffset(dst1Start, baseUtcOffset);
			  Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset(offset.Add(new TimeSpan(0, 0, 0, -1))), "dst1Start_with_baseUtcOffset#before");
			  Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset(offset), "dst1Start_with_baseUtcOffset#exact");
			  Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset(offset.Add(new TimeSpan(0, 0, 0, 1))), "dst1Start_with_baseUtcOffset#after");

			  offset = new DateTimeOffset(dst1End, dstOffset + baseUtcOffset);
			  Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset(offset.Add(new TimeSpan(0, 0, 0, -1))), "dst1End_with_dstOffset+baseUtcOffset#before");
			  Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset(offset), "dst1End_with_dstOffset+baseUtcOffset#exact");
			  Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset(offset.Add(new TimeSpan(0, 0, 0, 1))), "dst1End_with_dstOffset+baseUtcOffset#after");

			  offset = new DateTimeOffset(dst2Start, baseUtcOffset);
			  Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset(offset.Add(new TimeSpan(0, 0, 0, -1))), "dst2Start_with_baseUtcOffset#before");
			  Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset(offset), "dst2Start_with_baseUtcOffset#exact");
			  Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset(offset.Add(new TimeSpan(0, 0, 0, 1))), "dst2Start_with_baseUtcOffset#after");

			  offset = new DateTimeOffset(dst2End, baseUtcOffset + dstOffset);
			  Assert.AreEqual(dstUtcOffset, cairo.GetUtcOffset(offset.Add(new TimeSpan(0, 0, 0, -1))), "dst2End_with_dstOffset+baseUtcOffset#before");
			  Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset(offset), "dst2End_with_dstOffset+baseUtcOffset#exact");
			  Assert.AreEqual(baseUtcOffset, cairo.GetUtcOffset(offset.Add(new TimeSpan(0, 0, 0, 1))), "dst2End_with_dstOffset+baseUtcOffset#after");
		  }

			[Test]
			[Category ("MobileNotWorking")]
			public void DTS_WithMinimalDate ()
			{
				TimeZoneInfo.TransitionTime startTransition, endTransition;
				startTransition = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 4, 0, 0),
																				  10, 2, DayOfWeek.Sunday);
				endTransition = TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 3, 0, 0),
																				3, 2, DayOfWeek.Sunday);

				var ctz = TimeZoneInfo.CreateCustomTimeZone ("test", TimeSpan.FromHours (-5), "display", "sdisplay", "dst", new [] {
					TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (DateTime.MinValue, DateTime.MaxValue.Date, TimeSpan.FromHours (-1), startTransition, endTransition) });

				var offset = ctz.GetUtcOffset (DateTime.MinValue);
				Assert.AreEqual (TimeSpan.FromHours (-6), offset);
			}
    }

		[TestFixture]
		[Category ("NotWasm")]
		public class GetDaylightChanges
		{
			private static void GetDaylightTime (TimeZoneInfo tz, int year, out DateTime start, out DateTime end, out TimeSpan delta)
			{
#if !MOBILE
					var rule = tz.GetAdjustmentRules ().FirstOrDefault (r => r.DateStart.Year <= year && r.DateEnd.Year >= year);
					if (rule == null) {
						start = DateTime.MinValue;
						end = DateTime.MinValue;
						delta = TimeSpan.Zero;
						return;
					}
					var method = typeof (TimeZoneInfo).GetMethod ("GetDaylightTime", BindingFlags.Instance | BindingFlags.NonPublic);
					var daylightTime = method.Invoke(tz, new object[] { year, rule, null });
					var dts = daylightTime.GetType(); // internal readonly struct DaylightTimeStruct
					start = (DateTime) dts.GetField ("Start").GetValue (daylightTime);
					end = (DateTime) dts.GetField ("End").GetValue (daylightTime);
					delta = (TimeSpan) dts.GetField ("Delta").GetValue (daylightTime);
#else
					MethodInfo getChanges = typeof (TimeZoneInfo).GetMethod ("GetDaylightChanges", BindingFlags.Instance | BindingFlags.NonPublic);
					var changes = (DaylightTime) getChanges.Invoke (tz, new object [] {year});
					start = changes.Start;
					end = changes.End;
					delta = changes.Delta;
#endif
			}

			[Test]
			[Category ("MobileNotWorking")]
			[Category ("NotOnWindows")]
			public void TestSydneyDaylightChanges ()
			{
				TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Australia/Sydney"));

				GetDaylightTime (tz, 2014, out DateTime start, out DateTime end, out TimeSpan delta);

				Assert.AreEqual (new TimeSpan (1, 0, 0), delta);
				Assert.AreEqual (new DateTime (2014, 10, 6, 2, 0, 0), start);
				Assert.AreEqual (new DateTime (2014, 4, 6, 2, 59, 59), end);
			}

			[Test]
			[Category ("MobileNotWorking")]
			[Category ("NotOnWindows")]
			public void TestAthensDaylightChanges ()
			{
				TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById (MapTimeZoneId ("Europe/Athens"));

				GetDaylightTime (tz, 2014, out DateTime start, out DateTime end, out TimeSpan delta);

				Assert.AreEqual (new TimeSpan (0, 0, 0), delta);
				Assert.AreEqual (new DateTime (2014, 10, 27, 3, 0, 0), start);
				Assert.AreEqual (new DateTime (2014, 03, 30, 2, 59, 59), end);
			}

			[Test]
			public void AllTimeZonesDaylightChanges ()
			{
				foreach (var tz in TimeZoneInfo.GetSystemTimeZones ()) {
					try {
						for (var year = 1950; year <= 2051; year++)
							GetDaylightTime (tz, year, out DateTime start, out DateTime end, out TimeSpan delta);
					} catch (Exception e) {
						Assert.Fail ("TimeZone " + tz.Id + " exception: " + e.ToString ()); 
					}
				}
			}
		}

		[TestFixture]
		public class ParseTZBuffer
		{
			MethodInfo parseTZBuffer;

			[SetUp]
			public void Setup()
			{
				var flags = BindingFlags.Static | BindingFlags.NonPublic;
				parseTZBuffer = typeof (TimeZoneInfo).GetMethod ("ParseTZBuffer", flags);
			}

			[Test]
			[Category ("NotWorking")]
			public void Bug31432 ()
			{
				// Europe/Moscow from failing device
				var base64Data = "VFppZjIAAAAAAAAAAAAAAAAAAAAAAAAPAAAADwAAAAAAAABNAAAADwAAACKbXx7HnT7yeZ4q7vme9zlpn4RX+aDYbOmhABYJoTymQKQQbcCkPTKwpRVosKU9A8CnHkVQtaQZYBUnp9AWGNxAFwjbUBf6D8AY6g7QGdtDQBrMk9AbvKDwHKyR8B2cgvAejHPwH3xk8CBsVfAhXEbwIkw38CM8KPAkLBnwJRwK8CYL+/AnBSdwJ/UYcCjlF4ApeL+AKdTQQCrEszArtNxwLKTNcC2UvnAuhK9wL3SgcDBkkXAxXbzwMnKX8DM9nvA0UnnwNR2A8DYyW/A2/WLwOBt4cDjdRPA5+1pwOr0m8DvbPHA8pkNwPbsecD6GJXA/mwBwQGYHcEGEHPBCRelwQ2P+8EQly3BFQ+DwRgWtcEcjwvBH7snwSQOk8EnOq/BK44bwS66N8EzMo3BNjm/wVEwdYAIBAgMBAwUEBQYFBwgHCQcJBwkHCQoLCgsKCwoLCgsKCwoMDQoJBwsKCwoLCgsKCwoLCgsKCwoLCgsKCwoLCgsKCwoLCgsKCwoLCg4KAAAjOQAAAAAxhwEEAAAjdwAAAAA/lwEIAAAqMAADAAA4QAENAABGUAEPAAAqMAARAAAcIAAVAAA4QAEZAAAqMAARAAA4QAEZAAAqMAEdAAAcIAAVAAA4QAARTU1UAE1TVABNRFNUAFMATQBNU0sARUVUAE1TRABFRVNUAAAAAAAAAAAAAAABAQEBAQAAAAAAAAAAAAAAAAAAAFRaaWYyAAAAAAAAAAAAAAAAAAAAAAAAEAAAABAAAAAAAAAATgAAABAAAAAm/////1a2wMf/////m18ex/////+dPvJ5/////54q7vn/////nvc5af////+fhFf5/////6DYbOn/////oQAWCf////+hPKZA/////6QQbcD/////pD0ysP////+lFWiw/////6U9A8D/////px5FUP////+1pBlgAAAAABUnp9AAAAAAFhjcQAAAAAAXCNtQAAAAABf6D8AAAAAAGOoO0AAAAAAZ20NAAAAAABrMk9AAAAAAG7yg8AAAAAAcrJHwAAAAAB2cgvAAAAAAHoxz8AAAAAAffGTwAAAAACBsVfAAAAAAIVxG8AAAAAAiTDfwAAAAACM8KPAAAAAAJCwZ8AAAAAAlHArwAAAAACYL+/AAAAAAJwUncAAAAAAn9RhwAAAAACjlF4AAAAAAKXi/gAAAAAAp1NBAAAAAACrEszAAAAAAK7TccAAAAAAspM1wAAAAAC2UvnAAAAAALoSvcAAAAAAvdKBwAAAAADBkkXAAAAAAMV288AAAAAAycpfwAAAAADM9nvAAAAAANFJ58AAAAAA1HYDwAAAAADYyW/AAAAAANv1i8AAAAAA4G3hwAAAAADjdRPAAAAAAOftacAAAAAA6vSbwAAAAADvbPHAAAAAAPKZDcAAAAAA9ux5wAAAAAD6GJXAAAAAAP5sAcAAAAABAZgdwAAAAAEGEHPAAAAAAQkXpcAAAAABDY/7wAAAAAEQly3AAAAAARUPg8AAAAABGBa1wAAAAAEcjwvAAAAAAR+7J8AAAAABJA6TwAAAAAEnOq/AAAAAASuOG8AAAAABLro3wAAAAAEzMo3AAAAAATY5v8AAAAABUTB1gAQMCAwQCBAYFBgcGCAkICggKCAoICgsMCwwLDAsMCwwLDAsNDgsKCAwLDAsMCwwLDAsMCwwLDAsMCwwLDAsMCwwLDAsMCwwLDAsMCw8LAAAjOQAAAAAjOQAEAAAxhwEIAAAjdwAEAAA/lwEMAAAqMAADAAA4QAERAABGUAETAAAqMAAVAAAcIAAZAAA4QAEdAAAqMAAVAAA4QAEdAAAqMAEhAAAcIAAZAAA4QAAVTE1UAE1NVABNU1QATURTVABTAE0ATVNLAEVFVABNU0QARUVTVAAAAAAAAAAAAAAAAAEBAQEBAAAAAAAAAAAAAAAAAAAAAApNU0stMwo=";

				var data = Convert.FromBase64String (base64Data);

				var tz = parseTZBuffer.Invoke (null, new object[] { "Test", data, data.Length});
				Assert.IsTrue (tz != null);
			}
		}
	}
}
