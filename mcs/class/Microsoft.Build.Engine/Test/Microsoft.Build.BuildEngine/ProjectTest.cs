//
// ProjectTest.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
//
// (C) 2005 Marek Sieradzki
// Copyright 2009 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Xml;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;
using System.Text;

using MBT = MonoTests.Microsoft.Build.Tasks;

namespace MonoTests.Microsoft.Build.BuildEngine {

	class TestLogger : Logger {
		int target_started_events = 0;
		int target_finished_events = 0;

		public override void Initialize (IEventSource eventSource)
		{
			eventSource.TargetStarted += new TargetStartedEventHandler(TargetStarted);
			eventSource.TargetFinished += new TargetFinishedEventHandler(TargetFinished);
			eventSource.MessageRaised += new BuildMessageEventHandler(Message);
			eventSource.WarningRaised += new BuildWarningEventHandler(Warning);
		}

		void TargetStarted (object sender, TargetStartedEventArgs args)
		{
			target_started_events++;
		}

		void TargetFinished (object sender, TargetFinishedEventArgs args)
		{
			target_finished_events++;
		}

		void Message (object sender, BuildMessageEventArgs args)
		{
		}
		
		void Warning (object sender, BuildWarningEventArgs args)
		{
		}

		public int TargetStartedEvents { get { return target_started_events; } }

		public int TargetFinishedEvents { get { return target_finished_events; } }
	}

	[TestFixture]
	public class ProjectTest {

		/*
		Import [] GetImports (ImportCollection ic)
		{
			List<Import> list = new List<Import> ();
			foreach (Import i in ic)
				list.Add (i);
			return list.ToArray ();
		}
		*/

		[Test]
		public void TestAssignment1 ()
		{
			Engine engine;
			Project project;
			string documentString =
				"<Project></Project>";
			
			engine = new Engine (Consts.BinPath);

			DateTime time = DateTime.Now;
			project = engine.CreateNewProject ();
			try {
				project.LoadXml (documentString);
			} catch (InvalidProjectFileException) {
				Assert.AreEqual (true, project.BuildEnabled, "A1");
				Assert.AreEqual (String.Empty, project.DefaultTargets, "A2");
				Assert.AreEqual (String.Empty, project.FullFileName, "A3");
				Assert.AreEqual (false, project.IsDirty, "A4");
				Assert.AreEqual (false, project.IsValidated, "A5");
				Assert.AreEqual (engine, project.ParentEngine, "A6");
				Console.WriteLine ("time: {0} p.t: {1}", time, project.TimeOfLastDirty);
				Assert.IsTrue (time <= project.TimeOfLastDirty, "A7");
				Assert.IsTrue (String.Empty != project.Xml, "A8");
				return;
			}

			Assert.Fail ("Expected InvalidProjectFileException");
		}

		[Test]
		public void TestAssignment2 ()
		{
			Engine engine;
			Project project;
			string documentString =
				"<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"></Project>";
			
			engine = new Engine (Consts.BinPath);
			DateTime time = DateTime.Now;
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (true, project.BuildEnabled, "A1");
			Assert.AreEqual (String.Empty, project.DefaultTargets, "A2");
			Assert.AreEqual (String.Empty, project.FullFileName, "A3");
			Assert.AreEqual (true, project.IsDirty, "A4");
			Assert.AreEqual (false, project.IsValidated, "A5");
			Assert.AreEqual (engine, project.ParentEngine, "A6");
			Assert.IsTrue (time <= project.TimeOfLastDirty, "A7");
			Assert.IsTrue (String.Empty != project.Xml, "A8");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestAddNewImport1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup />
					<ItemGroup />
					<Target Name='a' />
					<Import Project='Test/resources/Import.csproj' />
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.AddNewImport ("a", "true");
			// reevaluation wasn't caused by anything so it has only old import
			Assert.AreEqual (1, project.Imports.Count, "A1");
		}

		[Test]
		[Ignore ("Too detailed probably (implementation specific)")]
		public void TestAddNewItem1 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItem item = project.AddNewItem ("A", "B");

			Assert.AreEqual (1, project.ItemGroups.Count, "A1");
			project.ItemGroups.CopyTo (groups, 0);
			Assert.AreEqual (1, groups [0].Count, "A2");
			Assert.AreEqual ("B", groups [0] [0].Include, "A3");
			Assert.AreEqual ("B", groups [0] [0].FinalItemSpec, "A4");
			Assert.AreEqual ("A", groups [0] [0].Name, "A5");
			//Assert.AreNotSame (item, groups [0] [0], "A6");
			Assert.IsFalse (object.ReferenceEquals (item, groups [0] [0]), "A6");

			Assert.AreEqual (1, project.EvaluatedItems.Count, "A7");
			Assert.AreEqual ("B", project.EvaluatedItems [0].Include, "A8");
			Assert.AreEqual ("B", project.EvaluatedItems [0].FinalItemSpec, "A9");
			Assert.AreEqual ("A", project.EvaluatedItems [0].Name, "A10");
			//Assert.AreNotSame (item, project.EvaluatedItems [0], "A11");
			Assert.IsFalse (object.ReferenceEquals (item, project.EvaluatedItems [0]), "A11");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestAddNewItem2 ()
		{
			Engine engine;
			Project project;

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			BuildItem item = project.AddNewItem ("A", "a;b;c");
			Assert.AreEqual ("a;b;c", item.Include, "A1");
			Assert.AreEqual ("a", item.FinalItemSpec, "A2");

			Assert.AreEqual (3, project.EvaluatedItems.Count, "A3");
		}

		[Test]
		public void TestAddNewItem3 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [4];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup />
					<ItemGroup>
						<A Include='a'/>
					</ItemGroup>
					<ItemGroup>
						<B Include='a'/>
					</ItemGroup>
					<ItemGroup>
						<B Include='a'/>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.AddNewItem ("B", "b");

			project.ItemGroups.CopyTo (groups, 0);
			Assert.AreEqual (0, groups [0].Count, "A1");
			Assert.AreEqual (1, groups [1].Count, "A2");
			Assert.AreEqual (1, groups [2].Count, "A3");
			Assert.AreEqual (2, groups [3].Count, "A4");
		}
		[Test]
		public void TestAddNewItemGroup ()
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

			BuildItemGroup big = project.AddNewItemGroup ();
			Assert.IsNotNull (big, "A1");
			Assert.AreEqual (String.Empty, big.Condition, "A2");
			Assert.AreEqual (0, big.Count, "A3");
			Assert.AreEqual (false, big.IsImported, "A4");
			Assert.IsTrue (project.IsDirty, "A5");
		}

		[Test]
		public void TestAddNewPropertyGroup ()
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

			BuildPropertyGroup bpg = project.AddNewPropertyGroup (false);
			Assert.IsNotNull (bpg, "A1");
			Assert.AreEqual (String.Empty, bpg.Condition, "A2");
			Assert.AreEqual (0, bpg.Count, "A3");
			Assert.AreEqual (false, bpg.IsImported, "A4");
			Assert.IsTrue (project.IsDirty, "A5");
		}

		[Test]
		public void TestBuild0 ()
		{
			Engine engine;
			Project project;
			IDictionary hashtable = new Hashtable ();

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target 
						Name='Main'
						Inputs='a;b;c'
						Outputs='d;e;f'
					>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (true, project.Build (new string [] { "Main" }, hashtable), "A1");
			Assert.AreEqual (1, hashtable.Count, "A2");

			IDictionaryEnumerator e = hashtable.GetEnumerator ();
			e.MoveNext ();

			string name = (string) e.Key;
			Assert.AreEqual ("Main", name, "A3");
			ITaskItem [] arr = (ITaskItem []) e.Value;

			Assert.AreEqual (3, arr.Length, "A4");
			Assert.AreEqual ("d", arr [0].ItemSpec, "A5");
			Assert.AreEqual ("e", arr [1].ItemSpec, "A6");
			Assert.AreEqual ("f", arr [2].ItemSpec, "A7");
		}

		[Test]
		public void TestBuild1 ()
		{
			Engine engine;
			Project project;
			IDictionary hashtable = new Hashtable ();
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='Main'>
						<Microsoft.Build.Tasks.Message Text='Text' />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (true, project.Build (new string[] { "Main" }, hashtable), "A1");
			Assert.AreEqual (1, hashtable.Count, "A2");

			IDictionaryEnumerator e = hashtable.GetEnumerator ();
			e.MoveNext ();

			string name = (string) e.Key;
			Assert.AreEqual ("Main", name, "A3");
			Assert.IsNotNull ((ITaskItem []) e.Value, "A4");
		}

		[Test]
		public void TestBuild2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T'>
						<Message Text='text' />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			MBT.TestMessageLogger tl = new MBT.TestMessageLogger();
			engine.RegisterLogger (tl);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.Build ("T");
			project.Build ("T");

			Assert.AreEqual (2, tl.TargetStarted, "A1");
			Assert.AreEqual (2, tl.TargetFinished, "A2");
			Assert.AreEqual (2, tl.TaskStarted, "A3");
			Assert.AreEqual (2, tl.TaskFinished, "A4");
		}

		[Test]
		public void TestBuild3 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T'>
						<Message Text='text' />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			MBT.TestMessageLogger tl = new MBT.TestMessageLogger ();
			engine.RegisterLogger (tl);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.Build (new string [1] { "T" }, null, BuildSettings.None);
			project.Build (new string [1] { "T" }, null, BuildSettings.None);

			Assert.AreEqual (2, tl.TargetStarted, "A1");
			Assert.AreEqual (2, tl.TargetFinished, "A2");
			Assert.AreEqual (2, tl.TaskStarted, "A3");
			Assert.AreEqual (2, tl.TaskFinished, "A4");
		}

		[Test]
		public void TestBuild4 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T'>
						<Message Text='text' />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			MBT.TestMessageLogger tl = new MBT.TestMessageLogger ();
			engine.RegisterLogger (tl);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.Build (new string [1] { "T" }, null, BuildSettings.DoNotResetPreviouslyBuiltTargets);
			project.Build (new string [1] { "T" }, null, BuildSettings.DoNotResetPreviouslyBuiltTargets);

			Assert.AreEqual (1, tl.TargetStarted, "A1");
			Assert.AreEqual (1, tl.TargetFinished, "A2");
			Assert.AreEqual (1, tl.TaskStarted, "A3");
			Assert.AreEqual (1, tl.TaskFinished, "A4");
		}

		[Test]
		public void TestBuild5 ()
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

			Assert.IsFalse (project.Build ("target_that_doesnt_exist"));
		}

		[Test]
		public void TestEvaluatedItems1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a' />
						<B Include='b' Condition='false' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (1, project.EvaluatedItems.Count, "A1");

			BuildItem bi = project.EvaluatedItems [0];

			bi.Name = "C";
			bi.Include = "c";

			BuildItemGroup [] big = new BuildItemGroup [1];
			project.ItemGroups.CopyTo (big, 0);
			Assert.AreEqual ("C", big [0] [0].Name, "A2");
			Assert.AreEqual ("c", big [0] [0].Include, "A3");
		}

		[Test]
		public void TestEvaluatedItems2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a;b;c' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup [] big = new BuildItemGroup [1];
			project.ItemGroups.CopyTo (big, 0);

			Assert.AreEqual (3, project.EvaluatedItems.Count, "A1");
			Assert.AreEqual ("a;b;c", big [0] [0].Include, "A2");
			Assert.AreEqual (1, big [0].Count, "A3");

			BuildItem bi = project.EvaluatedItems [0];

			bi.Include = "d";

			Assert.AreEqual (3, big [0].Count, "A4");
			Assert.AreEqual ("d", big [0] [0].Include, "A5");
			Assert.AreEqual ("b", big [0] [1].Include, "A6");
			Assert.AreEqual ("c", big [0] [2].Include, "A7");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestGetConditionedPropertyValues ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup Condition='true'>
						<A>A</A>
						<B Condition='true'>A</B>
					</PropertyGroup>
					<PropertyGroup>
						<C Condition='true'>A</C>
						<C Condition='false'>B</C>
						<C Condition='!false'>C</C>
						<D>A</D>
						<E Condition="" '$(C)' == 'A' "">E</E>
					</PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (0, project.GetConditionedPropertyValues ("A").Length, "A1");
			Assert.AreEqual (0, project.GetConditionedPropertyValues ("B").Length, "A2");
			Assert.AreEqual (1, project.GetConditionedPropertyValues ("C").Length, "A3");
			Assert.AreEqual (0, project.GetConditionedPropertyValues ("D").Length, "A4");
			Assert.AreEqual (0, project.GetConditionedPropertyValues ("E").Length, "A5");
			Assert.AreEqual ("A", project.GetConditionedPropertyValues ("C") [0], "A6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetEvaluatedItemsByName1 ()
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

			project.GetEvaluatedItemsByName (null);
		}

		[Test]
		public void TestGetEvaluatedItemsByName2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='1' />
						<B Include='2' Condition='true' />
						<C Include='3' Condition='false' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup big;

			big = project.GetEvaluatedItemsByName (String.Empty);

			Assert.AreEqual (0, big.Count, "A1");

			big = project.GetEvaluatedItemsByName ("A");

			Assert.AreEqual (1, big.Count, "A2");
			Assert.AreEqual ("1", big [0].FinalItemSpec, "A3");

			big = project.GetEvaluatedItemsByName ("B");

			Assert.AreEqual (1, big.Count, "A4");
			Assert.AreEqual ("2", big [0].FinalItemSpec, "A5");

			big = project.GetEvaluatedItemsByName ("C");

			Assert.AreEqual (0, big.Count, "A6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetEvaluatedItemsByNameIgnoringCondition1 ()
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

			project.GetEvaluatedItemsByNameIgnoringCondition (null);
		}

		[Test]
		public void TestGetEvaluatedItemsByNameIgnoringCondition2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='1' />
						<B Include='2' Condition='true' />
						<C Include='3' Condition='false' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup big;

			big = project.GetEvaluatedItemsByNameIgnoringCondition (String.Empty);

			Assert.AreEqual (0, big.Count, "A1");

			big = project.GetEvaluatedItemsByNameIgnoringCondition ("A");

			Assert.AreEqual (1, big.Count, "A2");
			Assert.AreEqual ("1", big [0].FinalItemSpec, "A3");

			big = project.GetEvaluatedItemsByNameIgnoringCondition ("B");

			Assert.AreEqual (1, big.Count, "A4");
			Assert.AreEqual ("2", big [0].FinalItemSpec, "A5");

			big = project.GetEvaluatedItemsByNameIgnoringCondition ("C");

			Assert.AreEqual (1, big.Count, "A6");
			Assert.AreEqual ("3", big [0].FinalItemSpec, "A7");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGetEvaluatedProperty1 ()
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

			project.GetEvaluatedProperty (null);
		}
		[Test]
		public void TestGetEvaluatedProperty2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<A>1</A>
						<B Condition='true'>2</B>
						<C Condition='false'>3</C>
					</PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual ("1", project.GetEvaluatedProperty ("A"), "A1");
			Assert.AreEqual ("2", project.GetEvaluatedProperty ("B"), "A2");
			Assert.IsNull (project.GetEvaluatedProperty ("C"), "A3");
		}

		[Test]
		public void TestGetProjectExtensions ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ProjectExtensions>
						<Node>Text</Node>
					</ProjectExtensions>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (String.Empty, project.GetProjectExtensions (null), "A1");
			Assert.AreEqual (String.Empty, project.GetProjectExtensions (String.Empty), "A2");
			Assert.AreEqual (String.Empty, project.GetProjectExtensions ("something"), "A3");
			Assert.AreEqual ("Text", project.GetProjectExtensions ("Node"), "A4");
		}

		[Test]
		public void TestGlobalProperties1 ()
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

			Assert.AreEqual (0, project.GlobalProperties.Count, "A1");
		}

		[Test]
		public void TestGlobalProperties2 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			engine.GlobalProperties.SetProperty ("Property", "Value");
			
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (1, project.GlobalProperties.Count, "A1");
			Assert.AreEqual ("Property", project.GlobalProperties ["Property"].Name, "A2");
			Assert.AreEqual ("Value", project.GlobalProperties ["Property"].Value, "A3");
			Assert.AreEqual ("Value", project.GlobalProperties ["Property"].FinalValue, "A4");
			Assert.AreEqual ("Property", project.EvaluatedProperties ["Property"].Name, "A2");
			Assert.AreEqual ("Value", project.EvaluatedProperties ["Property"].Value, "A3");
			Assert.AreEqual ("Value", project.EvaluatedProperties ["Property"].FinalValue, "A4");
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestGlobalProperties3 ()
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

			project.GlobalProperties = null;
		}

		[Test]
		[Ignore ("needs rewriting")]
		public void TestGlobalProperties4 ()
		{
			Engine engine;
			Project project;
			
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

			BuildPropertyGroup[] groups = new BuildPropertyGroup [1];
			project.PropertyGroups.CopyTo (groups, 0);

			project.GlobalProperties = groups [0];
			project.GlobalProperties = project.EvaluatedProperties;
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestGlobalProperties5 ()
		{
			Engine engine;
			Project project;
			
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

			BuildPropertyGroup[] groups = new BuildPropertyGroup [1];
			project.PropertyGroups.CopyTo (groups, 0);
			project.GlobalProperties = groups [0];
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestLoad1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestLoad2 ()
		{
			Engine engine;
			Project project;

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml ("project_file_that_doesnt_exist");
		}

		[Test]
		public void TestParentEngine ()
		{
			Engine engine;
			Project project;
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			Assert.AreEqual (engine, project.ParentEngine, "A1");
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveItemGroup1 ()
		{
			Engine engine;
			Project p1;

			engine = new Engine (Consts.BinPath);
			p1 = engine.CreateNewProject ();

			p1.RemoveItemGroup (null);
		}

		// The "BuildItemGroup" object specified does not belong to the correct "Project" object.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void TestRemoveItemGroup2 ()
		{
			Engine engine;
			Project p1;
			Project p2;
			BuildItemGroup [] groups  = new BuildItemGroup [1];

			engine = new Engine (Consts.BinPath);
			p1 = engine.CreateNewProject ();
			p2 = engine.CreateNewProject ();

			p1.AddNewItem ("A", "B");
			p1.ItemGroups.CopyTo (groups, 0);

			p2.RemoveItemGroup (groups [0]);
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveItem1 ()
		{
			Engine engine;
			Project project;

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			project.RemoveItem (null);
		}

		// The object passed in is not part of the project.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestRemoveItem2 ()
		{
			Engine engine;
			Project project;

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			project.RemoveItem (new BuildItem ("name", "include"));
		}

		// The "BuildItemGroup" object specified does not belong to the correct "Project" object.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestRemoveItem3 ()
		{
			Engine engine;
			Project p1;
			Project p2;

			engine = new Engine (Consts.BinPath);
			p1 = engine.CreateNewProject ();
			p2 = engine.CreateNewProject ();

			p1.AddNewItem ("A", "B");

			p2.RemoveItem (p1.EvaluatedItems [0]);
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestRemoveItem4 ()
		{
			Engine engine;
			Project p1;
			Project p2;

			engine = new Engine (Consts.BinPath);
			p1 = engine.CreateNewProject ();
			p2 = engine.CreateNewProject ();

			p1.AddNewItem ("A", "B");
			p1.AddNewItem ("A", "C");

			p2.RemoveItem (p1.EvaluatedItems [0]);
		}

		[Test]
		public void TestRemoveItem5 ()
		{
			Engine engine;
			Project project;
			BuildItemGroup [] groups = new BuildItemGroup [1];

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a'/>
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.RemoveItem (project.EvaluatedItems [0]);
			Assert.AreEqual (0, project.EvaluatedItems.Count, "A1");
			project.ItemGroups.CopyTo (groups, 0);
			Assert.IsNull (groups [0], "A2");
			Assert.AreEqual (0, project.ItemGroups.Count, "A3");
		}

		[Test]
		public void TestResetBuildStatus ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T' Inputs='Test\resources\TestTasks.cs' Outputs='Test\resources\TestTasks.dll'>
						<Message Text='text' />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			TestLogger tl = new TestLogger ();
			engine.RegisterLogger (tl);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.Build ("T");
			project.ResetBuildStatus ();
			project.Build (new string [1] { "T" }, null, BuildSettings.DoNotResetPreviouslyBuiltTargets);
			project.ResetBuildStatus ();
			project.Build (new string [1] { "T" }, null, BuildSettings.DoNotResetPreviouslyBuiltTargets);

			Assert.AreEqual (3, tl.TargetStartedEvents, "A1");
			Assert.AreEqual (3, tl.TargetFinishedEvents, "A1");
		}
		
		[Test]
		public void TestSchemaFile ()
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

			Assert.IsNull (project.SchemaFile, "A1");
		}
		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestSetProjectExtensions1 ()
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

			project.SetProjectExtensions (null, null);
		}

		[Test]
		public void TestSetProjectExtensions2 ()
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

			project.SetProjectExtensions ("name", "1");
			Assert.AreEqual ("1", project.GetProjectExtensions ("name"), "A1");
			project.SetProjectExtensions ("name", "2");
			Assert.AreEqual ("2", project.GetProjectExtensions ("name"), "A2");
			Assert.IsTrue (project.IsDirty, "A3");
		}

		[Test]
		public void TestSetProjectExtensions3 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ProjectExtensions>
					</ProjectExtensions>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.SetProjectExtensions ("name", "1");
			Assert.AreEqual ("1", project.GetProjectExtensions ("name"), "A1");
			Assert.IsTrue (project.IsDirty, "A2");
		}

		[Test]
		public void TestBuildProjectError1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			Assert.IsFalse (project.Build ((string) null), "A1");
			Assert.IsFalse (project.Build ((string[]) null), "A2");
			Assert.IsFalse (project.Build ((string []) null, null), "A3");
			Assert.IsFalse (project.Build ((string []) null, null, BuildSettings.None), "A4");
		}

		[Test]
		public void TestBuildProjectError2 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			try {
				project.Build (new string [] { null });
			} catch {
				return;
			}
			Assert.Fail ("Expected exception for project.Build, null string in targetNames []");
		}

		[Test]
		public void TestBuildProjectFile1 ()
		{
			Project project = CreateAndLoadProject ("foo.proj", false, new string [] { "1", "2" }, new bool [] { true, true }, "TBPF1");
			CheckProjectBuild (project, new string [] { "main" }, true, new string [] { "main" }, "TBPF1");
		}

		[Test]
		public void TestBuildProjectFileXml1 ()
		{
			Project project = CreateAndLoadProject (null, false, new string [] { "1", "2" }, new bool [] { true, true }, "TBPFX1");
			CheckProjectBuild (project, new string [] { "main" }, true, new string [] { "main" }, "TBPFX1");
		}

		[Test]
		public void TestBuildProjectFile2 ()
		{
			Project project = CreateAndLoadProject ("foo.proj", false, new string [] { "1", "2", "3" }, new bool [] { true, false, true }, "TBPF2");
			CheckProjectBuild (project, new string [] { "main" }, false, new string [0], "TBPF2");
		}

		[Test]
		public void TestBuildProjectFileXml2 ()
		{
			Project project = CreateAndLoadProject (null, false, new string [] { "1", "2", "3" }, new bool [] { true, false, true }, "TBPFX2");
			CheckProjectBuild (project, new string [] { "main" }, false, new string [0], "TBPFX2");
		}

		[Test]
		public void TestBuildProjectFile3 ()
		{
			Project project = CreateAndLoadProject ("foo.proj", false, new string [] { "1", "2", "3" }, new bool [] { true, true, true }, "TBPF3");
			CheckProjectBuild (project, new string [] { "1", "2" }, true, new string [] { "1", "2" }, "TBPF3");
		}

		[Test]
		public void TestBuildProjectFileXml3 ()
		{
			Project project = CreateAndLoadProject (null, false, new string [] { "1", "2", "3" }, new bool [] { true, true, true }, "TBPFX3");
			CheckProjectBuild (project, new string [] { "1", "2" }, true, new string [] { "1", "2" }, "TBPFX3");
		}

		[Test]
		public void TestBuildProjectFile4 ()
		{
			Project project = CreateAndLoadProject ("foo.proj", false, new string [] { "1", "2", "3" }, new bool [] { true, false, true }, "TBPF4");
			CheckProjectBuild (project, new string [] { "main" }, false, new string [0], "TBPF4");
		}

		[Test]
		public void TestBuildProjectFileXml4 ()
		{
			Project project = CreateAndLoadProject (null, false, new string [] { "1", "2", "3" }, new bool [] { true, false, true }, "TBPFX4");
			CheckProjectBuild (project, new string [] { "main" }, false, new string [0], "TBPFX4");
		}

		//Run separate tests

		//Run single target
		[Test]
		public void TestBuildProjectFile5 ()
		{
			Project project = CreateAndLoadProject ("foo.proj", true, new string [] { "1", "2", "3" }, new bool [] { true, false, true }, "TBPF5");
			CheckProjectBuild (project, new string [] { "main" }, false, new string [0], "TBPF5");
		}

		[Test]
		public void TestBuildProjectFileXml5 ()
		{
			Project project = CreateAndLoadProject (null, true, new string [] { "1", "2", "3" }, new bool [] { true, false, true }, "TBPFX5");
			CheckProjectBuild (project, new string [] { "main" }, false, new string [0], "TBPFX5");
		}

		[Test]
		public void TestBuildProjectFile6 ()
		{
			Project project = CreateAndLoadProject ("foo.proj", true, new string [] { "1", "2", "3" }, new bool [] { true, true, true }, "TBPF6");
			CheckProjectBuild (project, new string [] { "main" }, true, new string [] { "main" }, "TBPF6");
		}

		[Test]
		public void TestBuildProjectFileXml6 ()
		{
			Project project = CreateAndLoadProject (null, true, new string [] { "1", "2", "3" }, new bool [] { true, true, true }, "TBPFX6");
			CheckProjectBuild (project, new string [] { "main" }, true, new string [] { "main" }, "TBPFX6");
		}

		// run multiple targets
		[Test]
		public void TestBuildProjectFile7 ()
		{
			Project project = CreateAndLoadProject ("foo.proj", true, new string [] { "1", "2", "3" }, new bool [] { true, true, true }, "TBPF7");
			CheckProjectBuild (project, new string [] { "1", "2", "3" }, true, new string [] { "1", "2", "3" }, "TBPF7");
		}

		[Test]
		public void TestBuildProjectFileXml7 ()
		{
			Project project = CreateAndLoadProject (null, true, new string [] { "1", "2", "3" }, new bool [] { true, true, true }, "TBPFX7");
			CheckProjectBuild (project, new string [] { "1", "2", "3" }, true, new string [] { "1", "2", "3" }, "TBPFX7");
		}

		[Test]
		public void TestBuildProjectFile8 ()
		{
			Project project = CreateAndLoadProject ("foo.proj", true, new string [] { "1", "2", "3" }, new bool [] { true, true, false }, "TBPF8");
			CheckProjectBuild (project, new string [] { "1", "2", "3" }, false, new string [] { "1", "2"}, "TBPF8");
		}

		[Test]
		public void TestBuildProjectFileXml8 ()
		{
			Project project = CreateAndLoadProject (null, true, new string [] { "1", "2", "3" }, new bool [] { true, true, false }, "TBPFX8");
			CheckProjectBuild (project, new string [] { "1", "2", "3" }, false, new string [] { "1", "2"}, "TBPFX8");
		}

		[Test]
		public void TestBatchedMetadataRef1 ()
		{
			//test for multiple items with same metadata also
			 string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
			<UsingTask TaskName=""BatchingTestTask"" AssemblyFile=""Test/resources/TestTasks.dll"" />
			<ItemGroup>
				<Coll1 Include=""A1""><Name>Abc</Name></Coll1>
				<Coll1 Include=""A2""><Name>Def</Name></Coll1>
				<Coll1 Include=""A3""><Name>Xyz</Name></Coll1>
				<Coll1 Include=""A4""><Name>Xyz</Name></Coll1>
				<Coll2 Include=""B1""></Coll2>
			</ItemGroup>
				<Target Name=""ShowMessage"">
					<BatchingTestTask Sources=""%(Coll1.Name)"">
						<Output TaskParameter=""Output"" ItemName=""FinalList"" />
					</BatchingTestTask>
					<Message Text=""Msg: %(Coll1.Name)"" />
				</Target>
		 </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("ShowMessage"), "A1: Build failed");

			BuildItemGroup include = project.GetEvaluatedItemsByName ("FinalList");
			Assert.AreEqual (3, include.Count, "A2");

			Assert.AreEqual ("FinalList", include [0].Name, "A3");
			Assert.AreEqual ("Abc", include [0].FinalItemSpec, "A4");

			Assert.AreEqual ("FinalList", include [1].Name, "A5");
			Assert.AreEqual ("Def", include [1].FinalItemSpec, "A6");

			Assert.AreEqual ("FinalList", include [2].Name, "A7");
			Assert.AreEqual ("Xyz", include [2].FinalItemSpec, "A8");
		}

		[Test]
		public void TestBatchedMetadataRef2 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
			<UsingTask TaskName=""BatchingTestTask"" AssemblyFile=""Test/resources/TestTasks.dll"" />
			<ItemGroup>
				<Coll1 Include=""A1""><Name>Abc</Name></Coll1>
				<Coll1 Include=""A2""><Name>Def</Name></Coll1>
				<Coll1 Include=""A3""><Name>Xyz</Name></Coll1>
				<Coll1 Include=""A4""><Name>Abc</Name></Coll1>
				<Coll2 Include=""B1""><Name>Bar</Name></Coll2>
				<Coll2 Include=""B2""><Name>Bar</Name></Coll2>
			</ItemGroup>
				<Target Name=""ShowMessage"">
					<BatchingTestTask Sources=""%(Name)"" Strings=""@(Coll2)"">
						<Output TaskParameter=""Output"" ItemName=""FinalList"" />
					</BatchingTestTask>
					<Message Text=""Msg: %(Coll1.Name)"" />
				</Target>
				<Target Name=""ShowMessage2"">
					<BatchingTestTask Sources=""%(Name)"" Strings=""@(Coll1)"">
						<Output TaskParameter=""Output"" ItemName=""FinalList2"" />
					</BatchingTestTask>
					<Message Text=""Msg: %(Coll1.Name)"" />
				</Target>
		 </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("ShowMessage"), "A1: Build failed");

			BuildItemGroup include = project.GetEvaluatedItemsByName ("FinalList");
			Assert.AreEqual (1, include.Count, "A2");

			Assert.AreEqual ("FinalList", include [0].Name, "A3");
			Assert.AreEqual ("Bar", include [0].FinalItemSpec, "A4");

			Assert.IsTrue (project.Build ("ShowMessage2"), "A1: Build failed");
			include = project.GetEvaluatedItemsByName ("FinalList2");
			Assert.AreEqual (3, include.Count, "A5");

			Assert.AreEqual ("FinalList2", include [0].Name, "A6");
			Assert.AreEqual ("Abc", include [0].FinalItemSpec, "A7");

			Assert.AreEqual ("FinalList2", include [1].Name, "A8");
			Assert.AreEqual ("Def", include [1].FinalItemSpec, "A9");

			Assert.AreEqual ("FinalList2", include [2].Name, "A10");
			Assert.AreEqual ("Xyz", include [2].FinalItemSpec, "A11");
		}

		[Test]
		public void TestBatchedMetadataRef3 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
			<UsingTask TaskName=""BatchingTestTask"" AssemblyFile=""Test/resources/TestTasks.dll"" />
			<ItemGroup>
				<Coll1 Include=""A1""><Name>Abc</Name></Coll1>
				<Coll1 Include=""A2""><Name>Def</Name></Coll1>
				<Coll1 Include=""A3""><Name>Xyz</Name></Coll1>
				<Coll1 Include=""A4""><Name>Abc</Name></Coll1>
				<Coll2 Include=""B1""><Name>Bar</Name></Coll2>
				<Coll2 Include=""B2""><Name>Bar</Name></Coll2>
			</ItemGroup>
				<Target Name=""ShowMessage"">
					<BatchingTestTask SingleTaskItem=""%(Coll2.Name)"" >
						<Output TaskParameter=""SingleStringOutput"" ItemName=""FinalList"" />
					</BatchingTestTask>
					<Message Text=""Msg: %(Coll1.Name)"" />
				</Target>
		 </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("ShowMessage"), "A1: Build failed");

			BuildItemGroup include = project.GetEvaluatedItemsByName ("FinalList");
			Assert.AreEqual (1, include.Count, "A2");

			Assert.AreEqual ("FinalList", include [0].Name, "A3");
			Assert.AreEqual ("Bar", include [0].FinalItemSpec, "A4");

		}

		[Test]
		public void TestBatchedMetadataRef4 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
			<UsingTask TaskName=""BatchingTestTask"" AssemblyFile=""Test/resources/TestTasks.dll"" />
			<ItemGroup>
				<Coll1 Include=""A1""><Name>Abc</Name></Coll1>
				<Coll1 Include=""A2""><Name>Def</Name></Coll1>
				<Coll1 Include=""A3""><Name>Xyz</Name></Coll1>
				<Coll2 Include=""B1""><Name>Bar</Name></Coll2>
			</ItemGroup>
				<Target Name=""ShowMessage"">
					<BatchingTestTask SingleTaskItem=""%(Coll3.Name)"" >
						<Output TaskParameter=""SingleStringOutput"" ItemName=""FinalList"" />
					</BatchingTestTask>
				</Target>
		 </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("ShowMessage"), "A1: Build failed");

			BuildItemGroup include = project.GetEvaluatedItemsByName ("FinalList");
			Assert.AreEqual (0, include.Count, "A2");
		}

		[Test]
		public void TestBatchedMetadataRef5 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
			<UsingTask TaskName=""BatchingTestTask"" AssemblyFile=""Test/resources/TestTasks.dll"" />
			<ItemGroup>
				<Coll1 Include=""A1""><Name>Abc</Name></Coll1>
				<Coll1 Include=""A2""><Name>Def</Name></Coll1>
				<Coll1 Include=""A3""><Name>Xyz</Name></Coll1>
				<Coll2 Include=""B1""><Name>Bar</Name></Coll2>
			</ItemGroup>
				<Target Name=""ShowMessage"">
					<Message Text=""Coll1: @(Coll1->'Foo%(Name)Bar')"" />
					<BatchingTestTask Sources=""@(Coll1->'Foo%(Name)Bar')"" >
						<Output TaskParameter=""Output"" ItemName=""FinalList"" />
					</BatchingTestTask>
				</Target>
		 </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			project.LoadXml (projectString);
			bool result = project.Build ("ShowMessage");
			if (!result) {
				logger.DumpMessages ();
				Assert.Fail ("A1: Build failed");
			}
			logger.DumpMessages ();
			BuildItemGroup include = project.GetEvaluatedItemsByName ("FinalList");
			Assert.AreEqual (3, include.Count, "A2");
		}

		[Test]
		public void TestBatchedMetadataRefInOutput () {
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
			<UsingTask TaskName=""BatchingTestTask"" AssemblyFile=""Test/resources/TestTasks.dll"" />
			<ItemGroup>
				<Coll1 Include=""A1""><Name>Abc</Name></Coll1>
				<Coll1 Include=""A2""><Name>Def</Name></Coll1>
				<Coll1 Include=""A3""><Name>Abc</Name></Coll1>
				<Coll1 Include=""B1""><Name>Bar</Name></Coll1>
			</ItemGroup>
				<Target Name=""ShowMessage"">
					<BatchingTestTask Sources=""@(Coll1)"" >
						<Output TaskParameter=""Output"" ItemName=""AbcItems"" Condition=""'%(Coll1.Name)' == 'Abc'""/>
						<Output TaskParameter=""Output"" ItemName=""NonAbcItems"" Condition=""'%(Coll1.Name)' != 'Abc'""/>
					</BatchingTestTask>
					<Message Text='AbcItems: @(AbcItems)' />
					<Message Text='NonAbcItems: @(NonAbcItems)' />
				</Target>
		 </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			project.LoadXml (projectString);
			bool result = project.Build ("ShowMessage");
			if (!result) {
				logger.DumpMessages ();
				Assert.Fail ("A1: Build failed");
			}

			logger.CheckLoggedMessageHead ("AbcItems: A1;A3", "A2");
			logger.CheckLoggedMessageHead ("NonAbcItems: A2;B1", "A2");

			if (logger.NormalMessageCount != 0) {
				logger.DumpMessages ();
				Assert.Fail ("Unexpected extra messages found");
			}
		}

		[Test]
		public void TestInitialTargets ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			project.LoadXml (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" InitialTargets=""pre  "">
				<Target Name=""boo"">
					<Message Text=""Executing boo target""/>
				</Target>
				<Target Name=""pre"">
					<Message Text=""Executing pre target""/>
				</Target>
			</Project>");

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			try {
				Assert.IsTrue (project.Build (), "Build failed");

				logger.CheckLoggedMessageHead ("Executing pre target", "A1");
				logger.CheckLoggedMessageHead ("Executing boo target", "A2");

				Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");
			} catch {
				logger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestInitialTargetsWithImports () {
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			string second = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" InitialTargets=""One  "">
				<Target Name=""One"">
					<Message Text='Executing Second::One target'/>
				</Target>
				<Import Project='third.proj'/>
			</Project>
";
			using (StreamWriter sw = new StreamWriter (Path.Combine ("Test", Path.Combine ("resources", "second.proj")))) {
				sw.Write (second);
			}

			string third = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" InitialTargets=""Two"">
				<Target Name=""Two"">
					<Message Text='Executing Third::Two target'/>
				</Target>
			</Project>
";
			using (StreamWriter sw = new StreamWriter (Path.Combine ("Test", Path.Combine ("resources", "third.proj")))) {
				sw.Write (third);
			}

			project.LoadXml (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" InitialTargets=""pre"">
				<Target Name=""boo"">
					<Message Text=""Executing boo target""/>
				</Target>
				<Target Name=""pre"">
					<Message Text=""Executing pre target""/>
				</Target>
				<Import Project='Test/resources/second.proj'/>
			</Project>");

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			try {
				Assert.IsTrue (project.Build (), "Build failed");

				logger.CheckLoggedMessageHead ("Executing pre target", "A1");
				logger.CheckLoggedMessageHead ("Executing Second::One target", "A2");
				logger.CheckLoggedMessageHead ("Executing Third::Two target", "A3");
				logger.CheckLoggedMessageHead ("Executing boo target", "A4");
				Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");

				Assert.AreEqual ("pre; One; Two", project.InitialTargets, "List of initial targets");
			} catch {
				logger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestDefaultTargets () {
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			project.LoadXml (@"<Project DefaultTargets='pre' xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" >
				<Target Name=""boo"">
					<Message Text=""Executing boo target""/>
				</Target>
				<Target Name=""pre"">
					<Message Text=""Executing pre target""/>
				</Target>
			</Project>");

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			try {
				Assert.IsTrue (project.Build (), "Build failed");

				logger.CheckLoggedMessageHead ("Executing pre target", "A1");
				Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");

				Assert.AreEqual ("pre", project.DefaultTargets, "Default targets");
			} catch {
				logger.DumpMessages ();
				throw;
			}
		}


		[Test]
		public void TestDefaultTargetsWithImports () {
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			string second = @"<Project DefaultTargets='One' xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<Target Name=""One"">
					<Message Text='Executing Second::One target'/>
				</Target>
			</Project>";
			using (StreamWriter sw = new StreamWriter (Path.Combine ("Test", Path.Combine ("resources", "second.proj")))) {
				sw.Write (second);
			}

			project.LoadXml (@"<Project DefaultTargets='pre' xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" >
				<Target Name=""boo"">
					<Message Text=""Executing boo target""/>
				</Target>
				<Target Name=""pre"">
					<Message Text=""Executing pre target""/>
				</Target>
				<Import Project='Test/resources/second.proj'/>
			</Project>");

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			try {
				Assert.IsTrue (project.Build (), "Build failed");

				logger.CheckLoggedMessageHead ("Executing pre target", "A1");
				Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");

				Assert.AreEqual ("pre", project.DefaultTargets, "Default targets");
			} catch {
				logger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestNoDefaultTargetsWithImports () {
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();


			string second = @"<Project DefaultTargets='; One  ; Two' xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<Target Name=""One"">
					<Message Text='Executing Second::One target'/>
				</Target>
				<Target Name=""Two"">
					<Message Text='Executing Second::Two target'/>
				</Target>

			</Project>";
			using (StreamWriter sw = new StreamWriter (Path.Combine ("Test", Path.Combine ("resources", "second.proj")))) {
				sw.Write (second);
			}

			project.LoadXml (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" >
				<Target Name=""boo"">
					<Message Text=""Executing boo target""/>
				</Target>
				<Target Name=""pre"">
					<Message Text=""Executing pre target""/>
				</Target>
				<Import Project='Test/resources/second.proj'/>
			</Project>");

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			try {
				Assert.IsTrue (project.Build (), "Build failed");

				logger.CheckLoggedMessageHead ("Executing Second::One target", "A1");
				logger.CheckLoggedMessageHead ("Executing Second::Two target", "A2");
				Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");

				Assert.AreEqual ("One; Two", project.DefaultTargets, "Default targets");
			} catch {
				logger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestNoDefaultTargets () {
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			project.LoadXml (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" >
				<Target Name=""boo"">
					<Message Text=""Executing boo target""/>
				</Target>
				<Target Name=""pre"">
					<Message Text=""Executing pre target""/>
				</Target>
			</Project>");

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			try {
				Assert.IsTrue (project.Build (), "Build failed");

				logger.CheckLoggedMessageHead ("Executing boo target", "A1");
				Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");

				Assert.AreEqual ("", project.DefaultTargets, "Default targets");
			} catch {
				logger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestPropertiesFromImportedProjects ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			string second = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" " + Consts.ToolsVersionString + @">
	<PropertyGroup>
	  <Prop1>InitialVal</Prop1>
	</PropertyGroup>
	<ItemGroup>
		<Second Include=""$(ThirdProp):Third""/>
	</ItemGroup>

	<Target Name=""Main"">
		<Message Text=""Prop1: $(Prop1) FooItem: @(FooItem)""/>
		<Message Text=""Second: @(Second) ThirdProp: $(ThirdProp)""/>
	</Target>
	<Import Project=""third.proj""/>
</Project>";
			using (StreamWriter sw = new StreamWriter (Path.Combine ("Test", Path.Combine ("resources", "second.proj")))) {
				sw.Write (second);
			}

			string third = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" " + Consts.ToolsVersionString + @">
	<PropertyGroup>
	  <ThirdProp>Third Value</ThirdProp>
	</PropertyGroup>
</Project>";
			using (StreamWriter sw = new StreamWriter (Path.Combine ("Test", Path.Combine ("resources", "third.proj")))) {
				sw.Write (third);
			}

			project.LoadXml (@"<Project InitialTargets=""Main"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<ItemGroup>
		<FooItem Include=""$(Prop1):Something""/>
	</ItemGroup>

	<Import Project=""Test/resources/second.proj""/>
</Project>");

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			try {
				Assert.IsTrue (project.Build (), "Build failed");

				logger.CheckLoggedMessageHead ("Prop1: InitialVal FooItem: InitialVal:Something", "A1");
				logger.CheckLoggedMessageHead ("Second: Third Value:Third ThirdProp: Third Value", "A2");

				Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");
			} catch {
				logger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestMSBuildThisProperties ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			string base_dir = Path.GetFullPath (Path.Combine ("Test", "resources")) + Path.DirectorySeparatorChar;
			string tmp_dir = Path.GetFullPath (Path.Combine (base_dir, "tmp")) + Path.DirectorySeparatorChar;

			string first_project = Path.Combine (base_dir, "first.proj");
			string second_project = Path.Combine (tmp_dir, "second.proj");
			string third_project = Path.Combine (tmp_dir, "third.proj");

			string first = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" " + Consts.ToolsVersionString + @">
				<PropertyGroup>
					<FooInMain>$(MSBuildThisFileDirectory)</FooInMain>
				</PropertyGroup>
				<ItemGroup>
					<ItemInMain Include=""$(MSBuildThisFileFullPath)"" />
				</ItemGroup>
				<Import Project=""tmp\second.proj""/>
			</Project>";

			string second = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" " + Consts.ToolsVersionString + @">
				<PropertyGroup>
					<FooInImport1>$(MSBuildThisFileDirectory)</FooInImport1>
				</PropertyGroup>
				<PropertyGroup>
					<FooInImport2>$(MSBuildThisFileDirectory)</FooInImport2>
				</PropertyGroup>
				<ItemGroup>
					<ItemInImport1 Include=""$(MSBuildThisFileFullPath)"" />
				</ItemGroup>

				<Import Project=""third.proj""/>
			</Project>";

			string third = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" " + Consts.ToolsVersionString + @">
				<PropertyGroup>
					<FooInTwo>$(MSBuildThisFileFullPath)</FooInTwo>
				</PropertyGroup>
				<ItemGroup>
					<ItemInTwo Include=""$(MSBuildThisFileFullPath)"" />
				</ItemGroup>

				<Target Name=""TargetInTwo"">
					<Message Text=""FooInMain: $(FooInMain)""/>
					<Message Text=""FooInImport1: $(FooInImport1)""/>
					<Message Text=""FooInImport2: $(FooInImport2)""/>
					<Message Text=""FooInTwo: $(FooInTwo)""/>

					<Message Text=""ItemInMain: %(ItemInMain.Identity)""/>
					<Message Text=""ItemInImport1: %(ItemInImport1.Identity)""/>
					<Message Text=""ItemInTwo: %(ItemInTwo.Identity)""/>
					<Message Text=""Full path: $(MSBuildThisFileFullPath)""/>
				</Target>
			</Project>";

			File.WriteAllText (first_project, first);

			Directory.CreateDirectory (Path.Combine (base_dir, "tmp"));
			File.WriteAllText (second_project, second);
			File.WriteAllText (third_project, third);

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			project.Load (first_project);
			try {
				Assert.IsTrue (project.Build (), "Build failed");

				logger.CheckLoggedMessageHead ("FooInMain: " + base_dir, "A1");
				logger.CheckLoggedMessageHead ("FooInImport1: " + tmp_dir, "A2");
				logger.CheckLoggedMessageHead ("FooInImport2: " + tmp_dir, "A3");
				logger.CheckLoggedMessageHead ("FooInTwo: " + third_project, "A4");
				logger.CheckLoggedMessageHead ("ItemInMain: " + first_project, "A5");
				logger.CheckLoggedMessageHead ("ItemInImport1: " + second_project, "A6");
				logger.CheckLoggedMessageHead ("ItemInTwo: " + third_project, "A7");
				logger.CheckLoggedMessageHead ("Full path: " + third_project, "A8");

				Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");
			} catch {
				logger.DumpMessages ();
				throw;
			} finally {
				File.Delete (first_project);
				File.Delete (second_project);
				File.Delete (third_project);
			}
		}

		[Test]
		public void TestRequiredTask_String1 ()
		{
			CheckProjectForRequiredTests ("RequiredTestTask_String", "@(NonExistant)",
				false, "Should've failed: No value specified for required field - 'Property' of RequiredTestTask_String", null);
		}

		[Test]
		public void TestRequiredTask_String2 ()
		{
			CheckProjectForRequiredTests ("RequiredTestTask_String", "$(NonExistant)",
				false, "Should've failed: No value specified for required field - 'Property' of RequiredTestTask_String", null);
		}

		[Test]
		public void TestRequiredTask_Strings1 () {
			CheckProjectForRequiredTests ("RequiredTestTask_Strings", "@(NonExistant)",
				true, "Build failed", "0");
		}

		[Test]
		public void TestRequiredTask_Strings2 () {
			CheckProjectForRequiredTests ("RequiredTestTask_Strings", "$(NonExistant)",
				true, "Build failed", "0");
		}

		[Test]
		public void TestRequiredTask_Strings3 () {
			CheckProjectForRequiredTests ("RequiredTestTask_Strings", "%(NonExistant.Md)",
				true, "Build failed", "0");
		}

		[Test]
		public void TestRequiredTask_Strings4 () {
			CheckProjectForRequiredTests ("RequiredTestTask_Strings", "  %(NonExistant.Md)",
				true, "Build failed", "0");
		}

		[Test]
		public void TestRequiredTask_Ints1 () {
			CheckProjectForRequiredTests ("RequiredTestTask_IntArray", "@(NonExistant)",
				true, "Build failed", "count: 0");
		}

		[Test]
		public void TestRequiredTask_Ints2 () {
			CheckProjectForRequiredTests ("RequiredTestTask_IntArray", "$(NonExistant)",
				true, "Build failed", "count: 0");
		}

		[Test]
		public void TestRequiredTask_OtherObjectsArray () {
			CheckProjectForRequiredTests ("RequiredTestTask_OtherObjectArray", "@(NonExistant)",
				false, "Should've failed: ObjectArray type not supported as a property type", null);
		}

		[Test]
		public void TestRequiredTask_OtherObject () {
			CheckProjectForRequiredTests ("RequiredTestTask_OtherObjectArray", "@(NonExistant)",
				false, "Should've failed: ObjectArray type not supported as a property type", null);
		}

		[Test]
		public void TestRequiredTask_MyTaskItems1 () {
			CheckProjectForRequiredTests ("RequiredTestTask_MyTaskItemArray", "@(NonExistant)",
				false, "Should've failed: ObjectArray type not supported as a property type", null);
		}

		[Test]
		public void TestRequiredTask_TaskItem1 ()
		{
			Project p = CheckProjectForRequiredTests ("RequiredTestTask_TaskItem", "@(NonExistant)",
				false, "Should've failed: No value specified for required field - 'Property' of RequiredTestTask_TaskItem", null);
		}

		[Test]
		public void TestRequiredTask_TaskItem2 ()
		{
			Project p = CheckProjectForRequiredTests ("RequiredTestTask_TaskItem", "$(NonExistant)",
				false, "Should've failed: No value specified for required field - 'Property' of RequiredTestTask_TaskItem", null);
		}

		[Test]
		public void TestRequiredTask_TaskItemArray1 ()
		{
			Project p = CheckProjectForRequiredTests ("RequiredTestTask_TaskItems", "@(NonExistant)",
				true, "Build failed", "count: 0");

			BuildItemGroup group = p.GetEvaluatedItemsByName ("OutItem");
			Assert.AreEqual (1, group.Count, "A2");
			Assert.AreEqual ("count: 0", group [0].FinalItemSpec, "A3");
		}

		[Test]
		public void TestRequiredTask_TaskItemArray2 ()
		{
			Project p = CheckProjectForRequiredTests ("RequiredTestTask_TaskItems", "$(NonExistant)",
				true, "Build failed", "count: 0");

			BuildItemGroup group = p.GetEvaluatedItemsByName ("OutItem");
			Assert.AreEqual (1, group.Count, "A2");
			Assert.AreEqual ("count: 0", group [0].FinalItemSpec, "A3");
		}

		[Test]
		public void TestRequiredTask_TaskItemArray3 ()
		{
			Project p = CheckProjectForRequiredTests ("RequiredTestTask_IntArray", "$(NonExistant)",
				true, "Build failed", "count: 0");

			BuildItemGroup group = p.GetEvaluatedItemsByName ("OutItem");
			Assert.AreEqual (1, group.Count, "A2");
			Assert.AreEqual ("count: 0", group [0].FinalItemSpec, "A3");
		}

		[Test]
		public void TestRequiredTask_TaskItemArray4 () {
			Project p = CheckProjectForRequiredTests ("RequiredTestTask_IntArray", "%(NonExistant.Md)",
				true, "Build failed", "count: 0");

			BuildItemGroup group = p.GetEvaluatedItemsByName ("OutItem");
			Assert.AreEqual (1, group.Count, "A2");
			Assert.AreEqual ("count: 0", group[0].FinalItemSpec, "A3");
		}

		[Test]
		public void TestRequiredTask_TaskItemArray5 () {
			// with extra space in prop value
			Project p = CheckProjectForRequiredTests ("RequiredTestTask_IntArray", "  %(NonExistant.Md)",
				true, "Build failed", "count: 0");

			BuildItemGroup group = p.GetEvaluatedItemsByName ("OutItem");
			Assert.AreEqual (1, group.Count, "A2");
			Assert.AreEqual ("count: 0", group[0].FinalItemSpec, "A3");
		}


		[Test]
		public void TestCaseSensitivityOfProjectElements ()
		{
			string projectXml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" " + Consts.ToolsVersionString + @">
        <ItemGroup>
                <Abc Include=""foo"">
                        <MetaDaTA1>md1</MetaDaTA1>
                        <METadata2>md2</METadata2>
                </Abc>
                <Abc Include=""FOO"">
                        <MetaDaTA1>MD1 caps</MetaDaTA1>
                        <METadata2>MD2 caps</METadata2>
                </Abc>
                <Abc Include=""hmm"">
                        <MetaDaTA1>Md1 CAPS</MetaDaTA1>
                        <METadata2>MD2 CAPS</METadata2>
                </Abc>
                <Abc Include=""bar"">
                        <MeTAdata1>md3</MeTAdata1>
                        <Metadata2>md4</Metadata2>
                </Abc>
        </ItemGroup> 
        <PropertyGroup><ProP1>ValueProp</ProP1></PropertyGroup>
	<Target Name=""Main"">
		<MesSAGE Text=""Full item: @(ABC)""/>
		<MEssaGE Text=""metadata1 :%(AbC.MetaDATA1) metadata2: %(ABC.MetaDaTa2)""/>
		<MEssaGE Text=""metadata2 : %(AbC.MetaDAta2)""/>
		<MEssaGE Text=""Abc identity: %(ABC.IDENTitY)""/>
		<MEssaGE Text=""prop1 : $(pROp1)""/>
	</Target>
</Project>
";
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			project.LoadXml (projectXml);
			bool result = project.Build ("Main");
			if (!result) {
				logger.DumpMessages ();
				Assert.Fail ("A1: Build failed");
			}
			logger.DumpMessages ();

			logger.CheckLoggedMessageHead ("Full item: foo;FOO;hmm;bar", "#A2");
			logger.CheckLoggedMessageHead ("metadata1 :md1 metadata2: md2", "#A3");
			logger.CheckLoggedMessageHead ("metadata1 :MD1 caps metadata2: MD2 caps", "#A4");
			logger.CheckLoggedMessageHead ("metadata1 :md3 metadata2: md4", "#A5");
			logger.CheckLoggedMessageHead ("metadata2 : md2", "#A6");
			logger.CheckLoggedMessageHead ("metadata2 : MD2 caps", "#A7");
			logger.CheckLoggedMessageHead ("metadata2 : md4", "#A8");
			logger.CheckLoggedMessageHead ("Abc identity: foo", "#A9");
			logger.CheckLoggedMessageHead ("Abc identity: hmm", "#A10");
			logger.CheckLoggedMessageHead ("Abc identity: bar", "#A11");
			logger.CheckLoggedMessageHead ("prop1 : ValueProp", "#A12");

			Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");

		}

		// full solution test
		//[Test]
		public void TestBuildSolutionProject ()
		{
			string basepath = Path.Combine ("Test", Path.Combine ("resources", "Project01"));
			string [] project_dirs = new string [] {
				Path.Combine (basepath, "Lib4"),
				Path.Combine (basepath, "Lib3"),
				Path.Combine (basepath, "Lib2"),
				Path.Combine (basepath, "Lib1"),
				Path.Combine (basepath, "Project01")
			};
			string debug_extn = Consts.RunningOnMono () ? ".dll.mdb" : ".pdb";

			// List of expected output files
			// Lib3
			string [] [] project_files = new string [5][] {
				new string [] { "Lib4.dll", "Lib4" + debug_extn },
				new string [] { "Lib3.dll" , "Lib3" + debug_extn },
				// Lib2
				new string [] {
					"Lib2.dll", "Lib2" + debug_extn,
					"lib2_folder/Lib2.deploy.txt",
					Path.Combine ("fr-CA", "Lib2.resources.dll"),
					Path.Combine ("fr-FR", "Lib2.resources.dll"),
					"Lib4.dll", "Lib4" + debug_extn
				},
				
				// lib1
				new string [] {
					// lib1 files
					"Lib1.dll", "Lib2" + debug_extn,
					"Lib1.deploy.txt",
					Path.Combine ("fr-CA", "Lib1.resources.dll"),
					Path.Combine ("fr-FR", "Lib1.resources.dll"),
					Path.Combine ("en-US", "Lib1.resources.dll"),
					// lib2 files
					"Lib2.dll", "Lib2" + debug_extn,
					"lib2_folder/Lib2.deploy.txt",
					Path.Combine ("fr-CA", "Lib2.resources.dll"),
					Path.Combine ("fr-FR", "Lib2.resources.dll"),
					// lib3 files
					"Lib3.dll", "Lib3" + debug_extn,
					"Lib4.dll", "Lib4" + debug_extn
					},

				new string [] {
					"Project01.exe",
					"Project01" + (Consts.RunningOnMono () ? ".exe.mdb" : ".pdb"),
					// lib1 files
					"Lib1.dll", "Lib1" + debug_extn,
					"Lib1.deploy.txt",
					Path.Combine ("fr-CA", "Lib1.resources.dll"),
					Path.Combine ("fr-FR", "Lib1.resources.dll"),
					Path.Combine ("en-US", "Lib1.resources.dll"),
					// lib2 files
					"Lib2.dll", "Lib2" + debug_extn,
					"lib2_folder/Lib2.deploy.txt",
					Path.Combine ("fr-CA", "Lib2.resources.dll"),
					Path.Combine ("fr-FR", "Lib2.resources.dll"),
					"Lib4.dll", "Lib4" + debug_extn,
					}
			};

			// Cleanup
			for (int i = 0; i < project_dirs.Length; i ++) {
				string bin_path = Path.Combine (project_dirs [i], Path.Combine ("bin", "Debug"));
				string obj_path = Path.Combine (project_dirs [i], Path.Combine ("obj", "Debug"));

				DeleteAllInDir (bin_path);

				DeleteAllInDir (obj_path);
			}

			Engine engine = new Engine (Consts.BinPath);
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			engine.GlobalProperties = new BuildPropertyGroup ();
			engine.GlobalProperties.SetProperty ("TreatWarningsAsErrors", "false");

			Project project = engine.CreateNewProject ();
			project.Load (Path.Combine (basepath, "Project01.sln.proj"));
			
			bool result = project.Build ();
			if (!result) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			// We check only the output dir, not the 'obj'
			string debug = Path.Combine ("bin", "Debug");
			for (int i = 0; i < project_dirs.Length; i++) {
				CheckFilesExistInDir (Path.Combine (project_dirs [i], debug),
					project_files [i]);
			}
		}

		void DeleteAllInDir (string path)
		{
			if (!Directory.Exists (path))
				return;

			foreach (string file in Directory.GetFiles (path))
				File.Delete (file);
			Directory.Delete (path, true);
		}

		void CheckFilesExistInDir (string dir, params string [] files)
		{
			foreach (string file in files) {
				string path = Path.Combine (dir, file);
				Assert.IsTrue (File.Exists (path),
					String.Format ("Expected to find file {0}", path));
			}
		}

		Project CheckProjectForRequiredTests (string taskname, string property_arg, bool expected_result, string error_msg,
			string expected_output_msg)
		{
			string projectString = String.Format (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<UsingTask TaskName=""{0}"" AssemblyFile=""Test/resources/TestTasks.dll"" />
				<Target Name=""foo"">
					<{0} Property=""{1}"">
						<Output TaskParameter=""Output"" ItemName=""OutItem""/>
					</{0}>
					<Message Text='@(OutItem)'/>
				</Target>
			</Project>", taskname, property_arg);

			Engine engine = new Engine (Consts.BinPath);
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);
			Project project = engine.CreateNewProject ();
			project.LoadXml (projectString);
			try {
				Assert.AreEqual (expected_result, project.Build (), error_msg);
				if (expected_result) {
					logger.CheckLoggedMessageHead (expected_output_msg, "A");
					Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected messages found");
				}
			} finally {
				logger.DumpMessages ();
			}
			return project;
		}

		static void CheckBuildItem (BuildItem item, string name, string [,] metadata, string finalItemSpec, string prefix)
		{
			Assert.AreEqual (name, item.Name, prefix + "#1");
			for (int i = 0; i < metadata.GetLength (0); i++) {
				string key = metadata [i, 0];
				string val = metadata [i, 1];
				Assert.IsTrue (item.HasMetadata (key), String.Format ("{0}#2: Expected metadata '{1}' not found", prefix, key));
				Assert.AreEqual (val, item.GetMetadata (key), String.Format ("{0}#3: Value for metadata {1}", prefix, key));
				Assert.AreEqual (val, item.GetEvaluatedMetadata (key), String.Format ("{0}#4: Value for evaluated metadata {1}", prefix, key));
			}
			Assert.AreEqual (finalItemSpec, item.FinalItemSpec, prefix + "#5");
		}

		void CheckProjectBuild (Project project, string [] targetNames, bool result, string [] outputNames, string prefix)
		{
			IDictionary targetOutputs = new Hashtable ();

			Assert.AreEqual (result, project.Build (targetNames, targetOutputs), prefix + "A1");
			Assert.AreEqual (outputNames.Length, targetOutputs.Keys.Count, prefix + "A2");

			foreach (string outputName in outputNames) {
				Assert.IsTrue (targetOutputs.Contains (outputName), prefix + " A3: target " + outputName);

				object o = targetOutputs [outputName];
				Assert.IsTrue (typeof (ITaskItem []).IsAssignableFrom (o.GetType ()), prefix + " A4: target " + outputName);

				ITaskItem [] items = (ITaskItem [])o;
				Assert.AreEqual (0, items.Length, prefix + "A5: target " + outputName);
			}
		}

		string CreateProjectString (bool run_separate, string [] targets, bool [] results, string prefix)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">");
			sb.AppendFormat ("<Target Name = \"{0}\"><Message Text = \"#Target {1}:{0} called\" />", "main", prefix);

			sb.AppendFormat ("<CallTarget Targets=\"");
			for (int i = 0; i < targets.Length; i++)
				sb.AppendFormat ("{0};", targets [i]);
			sb.AppendFormat ("\" ");

			if (run_separate)
				sb.AppendFormat (" RunEachTargetSeparately=\"true\" ");
			sb.AppendFormat ("/></Target>\n");

			for (int i = 0; i < targets.Length; i++) {
				sb.AppendFormat ("<Target Name = \"{0}\"><Message Text = \"#Target {1}:{0} called\" />", targets [i], prefix);
				if (!results [i])
					sb.AppendFormat ("<Error Text = \"#Error message for target {0}:{1}\"/>", prefix, targets [i]);
				sb.Append ("</Target>\n");
			}

			sb.Append ("</Project>");

			return sb.ToString ();
		}

		void CreateProjectFile (string fname, bool run_separate, string [] targets, bool [] results, string prefix)
		{
			using (StreamWriter sw = new StreamWriter (fname))
				sw.Write (CreateProjectString (run_separate, targets, results, prefix));
		}

		Project CreateAndLoadProject (string fname, bool run_separate, string [] targets, bool [] results, string prefix)
		{
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			string projectXml = CreateProjectString (run_separate, targets, results, prefix);
			if (fname == null) {
				project.LoadXml (projectXml);
			} else {
				using (StreamWriter sw = new StreamWriter (fname))
					sw.Write (projectXml);
				project.Load (fname);
		                File.Delete (fname);
			}

			return project;
		}
	}
}
