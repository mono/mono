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
		}

		[Test]
#if NET_2_0
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void RegionByLocaleName ()
		{
			string [] names = new string [] {
				"en-US", "zh-TW"};

			foreach (string name in names)
				new RegionInfo (name);
		}
	}
}

