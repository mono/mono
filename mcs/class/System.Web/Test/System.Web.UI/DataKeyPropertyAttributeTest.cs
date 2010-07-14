//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// (C) 2010 Novell, Inc (http://novell.com)
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
using System.Web.UI;

using NUnit.Framework;

namespace MonoTests.System.Web.UI
{
	[TestFixture]
	public class DataKeyPropertyAttributeTest
	{
		[Test]
		public void Constructor ()
		{
			var a = new DataKeyPropertyAttribute (null);
			Assert.AreEqual (null, a.Name, "#A1");

			a = new DataKeyPropertyAttribute ("test");
			Assert.AreEqual ("test", a.Name, "#A2");
		}

		[Test]
		public void EqualsTest ()
		{
			var a = new DataKeyPropertyAttribute (null);

			Assert.IsFalse (a.Equals (null), "#A1-1");
			Assert.IsFalse (a.Equals ("test"), "#A1-2");

			a = new DataKeyPropertyAttribute ("test");
			Assert.IsFalse (a.Equals ("test"), "#A2-1");
			Assert.IsTrue (a.Equals ((object)new DataKeyPropertyAttribute ("test")), "#A2-2");
			Assert.IsFalse (a.Equals (new DataKeyPropertyAttribute ("invalid")), "#A2-3");
			Assert.IsFalse (a.Equals ((object) new DataKeyPropertyAttribute ("TEST")), "#A2-3");
		}
	}
}
#endif