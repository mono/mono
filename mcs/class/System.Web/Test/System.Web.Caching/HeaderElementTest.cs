//
// HeaderElementTest.cs
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
	public class HeaderElementTest
	{
		[Test]
		public void Constructor ()
		{
			HeaderElement he;

			Assert.Throws<ArgumentNullException> (() => {
				he = new HeaderElement (null, String.Empty);
			}, "#A1");

			Assert.Throws<ArgumentNullException> (() => {
				he = new HeaderElement ("Header", null);
			}, "#A2");

			he = new HeaderElement ("Header", String.Empty);
			Assert.AreEqual ("Header", he.Name, "#B1-1");
			Assert.AreEqual (String.Empty, he.Value, "#B1-2");

			he = new HeaderElement ("Header", "Value");
			Assert.AreEqual ("Header", he.Name, "#C1-1");
			Assert.AreEqual ("Value", he.Value, "#C1-2");

			he = new HeaderElement (String.Empty, String.Empty);
			Assert.AreEqual (String.Empty, he.Name, "#D1-1");
			Assert.AreEqual (String.Empty, he.Value, "#D1-2");
		}
	}
}
