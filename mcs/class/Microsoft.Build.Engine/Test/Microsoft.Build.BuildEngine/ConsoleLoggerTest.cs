//
// ConsoleLoggerTest.cs
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
using Microsoft.Build.Framework;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class ConsoleLoggerTest {
		[Test]
		[Category ("NotDotNet")]
		public void TestAssignment ()
		{
			ConsoleLogger cl = new ConsoleLogger ();

			Assert.IsNull (cl.Parameters, "A1");
			Assert.IsTrue (cl.ShowSummary, "A2");
			Assert.IsFalse (cl.SkipProjectStartedText, "A3");
			Assert.AreEqual (LoggerVerbosity.Normal, cl.Verbosity, "A4");

			cl.ApplyParameter ("name", "value");
			
			Assert.IsNull (cl.Parameters);
		}
	}
}
