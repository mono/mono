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
		[Category ("NotWorking")]
		public void TestExecution1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<A Include='1;2;3;4'/>
						<B Include='1;3' />
					</ItemGroup>
					<Target Name='1'>
						<CreateItem
							AdditionalMetadata='a=1;b=2'
							Include='@(A)'
							Exclude='@(B)'
						>
							<Output
								TaskParameter='Include'
								ItemName='Include'
							/>
						</CreateItem>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			Assert.IsTrue (project.Build ("1"), "A1");

			BuildItemGroup include = project.GetEvaluatedItemsByName ("Include");
			Assert.AreEqual (2, include.Count, "A2");

			Assert.AreEqual ("Include", include [0].Name, "A3");
			Assert.AreEqual ("1", include [0].GetMetadata ("a"), "A4");
			Assert.AreEqual ("2", include [0].GetMetadata ("b"), "A5");
			Assert.AreEqual ("1", include [0].GetEvaluatedMetadata ("a"), "A6");
			Assert.AreEqual ("2", include [0].GetEvaluatedMetadata ("b"), "A7");
			Assert.AreEqual ("2", include [0].FinalItemSpec, "A8");

			Assert.AreEqual ("Include", include [0].Name, "A9");
			Assert.AreEqual ("1", include [1].GetMetadata ("a"), "A10");
			Assert.AreEqual ("2", include [1].GetMetadata ("b"), "A11");
			Assert.AreEqual ("1", include [1].GetEvaluatedMetadata ("a"), "A12");
			Assert.AreEqual ("2", include [1].GetEvaluatedMetadata ("b"), "A13");
			Assert.AreEqual ("4", include [1].FinalItemSpec, "A14");
		}
	}
}
