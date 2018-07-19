//
// MemoryResponseElementTest.cs
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
	public class MemoryResponseElementTest
	{
		[Test]
		public void Constructor ()
		{
			MemoryResponseElement mre;

			Assert.Throws<ArgumentNullException> (() => {
				mre = new MemoryResponseElement (null, 0);
			}, "#A1");

			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mre = new MemoryResponseElement (new byte[1], -1);
			}, "#A2");

			Assert.Throws<ArgumentOutOfRangeException> (() => {
				mre = new MemoryResponseElement (new byte[1], 2);
			}, "#A2");

			var b = new byte[0];
			mre = new MemoryResponseElement (b, 0);
			Assert.AreEqual (b, mre.Buffer, "#B1-1");
			Assert.AreEqual (0, mre.Length, "#B1-2");

			b = new byte[10];
			mre = new MemoryResponseElement (b, 10);
			Assert.AreEqual (b, mre.Buffer, "#C1-1");
			Assert.AreEqual (10, mre.Length, "#C1-2");

			mre = new MemoryResponseElement (b, 5);
			Assert.AreEqual (b, mre.Buffer, "#D1-1");
			Assert.AreEqual (5, mre.Length, "#D1-2");
		}
	}
}
