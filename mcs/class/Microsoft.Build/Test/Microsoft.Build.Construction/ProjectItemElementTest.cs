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
	public class ProjectItemElementTest
	{
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void EmptyInclude ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Include='' />
  </ItemGroup>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            ProjectRootElement.Create (xml);
		}
		
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void MissingInclude ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo />
  </ItemGroup>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            ProjectRootElement.Create (xml);
		}
	}
}

