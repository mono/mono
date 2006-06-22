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
		// FIXME: split it to several tests and add more tests
		public void TestItems1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Item0 Include=""A"" />
						<Item1 Include=""A;B;C"" />
						<Item2 Include=""@(Item1);A;D"" />
						<Item3 Include=""@(Item2)"" Exclude=""A"" />
						<Item4 Include=""@(Item1);Q"" Exclude=""@(Item2)"" />
						<Item5 Include=""@(Item1)"" Exclude=""@(Item2)"" />
						<Item6 Include=""@(Item2)"" Exclude=""@(Item1)"" />

						<ItemOrig Include=""A\B.txt;A\C.txt;B\B.zip;B\C.zip"" />
						<ItemT1 Include=""@(Item1->'%(Identity)')"" />
						<ItemT2 Include=""@(Item1->'%(Identity)%(Identity)')"" />
						<ItemT3 Include=""@(Item1->'(-%(Identity)-)')"" />
						<ItemT4 Include=""@(ItemOrig->'%(Extension)')"" />
						<ItemT5 Include=""@(ItemOrig->'%(Filename)/%(Extension)')"" />
						<ItemT6 Include=""@(ItemOrig->'%(RelativeDir)X/%(Filename)')"" />
						
						<ItemS1 Include=""@(Item1,'-')"" />
						<ItemS2 Include=""@(Item1,'xx')"" />
						<ItemS3 Include=""@(Item1, '-')"" />
					</ItemGroup>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.AreEqual ("A", GetItems (proj, "Item0"), "A1");
			Assert.AreEqual ("A;B;C", GetItems (proj, "Item1"), "A2");
			Assert.AreEqual ("A;B;C;A;D", GetItems (proj, "Item2"), "A3");
			Assert.AreEqual ("B;C;D", GetItems (proj, "Item3"), "A4");
			Assert.AreEqual ("Q", GetItems (proj, "Item4"), "A5");
			Assert.AreEqual ("", GetItems (proj, "Item5"), "A6");
			Assert.AreEqual ("D", GetItems (proj, "Item6"), "A7");

			Assert.AreEqual ("A;B;C", GetItems (proj, "ItemT1"), "A8");
			Assert.AreEqual ("AA;BB;CC", GetItems (proj, "ItemT2"), "A9");
			Assert.AreEqual ("(-A-);(-B-);(-C-)", GetItems (proj, "ItemT3"), "A10");
			Assert.AreEqual (".txt;.txt;.zip;.zip", GetItems (proj, "ItemT4"), "A11");
			Assert.AreEqual ("B/.txt;C/.txt;B/.zip;C/.zip", GetItems (proj, "ItemT5"), "A12");
			Assert.AreEqual (@"A\X/B;A\X/C;B\X/B;B\X/C", GetItems (proj, "ItemT6"), "A13");

			Assert.AreEqual ("A-B-C", GetItems (proj, "ItemS1"), "A14");
			Assert.AreEqual ("AxxBxxC", GetItems (proj, "ItemS2"), "A15");
			// Will fail.
			Assert.AreEqual ("A-B-C", GetItems (proj, "ItemS3"), "A16");
		}
	}
}
