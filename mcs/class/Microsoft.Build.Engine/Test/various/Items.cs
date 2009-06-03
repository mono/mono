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
					<ItemGroup>
						<Item1 Include='A;B;C' />
						<Item2 Include=""A\B.txt;A\C.txt;B\B.zip;B\C.zip"" />
						<ItemT1 Include=""@(Item1->'%(Identity)')"" />
						<ItemT2 Include=""@(Item1->'%(Identity)%(Identity)')"" />
						<ItemT3 Include=""@(Item1->'(-%(Identity)-)')"" />
						<ItemT4 Include=""@(Item2->'%(Extension)')"" />
						<ItemT5 Include=""@(Item2->'%(Filename)/%(Extension)')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			CheckItems (proj, "ItemT1", "A1", "A", "B", "C");
			CheckItems (proj, "ItemT2", "A2", "AA", "BB", "CC");
			CheckItems (proj, "ItemT3", "A3", "(-A-)", "(-B-)", "(-C-)");
			CheckItems (proj, "ItemT4", "A4", ".txt", ".txt", ".zip", ".zip");
			CheckItems (proj, "ItemT5", "A5", "B/.txt", "C/.txt", "B/.zip", "C/.zip");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestItems5 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item Include=""A\B.txt;A\C.txt;B\B.zip;B\C.zip"" />
						<ItemT Include=""@(Item->'%(RelativeDir)X/%(Filename)')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			CheckItems (proj, "ItemT", "A1", @"A\X/B", @"A\X/C", @"B\X/B", @"B\X/C");
		}

		[Test]
		[Category ("NotWorking")]
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
		public void TestItemsInTarget1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='StringTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
					<PropertyGroup>
						<A>A</A>
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
						<StringTestTask Property=""%(A.Filename)"">
							<Output TaskParameter='Property' ItemName='I1' />
						</StringTestTask>
						<StringTestTask Property=""@(A) %(Filename)"">
							<Output TaskParameter='Property' ItemName='I2' />
						</StringTestTask>
					</Target>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.IsTrue (proj.Build ("1"), "A0, Build failed");

			Assert.AreEqual ("A;B;C", proj.GetEvaluatedProperty ("P1"), "A1");
			Assert.AreEqual ("ABC", proj.GetEvaluatedProperty ("P2"), "A2");
			Assert.AreEqual ("A@(A)B@(A)C", proj.GetEvaluatedProperty ("P3"), "A3");
			Assert.AreEqual ("AABAC", proj.GetEvaluatedProperty ("P4"), "A4");
			Assert.AreEqual ("@(A,'ABC')", proj.GetEvaluatedProperty ("P5"), "A5");
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
					<UsingTask TaskName='StringTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
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
		public void TestItemsInTarget3 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='StringTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
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
		//Test with ITaskItem[]
		public void TestItemsInTarget3a ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='BatchingTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
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
		}

		[Test]
		//Test with string[]
		public void TestItemsInTarget3b ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='BatchingTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
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
		//Test with string
		public void TestItemsInTarget3c ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='BatchingTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
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
						"@(A) $(C) @(A)"
					}) + "</Project>";

			proj.LoadXml (documentString);
			Assert.IsTrue (proj.Build ("1"), "Build failed");

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
		public void TestSingleTaskItem1 ()
		{
			Project proj = BuildProjectForSingleTaskItem ("$(D)$(C)");
			CheckItems (proj, "I0", "A0", "A");
		}

		[Test]
		public void TestSingleTaskItem2 ()
		{
			Project proj = BuildProjectForSingleTaskItem ("@(Item1)");
			CheckItems (proj, "I0", "A0", "F");
		}

		[Test]
		public void TestSingleTaskItem3 ()
		{
			Project proj = BuildProjectForSingleTaskItem ("$(A).foo");
			CheckItems (proj, "I0", "A0", "A.foo");
		}

		[Test]
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
					<UsingTask TaskName='BatchingTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
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
					<UsingTask TaskName='StringTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
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
					<UsingTask TaskName='StringTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
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
					<UsingTask TaskName='StringTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
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
					<UsingTask TaskName='BatchingTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
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
	}
}
