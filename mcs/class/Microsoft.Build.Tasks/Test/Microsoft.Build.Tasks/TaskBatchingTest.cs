//
// TaskBatchingTest.cs
//
// Author:
//   Ankit Jain (jankit@novell.com)
//
// Copyright 2008 Novell, Inc (http://www.novell.com)
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
			Assert.IsFalse (project.Build ("ShowMessage"), "A1: Build should have failed");
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

			CheckLoggedMessageHead (testLogger, "ResXFiles: Item1;Item2 NonResXFiles: ", "A2");
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
		}


		void CheckMessage (TestMessageLogger logger, string culture, string items, string id)
		{
			CheckLoggedMessageHead (logger, String.Format ("Culture: {0} -- ResXFile: {1}", culture, items), id);
		}

		void CheckMessage2 (TestMessageLogger logger, string culture, string resx_files, string nonresx_files, string id)
		{
			CheckLoggedMessageHead (logger, String.Format ("Culture: {0} -- ResXFiles: {1} NonResXFiles: {2}", culture, resx_files, nonresx_files), id);
		}

		void CheckLoggedMessageHead (TestMessageLogger logger, string expected, string id)
		{
			string actual;
			int result = logger.CheckHead (expected, MessageImportance.Normal, out actual);
			if (result == 1)
				Assert.Fail ("{0}: Expected message '{1}' was not emitted.", id, expected);
			if (result == 2)
				Assert.AreEqual (expected, actual, id);
		}

		void CheckLoggedMessageAny (TestMessageLogger logger, string expected, string id)
		{
			if (logger.CheckAny (expected, MessageImportance.Normal) == 1)
				Assert.Fail ("{0}: Expected message '{1}' was not emitted.", id, expected);
		}
	}
}
