//
// CodeTaskFactoryTest.cs
//
// Author:
//   Atsushi Enomoto <atsushi@xamarin.com>
//
// Copyright (C) 2014 Xamarin Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if NET_4_0
using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using NUnit.Framework;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging;

namespace MonoTests.Microsoft.Build.Tasks
{
	[TestFixture]
	public class CodeTaskFactoryTest
	{
		[Test]
		public void EmptyFactory ()
		{
			var f = new CodeTaskFactory ();
			Assert.AreEqual ("Code Task Factory", f.FactoryName, "Name");
			Assert.IsNull (f.TaskType, "TaskType");
			Assert.IsNull (f.CreateTask (null), "CreateTask");
		}

		[Test]
		public void Hello ()
		{
			string project_xml = @"<Project ToolsVersion='4.0' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <UsingTask
    TaskName='DoNothing'
    TaskFactory='CodeTaskFactory'
    AssemblyFile='$(MSBuildToolsPath)\Microsoft.Build.Tasks.v4.0.dll' >
    <ParameterGroup />
    <Task>
      <Code Type='Fragment' Language='cs'>
<![CDATA[
// Display ""Hello, world!""
Log.LogWarning(""Hello, world!"");
]]>      </Code>
    </Task>
  </UsingTask>
  <Target Name='default'>
    <DoNothing />
  </Target>
</Project>";
			var root = ProjectRootElement.Create (XmlReader.Create (new StringReader (project_xml))); 
			root.FullPath = "CodeTaskFactoryTest.Hello.proj";
  			var project = new Project (root);
			Assert.IsTrue (project.Build (new ConsoleLogger (LoggerVerbosity.Diagnostic)), "Build");
		}

		[Test]
		public void MultipleCodeElements ()
		{
			string project_xml = @"<Project ToolsVersion='4.0' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <UsingTask
    TaskName='DoNothing'
    TaskFactory='CodeTaskFactory'
    AssemblyFile='$(MSBuildToolsPath)\Microsoft.Build.Tasks.v4.0.dll' >
    <ParameterGroup />
    <Task>
      <Code Type='Fragment' Language='cs'></Code>
      <Code Type='Fragment' Language='cs'></Code>
    </Task>
  </UsingTask>
  <Target Name='default'>
    <DoNothing />
  </Target>
</Project>";
			var root = ProjectRootElement.Create (XmlReader.Create (new StringReader (project_xml))); 
			root.FullPath = "CodeTaskFactoryTest.MultipleCodeElements.proj";
  			var project = new Project (root);
			Assert.IsFalse (project.Build (), "Build");
		}

		[Test]
		public void InvalidLanguage ()
		{
			string project_xml = @"<Project ToolsVersion='4.0' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <UsingTask
    TaskName='DoNothing'
    TaskFactory='CodeTaskFactory'
    AssemblyFile='$(MSBuildToolsPath)\Microsoft.Build.Tasks.v4.0.dll' >
    <ParameterGroup />
    <Task>
      <Code Type='Fragment' Language='ts'></Code>
    </Task>
  </UsingTask>
  <Target Name='default'>
    <DoNothing />
  </Target>
</Project>";
			var root = ProjectRootElement.Create (XmlReader.Create (new StringReader (project_xml))); 
			root.FullPath = "CodeTaskFactoryTest.InvalidLanguage.proj";
  			var project = new Project (root);
			Assert.IsFalse (project.Build (), "Build");
		}

		[Test]
		public void InvalidCSharp ()
		{
			string project_xml = @"<Project ToolsVersion='4.0' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <UsingTask
    TaskName='DoNothing'
    TaskFactory='CodeTaskFactory'
    AssemblyFile='$(MSBuildToolsPath)\Microsoft.Build.Tasks.v4.0.dll' >
    <ParameterGroup />
    <Task>
      <Code Type='Fragment' Language='cs'>
""Hello, world!""
      </Code>
    </Task>
  </UsingTask>
  <Target Name='default'>
    <DoNothing />
  </Target>
</Project>";
			var root = ProjectRootElement.Create (XmlReader.Create (new StringReader (project_xml))); 
			root.FullPath = "CodeTaskFactoryTest.InvalidCSharp.proj";
  			var project = new Project (root);
			Assert.IsFalse (project.Build (), "Build");
		}
	}
}

#endif
