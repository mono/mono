//
// Items.cs
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
			Assert.AreEqual ("A", GetItems (proj, "Item0"), "A1");
			Assert.AreEqual ("A;B;C", GetItems (proj, "Item1"), "A2");
			Assert.AreEqual ("A;B;C;A;D", GetItems (proj, "Item2"), "A3");
			Assert.AreEqual ("B;C;D", GetItems (proj, "Item3"), "A4");
			Assert.AreEqual ("Q", GetItems (proj, "Item4"), "A5");
			Assert.AreEqual (String.Empty, GetItems (proj, "Item5"), "A6");
			Assert.AreEqual ("D", GetItems (proj, "Item6"), "A7");
			Assert.AreEqual (String.Empty, GetItems (proj, "Item7"), "A8");
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

			Assert.AreEqual ("A-B-C", GetItems (proj, "Item2"), "A1");
			Assert.AreEqual ("AxxBxxC", GetItems (proj, "Item3"), "A2");
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

			Assert.AreEqual ("A-B-C", GetItems (proj, "Item2"), "A1");
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

			Assert.AreEqual ("A;B;C", GetItems (proj, "ItemT1"), "A1");
			Assert.AreEqual ("AA;BB;CC", GetItems (proj, "ItemT2"), "A2");
			Assert.AreEqual ("(-A-);(-B-);(-C-)", GetItems (proj, "ItemT3"), "A3");
			Assert.AreEqual (".txt;.txt;.zip;.zip", GetItems (proj, "ItemT4"), "A4");
			Assert.AreEqual ("B/.txt;C/.txt;B/.zip;C/.zip", GetItems (proj, "ItemT5"), "A5");
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

			Assert.AreEqual (@"A\X/B;A\X/C;B\X/B;B\X/C", GetItems (proj, "ItemT"), "A1");
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

			Assert.AreEqual ("%(A.B)", GetItems (proj, "Item2"), "A1");
			Assert.AreEqual (String.Empty, GetItems (proj, "Item3"), "A2");
			Assert.AreEqual ("AABAC", GetItems (proj, "Item4"), "A3");
			Assert.AreEqual ("A%(A)B%(A)C", GetItems (proj, "Item5"), "A4");
			Assert.AreEqual ("A@(A)B@(A)C", GetItems (proj, "Item6"), "A6");
			Assert.AreEqual ("A;B;C", GetItems (proj, "Item7"), "A6");
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException),
			"The expression \"@(Item1, '@(A,'')')\" cannot be used in this context. " +
			"Item lists cannot be concatenated with other strings where an item list is expected. " +
			"Use a semicolon to separate multiple item lists.  ")]
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

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException),
			"The expression \"@(Item1, '@(A->'')')\" cannot be used in this context. " +
			"Item lists cannot be concatenated with other strings where an item list is expected. " +
			"Use a semicolon to separate multiple item lists.  ")]
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

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException),
			"The expression \"@(Item1, '@(A->'','')')\" cannot be used in this context. " +
			"Item lists cannot be concatenated with other strings where an item list is expected. " +
			"Use a semicolon to separate multiple item lists.  ")]
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
		[Category ("NotWorking")]
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
			proj.Build ("1");

			Assert.AreEqual ("A;B;C", proj.GetEvaluatedProperty ("P1"), "A1");
			Assert.AreEqual ("ABC", proj.GetEvaluatedProperty ("P2"), "A2");
			Assert.AreEqual ("A@(A)B@(A)C", proj.GetEvaluatedProperty ("P3"), "A3");
			Assert.AreEqual ("AABAC", proj.GetEvaluatedProperty ("P4"), "A4");
			Assert.AreEqual ("@(A,'ABC')", proj.GetEvaluatedProperty ("P5"), "A5");
			Assert.AreEqual ("A;B;C", GetItems (proj, "I1"), "A6");
			Assert.AreEqual ("A A;B B;C C", GetItems (proj, "I2"), "A7");
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
		[Category ("NotWorking")]
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
						<C>A;</C>
					</PropertyGroup>
					<ItemGroup>
						<A Include='A;B;C' />
					</ItemGroup>

					<Target Name='1'>
						<StringTestTask Array='$(A)$(A)'>
							<Output TaskParameter='Array' ItemName='I0' />
						</StringTestTask>
						<StringTestTask Array='$(B)$(B)'>
							<Output TaskParameter='Array' ItemName='I1' />
						</StringTestTask>
						<StringTestTask Array='$(C)'>
							<Output TaskParameter='Array' ItemName='I2' />
						</StringTestTask>
						<StringTestTask Array='$(C)$(C)'>
							<Output TaskParameter='Array' ItemName='I3' />
						</StringTestTask>

						<StringTestTask Array='@(A);$(C)'>
							<Output TaskParameter='Array' ItemName='I4' />
						</StringTestTask>
						<StringTestTask Array='@(A);A;B;C'>
							<Output TaskParameter='Array' ItemName='I5' />
						</StringTestTask>
					</Target>
				</Project>
			";

			proj.LoadXml (documentString);
			proj.Build ("1");

			Assert.AreEqual ("AA", GetItems (proj, "I0"), "A1");
			Assert.AreEqual ("A;BA;B", GetItems (proj, "I1"), "A2");
			Assert.AreEqual (3, proj.GetEvaluatedItemsByName ("I1").Count, "A3");
			Assert.AreEqual ("A", GetItems (proj, "I2"), "A4");
			Assert.AreEqual (1, proj.GetEvaluatedItemsByName ("I2").Count, "A5");
			Assert.AreEqual ("A;A", GetItems (proj, "I3"), "A6");
			Assert.AreEqual (2, proj.GetEvaluatedItemsByName ("I3").Count, "A7");

			Assert.AreEqual ("A;B;C;A", GetItems (proj, "I4"), "A8");
			Assert.AreEqual (4, proj.GetEvaluatedItemsByName ("I4").Count, "A9");
			Assert.AreEqual ("A;B;C;A;B;C", GetItems (proj, "I5"), "A10");
			Assert.AreEqual (6, proj.GetEvaluatedItemsByName ("I5").Count, "A11");
		}

		[Test]
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
	}
}
