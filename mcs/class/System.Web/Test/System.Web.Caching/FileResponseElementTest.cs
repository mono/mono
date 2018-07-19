//
// FileResponseElementTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Web;
using System.Web.Caching;

using NUnit.Framework;

namespace MonoTests.System.Web.Caching
{
	[TestFixture]
	public class FileResponseElementTest
	{
		[Test]
		public void Constructor ()
		{
			FileResponseElement fre;

			Assert.Throws<ArgumentNullException> (() => {
				fre = new FileResponseElement (null, 0, 0);
			}, "#A1");

			Assert.Throws<ArgumentOutOfRangeException> (() => {
				fre = new FileResponseElement ("file.txt", -1, 0);
			}, "#A2");

			Assert.Throws<ArgumentOutOfRangeException> (() => {
				fre = new FileResponseElement ("file.txt", 0, -1);
			}, "#A3");

			fre = new FileResponseElement (String.Empty, 0, 0);
			Assert.AreEqual (String.Empty, fre.Path, "#B1-1");
			Assert.AreEqual (0, fre.Length, "#B1-2");
			Assert.AreEqual (0, fre.Offset, "#B1-3");

			fre = new FileResponseElement ("file.txt", 10, 30);
			Assert.AreEqual ("file.txt", fre.Path, "#C1-1");
			Assert.AreEqual (30, fre.Length, "#C1-2");
			Assert.AreEqual (10, fre.Offset, "#C1-3");
		}
	}
}
