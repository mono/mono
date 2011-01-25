//
// Conditions.cs: Tests various conditions by checking if a property
// is added to EvaluatedProperties.
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
	public class Conditions {

		[Test]
		public void TestCondition1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition='true'></A>
						<B Condition='false'></B>
						<C Condition='TRUE'></C>
						<D Condition='FALSE'></D>
						<E Condition=''>A</E>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.IsNotNull (proj.EvaluatedProperties ["A"], "A1");
			Assert.IsNull (proj.EvaluatedProperties ["B"], "A2");
			Assert.IsNotNull (proj.EvaluatedProperties ["C"], "A3");
			Assert.IsNull (proj.EvaluatedProperties ["D"], "A4");
			Assert.IsNotNull (proj.EvaluatedProperties ["E"], "A5");
		}

		[Test]
		public void TestCondition2 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition='true or true'></A>
						<B Condition='false or false'></B>
						<C Condition='true or false'></C>
						<D Condition='false or true'></D>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.IsNotNull (proj.EvaluatedProperties ["A"], "A1");
			Assert.IsNull (proj.EvaluatedProperties ["B"], "A2");
			Assert.IsNotNull (proj.EvaluatedProperties ["C"], "A3");
			Assert.IsNotNull (proj.EvaluatedProperties ["D"], "A4");
		}

		[Test]
		public void TestCondition3 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition='true and true'></A>
						<B Condition='false and false'></B>
						<C Condition='true and false'></C>
						<D Condition='false and true'></D>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.IsNotNull (proj.EvaluatedProperties ["A"], "A1");
			Assert.IsNull (proj.EvaluatedProperties ["B"], "A2");
			Assert.IsNull (proj.EvaluatedProperties ["C"], "A3");
			Assert.IsNull (proj.EvaluatedProperties ["D"], "A4");
		}

		[Test]
		public void TestCondition4 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition='!true'></A>
						<B Condition='!false'></B>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.IsNull (proj.EvaluatedProperties ["A"], "A1");
			Assert.IsNotNull (proj.EvaluatedProperties ["B"], "A2");
		}

		[Test]
		public void TestCondition5 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<a Condition='-1 &lt; 0'></a>
						<b Condition='-1 &lt;= 0'></b>
						<c Condition='-1.0 &lt; 0.0'></c>
						<d Condition='-1.0 &lt;= 0.0'></d>
						<e Condition='-1.0 == 0.0'></e>
						<f Condition='0.0 == 0.0'></f>
						<g Condition='-1.0 != 0.0'></g>
						<h Condition='0.0 != 0.0'></h>
						<i Condition='1 &gt; 0'></i>
						<j Condition='1 &gt;= 0'></j>
						<k Condition='-1 &gt; 0'></k>
						<l Condition='-1 &gt;= 0'></l>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.IsNotNull (proj.EvaluatedProperties ["a"], "A1");
			Assert.IsNotNull (proj.EvaluatedProperties ["b"], "A2");
			Assert.IsNotNull (proj.EvaluatedProperties ["c"], "A3");
			Assert.IsNotNull (proj.EvaluatedProperties ["d"], "A4");
			Assert.IsNull (proj.EvaluatedProperties ["e"], "A5");
			Assert.IsNotNull (proj.EvaluatedProperties ["f"], "A6");
			Assert.IsNotNull (proj.EvaluatedProperties ["g"], "A7");
			Assert.IsNull (proj.EvaluatedProperties ["h"], "A8");
			Assert.IsNotNull (proj.EvaluatedProperties ["i"], "A1");
			Assert.IsNotNull (proj.EvaluatedProperties ["j"], "A2");
			Assert.IsNull (proj.EvaluatedProperties ["k"], "A3");
			Assert.IsNull (proj.EvaluatedProperties ["l"], "A4");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestCondition6 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A>true</A>
						<B>false</B>
						<C Condition='$(A)'></C>
						<D Condition='$(B)'></D>
						<E Condition="" '$(A)' ""></E>
						<F Condition="" '$(B)' ""></F>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.IsNotNull (proj.EvaluatedProperties ["C"], "A1");
			Assert.IsNull (proj.EvaluatedProperties ["D"], "A2");
			Assert.IsNotNull (proj.EvaluatedProperties ["E"], "A3");
			Assert.IsNull (proj.EvaluatedProperties ["F"], "A4");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestCondition7 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A>true</A>
					</PropertyGroup>
					<ItemGroup>
						<B Include='true' />
						<C Include='true;false' />
					</ItemGroup>

					<Target Name='1'>
						<Message Text='a' Condition='$(A)' />
						<Message Text='b' Condition='@(B)' />
						<Message Text='c' Condition='%(C.Filename)' />
					</Target>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.IsTrue (proj.Build ("1"), "A1");
		}

		[Test]
		public void TestCondition8 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition='true == true'></A>
						<B Condition='true != true'></B>
						<C Condition='false == false'></C>
						<D Condition='false != false'></D>
						<E Condition='true != false'></E>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.IsNotNull (proj.EvaluatedProperties ["A"], "A1");
			Assert.IsNull (proj.EvaluatedProperties ["B"], "A2");
			Assert.IsNotNull (proj.EvaluatedProperties ["C"], "A3");
			Assert.IsNull (proj.EvaluatedProperties ["D"], "A4");
			Assert.IsNotNull (proj.EvaluatedProperties ["E"], "A5");
		}

		[Test]
		public void TestCondition9 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition=""'A' == 'A'""></A>
						<B Condition="" 'A' == 'A' ""></B>
						<C Condition=""'A' == 'a'""></C>
						<D Condition=""'A' == 'b'""></D>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.IsNotNull (proj.EvaluatedProperties ["A"], "A1");
			Assert.IsNotNull (proj.EvaluatedProperties ["B"], "A2");
			Assert.IsNotNull (proj.EvaluatedProperties ["C"], "A3");
			Assert.IsNull (proj.EvaluatedProperties ["D"], "A4");
		}

		[Test]
		public void TestCondition10 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition="" !'true' ""></A>
						<B Condition="" 'on' == 'true' ""></B>
						<C Condition="" 4 == 4.0 and 04 == 4""></C>
						<D Condition="" !(false and false) ==  !false or !false ""></D>
						<E Condition="" Exists ('Test\resources\Import.csproj') ""></E>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.IsNull (proj.EvaluatedProperties ["A"], "A1");
			Assert.IsNotNull (proj.EvaluatedProperties ["B"], "A2");
			Assert.IsNotNull (proj.EvaluatedProperties ["C"], "A3");
			Assert.IsNotNull (proj.EvaluatedProperties ["D"], "A4");
			Assert.IsNotNull (proj.EvaluatedProperties ["E"], "A5");
		}
		[Test]
		public void TestCondition11 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();
			string documentString = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<PropertyGroup>
		<FooProp>true</FooProp>
	</PropertyGroup>
	<ItemGroup>
		<FooList Include=""abc.exe""/>
		<List1 Include=""fr_a.txt"" Condition="" $(FooProp) == 'true'"" />
		<List1 Include=""fr_b.txt"" Condition="" '@(FooList->'%(Extension)a(foo', ',')' == '.exea(foo'"" />
		<List1 Include=""fr_c.txt"" Condition="" @(FooList -> '%(Extension)', ',') == '.exe'"" />
	</ItemGroup>
</Project>";

			proj.LoadXml (documentString);

			BuildItemGroup bgp = proj.GetEvaluatedItemsByName ("List1");
			Assert.IsNotNull (bgp, "Expected values in List1");
			Assert.AreEqual (3, bgp.Count, "A1");
			Assert.AreEqual ("fr_a.txt", bgp [0].FinalItemSpec, "A2");
			Assert.AreEqual ("fr_b.txt", bgp [1].FinalItemSpec, "A3");
			Assert.AreEqual ("fr_c.txt", bgp [2].FinalItemSpec, "A4");
		}

		// Test shortcircuiting
		[Test]
		public void TestCondition12 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition=""'$(NonExistant)' != '' and $(NonExistant)""></A>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.IsNull (proj.EvaluatedProperties ["A"], "A1");
		}


		[Test]
		public void TestHasTrailingSlash1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<EmptyProp></EmptyProp>
						<WithTrailingBackSlash>foo\ </WithTrailingBackSlash>
						<WithTrailingFwdSlash>foo/  </WithTrailingFwdSlash>
						<NoTrailing>Foo</NoTrailing>

						<A Condition="" HasTrailingSlash('$(EmptyProp)') ""></A>
						<B Condition="" HasTrailingSlash('$(WithTrailingBackSlash)') ""></B>
						<C Condition="" HasTrailingSlash('$(WithTrailingFwdSlash)') ""></C>
						<D Condition="" HasTrailingSlash('$(NoTrailing)') ""></D>
						<E Condition="" HasTrailingSlash('$(NonExistant)') ""></E>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.IsNull (proj.EvaluatedProperties ["A"], "A1");
			Assert.IsNotNull (proj.EvaluatedProperties ["B"], "A2");
			Assert.IsNotNull (proj.EvaluatedProperties ["C"], "A3");
			Assert.IsNull (proj.EvaluatedProperties ["D"], "A4");
			Assert.IsNull (proj.EvaluatedProperties ["E"], "A5");
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestUnknownFunction ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition="" NonExistantFunction('$(EmptyProp)') ""></A>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestIncorrectCondition1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition='x'>A</A>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);
		}

		// A reference to an item list at position 1 is not allowed in this condition "@(A)".
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		[Category ("NotWorking")]
		public void TestIncorrectCondition2 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition='@(A)'>A</A>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);
		}

		// Found an unexpected character '%' at position 0 in condition \%(A)\.
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		[Category ("NotWorking")]
		public void TestIncorrectCondition3 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A Condition='%(A)'>A</A>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);
		}

		// Found an unexpected character '%' at position 0 in condition "%(A)\.
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		[Category ("NotWorking")]
		public void TestIncorrectCondition4 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<A Include='a' Condition='%(A)' />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestIncorrectCondition5 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<A Include='a' Condition="" '  == ''  "" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);
		}
	}
}
