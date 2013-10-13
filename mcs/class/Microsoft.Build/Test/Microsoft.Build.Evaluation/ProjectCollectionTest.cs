using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Evaluation
{
	[TestFixture]
	public class ProjectCollectionTest
	{
		public void GlobalProperties ()
		{
			var g = ProjectCollection.GlobalProjectCollection;
			Assert.AreEqual (0, g.GlobalProperties.Count, "#1");
			Assert.IsTrue (g.GlobalProperties.IsReadOnly, "#2");
		}
	}
}

