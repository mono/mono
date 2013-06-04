//
// TargetTest.cs
//
// Authors:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Andres G. Aragoneses (knocte@gmail.com)
//
// (C) 2006 Marek Sieradzki
// (C) 2012 Andres G. Aragoneses
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
using MonoTests.Microsoft.Build.Tasks;
using NUnit.Framework;
using System.IO;
using System.Xml;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class TargetTest {
		
		Engine			engine;
		Project			project;
		
		[Test]
		public void TestFromXml1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='Target'>
					</Target>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Target[] t = new Target [1];
			project.Targets.CopyTo (t, 0);

			Assert.AreEqual (String.Empty, t [0].Condition, "A1");
			Assert.AreEqual (String.Empty, t [0].DependsOnTargets, "A2");
			Assert.IsFalse (t [0].IsImported, "A3");
			Assert.AreEqual ("Target", t [0].Name, "A4");
		}

		[Test]
		public void TestFromXml2 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='Target' Condition='false' DependsOnTargets='X' >
					</Target>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Target[] t = new Target [1];
			project.Targets.CopyTo (t, 0);

			Assert.AreEqual ("false", t [0].Condition, "A1");
			Assert.AreEqual ("X", t [0].DependsOnTargets, "A2");

			t [0].Condition = "true";
			t [0].DependsOnTargets = "A;B";

			Assert.AreEqual ("true", t [0].Condition, "A3");
			Assert.AreEqual ("A;B", t [0].DependsOnTargets, "A4");
		}

		[Test]
		public void TestAddNewTask1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='Target' >
					</Target>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Target[] t = new Target [1];
			project.Targets.CopyTo (t, 0);

			BuildTask bt = t [0].AddNewTask ("Message");

			Assert.AreEqual ("Message", bt.Name, "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddNewTask2 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='Target' >
					</Target>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Target[] t = new Target [1];
			project.Targets.CopyTo (t, 0);

			t [0].AddNewTask (null);
		}

		[Test]
		public void TestGetEnumerator ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='Target' >
						<Message Text='text' />
						<Warning Text='text' />
					</Target>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Target[] t = new Target [1];
			project.Targets.CopyTo (t, 0);

			IEnumerator e = t [0].GetEnumerator ();
			e.MoveNext ();
			Assert.AreEqual ("Message", ((BuildTask) e.Current).Name, "A1");
			e.MoveNext ();
			Assert.AreEqual ("Warning", ((BuildTask) e.Current).Name, "A2");
			Assert.IsFalse (e.MoveNext (), "A3");
		}

		[Test]
		public void TestOutOfRangeElementsOfTheEnumerator()
		{
			string documentString =
				@"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='A'>
						<Message Text='text' />
					</Target>
				</Project>";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsFalse (project.Targets == null, "A1");
			Assert.AreEqual (1, project.Targets.Count, "A2");

			Target target = project.Targets ["A"];
			Assert.IsFalse (target == null, "A3");

			IEnumerator e = target.GetEnumerator ();

			bool thrown = false;
			try {
				var name = ((BuildTask)e.Current).Name;
			} catch (InvalidOperationException) { // "Enumeration has not started. Call MoveNext"
				thrown = true;
			}
			if (!thrown)
				Assert.Fail ("A4: Should have thrown IOE");


			Assert.AreEqual (true, e.MoveNext (), "A5");
			Assert.AreEqual ("Message", ((BuildTask)e.Current).Name, "A6");
			Assert.AreEqual (false, e.MoveNext (), "A7");
			try {
				var name = ((BuildTask) e.Current).Name;
			} catch (InvalidOperationException) { //"Enumeration already finished."
				return;
			}
			Assert.Fail ("A8: Should have thrown IOE, because there's only one buidTask");
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestOnError1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='A'>
						<OnError ExecuteTargets='B' />
						<Error Text='text' />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

		[Test]
		public void TestRemoveTask1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='Target' >
						<Message Text='text' />
						<Warning Text='text' />
					</Target>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Target[] t = new Target [1];
			project.Targets.CopyTo (t, 0);

			IEnumerator e = t [0].GetEnumerator ();
			e.MoveNext ();
			t [0].RemoveTask ((BuildTask) e.Current);
			e = t [0].GetEnumerator ();
			e.MoveNext ();
			Assert.AreEqual ("Warning", ((BuildTask) e.Current).Name, "A1");
			Assert.IsFalse (e.MoveNext (), "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestRemoveTask2 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='Target' >
					</Target>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Target[] t = new Target [1];
			project.Targets.CopyTo (t, 0);
			t [0].RemoveTask (null);
		}

		[Test]
		public void TestTargetOutputs1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
			<ItemGroup>
				<fruit Include=""apple""/>
				<fruit Include=""rhubarb""/>
				<fruit Include=""apricot""/>
			</ItemGroup>

			<Target Name=""Main"">
				<CallTarget Targets=""foo"">
					<Output TaskParameter=""TargetOutputs"" ItemName=""AllOut""/>
				</CallTarget>

				<CallTarget Targets=""foo"">
					<Output TaskParameter=""TargetOutputs"" ItemName=""AllOut""/>
				</CallTarget>
				<Message Text=""AllOut: @(AllOut)""/>
			</Target>

			<Target Name=""foo"" Outputs=""@(FooItem)"">
				<Message Text=""foo called""/>
				<CreateItem Include=""%(fruit.Identity)"">
					<Output TaskParameter=""Include"" ItemName=""FooItem""/>
				</CreateItem>
				<Message Text=""FooItem: @(FooItem)""/>
			</Target>
		</Project>";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			bool result = project.Build ("Main");
			if (!result) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			try {
				Assert.AreEqual (3, logger.NormalMessageCount, "Expected number of messages");
				logger.CheckLoggedMessageHead ("foo called", "A1");
				logger.CheckLoggedMessageHead ("FooItem: apple;rhubarb;apricot", "A2");
				logger.CheckLoggedMessageHead ("AllOut: apple;rhubarb;apricot;apple;rhubarb;apricot", "A3");
				Assert.AreEqual (0, logger.NormalMessageCount, "Extra messages found");

				Assert.AreEqual (2, logger.TargetStarted, "TargetStarted count");
				Assert.AreEqual (2, logger.TargetFinished, "TargetFinished count");
				Assert.AreEqual (8, logger.TaskStarted, "TaskStarted count");
				Assert.AreEqual (8, logger.TaskFinished, "TaskFinished count");

			} catch (AssertionException) {
				logger.DumpMessages ();
				throw;
			}
		}

#if NET_3_5
		bool Build (string projectXml, ILogger logger)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				var reader = new StringReader (projectXml);
				var xml = XmlReader.Create (reader);
				return BuildOnWindows (xml, logger);
			} else {
				return BuildOnLinux (projectXml, logger);
			}
		}

		bool BuildOnWindows (XmlReader reader, ILogger logger)
		{
			var type = Type.GetType ("Microsoft.Build.Evaluation.ProjectCollection, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

			var prop = type.GetProperty ("GlobalProjectCollection");
			var coll = prop.GetValue (null);
				
			var loadProject = coll.GetType ().GetMethod (
					"LoadProject", new Type[] { typeof (XmlReader), typeof (string) });
			var proj = loadProject.Invoke (coll, new object[] { reader, "4.0" });
				
			var build = proj.GetType ().GetMethod ("Build", new Type[] { typeof (string), typeof (ILogger[]) });
			var ret = (bool)build.Invoke (proj, new object[] { "Main", new ILogger[] { logger }});
			return ret;
		}

		bool BuildOnLinux (string projectXml, ILogger logger)
		{
			var engine = new Engine (Consts.BinPath);
			var project = engine.CreateNewProject ();
			project.LoadXml (projectXml);
			
			engine.RegisterLogger (logger);
			
			return project.Build ("Main");
		}

		TestMessageLogger CreateLogger (string projectXml)
		{
			var logger = new TestMessageLogger ();
			var result = Build (projectXml, logger);

			if (!result) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			return logger;
		}

		void ItemGroupInsideTarget (string xml, params string[] messages)
		{
			var logger = CreateLogger (xml);
			
			try
			{
				Assert.AreEqual(messages.Length, logger.NormalMessageCount, "Expected number of messages");
				for (int i = 0; i < messages.Length; i++)
					logger.CheckLoggedMessageHead (messages [i], i.ToString ());
				Assert.AreEqual(0, logger.NormalMessageCount, "Extra messages found");
				
				Assert.AreEqual(1, logger.TargetStarted, "TargetStarted count");
				Assert.AreEqual(1, logger.TargetFinished, "TargetFinished count");
				Assert.AreEqual(messages.Length, logger.TaskStarted, "TaskStarted count");
				Assert.AreEqual(messages.Length, logger.TaskFinished, "TaskFinished count");
			}
			catch (AssertionException)
			{
				logger.DumpMessages();
				throw;
			}
		}

		[Test]
		public void BuildProjectWithItemGroupInsideTarget ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
					<fruit Include=""apple""/>
						<fruit Include=""apricot""/>
					</ItemGroup>

					<Target Name=""Main"">
						<ItemGroup>
							<fruit Include=""raspberry"" />
						</ItemGroup>
						<Message Text=""%(fruit.Identity)""/>
					</Target>
				</Project>", "apple", "apricot", "raspberry");
		}
		
		[Test]
		public void BuildProjectWithItemGroupInsideTarget2 ()
		{
			ItemGroupInsideTarget (
				@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
					<ItemGroup>
						<A Include='1'>
							<Sub>Foo</Sub>
						</A>
					</ItemGroup>
					<PropertyGroup>
						<Foo>Bar</Foo>
					</PropertyGroup>

					<Target Name='Main'>
						<ItemGroup>
							<A Include='2'>
								<Sub>$(Foo);Hello</Sub>
							</A>
						</ItemGroup>
				
						<Message Text='@(A)' />
						<Message Text='%(A.Sub)' />
					</Target>
				</Project>", "1;2", "Foo", "Bar;Hello");
		}
		
		[Test]
		public void BuildProjectWithPropertyGroupInsideTarget ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A>Foo</A>
						<B>Bar</B>
					</PropertyGroup>

					<Target Name=""Main"">
						<Message Text='$(A)' />
						<PropertyGroup>
							<A>$(B)</A>
						</PropertyGroup>
						<Message Text='$(A)' />
					</Target>
				</Project>", "Foo", "Bar");
		}

		[Test]
		public void BuildProjectWithPropertyGroupInsideTarget2 ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A>Foo</A>
						<B>Bar</B>
					</PropertyGroup>

					<Target Name=""Main"">
						<Message Text='$(A)' />
						<PropertyGroup Condition='true'>
							<B Condition='false'>False</B>
						</PropertyGroup>
						<PropertyGroup Condition='true'>
							<A>$(B)</A>
						</PropertyGroup>
						<Message Text='$(A)' />
						<Message Text='$(B)' />
						<PropertyGroup>
							<A Condition='$(A) == $(B)'>Equal</A>
						</PropertyGroup>
						<Message Text='$(A)' />
					</Target>
				</Project>", "Foo", "Bar", "Bar", "Equal");
		}

		[Test]
		public void ItemGroupInsideTarget_ModifyMetadata ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Server Include='Server1'>
							<AdminContact>Mono</AdminContact>
						</Server>
						<Server Include='Server2'>
							<AdminContact>Mono</AdminContact>
						</Server>
						<Server Include='Server3'>
							<AdminContact>Root</AdminContact>
						</Server>
					</ItemGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Server Condition=""'%(Server.AdminContact)' == 'Mono'"">
								<AdminContact>Monkey</AdminContact>
							</Server>
						</ItemGroup>
					
						<Message Text='%(Server.Identity) : %(Server.AdminContact)' />
						</Target>
					</Project>", "Server1 : Monkey", "Server2 : Monkey", "Server3 : Root");
		}

		[Test]
		public void ItemGroupInsideTarget_RemoveItem ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Foo Include='A;B;C;D' />
					</ItemGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Foo Remove='B' />
						</ItemGroup>

						<Message Text='@(Foo)' />
					</Target>
				</Project>", "A;C;D");
		}

		[Test]
		public void ItemGroupInsideTarget_DontKeepDuplicates ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Foo Include='A;B' />
						<Foo Include='C'>
							<Hello>World</Hello>
						</Foo>
						<Foo Include='D'>
							<Hello>Boston</Hello>
						</Foo>
					</ItemGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Foo Include='B;C;D' KeepDuplicates='false'>
								<Hello>Boston</Hello>
							</Foo>
						</ItemGroup>
				
						<Message Text='@(Foo)' />
					</Target>
				</Project>", "A;B;C;D;B;C");
		}

		[Test]
		public void ItemGroupInsideTarget_RemoveMetadata ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Foo Include='A' />
						<Foo Include='B'>
							<Hello>World</Hello>
						</Foo>
						<Foo Include='C'>
							<Hello>Boston</Hello>
						</Foo>
						<Foo Include='D'>
							<Test>Monkey</Test>
						</Foo>
					</ItemGroup>
					<PropertyGroup>
						<Foo>Hello</Foo>
					</PropertyGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Bar Include='@(Foo)' RemoveMetadata='$(Foo)' />
							<Bar Include='E'>
								<Hello>Monkey</Hello>
							</Bar>
						</ItemGroup>
				
						<Message Text='%(Bar.Identity)' Condition=""'%(Bar.Hello)' != ''""/>
					</Target>
				</Project>", "E");
		}

		[Test]
		public void ItemGroupInsideTarget_RemoveMetadata2 ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Foo Include='A' />
						<Foo Include='B'>
							<Hello>World</Hello>
						</Foo>
						<Foo Include='C'>
							<Hello>Boston</Hello>
						</Foo>
						<Foo Include='D'>
							<Test>Monkey</Test>
						</Foo>
					</ItemGroup>
					<PropertyGroup>
					<Foo>Hello</Foo>
					</PropertyGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Foo RemoveMetadata='$(Foo)' />
							<Foo Include='E'>
								<Hello>Monkey</Hello>
							</Foo>
						</ItemGroup>
				
					<Message Text='%(Foo.Identity)' Condition=""'%(Foo.Hello)' != ''""/>
					</Target>
				</Project>", "E");
		}

		[Test]
		public void ItemGroupInsideTarget_KeepMetadata ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Foo Include='A' />
						<Foo Include='B'>
							<Hello>World</Hello>
						</Foo>
						<Foo Include='C'>
							<Hello>Boston</Hello>
						</Foo>
						<Foo Include='D'>
							<Test>Monkey</Test>
						</Foo>
					</ItemGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Foo KeepMetadata='Test' />
							<Foo Include='E'>
								<Hello>Monkey</Hello>
							</Foo>
						</ItemGroup>
				
						<Message Text='%(Foo.Identity)' Condition=""'%(Foo.Test)' != ''""/>
					</Target>
				</Project>", "D");
		}

		[Test]
		public void ItemGroupInsideTarget_Batching ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='Main'>
						<ItemGroup>
							<Foo Include='A;B' />
							<All Include='%(Foo.Identity)' />
						</ItemGroup>
						<Message Text='%(All.Identity)' />
					</Target>
				</Project>", "A", "B");
		}

		[Test]
		public void ItemGroupInsideTarget_Condition ()
		{
			ItemGroupInsideTarget (
				@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
					<PropertyGroup>
						<Summer>true</Summer>
					</PropertyGroup>
					<ItemGroup>
						<Weather Include='Sun;Rain' />
					</ItemGroup>
				
					<Target Name='Main'>
						<ItemGroup Condition=""'$(Summer)' != 'true'"">
							<Weather Include='Snow' />
						</ItemGroup>
						<Message Text='%(Weather.Identity)' />
					</Target>
				</Project>", "Sun", "Rain");
		}
		#endif

		[Test]
		public void TestTargetOutputsIncludingMetadata ()
		{
			Engine engine;
			Project project;

			string documentString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
			<ItemGroup>
				<fruit Include=""apple""><md>a</md></fruit>
				<fruit Include=""rhubarb""><md>b</md></fruit>
				<fruit Include=""apricot""><md>c</md></fruit>
			</ItemGroup>

			<Target Name=""Main"">
				<CallTarget Targets=""foo"">
					<Output TaskParameter=""TargetOutputs"" ItemName=""AllOut""/>
				</CallTarget>

				<CallTarget Targets=""foo"">
					<Output TaskParameter=""TargetOutputs"" ItemName=""AllOut""/>
				</CallTarget>
				<Message Text=""AllOut: @(AllOut) metadata: %(AllOut.md)""/>
			</Target>

			<Target Name=""foo"" Outputs=""@(FooItem)"">
				<Message Text=""foo called""/>
				<CreateItem Include=""@(fruit)"">
					<Output TaskParameter=""Include"" ItemName=""FooItem""/>
				</CreateItem>
				<Message Text=""FooItem: @(FooItem) metadata: %(FooItem.md)""/>
			</Target>
		</Project>";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			bool result = project.Build ("Main");
			if (!result) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			try {
				logger.CheckLoggedMessageHead ("foo called", "A1");
				logger.CheckLoggedMessageHead ("FooItem: apple metadata: a", "A2");
				logger.CheckLoggedMessageHead ("FooItem: rhubarb metadata: b", "A3");
				logger.CheckLoggedMessageHead ("FooItem: apricot metadata: c", "A4");

				logger.CheckLoggedMessageHead ("AllOut: apple;apple metadata: a", "A5");
				logger.CheckLoggedMessageHead ("AllOut: rhubarb;rhubarb metadata: b", "A6");
				logger.CheckLoggedMessageHead ("AllOut: apricot;apricot metadata: c", "A7");

				Assert.AreEqual (0, logger.NormalMessageCount, "Extra messages found");

				Assert.AreEqual (2, logger.TargetStarted, "TargetStarted count");
				Assert.AreEqual (2, logger.TargetFinished, "TargetFinished count");
				Assert.AreEqual (10, logger.TaskStarted, "TaskStarted count");
				Assert.AreEqual (10, logger.TaskFinished, "TaskFinished count");

			} catch (AssertionException) {
				logger.DumpMessages ();
				throw;
			}
		}

		[Test]
		public void TestOverridingTargets ()
		{
			Engine engine;
			Project project;

			string second = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<Target Name='BeforeBuild'/>
				<Target Name='AfterBuild'/>
				<Target Name='Build' DependsOnTargets='BeforeBuild'>
					<Message Text='Build executing'/>
					<CallTarget Targets='AfterBuild'/>
				</Target>
		</Project>";
			using (StreamWriter sw = new StreamWriter (Path.Combine ("Test", Path.Combine ("resources", "second.proj")))) {
				sw.Write (second);
			}

			string documentString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<Target Name='AfterBuild'>
					<Message Text='Overriding AfterBuild'/>
				</Target>

				<Import Project='Test/resources/second.proj'/>
				<Target Name='BeforeBuild'>
					<Message Text='Overriding BeforeBuild'/>
				</Target>
		</Project>";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			bool result = project.Build ("Build");
			if (!result) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("Overriding BeforeBuild", "A1");
			logger.CheckLoggedMessageHead ("Build executing", "A1");

			Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");
		}

#if NET_4_0
		[Test]
		[Category ("NotDotNet")]
		public void TestBeforeAndAfterTargets ()
		{
			Engine engine;
			Project project;

			string projectString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
			  <Target Name=""DefaultBeforeTarget1"" BeforeTargets=""Default"">
			    <Message Text=""Hello from DefaultBeforeTarget1""/>
			  </Target>

			  <Target Name=""DefaultBeforeTarget2"" BeforeTargets=""Default;Default;NonExistant"">
			    <Message Text=""Hello from DefaultBeforeTarget2""/>
			  </Target>


			  <Target Name=""DefaultAfterTarget"" AfterTargets=""Default  ; Foo"">
			    <Message Text=""Hello from DefaultAfterTarget""/>
			  </Target>

			  <Target Name=""Default"" DependsOnTargets=""DefaultDependsOn"">
			    <Message Text=""Hello from Default""/>
			  </Target>

			  <Target Name=""DefaultDependsOn"">
			    <Message Text=""Hello from DefaultDependsOn""/>
			  </Target>
			</Project>";

			engine = new Engine ();
			project = engine.CreateNewProject ();
			project.LoadXml (projectString);

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			if (!project.Build ("Default")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("Hello from DefaultDependsOn", "A1");
			logger.CheckLoggedMessageHead ("Hello from DefaultBeforeTarget1", "A1");
			logger.CheckLoggedMessageHead ("Hello from DefaultBeforeTarget2", "A1");
			logger.CheckLoggedMessageHead ("Hello from Default", "A1");
			logger.CheckLoggedMessageHead ("Hello from DefaultAfterTarget", "A1");

			Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected messages found");

			//warnings for referencing unknown targets: NonExistant and Foo
			Assert.AreEqual (2, logger.WarningsCount, "Expected warnings not raised");
		}
#endif

	}
}
