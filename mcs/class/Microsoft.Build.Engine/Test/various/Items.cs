//
// Items.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
//
// (C) 2006 Marek Sieradzki
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
using System.Text;
using System.Xml;
using Microsoft.Build.BuildEngine;
using NUnit.Framework;
using System.IO;
using Microsoft.Build.Framework;

using MonoTests.Helpers;

namespace MonoTests.Microsoft.Build.BuildEngine.Various {
	[TestFixture]
	public class Items {
		private string GetItems (Project proj, string name)
		{
			BuildItemGroup big = proj.GetEvaluatedItemsByName (name);
			string str = String.Empty;
			if (big == null)
				return str;

			foreach (BuildItem bi in big) {
				if (str == String.Empty)
					str = bi.FinalItemSpec;
				else 
					str += ";" + bi.FinalItemSpec;
			}
			
			return str;
		}

		private void CheckItems (Project proj, string name, string prefix, params string [] values)
		{
			BuildItemGroup big = proj.GetEvaluatedItemsByName (name);
			if (big == null) {
				Assert.Fail ("{0}: Item corresponding '{1}' not found.", prefix, name);
				return;
			}

			if (values.Length != big.Count) {
				Console.Write ("Expected> ");
				foreach (string s in values)
					Console.Write ("{0}|", s);
				Console.WriteLine ();
				Console.Write ("Actual> ");
				foreach (BuildItem item in big)
					Console.Write ("{0}|", item.FinalItemSpec);
				Console.WriteLine ();
				Assert.AreEqual (values.Length, big.Count, String.Format ("{0}: Number of items", prefix));
			}
			for (int i = 0; i < values.Length; i ++)
				Assert.AreEqual (values [i], big [i].FinalItemSpec,
					String.Format ("{0}: Item named {1}, numbered {2}", prefix, name, i));
		}

		[Test]
		public void TestItems1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item0 Include='A' />
						<Item1 Include='A;B;C' />
						<Item2 Include='@(Item1);A;D' />
						<Item3 Include='@(Item2)' Exclude='A' />
						<Item4 Include='@(Item1);Q' Exclude='@(Item2)' />
						<Item5 Include='@(Item1)' Exclude='@(Item2)' />
						<Item6 Include='@(Item2)' Exclude='@(Item1)' />
						<Item7 Include='@(item_that_doesnt_exist)' />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);
			CheckItems (proj, "Item0", "A1", "A");
			CheckItems (proj, "Item1", "A2", "A", "B", "C");
			CheckItems (proj, "Item2", "A3", "A", "B", "C", "A", "D");
			CheckItems (proj, "Item3", "A4", "B", "C", "D");
			CheckItems (proj, "Item4", "A5", "Q");
			CheckItems (proj, "Item5", "A6");
			CheckItems (proj, "Item6", "A7", "D");
			CheckItems (proj, "Item7", "A8");
		}

		[Test]
		public void TestItems2 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<Item1 Include='A;B;C' />
						<Item2 Include=""@(Item1,'-')"" />
						<Item3 Include=""@(Item1,'xx')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			CheckItems (proj, "Item2", "A1", "A-B-C");
			CheckItems (proj, "Item3", "A2", "AxxBxxC");
		}

		[Test]
		// item with 1. item ref with a separator and 2. another item ref
		public void TestItems2a () {
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<Item0 Include='D'/>
						<Item1 Include='A;B;C' />
						<Item2 Include=""@(Item1,'-');@(Item0)"" />
						<Item3 Include=""@(Item1,'xx')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			CheckItems (proj, "Item2", "A1", "A-B-C", "D");
			CheckItems (proj, "Item3", "A2", "AxxBxxC");
		}

		[Test]
		public void TestInheritedMetadataFromItemRefs () {
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<Item0 Include='D'>
							<MD0>Val0</MD0>
						</Item0>
						<Item1 Include='A;@(Item0)' >
							<MD1>Val1</MD1>
						</Item1>
						<Item2 Include=""@(Item1,'-')"" />
						<Item3 Include=""@(Item1);Z"" />
					</ItemGroup>

						<Target Name=""Main"">
		<Message Text=""Item2: %(Item2.Identity) MD0: %(Item2.MD0) MD1: %(Item2.MD1)""/>
		<Message Text=""Item3: %(Item3.Identity) MD0: %(Item3.MD0) MD1: %(Item3.MD1)""/>
	</Target>
				</Project>
			";

			proj.LoadXml (documentString);

			CheckItems (proj, "Item2", "A1", "A-D");
			CheckItems (proj, "Item3", "A2", "A", "D", "Z");

			if (!proj.Build ("Main")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("Item2: A-D MD0:  MD1: ", "A4");

			logger.CheckLoggedMessageHead ("Item3: A MD0:  MD1: Val1", "A5");
			logger.CheckLoggedMessageHead ("Item3: D MD0: Val0 MD1: Val1", "A6");
			logger.CheckLoggedMessageHead ("Item3: Z MD0:  MD1: ", "A7");

			Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");
		}

		[Test]
		public void TestInheritedMetadataFromItemRefs2 () {
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<Item5 Include='ZZ'>
							<MD5>Val5</MD5>
						</Item5>

						<Item0 Include='D'>
							<MD0>Val0</MD0>
						</Item0>
						<Item1 Include='A;@(Item0)' >
							<MD1>Val1</MD1>
						</Item1>
						<Item2 Include=""@(Item1,'-');@(Item5)"" />
					</ItemGroup>

						<Target Name=""Main"">
		<Message Text=""Item2: %(Item2.Identity) MD0: %(Item2.MD0) MD1: %(Item2.MD1) MD5: %(Item2.MD5)""/>
	</Target>
				</Project>
			";

			proj.LoadXml (documentString);

			CheckItems (proj, "Item2", "A1", "A-D", "ZZ");

			if (!proj.Build ("Main")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("Item2: A-D MD0:  MD1:  MD5: ", "A4");
			logger.CheckLoggedMessageHead ("Item2: ZZ MD0:  MD1:  MD5: Val5", "A5");
			Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");
		}

		[Test]
		public void TestItems3 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<Item1 Include='A;B;C' />
						<Item2 Include=""@(Item1, '-')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			CheckItems (proj, "Item2", "A1", "A-B-C");
		}
	

		[Test]
		public void TestItems4 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<Prop1>@(Item0)</Prop1>
					</PropertyGroup>
					<ItemGroup>
						<Item0 Include=""Foo""/>
						<Item1 Include='A;B;C' />
						<Item2 Include=""A\B.txt;A\C.txt;B\B.zip;B\C.zip"" />
						<ItemT0 Include=""@(Item1)"" />
						<ItemT1 Include=""@(Item1->'%(Identity)')"" />
						<ItemT2 Include=""@(Item1->'%(Identity)%(Identity)')"" />
						<ItemT3 Include=""@(Item1->'(-%(Identity)-)')"" />
						<ItemT4 Include=""@(Item2->'%(Extension)')"" />
						<ItemT5 Include=""@(Item2->'%(Filename)/%(Extension)')"" />
						<ItemT6 Include=""@(Item2->'%(Extension)/$(Prop1)')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			//Assert.IsTrue (proj.Build (), "Build failed");

			Assert.AreEqual ("@(Item0)", proj.EvaluatedProperties["Prop1"].FinalValue, "A0");
			//Assert.AreEqual ("@(Item2->'%(Extension)/$(Prop1)')", proj.EvaluatedItems [7].FinalItemSpec, "B0");

			CheckItems (proj, "ItemT0", "A1", "A", "B", "C");
			CheckItems (proj, "ItemT1", "A1", "A", "B", "C");
			CheckItems (proj, "ItemT2", "A2", "AA", "BB", "CC");
			CheckItems (proj, "ItemT3", "A3", "(-A-)", "(-B-)", "(-C-)");
			CheckItems (proj, "ItemT4", "A4", ".txt", ".txt", ".zip", ".zip");
			CheckItems (proj, "ItemT5", "A5", "B/.txt", "C/.txt", "B/.zip", "C/.zip");
			CheckItems (proj, "ItemT6", "A6", ".txt/@(Item0)", ".txt/@(Item0)", ".zip/@(Item0)", ".zip/@(Item0)");
		}

		[Test]
		public void TestItems5 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item Include=""A\B.txt;A\C.txt;B\B.zip;B\C.zip"" />
						<ItemT Include=""@(Item->'%(RelativeDir)X\%(Filename)')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			string dir_a = Path.Combine ("A", "X");
			string dir_b = Path.Combine ("B", "X");
			CheckItems (proj, "ItemT", "A1", Path.Combine (dir_a, "B"), Path.Combine (dir_a, "C"),
								Path.Combine (dir_b, "B"), Path.Combine (dir_b, "C"));
		}

		[Test]
		public void TestItems6 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A>A</A>
					</PropertyGroup>

					<ItemGroup>
						<Item1 Include='A;B;C' />
						<Item2 Include='%(A.B)' />
						<Item3 Include='$(Z)' />
						<Item4 Include=""@(Item1, '$(A)')"" />
						<Item5 Include=""@(Item1, '%(A)')"" />
						<Item6 Include=""@(Item1, '@(A)')"" />
						<Item7 Include=""@(Item1-> '%(Filename)')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			CheckItems (proj, "Item2", "A1", "%(A.B)");
			CheckItems (proj, "Item3", "A2");
			CheckItems (proj, "Item4", "A3", "AABAC");
			CheckItems (proj, "Item5", "A4", "A%(A)B%(A)C");
			CheckItems (proj, "Item6", "A6", "A@(A)B@(A)C");
			CheckItems (proj, "Item7", "A6", "A", "B", "C");
		}

		// The expression "@(Item1, '@(A,'')')" cannot be used in this context. 
		// Item lists cannot be concatenated with other strings where an item list is expected. 
		// Use a semicolon to separate multiple item lists.
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		[Category ("NotWorking")]
		public void TestItems7 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item1 Include='A;B;C' />
						<Item7 Include=""@(Item1, '@(A,'')')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);
		}

		// The expression "@(Item1, '@(A->'')')" cannot be used in this context.
		// Item lists cannot be concatenated with other strings where an item list is expected.
		// Use a semicolon to separate multiple item lists.
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		[Category ("NotWorking")]
		public void TestItems8 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item1 Include='A;B;C' />
						<Item8 Include=""@(Item1, '@(A->'')')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);
		}

		// The expression "@(Item1, '@(A->'','')')" cannot be used in this context.
		// Item lists cannot be concatenated with other strings where an item list is expected.
		// Use a semicolon to separate multiple item lists.
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		[Category ("NotWorking")]
		public void TestItems9 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item1 Include='A;B;C' />
						<Item9 Include=""@(Item1, '@(A->'','')')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);
		}

		[Test]
		// test item metadata
		public void TestItems10 ()
		{
			string project_xml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<PropertyGroup>
		<Prop1>@(Item0)</Prop1>
		<Prop2>@(Ref1)</Prop2>
	</PropertyGroup>
	<ItemGroup>
		<Item0 Include=""Foo""/>
		<Ref1 Include=""File1"" />
		<IWithM Include=""A"">
			<Md>@(Item0)</Md>
			<Md2>$(Prop2)</Md2>
		</IWithM>
	</ItemGroup>

	<Target Name=""1"">
		<Message Text=""IWithM.md: %(IWithM.Md)""/>
		<Message Text=""IWithM.md2: %(IWithM.Md2)""/>

		<CreateItem Include=""Bar;Xyz"">
			<Output TaskParameter=""Include"" ItemName=""Item0""/>
		</CreateItem>
		
		<Message Text=""IWithM.md: %(IWithM.Md)""/>
		<Message Text=""IWithM.md2: %(IWithM.Md2)""/>
	</Target>
</Project>
";

			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			proj.LoadXml (project_xml);
			engine.RegisterLogger (logger);

			if (!proj.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("IWithM.md: Foo", "A1");
			logger.CheckLoggedMessageHead ("IWithM.md2: File1", "A2");

			logger.CheckLoggedMessageHead ("IWithM.md: Foo", "A3");
			logger.CheckLoggedMessageHead ("IWithM.md2: File1", "A4");
			Assert.AreEqual (0, logger.NormalMessageCount, "unexpected messages found");
		}

		[Test]
		// Test Item/prop references in conditions
		public void TestItems11 () {
			string project_xml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<PropertyGroup>
		<Prop1>@(Item0)</Prop1>
	</PropertyGroup>
	<ItemGroup>
		<Item0 Include=""Foo""/>
		<Item1 Include=""@(Item0)""/>

		<CondItem Condition=""'@(Item1)' == '@(Item0)'"" Include=""Equal to item0""/>
		<CondItem Condition=""'@(Item1)' == 'Foo'"" Include=""Equal to item0's value""/>

		<CondItem1 Condition=""'$(Prop1)' == '@(Item0)'"" Include=""Equal to item0""/>
		<CondItem1 Condition=""'$(Prop1)' == 'Foo'"" Include=""Equal to item0's value""/>
	</ItemGroup>

	<Target Name=""1"">
		<Message Text = ""CondItem: %(CondItem.Identity)""/>
		<Message Text = ""CondItem1: %(CondItem1.Identity)""/>
	</Target>
</Project>
";

			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			proj.LoadXml (project_xml);
			engine.RegisterLogger (logger);

			if (!proj.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("CondItem: Equal to item0", "A1");
			logger.CheckLoggedMessageHead ("CondItem: Equal to item0's value", "A2");
			logger.CheckLoggedMessageHead ("CondItem1: Equal to item0", "A3");
			logger.CheckLoggedMessageHead ("CondItem1: Equal to item0's value", "A4");
			Assert.AreEqual (0, logger.NormalMessageCount, "unexpected messages found");
		}

		[Test]
		// test properties and item refs, with dynamic properties/items
		public void TestItems12 ()
		{
			string project_xml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<PropertyGroup>
		<Prop2>@(Ref1)</Prop2>
	</PropertyGroup>
	<ItemGroup>
		<Ref1 Include=""File1"" />
		<Files Include=""@(Ref1)""/>
	</ItemGroup>

	<Target Name=""1"">
		<Message Text=""Prop2: $(Prop2)""/>
		
		<Message Text=""Files: @(Files)""/>
		<CreateItem Include=""foobar"">
			<Output TaskParameter=""Include"" ItemName=""Ref1""/>
		</CreateItem>
		<Message Text=""Files: @(Files)""/>

		<Message Text=""Prop2: $(Prop2)""/>
		<CreateProperty Value=""NewValue"">
			<Output TaskParameter=""Value"" PropertyName=""Prop2""/>
		</CreateProperty>
		<Message Text=""Prop2: $(Prop2)""/>
	</Target>
</Project>
";

			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			proj.LoadXml (project_xml);
			engine.RegisterLogger (logger);

			if (!proj.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("Prop2: File1", "A1");
			logger.CheckLoggedMessageHead ("Files: File1", "A1");
			logger.CheckLoggedMessageHead ("Files: File1", "A1");
			logger.CheckLoggedMessageHead ("Prop2: File1;foobar", "A1");
			logger.CheckLoggedMessageHead ("Prop2: NewValue", "A1");
			Assert.AreEqual (0, logger.NormalMessageCount, "unexpected messages found");
		}

		[Test]
		// test item refs in properties
		public void TestItems13 () {
			string project_xml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<PropertyGroup>
		<Prop1>@(Item0)</Prop1>
	</PropertyGroup>
	<ItemGroup>
		<Item0 Include=""Foo""/>
		<Item1 Include=""A\B.txt;A\C.txt;B\B.zip;B\C.zip"" />
		<Item2 Include=""@(Item1->'%(Extension)/$(Prop1)')"" />
	</ItemGroup>

	<Target Name='1'>
		<Message Text=""Item2: @(Item2)""/>
	</Target>
</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			proj.LoadXml (project_xml);
			engine.RegisterLogger (logger);

			if (!proj.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("Item2: .txt/@(Item0);.txt/@(Item0);.zip/@(Item0);.zip/@(Item0)", "A1");
			Assert.AreEqual (0, logger.NormalMessageCount, "unexpected messages found");
		}

		[Test]
		public void TestMetadataFromItemReferences () {
			string project_xml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<ItemGroup>
		<Item1 Include=""Item1Val1;Item1Val2"">
			<Item1Md>False</Item1Md>
		</Item1>
		<Item2 Include=""Val1;Val2;@(Item1);Val3"">
			<Name>Random name</Name>
		</Item2>
		<Item3 Include=""foo;bar;@(Item2);Last""/>
	</ItemGroup>

	<Target Name=""Main"">
		<CreateItem Include=""@(Item3)"">
			<Output TaskParameter=""Include""  ItemName=""Final""/>
		</CreateItem>

		<Message Text=""Final: %(Final.Identity) Item1Md: %(Final.Item1Md) Name: %(Final.Name)""/>
	</Target>
</Project>";

			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			proj.LoadXml (project_xml);
			engine.RegisterLogger (logger);

			if (!proj.Build ("Main")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			CheckItems (proj, "Final", "Z", "foo", "bar", "Val1", "Val2", "Item1Val1", "Item1Val2", "Val3", "Last");

			logger.CheckLoggedMessageHead ("Final: foo Item1Md:  Name: ", "A1");
			logger.CheckLoggedMessageHead ("Final: bar Item1Md:  Name: ", "A2");
			logger.CheckLoggedMessageHead ("Final: Val1 Item1Md:  Name: Random name", "A3");
			logger.CheckLoggedMessageHead ("Final: Val2 Item1Md:  Name: Random name", "A4");
			logger.CheckLoggedMessageHead ("Final: Item1Val1 Item1Md: False Name: Random name", "A5");
			logger.CheckLoggedMessageHead ("Final: Item1Val2 Item1Md: False Name: Random name", "A6");
			logger.CheckLoggedMessageHead ("Final: Val3 Item1Md:  Name: Random name", "A7");
			logger.CheckLoggedMessageHead ("Final: Last Item1Md:  Name: ", "A8");

			Assert.AreEqual (0, logger.NormalMessageCount, "unexpected messages found");
		}

		[Test]
		public void TestSelfRefrentialItems ()
		{
			string project_xml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<PropertyGroup>
		<Prop1>@(Item1);Val</Prop1>
		<Prop2>@(Item2)</Prop2>
		<Prop3>@(Item3)</Prop3>
	</PropertyGroup>
	<ItemGroup>
		<Item1 Include=""Item1OldVal""/>
		<Item1 Include=""@(Item1);$(Prop1)""/>

		<Item2 Include=""Item2OldVal""/>
		<Item2 Include=""@(Item2->'%(Identity)');$(Prop2)""/>

		<Item3 Include=""Item3OldVal""/>
		<Item3 Include=""@(Item3, '_');$(Prop3)""/>

		<Item4 Include=""@(Item4)""/>
	</ItemGroup>

	<Target Name=""1"">
		<Message Text=""Item1: %(Item1.Identity)""/>
		<Message Text=""Item2: %(Item2.Identity)""/>
		<Message Text=""Item3: %(Item3.Identity)""/>
		<Message Text=""%(Item4.Identity)""/>
		<Message Text=""Item4: %(Item4.Identity)""/>
	</Target>
</Project>
";

			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			proj.LoadXml (project_xml);
			engine.RegisterLogger (logger);

			if (!proj.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("Item1: Item1OldVal", "A1");
			logger.CheckLoggedMessageHead ("Item1: Val", "A2");
			logger.CheckLoggedMessageHead ("Item2: Item2OldVal", "A3");
			logger.CheckLoggedMessageHead ("Item3: Item3OldVal", "A4");
			logger.CheckLoggedMessageHead ("Item4: ", "A5");
			Assert.AreEqual (0, logger.NormalMessageCount, "unexpected messages found");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestEmptyItemsWithBatching ()
		{
			string project_xml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
			<UsingTask TaskName='StringTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
			<UsingTask TaskName='TestTask_TaskItem' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
			<UsingTask TaskName='TestTask_TaskItems' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
	<Target Name=""1"">
		<StringTestTask Property=""%(Item4.Identity)"">
			<Output TaskParameter=""OutputString"" PropertyName=""OutputString""/>
		</StringTestTask>
		<Message Text='output1: $(OutputString)'/>

		<StringTestTask Property=""  %(Item4.Identity)"">
			<Output TaskParameter=""OutputString"" PropertyName=""OutputString""/>
		</StringTestTask>
		<Message Text='output2: $(OutputString)'/>

		<StringTestTask Array=""%(Item4.Identity)"">
			<Output TaskParameter=""OutputString"" PropertyName=""OutputString""/>
		</StringTestTask>
		<Message Text='output3: $(OutputString)'/>

		<StringTestTask Array=""  %(Item4.Identity)"">
			<Output TaskParameter=""OutputString"" PropertyName=""OutputString""/>
		</StringTestTask>
		<Message Text='output4: $(OutputString)'/>


		<TestTask_TaskItem Property=""%(Item4.Identity)"">
			<Output TaskParameter=""Output"" PropertyName=""OutputString""/>
		</TestTask_TaskItem>
		<Message Text='output5: $(OutputString)'/>

		<TestTask_TaskItem Property=""  %(Item4.Identity)"">
			<Output TaskParameter=""Output"" PropertyName=""OutputString""/>
		</TestTask_TaskItem>
		<Message Text='output6: $(OutputString)'/>


		<TestTask_TaskItems Property=""  %(Item4.Identity)"">
			<Output TaskParameter=""Output"" PropertyName=""OutputString""/>
		</TestTask_TaskItems>
		<Message Text='output7: $(OutputString)'/>
	

		<!-- no space in property -->
		<TestTask_TaskItems Property=""%(Item4.Identity)"">
			<Output TaskParameter=""Output"" PropertyName=""OutputString""/>
		</TestTask_TaskItems>
		<Message Text='output8: $(OutputString)'/>

	</Target>
</Project>
";

			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			proj.LoadXml (project_xml);
			engine.RegisterLogger (logger);

			if (!proj.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("output1: property: null ## array: null", "A1");
			logger.CheckLoggedMessageHead ("output2: property:    ## array: null", "A2");
			logger.CheckLoggedMessageHead ("output3: property: null ## array: null", "A3");
			logger.CheckLoggedMessageHead ("output4: property: null ## array: null", "A4");

			logger.CheckLoggedMessageHead ("output5: null", "A5");
			logger.CheckLoggedMessageHead ("output6: null", "A6");
			logger.CheckLoggedMessageHead ("output7: null", "A7");
			logger.CheckLoggedMessageHead ("output8: null", "A8");

			Assert.AreEqual (0, logger.NormalMessageCount, "unexpected messages found");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestItemsInTarget1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='StringTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<PropertyGroup>
						<A>A</A>
						<B>@(A)g</B>
					</PropertyGroup>
					<ItemGroup>
						<A Include='A;B;C' />
					</ItemGroup>

					<Target Name='1'>
						<StringTestTask Property='@(A)@(Z)'>
							<Output TaskParameter='Property' PropertyName='P1' />
						</StringTestTask>
						<StringTestTask Property=""@(A,'')"">
							<Output TaskParameter='Property' PropertyName='P2' />
						</StringTestTask>
						<StringTestTask Property=""@(A,'@(A)')"">
							<Output TaskParameter='Property' PropertyName='P3' />
						</StringTestTask>
						<StringTestTask Property=""@(A,'$(A)')"">
							<Output TaskParameter='Property' PropertyName='P4' />
						</StringTestTask>
						<StringTestTask Property=""@(A,'@(A,'')')"">
							<Output TaskParameter='Property' PropertyName='P5' />
						</StringTestTask>
						<StringTestTask Property=""@(A,'$(B)')"">
							<Output TaskParameter='Property' PropertyName='P6' />
						</StringTestTask>
						<StringTestTask Property=""%(A.Filename)"">
							<Output TaskParameter='Property' ItemName='I1' />
						</StringTestTask>
						<StringTestTask Property=""@(A) %(Filename)"">
							<Output TaskParameter='Property' ItemName='I2' />
						</StringTestTask>
					</Target>
				</Project>
			";

			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger = new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);
			proj.LoadXml (documentString);
			if (!proj.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("build failed");
			}

			Assert.AreEqual ("A;B;C", proj.GetEvaluatedProperty ("P1"), "A1");
			Assert.AreEqual ("ABC", proj.GetEvaluatedProperty ("P2"), "A2");
			Assert.AreEqual ("A@(A)B@(A)C", proj.GetEvaluatedProperty ("P3"), "A3");
			Assert.AreEqual ("AABAC", proj.GetEvaluatedProperty ("P4"), "A4");
			Assert.AreEqual ("@(A,'ABC')", proj.GetEvaluatedProperty ("P5"), "A5");
			Assert.AreEqual ("A@(A)gB@(A)gC", proj.GetEvaluatedProperty ("P6"), "A6");
			CheckItems (proj, "I1", "A6", "A", "B", "C");
			CheckItems (proj, "I2", "A7", "A A", "B B", "C C");
		}



		[Test]
		[Category ("NotWorking")]
		public void TestItemsInTarget2 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='StringTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<ItemGroup>
						<A Include='A;B;C' />
					</ItemGroup>

					<Target Name='1'>
						<StringTestTask Property=""%(Filename)"">
							<Output TaskParameter='Property' ItemName='I2' />
						</StringTestTask>
					</Target>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.IsFalse (proj.Build ("1"), "A1");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestItemsInTarget3 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='StringTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<PropertyGroup>
						<A>A</A>
						<B>A;B</B>
						<C>A;;</C>
					</PropertyGroup>
					<ItemGroup>
						<A Include='A;B;C' />
					</ItemGroup>";

			documentString += CreateTargetFragment ("StringTestTask", "Array", "Array", "I",
					new string [] {
						"$(A)$(A)",
						"$(B)$(B)",
						"$(C)",
						"$(C)$(C)",
						"@(A);$(C)",
						"@(A);A;B;C",
						"Foo;@(A)",
						"@(A);Foo"
					}) + "</Project>";

			proj.LoadXml (documentString);
			Assert.IsTrue (proj.Build ("1"), "Build failed");

			CheckItems (proj, "I0", "A0", "AA");
			CheckItems (proj, "I1", "A1", "A", "BA", "B");
			CheckItems (proj, "I2", "A2", "A");
			CheckItems (proj, "I3", "A3", "A", "A");
			CheckItems (proj, "I4", "A4", "A", "B", "C", "A");
			CheckItems (proj, "I5", "A5", "A", "B", "C", "A", "B", "C");
			CheckItems (proj, "I6", "A6", "Foo", "A", "B", "C");
			CheckItems (proj, "I7", "A7", "A", "B", "C", "Foo");
		}

		[Test]
		[Category ("NotDotNet")]
		//Test with ITaskItem[]
		public void TestItemsInTarget3a ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='BatchingTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<PropertyGroup>
						<A>A</A>
						<B>A;B</B>
						<C>A;;</C>
					</PropertyGroup>
					<ItemGroup>
						<A Include='A;B;C' />
						<B Include='Foo;' />
					</ItemGroup>";

			documentString += CreateTargetFragment ("BatchingTestTask", "Sources", "Output", "I",
					new string [] {
						"$(A)$(A)",
						"$(B)$(B)",
						"$(C)",
						"$(C)$(C)",
						"$(C)   $(C)",
						"  $(C)   Foo    $(C)  Bar ; $(B)   ",
						"@(A);$(C)",
						"@(A);A;B;C",
						"  abc;  @(A)  ;  $(C)  ;foo",
					}) + "</Project>";


			proj.LoadXml (documentString);
			Assert.IsTrue (proj.Build ("1"), "Build failed");

			CheckItems (proj, "I0", "A0", "AA");
			CheckItems (proj, "I1", "A1", "A", "BA", "B");
			CheckItems (proj, "I2", "A2", "A");
			CheckItems (proj, "I3", "A3", "A", "A");
			CheckItems (proj, "I4", "A4", "A", "A");
			CheckItems (proj, "I5", "A5", "A", "Foo    A", "Bar", "A", "B");
			CheckItems (proj, "I6", "A6", "A", "B", "C", "A");
			CheckItems (proj, "I7", "A7", "A", "B", "C", "A", "B", "C");
			CheckItems(proj, "I8", "A8", "abc", "A", "B", "C", "A", "foo");
		}

		[Test]
		[Category ("NotDotNet")]
		//Test with string[]
		public void TestItemsInTarget3b ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='BatchingTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<PropertyGroup>
						<A>A</A>
						<B>A;B</B>
						<C>A;;</C>
					</PropertyGroup>
					<ItemGroup>
						<A Include='A;B;;;C' />
					</ItemGroup>";

			documentString += CreateTargetFragment ("BatchingTestTask", "Strings", "StringsOutput", "I",
					new string [] {
						"$(A)$(A)",
						"$(B)$(B)",
						"$(C)",
						"$(C)$(C)",
						"$(C) $(C) $(C)Bar$(C)",
						"@(A);$(C)",
						"@(A);A;B;C"
					}) + "</Project>";

			proj.LoadXml (documentString);
			Assert.IsTrue (proj.Build ("1"), "Build failed");

			CheckItems (proj, "I0", "A0", "AA");
			CheckItems (proj, "I1", "A1", "A", "BA", "B");
			CheckItems (proj, "I2", "A2", "A");
			CheckItems (proj, "I3", "A3", "A", "A");
			CheckItems (proj, "I4", "A4", "A", "A", "A", "BarA");
			CheckItems (proj, "I5", "A5", "A", "B", "C", "A");
			CheckItems (proj, "I6", "A6", "A", "B", "C", "A", "B", "C");
		}

		[Test]
		[Category ("NotDotNet")]
		//Test with string
		public void TestItemsInTarget3c ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger();
			engine.RegisterLogger(logger);

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='BatchingTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<PropertyGroup>
						<A>A</A>
						<B>A;B</B>
						<C>A;;</C>
						<D>$(C);Foo</D>
					</PropertyGroup>
					<ItemGroup>
						<A Include='A;B;;;C' />
					</ItemGroup>";

			documentString += CreateTargetFragment ("BatchingTestTask", "SingleString", "SingleStringOutput", "I",
					new string [] {
						"$(A)$(A)",
						"$(B)$(B)",
						"$(C)",
						"$(C)$(C)",
						"$(C) $(C)",
						"@(A);$(C)",
						"@(A);A;B;C",
						"@(A) $(C) @(A)",
					}) + "</Project>";

			proj.LoadXml (documentString);
			if (!proj.Build("1")) {
				logger.DumpMessages();
				Assert.Fail("Build failed");
			}

			BuildProperty bp = proj.EvaluatedProperties ["D"];
			Assert.AreEqual ("$(C);Foo", bp.Value, "B0");
			Assert.AreEqual ("A;;;Foo", bp.FinalValue, "B1");

			bp = proj.EvaluatedProperties ["C"];
			Assert.AreEqual ("A;;", bp.Value, "B3");
			Assert.AreEqual ("A;;", bp.FinalValue, "B4");

			CheckItems (proj, "I0", "A0", "AA");
			CheckItems (proj, "I1", "A1", "A;BA;B");
			CheckItems (proj, "I2", "A2", "A;;");
			CheckItems (proj, "I3", "A3", "A;;A;;");
			CheckItems (proj, "I4", "A4", "A;; A;;");
			CheckItems (proj, "I5", "A5", "A;B;C;A;;");
			CheckItems (proj, "I6", "A6", "A;B;C;A;B;C");
			CheckItems (proj, "I7", "A7", "A;B;C A;; A;B;C");
		}

		[Test]
		public void TestSingleTaskItemError1 ()
		{
			CheckSingleTaskItemProject ("$(B)$(B)");
		}

		[Test]
		public void TestSingleTaskItemError2 ()
		{
			CheckSingleTaskItemProject ("$(C)$(C)");
		}

		[Test]
		public void TestSingleTaskItemError3 ()
		{
			CheckSingleTaskItemProject ("$(C) $(C)");
		}

		[Test]
		public void TestSingleTaskItemError4 ()
		{
			CheckSingleTaskItemProject ("@(A)");
		}

		[Test]
		public void TestSingleTaskItemError5 ()
		{
			CheckSingleTaskItemProject ("@(A);$(C))");
		}

		[Test]
		public void TestSingleTaskItemError6 ()
		{
			CheckSingleTaskItemProject ("@(A);A;B;C");
		}

		[Test]
		public void TestSingleTaskItemError7 ()
		{
			CheckSingleTaskItemProject ("@(Item1)$(C)");
		}

		[Test]
		public void TestSingleTaskItemError8 ()
		{
			CheckSingleTaskItemProject ("$(B).foo");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestSingleTaskItem1 ()
		{
			Project proj = BuildProjectForSingleTaskItem ("$(D)$(C)");
			CheckItems (proj, "I0", "A0", "A");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestSingleTaskItem2 ()
		{
			Project proj = BuildProjectForSingleTaskItem ("@(Item1)");
			CheckItems (proj, "I0", "A0", "F");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestSingleTaskItem3 ()
		{
			Project proj = BuildProjectForSingleTaskItem ("$(A).foo");
			CheckItems (proj, "I0", "A0", "A.foo");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestSingleTaskItem4 ()
		{
			Project proj = BuildProjectForSingleTaskItem ("$(C)");
			CheckItems (proj, "I0", "A0", "A");
		}

		void CheckSingleTaskItemProject (string expression)
		{
			string documentString = CreateProjectForSingleTaskItem (expression);
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			proj.LoadXml (documentString);
			Assert.IsFalse (proj.Build ("1"), "Build should've failed");
		}

		Project BuildProjectForSingleTaskItem (string expression)
		{
			string documentString = CreateProjectForSingleTaskItem (expression);
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			proj.LoadXml (documentString);
			Assert.IsTrue (proj.Build ("1"), "Build failed");

			return proj;
		}

		string CreateProjectForSingleTaskItem (string expression)
		{
			return @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='BatchingTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<PropertyGroup>
						<A>A</A>
						<B>A;B</B>
						<C>A;;</C>
						<D></D>
					</PropertyGroup>
					<ItemGroup>
						<A Include='A;B;C' />
						<Item1 Include='F' />
					</ItemGroup>

					<Target Name='1'>
						<BatchingTestTask SingleTaskItem='" + expression + @"'>
							<Output TaskParameter='SingleStringOutput' ItemName='I0' />
						</BatchingTestTask>
					</Target>
				</Project>";
		}

		string CreateTargetFragment (string taskname, string task_param_in, string task_param_out, string item_prefix,
				string [] args)
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("<Target Name='1'>");
			for (int i = 0; i < args.Length; i ++) {
				sb.AppendFormat ("<{0} {1}='{2}'>\n", taskname, task_param_in, args [i]);
				sb.AppendFormat ("\t<Output TaskParameter='{0}' ItemName='{1}{2}' />\n", task_param_out, item_prefix, i);
				sb.AppendFormat ("</{0}>\n", taskname);
			}
			sb.Append ("</Target>");

			return sb.ToString ();
		}

		[Test]
		public void TestItemsInTarget4 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='StringTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<ItemGroup>
						<A Include='A;B;C' />
					</ItemGroup>
					<Target Name='1'>
						<StringTestTask Array='@(A)@(A)'>
							<Output TaskParameter='Array' ItemName='I1' />
						</StringTestTask>
					</Target>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.IsFalse (proj.Build ("1"));
		}

		[Test]
		public void TestItemsInTarget5 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='StringTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<ItemGroup>
						<A Include='A;B;C' />
					</ItemGroup>
					<Target Name='1'>
						<StringTestTask Array='@(A)AAA'>
							<Output TaskParameter='Array' ItemName='I1' />
						</StringTestTask>
					</Target>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.IsFalse (proj.Build ("1"));
		}

		[Test]
		public void TestItemsInTarget6 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='StringTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<ItemGroup>
						<A Include='A;B;C' />
					</ItemGroup>
					<PropertyGroup>
						<A>A</A>
					</PropertyGroup>
					<Target Name='1'>
						<StringTestTask Array='@(A)$(A)'>
							<Output TaskParameter='Array' ItemName='I1' />
						</StringTestTask>
					</Target>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.IsFalse (proj.Build ("1"));
		}

		[Test]
		public void TestItemsInTarget7 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='BatchingTestTask' AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"' />
					<ItemGroup>
						<A Include='A;B;C' />
						<B Include='Foo;' />
					</ItemGroup>
					<Target Name='1'>
						<BatchingTestTask SingleTaskItem='Bar%(B.Identity)@(A)' />
					</Target>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.IsFalse (proj.Build ("1"));
		}

		[Test]
		public void TestItemsInTarget8 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<Foo>Five</Foo>
					</PropertyGroup>
					<ItemGroup>
						<A Include='A'>
							<M>True</M>
							<M>False</M>
						</A>
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual (1, proj.EvaluatedItems.Count, "A1");
			BuildItem bi = proj.EvaluatedItems [0];
			Assert.AreEqual ("False", bi.GetMetadata ("M"), "A2");
		}


		[Test]
		public void TestItemsInTarget9 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<Foo>Five</Foo>
					</PropertyGroup>
					<ItemGroup>
						<A Include='A'>
							<M Condition="" '$(Foo)' == 'Five' "">True</M>
							<M Condition="" '$(Foo)' != 'Five' "">False</M>
						</A>
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual (1, proj.EvaluatedItems.Count, "A1");
			BuildItem bi = proj.EvaluatedItems [0];
			Assert.AreEqual ("True", bi.GetMetadata ("M"), "A2");
			Assert.AreEqual (0, bi.Condition.Length, "A3");

			BuildItemGroup big = proj.GetEvaluatedItemsByNameIgnoringCondition ("A");
			Assert.AreEqual (1, big.Count, "A4");
			bi = big [0];
			Assert.AreEqual ("True", bi.GetMetadata ("M"), "A5");
			Assert.AreEqual ("True", bi.GetEvaluatedMetadata ("M"), "A6");

			/*proj.SetProperty ("Foo", "Six");
			proj.Build ();
			bi = proj.GetEvaluatedItemsByName ("A") [0];
			Assert.AreEqual ("False", bi.GetMetadata ("M"), "A7");
			Assert.AreEqual ("False", bi.GetEvaluatedMetadata ("M"), "A7a");
			Assert.AreEqual (0, bi.Condition.Length, "A8");

			big = proj.GetEvaluatedItemsByNameIgnoringCondition ("A");
			Assert.AreEqual (1, big.Count, "A9");
			bi = big [0];
			Assert.AreEqual ("True", bi.GetMetadata ("M"), "A10");
			Assert.AreEqual ("True", bi.GetEvaluatedMetadata ("M"), "A11");*/
		}

		[Test]
		// Fails on wrench
		[Category ("NotWorking")]
		public void TestItemsWithWildcards ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			// Setup

			string basedir = PathCombine ("Test", "resources", "dir");
			string aaa = PathCombine ("a", "aa", "aaa");
			string bb = Path.Combine ("b", "bb");

			string[] dirs = { aaa, bb, "c" };
			string [] files = {
								PathCombine (basedir, aaa, "foo.dll"),
								PathCombine (basedir, bb, "bar.dll"),
								PathCombine (basedir, bb, "sample.txt"),
								Path.Combine (basedir, "xyz.dll")
							  };

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<ItemsRel Include='dir\**\*.dll' Exclude='*\x*.dll' />
						<ItemsRelExpanded Include=""@(ItemsRel->'%(FullPath)')"" />
						<ItemsAbs Include='$(MSBuildProjectDirectory)\dir\**\*.dll'/>
					</ItemGroup>

					<Target Name='Main'>
						<Message Text=""ItemsRel: %(ItemsRel.FullPath) RecDir: %(ItemsRel.RecursiveDir)""/>
						<Message Text=""ItemsRelExpanded: %(ItemsRelExpanded.Identity)""/>
						<Message Text='ItemsAbs: %(ItemsAbs.Identity) RecDir: %(ItemsAbs.RecursiveDir)'/>
					</Target>
				</Project>";

			try {
				CreateDirectoriesAndFiles (basedir, dirs, files);
				string projectdir = Path.Combine ("Test", "resources");
				File.WriteAllText (Path.Combine (projectdir, "wild1.proj"), documentString);
				proj.Load (Path.Combine (projectdir, "wild1.proj"));
				if (!proj.Build ("Main")) {
					logger.DumpMessages ();
					Assert.Fail ("Build failed");
				}
				string full_base_dir = Path.GetFullPath (basedir);

				logger.CheckLoggedAny (@"ItemsRel: "+ PathCombine (full_base_dir, aaa, "foo.dll") +
							" RecDir: " + aaa + Path.DirectorySeparatorChar, MessageImportance.Normal, "A1");

				logger.CheckLoggedAny (@"ItemsRel: " + PathCombine (full_base_dir, bb, "bar.dll") +
							" RecDir: " + bb + Path.DirectorySeparatorChar, MessageImportance.Normal, "A2");

				logger.CheckLoggedAny (@"ItemsRelExpanded: " + PathCombine (full_base_dir, aaa, "foo.dll"), MessageImportance.Normal, "A3");
				logger.CheckLoggedAny (@"ItemsRelExpanded: " + PathCombine (full_base_dir, bb, "bar.dll"), MessageImportance.Normal, "A4");

				logger.CheckLoggedAny (@"ItemsAbs: " + PathCombine (full_base_dir, aaa, "foo.dll") +
							@" RecDir: " + aaa + Path.DirectorySeparatorChar, MessageImportance.Normal, "A5");
				logger.CheckLoggedAny (@"ItemsAbs: " + PathCombine (full_base_dir, bb, "bar.dll") +
							@" RecDir: " + bb + Path.DirectorySeparatorChar, MessageImportance.Normal, "A6");
				logger.CheckLoggedAny (@"ItemsAbs: " + PathCombine (full_base_dir, "xyz.dll") +
							@" RecDir: ", MessageImportance.Normal, "A7");

				Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");
			} catch (AssertionException) {
				logger.DumpMessages ();
				throw;
			} finally {
				Directory.Delete (basedir, true);
			}
		}

		[Test]
		// Fails on wrench
		[Category ("NotWorking")]
		public void TestReservedMetadata ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup><File1 Include=""bar\foo.dll""/></ItemGroup>
					<Target Name='Main'>
						<Message Text='file1: @(File1)'/>
						<Message Text='file1: RootDir: %(File1.RootDir)'/>
						<Message Text='file1: Directory: %(File1.Directory)'/>
					</Target>
				</Project>";

			string projectdir = Path.Combine ("Test", "resources");
			File.WriteAllText (Path.Combine (projectdir, "test1.proj"), documentString);
			proj.Load (Path.Combine (projectdir, "test1.proj"));
			if (!proj.Build ("Main")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("file1: " + Path.Combine ("bar", "foo.dll"), "A1");

			string path_root = Path.GetPathRoot (Path.GetFullPath (projectdir));
			logger.CheckLoggedMessageHead ("file1: RootDir: " + path_root, "A2");

			string fullpath = Path.GetFullPath (Path.Combine (projectdir, "bar"));
			logger.CheckLoggedMessageHead ("file1: Directory: " + fullpath.Substring (path_root.Length) + Path.DirectorySeparatorChar, "A3");

			if (logger.NormalMessageCount != 0) {
				logger.DumpMessages ();
				Assert.Fail ("Unexpected extra messages found");
			}
		}

		void CreateDirectoriesAndFiles (string basedir, string[] dirs, string[] files)
		{
			foreach (string dir in dirs)
				Directory.CreateDirectory (Path.Combine (basedir, dir));

			foreach (string file in files)
				File.WriteAllText (file, String.Empty);
		}

		string PathCombine (string path1, params string[] parts)
		{
			if (parts == null || parts.Length == 0)
				return path1;

			string final_path = path1;
			foreach (string part in parts)
				final_path = Path.Combine (final_path, part);

			return final_path;
		}
	}
}
