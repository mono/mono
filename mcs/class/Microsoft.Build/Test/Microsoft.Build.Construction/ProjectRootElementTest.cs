using System;
using System.IO;
using System.Xml;
using Microsoft.Build.Construction;
using NUnit.Framework;
using Microsoft.Build.Exceptions;

namespace MonoTests.Microsoft.Build.Construction
{
	[TestFixture]
	public class ProjectRootElementTest
	{
		const string empty_project_xml = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		[Category ("NotWorking")] // URL is constructed for ElementLocation, which we don't support yet.
		public void CreateExpectsAbsoluteUri ()
		{
			var xml = XmlReader.Create (new StringReader (empty_project_xml), null, "foo.xml");
			ProjectRootElement.Create (xml);
		}

		[Test]
		public void CreateAndPaths ()
		{
			Assert.IsNull (ProjectRootElement.Create ().FullPath, "#1");
			var xml = XmlReader.Create (new StringReader (empty_project_xml), null, "file:///foo.xml");
			// This creator does not fill FullPath...
			var root = ProjectRootElement.Create (xml);
			Assert.IsNull (root.FullPath, "#2");
			Assert.AreEqual (Path.GetDirectoryName (new Uri (GetType ().Assembly.CodeBase).LocalPath), root.DirectoryPath, "#3");
		}

		[Test]
		public void FullPathSetter ()
		{
			var root = ProjectRootElement.Create ();
			root.FullPath = "test" + Path.DirectorySeparatorChar + "foo.xml";
			var full = Path.Combine (Path.GetDirectoryName (new Uri (GetType ().Assembly.CodeBase).LocalPath), "test", "foo.xml");
			Assert.AreEqual (full, root.FullPath, "#1");
			Assert.AreEqual (Path.GetDirectoryName (full), root.DirectoryPath, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FullPathSetNull ()
		{
			ProjectRootElement.Create ().FullPath = null;
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void InvalidProject ()
		{
			ProjectRootElement.Create (XmlReader.Create (new StringReader ("<root/>")));
		}
	}
}

