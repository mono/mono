//
// BuildPropertyTest.cs
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
using System.Xml;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class BuildPropertyTest {
		
		BuildProperty	bp;
		Engine		engine;
		Project		project;

		BuildProperty [] GetProperties (BuildPropertyGroup bpg)
		{
			BuildProperty [] arr = new BuildProperty [bpg.Count];
			int i = 0;
			foreach (BuildProperty bp in bpg)
				arr [i++] = bp;
			return arr;
		}

		[Test]
		public void TestCtor1 ()
		{
			string name = "name";
			string value = "value";
		
			bp = new BuildProperty (name, value);
			
			Assert.AreEqual (name, bp.Name, "A1");
			Assert.AreEqual (value, bp.Value, "A2");
			Assert.AreEqual (String.Empty, bp.Condition, "A3");
			Assert.AreEqual (value, bp.FinalValue, "A4");
			Assert.AreEqual (false, bp.IsImported, "A5");
			Assert.AreEqual (value, bp.ToString (), "A6");
		
			name = "name";
			value = "$(AnotherProperty)";
		
			bp = new BuildProperty (name, value);
			
			Assert.AreEqual (name, bp.Name, "A7");
			Assert.AreEqual (value, bp.Value, "A8");
			Assert.AreEqual (String.Empty, bp.Condition, "A9");
			Assert.AreEqual (value, bp.FinalValue, "A10");
			Assert.AreEqual (false, bp.IsImported, "A11");
			Assert.AreEqual (value, bp.ToString (), "A12");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtor2 ()
		{
			bp = new BuildProperty (null, "value");
			
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCtor3 ()
		{
			bp = new BuildProperty ("name", null);
			
		}

		// A shallow clone of this object cannot be created.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestClone1 ()
		{
			bp = new BuildProperty ("name", "value");
			
			bp.Clone (false);
		}

		[Test]
		public void TestClone2 ()
		{
			bp = new BuildProperty ("name", "value");
			
			bp.Clone (true);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void TestClone3 ()
		{
			BuildProperty a,b;
			
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

			a = project.EvaluatedProperties ["Name"];
			Assert.AreEqual ("Value", a.Value, "A1");
			
			b = a.Clone (false);
			
			b.Value = "AnotherValue";
			Assert.AreEqual ("Value", a.Value, "A2");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestClone4 ()
		{
			BuildProperty a,b;
			
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

			a = project.EvaluatedProperties ["Name"];
			Assert.AreEqual ("Value", a.Value, "A1");
			
			b = a.Clone (true);
			
			b.Value = "AnotherValue";
			Assert.AreEqual ("Value", a.Value, "A2");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestClone5 ()
		{
			BuildProperty a,b;
			IList properties = new ArrayList ();
			
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

			foreach (BuildPropertyGroup bpg in project.PropertyGroups)
				foreach (BuildProperty bpr in bpg)
					properties.Add (bpr);
			
			a = (BuildProperty) properties [0];
			Assert.AreEqual ("Value", a.Value, "A1");
			
			b = a.Clone (false);
			
			b.Value = "AnotherValue";
			Assert.AreEqual ("Value", a.Value, "A2");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestClone6 ()
		{
			BuildProperty a,b;
			IList properties = new ArrayList ();
			
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

			foreach (BuildPropertyGroup bpg in project.PropertyGroups)
				foreach (BuildProperty bpr in bpg)
					properties.Add (bpr);
			
			a = (BuildProperty) properties [0];
			Assert.AreEqual ("Value", a.Value, "A1");
			
			b = a.Clone (true);
			
			b.Value = "AnotherValue";
			Assert.AreEqual ("Value", a.Value, "A2");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestCondition1 ()
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

			BuildProperty a = project.EvaluatedProperties ["Name"];

			a.Condition = "true";
			Assert.AreEqual ("true", a.Condition, "A1");
		}

		// Cannot set a condition on an object not represented by an XML element in the project file.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestCondition2 ()
		{
			BuildProperty a = new BuildProperty ("name", "value");
			a.Condition = "true";
		}

		[Test]
		public void TestOpExplicit1 ()
		{
			bp = new BuildProperty ("name", "value");
			
			Assert.AreEqual ("value", (string) bp, "A1");
		}

		[Test]
		public void TestOpExplicit2 ()
		{
			BuildProperty bp = null;
			
			Assert.AreEqual (String.Empty, (string) bp, "A1");
		}
		
		[Test]
		public void TestToString ()
		{
			bp = new BuildProperty ("name", "a;b");
			Assert.AreEqual ("a;b", bp.ToString ());
		}

		[Test]
		public void TestValue1 ()
		{
			BuildProperty a;
			BuildPropertyGroup [] bpgs = new BuildPropertyGroup [1];
			BuildProperty [] props;

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

			a = project.EvaluatedProperties ["Name"];
			a.Value = "$(something)";
			Assert.AreEqual ("$(something)", a.Value, "A1");

			project.PropertyGroups.CopyTo (bpgs, 0);
			props = GetProperties (bpgs [0]);
			Assert.AreEqual ("Value", props [0].Value, "A2");
		}

		[Test]
		public void TestValue2 ()
		{
			BuildPropertyGroup [] bpgs = new BuildPropertyGroup [1];
			BuildProperty [] props;
			XmlDocument xd;
			XmlNode node;

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
			
			project.PropertyGroups.CopyTo (bpgs, 0);
			props = GetProperties (bpgs [0]);
			props [0].Value = "AnotherValue";

			xd = new XmlDocument ();
			xd.LoadXml (project.Xml);
			node = xd.SelectSingleNode ("tns:Project/tns:PropertyGroup/tns:Name", TestNamespaceManager.NamespaceManager);
			Assert.AreEqual ("AnotherValue", node.InnerText, "A1");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestValueXml ()
		{
			BuildPropertyGroup [] bpgs = new BuildPropertyGroup [1];
			BuildProperty [] props;
			XmlDocument xd;
			XmlNode node;

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

			project.PropertyGroups.CopyTo (bpgs, 0);
			bpgs[0].AddNewProperty("XmlProp", "<XmlStuff></XmlStuff>");

			xd = new XmlDocument ();
			xd.LoadXml (project.Xml);
			node = xd.SelectSingleNode ("tns:Project/tns:PropertyGroup/tns:XmlProp/tns:XmlStuff", TestNamespaceManager.NamespaceManager);
			if (node == null) {
				Console.WriteLine (project.Xml);
				Assert.Fail ("Expected node to be non-null");
			}
		}

	}
}
