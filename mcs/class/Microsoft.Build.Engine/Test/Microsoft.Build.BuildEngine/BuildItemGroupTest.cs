//
// BuildItemGroupTest.cs
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
using System.Collections;
using System.Xml;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class BuildItemGroupTest {
		[Test]
		public void TestCtor ()
		{
			BuildItemGroup big = new BuildItemGroup ();

			Assert.AreEqual (String.Empty, big.Condition, "A1");
			Assert.AreEqual (0, big.Count, "A2");
			Assert.IsFalse (big.IsImported, "A3");
		}

		[Test]
		public void TestAddNewItem1 ()
		{
			string name = "name";
			string include = "a;b;c";
			
			BuildItemGroup big = new BuildItemGroup ();
			BuildItem bi = big.AddNewItem (name, include);

			Assert.AreEqual (String.Empty, bi.Condition, "A1");
			Assert.AreEqual (String.Empty, bi.Exclude, "A2");
			Assert.AreEqual (include, bi.FinalItemSpec, "A3");
			Assert.AreEqual (include, bi.Include, "A4");
			Assert.IsFalse (bi.IsImported, "A5");
			Assert.AreEqual (name, bi.Name, "A6");
			Assert.AreEqual (1, big.Count, "A7");
		}

		[Test]
		public void TestAddNewItem2 ()
		{
			Engine engine;
			Project project;
			string name = "name";
			string include = "$(Property)";
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<Property>a</Property>
					</PropertyGroup>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItem bi = project.EvaluatedItems.AddNewItem (name, include, true);

			Assert.AreEqual (String.Empty, bi.Condition, "A1");
			Assert.AreEqual (String.Empty, bi.Exclude, "A2");
			Assert.AreEqual (include, bi.FinalItemSpec, "A3");
			Assert.AreEqual (Utilities.Escape (include), bi.Include, "A4");
			Assert.IsFalse (bi.IsImported, "A5");
			Assert.AreEqual (name, bi.Name, "A6");

			bi = project.EvaluatedItems.AddNewItem (name, include, false);

			Assert.AreEqual (String.Empty, bi.Condition, "A7");
			Assert.AreEqual (String.Empty, bi.Exclude, "A8");
			Assert.AreEqual (include, bi.FinalItemSpec, "A9");
			Assert.AreEqual (include, bi.Include, "A10");
			Assert.IsFalse (bi.IsImported, "A11");
			Assert.AreEqual (name, bi.Name, "A12");
		}

		[Test]
		public void TestAddNewItem3 ()
		{
			Engine engine;
			Project project;
			string name = "name";
			string include = "$(Property)";

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<Property>a</Property>
					</PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItem bi = project.EvaluatedItems.AddNewItem (name, include, true);

			Assert.AreEqual (String.Empty, bi.Condition, "A1");
			Assert.AreEqual (String.Empty, bi.Exclude, "A2");
			Assert.AreEqual (include, bi.FinalItemSpec, "A3");
			Assert.AreEqual (Utilities.Escape (include), bi.Include, "A4");
			Assert.IsFalse (bi.IsImported, "A5");
			Assert.AreEqual (name, bi.Name, "A6");

			bi = project.EvaluatedItems.AddNewItem (name, include, false);

			Assert.AreEqual (String.Empty, bi.Condition, "A7");
			Assert.AreEqual (String.Empty, bi.Exclude, "A8");
			Assert.AreEqual (include, bi.FinalItemSpec, "A9");
			Assert.AreEqual (include, bi.Include, "A10");
			Assert.IsFalse (bi.IsImported, "A11");
			Assert.AreEqual (name, bi.Name, "A12");

			Assert.AreEqual (0, project.EvaluatedItems.Count, "A13");

			project.GlobalProperties.SetProperty ("a", "b");

			Assert.AreEqual (0, project.EvaluatedItems.Count, "A14");
		}

		[Test]
		public void TestAddNewItem4 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.EvaluatedItems.AddNewItem ("I1", "Value");

			BuildItem bi = project.EvaluatedItems.AddNewItem ("I2", "@(I1)");

			Assert.AreEqual ("@(I1)", bi.FinalItemSpec, "A1");
			Assert.AreEqual ("@(I1)", bi.Include, "A2");
			Assert.AreEqual (0, project.EvaluatedItems.Count, "A3");
		}

		[Test]
		public void TestAddNewItem5 ()
		{
			BuildItemGroup big = new BuildItemGroup ();

			big.AddNewItem ("I1", "Value");

			BuildItem bi = big.AddNewItem ("I2", "@(I1)");

			Assert.AreEqual ("@(I1)", bi.FinalItemSpec, "A1");
			Assert.AreEqual ("@(I1)", bi.Include, "A2");
			Assert.AreEqual (2, big.Count, "A3");
		}

		[Test]
		public void TestAddNewItem6 ()
		{
			Engine engine;
			Project project;
			string name = "name";
			string include = "include";
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup />
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.ItemGroups.CopyTo (groups, 0);
			BuildItem bi = groups [0].AddNewItem (name, include, true);

			Assert.AreEqual (String.Empty, bi.Condition, "A1");
			Assert.AreEqual (String.Empty, bi.Exclude, "A2");
			Assert.AreEqual (include, bi.FinalItemSpec, "A3");
			Assert.AreEqual (Utilities.Escape (include), bi.Include, "A4");
			Assert.IsFalse (bi.IsImported, "A5");
			Assert.AreEqual (name, bi.Name, "A6");
			//Assert.AreSame (bi, groups [0] [0], "A7");
			Assert.IsTrue (object.ReferenceEquals (bi, groups [0][0]), "A7");
			//Assert.AreNotSame (bi, project.EvaluatedItems [0], "A8");
			Assert.IsFalse (object.ReferenceEquals (bi, project.EvaluatedItems [0]), "A8");
		}

		[Test]
		public void TestClear1 ()
		{
			BuildItemGroup big = new BuildItemGroup ();
			big.AddNewItem ("a", "a");
			big.AddNewItem ("b", "a");

			Assert.AreEqual (2, big.Count, "A1");
			
			big.Clear ();

			Assert.AreEqual (0, big.Count, "A2");
		}

		[Test]
		public void TestClear2 ()
		{
			Engine engine;
			Project project;
			XmlDocument xd;
			XmlNode node;
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<Item Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			xd = new XmlDocument ();
			xd.LoadXml (project.Xml);
			node = xd.SelectSingleNode ("tns:Project/tns:ItemGroup/*", TestNamespaceManager.NamespaceManager);
			Assert.IsNotNull (node, "A1");

			BuildItemGroup [] big = new BuildItemGroup [1];
			project.ItemGroups.CopyTo (big, 0);
			big [0].Clear ();

			Assert.AreEqual (0, big [0].Count, "A2");
			xd = new XmlDocument ();
			xd.LoadXml (project.Xml);
			node = xd.SelectSingleNode ("tns:Project/tns:ItemGroup/*", TestNamespaceManager.NamespaceManager);
			Assert.IsNull (node, "A3");
		}
		[Test]
		[Category ("NotWorking")]
		public void TestClone1 ()
		{
			BuildItemGroup big = new BuildItemGroup ();
			big.AddNewItem ("a", "a");
			big.AddNewItem ("b", "a");

			BuildItemGroup big2 = big.Clone (false);
			BuildItem[] items = big2.ToArray ();

			Assert.AreEqual (2, big2.Count, "A1");

			Assert.AreEqual (String.Empty, items [0].Condition, "A2");
			Assert.AreEqual (String.Empty, items [0].Exclude, "A3");
			Assert.AreEqual ("a", items [0].FinalItemSpec, "A4");
			Assert.AreEqual ("a", items [0].Include, "A5");
			Assert.IsFalse (items [0].IsImported, "A6");
			Assert.AreEqual ("a", items [0].Name, "A7");
			
			Assert.AreEqual (String.Empty, items [1].Condition, "A8");
			Assert.AreEqual (String.Empty, items [1].Exclude, "A9");
			Assert.AreEqual ("a", items [1].FinalItemSpec, "A10");
			Assert.AreEqual ("a", items [1].Include, "A11");
			Assert.IsFalse (items [1].IsImported, "A12");
			Assert.AreEqual ("b", items [1].Name, "A13");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestClone2 ()
		{
			BuildItemGroup big = new BuildItemGroup ();
			big.AddNewItem ("a", "a");
			big.AddNewItem ("b", "a");

			BuildItemGroup big2 = big.Clone (true);
			BuildItem[] items = big2.ToArray ();

			Assert.AreEqual (2, big2.Count, "A1");

			Assert.AreEqual (String.Empty, items [0].Condition, "A2");
			Assert.AreEqual (String.Empty, items [0].Exclude, "A3");
			Assert.AreEqual ("a", items [0].FinalItemSpec, "A4");
			Assert.AreEqual ("a", items [0].Include, "A5");
			Assert.IsFalse (items [0].IsImported, "A6");
			Assert.AreEqual ("a", items [0].Name, "A7");
			
			Assert.AreEqual (String.Empty, items [1].Condition, "A8");
			Assert.AreEqual (String.Empty, items [1].Exclude, "A9");
			Assert.AreEqual ("a", items [1].FinalItemSpec, "A10");
			Assert.AreEqual ("a", items [1].Include, "A11");
			Assert.IsFalse (items [1].IsImported, "A12");
			Assert.AreEqual ("b", items [1].Name, "A13");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestClone3 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<Item Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup big2 = project.EvaluatedItems.Clone (false);
			BuildItem[] items = big2.ToArray ();

			Assert.AreEqual (1, big2.Count, "A1");

			Assert.AreEqual (String.Empty, items [0].Condition, "A2");
			Assert.AreEqual (String.Empty, items [0].Exclude, "A3");
			Assert.AreEqual ("a", items [0].FinalItemSpec, "A4");
			Assert.AreEqual ("a", items [0].Include, "A5");
			Assert.IsFalse (items [0].IsImported, "A6");
			Assert.AreEqual ("Item", items [0].Name, "A7");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestClone4 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<Item Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup big2 = project.EvaluatedItems.Clone (true);
			BuildItem[] items = big2.ToArray ();

			Assert.AreEqual (1, big2.Count, "A1");

			Assert.AreEqual (String.Empty, items [0].Condition, "A2");
			Assert.AreEqual (String.Empty, items [0].Exclude, "A3");
			Assert.AreEqual ("a", items [0].FinalItemSpec, "A4");
			Assert.AreEqual ("a", items [0].Include, "A5");
			Assert.IsFalse (items [0].IsImported, "A6");
			Assert.AreEqual ("Item", items [0].Name, "A7");
		}

		// A shallow clone of this object cannot be created
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestClone5 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<Item Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup[] groups = new BuildItemGroup [1];
		       	project.ItemGroups.CopyTo (groups, 0);
			
			groups [0].Clone (false);
		}

		[Test]
		[Category ("NotWorking")]
		public void TestClone6 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<Item Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup[] groups = new BuildItemGroup [1];
		       	project.ItemGroups.CopyTo (groups, 0);
			
			BuildItemGroup big2 = groups [0].Clone (true);
			BuildItem[] items = big2.ToArray ();

			Assert.AreEqual (1, big2.Count, "A1");

			Assert.AreEqual (String.Empty, items [0].Condition, "A2");
			Assert.AreEqual (String.Empty, items [0].Exclude, "A3");
			Assert.AreEqual ("a", items [0].FinalItemSpec, "A4");
			Assert.AreEqual ("a", items [0].Include, "A5");
			Assert.IsFalse (items [0].IsImported, "A6");
			Assert.AreEqual ("Item", items [0].Name, "A7");
		}

		[Test]
		public void TestCondition1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup Condition='true' >
						<Item Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup [] groups = new BuildItemGroup [1];
			project.ItemGroups.CopyTo (groups, 0);

			Assert.AreEqual ("true", groups [0].Condition, "A1");
			Assert.IsFalse (groups [0].IsImported, "A2");
		}

		// Cannot set a condition on an object not represented by an XML element in the project file.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestCondition2 ()
		{
			BuildItemGroup big = new BuildItemGroup ();

			big.Condition = "true";
		}

		[Test]
		public void TestCondition3 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup Condition='true' >
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup [] groups = new BuildItemGroup [1];
			project.ItemGroups.CopyTo (groups, 0);

			Assert.AreEqual ("true", groups [0].Condition, "A1");
			groups [0].Condition = "false";

			Assert.AreEqual ("false", groups [0].Condition, "A2");
		}

		[Test]
		public void TestGetEnumerator ()
		{
			BuildItemGroup big = new BuildItemGroup ();
			big.AddNewItem ("a", "c");
			big.AddNewItem ("b", "d");

			IEnumerator e = big.GetEnumerator ();
			e.MoveNext ();
			Assert.AreEqual ("a", ((BuildItem) e.Current).Name, "A1");
			Assert.AreEqual ("c", ((BuildItem) e.Current).FinalItemSpec, "A2");
			e.MoveNext ();
			Assert.AreEqual ("b", ((BuildItem) e.Current).Name, "A3");
			Assert.AreEqual ("d", ((BuildItem) e.Current).FinalItemSpec, "A4");
			
			Assert.IsFalse (e.MoveNext ());
		}

		[Test]
		public void TestRemoveItem1 ()
		{
			BuildItemGroup big = new BuildItemGroup ();

			big.AddNewItem ("a", "b");
			BuildItem b = big.AddNewItem ("b", "c");
			big.AddNewItem ("c", "d");

			big.RemoveItem (b);

			BuildItem[] items = big.ToArray ();
			Assert.AreEqual (2, big.Count, "A1");
			Assert.AreEqual ("a", items [0].Name, "A2");
			Assert.AreEqual ("c", items [1].Name, "A3");
		}

		[Test]
		// NOTE: maybe it should throw an exception?
		// at the moment it probably doesn't find a "null" element
		public void TestRemoveItem2 ()
		{
			BuildItemGroup big = new BuildItemGroup ();

			big.RemoveItem (null);
		}

		[Test]
		public void TestRemoveItem3 ()
		{
			Engine engine;
			Project project;
			XmlDocument xd;
			XmlNode node;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='B'/>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup [] groups = new BuildItemGroup [1];
			project.ItemGroups.CopyTo (groups, 0);

			groups [0].RemoveItem (groups [0] [0]);
			Assert.AreEqual (0, groups [0].Count, "A1");
			Assert.AreEqual (1, project.ItemGroups.Count, "A2");

			xd = new XmlDocument ();
			xd.LoadXml (project.Xml);
			node = xd.SelectSingleNode ("tns:Project/tns:ItemGroup/tns:A", TestNamespaceManager.NamespaceManager);
			Assert.IsNull (node, "A3");
		}

		[Test]
		public void TestRemoveItemAt1 ()
		{
			BuildItemGroup big = new BuildItemGroup ();

			big.AddNewItem ("a", "b");
			big.AddNewItem ("b", "c");
			big.AddNewItem ("c", "d");

			big.RemoveItemAt (1);

			BuildItem[] items = big.ToArray ();
			Assert.AreEqual (2, big.Count, "A1");
			Assert.AreEqual ("a", items [0].Name, "A2");
			Assert.AreEqual ("c", items [1].Name, "A3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TestRemoveItemAt2 ()
		{
			BuildItemGroup big = new BuildItemGroup ();

			big.AddNewItem ("a", "b");
			big.AddNewItem ("b", "c");
			big.AddNewItem ("c", "d");

			big.RemoveItemAt (-1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TestRemoveItemAt3 ()
		{
			BuildItemGroup big = new BuildItemGroup ();

			big.AddNewItem ("a", "b");
			big.AddNewItem ("b", "c");
			big.AddNewItem ("c", "d");

			big.RemoveItemAt (3);
		}

		[Test]
		public void TestRemoveItemAt4 ()
		{
			Engine engine;
			Project project;
			XmlDocument xd;
			XmlNode node;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='D'/>
						<B Include='D'/>
						<C Include='D'/>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup [] groups = new BuildItemGroup [1];
			project.ItemGroups.CopyTo (groups, 0);

			groups [0].RemoveItemAt (1);
			Assert.AreEqual (2, groups [0].Count, "A1");
			Assert.AreEqual (1, project.ItemGroups.Count, "A2");

			xd = new XmlDocument ();
			xd.LoadXml (project.Xml);
			node = xd.SelectSingleNode ("tns:Project/tns:ItemGroup/tns:B", TestNamespaceManager.NamespaceManager);
			Assert.IsNull (node, "A3");
			Assert.IsTrue (project.IsDirty, "A4");
		}

		[Test]
		public void TestToArray1 ()
		{
			BuildItemGroup big = new BuildItemGroup ();

			BuildItem[] items = big.ToArray ();

			Assert.AreEqual (0, items.Length, "A1");

			big.AddNewItem ("a", "b");
			big.AddNewItem ("c", "d");

			items = big.ToArray ();

			Assert.AreEqual ("a", items [0].Name, "A2");
			Assert.AreEqual ("c", items [1].Name, "A3");
		}

		[Test]
		public void TestIndexer1 ()
		{
			BuildItemGroup big = new BuildItemGroup ();
			big.AddNewItem ("a", "b");
			big.AddNewItem ("c", "d");

			Assert.AreEqual ("a", big [0].Name, "A1");
			Assert.AreEqual ("c", big [1].Name, "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TestIndexer2 ()
		{
			BuildItemGroup big = new BuildItemGroup ();
			Assert.IsNotNull (big [0], "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TestIndexer3 ()
		{
			BuildItemGroup big = new BuildItemGroup ();
			Assert.IsNotNull (big [-1], "A1");
		}
	}
}
