using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using NUnit.Framework;
using Microsoft.Build.Evaluation;
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
			Assert.AreEqual (Environment.CurrentDirectory, root.DirectoryPath, "#3");
		}

		[Test]
		public void FullPathSetter ()
		{
			var root = ProjectRootElement.Create ();
			root.FullPath = "test" + Path.DirectorySeparatorChar + "foo.xml";

			var full = Path.Combine (Environment.CurrentDirectory, "test", "foo.xml");
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
		public void InvalidProject ()
		{
			try {
				ProjectRootElement.Create (XmlReader.Create (new StringReader (" <root/>")));
				Assert.Fail ("should throw InvalidProjectFileException");
			} catch (InvalidProjectFileException ex) {
				Assert.AreEqual (1, ex.LineNumber, "#1");
				// it is very interesting, but unlike XmlReader.LinePosition it returns the position for '<'.
				Assert.AreEqual (2, ex.ColumnNumber, "#2");
			}
		}

		[Test]
		public void CreateWithXmlLoads ()
		{
			string project_xml_1 = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'><ItemGroup><None Include='bar.txt' /></ItemGroup></Project>";
			var xml = XmlReader.Create (new StringReader (project_xml_1), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
			Assert.AreEqual (1, root.Items.Count, "#1");
		}
		
		[Test]
		public void ToolsVersionDefault ()
		{
			var g = ProjectCollection.GlobalProjectCollection;
			var root = ProjectRootElement.Create ();
			// this will be wrong in the future version, but since .NET 4.5 still expects "4.0" we can't say for sure.
			Assert.AreEqual ("4.0", root.ToolsVersion, "#1");
		}
		
		[Test]
		public void ToolsVersionIsEmptyWithXml ()
		{
			string project_xml_1 = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'><ItemGroup><None Include='bar.txt' /></ItemGroup></Project>";
			var xml = XmlReader.Create (new StringReader (project_xml_1), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
			Assert.AreEqual (string.Empty, root.ToolsVersion, "#1");
		}

		[Test]
		public void LoadUnknownChild ()
		{
			string project_xml_1 = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'><Unknown /></Project>";
			var xml = XmlReader.Create (new StringReader (project_xml_1), null, "file://localhost/foo.xml");
			try {
				ProjectRootElement.Create (xml);
				Assert.Fail ("should throw InvalidProjectFileException");
			} catch (InvalidProjectFileException ex) {
				Assert.AreEqual (1, ex.LineNumber, "#1");
				// unlike unexpected element case which returned the position for '<', it does return the name start char...
				Assert.AreEqual (70, ex.ColumnNumber, "#2");
			}
		}

		[Test]
		public void LoadUnregisteredItem ()
		{
			string project_xml_1 = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'><ItemGroup><UnregisteredItem Include='bar.txt' /></ItemGroup></Project>";
			var xml = XmlReader.Create (new StringReader (project_xml_1), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
			Assert.AreEqual (1, root.Items.Count, "#1");
		}
		
		[Test]
		public void LoadInvalidProjectForBadCondition ()
		{
			string xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <Foo>What are 'ESCAPE' &amp; ""EVALUATE"" ? $ # % ^</Foo>
    <!-- Note that this contains invalid Condition expression. Project.ctor() fails to load. -->
    <Baz Condition=""$(Void)=="">$(FOO)</Baz>
  </PropertyGroup>
</Project>";
			var path = "file://localhost/foo.xml";
			var reader = XmlReader.Create (new StringReader (xml), null, path);
			var root = ProjectRootElement.Create (reader);
			Assert.AreEqual (2, root.Properties.Count, "#1");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void LoadInvalidProjectGroupInProjectGroup ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
  <PropertyGroup>
    <Foo>Bar</Foo>
    <PropertyGroup>
      <X>x</X>
      <Y>y</Y>
      <Z>z</Z>
    </PropertyGroup>
  </PropertyGroup>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            ProjectRootElement.Create (xml);
		}
		
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void LoadInvalidItemGroupInProjectGroup ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
  <PropertyGroup>
    <Foo>Bar</Foo>
    <ItemGroup/>
  </PropertyGroup>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            ProjectRootElement.Create (xml);
		}
		
		[Test]
		public void ChildAndAllChildren ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
  <PropertyGroup>
    <Foo>Bar</Foo>
    <Item/>
  </PropertyGroup>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			Assert.AreEqual (2, root.Children.Count, "#1");
			// AllChildren expands descendants
			Assert.AreEqual (4, root.AllChildren.Count (), "#2");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SaveWithoutFullPath ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
			root.Save ();
		}
		
		[Test]
		public void SaveToWriter ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
			var sw = new StringWriter ();
			root.Save (sw);
			// CRLF? mmm, k...
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" + project_xml.Replace ('\'', '"'), sw.ToString (), "#1");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void ImportsMissingProject ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='' />
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			ProjectRootElement.Create (xml);
		}
	}
}
