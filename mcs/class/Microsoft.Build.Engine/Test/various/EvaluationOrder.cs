//
// EvaluationOrder.cs
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
	public class EvaluationOrder {
		string GetItems (Project proj, string name)
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
		public void TestOrder0 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item Include='A' />
					</ItemGroup>

					<PropertyGroup>
						<A>A</A>
						<Property>@(Item)$(A)$(B)</Property>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("@(Item)A", proj.EvaluatedProperties ["Property"].FinalValue, "A1");
			Assert.AreEqual ("@(Item)$(A)$(B)", proj.EvaluatedProperties ["Property"].Value, "A2");
			Assert.AreEqual ("A", GetItems (proj, "Item"), "A3");
		}

		[Test]
		public void TestOrder1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item Include='A' />
					</ItemGroup>

					<PropertyGroup>
						<Property>@(Item)</Property>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("@(Item)", proj.EvaluatedProperties ["Property"].FinalValue, "A1");
			Assert.AreEqual ("@(Item)", proj.EvaluatedProperties ["Property"].Value, "A2");
			Assert.AreEqual ("A", GetItems (proj, "Item"), "A3");
		}

		[Test]
		public void TestOrder2 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<Property>@(Item)</Property>
					</PropertyGroup>

					<ItemGroup>
						<Item Include='A' />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("@(Item)", proj.EvaluatedProperties ["Property"].FinalValue, "A1");
			Assert.AreEqual ("@(Item)", proj.EvaluatedProperties ["Property"].Value, "A2");
			Assert.AreEqual ("A", GetItems (proj, "Item"), "A3");
		}

		[Test]
		public void TestOrder3 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<Property>A</Property>
					</PropertyGroup>

					<ItemGroup>
						<Item Include='$(Property)' />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("A", proj.EvaluatedProperties ["Property"].FinalValue, "A1");
			Assert.AreEqual ("A", proj.EvaluatedProperties ["Property"].Value, "A2");
			Assert.AreEqual ("A", GetItems (proj, "Item"), "A3");
		}

		[Test]
		public void TestOrder4 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item Include='$(Property)' />
					</ItemGroup>

					<PropertyGroup>
						<Property>A</Property>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("A", proj.EvaluatedProperties ["Property"].FinalValue, "A1");
			Assert.AreEqual ("A", proj.EvaluatedProperties ["Property"].Value, "A2");
			Assert.AreEqual ("A", GetItems (proj, "Item"), "A3");
		}

		[Test]
		public void TestOrder5 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item Include='$(Property)' />
					</ItemGroup>

					<PropertyGroup>
						<Property>A</Property>
						<Property2>@(Item)</Property2>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("A", proj.EvaluatedProperties ["Property"].FinalValue, "A1");
			Assert.AreEqual ("A", proj.EvaluatedProperties ["Property"].Value, "A2");
			Assert.AreEqual ("A", GetItems (proj, "Item"), "A3");
			Assert.AreEqual ("@(Item)", proj.EvaluatedProperties ["Property2"].FinalValue, "A4");
			Assert.AreEqual ("@(Item)", proj.EvaluatedProperties ["Property2"].Value, "A5");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestOrder6 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item Include='A' />
						<Item2 Include='$(Property)' />
					</ItemGroup>

					<PropertyGroup>
						<Property>@(Item)</Property>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("@(Item)", proj.EvaluatedProperties ["Property"].FinalValue, "A1");
			Assert.AreEqual ("@(Item)", proj.EvaluatedProperties ["Property"].Value, "A2");
			Assert.AreEqual ("A", GetItems (proj, "Item"), "A3");
			Assert.AreEqual ("A", GetItems (proj, "Item2"), "A4");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestImportOrder1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<Property>Test/resources/Import.csproj</Property>
					</PropertyGroup>

					<Import Project='$(Property)'/>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("Value", proj.EvaluatedProperties ["ImportedProperty"].FinalValue, "A1");
			Assert.AreEqual ("Value", proj.EvaluatedProperties ["ImportedProperty"].Value, "A2");
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestImportOrder2 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='$(Property)'/>

					<PropertyGroup>
						<Property>Test/resources/Import.csproj</Property>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("Value", proj.EvaluatedProperties ["ImportedProperty"].FinalValue, "A1");
			Assert.AreEqual ("Value", proj.EvaluatedProperties ["ImportedProperty"].Value, "A2");
		}

		[Test]
		// NOTE: It will try to import "@(Item)" instead of Test/...
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestImportOrder3 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item Include='Test/resources/Import.csproj' />
					</ItemGroup>

					<Import Project='@(Item)'/>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("Value", proj.EvaluatedProperties ["ImportedProperty"].FinalValue, "A1");
			Assert.AreEqual ("Value", proj.EvaluatedProperties ["ImportedProperty"].Value, "A2");
		}

		[Test]
		// NOTE: It will try to import "@(Item)" instead of Test/...
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestImportOrder4 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='@(Item)'/>

					<ItemGroup>
						<Item Include='Test/resources/Import.csproj' />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("Value", proj.EvaluatedProperties ["ImportedProperty"].FinalValue, "A1");
			Assert.AreEqual ("Value", proj.EvaluatedProperties ["ImportedProperty"].Value, "A2");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestImportOrder5 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<ImportedProperty>AnotherValue</ImportedProperty>
					</PropertyGroup>

					<Import Project='Test/resources/Import.csproj'/>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("Value", proj.EvaluatedProperties ["ImportedProperty"].FinalValue, "A1");
			Assert.AreEqual ("Value", proj.EvaluatedProperties ["ImportedProperty"].Value, "A2");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestImportOrder6 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='Test/resources/Import.csproj'/>

					<PropertyGroup>
						<ImportedProperty>AnotherValue</ImportedProperty>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("AnotherValue", proj.EvaluatedProperties ["ImportedProperty"].FinalValue, "A1");
			Assert.AreEqual ("AnotherValue", proj.EvaluatedProperties ["ImportedProperty"].Value, "A2");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestImportOrder7 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='Test/resources/Import.csproj'/>

					<PropertyGroup>
						<ImportedProperty>Another$(ImportedProperty)</ImportedProperty>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);

			Assert.AreEqual ("AnotherValue", proj.EvaluatedProperties ["ImportedProperty"].FinalValue, "A1");
			Assert.AreEqual ("Another$(ImportedProperty)", proj.EvaluatedProperties ["ImportedProperty"].Value, "A2");
		}

		[Test]
		public void TestUsingTaskOrder1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<Property>Test\resources\TestTasks.dll</Property>
					</PropertyGroup>

					<UsingTask AssemblyFile='$(Property)' TaskName='TrueTestTask' />
				</Project>
			";

			proj.LoadXml (documentString);

			UsingTask [] ut = new UsingTask [1];
			proj.UsingTasks.CopyTo (ut, 0);

			Assert.AreEqual ("$(Property)", ut [0].AssemblyFile, "A1");
		}
	}
}
