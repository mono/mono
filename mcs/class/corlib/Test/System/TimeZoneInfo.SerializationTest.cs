using System;
using System.IO;
using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class SerializedStringTests
	{
		[Test]
		public void SerializeUtc ()
		{
			Assert.AreEqual ("UTC;0;UTC;UTC;UTC;;", TimeZoneInfo.Utc.ToSerializedString ());
		}

		[Test]
		public void DeserializeUtc ()
		{
			var utc = TimeZoneInfo.FromSerializedString ("UTC;0;UTC;UTC;UTC;;");
			Assert.AreEqual ("UTC", utc.Id);
			Assert.AreEqual ("UTC", utc.DisplayName);
			Assert.AreEqual ("UTC", utc.StandardName);
			Assert.IsFalse (utc.SupportsDaylightSavingTime);
			Assert.AreEqual (0, utc.GetAdjustmentRules ().Length);
		}

		[Test]
		public void SerializeCustomUtcZoneWithOddNaming ()
		{
			var tz1 = TimeZoneInfo.CreateCustomTimeZone (@"My\; Zone, @1!.", TimeSpan.FromMinutes (0), @"My\\; Zone 1 Name", "My; Zone 1 Standard Time");
			Assert.AreEqual (@"My\\\; Zone, @1!.;0;My\\\\\; Zone 1 Name;My\; Zone 1 Standard Time;My\; Zone 1 Standard Time;;", tz1.ToSerializedString ());
		}

		[Test]
		public void SerializeCustomZoneWithOddOffset ()
		{
			var tz2 = TimeZoneInfo.CreateCustomTimeZone ("My Zone 2", TimeSpan.FromHours (1.25), "My Zone 2 Name", "My Zone 2 Standard Time");
			Assert.AreEqual ("My Zone 2;75;My Zone 2 Name;My Zone 2 Standard Time;My Zone 2 Standard Time;;", tz2.ToSerializedString ());
		}

		[Test]
		public void SerializeCustomZoneWithFloatingDaylightTransitions ()
		{
			var tz3rules = new TimeZoneInfo.AdjustmentRule[] { TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (1, 1, 1), new DateTime (9999, 12, 31), TimeSpan.FromMinutes (23), TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 15, 58, 0), 3, 2, DayOfWeek.Tuesday), TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 15, 59, 999), 6, 2, DayOfWeek.Tuesday)) };
			var tz3 = TimeZoneInfo.CreateCustomTimeZone ("My Zone 3", TimeSpan.FromHours (-4), "My Zone 3 Name", "My Zone 3 Standard Time", "My Zone 3 Daylight Time", tz3rules);
			Assert.AreEqual ("My Zone 3;-240;My Zone 3 Name;My Zone 3 Standard Time;My Zone 3 Daylight Time;[01:01:0001;12:31:9999;23;[0;02:15:58;3;2;2;];[0;02:15:59.999;6;2;2;];];", tz3.ToSerializedString ());
		}

		[Test]
		public void SerializeCustomZoneWithFixedDaylightTransitions ()
		{
			var tz4rules = new TimeZoneInfo.AdjustmentRule[] { TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (1, 1, 1), new DateTime (9999, 12, 31), TimeSpan.FromMinutes (23), TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1, 1, 1, 2, 15, 59, 48), 3, 2), TimeZoneInfo.TransitionTime.CreateFixedDateRule (new DateTime (1, 1, 1, 2, 15, 59, 999), 6, 2)) };
			var tz4 = TimeZoneInfo.CreateCustomTimeZone ("My Zone 4", TimeSpan.FromHours (-4), "My Zone 4 Name", "My Zone 4 Standard Time", "My Zone 4 Daylight Time", tz4rules);
			Assert.AreEqual ("My Zone 4;-240;My Zone 4 Name;My Zone 4 Standard Time;My Zone 4 Daylight Time;[01:01:0001;12:31:9999;23;[1;02:15:59.048;3;2;];[1;02:15:59.999;6;2;];];", tz4.ToSerializedString ());
		}

		[Test]
		public void SerializeCustomZoneWithMultipleDaylightRules ()
		{
			var tz5rules = new TimeZoneInfo.AdjustmentRule[] {
				TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (1, 1, 1), new DateTime (2012, 12, 31), TimeSpan.FromMinutes (23), TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 15, 59, 999), 3, 2, DayOfWeek.Tuesday), TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 15, 59, 999), 6, 2, DayOfWeek.Tuesday)),
				TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (2013, 1, 1), new DateTime (9999, 12, 31), TimeSpan.FromMinutes (48), TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 15, 59, 999), 3, 2, DayOfWeek.Tuesday), TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 15, 59, 999), 6, 2, DayOfWeek.Tuesday))
			};
			var tz5 = TimeZoneInfo.CreateCustomTimeZone ("My Zone 5", TimeSpan.FromHours (-6.75), "My Zone 5 Name", "My Zone 5 Standard Time", "My Zone 5 Daylight Time", tz5rules);
			Assert.AreEqual ("My Zone 5;-405;My Zone 5 Name;My Zone 5 Standard Time;My Zone 5 Daylight Time;[01:01:0001;12:31:2012;23;[0;02:15:59.999;3;2;2;];[0;02:15:59.999;6;2;2;];][01:01:2013;12:31:9999;48;[0;02:15:59.999;3;2;2;];[0;02:15:59.999;6;2;2;];];", tz5.ToSerializedString ());
		}

		[Test]
		public void DeserializeCustomZoneWithOddNamingAndMultipleDaylightRules ()
		{
			var rule1 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (1, 1, 1), new DateTime (2012, 12, 31), TimeSpan.FromMinutes (23), TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 15, 59, 999), 3, 2, DayOfWeek.Tuesday), TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 15, 59, 999), 6, 2, DayOfWeek.Tuesday));
			var rule2 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule (new DateTime (2013, 1, 1), new DateTime (9999, 12, 31), TimeSpan.FromMinutes (48), TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 15, 59, 999), 3, 2, DayOfWeek.Tuesday), TimeZoneInfo.TransitionTime.CreateFloatingDateRule (new DateTime (1, 1, 1, 2, 15, 59, 999), 6, 2, DayOfWeek.Tuesday));

			var tz1 = TimeZoneInfo.FromSerializedString ("My\\; Zone 5;-405;My Zone\\; 5 Name;My Zone 5\\; Standard Time;My Zone 5 Daylight\\; Time;[01:01:0001;12:31:2012;23;[0;02:15:59.999;3;2;2;];[0;02:15:59.999;6;2;2;];][01:01:2013;12:31:9999;48;[0;02:15:59.999;3;2;2;];[0;02:15:59.999;6;2;2;];];");
			Assert.AreEqual ("My; Zone 5", tz1.Id);
			Assert.AreEqual ("My Zone; 5 Name", tz1.DisplayName);
			Assert.AreEqual ("My Zone 5; Standard Time", tz1.StandardName);
			Assert.AreEqual ("My Zone 5 Daylight; Time", tz1.DaylightName);
			Assert.AreEqual (TimeSpan.FromMinutes (-405), tz1.BaseUtcOffset);
			Assert.IsTrue (tz1.SupportsDaylightSavingTime);

			var deserializedRules = tz1.GetAdjustmentRules ();
			Assert.AreEqual (2, deserializedRules.Length);
			Assert.IsFalse (deserializedRules [0].Equals (deserializedRules [1]));
			Assert.IsTrue (rule1.Equals (deserializedRules [0]));
			Assert.IsTrue (rule2.Equals (deserializedRules [1]));
		}

		[Test]
		public void DeserializeAndUseEasternTimeZone ()
		{
			var et = TimeZoneInfo.FromSerializedString (@"Eastern Standard Time;-300;(UTC-05:00) Eastern Time (US & Canada);Eastern Standard Time;Eastern Daylight Time;[01:01:0001;12:31:2006;60;[0;02:00:00;4;1;0;];[0;02:00:00;10;5;0;];][01:01:2007;12:31:9999;60;[0;02:00:00;3;2;0;];[0;02:00:00;11;1;0;];];");
			var testDate = new DateTime (2014, 8, 1, 6, 0, 0, DateTimeKind.Unspecified);
			Assert.AreEqual (TimeSpan.FromHours (-4), et.GetUtcOffset (testDate));
		}
	}
}
