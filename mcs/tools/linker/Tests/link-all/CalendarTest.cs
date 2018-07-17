// Copyright 2013 Xamarin Inc. All rights reserved.

using System;
using System.Globalization;
#if XAMCORE_2_0
using Foundation;
#else
#if __MONODROID__
using Android.Runtime;
#else
#if MONOTOUCH
using MonoTouch.Foundation;
#endif
#endif
#endif
using NUnit.Framework;

namespace LinkAll.Calendars {

	[TestFixture]
	// we want the tests to be available because we use the linker
	[Preserve (AllMembers = true)]
	public class CalendarTest {

		// application *MUST* be build with I18N.MidEast and I18N.Other (Thai)

#if MOBILE // TODO: fails on Mono Desktop, XI has explicit linker step for preserving these calendars
		[Test]
		public void UmAlQura ()
		{
			var ci = CultureInfo.GetCultureInfo ("ar");
			Assert.That (ci.Calendar.ToString (), Is.EqualTo ("System.Globalization.UmAlQuraCalendar"), "Calendar");
		}

		[Test]
		public void Hijri ()
		{
			var ci = CultureInfo.GetCultureInfo ("ps");
			Assert.That (ci.Calendar.ToString (), Is.EqualTo ("System.Globalization.HijriCalendar"), "Calendar");
		}

		[Test]
		public void ThaiBuddhist ()
		{
			var ci = CultureInfo.GetCultureInfo ("th");
			Assert.That (ci.Calendar.ToString (), Is.EqualTo ("System.Globalization.ThaiBuddhistCalendar"), "Calendar");
		}
#endif
	}
}