//
// BuildItemTest.cs
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
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class BuildItemTest {

		BuildItem item;
	
		[Test]
		public void TestCtor1 ()
		{
			string itemName = "itemName";
			string itemInclude = "a;b;c";
	
			item = new BuildItem (itemName, itemInclude);
	
			Assert.AreEqual (itemInclude, item.FinalItemSpec, "A1");
			Assert.AreEqual (itemInclude, item.Include, "A2");
			Assert.AreEqual (String.Empty, item.Exclude, "A3");
			Assert.AreEqual (String.Empty, item.Condition, "A4");
			Assert.AreEqual (false, item.IsImported, "A5");
			Assert.AreEqual (itemName, item.Name, "A6");
		}
	
		[Test]
		public void TestCtor2 ()
		{
			string itemName = "itemName";
			string itemSpec = "a;b;c";
			// result of Utilities.Escape (itemSpec)
			string escapedInclude = "a%3bb%3bc";
			ITaskItem taskItem = new TaskItem (itemSpec);

			item = new BuildItem (itemName, taskItem);
	
			Assert.AreEqual (itemSpec, item.FinalItemSpec, "A1");
			Assert.AreEqual (escapedInclude, item.Include, "A2");
			Assert.AreEqual (String.Empty, item.Exclude, "A3");
			Assert.AreEqual (String.Empty, item.Condition, "A4");
			Assert.AreEqual (false, item.IsImported, "A5");
			Assert.AreEqual (itemName, item.Name, "A6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtor3 ()
		{
			new BuildItem (null, (string) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotDotNet")]
		public void TestCtor4 ()
		{
			new BuildItem (null, (ITaskItem) null);
		}

		[Test]
		public void TestCtor5 ()
		{
			new BuildItem (null, "something");
		}

		// Parameter "itemInclude" cannot have zero length.
		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentException))]
		public void TestCtor6 ()
		{
			new BuildItem (null, String.Empty);
		}

		[Test]
		[Category ("NotDotNet")] //IndexOutOfRange throw by MS .NET 2.0
		public void TestCtor7 ()
		{
			new BuildItem (String.Empty, "something");
		}

		[Test]
		public void TestClone1 ()
		{
			item = new BuildItem ("name", "1;2;3");
			item.SetMetadata ("a", "b");

			BuildItem item2 = item.Clone ();

			Assert.AreEqual ("1;2;3", item2.FinalItemSpec, "A1");
			Assert.AreEqual ("1;2;3", item2.Include, "A2");
			Assert.AreEqual (String.Empty, item2.Exclude, "A3");
			Assert.AreEqual (String.Empty, item2.Condition, "A4");
			Assert.AreEqual (false, item2.IsImported, "A5");
			Assert.AreEqual ("name", item2.Name, "A6");
		}

		// Cannot set a condition on an object not represented by an XML element in the project file.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestCondition1 ()
		{
			item = new BuildItem ("name", "1");
			item.Condition = "true";
		}

		[Test]
		[Ignore ("weird test need to check how project.Xml looks")]
		public void TestCondition2 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a;b' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.EvaluatedItems [0].Condition = "true";
			project.ItemGroups.CopyTo (groups, 0);
			Assert.AreEqual (String.Empty, groups [0] [0].Condition, "A1");
			Assert.AreEqual ("true", project.EvaluatedItems [0].Condition, "A2");
		}

		[Test]
		[Ignore ("weird test need to check how project.Xml looks")]
		public void TestCondition3 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a;b' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.ItemGroups.CopyTo (groups, 0);
			groups [0] [0].Condition = "true";
			Assert.AreEqual ("true", groups [0] [0].Condition, "A1");
			Assert.AreEqual ("true", project.EvaluatedItems [0].Condition, "A2");
		}

		[Test]
		public void TestCopyCustomMetadataTo1 ()
		{
			BuildItem source, destination;
			string itemName1 = "a";
			string itemName2 = "b";
			string itemInclude = "a;b;c";
			string metadataName = "name";
			string metadataValue = "value";

			source = new BuildItem (itemName1, itemInclude);
			destination = new BuildItem (itemName2, itemInclude);

			source.SetMetadata (metadataName, metadataValue);

			source.CopyCustomMetadataTo (destination);

			Assert.AreEqual (metadataValue, destination.GetMetadata (metadataName), "A1");
			Assert.AreEqual (metadataValue, destination.GetEvaluatedMetadata (metadataName), "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotDotNet")]
		public void TestCopyCustomMetadataTo2 ()
		{
			BuildItem item = new BuildItem ("name", "include");
			item.SetMetadata ("name", "value");
			
			item.CopyCustomMetadataTo (null);
		}

		// Assigning the "Exclude" attribute of a virtual item is not allowed.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestExclude1 ()
		{
			item = new BuildItem ("name", "1");
			item.Exclude = "e";
		}

		[Test]
		public void TestExclude2 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a;b' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.ItemGroups.CopyTo (groups, 0);

			Assert.AreEqual (String.Empty, groups [0] [0].Exclude, "A1");

			groups [0] [0].Exclude = "b";

			Assert.AreEqual ("b", groups [0] [0].Exclude, "A2");
			Assert.AreEqual ("a;b", groups [0] [0].Include, "A3");
		}

		[Test]
		public void TestGetMetadata1 ()
		{
			string itemName = "a";
			string itemInclude = "a;b;c";
			string metadataName = "name";
			string metadataValue = "a;b;c";

			item = new BuildItem (itemName, itemInclude);

			Assert.AreEqual (String.Empty, item.GetMetadata (metadataName), "A1");

			item.SetMetadata (metadataName, metadataValue);

			Assert.AreEqual (metadataValue, item.GetMetadata (metadataName), "A2");
			Assert.IsTrue (item.GetMetadata ("FullPath").EndsWith (Utilities.Escape (itemInclude)), "A3");
			//Assert.IsTrue (String.Empty != item.GetMetadata ("RootDir"), "A4");
			Assert.AreEqual (itemInclude, item.GetMetadata ("Filename"), "A5");
			Assert.AreEqual (String.Empty, item.GetMetadata ("Extension"), "A6");
			Assert.AreEqual (String.Empty, item.GetMetadata ("RelativeDir"), "A7");
			Assert.IsTrue (String.Empty != item.GetMetadata ("Directory"), "A8");
			Assert.AreEqual (String.Empty, item.GetMetadata ("RecursiveDir"), "A9");
			Assert.AreEqual (itemInclude, item.GetMetadata ("Identity"), "A10");
			// FIXME: test with CreatedTime
			Assert.AreEqual (String.Empty, item.GetMetadata ("ModifiedTime"), "A11");
			Assert.AreEqual (String.Empty, item.GetMetadata ("ModifiedTime"), "A12");
			Assert.AreEqual (String.Empty, item.GetMetadata ("AccessedTime"), "A13");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetMetadata2 ()
		{
			item = new BuildItem ("name", "spec");
			item.GetMetadata (null);
		}

		[Test]
		public void TestGetMetadata3 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a;b'>
							<Meta>Value</Meta>
						</A>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.ItemGroups.CopyTo (groups, 0);

			Assert.AreEqual ("Value", groups [0] [0].GetMetadata ("Meta"), "A1");
			Assert.AreEqual (String.Empty, groups [0] [0].GetMetadata ("Other"), "A2");
		}

		[Test]
		public void TestGetEvaluatedMetadata1 ()
		{
			string itemName = "a";
			string itemInclude = "a";
			string metadataName = "name";
			string metadataValue = "a;b;c";

			item = new BuildItem (itemName, itemInclude);

			Assert.AreEqual (String.Empty, item.GetEvaluatedMetadata (metadataName), "A1");

			item.SetMetadata (metadataName, metadataValue);

			Assert.AreEqual (metadataValue, item.GetEvaluatedMetadata (metadataName), "A2");
			Assert.AreEqual (itemInclude, item.GetEvaluatedMetadata ("Identity"), "A3");
		}

		[Test]
		public void TestGetEvaluatedMetadata2 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<A>A</A>
					</PropertyGroup>
					<ItemGroup>
						<A Include='a;b'>
							<Meta>Value</Meta>
							<Meta2>$(A)</Meta2>
						</A>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.ItemGroups.CopyTo (groups, 0);

			Assert.AreEqual ("Value", groups [0] [0].GetEvaluatedMetadata ("Meta"), "A1");
			Assert.AreEqual (String.Empty, groups [0] [0].GetEvaluatedMetadata ("Other"), "A2");
			Assert.AreEqual ("A", groups [0] [0].GetEvaluatedMetadata ("Meta2"), "A3");
		}

		[Test]
		public void TestHasMetadata1 ()
		{
			string itemName = "a";
			string itemInclude = "a";
			string metadataName = "name";

			item = new BuildItem (itemName, itemInclude);

			Assert.AreEqual (false, item.HasMetadata (metadataName), "A1");

			item.SetMetadata (metadataName, "value");

			Assert.AreEqual (true, item.HasMetadata (metadataName), "A2");
			Assert.IsTrue (item.HasMetadata ("FullPath"), "A3");
			Assert.IsTrue (item.HasMetadata ("RootDir"), "A4");
			Assert.IsTrue (item.HasMetadata ("Filename"), "A5");
			Assert.IsTrue (item.HasMetadata ("Extension"), "A6");
			Assert.IsTrue (item.HasMetadata ("RelativeDir"), "A7");
			Assert.IsTrue (item.HasMetadata ("Directory"), "A8");
			Assert.IsTrue (item.HasMetadata ("RecursiveDir"), "A9");
			Assert.IsTrue (item.HasMetadata ("Identity"), "A10");
			Assert.IsTrue (item.HasMetadata ("ModifiedTime"), "A11");
			Assert.IsTrue (item.HasMetadata ("CreatedTime"), "A12");
			Assert.IsTrue (item.HasMetadata ("AccessedTime"), "A13");
		}

		[Test]
		public void TestHasMetadata2 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a;b'>
							<Meta>Value</Meta>
						</A>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.ItemGroups.CopyTo (groups, 0);

			BuildItem item = groups [0] [0];

			Assert.IsFalse (item.HasMetadata ("Other"), "A1");
			Assert.IsTrue (item.HasMetadata ("Meta"), "A2");
			Assert.IsTrue (item.HasMetadata ("FullPath"), "A3");
			Assert.IsTrue (item.HasMetadata ("RootDir"), "A4");
			Assert.IsTrue (item.HasMetadata ("Filename"), "A5");
			Assert.IsTrue (item.HasMetadata ("Extension"), "A6");
			Assert.IsTrue (item.HasMetadata ("RelativeDir"), "A7");
			Assert.IsTrue (item.HasMetadata ("Directory"), "A8");
			Assert.IsTrue (item.HasMetadata ("RecursiveDir"), "A9");
			Assert.IsTrue (item.HasMetadata ("Identity"), "A10");
			Assert.IsTrue (item.HasMetadata ("ModifiedTime"), "A11");
			Assert.IsTrue (item.HasMetadata ("CreatedTime"), "A12");
			Assert.IsTrue (item.HasMetadata ("AccessedTime"), "A13");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestHasMetadata3 ()
		{
			item = new BuildItem ("name", "spec");
			item.HasMetadata (null);
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestInclude1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

		[Test]
		public void TestInclude2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual ("a", project.EvaluatedItems [0].Include, "A1");
		}

		[Test]
		public void TestInclude3 ()
		{
			BuildItem item = new BuildItem ("name", "a");
			item.Include = "b";
			Assert.AreEqual ("b", item.Include, "A1");
		}

		[Test]
		public void TestName1 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a;b' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.EvaluatedItems [0].Name = "C";

			Assert.AreEqual (2, project.EvaluatedItems.Count, "A1");
			Assert.AreEqual ("C", project.EvaluatedItems [0].Name, "A2");
			Assert.AreEqual ("A", project.EvaluatedItems [1].Name, "A3");
			project.ItemGroups.CopyTo (groups, 0);
			Assert.AreEqual (2, groups [0].Count, "A4");
			Assert.AreEqual ("C", groups [0] [0].Name, "A5");
			Assert.AreEqual ("A", groups [0] [1].Name, "A6");
		}

		[Test]
		public void TestName2 ()
		{
			BuildItem item = new BuildItem ("A", "V");
			item.Name = "B";
			Assert.AreEqual ("B", item.Name, "A1");
		}

		[Test]
		public void TestRemoveMetadata1 ()
		{
			string itemName = "a";
			string itemInclude = "a";
			string metadataName = "name";
			string metadataValue = "a;b;c";

			item = new BuildItem (itemName, itemInclude);

			item.SetMetadata (metadataName, metadataValue);

			Assert.AreEqual (true, item.HasMetadata (metadataName), "A1");

			item.RemoveMetadata (metadataName);

			Assert.AreEqual (false, item.HasMetadata (metadataName), "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveMetadata2 ()
		{
			item = new BuildItem ("name", "value");
			item.RemoveMetadata (null);
		}

		[Test]
		public void TestRemoveMetadata3 ()
		{
			item = new BuildItem ("name", "value");
			item.RemoveMetadata ("undefined_metadata");
		}

		// "Filename" is a reserved item meta-data, and cannot be modified or deleted.
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestRemoveMetadata4 ()
		{
			item = new BuildItem ("name", "value");
			item.RemoveMetadata ("Filename");
		}

		[Test]
		public void TestRemoveMetadata5 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a;b'>
							<Meta>Value</Meta>
							<Meta2>$(A)</Meta2>
						</A>
						<B Include='a'/>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.ItemGroups.CopyTo (groups, 0);

			Assert.AreEqual (3, project.EvaluatedItems.Count, "A1");

			groups [0] [0].RemoveMetadata ("Meta");
			Assert.IsFalse (groups [0] [0].HasMetadata ("Meta"), "A2");
			groups [0] [0].RemoveMetadata ("undefined_metadata");

			Assert.AreEqual (2, groups [0].Count, "A3");
			Assert.AreEqual (3, project.EvaluatedItems.Count, "A4");
		}

		[Test]
		public void TestRemoveMetadata6 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a;b;c'>
							<Meta>Value</Meta>
							<Meta2>$(A)</Meta2>
						</A>
						<B Include='a'/>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (4, project.EvaluatedItems.Count, "A1");
			project.ItemGroups.CopyTo (groups, 0);
			Assert.AreEqual (2, groups [0].Count, "A2");

			BuildItem b1 = project.EvaluatedItems [0];

			b1.RemoveMetadata ("Meta");

			Assert.AreEqual (4, project.EvaluatedItems.Count, "A3");
			project.ItemGroups.CopyTo (groups, 0);
			Assert.AreEqual (4, groups [0].Count, "A4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestSetMetadata1 ()
		{
			item = new BuildItem ("name", "include");
			item.SetMetadata (null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestSetMetadata2 ()
		{
			item = new BuildItem ("name", "include");
			item.SetMetadata ("name", null);
		}

		[Test]
		public void TestSetMetadata3 ()
		{
			item = new BuildItem ("name", "include");
			item.SetMetadata ("a", "$(A)");
			item.SetMetadata ("b", "$(A)", true);
			item.SetMetadata ("c", "$(A)", false);

			Assert.AreEqual ("$(A)", item.GetEvaluatedMetadata ("a"), "A1");
			Assert.AreEqual ("$(A)", item.GetEvaluatedMetadata ("b"), "A2");
			Assert.AreEqual ("$(A)", item.GetEvaluatedMetadata ("c"), "A3");
			Assert.AreEqual ("$(A)", item.GetMetadata ("a"), "A4");
			Assert.AreEqual (Utilities.Escape ("$(A)"), item.GetMetadata ("b"), "A5");
			Assert.AreEqual ("$(A)", item.GetMetadata ("c"), "A6");
		}

		// "Filename" is a reserved item meta-data, and cannot be modified or deleted.
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestSetMetadata4 ()
		{
			item = new BuildItem ("name", "include");
			item.SetMetadata ("Filename", "something");
		}

		[Test]
		public void TestSetMetadata5 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<A>A</A>
					</PropertyGroup>
					<ItemGroup>
						<A Include='a;b'/>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.ItemGroups.CopyTo (groups, 0);

			groups [0] [0].SetMetadata ("Meta", "$(A)");
			Assert.AreEqual (2, project.EvaluatedItems.Count, "A0");

			Assert.AreEqual (1, groups [0].Count, "A1");
			Assert.AreEqual ("$(A)", groups [0] [0].GetMetadata ("Meta"), "A2");
			Assert.AreEqual ("$(A)", project.EvaluatedItems [0].GetMetadata ("Meta"), "A3");
			Assert.AreEqual ("$(A)", project.EvaluatedItems [1].GetMetadata ("Meta"), "A4");
			Assert.AreEqual ("A", project.EvaluatedItems [0].GetEvaluatedMetadata ("Meta"), "A5");
			Assert.AreEqual ("A", project.EvaluatedItems [1].GetEvaluatedMetadata ("Meta"), "A6");
		}

		[Test]
		public void TestSetMetadata5a () {
			Engine engine;
			Project project;
			BuildItemGroup[] groups = new BuildItemGroup[1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<A>A</A>
						<C>@(D)</C>
					</PropertyGroup>
					<ItemGroup>
						<D Include='D'/>
						<C Include='$(C)'/>
						<A Include='a;b'>
							<Md>@(C)</Md>
						</A>
						<B Include='$(A)'/>
					</ItemGroup>
					<Target Name='main'>
						<Message Text=""a.md: %(A.Md)""/>
						<Message Text=""a.md: %(A.Meta)""/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger = new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);
			project.LoadXml (documentString);

			CheckMetadata (project, "A", "Md", new string [] {"@(C)", "@(C)"}, "G1");
			CheckEvaluatedMetadata (project, "A", "Md", new string[] { "D", "D" }, "G2");

			//@(B)
			Assert.AreEqual ("A", project.GetEvaluatedItemsByName ("B")[0].FinalItemSpec, "B2");

			project.ItemGroups.CopyTo (groups, 0);
			/*Broken right now:
			  CheckBuildItemGroup (groups[0], new string[] {
				"D", "D",
				"C", "$(C)",
				"A", "a;b",
				"B", "$(A)"
			}, "H1");*/

			CheckBuildItemGroup (project.GetEvaluatedItemsByName ("C"), new string[] {
				"C", "D"
			}, "H2");

			CheckBuildItemGroup (project.GetEvaluatedItemsByName ("C"), new string[] {
				"C", "D"
			}, "I");

			project.GetEvaluatedItemsByName ("A")[0].SetMetadata ("Meta", "@(B)");

			Assert.AreEqual (5, project.EvaluatedItems.Count, "A0");
			Assert.AreEqual (2, project.GetEvaluatedItemsByName ("A").Count, "A7");

			CheckMetadata (project, "A", "Meta", new string[] { "@(B)", "" }, "J");

			if (!project.Build ()) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			CheckMetadata (project, "A", "Meta", new string[] { "@(B)", "" }, "K1");
			CheckEvaluatedMetadata (project, "A", "Meta", new string[] { "", "" }, "K2");

			logger.CheckLoggedMessageHead ("a.md: D", "E10");
			logger.CheckLoggedMessageHead ("a.md: ", "E11");
			Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected messages left");
		}

		[Test]
		public void TestSetMetadata6 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a;b;c'/>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.EvaluatedItems [0].SetMetadata ("Meta", "Value");
			//NOTE: this triggers reevaluation
			Assert.AreEqual ("A", project.EvaluatedItems [0].Name, "A0");
			project.ItemGroups.CopyTo (groups, 0);

			Assert.AreEqual (3, groups [0].Count, "A1");
			Assert.AreEqual ("Value", groups [0] [0].GetMetadata ("Meta"), "A2");
			Assert.AreEqual (String.Empty, groups [0] [1].GetMetadata ("Meta"), "A3");
			Assert.AreEqual (String.Empty, groups [0] [2].GetMetadata ("Meta"), "A4");
			Assert.AreEqual (3, project.EvaluatedItems.Count, "A5");
			Assert.AreEqual ("Value", project.EvaluatedItems [0].GetMetadata ("Meta"), "A6");
			Assert.AreEqual (String.Empty, project.EvaluatedItems [1].GetMetadata ("Meta"), "A7");
			Assert.AreEqual (String.Empty, project.EvaluatedItems [1].GetMetadata ("Meta"), "A8");
		}

		[Test]
		public void TestSetMetadata7 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a;b;c'>
							<Meta>Value2</Meta>
						</A>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.ItemGroups.CopyTo (groups, 0);
			groups [0][0].SetMetadata ("Meta", "Value");
			Assert.AreEqual ("Value", groups [0] [0].GetEvaluatedMetadata ("Meta"), "A1");
			Assert.AreEqual ("Value", groups [0] [0].GetMetadata ("Meta"), "A2");
		}

		[Test]
		public void TestSetMetadata8 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.EvaluatedItems [0].SetMetadata ("Meta", "Value");

			Assert.AreEqual (1, project.EvaluatedItems.Count, "A1");
			Assert.AreEqual ("Value", project.EvaluatedItems [0].GetMetadata ("Meta"), "A2");
			project.ItemGroups.CopyTo (groups, 0);
			Assert.AreEqual (1, groups [0].Count, "A3");
		}

		[Test]
		public void TestBuildItemTransform ()
		{
			string projectText = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<UsingTask TaskName='BatchingTestTask' AssemblyFile='Test\resources\TestTasks.dll' />

				<ItemGroup>
					<Foo Include='abc'/>
					<Foo Include='def'/>
				</ItemGroup>
				<PropertyGroup>
					<FooProp>PropValue/</FooProp>
				</PropertyGroup>

				<Target Name=""main"">
					<CreateItem Include=""@(Foo)"">
						<Output TaskParameter =""Include"" ItemName=""SyntheticFoo""/>
					</CreateItem>

					<BatchingTestTask
						TaskItemsOutput=""@(SyntheticFoo->'$(FooProp)%(Identity).txt')"">
						<Output TaskParameter='TaskItemsOutput' ItemName='I0' />
					</BatchingTestTask>
				</Target>
			</Project>";

			Engine engine = new Engine (Consts.BinPath);
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);
			Project project = engine.CreateNewProject ();
			project.LoadXml (projectText);

			bool result = project.Build ("main");
			if (!result) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			BuildItemGroup grp = project.GetEvaluatedItemsByName ("I0");
			Assert.AreEqual (2, grp.Count, "A1");
			Assert.AreEqual ("PropValue/abc.txt", grp [0].FinalItemSpec, "A2");
			Assert.AreEqual ("PropValue/def.txt", grp [1].FinalItemSpec, "A3");
		}

		void CheckMetadata (Project p, string itemname, string metadataname, string[] values, string prefix)
		{
			BuildItemGroup group = p.GetEvaluatedItemsByName (itemname);

			Assert.AreEqual (values.Length, group.Count, "Number of items for itemname " + itemname);

			for (int i = 0; i < values.Length; i++) {
				Assert.AreEqual (values[i], group [i].GetMetadata (metadataname), prefix + "#" + i.ToString ());
			}
		}

		void CheckEvaluatedMetadata (Project p, string itemname, string metadataname, string[] values, string prefix)
		{
			BuildItemGroup group = p.GetEvaluatedItemsByName (itemname);

			Assert.AreEqual (values.Length, group.Count, "Number of items for itemname " + itemname);

			for (int i = 0; i < values.Length; i++) {
				Assert.AreEqual (values[i], group [i].GetEvaluatedMetadata (metadataname), prefix + "#" + i.ToString ());
			}
		}

		void CheckBuildItemGroup (BuildItemGroup group, string[] names, string prefix)
		{
			try {
				Assert.AreEqual (group.Count, names.Length / 2, "Number of items in group");
				for (int i = 0; i < group.Count; i++) {
					Assert.AreEqual (names[i * 2], group[i].Name, String.Format ("{0}#{1} : item name", prefix, i));
					Assert.AreEqual (names[(i * 2) + 1], group[i].FinalItemSpec, String.Format ("{0}#{1} : FinalItemSpec", prefix, i));
				}
			} catch (AssertionException) {
				for (int i = 0; i < group.Count; i++) {
					Console.WriteLine ("group[{0}] = {1}", i, group[i].Name);
					Console.WriteLine ("group[{0}] = {1}", i, group[i].FinalItemSpec);
				}
				throw;
			}
		}
	}
}
