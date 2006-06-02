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
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class BuildPropertyTest {
		
		BuildProperty	bp;
		string		binPath;
		Engine		engine;
		Project		project;
		
		[SetUp]
		public void SetUp ()
		{
			binPath = "../../tools/xbuild/xbuild";
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
		[ExpectedException (typeof (ArgumentNullException),
			"Parameter \"propertyName\" cannot be null.")]
		public void TestCtor2 ()
		{
			bp = new BuildProperty (null, "value");
			
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException),
			"Parameter \"propertyValue\" cannot be null.")]
		public void TestCtor3 ()
		{
			bp = new BuildProperty ("name", null);
			
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException),
			"A shallow clone of this object cannot be created.")]
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

                        engine = new Engine (binPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			a = project.EvaluatedProperties ["Name"];
			Assert.AreEqual ("Value", a.Value, "A1");
			
			b = a.Clone (false);
			
			b.Value = "AnotherValue";
			Assert.AreEqual ("Value", a.Value, "A2");
		}

		[Test]
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

                        engine = new Engine (binPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			a = project.EvaluatedProperties ["Name"];
			Assert.AreEqual ("Value", a.Value, "A1");
			
			b = a.Clone (true);
			
			b.Value = "AnotherValue";
			Assert.AreEqual ("Value", a.Value, "A2");
		}

		[Test]
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

                        engine = new Engine (binPath);

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

                        engine = new Engine (binPath);

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
		public void TestOpExplicit ()
		{
			bp = new BuildProperty ("name", "value");
			
			Assert.AreEqual ("value", (string) bp, "A1");
		}
		
		[Test]
		public void TestToString ()
		{
			bp = new BuildProperty ("name", "a;b");
			Assert.AreEqual ("a;b", bp.ToString ());
		}
	}
}
