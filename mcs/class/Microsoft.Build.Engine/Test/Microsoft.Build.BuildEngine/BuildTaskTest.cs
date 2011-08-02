//
// BuildTaskTest.cs
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
using System.Collections.Generic;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class BuildTaskTest {

		static BuildTask[] GetTasks (Target t)
		{
			List <BuildTask> list = new List <BuildTask> ();
			foreach (BuildTask bt in t)
				list.Add (bt);
			return list.ToArray ();
		}

		[Test]
		public void TestFromXml ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<Property>true</Property>
					</PropertyGroup>
					<Target Name='T'>
						<Message Text='Text' />
						<Message Text='Text' Condition='$(Property)' ContinueOnError='$(Property)' />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target[] t = new Target [1];
			BuildTask[] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);

			Assert.AreEqual (String.Empty, bt [0].Condition, "A1");
			Assert.IsFalse (bt [0].ContinueOnError, "A2");
			Assert.IsNull (bt [0].HostObject, "A3");
			Assert.AreEqual ("Message", bt [0].Name, "A4");
			Assert.IsNotNull (bt [0].Type, "A5");

			Assert.AreEqual ("$(Property)", bt [1].Condition, "A6");
			Assert.IsTrue (bt [1].ContinueOnError, "A7");
			Assert.IsNull (bt [1].HostObject, "A8");
			Assert.AreEqual ("Message", bt [0].Name, "A9");
			Assert.IsNotNull (bt [0].Type, "A10");
		}

		[Test]
		public void TestGetParameterNames ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T'>
						<Message Text='Text' Importance='low' Condition='true' ContinueOnError='true' />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target[] t = new Target [1];
			BuildTask[] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);

			string[] names = bt [0].GetParameterNames ();

			Assert.AreEqual (2, names.Length, "A1");
			Assert.AreEqual ("Text", names [0], "A2");
			Assert.AreEqual ("Importance", names [1], "A3");
		}

		[Test]
		public void TestGetParameterValue1 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T'>
						<Message Text='$(A)' Condition='true' />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target[] t = new Target [1];
			BuildTask[] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);

			Assert.AreEqual (String.Empty, bt [0].GetParameterValue ("text"), "A1");
			Assert.AreEqual (String.Empty, bt [0].GetParameterValue (null), "A2");
			Assert.AreEqual ("$(A)", bt [0].GetParameterValue ("Text"), "A3");
		}

		[Test]
		public void TestGetParameterValue2 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T'>
						<Message />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target[] t = new Target [1];
			BuildTask[] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);

			{
				bool exception = false;

				try {
					bt [0].GetParameterValue ("Condition");
				} catch (ArgumentException) {
					exception = true;
				}

				Assert.IsTrue (exception, "A1");
			}
			{
				bool exception = false;

				try {
					bt [0].GetParameterValue ("ContinueOnError");
				} catch (ArgumentException) {
					exception = true;
				}

				Assert.IsTrue (exception, "A2");
			}
		}

		[Test]
		public void TestSetParameterValue1 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T'>
						<Message />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target[] t = new Target [1];
			BuildTask[] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);

			bt [0].SetParameterValue ("Text", "Value");
			Assert.AreEqual ("Value", bt [0].GetParameterValue ("Text"), "A1");
			bt [0].SetParameterValue ("text", "Value");
			Assert.AreEqual ("Value", bt [0].GetParameterValue ("text"), "A2");
			bt [0].SetParameterValue ("something", "Value");
			Assert.AreEqual ("Value", bt [0].GetParameterValue ("something"), "A3");
			bt [0].SetParameterValue ("Text", "$(A)");
			Assert.AreEqual ("$(A)", bt [0].GetParameterValue ("Text"), "A4");
		}

		[Test]
		public void TestSetParameterValue2 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T'>
						<Message />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target[] t = new Target [1];
			BuildTask[] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);

			bt [0].SetParameterValue ("Text", "$(A)", true);
			Assert.AreEqual (Utilities.Escape ("$(A)"), bt [0].GetParameterValue ("Text"), "A1");
			bt [0].SetParameterValue ("Text", "$(A)", false);
			Assert.AreEqual ("$(A)", bt [0].GetParameterValue ("Text"), "A2");
		}

		[Test]
		public void TestProperties ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T'>
						<Message />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target[] t = new Target [1];
			BuildTask[] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);

			bt [0].Condition = null;
			Assert.AreEqual (String.Empty, bt [0].Condition, "A1");
			bt [0].Condition = "something";
			Assert.AreEqual ("something", bt [0].Condition, "A2");
			
			bt [0].ContinueOnError = true;
			Assert.IsTrue (bt [0].ContinueOnError, "A3");
			bt [0].ContinueOnError = false;
			Assert.IsFalse (bt [0].ContinueOnError, "A4");
		}

		[Test]
		public void TestAddOutputItem1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T'>
						<Message />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target [] t = new Target [1];
			BuildTask [] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);

			bt [0].AddOutputItem (null, null);
		}

		[Test]
		public void TestAddOutputItem2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<UsingTask
						AssemblyFile='Test\resources\TestTasks.dll'
						TaskName='OutputTestTask'
					/>
					<Target Name='T'>
						<OutputTestTask />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target [] t = new Target [1];
			BuildTask [] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);

			bt [0].AddOutputItem ("Property", "ItemName");
			project.Build ("T");

			Assert.IsNotNull (project.EvaluatedItems [0], "No items found");
			Assert.AreEqual ("ItemName", project.EvaluatedItems [0].Name, "A1");
			Assert.AreEqual ("some_text", project.EvaluatedItems [0].FinalItemSpec, "A2");
        }

        [Test]
        public void TestTaskInNamespace()
        {
            Engine engine;
            Project project;

            string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<UsingTask
						AssemblyFile='Test\resources\TestTasks.dll'
						TaskName='NamespacedOutputTestTask'
					/>
					<Target Name='T'>
						<NamespacedOutputTestTask />
					</Target>
				</Project>
			";

            engine = new Engine(Consts.BinPath);
            project = engine.CreateNewProject();
            project.LoadXml(documentString);

            Target[] t = new Target[1];
            BuildTask[] bt;
            project.Targets.CopyTo(t, 0);
            bt = GetTasks(t[0]);

            bt[0].AddOutputItem("Property", "ItemName");
            project.Build("T");

            Assert.AreEqual("ItemName", project.EvaluatedItems[0].Name, "A1");
            Assert.AreEqual("some_text", project.EvaluatedItems[0].FinalItemSpec, "A2");
        }

		[Test]
		public void TestAddOutputProperty1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T'>
						<Message />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target [] t = new Target [1];
			BuildTask [] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);

			bt [0].AddOutputProperty (null, null);
		}

		[Test]
		public void TestAddOutputProperty2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<UsingTask
						AssemblyFile='Test\resources\TestTasks.dll'
						TaskName='OutputTestTask'
					/>
					<Target Name='T'>
						<OutputTestTask />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target [] t = new Target [1];
			BuildTask [] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);
			bt [0].AddOutputProperty ("Property", "PropertyName");
			project.Build ("T");

			Assert.AreEqual ("some_text", project.EvaluatedProperties ["PropertyName"].Value, "A1");
			Assert.AreEqual ("some_text", project.EvaluatedProperties ["PropertyName"].FinalValue, "A1");
		}

		// FIXME: edit
		public void TestPublish1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<UsingTask
						AssemblyFile='Test\resources\TestTasks.dll'
						TaskName='OutputTestTask'
					/>
					<Target Name='T'>
						<OutputTestTask />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target [] t = new Target [1];
			BuildTask [] bt;
			project.Targets.CopyTo (t, 0);
			bt = GetTasks (t [0]);
			bt [0].AddOutputProperty ("Property", "PropertyName");
			project.Build ("T");

			Assert.AreEqual ("some_text", project.EvaluatedProperties ["PropertyName"].Value, "A1");
			Assert.AreEqual ("some_text", project.EvaluatedProperties ["PropertyName"].FinalValue, "A1");
		}
	}
}
