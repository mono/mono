//
// Properties.cs
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
	public class Properties {
		[Test]
		public void TestProperties1 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<Config>debug</Config>
						<ExpProp>$(Config)-$(Config)</ExpProp>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.AreEqual (1, proj.PropertyGroups.Count, "A1");
			Assert.AreEqual ("debug", proj.GetEvaluatedProperty ("Config"), "A2");
			Assert.AreEqual ("debug-debug", proj.GetEvaluatedProperty ("ExpProp"), "A3");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestProperties2 ()
		{
			Engine engine = new Engine (Consts.BinPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask TaskName='StringTestTask' AssemblyFile='Test\resources\TestTasks.dll' />
					<PropertyGroup>
						<A>A</A>
						<B>B</B>
					</PropertyGroup>

					<Target Name='Main' >
						<StringTestTask Array='$(A)$(B)'>
							<Output TaskParameter='Array' ItemName='Out' />
						</StringTestTask>
					</Target>
				</Project>
			";

			proj.LoadXml (documentString);
			proj.Build ("Main");
			Assert.AreEqual (1, proj.GetEvaluatedItemsByName ("Out").Count, "A1");
			Assert.AreEqual ("AB", proj.GetEvaluatedItemsByName ("Out") [0].Include, "A2");
		}
	}
}
