// InstallContextTest.cs
//   NUnit Test Cases for System.Configuration.Install.InstallContext class
//
// Author:
//    Muthu Kannan (t.manki@gmail.com)
//
// (C) 2005 Novell, Inc.  http://www.novell.com/
// 

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
//

using NUnit.Framework;
using System;
using System.Configuration.Install;
using System.Collections.Specialized;

namespace MonoTests.System.Configuration.Install
{

[TestFixture]
public class TestInstallContext {
	private InstallContext ic;

	// Create an instance to be used in tests
	[SetUp]
	public void SetUp ()
	{
		string logFile = "./InstallContextTestLog.txt";
		string[] param = new string[] {
		    "/Option1=value1", 
		    "-Option2=value2",
		    "-Option3",
		    "Option4",
		    "/Option5=True",
		    "/Option6=no"
		};
		ic = new InstallContext (logFile, param);
	}

	// Testing constructor with typical arguments
	[Test]
	public void TestCtor ()
	{
		StringDictionary param;

		Assert.IsFalse (ic.Parameters == null, "#ICTXaa01");
		param = ic.Parameters;
		Assert.IsTrue (param.ContainsKey ("Option1"), "#ICTXaa03");
		Assert.IsTrue (param.ContainsKey ("Option2"), "#ICTXaa04");
		Assert.IsTrue (param.ContainsKey ("Option3"), "#ICTXaa05");
		Assert.IsTrue (param.ContainsKey ("Option4"), "#ICTXaa06");
		Assert.IsTrue (param.ContainsKey ("Option5"), "#ICTXaa07");
		Assert.IsTrue (param.ContainsKey ("Option6"), "#ICTXaa08");
		Assert.AreEqual ("value1", param["Option1"], "#ICTXaa09");
		Assert.AreEqual ("value2", param["Option2"], "#ICTXaa10");
		Assert.AreEqual ("", param["Option3"], "#ICTXaa11");
		Assert.AreEqual ("", param["Option4"], "#ICTXaa12");
		Assert.AreEqual ("True", param["Option5"], "#ICTXaa13");
		Assert.AreEqual ("no", param["Option6"], "#ICTXaa14");
		Assert.IsTrue (ic.IsParameterTrue ("Option5"), "#ICTXaa15");
		Assert.IsFalse (ic.IsParameterTrue ("Option6"), "#ICTXaa16");
	}
}

}
