//
// BuildPropertyGroupTest.cs
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
using System.Collections.Generic;
using System.Xml;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class BuildPropertyGroupTest {
		
		BuildPropertyGroup	bpg;
		Engine			engine;
		Project			project;

		BuildProperty [] GetProperties (BuildPropertyGroup bpg)
		{
			List<BuildProperty> list = new List<BuildProperty> ();
			foreach (BuildProperty bp in bpg)
				list.Add (bp);
			return list.ToArray ();
		}
		
		[Test]
		public void TestAssignment ()
		{
			bpg = new BuildPropertyGroup ();
			
			Assert.AreEqual (0, bpg.Count);
			Assert.AreEqual (false, bpg.IsImported);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAddNewProperty1 ()
		{
			string name = "name";
			string value = "value";

			bpg = new BuildPropertyGroup ();

			bpg.AddNewProperty (name, value);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAddNewProperty2 ()
		{
			Engine engine;
			Project project;

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			project.EvaluatedProperties.AddNewProperty ("a", "b");
		}

		[Test]
		public void TestAddNewProperty3 ()
		{
			Engine engine;
			Project project;
			BuildPropertyGroup [] groups = new BuildPropertyGroup [2];
			XmlDocument xd;
			XmlNode node;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
					</PropertyGroup>
					<PropertyGroup>
						<B>$(A)</B>
					</PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.PropertyGroups.CopyTo (groups, 0);
			Assert.AreEqual ("", project.EvaluatedProperties ["B"].FinalValue, "A0");
			groups [0].AddNewProperty ("A", "A");

			Assert.AreEqual (1, groups [0].Count, "A1");
			Assert.AreEqual ("A", project.EvaluatedProperties ["A"].FinalValue, "A2");
			Assert.AreEqual ("A", project.EvaluatedProperties ["B"].FinalValue, "A3");

			xd = new XmlDocument ();
			xd.LoadXml (project.Xml);
			node = xd.SelectSingleNode ("/tns:Project/tns:PropertyGroup/tns:A", TestNamespaceManager.NamespaceManager);
			Assert.IsNotNull (node, "A4");
		}

		// FIXME: what was that supposed to test?
		// Properties in persisted property groups cannot be accessed by name.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestClone1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                	<PropertyGroup>
                                		<Name>Value</Name>
                                	</PropertyGroup>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			IEnumerator en = project.PropertyGroups.GetEnumerator ();
			en.MoveNext ();
			bpg = (BuildPropertyGroup) en.Current;
			Assert.AreEqual ("Value", bpg ["Name"].Value, "A3");
		}

		[Test]
		public void TestClear1 ()
		{
			bpg = new BuildPropertyGroup ();
			
			bpg.SetProperty ("a", "b");
			Assert.AreEqual (1, bpg.Count, "A1");
			bpg.Clear ();
			Assert.AreEqual (0, bpg.Count, "A2");
		}

		[Test]
		public void TestClear2 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                	<PropertyGroup>
                                		<Name>Value</Name>
                                	</PropertyGroup>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			BuildPropertyGroup [] array = new BuildPropertyGroup [1];
			project.PropertyGroups.CopyTo (array, 0);

			array [0].Clear ();

			Assert.AreEqual (0, array [0].Count, "A1");
		}

		// Cannot set a condition on an object not represented by an XML element in the project file.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestCondition1 ()
		{
			string condition = "condition";
		
			bpg = new BuildPropertyGroup ();
		
			bpg.Condition = condition;
		}

		[Test]
		public void TestCondition2 ()
		{
			bpg = new BuildPropertyGroup ();
		
			Assert.AreEqual (String.Empty, bpg.Condition, "A1");
		}

		[Test]
		public void TestCondition3 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                	<PropertyGroup>
                                	</PropertyGroup>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			BuildPropertyGroup [] array = new BuildPropertyGroup [1];
			project.PropertyGroups.CopyTo (array, 0);

			array [0].Condition = "true";
			Assert.AreEqual ("true", array [0].Condition, "A1");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestGetEnumerator1 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();
			bpg.SetProperty ("a", "c");
			bpg.SetProperty ("b", "d");

			IEnumerator e = bpg.GetEnumerator ();
			e.MoveNext ();
			Assert.AreEqual ("a", ((BuildProperty) e.Current).Name, "A1");
			Assert.AreEqual ("c", ((BuildProperty) e.Current).Value, "A2");
			Assert.AreEqual ("c", ((BuildProperty) e.Current).FinalValue, "A3");
			e.MoveNext ();
			Assert.AreEqual ("b", ((BuildProperty) e.Current).Name, "A4");
			Assert.AreEqual ("d", ((BuildProperty) e.Current).Value, "A5");
			Assert.AreEqual ("d", ((BuildProperty) e.Current).FinalValue, "A6");

			Assert.IsFalse (e.MoveNext (), "A7");
		}

		[Test]
		public void TestGetEnumerator2 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                	<PropertyGroup>
                                		<P1>1</P1>
                                		<P2>2</P2>
                                	</PropertyGroup>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			BuildPropertyGroup [] array = new BuildPropertyGroup [1];
			project.PropertyGroups.CopyTo (array, 0);

			IEnumerator e = array [0].GetEnumerator ();
			e.MoveNext ();
			Assert.AreEqual ("P1", ((BuildProperty) e.Current).Name, "A1");
			Assert.AreEqual ("1", ((BuildProperty) e.Current).Value, "A2");
			Assert.AreEqual ("1", ((BuildProperty) e.Current).FinalValue, "A3");
			e.MoveNext ();
			Assert.AreEqual ("P2", ((BuildProperty) e.Current).Name, "A4");
			Assert.AreEqual ("2", ((BuildProperty) e.Current).Value, "A5");
			Assert.AreEqual ("2", ((BuildProperty) e.Current).FinalValue, "A6");

			Assert.IsFalse (e.MoveNext (), "A7");
		}

		[Test]
		public void TestIndexer1 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();
			bpg.SetProperty ("a", "1");
			bpg.SetProperty ("b", "2");

			Assert.AreEqual ("a", bpg ["a"].Name, "A1");
			Assert.AreEqual ("b", bpg ["b"].Name, "A2");
			Assert.IsNull (bpg ["something_that_doesnt_exist"], "A3");
			bpg ["a"].Value = "3";
			Assert.AreEqual ("3", bpg ["a"].Value, "A4");
		}

		// Properties in persisted property groups cannot be accessed by name.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestIndexer2 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                	<PropertyGroup>
                                		<a>1</a>
                                		<b>2</b>
                                	</PropertyGroup>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			BuildPropertyGroup [] array = new BuildPropertyGroup [1];
			project.PropertyGroups.CopyTo (array, 0);

			Assert.AreEqual ("a", array [0] ["a"].Name, "A1");
		}

		// Properties in persisted property groups cannot be accessed by name.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestIndexer3 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                	<PropertyGroup>
                                		<a>1</a>
                                		<b>2</b>
                                	</PropertyGroup>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			BuildPropertyGroup [] array = new BuildPropertyGroup [1];
			project.PropertyGroups.CopyTo (array, 0);

			array [0] ["a"].Value = "3";
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveProperty1 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();
			bpg.RemoveProperty ((BuildProperty) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveProperty2 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();
			bpg.SetProperty ("a", "b");
			bpg.SetProperty ("c", "d");

			bpg.RemoveProperty ((string) null);
		}

		[Test]
		public void TestRemoveProperty3 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();
			bpg.SetProperty ("a", "b");
			bpg.SetProperty ("c", "d");

			bpg.RemoveProperty ("value_not_in_group");
			bpg.RemoveProperty (new BuildProperty ("name", "value"));

			BuildProperty bp = bpg ["a"];

			bpg.RemoveProperty (bp);
		}

		[Test]
		public void TestRemoveProperty4 ()
		{
			Engine engine;
			Project project;
			BuildPropertyGroup [] bpg = new BuildPropertyGroup [1];
			XmlDocument xd;
			XmlNode node;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<A>A</A>
					</PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.PropertyGroups.CopyTo (bpg, 0);

			bpg [0].RemoveProperty ("A");
			Assert.AreEqual (0, bpg [0].Count, "A1");
			xd = new XmlDocument ();
			xd.LoadXml (project.Xml);
			node = xd.SelectSingleNode ("/tns:Project/tns:PropertyGroup/tns:A", TestNamespaceManager.NamespaceManager);
			Assert.IsNull (node, "A3");

			bpg [0].RemoveProperty ("B");
		}

		[Test]
		public void TestRemoveProperty5 ()
		{
			Engine engine;
			Project project;
			BuildPropertyGroup [] bpg = new BuildPropertyGroup [1];
			XmlDocument xd;
			XmlNode node;
			BuildProperty [] properties;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<A>A</A>
					</PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.PropertyGroups.CopyTo (bpg, 0);

			properties = GetProperties (bpg [0]);

			bpg [0].RemoveProperty (properties [0]);
			Assert.AreEqual (0, bpg [0].Count, "A1");
			xd = new XmlDocument ();
			xd.LoadXml (project.Xml);
			node = xd.SelectSingleNode ("/tns:Project/tns:PropertyGroup/tns:A", TestNamespaceManager.NamespaceManager);
			Assert.IsNull (node, "A3");
		}

		// The specified property does not belong to the current property group.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestRemoveProperty6 ()
		{
			Engine engine;
			Project project;
			BuildPropertyGroup [] bpg = new BuildPropertyGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
					</PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.PropertyGroups.CopyTo (bpg, 0);

			bpg [0].RemoveProperty (new BuildProperty ("A", "b"));
		}

		[Test]
		[Category ("NotDotNet")]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestRemoveProperty7 ()
		{
			Engine engine;
			Project project;
			BuildPropertyGroup [] bpg = new BuildPropertyGroup [2];
			BuildProperty [] properties;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<A></A>
					</PropertyGroup>
					<PropertyGroup>
					</PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			project.PropertyGroups.CopyTo (bpg, 0);

			properties = GetProperties (bpg [0]);
			bpg [1].RemoveProperty (properties [0]);
			Assert.AreEqual (1, bpg [0].Count, "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestSetProperty1 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();
			bpg.SetProperty (null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestSetProperty2 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();
			bpg.SetProperty ("name", null);
		}

		[Test]
		public void TestSetProperty3 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();
			
			bpg.SetProperty ("name", "$(A)");

			BuildProperty bp = bpg ["name"];

			Assert.AreEqual ("name", bp.Name, "A1");
			Assert.AreEqual ("$(A)", bp.Value, "A2");
			Assert.AreEqual ("$(A)", bp.FinalValue, "A3");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSetProperty4 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();

			bpg.SetProperty ("P1", "$(A)", true);
			bpg.SetProperty ("P2", "$(A)", false);

			BuildProperty b1 = bpg ["P1"];
			BuildProperty b2 = bpg ["P2"];

			Assert.AreEqual ("P1", b1.Name, "A1");
			Assert.AreEqual (Utilities.Escape ("$(A)"), b1.Value, "A2");
			Assert.AreEqual ("$(A)", b1.FinalValue, "A3");
			Assert.AreEqual ("P2", b2.Name, "A4");
			Assert.AreEqual ("$(A)", b2.Value, "A5");
			Assert.AreEqual ("$(A)", b2.FinalValue, "A6");
		}

		// The name "1" contains an invalid character "1".
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestSetProperty5 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();

			bpg.SetProperty ("1", "$(A)");
		}

		[Test]
		public void TestSetProperty6 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<Property Condition=""'$(P)' != ''"">a</Property>
					</PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.GlobalProperties.SetProperty ("P", "V");

			Assert.IsNotNull (project.EvaluatedProperties ["Property"], "A1");
		}

		[Test]
		public void TestSetProperty7 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<Property Condition=""'$(P)' != ''"">a</Property>
						<A>A</A>
					</PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildProperty p1 = project.EvaluatedProperties ["A"];
			p1.Value = "B";

			project.GlobalProperties.SetProperty ("P", "V");

			Assert.AreEqual ("A", project.EvaluatedProperties ["A"].Value, "A1");
		}

		[Test]
		public void TestSetProperty8 ()
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

			project.EvaluatedProperties.SetProperty ("A", "B");

			Assert.IsNull (project.EvaluatedProperties ["A"], "A1");
		}

		// This method is only valid for virtual property groups, not <PropertyGroup> elements.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestSetProperty9 ()
		{
			Engine engine;
			Project project;
			BuildPropertyGroup [] groups = new BuildPropertyGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup />
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.PropertyGroups.CopyTo (groups, 0);
			groups [0].SetProperty ("A", "B");
		}
	}
}
