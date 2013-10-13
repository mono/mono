using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class BuildParametersTest
	{
		[Test]
		public void GetToolset ()
		{
			var bp = new BuildParameters (ProjectCollection.GlobalProjectCollection);
			Assert.IsNull (bp.GetToolset ("0.1"), "#1");
			var ts = bp.GetToolset ("2.0");
			// They are equal
			Assert.AreEqual (ProjectCollection.GlobalProjectCollection.Toolsets.First (t => t.ToolsVersion == "2.0"), ts, "#2");

			bp = new BuildParameters ();
			Assert.IsNull (bp.GetToolset ("0.1"), "#1");
			ts = bp.GetToolset ("2.0");
			// They are NOT equal, because ProjectCollection seems to be different.
			Assert.AreNotEqual (ProjectCollection.GlobalProjectCollection.Toolsets.First (t => t.ToolsVersion == "2.0"), ts, "#2");			
		}
	}
}

