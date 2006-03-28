//
// ProjectTest.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
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
using System.Xml;
using Microsoft.Build.BuildEngine;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class ProjectTest {

		string binPath;

		[SetUp]
		public void SetUp ()
		{
		    binPath = "binPath";
		}

		// Clones a project by reloading from original.Xml
		private Project CloneProject (Project original)
		{
			Project clone;
			
			clone = original.ParentEngine.CreateNewProject ();
			clone.LoadXml (original.Xml);

			return clone;
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException),
		@"The default XML namespace of the project must be the MSBuild XML namespace." + 
		" If the project is authored in the MSBuild 2003 format, please add " +
		"xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\" to the <Project> element. " +
		"If the project has been authored in the old 1.0 or 1.2 format, please convert it to MSBuild 2003 format.  ")]
		public void TestAssignment ()
		{
			Engine engine;
			Project project;
			string documentString =
				"<Project></Project>";
			
			engine = new Engine (binPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

		[Test]
		public void TestDefaultTargets ()
		{
			Engine engine;
			Project proj;
			Project cproj;
			string documentString = @"
			<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" DefaultTargets=""Build;Compile"">
			</Project>
			";
			
			engine = new Engine (binPath);
			proj = engine.CreateNewProject ();
			Assert.AreEqual (String.Empty, proj.FullFileName, "A1");

			proj.LoadXml (documentString);
			Assert.AreEqual (String.Empty, proj.FullFileName, "A2");
			proj.DefaultTargets = "Build";
			Assert.AreEqual ("Build", proj.DefaultTargets, "A3");
			cproj = CloneProject (proj);
			Assert.AreEqual (proj.DefaultTargets, cproj.DefaultTargets, "A4");
		}

		[Test]
		public void TestProperties ()
		{
			Engine engine = new Engine (binPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<Config>debug</Config>
						<ExpProp>$(Config)-$(Config)</ExpProp>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.AreEqual (1, proj.PropertyGroups.Count, "A1");
			Assert.AreEqual ("debug", proj.GetEvaluatedProperty ("Config"), "A2");
			Assert.AreEqual ("debug-debug", proj.GetEvaluatedProperty ("ExpProp"), "A3");
		}

		// Get all items with a specific name, separated by ;
		private string GetItems (Project proj, string name)
		{
			BuildItemGroup big = proj.GetEvaluatedItemsByName (name);
			string str = String.Empty;
			if (big == null) {
				return str;
			}
			foreach (BuildItem bi in big) {
				if (str == String.Empty) {
					str = bi.FinalItemSpec;
				} else {
					str += ";" + bi.FinalItemSpec;
				}
			}
			return str;
		}

		[Test]
		public void TestItems ()
		{
			Engine engine = new Engine (binPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item0 Include=""A"" />
						<Item1 Include=""A;B;C"" />
						<Item2 Include=""@(Item1);A;D"" />
						<Item3 Include=""@(Item2)"" Exclude=""A"" />
						<Item4 Include=""@(Item1);Q"" Exclude=""@(Item2)"" />
						<Item5 Include=""@(Item1)"" Exclude=""@(Item2)"" />
						<Item6 Include=""@(Item2)"" Exclude=""@(Item1)"" />

						<ItemOrig Include=""A/B.txt;A/C.txt;B/B.zip;B/C.zip"" />
						<ItemT1 Include=""@(Item1->'%(Identity)')"" />
						<ItemT2 Include=""@(Item1->'%(Identity)%(Identity)')"" />
						<ItemT3 Include=""@(Item1->'(-%(Identity)-)')"" />
						<ItemT4 Include=""@(ItemOrig->'%(Extension)')"" />
						<ItemT5 Include=""@(ItemOrig->'%(Filename)/%(Extension)')"" />
						<ItemT6 Include=""@(ItemOrig->'%(RelativeDir)/X/%(Filename)')"" />
						
						<ItemS1 Include=""@(Item1,'-')"" />
						<ItemS2 Include=""@(Item1,'xx')"" />
						<ItemS3 Include=""@(Item1, '-')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.AreEqual ("A", GetItems (proj, "Item0"), "Item0");
			Assert.AreEqual ("A;B;C", GetItems (proj, "Item1"), "Item1");
			Assert.AreEqual ("A;B;C;A;D", GetItems (proj, "Item2"), "Item2");
			Assert.AreEqual ("B;C;D", GetItems (proj, "Item3"), "Item3");
			Assert.AreEqual ("Q", GetItems (proj, "Item4"), "Item4");
			Assert.AreEqual ("", GetItems (proj, "Item5"), "Item5");
			Assert.AreEqual ("D", GetItems (proj, "Item6"), "Item6");

			Assert.AreEqual ("A;B;C", GetItems (proj, "ItemT1"), "ItemT1");
			Assert.AreEqual ("AA;BB;CC", GetItems (proj, "ItemT2"), "ItemT2");
			Assert.AreEqual ("(-A-);(-B-);(-C-)", GetItems (proj, "ItemT3"), "ItemT3");
			Assert.AreEqual (".txt;.txt;.zip;.zip", GetItems (proj, "ItemT4"), "ItemT4");
			Assert.AreEqual ("B/.txt;C/.txt;B/.zip;C/.zip", GetItems (proj, "ItemT5"), "ItemT5");
			Assert.AreEqual ("A/X/B;A/X/C;B/X/B;B/X/C", GetItems (proj, "ItemT6"), "ItemT6");

			Assert.AreEqual ("A-B-C", GetItems (proj, "ItemS1"), "ItemS1");
			Assert.AreEqual ("AxxBxxC", GetItems (proj, "ItemS2"), "ItemS2");
			// Will fail.
			Assert.AreEqual ("A-B-C", GetItems (proj, "ItemS3"), "ItemS3");
		}
	}
}
