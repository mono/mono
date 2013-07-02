using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Monodoc;

namespace MonoTests.Monodoc
{
	[TestFixture]
	public class SettingsTest
	{
		[Test]
		public void DocPathConfigTest ()
		{
			// the docPath variable is the only one we know for sure should exist
			Assert.IsNotNull (Config.Get ("docPath"));
			Assert.IsNotEmpty (Config.Get ("docPath"));
		}
	}
}
