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
		
		[Test]
		public void TestAssignment ()
		{
			bpg = new BuildPropertyGroup ();
			
			Assert.AreEqual (0, bpg.Count);
			Assert.AreEqual (false, bpg.IsImported);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException),
			"This method is only valid for persisted <System.Object[]> elements.")]
		public void TestAddNewProperty1 ()
		{
			string name = "name";
			string value = "value";

			bpg = new BuildPropertyGroup ();

			bpg.AddNewProperty (name, value);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException),
			"Properties in persisted property groups cannot be accessed by name.")]
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
		public void TestClear ()
		{
			bpg = new BuildPropertyGroup ();
			
			bpg.SetProperty ("a", "b");
			Assert.AreEqual (1, bpg.Count, "A1");
			bpg.Clear ();
			Assert.AreEqual (0, bpg.Count, "A2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException),
			"Cannot set a condition on an object not represented by an XML element in the project file.")]
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
		public void TestGetEnumerator ()
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

			Assert.IsFalse (e.MoveNext ());
		}

		[Test]
		[Ignore ("NullRefException on MS .NET 2.0")]
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
		}

		[Test]
		public void TestRemoveProperty4 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();
			bpg.SetProperty ("a", "b");
			bpg.SetProperty ("c", "d");

			bpg.RemoveProperty (new BuildProperty ("name", "value"));
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

		[Test]
		[ExpectedException (typeof (ArgumentException),
			"The name \"1\" contains an invalid character \"1\".")]
		[Category ("NotWorking")]
		public void TestSetProperty5 ()
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup ();

			bpg.SetProperty ("1", "$(A)");
		}
	}
}
