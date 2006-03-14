// 
// ResourceManager.cs:
//     NUnit Test Cases for System.Resources.ResourceManager
//
// Authors:
//     Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2005 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Globalization;
using System.Resources;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResourceManagerTest
	{
		[Test]
		public void TestInvariantCulture ()
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("MyResources", "Test/resources", null);
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld"), "#01");
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld"), "#02");
		}

		[Test]
		public void TestGermanCulture ()
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo ("de-DE");
			ResourceManager rm = ResourceManager.
				CreateFileBasedResourceManager ("MyResources", "Test/resources", null);
			Assert.AreEqual ("Hello World", rm.GetString ("HelloWorld"), "#01");
			Assert.AreEqual ("Hello World", rm.GetObject ("HelloWorld"), "#02");
			Assert.AreEqual ("Hallo Welt", rm.GetString ("deHelloWorld"), "#03");
			Assert.AreEqual ("Hallo Welt", rm.GetObject ("deHelloWorld"), "#04");
		}
	}
}
