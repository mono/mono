//
// MonoTests.System.Web.Services.Discovery.DiscoveryClientProtocolTest.cs
//
// Author:
//   Marcos Henrich (marcos.henrich@xamarin.com)
//
// Copyright (C) Xamarin Inc. 2016
//

using NUnit.Framework;
using System;
using System.IO;
using System.Web.Services.Discovery;

namespace MonoTests.System.Web.Services.Discovery {

	[TestFixture]
	public class DiscoveryClientProtocolTest {

		[Test] // Covers #36116
		[Category ("NotWorking")]
		[Category ("InetAccess")]
		public void ReadWriteTest ()
		{
			string directory = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			Directory.CreateDirectory (directory);
			try {
				string url = "http://www.w3schools.com/WebServices/TempConvert.asmx";
				var p1 = new DiscoveryClientProtocol ();
				p1.DiscoverAny (url);
				p1.ResolveAll ();

				p1.WriteAll (directory, "Reference.map");

				var p2 = new DiscoveryClientProtocol ();
				var results = p2.ReadAll (Path.Combine (directory, "Reference.map"));

				Assert.AreEqual (2, results.Count);
				Assert.AreEqual ("TempConvert.disco", results [0].Filename);
				Assert.AreEqual ("TempConvert.wsdl", results [1].Filename);
			} finally {
				Directory.Delete (directory, true);
			}
		}
	}
}
