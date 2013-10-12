using System;
using Microsoft.Build.Evaluation;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Evaluation
{
	[TestFixture]
	public class ToolsetTest
	{
		[Test]
		public void Constructor ()
		{
			var ts = new Toolset ("4.3", "c:\\", ProjectCollection.GlobalProjectCollection, null);
			Assert.IsNotNull (ts.Properties, "#1");
			Assert.AreEqual (0, ts.Properties.Count, "#2");
#if NET_4_5
			Assert.IsNull (ts.DefaultSubToolsetVersion, "#3");
			Assert.IsNotNull (ts.SubToolsets, "#4");
			Assert.AreEqual (0, ts.SubToolsets.Count, "#5");
#endif
			Assert.AreEqual ("c:\\", ts.ToolsPath, "#6");
			Assert.AreEqual ("4.3", ts.ToolsVersion, "#7");
		}
	}
}

