//
// System.Globalization.RegionInfoTest.cs
//
// Author:
// 	Atsushi Enomoto  <atsushi@ximian.com>
//
// (c) 2007 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System.IO;
using System;
using System.Globalization;
using System.Threading;

namespace MonoTests.System.Globalization
{
	[TestFixture]
	public class RegionInfoTest
	{
		[Test]
		public void RegionByName ()
		{
			string [] names = new string [] {
				"AR", "ES", "HK", "TW", "US"};

			foreach (string name in names)
				new RegionInfo (name);

		}

		[Test]
		public void RegionByWrongName ()
		{
			string [] names = new string [] {
				"en", "EN"};

			foreach (string name in names) {
				try {
					new RegionInfo (name);
					Assert.Fail ("should be invalid: " + name);
				} catch (ArgumentException) {
				}
			}

			try {
				new RegionInfo ("2342#");
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void RegionByLocaleName ()
		{
			string [] names = new string [] {
				"en-US", "zh-TW"};

			foreach (string name in names)
				new RegionInfo (name);
		}
		
		[Test]
		// This can fail on systems where CultureInfo.CurrentCulture==null
		[Category ("NotWorking")]
		public void CurrentRegion ()
		{
			Assert.IsNotNull (RegionInfo.CurrentRegion, "CurrentRegion");
		}
		
		[Test]
		public void HongKong ()
		{
			// https://bugzilla.xamarin.com/show_bug.cgi?id=3476
			RegionInfo hk = new RegionInfo ("HK");
			// subset that match in both .NET 4 (Win7) and Mono
			Assert.AreEqual (hk.CurrencyEnglishName, "Hong Kong Dollar", "CurrencyEnglishName");
			Assert.IsTrue (hk.IsMetric, "IsMetric");
			Assert.AreEqual (hk.ISOCurrencySymbol, "HKD", "ISOCurrencySymbol");
			Assert.AreEqual (hk.Name, "HK", "Name");
			Assert.AreEqual (hk.TwoLetterISORegionName, "HK", "TwoLetterISORegionName");
			// the bug messed the order leading to DisplayName used for TLA (mono returns String.Empty)
			Assert.IsTrue (hk.ThreeLetterISORegionName.Length <= 3, "ThreeLetterISORegionName");
			Assert.IsTrue (hk.ThreeLetterWindowsRegionName.Length <= 3, "ThreeLetterWindowsRegionName");
		}

		[Test]
		public void Equals ()
		{
			var a = new RegionInfo (0x414);
			var b = new RegionInfo (0x43B);
			Assert.AreEqual (a, b);
		}
	}
}