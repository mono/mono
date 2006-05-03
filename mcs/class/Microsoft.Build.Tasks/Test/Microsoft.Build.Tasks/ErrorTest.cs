//
// ErrorTest.cs
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
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Tasks {
	[TestFixture]
	public class ErrorTest {

		string binPath = "../../tools/xbuild/xbuild";

		Engine engine;
		Project project;

		[Test]
		public void TestAssignment ()
		{
			string code = "code";
			string helpKeyword = "helpKeyword";
			string text = "text";
			
			Error error = new Error ();
			
			error.Code = code;
			error.HelpKeyword = helpKeyword;
			error.Text = text;

			Assert.AreEqual (code, error.Code, "#1");
			Assert.AreEqual (helpKeyword, error.HelpKeyword, "#2");
			Assert.AreEqual (text, error.Text, "#3");
		}

		[Test]
		public void TestExecute ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='1'>
						<Error Text='Text' HelpKeyword='HelpKeyword' Code='Code' />
					</Target>
				</Project>
			";
			
			engine = new Engine (binPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			bool result = project.Build ("1");

			Assert.AreEqual (false, result, "#1");
		}
	}
}
        
