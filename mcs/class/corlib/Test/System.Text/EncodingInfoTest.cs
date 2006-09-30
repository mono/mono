//
// EncodingInfoTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2006 Novell, Inc.
// 

#if NET_2_0

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MonoTests.System.Text
{
	[TestFixture]
	public class EncodingInfoTest
	{
		[Test]
		// The purpose of this test is to make sure that
		// new encodings added to I18N are also listed in the
		// returned array from Encoding.GetEncodings() so that
		// we can make sure to put additional encodings into
		// Encoding.GetEncodings() code.
		public void EncodingGetEncodingsReturnsAll ()
		{
			// Make sure that those I18N assemblies are loaded.
			string basePath = Assembly.GetAssembly (typeof (int)).CodeBase;
			basePath = basePath.Substring (0, basePath.LastIndexOf ('/'));
			Assert.IsNotNull (Assembly.LoadFrom (basePath + "/I18N.West.dll"), "West");
			Assert.IsNotNull (Assembly.LoadFrom (basePath + "/I18N.CJK.dll"), "CJK");
			Assert.IsNotNull (Assembly.LoadFrom (basePath + "/I18N.MidEast.dll"), "MidEast");
			Assert.IsNotNull (Assembly.LoadFrom (basePath + "/I18N.Rare.dll"), "Rare");
			Assert.IsNotNull (Assembly.LoadFrom (basePath + "/I18N.Other.dll"), "Other");

			List<int> list = new List<int> ();
			for (int i = 1; i < 0x10000; i++) {
				try {
					Encoding.GetEncoding (i);
					list.Add (i);
				} catch {
				}
			}
			int [] reference = list.ToArray ();

			EncodingInfo [] infos = Encoding.GetEncodings ();
			int [] actual = new int [infos.Length];

			for (int i = 0; i < infos.Length; i++)
				actual [i] = infos [i].CodePage;

			Assert.AreEqual (reference, actual);
		}

		[Test]
		public void GetEncodingForAllInfo ()
		{
			foreach (EncodingInfo i in Encoding.GetEncodings ())
				Assert.IsNotNull (i.GetEncoding (), "codepage " + i);
		}
	}
}

#endif
