// ***********************************************************************
// Copyright (c) 2012 Charlie Poole
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
// ***********************************************************************

#if !NETCF
using System;
using System.IO;
using System.Reflection;

namespace NUnit.Framework.Internal
{
    [TestFixture]
    public class AssemblyHelperTests
    {
        [Test]
        public void GetPathForAssembly()
        {
            string path = AssemblyHelper.GetAssemblyPath(this.GetType().Assembly);
            Assert.That(Path.GetFileName(path), Is.EqualTo("nunitlite.tests.exe").IgnoreCase);
            Assert.That(File.Exists(path));
        }

        //[Test]
        //public void GetPathForType()
        //{
        //    string path = AssemblyHelper.GetAssemblyPath(this.GetType());
        //    Assert.That(Path.GetFileName(path), Is.EqualTo("nunitlite.tests.exe").IgnoreCase);
        //    Assert.That(File.Exists(path));
        //}
		
        // The following tests are only useful to the extent that the test cases
        // match what will actually be provided to the method in production.
        // As currently used, NUnit's codebase can only use the file: schema,
        // since we don't load assemblies from anything but files. The uri's
        // provided can be absolute file paths or UNC paths.

        // Local paths - Windows Drive
        [TestCase(@"file:///C:/path/to/assembly.dll", @"C:\path\to\assembly.dll")]
        [TestCase(@"file:///C:/my path/to my/assembly.dll", @"C:/my path/to my/assembly.dll")]
        [TestCase(@"file:///C:/dev/C#/assembly.dll", @"C:\dev\C#\assembly.dll")]
        [TestCase(@"file:///C:/dev/funnychars?:=/assembly.dll", @"C:\dev\funnychars?:=\assembly.dll")]
        // Local paths - Linux or Windows absolute without a drive
        [TestCase(@"file:///path/to/assembly.dll", @"/path/to/assembly.dll")]
        [TestCase(@"file:///my path/to my/assembly.dll", @"/my path/to my/assembly.dll")]
        [TestCase(@"file:///dev/C#/assembly.dll", @"/dev/C#/assembly.dll")]
        [TestCase(@"file:///dev/funnychars?:=/assembly.dll", @"/dev/funnychars?:=/assembly.dll")]
        // Windows drive specified as if it were a server - odd case, sometimes seen
        [TestCase(@"file://C:/path/to/assembly.dll", @"C:\path\to\assembly.dll")]
        [TestCase(@"file://C:/my path/to my/assembly.dll", @"C:\my path\to my\assembly.dll")]
        [TestCase(@"file://C:/dev/C#/assembly.dll", @"C:\dev\C#\assembly.dll")]
        [TestCase(@"file://C:/dev/funnychars?:=/assembly.dll", @"C:\dev\funnychars?:=\assembly.dll")]
        // UNC format with server and path
        [TestCase(@"file://server/path/to/assembly.dll", @"//server/path/to/assembly.dll")]
        [TestCase(@"file://server/my path/to my/assembly.dll", @"//server/my path/to my/assembly.dll")]
        [TestCase(@"file://server/dev/C#/assembly.dll", @"//server/dev/C#/assembly.dll")]
        [TestCase(@"file://server/dev/funnychars?:=/assembly.dll", @"//server/dev/funnychars?:=/assembly.dll")]
        //[TestCase(@"http://server/path/to/assembly.dll", "//server/path/to/assembly.dll")]
        public void GetAssemblyPathFromCodeBase(string uri, string expectedPath)
        {
            string localPath = AssemblyHelper.GetAssemblyPathFromCodeBase(uri);
            Assert.That(localPath, Is.SamePath(expectedPath));
        }
    }
}
#endif
