//
// ProjectInstanceTest.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using NUnit.Framework;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class ProjectInstanceTest
	{
		[Test]
		public void ItemsAndProperties ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <X Condition='false' Include='bar.txt' />
    <X Include='foo.txt'>
      <M>m</M>
      <N>=</N>
    </X>
  </ItemGroup>
  <PropertyGroup>
    <P Condition='false'>void</P>
    <P Condition='true'>valid</P>
  </PropertyGroup>
</Project>";
            var xml = XmlReader.Create (new StringReader(project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new ProjectInstance (root);
            var item = proj.Items.First ();
			Assert.AreEqual ("foo.txt", item.EvaluatedInclude, "#1");
			var prop = proj.Properties.First (p => p.Name=="P");
			Assert.AreEqual ("valid", prop.EvaluatedValue, "#2");
			Assert.IsNotNull (proj.GetProperty ("MSBuildProjectDirectory"), "#3");
			Assert.AreEqual ("4.0", proj.ToolsVersion, "#4");
		}
		
		[Test]
		public void ExplicitToolsVersion ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
            var xml = XmlReader.Create (new StringReader(project_xml));
            var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root, null, "4.0", new ProjectCollection ());
			Assert.AreEqual ("4.0", proj.ToolsVersion, "#1");
		}
		
		[Test]
		public void BuildEmptyProject ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
			// This seems to do nothing and still returns true
			root.FullPath = "ProjectInstanceTest.BuildEmptyProject.1.proj";
			Assert.IsTrue (new ProjectInstance (root).Build (), "#1");
			// This seems to fail to find the appropriate target
			root.FullPath = "ProjectInstanceTest.BuildEmptyProject.2.proj";
			Assert.IsFalse (new ProjectInstance (root).Build ("Build", null), "#2");
			// Thus, this tries to build all the targets (empty) and no one failed, so returns true(!)
			root.FullPath = "ProjectInstanceTest.BuildEmptyProject.3.proj";
			Assert.IsTrue (new ProjectInstance (root).Build (new string [0], null), "#3");
			// Actially null "targets" is accepted and returns true(!!)
			root.FullPath = "ProjectInstanceTest.BuildEmptyProject.4.proj";
			Assert.IsTrue (new ProjectInstance (root).Build ((string []) null, null), "#4");
			// matching seems to be blindly done, null string also results in true(!!)
			root.FullPath = "ProjectInstanceTest.BuildEmptyProject.5.proj";
			Assert.IsTrue (new ProjectInstance (root).Build ((string) null, null), "#5");
		}
		
		[Test]
		public void DefaultTargets ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.Common.targets' />
</Project>";
			var xml = XmlReader.Create (new StringReader(project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root);
			Assert.AreEqual (1, proj.DefaultTargets.Count, "#1");
			Assert.AreEqual ("Build", proj.DefaultTargets [0], "#2");
		}
		
		[Test]
		public void DefaultTargets2 ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectTest.BuildCSharpTargetBuild.proj";
			var proj = new ProjectInstance (root);
			Assert.AreEqual (1, proj.DefaultTargets.Count, "#1");
			Assert.AreEqual ("Build", proj.DefaultTargets [0], "#2");
		}
		
		[Test]
		public void PropertyOverrides ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <X>x</X>
  </PropertyGroup>
  <PropertyGroup>
    <X>y</X>
  </PropertyGroup>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectTest.BuildCSharpTargetBuild.proj";
			var proj = new ProjectInstance (root);
			Assert.AreEqual ("y", proj.GetPropertyValue ("X"), "#1");
		}
		
		[Test]
		public void FirstUsingTaskTakesPrecedence1 ()
		{
			FirstUsingTaskTakesPrecedenceCommon (false, false);
		}
		
		[Test]
		public void FirstUsingTaskTakesPrecedence2 ()
		{
			FirstUsingTaskTakesPrecedenceCommon (true, true);
		}
		
		public void FirstUsingTaskTakesPrecedenceCommon (bool importFirst, bool buildShouldSucceed)
		{
			string thisAssembly = new Uri (GetType ().Assembly.CodeBase).LocalPath;
			Directory.CreateDirectory ("Test");
			string filename = "Test/ProjectTargetInstanceTest.FirstUsingTaskTakesPrecedence.Import.proj";
			string imported_xml = string.Format (@"<Project DefaultTargets='Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <UsingTask TaskName='MonoTests.Microsoft.Build.Execution.MyTask' AssemblyFile='{0}' />
</Project>", thisAssembly);
			string usingTask =  string.Format ("<UsingTask TaskName='MonoTests.Microsoft.Build.Execution.SubNamespace.MyTask' AssemblyFile='{0}' />", thisAssembly);
			string import = string.Format ("<Import Project='{0}' />", filename);
			string project_xml = string.Format (@"<Project DefaultTargets='Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
			{0}
			{1}
  <Target Name='Foo'>
    <MyTask />
  </Target>
</Project>", 
				importFirst ? import : usingTask, importFirst ? usingTask : import);
			try {
				File.WriteAllText (filename, imported_xml);
				var xml = XmlReader.Create (new StringReader (project_xml));
				var root = ProjectRootElement.Create (xml);
				Assert.IsTrue (root.UsingTasks.All (u => !string.IsNullOrEmpty (u.AssemblyFile)), "#1");
				Assert.IsTrue (root.UsingTasks.All (u => string.IsNullOrEmpty (u.AssemblyName)), "#2");
				root.FullPath = "ProjectTargetInstanceTest.FirstUsingTaskTakesPrecedence.proj";
				var proj = new ProjectInstance (root);
				Assert.AreEqual (buildShouldSucceed, proj.Build (), "#3");
			} finally {
				File.Delete (filename);
			}
		}
		
		[Test]
		public void MissingTypeForUsingTaskStillWorks ()
		{
			string thisAssembly = new Uri (GetType ().Assembly.CodeBase).LocalPath;
			string project_xml = string.Format (@"<Project DefaultTargets='X' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <UsingTask AssemblyFile='{0}' TaskName='NonExistent' />
  <Target Name='X' />
</Project>", thisAssembly);
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectInstanceTest.MissingTypeForUsingTaskStillWorks.proj";
			var proj = new ProjectInstance (root);
			Assert.IsTrue (proj.Build (), "#1");
		}
		
		[Test]
		public void MissingTypeForUsingTaskStillWorks2 ()
		{
			string thisAssembly = new Uri (GetType ().Assembly.CodeBase).LocalPath;
			string project_xml = @"<Project DefaultTargets='X' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <UsingTask AssemblyFile='nonexistent.dll' TaskName='NonExistent' />
  <Target Name='X' />
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectInstanceTest.MissingTypeForUsingTaskStillWorks2.proj";
			var proj = new ProjectInstance (root);
			Assert.IsTrue (proj.Build (), "#1");
		}

		[Test]
		public void ExpandStringWithMetadata ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Include='xxx'><M>x</M></Foo>
    <Foo Include='yyy'><M>y</M></Foo>
  </ItemGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectInstanceTest.ExpandStringWithMetadata.proj";
			var proj = new ProjectInstance (root);
			Assert.AreEqual ("xxx;yyy", proj.ExpandString ("@(FOO)"), "#1"); // so, metadata is gone...
		}

		[Test]
		public void EvaluatePropertyWithQuotation ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Include='abc/xxx.txt' />
  </ItemGroup>
  <PropertyGroup>
    <B>foobar</B>
  </PropertyGroup>
  <Target Name='default'>
    <CreateProperty Value=""@(Foo->'%(Filename)%(Extension)')"">
      <Output TaskParameter='Value' PropertyName='P' />
    </CreateProperty>
    <CreateProperty Value='$(B)|$(P)'>
      <Output TaskParameter='Value' PropertyName='Q' />
    </CreateProperty>
  </Target>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectInstanceTest.EvaluatePropertyWithQuotation.proj";
			var proj = new ProjectInstance (root);
			proj.Build ();
			var p = proj.GetProperty ("P");
			Assert.AreEqual ("xxx.txt", p.EvaluatedValue, "#1");
			var q = proj.GetProperty ("Q");
			Assert.AreEqual ("foobar|xxx.txt", q.EvaluatedValue, "#2");
		}

		[Test]
		public void Choose ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Choose>
    <When Condition="" '$(DebugSymbols)' != '' "">
      <PropertyGroup>
        <DebugXXX>True</DebugXXX>
      </PropertyGroup>
    </When>
    <Otherwise>
      <PropertyGroup>
        <DebugXXX>False</DebugXXX>
      </PropertyGroup>
    </Otherwise>
  </Choose>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectInstanceTest.Choose.proj";
			var proj = new ProjectInstance (root);
			var p = proj.GetProperty ("DebugXXX");
			Assert.IsNotNull (p, "#1");
			Assert.AreEqual ("False", p.EvaluatedValue, "#2");
		}

		[Test]
		public void ConditionalExpression ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<PropertyGroup>
		<NoCompilerStandardLib>true</NoCompilerStandardLib>
                <ResolveAssemblyReferencesDependsOn>$(ResolveAssemblyReferencesDependsOn);_AddCorlibReference</ResolveAssemblyReferencesDependsOn>
        </PropertyGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectInstanceTest.ConditionalExpression.proj";
			var proj = new ProjectInstance (root);
			var p = proj.GetProperty ("ResolveAssemblyReferencesDependsOn");
			Assert.IsNotNull (p, "#1");
			Assert.AreEqual (";_AddCorlibReference", p.EvaluatedValue, "#2");
		}

		[Test]
		public void ItemsAndPostEvaluationCondition ()
		{
			// target-assigned property X is not considered when evaluating condition for C.
			string project_xml = @"<Project DefaultTargets='X;Y' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<ItemGroup>
		<A Include='foo.txt' />
		<B Condition='False' Include='bar.txt' />
		<C Condition=""'$(X)'=='True'"" Include='baz.txt' />
        </ItemGroup>
        <Target Name='X'>
    		<CreateProperty Value='True'>
	    		<Output TaskParameter='Value' PropertyName='X' />
		    </CreateProperty>
        </Target>
        <Target Name='Y'>
		<Error Condition=""'@(C)'==''"" Text='missing C. X is $(X)' />
        </Target>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectInstanceTest.ItemsAndPostEvaluationCondition.proj";
			var proj = new ProjectInstance (root);
			Assert.AreEqual (1, proj.Items.Count, "Count1");
			Assert.IsFalse (proj.Build (), "Build");
			Assert.AreEqual (1, proj.Items.Count, "Count2");
		}

		[Test]
		[Category ("NotWorking")] // until we figure out why it fails on wrench.
		public void ItemsInTargets ()
		{
			string project_xml = @"<Project DefaultTargets='Default' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<Target Name='Default'>
		<PropertyGroup>
			<_ExplicitMSCorlibPath>$([Microsoft.Build.Utilities.ToolLocationHelper]::GetPathToStandardLibraries ('$(TargetFrameworkIdentifier)', '$(TargetFrameworkVersion)', '$(TargetFrameworkProfile)'))\mscorlib.dll</_ExplicitMSCorlibPath>
		</PropertyGroup>
		<ItemGroup>
			<_ExplicitReference
				Include='$(_ExplicitMSCorlibPath)'
				Condition='Exists($(_ExplicitMSCorlibPath))'>
				<Private>false</Private>
			</_ExplicitReference>
		</ItemGroup>
	</Target>
	<Import Project='$(MSBuildBinPath)\\Microsoft.CSharp.targets' />
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectInstanceTest.ConditionalExpression.proj";
			var proj = new ProjectInstance (root, null, "4.0", ProjectCollection.GlobalProjectCollection);
			proj.Build ();
			// make sure the property value expansion is done successfully.
			Assert.IsTrue (!string.IsNullOrEmpty (proj.GetPropertyValue ("_ExplicitMSCorlibPath")), "premise: propertyValue by ToolLocationHelper func call");
			var items = proj.GetItems ("_ExplicitReference");
			// make sure items are stored after build.
			Assert.IsTrue (items.Any (), "items.Any");
			Assert.IsTrue (!string.IsNullOrEmpty (items.First ().EvaluatedInclude), "item.EvaluatedInclude");
		}

		[Test]
		[Category ("NotWorking")]
		public void ConditionalCyclicDependence ()
		{
			string project_xml = @"<Project DefaultTargets='Build' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<PropertyGroup>
		<C>False</C>
	</PropertyGroup>
	<Target Name='Build' DependsOnTargets='ResolveReferences' />
	<Target Name='Build2' DependsOnTargets='Bar' />
	<Target Name='ResolveReferences' DependsOnTargets='Foo;Bar' />
	<Target Name='Foo'>
		<CreateProperty Value='True'>
			<Output TaskParameter='Value' PropertyName='C' />
		</CreateProperty>
	</Target>
	<Target Name='Bar' Condition='!($(C))' DependsOnTargets='ResolveReferences'>
	</Target>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectInstanceTest.ConditionalCyclicDependence.proj";
			var proj = new ProjectInstance (root, null, "4.0", ProjectCollection.GlobalProjectCollection);
			Assert.IsTrue (proj.Build (), "#1");
			Assert.IsFalse (proj.Build ("Build2", new ILogger [0]), "#2");
		}
	}
	
	namespace SubNamespace
	{
		public class MyTask : Task
		{
			public override bool Execute ()
			{
				return false;
			}
		}
	}
		
	public class MyTask : Task
	{
		public override bool Execute ()
		{
			return true;
		}
	}
}

