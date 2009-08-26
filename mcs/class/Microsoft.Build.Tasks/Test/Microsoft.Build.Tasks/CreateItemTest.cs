//
// CreateItemTest.cs
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
using System.IO;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Tasks {

	[TestFixture]
	public class CreateItemTest {
		[Test]
		public void TestAssignment ()
		{
			CreateItem ci = new CreateItem ();

			ci.AdditionalMetadata = new string [1] { "a=1" };
			ci.Include = new ITaskItem [1] { new TaskItem ("1") };
			ci.Exclude = new ITaskItem [1] { new TaskItem ("2") };

			Assert.AreEqual ("a=1", ci.AdditionalMetadata [0], "A1");
			Assert.AreEqual ("1", ci.Include [0].ItemSpec, "A2");
			Assert.AreEqual ("2", ci.Exclude [0].ItemSpec, "A3");
		}

		[Test]
		public void TestExecution1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
								<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<A Include='1;2'>
							<Sub>fooA</Sub>
						</A>
						<A Include='3;4'>
							<Sub>fooC</Sub>
						</A>
						<B Include='1;3'>
							<Sub>fooB</Sub>
						</B>
					</ItemGroup>
					<Target Name='1'>
						<CreateItem
							AdditionalMetadata='a=1; b  = 2 '
							Include='@(A)'
							Exclude='@(B)'
						>
							<Output
								TaskParameter='Include'
								ItemName='NewItem'
							/>
						</CreateItem>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			Assert.IsTrue (project.Build ("1"), "A1");

			BuildItemGroup include = project.GetEvaluatedItemsByName ("NewItem");
			Assert.AreEqual (2, include.Count, "A2");

			string [,] additional_metadata = new string [,] { { "a", "1" }, { "b", "2" }, { "Sub", "fooA" } };
			CheckBuildItem (include [0], "NewItem", additional_metadata, "2", "A");

			additional_metadata = new string [,] { { "a", "1" }, { "b", "2" }, { "Sub", "fooC" } };
			CheckBuildItem (include [1], "NewItem", additional_metadata, "4", "B");
		}

		[Test]
		public void TestExcludeAndCondition ()
		{
			Engine engine;
			Project project;

			string documentString = @"
					<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<A Include='1;2;5'>
							<Sub>fooA</Sub>
						</A>
						<A Include='3;4'>
							<Sub>fooC</Sub>
						</A>
						<B Include='1;3'>
							<Sub>fooB</Sub>
						</B>
					</ItemGroup>
					<Target Name='1'>
						<CreateItem
							AdditionalMetadata='a=1;b=2'
							Include='@(A)'
							Exclude='@(B)'
							Condition=""'%(Sub)' == 'fooA'""
						>
							<Output
								TaskParameter='Include'
								ItemName='NewItem'
							/>
						</CreateItem>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			Assert.IsTrue (project.Build ("1"), "A1");

			BuildItemGroup include = project.GetEvaluatedItemsByName ("NewItem");
			Assert.AreEqual (3, include.Count, "A2");

			string [,] additional_metadata = new string [,] { { "a", "1" }, {"b", "2"}, {"Sub", "fooA" } };
			CheckBuildItem (include [0], "NewItem", additional_metadata, "1", "A");
			CheckBuildItem (include [1], "NewItem", additional_metadata, "2", "B");
			CheckBuildItem (include [2], "NewItem", additional_metadata, "5", "C");
		}

		[Test]
		public void TestNullFields ()
		{
		    Engine engine;
		    Project project;

		    string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<A Include='1;2;5'>
							<Sub>fooA</Sub>
						</A>
					</ItemGroup>
					<Target Name='1'>
						<CreateItem Include='@(A)' >
							<Output
								TaskParameter='Include'
								ItemName='NewItem'
							/>
						</CreateItem>
					</Target>
				</Project>";

		    engine = new Engine (Consts.BinPath);
		    project = engine.CreateNewProject ();
		    project.LoadXml (documentString);
		    Assert.IsTrue (project.Build ("1"), "A1, Build failed");

		    BuildItemGroup include = project.GetEvaluatedItemsByName ("NewItem");
		    Assert.AreEqual (3, include.Count, "A2");

		    string [,] additional_metadata = new string [0, 0];
		    CheckBuildItem (include [0], "NewItem", additional_metadata, "1", "A");
		    CheckBuildItem (include [1], "NewItem", additional_metadata, "2", "B");
		    CheckBuildItem (include [2], "NewItem", additional_metadata, "5", "C");
		}

		[Test]
		public void TestVariableExpansion ()
		{
		    Engine engine;
		    Project project;

			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<PropertyGroup>
					<P1>FooP1</P1>
					<P2>FooP2</P2>
					<C>@(IG)</C>
					<P3>@(Nine)</P3>
				</PropertyGroup>
				<ItemGroup>
					<Nine Include=""Nine""/>
					<Eight Include=""Eight""/>
					<Seven Include=""@(Eight)""/>
					<Six Include=""@(Seven);$(P3)""/>
					<Third Include=""Abc""/>
					<Fourth Include=""$(P2)""/>
					<Second Include=""@(Third);$(P1);@(Fourth);@(Six)""/>
					<IG Include=""@(Second)""/>
				</ItemGroup>

					<Target Name='1'>
						<CreateItem Include='$(C)' >
							<Output
								TaskParameter='Include'
								ItemName='Items'
							/>
						</CreateItem>

						<Message Text=""C: $(C)""/>
						<Message Text=""items: @(items)""/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			BuildItemGroup include = project.GetEvaluatedItemsByName ("Items");
			Assert.AreEqual (5, include.Count, "A2");

			Assert.AreEqual ("Abc", include [0].FinalItemSpec, "A#3");
			Assert.AreEqual ("FooP1", include[1].FinalItemSpec, "A#4");
			Assert.AreEqual ("FooP2", include[2].FinalItemSpec, "A#5");
			Assert.AreEqual ("Eight", include[3].FinalItemSpec, "A#6");
			Assert.AreEqual ("Nine", include[4].FinalItemSpec, "A#7");

			testLogger.CheckLoggedMessageHead ("C: Abc;FooP1;FooP2;Eight;Nine", "A#10");
			testLogger.CheckLoggedMessageHead ("items: Abc;FooP1;FooP2;Eight;Nine", "A#10");

		}

		public static void CheckBuildItem (BuildItem item, string name, string [,] metadata, string finalItemSpec, string prefix)
		{
			Assert.AreEqual (name, item.Name, prefix + "#1");
			for (int i = 0; i < metadata.GetLength (0); i ++) {
				string key = metadata [i, 0];
				string val = metadata [i, 1];
				Assert.IsTrue (item.HasMetadata (key), String.Format ("{0}#2: Expected metadata '{1}' not found", prefix, key));
				Assert.AreEqual (val, item.GetMetadata (key), String.Format ("{0}#3: Value for metadata {1}", prefix, key));
				Assert.AreEqual (val, item.GetEvaluatedMetadata (key), String.Format ("{0}#4: Value for evaluated metadata {1}", prefix, key));
			}
			Assert.AreEqual (finalItemSpec, item.FinalItemSpec, prefix + "#5");
		}
	}
}
