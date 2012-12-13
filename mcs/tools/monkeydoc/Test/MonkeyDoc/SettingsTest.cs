using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using MonkeyDoc;

namespace MonoTests.MonkeyDoc
{
	[TestFixture]
	public class SettingsTest
	{
		[Test]
		public void DocPathConfigTest ()
		{
			// the docPath variable is the only one we know for sure should exist
			Assert.IsNotNull (Settings.Get ("docPath"));
			Assert.IsNotEmpty (Settings.Get ("docPath"));
		}
	}
}
