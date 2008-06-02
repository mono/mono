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
		[Ignore ("it won't succeed if mono is not installed")]
		public void TestGac1 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Reference Include='System' />
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
						</ResolveAssemblyReference>
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsTrue (project.Build ("A"), "A1");
			big = project.GetEvaluatedItemsByName ("ResolvedFiles");
			Assert.AreEqual (1, big.Count, "A2");
			Assert.IsTrue (big [0].Include.EndsWith (".dll"), "A3");
		}

		[Test]
		[Ignore ("it won't succeed if mono is not installed")]
		public void TestHintPath1 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Reference Include='test'>
							<HintPath>Test\resources\test.dll</HintPath>
						</Reference>
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
						</ResolveAssemblyReference>
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsTrue (project.Build ("A"), "A1");
			big = project.GetEvaluatedItemsByName ("ResolvedFiles");
			Assert.AreEqual (1, big.Count, "A2");
			Assert.IsTrue (big [0].Include.EndsWith (".dll"), "A3");
		}
	}
} 
