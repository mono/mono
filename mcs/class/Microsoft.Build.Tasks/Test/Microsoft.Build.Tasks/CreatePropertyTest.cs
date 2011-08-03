//
// CreatePropertyTest.cs
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
	public class CreatePropertyTest {
		[Test]
		public void TestAssignment ()
		{
			CreateProperty cp = new CreateProperty ();

			cp.Value = new string [1] { "1" };

			Assert.AreEqual ("1", cp.Value [0], "A1");
			Assert.AreEqual ("1", cp.ValueSetByTask [0], "A2");
		}

		[Test]
		public void TestExecution1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A>1</A>
						<B>2</B>
					</PropertyGroup>
					<Target Name='1'>
						<CreateProperty Value='$(A)$(B)' >
							<Output
								TaskParameter='Value'
								PropertyName='Value'
							/>
							<Output
								TaskParameter='ValueSetByTask'
								PropertyName='ValueSetByTask'
							/>
						</CreateProperty>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			Assert.IsTrue (project.Build ("1"), "A1");

			Assert.AreEqual ("12", project.EvaluatedProperties ["Value"].Value, "A2");
			Assert.AreEqual ("12", project.EvaluatedProperties ["Value"].FinalValue, "A3");
			Assert.AreEqual ("12", project.EvaluatedProperties ["ValueSetByTask"].Value, "A4");
			Assert.AreEqual ("12", project.EvaluatedProperties ["ValueSetByTask"].FinalValue, "A5");
		}

		[Test]
		public void TestExecution2 () {
			Engine engine;
			Project project;

			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<Second Include=""Abc""/>
					<IG Include=""@(Second)""/></ItemGroup>
					<PropertyGroup>
						<C>@(IG)</C>
					</PropertyGroup>
					<Target Name='1'>
						<CreateProperty Value='$(C)' >
							<Output
								TaskParameter='Value'
								PropertyName='Value'
							/>
							<Output
								TaskParameter='ValueSetByTask'
								PropertyName='ValueSetByTask'
							/>
						</CreateProperty>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			Assert.IsTrue (project.Build ("1"), "A1");

			Assert.AreEqual ("Abc", project.EvaluatedProperties["Value"].Value, "A2");
			Assert.AreEqual ("Abc", project.EvaluatedProperties["Value"].FinalValue, "A3");
			Assert.AreEqual ("Abc", project.EvaluatedProperties["ValueSetByTask"].Value, "A4");
			Assert.AreEqual ("Abc", project.EvaluatedProperties["ValueSetByTask"].FinalValue, "A5");
			Assert.AreEqual ("@(IG)", project.EvaluatedProperties["C"].FinalValue, "A6");
		}

		[Test]
		public void TestEmptyPropertyValue ()
		{
			Engine engine;
			Project project;

			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A>1</A>
					</PropertyGroup>
					<Target Name='1'>
						<Message Text='Before: $(A)'/>
						<CreateProperty Value=''>
							<Output
								TaskParameter='Value'
								PropertyName='A'
							/>
						</CreateProperty>
						<Message Text='After: $(A)'/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			testLogger.CheckLoggedMessageHead ("Before: 1", "A1");
			testLogger.CheckLoggedMessageHead ("After: ", "A2");
			Assert.AreEqual (0, testLogger.NormalMessageCount, "Unexpected messages found");
		}
	}
}
