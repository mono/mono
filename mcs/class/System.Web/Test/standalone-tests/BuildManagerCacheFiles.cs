//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2010 Novell, Inc http://novell.com/
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
#if NET_4_0
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.IO;
using System.Web;
using System.Web.Hosting;

using StandAloneRunnerSupport;
using StandAloneTests;

using NUnit.Framework;

namespace StandAloneTests.BuildManagerCacheFiles
{
	[TestCase ("BuildManagerCacheFiles 01", "Tests for BuildManager.{Create,Read}CacheFile")]
	public sealed class BuildManagerCacheFiles_01 : ITestCase
	{
		static string[] expectedMessages = {
			"create[1]: codeGen",
			"create[1]: fileStream",
			"create[1]: can read",
			"create[1]: can write",
			"create[1]: pathSubdirOfCodeGen",
			"create[1]: our file name",
			"read[1]: codeGen",
			"read[1]: fileStream",
			"read[1]: can read",
			"read[1]: cannot write",
			"read[1]: pathSubdirOfCodeGen",
			"read[1]: our file name",
			"read[1]: contents ok",
			
			"create[2]: codeGen",

			// .NET exception:
			//
			// System.ArgumentException: Value does not fall within the expected range.
			// at System.Web.Compilation.BuildManager.GetUserCacheFilePath(String fileName)
			// at System.Web.Compilation.BuildManager.CreateCachedFile(String fileName)
			// at _Default.RunTest(String fileName, String logTag, List`1 messages, Boolean noCreate) in c:\Users\grendel\Documents\Visual Studio 2010\Websites\BuildManager4.0\Default.aspx.cs:line 56)
			"create[2]: error write (System.ArgumentException)",
			"read[2]: codeGen",

			// .NET exception:
			//
			// System.ArgumentException: Value does not fall within the expected range.
			// at System.Web.Compilation.BuildManager.GetUserCacheFilePath(String fileName)
			// at System.Web.Compilation.BuildManager.ReadCachedFile(String fileName)
			// at _Default.RunTest(String fileName, String logTag, List`1 messages, Boolean noCreate) in c:\Users\grendel\Documents\Visual Studio 2010\Websites\BuildManager4.0\Default.aspx.cs:line 86)
			"read[2]: error read (System.ArgumentException)",
			
			"read[3]: codeGen",
			"read[3]: stream is null",

			"create[4]: codeGen",

			// .NET exception
			// System.ArgumentNullException: Value cannot be null.
			// Parameter name: path2
			// at System.IO.Path.Combine(String path1, String path2)
			// at System.Web.Compilation.BuildManager.GetUserCacheFilePath(String fileName)
			// at System.Web.Compilation.BuildManager.CreateCachedFile(String fileName)
			// at _Default.RunTest(String fileName, String logTag, List`1 messages, Boolean noCreate) in c:\Users\grendel\Documents\Visual Studio 2010\Websites\BuildManager4.0\Default.aspx.cs:line 61)
			"create[4]: error write (System.ArgumentNullException)",
			"read[4]: codeGen",

			// .NET exception
			// System.ArgumentNullException: Value cannot be null.
			// Parameter name: path2
			// at System.IO.Path.Combine(String path1, String path2)
			// at System.Web.Compilation.BuildManager.GetUserCacheFilePath(String fileName)
			// at System.Web.Compilation.BuildManager.ReadCachedFile(String fileName)
			// at _Default.RunTest(String fileName, String logTag, List`1 messages, Boolean noCreate) in c:\Users\grendel\Documents\Visual Studio 2010\Websites\BuildManager4.0\Default.aspx.cs:line 91)
			"read[4]: error read (System.ArgumentNullException)",
			
			"create[5]: codeGen",

			// .NET exception
			// System.ArgumentException: Value does not fall within the expected range.
			// at System.Web.Compilation.BuildManager.GetUserCacheFilePath(String fileName)
			// at System.Web.Compilation.BuildManager.CreateCachedFile(String fileName)
			// at _Default.RunTest(String fileName, String logTag, List`1 messages, Boolean noCreate) in c:\Users\grendel\Documents\Visual Studio 2010\Websites\BuildManager4.0\Default.aspx.cs:line 61)
			"create[5]: error write (System.ArgumentException)",
			
			"read[5]: codeGen",

			// .NET exception
			// System.ArgumentException: Value does not fall within the expected range.
			// at System.Web.Compilation.BuildManager.GetUserCacheFilePath(String fileName)
			// at System.Web.Compilation.BuildManager.ReadCachedFile(String fileName)
			// at _Default.RunTest(String fileName, String logTag, List`1 messages, Boolean noCreate) in c:\Users\grendel\Documents\Visual Studio 2010\Websites\BuildManager4.0\Default.aspx.cs:line 91)
			"read[5]: error read (System.ArgumentException)",
		};
		
		public string PhysicalPath {
			get {
				return Path.Combine (
					Consts.BasePhysicalDir,
					"BuildManagerCacheFiles"
				);
			}
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("/Default.aspx", Default_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
			var messages = runItem.TestRunData as List <string>;

			Assert.IsNotNull (messages, "#A1");

			int len = messages.Count;
			int i = 0;
			for (; i < len; i++)
				Assert.AreEqual (expectedMessages [i], messages [i], "#A2-" + i.ToString ());

			if (i != len)
				Assert.Fail ("Expected {0} messages, found {1}", i, len);
		}
	}
}
#endif