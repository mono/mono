//
// ResourceWriterTest.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Resources;

using NUnit.Framework;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResourceWriterTest
	{
#if NET_2_0
		[Test]
		public void Bug81759 ()
		{
			MemoryStream ms = new MemoryStream ();
			using (ResourceReader xr = new ResourceReader (
				"Test/resources/bug81759.resources")) {
				ResourceWriter rw = new ResourceWriter (ms);
				foreach (DictionaryEntry de in xr)
					rw.AddResource ((string) de.Key, de.Value);
				rw.Close ();
			}
			ResourceReader rr = new ResourceReader (new MemoryStream (ms.ToArray ()));
			foreach (DictionaryEntry de in rr) {
				Assert.AreEqual ("imageList.ImageSize", de.Key as string, "#1");
				Assert.AreEqual ("Size", de.Value.GetType ().Name, "#2");
			}
		}
#endif

		[Test] // bug #82566
#if NET_2_0
		[Category ("NotWorking")]
#endif
		public void WriteEnum ()
		{
			MemoryStream ms = new MemoryStream ();

			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Targets", AttributeTargets.Assembly);
			writer.Generate ();

			ms.Position = 0;

			bool found = false;

			ResourceReader reader = new ResourceReader (ms);
			foreach (DictionaryEntry de in reader) {
				string name = de.Key as string;
				Assert.IsNotNull (name, "#1");
				Assert.AreEqual ("Targets", name, "#2");
				Assert.IsNotNull (de.Value, "#3");
				Assert.AreEqual (AttributeTargets.Assembly, de.Value, "#4");
				found = true;
			}

			Assert.IsTrue (found, "#5");

			writer.Dispose ();
		}

	}
}
