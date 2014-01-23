//
// ResolveAssemblyReferenceTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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

using System;
using System.Collections;
using System.Linq;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Tasks {
	
	[TestFixture]
	public class ResolveAssemblyReferenceTest {
		
		Engine engine;
		Project project;
		BuildItemGroup big;

		[Test]
		public void TestGac1 ()
		{
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (ResolveAssembly ("System", null));
			
			Assert.IsTrue (project.Build ("A"), "A1");
			big = project.GetEvaluatedItemsByName ("ResolvedFiles");
			Assert.AreEqual (1, big.Count, "A2");
			Assert.IsTrue (big [0].Include.EndsWith (".dll"), "A3");
		}
		
		[Test]
		public void TestHintPath1 ()
		{
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (ResolveAssembly (null, @"Test\resources\test.dll"));
			
			Assert.IsTrue (project.Build ("A"), "A1");
			big = project.GetEvaluatedItemsByName ("ResolvedFiles");
			Assert.AreEqual (1, big.Count, "A2");
			Assert.IsTrue (big [0].Include.EndsWith (".dll"), "A3");
		}
		
		[Test, Category("NotWorking")]
		public void ResolveBinary_FancyStuff ()
		{
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (ResolveAssembly (null, @"Test\resources\binary\FancyStuff.dll"));
			
			Assert.IsTrue (project.Build ("A"), "A1");
			big = project.GetEvaluatedItemsByName ("ResolvedFiles");
			Assert.AreEqual (1, big.Count, "A2");
			Assert.IsTrue (big[0].Include.EndsWith ("FancyStuff.dll"), "A3");
			
			big = project.GetEvaluatedItemsByName ("ResolvedDependencyFiles");
			Assert.AreEqual (1, big.Count, "A4");
			Assert.IsTrue (big.Cast<BuildItem> ().Any (item => item.Include.EndsWith ("SimpleWrite.dll")), "A5");
		}
		
		[Test, Category("NotWorking")]
		public void ResolveBinary_SimpleWrite ()
		{
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (ResolveAssembly (null, @"Test\resources\binary\SimpleWrite.dll"));
			
			Assert.IsTrue (project.Build ("A"), "A1");
			big = project.GetEvaluatedItemsByName ("ResolvedFiles");
			Assert.AreEqual (1, big.Count, "A2");
			Assert.IsTrue (big[0].Include.EndsWith ("SimpleWrite.dll"), "A3");
			
			big = project.GetEvaluatedItemsByName ("ResolvedDependencyFiles");
			Assert.AreEqual (0, big.Count, "A4");
		}
		
		[Test, Category("NotWorking")]
		public void ResolveBinary_Testing ()
		{
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (ResolveAssembly (null, @"Test\resources\binary\Testing.dll"));
			
			Assert.IsTrue (project.Build ("A"), "A1");
			big = project.GetEvaluatedItemsByName ("ResolvedFiles");
			Assert.AreEqual (1, big.Count, "A2");
			Assert.IsTrue (big[0].Include.EndsWith ("Testing.dll"), "A3");
			
			big = project.GetEvaluatedItemsByName ("ResolvedDependencyFiles");
			Assert.AreEqual (0, big.Count, "A4");
		}
		
		[Test, Category("NotWorking")]
		public void ResolveBinary_XbuildReferenceBugTest ()
		{
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (ResolveAssembly (null, @"Test\resources\binary\XbuildReferenceBugTest.exe"));
			
			Assert.IsTrue (project.Build ("A"), "A1");
			big = project.GetEvaluatedItemsByName ("ResolvedFiles");
			Assert.AreEqual (1, big.Count, "A2");
			Assert.IsTrue (big[0].Include.EndsWith ("XbuildReferenceBugTest.exe"), "A3");
			
			big = project.GetEvaluatedItemsByName ("ResolvedDependencyFiles");
			Assert.AreEqual (3, big.Count, "A4");
			Assert.IsTrue (big.Cast<BuildItem> ().Any (item => item.Include.EndsWith ("SimpleWrite.dll")), "A5");
			Assert.IsTrue (big.Cast<BuildItem> ().Any (item => item.Include.EndsWith ("FancyStuff.dll")), "A6");
			Assert.IsTrue (big.Cast<BuildItem> ().Any (item => item.Include.EndsWith ("Testing.dll")), "A7");
		}

		[Test, Category("NotWorking")]
		public void ResolveBinary_FancyStuffAndXbuild ()
		{
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (ResolveAssembly (null, @"Test\resources\binary\FancyStuff.dll", @"Test\resources\binary\XbuildReferenceBugTest.exe"));
			
			Assert.IsTrue (project.Build ("A"), "A1");
			big = project.GetEvaluatedItemsByName ("ResolvedFiles");
			Assert.AreEqual (2, big.Count, "A2");
			Assert.IsTrue (big[0].Include.EndsWith ("FancyStuff.dll"), "A3");
			Assert.IsTrue (big[1].Include.EndsWith ("XbuildReferenceBugTest.exe"), "A3");
			
			big = project.GetEvaluatedItemsByName ("ResolvedDependencyFiles");
			Assert.AreEqual (2, big.Count, "A4");
			Assert.IsTrue (big.Cast<BuildItem> ().Any (item => item.Include.EndsWith ("SimpleWrite.dll")), "A5");
			Assert.IsTrue (big.Cast<BuildItem> ().Any (item => item.Include.EndsWith ("Testing.dll")), "A6");
		}

		[Test, Category("NotWorking")]
		public void ResolveBinary_SameAssemblyTwice ()
		{
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (ResolveAssembly (null, @"Test\resources\binary\XbuildReferenceBugTest.exe", @"Test\resources\binary\XbuildReferenceBugTest2.exe"));

			Assert.IsTrue (project.Build ("A"), "A1");
			big = project.GetEvaluatedItemsByName ("ResolvedFiles");
			Assert.AreEqual (2, big.Count, "A2");
			Assert.IsTrue (big[0].Include.EndsWith ("XbuildReferenceBugTest.exe"), "A3");
			Assert.IsTrue (big[1].Include.EndsWith ("XbuildReferenceBugTest2.exe"), "A3b");
			
			big = project.GetEvaluatedItemsByName ("ResolvedDependencyFiles");
			Assert.AreEqual (3, big.Count, "A4");
			Assert.IsTrue (big.Cast<BuildItem> ().Any (item => item.Include.EndsWith ("SimpleWrite.dll")), "A5");
			Assert.IsTrue (big.Cast<BuildItem> ().Any (item => item.Include.EndsWith ("FancyStuff.dll")), "A6");
			Assert.IsTrue (big.Cast<BuildItem> ().Any (item => item.Include.EndsWith ("Testing.dll")), "A7");
		}

		string ResolveAssembly (string gacAssembly, params string[] hintPaths)
		{
			string reference = "";
			if (string.IsNullOrEmpty (gacAssembly)) {
				foreach (var hintPath in hintPaths) {
					reference += string.Format (
						@"<Reference Include='{0}'>
								<HintPath>{1}</HintPath>
							</Reference>", System.IO.Path.GetFileNameWithoutExtension (hintPath), hintPath);
				}
			} else {
				reference += string.Format (
					@"<Reference Include='{0}'/>", gacAssembly);
			}
			
			return @"
                  <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
					" + reference + @"	
					</ItemGroup>
					<PropertyGroup>
						<SearchPaths>
							{CandidateAssemblyFiles};
							$(ReferencePath);
							{HintPathFromItem};
							{TargetFrameworkDirectory};
							{AssemblyFolders};
							{GAC};
							{RawFileName};
							$(OutputPath)
						</SearchPaths>
					</PropertyGroup>
					<Target Name='A'>
						<ResolveAssemblyReference
							Assemblies='@(Reference)'
							SearchPaths='$(SearchPaths)'
						>
							<Output TaskParameter='ResolvedFiles' ItemName='ResolvedFiles'/>
							<Output TaskParameter='ResolvedDependencyFiles' ItemName='ResolvedDependencyFiles'/>
						</ResolveAssemblyReference>
					</Target>
				</Project>
			";
		}
	}
} 
