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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Alan McGovern (amcgovern@novell.com)
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace System.IO.Packaging.Tests {
    public abstract class TestBase {

        protected string contentType = "mime/type";
        protected Package package;
        protected Uri relationshipUri = new Uri ("/_rels/.rels", UriKind.Relative);
        protected FakeStream stream;
        protected Uri [] uris = { new Uri("/file1.png", UriKind.Relative),
                       new Uri("/file2.png", UriKind.Relative),
                       new Uri("/file3.png", UriKind.Relative) };

        [TestFixtureSetUp]
        public virtual void FixtureSetup ()
        {

        }

        [SetUp]
        public virtual void Setup ()
        {
            stream = new FakeStream ();
            package = Package.Open (stream, FileMode.Create);
        }

        [TearDown]
        public virtual void TearDown ()
        {
			try {
	            if (package != null)
	                package.Close ();
			} catch {
				
			}
			
            if (stream != null)
                stream.Close ();
        }

        [TestFixtureTearDown]
        public virtual void FixtureTeardown ()
        {

        }
    }
}
