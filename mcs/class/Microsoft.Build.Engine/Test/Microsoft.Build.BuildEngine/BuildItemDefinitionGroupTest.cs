//
// BuildItemDefinitionGroupTest.cs
//
// Author:
//   Haakon Sporsheim (haakon.sporsheim@gmail.com)
//
// (C) 2010 Haakon Sporsheim
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
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class BuildItemDefinitionGroupTest {
		[Test]
		public void TestItemMetadataFallback1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemDefinitionGroup>
						<A>
							<Meta>Foo</Meta>
						</A>
					</ItemDefinitionGroup>
					<ItemGroup>
						<A Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (1, project.EvaluatedItems.Count, "A1");
			Assert.AreEqual ("Foo", project.EvaluatedItems [0].GetEvaluatedMetadata ("Meta"), "A2");
		}

		[Test]
		public void TestItemMetadataFallback2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemGroup>
						<A Include='a' />
					</ItemGroup>
					<ItemDefinitionGroup>
						<A>
							<Meta>Foo</Meta>
						</A>
					</ItemDefinitionGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (1, project.EvaluatedItems.Count, "A1");
			Assert.AreEqual ("Foo", project.EvaluatedItems [0].GetEvaluatedMetadata ("Meta"), "A2");
		}

		[Test]
		public void TestItemDefinitionMetadataCondition ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemDefinitionGroup>
						<A>
							<Meta>Data</Meta>
							<Foo>Bar</Foo>
						</A>
					</ItemDefinitionGroup>
					<ItemDefinitionGroup>
						<A>
							<Meta Condition="" '%(A.Meta)' == '' "">NotData</Meta>
							<Foo Condition="" '%(A.Foo)' == 'Bar' "">NotBar</Foo>
						</A>
					</ItemDefinitionGroup>
					<ItemGroup>
						<A Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (1, project.EvaluatedItems.Count, "A1");
			Assert.AreEqual ("Data", project.EvaluatedItems [0].GetEvaluatedMetadata ("Meta"), "A2");
			Assert.AreEqual ("NotBar", project.EvaluatedItems [0].GetEvaluatedMetadata ("Foo"), "A3");
		}

		[Test]
		public void TestItemDefinitionMetadataConditionNoItemName ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemDefinitionGroup>
						<A>
							<Meta>Data</Meta>
							<Foo>Bar</Foo>
						</A>
					</ItemDefinitionGroup>
					<ItemDefinitionGroup>
						<A>
							<Meta Condition="" '%(Meta)' == '' "">NotData</Meta>
							<Foo Condition="" '%(Foo)' == 'Bar' "">NotBar</Foo>
						</A>
					</ItemDefinitionGroup>
					<ItemGroup>
						<A Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (1, project.EvaluatedItems.Count, "A1");
			Assert.AreEqual ("Data", project.EvaluatedItems [0].GetEvaluatedMetadata ("Meta"), "A2");
			Assert.AreEqual ("NotBar", project.EvaluatedItems [0].GetEvaluatedMetadata ("Foo"), "A3");
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestItemDefinitionMetadataConditionInvalidItemName ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ItemDefinitionGroup>
						<A>
							<Meta Condition="" '%(B.Meta)' == '' "">Data</Meta>
						</A>
					</ItemDefinitionGroup>
					<ItemGroup>
						<A Include='a' />
					</ItemGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}
	}
}
