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
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

using NUnit.Framework;
#if NET_2_0
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
					return;
				TimeZoneInfo local = TimeZoneInfo.Local;
				Assert.IsNotNull (local);
				Assert.IsTrue (true);
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
					return;
				DateTime june01 = new DateTime (2007, 06, 01);
				DateTime xmas = new DateTime (2007, 12, 25);
				Assert.IsTrue (london.IsDaylightSavingTime (june01), "June 01 is DST in London");
				Assert.IsFalse (london.IsDaylightSavingTime (xmas), "Xmas is not DST in London");
			}
		
			[Test]
			public void DSTTransisions ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;
				DateTime beforeDST = new DateTime (2007, 03, 25, 0, 59, 59, DateTimeKind.Unspecified);
				DateTime startDST = new DateTime (2007, 03, 25, 2, 0, 0, DateTimeKind.Unspecified);
				DateTime endDST = new DateTime (2007, 10, 28, 1, 59, 59, DateTimeKind.Unspecified);
				DateTime afterDST = new DateTime (2007, 10, 28, 2, 0, 0, DateTimeKind.Unspecified);
				Assert.IsFalse (london.IsDaylightSavingTime (beforeDST), "Just before DST");
				Assert.IsTrue (london.IsDaylightSavingTime (startDST), "the first seconds of DST");
				Assert.IsTrue (london.IsDaylightSavingTime (endDST), "The last seconds of DST");
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
					return;
				DateTime utc = new DateTime (2007, 12, 25, 12, 0, 0);
				DateTime converted = TimeZoneInfo.ConvertTimeFromUtc (utc, london);
				Assert.AreEqual (utc, converted);
			}
		
			[Test]
			public void ConvertFromUtc_ConvertInSummer ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;
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
					return;
				DateTime utc = DateTime.UtcNow;
				Assert.AreEqual (utc.Kind, DateTimeKind.Utc);
				DateTime converted = TimeZoneInfo.ConvertTimeFromUtc (utc, london);
				Assert.AreEqual (converted.Kind, DateTimeKind.Unspecified);
				DateTime back = TimeZoneInfo.ConvertTimeToUtc (converted, london);
				Assert.AreEqual (back.Kind, DateTimeKind.Utc);
				Assert.AreEqual (utc, back);
		
			}

			[Test]
			public void ConvertToTimeZone ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;

				TimeZoneInfo.ConvertTime (DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland"));
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
					return;
				Assert.IsFalse (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 1, 0, 0)));
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 1, 0, 1)));
				Assert.IsTrue (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 2, 0, 0)));
				Assert.IsFalse (london.IsAmbiguousTime (new DateTime (2007, 10, 28, 2, 0, 1)));
			}
		
			[Test]
			public void AmbiguousUTCDates ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;
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
			public void NotEmpty ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;
				global::System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> systemTZ = TimeZoneInfo.GetSystemTimeZones ();
				Assert.IsNotNull(systemTZ, "SystemTZ is null");
				Assert.IsFalse (systemTZ.Count == 0, "SystemTZ is empty");
			}
		
			[Test]
			public void ContainsBrussels ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;
				global::System.Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> systemTZ = TimeZoneInfo.GetSystemTimeZones ();
				foreach (TimeZoneInfo tz in systemTZ) {
					if (tz.Id == "Europe/Brussels")
						return;
				}
				Assert.Fail ("Europe/Brussels not found in SystemTZ");
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
					return;
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Brussels");
				Assert.IsNotNull (brussels);
			}
		
			[Test]
			public void OffsetIsCorrectInKinshasa ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;
				TimeZoneInfo kin = TimeZoneInfo.FindSystemTimeZoneById ("Africa/Kinshasa");
				Assert.AreEqual (new TimeSpan (1,0,0), kin.BaseUtcOffset, "BaseUtcOffset in Kinshasa is not +1h");
			}
		
			[Test]
			public void OffsetIsCorrectInBrussels ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Brussels");
				Assert.AreEqual (new TimeSpan (1,0,0), brussels.BaseUtcOffset, "BaseUtcOffset for Brussels is not +1h");
			}
		
			[Test]
			public void NoDSTInKinshasa ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;
				TimeZoneInfo kin = TimeZoneInfo.FindSystemTimeZoneById ("Africa/Kinshasa");
				Assert.IsFalse (kin.SupportsDaylightSavingTime);
			}
		
			[Test]
			public void BrusselsSupportsDST ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;
				TimeZoneInfo brussels = TimeZoneInfo.FindSystemTimeZoneById ("Europe/Brussels");
				Assert.IsTrue (brussels.SupportsDaylightSavingTime);
			}
		
			[Test]
			public void MelbourneSupportsDST ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;
				TimeZoneInfo melbourne = TimeZoneInfo.FindSystemTimeZoneById ("Australia/Melbourne");
				Assert.IsTrue (melbourne.SupportsDaylightSavingTime);
			}
		
			[Test]
			public void RomeAndVaticanSharesTime ()
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return;
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
					return;
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
	}
}
#endif
