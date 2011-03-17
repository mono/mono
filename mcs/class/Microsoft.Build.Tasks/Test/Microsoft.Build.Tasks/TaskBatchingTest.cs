//
// TaskBatchingTest.cs
//
// Author:
//   Ankit Jain (jankit@novell.com)
//
// Copyright 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;

namespace MonoTests.Microsoft.Build.Tasks
{
	[TestFixture]
	public class TaskBatchingTest
	{
		string projectHeader;
		public TaskBatchingTest ()
		{
			projectHeader = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" " + Consts.ToolsVersionString + ">";
		}

		[Test]
		public void Test1 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<ResXFile Include=""Item1"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item2"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item3"">
						<Culture>en</Culture>
					</ResXFile>
					<ResXFile Include=""Item4"">
						<Culture>gb</Culture>
					</ResXFile>
					<ResXFile Include=""Item5"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item6"">
						<Culture>it</Culture>
					</ResXFile>
				</ItemGroup>

				<Target Name=""ShowMessage"">
					<Message
						Text = ""Culture: %(ResXFile.Culture) -- ResXFile: @(ResXFile)"" />
				</Target>
			  </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("ShowMessage"), "A1: Build failed");

			CheckMessage (testLogger, "fr", "Item1;Item2;Item5", "A2");
			CheckMessage (testLogger, "en", "Item3", "A3");
			CheckMessage (testLogger, "gb", "Item4", "A4");
			CheckMessage (testLogger, "it", "Item6", "A5");

			CheckEngineEventCounts (testLogger, 1, 1, 4, 4);
		}

		// Test1 with unqualified %(Culture)
		[Test]
		public void Test2 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<ResXFile Include=""Item1"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item2"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item3"">
						<Culture>en</Culture>
					</ResXFile>
					<ResXFile Include=""Item4"">
						<Culture>gb</Culture>
					</ResXFile>
					<ResXFile Include=""Item5"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item6"">
						<Culture>it</Culture>
					</ResXFile>
				</ItemGroup>

				<Target Name=""ShowMessage"">
					<Message
						Text = ""Culture: %(Culture) -- ResXFile: @(ResXFile)"" />
				</Target>
			  </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("ShowMessage"), "A1: Build failed");

			CheckMessage (testLogger, "fr", "Item1;Item2;Item5", "A2");
			CheckMessage (testLogger, "en", "Item3", "A3");
			CheckMessage (testLogger, "gb", "Item4", "A4");
			CheckMessage (testLogger, "it", "Item6", "A5");

			CheckEngineEventCounts (testLogger, 1, 1, 4, 4);
		}

		[Test]
		public void TestUnqualifiedMetadataReference ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<ResXFile Include=""Item1"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item5"" />
					<ResXFile Include=""Item6"">
						<Culture>it</Culture>
					</ResXFile>
				</ItemGroup>

				<Target Name=""ShowMessage"">
					<Message
						Text = ""Culture: %(Culture) -- ResXFile: @(ResXFile)"" />
				</Target>
			  </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);

			//Fails as Culture is being referenced unqualified, and no Culture is
			//specified for "Item5"
			bool result = project.Build ("ShowMessage");
			if (result)
				Assert.Fail ("A1: Build should have failed");

			CheckEngineEventCounts (testLogger, 1, 1, 0, 0);
		}

		[Test]
		public void Test4 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<ResXFile Include=""Item1"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item5"" />
					<ResXFile Include=""Item6"">
						<Culture>it</Culture>
					</ResXFile>
				</ItemGroup>

				<Target Name=""ShowMessage"">
					<Message
						Text = ""Culture: %(ResXFile.Culture) -- ResXFile: @(ResXFile)"" />
				</Target>
			  </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);

			//no Culture is specified for "Item5", but
			//Culture is being referenced __qualified__, so works
			Assert.IsTrue (project.Build ("ShowMessage"), "A1: Build failed");

			CheckMessage (testLogger, "fr", "Item1", "A2");
			CheckMessage (testLogger, "", "Item5", "A3");
			CheckMessage (testLogger, "it", "Item6", "A3");
			CheckEngineEventCounts (testLogger, 1, 1, 3, 3);
		}

		[Test]
		public void TestMultiItemCollections ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<ResXFile Include=""Item1"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item2"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item3"">
						<Culture>en</Culture>
					</ResXFile>
					<ResXFile Include=""Item4"">
						<Culture>gb</Culture>
					</ResXFile>
					<ResXFile Include=""Item6"">
						<Culture>it</Culture>
					</ResXFile>

					<NonResXFile Include=""Item7"">
						<Culture>it</Culture>
					</NonResXFile>
					<NonResXFile Include=""Item8"">
						<Culture>en</Culture>
					</NonResXFile>
					<NonResXFile Include=""Item9"">
						<Culture>en</Culture>
					</NonResXFile>
				</ItemGroup>

				<Target Name=""ShowMessage"">
					<Message
						Text = ""Culture: %(Culture) -- ResXFiles: @(ResXFile) NonResXFiles: @(NonResXFile)"" />
				</Target>
			  </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("ShowMessage"), "A1: Build failed");

			CheckMessage2 (testLogger, "fr", "Item1;Item2", string.Empty, "A2");
			CheckMessage2 (testLogger, "en", "Item3", "Item8;Item9", "A3");
			CheckMessage2 (testLogger, "gb", "Item4", string.Empty, "A4");
			CheckMessage2 (testLogger, "it", "Item6", "Item7", "A6");
			CheckEngineEventCounts (testLogger, 1, 1, 4, 4);
		}

		[Test]
		public void TestConditionalBatching ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<ResXFile Include=""Item1"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item2"">
						<Culture>fr</Culture>
					</ResXFile>
					<ResXFile Include=""Item3"">
						<Culture>en</Culture>
					</ResXFile>
					<ResXFile Include=""Item4"">
						<Culture>gb</Culture>
					</ResXFile>
					<ResXFile Include=""Item6"">
						<Culture>it</Culture>
					</ResXFile>

					<NonResXFile Include=""Item7"">
						<Culture>it</Culture>
					</NonResXFile>
					<NonResXFile Include=""Item8"">
						<Culture>en</Culture>
					</NonResXFile>
					<NonResXFile Include=""Item9"">
						<Culture>en</Culture>
					</NonResXFile>
				</ItemGroup>

				<Target Name=""ShowMessage"">
					<Message
						Text = ""ResXFiles: @(ResXFile) NonResXFiles: @(NonResXFile)""
						Condition = ""'%(Culture)' == 'fr'""/>
				</Target>
			  </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("ShowMessage"), "A1: Build failed");

			testLogger.CheckLoggedMessageHead ("ResXFiles: Item1;Item2 NonResXFiles: ", "A2");
			CheckEngineEventCounts (testLogger, 1, 1, 1, 1);
		}

		[Test]
		public void TestMultipleMetadataReference ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<ExampColl Include=""Item1"">
						<Number>1</Number>
					</ExampColl>
					<ExampColl Include=""Item2"">
						<Number>2</Number>
					</ExampColl>
					<ExampColl Include=""Item3"">
					<Number>1</Number>
					</ExampColl>

					<ExampColl2 Include=""Item4"">
						<Number>1</Number>
					</ExampColl2>
					<ExampColl2 Include=""Item5"">
						<Number>2</Number>
					<Color>Red</Color>
					</ExampColl2>
					<ExampColl2 Include=""Item6"">
						<Number>3</Number>
					<Color>Green</Color>
					</ExampColl2>
				</ItemGroup>
				<Target Name=""ShowMessage"">
					<Message Text = ""Number: %(Number) Color: %(ExampColl2.Color)-- Items in ExampColl: @(ExampColl) ExampColl2: @(ExampColl2)""/>
				</Target>
			</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("ShowMessage"), "A1: Build failed");

			CheckLoggedMessageAny (testLogger, "Number: 1 Color: -- Items in ExampColl: Item1;Item3 ExampColl2: Item4", "A2");
			CheckLoggedMessageAny (testLogger, "Number: 2 Color: Red-- Items in ExampColl:  ExampColl2: Item5", "A3");
			CheckLoggedMessageAny (testLogger, "Number: 3 Color: Green-- Items in ExampColl:  ExampColl2: Item6", "A4");
			CheckLoggedMessageAny (testLogger, "Number: 2 Color: -- Items in ExampColl: Item2 ExampColl2: ", "A5");
			Assert.AreEqual (0, testLogger.Count, "A6");
			CheckEngineEventCounts (testLogger, 1, 1, 4, 4);
		}

		[Test]
		public void TestMultipleMetadataReference2 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<GroupA Include=""file1.txt""/>
					<GroupA Include=""file2.txt""/>
					<GroupA Include=""file3.txt""/>
					<GroupA Include=""file3.txt""/>
					<GroupA Include=""file4.txt""/>
				</ItemGroup>

				<ItemGroup>
					<GroupB Include=""file1.txt""/>
					<GroupB Include=""file3.txt""/>
					<GroupB Include=""file5.txt""/>

					<GroupC Include=""PreExistingValue""/>
				</ItemGroup>

				<Target Name=""Build"">
					<CreateItem Include=""@(GroupA)"" Condition=""'%(Identity)' != '' and '@(GroupA)' != '' and '@(GroupB)' != ''"" >
						<Output TaskParameter=""Include"" ItemName=""GroupC""/>
					</CreateItem>
					<Message Text=""%(GroupC.Identity)""/>
				</Target>
			</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("Build"), "A1: Build failed");

			BuildItemGroup include = project.GetEvaluatedItemsByName ("GroupC");
			Assert.AreEqual (4, include.Count, "A2");

			string [,] additional_metadata = new string [,] { { "Identity", "PreExistingValue" } };
			CreateItemTest.CheckBuildItem (include [0], "GroupC", additional_metadata, "PreExistingValue", "A3");

			additional_metadata = new string [,] { { "Identity", "file1.txt" } };
			CreateItemTest.CheckBuildItem (include [1], "GroupC", additional_metadata, "file1.txt", "A4");

			additional_metadata = new string [,] { { "Identity", "file3.txt" } };
			CreateItemTest.CheckBuildItem (include [2], "GroupC", additional_metadata, "file3.txt", "A5");
			CreateItemTest.CheckBuildItem (include [3], "GroupC", additional_metadata, "file3.txt", "A6");

			CheckEngineEventCounts (testLogger, 1, 1, 5, 5);
		}

		[Test]
		public void TestIdentity ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<ExampColl Include=""Item1""/>
					<ExampColl Include=""Item2""/>
					<ExampColl Include=""Item3""/>
					<ExampColl Include=""Item4""/>
					<ExampColl Include=""Item4""/>
					<ExampColl Include=""Item5""/>
					<ExampColl Include=""Item6""/>
				</ItemGroup>
				<Target Name=""ShowMessage"">
					<Message Text = ""Identity: %(IdenTIty) -- Items in ExampColl: @(ExampColl)""/>
				</Target>
			</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("ShowMessage"), "A1: Build failed");

			CheckLoggedMessageAny (testLogger, "Identity: Item1 -- Items in ExampColl: Item1", "A2");
			CheckLoggedMessageAny (testLogger, "Identity: Item2 -- Items in ExampColl: Item2", "A3");
			CheckLoggedMessageAny (testLogger, "Identity: Item3 -- Items in ExampColl: Item3", "A4");
			CheckLoggedMessageAny (testLogger, "Identity: Item4 -- Items in ExampColl: Item4;Item4", "A5");
			CheckLoggedMessageAny (testLogger, "Identity: Item5 -- Items in ExampColl: Item5", "A6");
			CheckLoggedMessageAny (testLogger, "Identity: Item6 -- Items in ExampColl: Item6", "A7");
			Assert.AreEqual (0, testLogger.Count, "A8");
			CheckEngineEventCounts (testLogger, 1, 1, 6, 6);
		}

		[Test]
		public void TestFilter ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				 <ItemGroup>
					 <fruit Include=""apple"">
						 <consistency>firm</consistency>
					 </fruit>
					 <fruit Include=""orange"">
						 <consistency>pulpy</consistency>
					 </fruit>
					 <fruit Include=""banana"">
						 <consistency>softish</consistency>
					 </fruit>
					 <fruit Include=""pear"">
						 <consistency>unsound</consistency>
					 </fruit>
					 <fruit Include=""apricot"">
						 <consistency>unsound</consistency>
					 </fruit>
				 </ItemGroup>
				 <Target Name=""Compost"">
					 <CreateItem Include=""@(fruit)"" Condition=""'%(consistency)' == 'pulpy' or '%(consistency)' == 'unsound' "">
						<Output TaskParameter=""Include"" ItemName=""Final""/>
					 </CreateItem>
				 </Target>
			 </Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			Assert.IsTrue (project.Build ("Compost"), "A1: Build failed");

			BuildItemGroup include = project.GetEvaluatedItemsByName ("Final");
			Assert.AreEqual (3, include.Count, "A2");

			string [,] additional_metadata = new string [,] { { "Identity", "orange" } };
			CreateItemTest.CheckBuildItem (include [0], "Final", additional_metadata, "orange", "A3");

			additional_metadata = new string [,] { { "Identity", "pear" } };
			CreateItemTest.CheckBuildItem (include [1], "Final", additional_metadata, "pear", "A4");

			additional_metadata = new string [,] { { "Identity", "apricot" } };
			CreateItemTest.CheckBuildItem (include [2], "Final", additional_metadata, "apricot", "A5");
			CheckEngineEventCounts (testLogger, 1, 1, 2, 2);
		}

		[Test]
		// test for metadata refs in properties or items
		public void TestNoBatching () {
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<ItemGroup>
		<item3 Include=""foo""/>
		<item2 Include=""%(item3.Identity)""/>
		<item0 Include=""@(item3)""/>
	</ItemGroup>
	<PropertyGroup>
		<Prop1>%(item0.Identity)</Prop1>
	</PropertyGroup>
	<Target Name='1'>
		<Message Text=""Prop1: $(Prop1)""/>
		<Message Text=""Item2: @(item2)""/>
	</Target>
</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			testLogger.CheckLoggedMessageHead ("Prop1: %(item0.Identity)", "A1");
			testLogger.CheckLoggedMessageHead ("Item2: %(item3.Identity)", "A2");
			Assert.AreEqual (0, testLogger.NormalMessageCount, "Unexpected extra messages found");
		}

		[Test]
		// test for metadata refs via metadata refs
		// batching should happen only on basis of the task attributes,
		// and not the resolved expression values
		public void TestBatching1 () {
			string projectString = projectHeader + @"
	<ItemGroup>
		<item3 Include=""foo""/>
		<item2 Include=""%(item3.Identity)""/>

		<item4 Include=""%(item2.Identity);@(item3);@(nonexistant)""/>
		<item4 Include=""bar""/>
	</ItemGroup>
	<Target Name='1'>
		<Message Text=""Item4: %(item4.Identity)""/>
	</Target>
</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			testLogger.CheckLoggedMessageHead ("Item4: %(item2.Identity)", "A1");
			testLogger.CheckLoggedMessageHead ("Item4: foo", "A2");
			testLogger.CheckLoggedMessageHead ("Item4: bar", "A3");
			Assert.AreEqual (0, testLogger.NormalMessageCount, "Unexpected extra messages found");
		}

		[Test]
		// test for metadata refs via metadata refs
		// batching should happen only on basis of the task attributes,
		// and not the resolved expression values
		public void TestConditionalBatching2 () {
			string projectString = projectHeader + @"
	<ItemGroup>
		<item2 Include=""%(item3.Identity)""/>
		<item4 Include=""%(item2.Identity);@(item3)""/>
	</ItemGroup>
	<Target Name='1'>
		<Message Text=""Item3: %(item2.Identity)"" Condition="" '%(item5.Identity)' == '' ""/>
		<Message Text=""Item4: %(item4.Identity)""/>
	</Target>
</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			testLogger.CheckLoggedMessageHead ("Item3: %(item3.Identity)", "A1");
			testLogger.CheckLoggedMessageHead ("Item4: %(item2.Identity)", "A2");
			Assert.AreEqual (0, testLogger.NormalMessageCount, "Unexpected extra messages found");
		}

		[Test]
		public void TestBatchingWithUnbatchedItems () {
			string projectString = projectHeader + @"
	<ItemGroup>
		<Item1 Include=""One""/>
		<Item1 Include=""Two""/>

		<B Include=""abc""/>
	</ItemGroup>
	<Target Name='1'>
		<Message Text=""Item1: %(Item1.Identity) | B: @(B)""/>
	</Target>
</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			try {
				testLogger.CheckLoggedMessageHead ("Item1: One | B: abc", "A1");
				testLogger.CheckLoggedMessageHead ("Item1: Two | B: abc", "A2");

				Assert.AreEqual (0, testLogger.NormalMessageCount, "Unexpected extra messages found");
			} catch (AssertionException) {
				testLogger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestPropertiesWithBatchedReferences () {
			string projectString = projectHeader + @"
	<ItemGroup>
		<Item1 Include=""One""/>
		<Item1 Include=""Two""/>

		<Item1Ref Include=""@(Item1)""/>
	</ItemGroup>
	<PropertyGroup>
		<Prop1>@(Item1)</Prop1>
		<Prop2>@(Item1Ref)</Prop2>
	</PropertyGroup>
	<Target Name='1'>
		<Message Text=""Item1: %(Item1.Identity) | Prop1: $(Prop1) | Prop2: $(Prop2)""/>
	</Target>
</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			if (!project.Build ("1")) {

				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			try {
				testLogger.CheckLoggedMessageHead ("Item1: One | Prop1: One | Prop2: One;Two", "A1");
				testLogger.CheckLoggedMessageHead ("Item1: Two | Prop1: Two | Prop2: One;Two", "A2");

				Assert.AreEqual (0, testLogger.NormalMessageCount, "Unexpected extra messages found");
			} catch (AssertionException) {
				testLogger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestPropertiesWithDynamicItems () {
			string projectString = projectHeader + @"
	<ItemGroup>
		<Item1 Include=""One""/>
		<Item1 Include=""Two""/>

		<Item2 Include=""Z""/>
		<Item2Ref Include=""@(Item2)"" />
	</ItemGroup>
	<PropertyGroup>
		<Prop1>@(Item2)</Prop1>
		<Prop2>@(Item2Ref)</Prop2>
	</PropertyGroup>
	<Target Name='1'>
		<Message Text=""Item1: %(Item1.Identity) | Prop1: $(Prop1) | Prop2: $(Prop2)""/>
		<Message Text=""Item2: @(Item2) | Item2Ref: @(Item2Ref)""/>
		<CreateItem Include=""A;B"">
			<Output TaskParameter=""Include"" ItemName=""Item2""/>
		</CreateItem>
		<Message Text=""Item1: %(Item1.Identity) | Prop1: $(Prop1) | Prop2: $(Prop2)""/>
		<Message Text=""Item2: @(Item2) | Item2Ref: @(Item2Ref)""/>
	</Target>
</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			if (!project.Build ("1")) {

				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			try {

				testLogger.CheckLoggedMessageHead ("Item1: One | Prop1: Z | Prop2: Z", "A1");
				testLogger.CheckLoggedMessageHead ("Item1: Two | Prop1: Z | Prop2: Z", "A2");
				testLogger.CheckLoggedMessageHead ("Item2: Z | Item2Ref: Z", "A3");

				testLogger.CheckLoggedMessageHead ("Item1: One | Prop1: Z;A;B | Prop2: Z", "A4");
				testLogger.CheckLoggedMessageHead ("Item1: Two | Prop1: Z;A;B | Prop2: Z", "A5");
				testLogger.CheckLoggedMessageHead ("Item2: Z;A;B | Item2Ref: Z", "A3");

				Assert.AreEqual (0, testLogger.NormalMessageCount, "Unexpected extra messages found");
			} catch (AssertionException) {
				testLogger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestTargetInvocationFromBatchedTask () {
			string projectString = projectHeader + @"
	<ItemGroup>
		<Item1 Include=""One""/>
		<Item1 Include=""Two""/>

		<Item1Ref Include=""@(Item1)"" />
	</ItemGroup>
	<PropertyGroup>
		<Prop1>@(Item1)</Prop1>
		<Prop2>@(Item1Ref)</Prop2>
	</PropertyGroup>
	<Target Name='1'>
		<CallTarget Targets='foo' Condition="" '%(Item1.Identity)' != ''"" />
		<Message Text=""Item1: @(Item1) Item1Ref: @(Item1Ref)""/>
		<Message Text=""Prop1: $(Prop1) Prop2: $(Prop2)""/>
	</Target>
	<Target Name='foo'>
		<Message Text=""(foo) Item1: @(Item1) Item1Ref: @(Item1Ref)""/>
		<Message Text=""(foo) Prop1: $(Prop1) Prop2: $(Prop2)""/>
	</Target>
</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			if (!project.Build ("1")) {

				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			try {
				testLogger.CheckLoggedMessageHead ("(foo) Item1: One;Two Item1Ref: One;Two", "A1");
				testLogger.CheckLoggedMessageHead ("(foo) Prop1: One;Two Prop2: One;Two", "A2");

				testLogger.CheckLoggedMessageHead ("Item1: One;Two Item1Ref: One;Two", "A3");
				testLogger.CheckLoggedMessageHead ("Prop1: One;Two Prop2: One;Two", "A4");

				Assert.AreEqual (0, testLogger.NormalMessageCount, "Unexpected extra messages found");
			} catch (AssertionException) {
				testLogger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestTargetInvocationFromBatchedTarget () {
			string projectString = projectHeader + @"
	<ItemGroup>
		<Item1 Include=""One""/>
		<Item1 Include=""Two""/>

		<Item1Ref Include=""@(Item1)"" />
	</ItemGroup>
	<PropertyGroup>
		<Prop1>@(Item1)</Prop1>
		<Prop2>@(Item1Ref)</Prop2>
	</PropertyGroup>
	<Target Name='1' Inputs=""%(Item1.Identity)"" Outputs=""Nonexistant.foobar"">
		<Message Text=""Item1: @(Item1) Item1Ref: @(Item1Ref)""/>
		<Message Text=""Prop1: $(Prop1) Prop2: $(Prop2)""/>

		<CallTarget Targets='foo' />

		<Message Text=""Item1: @(Item1) Item1Ref: @(Item1Ref)""/>
		<Message Text=""Prop1: $(Prop1) Prop2: $(Prop2)""/>
	</Target>
	<Target Name='foo' Condition="" '@(Item1)' != 'One' "">
		<Message Text=""(foo) Item1: @(Item1) Item1Ref: @(Item1Ref)""/>
		<Message Text=""(foo) Prop1: $(Prop1) Prop2: $(Prop2)""/>
	</Target>
</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			if (!project.Build ("1")) {

				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			try {
				testLogger.CheckLoggedMessageHead ("Item1: One Item1Ref: One;Two", "A1");
				testLogger.CheckLoggedMessageHead ("Prop1: One Prop2: One;Two", "A2");

				testLogger.CheckLoggedMessageHead ("(foo) Item1: One;Two Item1Ref: One;Two", "A3");
				testLogger.CheckLoggedMessageHead ("(foo) Prop1: One;Two Prop2: One;Two", "A4");

				testLogger.CheckLoggedMessageHead ("Item1: One Item1Ref: One;Two", "A5");
				testLogger.CheckLoggedMessageHead ("Prop1: One Prop2: One;Two", "A6");

				//second batch, foo has already run, so doesn't execute again
				testLogger.CheckLoggedMessageHead ("Item1: Two Item1Ref: One;Two", "A7");
				testLogger.CheckLoggedMessageHead ("Prop1: Two Prop2: One;Two", "A8");

				testLogger.CheckLoggedMessageHead ("Item1: Two Item1Ref: One;Two", "A9");
				testLogger.CheckLoggedMessageHead ("Prop1: Two Prop2: One;Two", "A10");

				Assert.AreEqual (0, testLogger.NormalMessageCount, "Unexpected extra messages found");
			} catch (AssertionException) {
				testLogger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestBatchingWithUnqualifiedMetadataReference () {
			string projectString = projectHeader + @"
	<ItemGroup>
		<Item1 Include=""One""><Md>1</Md></Item1>
		<Item1 Include=""Two""><Md>2</Md></Item1>
		<Item1Ref Include=""@(Item1)"" />

		<Item2 Include=""Three""><Md>3</Md></Item2>
		<Item2 Include=""Four""><Md>4</Md></Item2>
		<Item2Ref Include=""@(Item2)"" />
	</ItemGroup>
	<PropertyGroup>
		<Prop1>@(Item1)</Prop1>
		<Prop1Ref>@(Item1Ref)</Prop1Ref>

		<Prop2>@(Item2)</Prop2>
		<Prop2Ref>@(Item2Ref)</Prop2Ref>
	</PropertyGroup>
	<Target Name='1'>
		<Message Text=""For md: %(Md) Item1: @(Item1) Item1Ref: @(Item1Ref) Item2: @(Item2) Item2Ref: @(Item2Ref) " +
					  @" Prop1: $(Prop1) Prop1Ref: $(Prop1Ref) Prop2: $(Prop2) Prop2Ref: $(Prop2Ref)""/>
	</Target>
</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			if (!project.Build ("1")) {

				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			try {
				testLogger.CheckLoggedAny ("For md: 3 Item1:  Item1Ref:  Item2: Three Item2Ref: Three  Prop1:  Prop1Ref:  Prop2: Three Prop2Ref: Three", MessageImportance.Normal, "A1");
				testLogger.CheckLoggedAny ("For md: 4 Item1:  Item1Ref:  Item2: Four Item2Ref: Four  Prop1:  Prop1Ref:  Prop2: Four Prop2Ref: Four", MessageImportance.Normal, "A2");
				testLogger.CheckLoggedAny ("For md: 1 Item1: One Item1Ref: One Item2:  Item2Ref:   Prop1: One Prop1Ref: One Prop2:  Prop2Ref: ", MessageImportance.Normal, "A3");
				testLogger.CheckLoggedAny ("For md: 2 Item1: Two Item1Ref: Two Item2:  Item2Ref:   Prop1: Two Prop1Ref: Two Prop2:  Prop2Ref: ", MessageImportance.Normal, "A4");

				Assert.AreEqual (0, testLogger.NormalMessageCount, "Unexpected extra messages found");
			} catch (AssertionException) {
				testLogger.DumpMessages ();
				throw;
			}
		}


		//Target batching

		[Test]
		public void TestTargetBatching1 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<List1 Include=""fr_a.txt""><Culture>fr</Culture></List1>
					<List1 Include=""en_b.txt""><Culture>en</Culture></List1>
					<List1 Include=""fr_c.txt""><Culture>fr</Culture></List1>
					<List1 Include=""gb_d.txt""><Culture>gb</Culture></List1>

					<List2 Include=""fr_x.txt""><Culture>fr</Culture></List2>
					<List2 Include=""gb_z.txt""><Culture>gb</Culture></List2>

					<Foo Include=""1_a1""><Md>1</Md></Foo>
					<Foo Include=""2_b1""><Md>2</Md></Foo>
					<Foo Include=""1_c1""><Md>1</Md></Foo>
				</ItemGroup>

				<Target Name=""foo"" >
					<Message Text=""TargetStarted""/>
					<Message Text=""List1: @(List1): %(Culture)""/>
					<Message Text=""Foo: @(Foo): %(Md)""/>
				</Target>
		</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			bool res = project.Build ("foo");
			if (!res) {
				testLogger.DumpMessages ();
				Assert.Fail ("A1: Build failed");
			}

			CheckLoggedMessagesInOrder (testLogger, new string [] {
				"TargetStarted", "List1: fr_a.txt;fr_c.txt: fr",
				"List1: en_b.txt: en", "List1: gb_d.txt: gb",
				"Foo: 1_a1;1_c1: 1", "Foo: 2_b1: 2"
			}, "A2");
			CheckEngineEventCounts (testLogger, 1, 1, 6, 6);
		}

		[Test]
		public void TestTargetBatching2 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<List1 Include=""fr_a.txt""><Culture>fr</Culture></List1>
					<List1 Include=""en_b.txt""><Culture>en</Culture></List1>
					<List1 Include=""fr_c.txt""><Culture>fr</Culture></List1>
					<List1 Include=""gb_d.txt""><Culture>gb</Culture></List1>

					<List2 Include=""fr_x.txt""><Culture>fr</Culture></List2>
					<List2 Include=""gb_z.txt""><Culture>gb</Culture></List2>

					<Foo Include=""1_a1""><Md>1</Md></Foo>
					<Foo Include=""2_b1""><Md>2</Md></Foo>
					<Foo Include=""1_c1""><Md>1</Md></Foo>
				</ItemGroup>

				<Target Name=""foo"" Inputs=""@(List1)"" Outputs=""%(Culture).foo"">
					<Message Text=""TargetStarted""/>
					<Message Text=""List1: @(List1): %(Culture)""/>
					<Message Text=""Foo: @(Foo): %(Md)""/>
				</Target>
		</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			bool res = project.Build ("foo");
			if (!res) {
				testLogger.DumpMessages ();
				Assert.Fail ("A1: Build failed");
			}

			CheckLoggedMessagesInOrder (testLogger, new string [] {
				"TargetStarted", "List1: fr_a.txt;fr_c.txt: fr",
				"Foo: 1_a1;1_c1: 1", "Foo: 2_b1: 2",

				"TargetStarted", "List1: en_b.txt: en",
				"Foo: 1_a1;1_c1: 1", "Foo: 2_b1: 2",

				"TargetStarted", "List1: gb_d.txt: gb",
				"Foo: 1_a1;1_c1: 1", "Foo: 2_b1: 2"
			}, "A2");
			CheckEngineEventCounts (testLogger, 3, 3, 12, 12);
		}

		[Test]
		public void TestTargetBatching3 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<List1 Include=""fr_a.txt""><Culture>fr</Culture></List1>
					<List1 Include=""en_b.txt""><Culture>en</Culture></List1>
					<List1 Include=""fr_c.txt""><Culture>fr</Culture></List1>
					<List1 Include=""gb_d.txt""><Culture>gb</Culture></List1>

					<List2 Include=""fr_x.txt""><Culture>fr</Culture></List2>
					<List2 Include=""gb_z.txt""><Culture>gb</Culture></List2>

					<Foo Include=""1_a1""><Md>1</Md></Foo>
					<Foo Include=""2_b1""><Md>2</Md></Foo>
					<Foo Include=""1_c1""><Md>1</Md></Foo>
				</ItemGroup>
				<Target Name=""foo"" Inputs=""@(Foo)"" Outputs=""%(Md).foo"">
					<Message Text=""TargetStarted""/>
					<Message Text=""List1: @(List1): %(Culture)""/>
					<Message Text=""Foo: @(Foo): %(Md)""/>
				</Target>
		</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			bool res = project.Build ("foo");
			if (!res) {
				testLogger.DumpMessages ();
				Assert.Fail ("A1: Build failed");
			}

			CheckLoggedMessagesInOrder (testLogger, new string [] {
				"TargetStarted", "List1: fr_a.txt;fr_c.txt: fr",
				"List1: en_b.txt: en", "List1: gb_d.txt: gb",
				"Foo: 1_a1;1_c1: 1",
				"TargetStarted", "List1: fr_a.txt;fr_c.txt: fr",
				"List1: en_b.txt: en", "List1: gb_d.txt: gb",
				"Foo: 2_b1: 2"
			}, "A2");
			CheckEngineEventCounts (testLogger, 2, 2, 10, 10);
		}

		[Test]
		public void TestTargetBatching4 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<List1 Include=""fr_a.txt""><Culture>fr</Culture></List1>
					<List1 Include=""en_b.txt""><Culture>en</Culture></List1>
					<List1 Include=""fr_c.txt""><Culture>fr</Culture></List1>
					<List1 Include=""gb_d.txt""><Culture>gb</Culture></List1>

					<List2 Include=""fr_x.txt""><Culture>fr</Culture></List2>
					<List2 Include=""gb_z.txt""><Culture>gb</Culture></List2>

					<Foo Include=""1_a1""><Md>1</Md></Foo>
					<Foo Include=""2_b1""><Md>2</Md></Foo>
					<Foo Include=""1_c1""><Md>1</Md></Foo>
				</ItemGroup>
				<Target Name=""foo"" Inputs=""@(List1)"" Outputs=""%(Culture).foo"">
					<Message Text=""TargetStarted""/>
					<Message Text=""List1: @(List1): %(Culture)""/>
					<Message Text=""List2: @(List2): %(Culture)""/>
					<Message Text=""Foo: @(Foo): %(Md)""/>
				</Target>
		</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			bool res = project.Build ("foo");
			if (!res) {
				testLogger.DumpMessages ();
				Assert.Fail ("A1: Build failed");
			}

			CheckLoggedMessagesInOrder (testLogger, new string [] {
				"TargetStarted", "List1: fr_a.txt;fr_c.txt: fr",
				"List2: fr_x.txt: fr", "List2: gb_z.txt: gb",
				"Foo: 1_a1;1_c1: 1", "Foo: 2_b1: 2",

				"TargetStarted", "List1: en_b.txt: en",
				"List2: fr_x.txt: fr", "List2: gb_z.txt: gb",
				"Foo: 1_a1;1_c1: 1", "Foo: 2_b1: 2",

				"TargetStarted", "List1: gb_d.txt: gb",
				"List2: fr_x.txt: fr", "List2: gb_z.txt: gb",
				"Foo: 1_a1;1_c1: 1", "Foo: 2_b1: 2"
			}, "A2");
			CheckEngineEventCounts (testLogger, 3, 3, 18, 18);
		}

		[Test]
		public void TestTargetBatching5 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<Target Name=""foo"" Inputs=""@(List1)"" Outputs=""%(Culture).foo"">
					<Message Text=""TargetStarted""/>
				</Target>
			</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			bool res = project.Build ("foo");
			if (!res) {
				testLogger.DumpMessages ();
				Assert.Fail ("A1: Build failed");
			}
			Assert.AreEqual (1, testLogger.CheckAny ("TargetStarted", MessageImportance.Normal),
				"A2: Target should've been skipped because of no inputs");
			CheckEngineEventCounts (testLogger, 1, 1, 0, 0);
		}

		[Test]
		public void TestTargetBatching6 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<List1 Include=""fr_a.txt""><Culture>fr</Culture></List1>
					<List1 Include=""en_b.txt""><Culture>en</Culture></List1>
					<List1 Include=""fr_c.txt""><Culture>fr</Culture></List1>
					<List1 Include=""gb_d.txt""><Culture>gb</Culture></List1>

					<List3 Include=""fr_x.txt""><Culture>fr</Culture></List3>
					<List3 Include=""gb_z.txt""><Culture>gb</Culture></List3>

					<Foo Include=""1_a1""><Md>1</Md></Foo>
					<Foo Include=""2_b1""><Md>2</Md></Foo>
					<Foo Include=""1_c1""><Md>1</Md></Foo>
				</ItemGroup>

				<Target Name=""foo"" Inputs=""@(List1);@(List2)"" Outputs=""%(Culture).foo"">
					<Message Text=""TargetStarted"" />
					<Message Text=""List1: %(List1.Culture), List2: %(List2.Culture)"" />
					<Message Text=""List2: @(List2), Culture: %(Culture)"" />
					<Message Text=""List3: @(List3), Culture: %(Culture)"" />
				</Target>
			</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			bool res = project.Build ("foo");
			if (!res) {
				testLogger.DumpMessages ();
				Assert.Fail ("A1: Build failed");
			}

			CheckLoggedMessagesInOrder (testLogger, new string [] {
				"TargetStarted",
				"List1: fr, List2: ",
				"List2: , Culture: ",
				"List3: fr_x.txt, Culture: fr",
				"List3: gb_z.txt, Culture: gb",

				"TargetStarted",
				"List1: en, List2: ",
				"List2: , Culture: ",
				"List3: fr_x.txt, Culture: fr",
				"List3: gb_z.txt, Culture: gb",

				"TargetStarted",
				"List1: gb, List2: ",
				"List2: , Culture: ",
				"List3: fr_x.txt, Culture: fr",
				"List3: gb_z.txt, Culture: gb"
			}, "A2");
			CheckEngineEventCounts (testLogger, 3, 3, 15, 15);
		}

		[Test]
		public void TestTargetBatching7 ()
		{
			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<List1 Include=""fr_a.txt""><Culture>fr</Culture></List1>
					<List1 Include=""en_b.txt""><Culture>en</Culture></List1>
					<List1 Include=""fr_c.txt""><Culture>fr</Culture></List1>
					<List1 Include=""gb_d.txt""><Culture>gb</Culture></List1>

					<List2 Include=""fr_x.txt""><Culture>fr</Culture></List2>
					<List2 Include=""gb_z.txt""><Culture>gb</Culture></List2>

					<Foo Include=""1_a1""><Md>1</Md></Foo>
					<Foo Include=""2_b1""><Md>2</Md></Foo>
					<Foo Include=""1_c1""><Md>1</Md></Foo>
				</ItemGroup>

				<Target Name=""foo"" Inputs=""@(List1);@(List2)"" Outputs=""%(Culture).foo"">
					<Message Text=""TargetStarted"" />
					<Message Text=""List1: @(List1), List2: @(List2)""/>
				</Target>
			</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (projectString);
			bool res = project.Build ("foo");
			if (!res) {
				testLogger.DumpMessages ();
				Assert.Fail ("A1: Build failed");
			}

			CheckLoggedMessagesInOrder (testLogger, new string [] {
				"TargetStarted",
				"List1: fr_a.txt;fr_c.txt, List2: fr_x.txt",

				"TargetStarted",
				"List1: en_b.txt, List2: ",

				"TargetStarted",
				"List1: gb_d.txt, List2: gb_z.txt"
			}, "A2");
			CheckEngineEventCounts (testLogger, 3, 3, 6, 6);
		}

		void CheckLoggedMessagesInOrder (TestMessageLogger logger, string [] values, string prefix)
		{
			try {
				for (int i = 0; i < values.Length; i++) {
					logger.CheckLoggedMessageHead (values [i], prefix + "#" + i);
				}
				if (logger.NormalMessageCount > 0)
					Assert.Fail ("{0}: Expected {1} messages, but found {2}",
						prefix, values.Length, values.Length + logger.NormalMessageCount);
			} catch (NUnit.Framework.AssertionException) {
				logger.DumpMessages ();
				throw;
			}
		}

		void CheckMessage (TestMessageLogger logger, string culture, string items, string id)
		{
			logger.CheckLoggedMessageHead (String.Format ("Culture: {0} -- ResXFile: {1}", culture, items), id);
		}

		void CheckMessage2 (TestMessageLogger logger, string culture, string resx_files, string nonresx_files, string id)
		{
			logger.CheckLoggedMessageHead (String.Format ("Culture: {0} -- ResXFiles: {1} NonResXFiles: {2}", culture, resx_files, nonresx_files), id);
		}

		void CheckLoggedMessageAny (TestMessageLogger logger, string expected, string id)
		{
			if (logger.CheckAny (expected, MessageImportance.Normal) == 1)
				Assert.Fail ("{0}: Expected message '{1}' was not emitted.", id, expected);
		}

		void CheckEngineEventCounts (TestMessageLogger logger, int target_start, int target_finish, int task_start, int task_finish)
		{
			Assert.AreEqual (target_start, logger.TargetStarted, "TargetStarted event count doesn't match");
			Assert.AreEqual (target_finish, logger.TargetFinished, "TargetFinished event count doesn't match");
			Assert.AreEqual (task_start, logger.TaskStarted, "TaskStarted event count doesn't match");
			Assert.AreEqual (task_finish, logger.TaskFinished, "TaskFinished event count doesn't match");
		}
	}
}
