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
		string			binPath;
		Engine			engine;
		Project			project;
		
		[SetUp]
		public void SetUp ()
		{
			binPath = "../../tools/xbuild/xbuild";
		}
		
		[Test]
		public void TestAssignment ()
		{
			bpg = new BuildPropertyGroup ();
			
			Assert.AreEqual (0, bpg.Count);
			Assert.AreEqual (false, bpg.IsImported);
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

                        engine = new Engine (binPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			IEnumerator en = project.PropertyGroups.GetEnumerator ();
			en.MoveNext ();
			bpg = (BuildPropertyGroup) en.Current;
			Assert.AreEqual ("Value", bpg ["Name"].Value, "A3");
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
	}
}
