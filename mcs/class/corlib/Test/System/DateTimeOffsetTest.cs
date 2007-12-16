//
// DateTimeOffsetTest.cs - NUnit Test Cases for the System.DateTimeOffset struct
//
// Authors:
//	Stephane Delcroix  (sdelcroix@novell.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//

#if NET_2_0
using NUnit.Framework;
using System;

namespace MonoTests.System {

	[TestFixture]
	public class DateTimeOffsetTest
	{
		//ctor exception checking...
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UtcWithWrongOffset ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.Now, DateTimeKind.Utc);
			TimeSpan offset = new TimeSpan (2, 0, 0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LocalWithWrongOffset ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.Now, DateTimeKind.Local);
			TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset (dt) + new TimeSpan (1,0,0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void OffsetNotInWholeminutes ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.Now, DateTimeKind.Unspecified);
			TimeSpan offset = new TimeSpan (1,0,59);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void OffsetOutOfRange1 ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.Now, DateTimeKind.Unspecified);
			TimeSpan offset = new TimeSpan (14, 1, 0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void OffsetOutOfRange2 ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.Now, DateTimeKind.Unspecified);
			TimeSpan offset = new TimeSpan (-14, -1, 0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void UtcDateTimeOutOfRange1 ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.MinValue, DateTimeKind.Unspecified);
			TimeSpan offset = new TimeSpan (1, 0, 0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void UtcDateTimeOutOfRange2 ()
		{
			DateTime dt = DateTime.SpecifyKind (DateTime.MaxValue, DateTimeKind.Unspecified);
			TimeSpan offset = new TimeSpan (-1, 0, 0);
			new DateTimeOffset (dt, offset);
		}

		[Test]
		public void CompareTwoDateInDiffTZ ()
		{
			DateTimeOffset dt1 = new DateTimeOffset (2007, 12, 16, 15, 06, 00, new TimeSpan (1, 0, 0));
			DateTimeOffset dt2 = new DateTimeOffset (2007, 12, 16, 9, 06, 00, new TimeSpan (-5, 0, 0));
			DateTimeOffset dt3 = new DateTimeOffset (2007, 12, 16, 14, 06, 00, new TimeSpan (1, 0, 0));
			Assert.IsTrue (dt1.CompareTo (dt2) == 0);
			Assert.IsTrue (DateTimeOffset.Compare (dt1, dt2) == 0);
			Assert.IsTrue (dt1 == dt2);
			Assert.IsTrue (dt1.Equals (dt2));
			Assert.IsFalse (dt1 == dt3);
			Assert.IsTrue (dt1 != dt3);
			Assert.IsFalse (dt1.EqualsExact (dt2));
			Assert.IsTrue (dt1.CompareTo (dt3) > 0);
		}
	}
}
#endif

