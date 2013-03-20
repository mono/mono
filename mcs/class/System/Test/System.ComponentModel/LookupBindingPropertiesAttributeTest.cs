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
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)

#if !MOBILE

using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MonoTests.System.ComponentModel {

	[TestFixture]
	public class LookupBindingPropertiesAttributeTest {

		[Test]
		public void CtorTest ()
		{
			LookupBindingPropertiesAttribute a, b, c, d;

			a = new LookupBindingPropertiesAttribute ();
			Assert.IsNull (a.DataSource, "#A1");
			Assert.IsNull (a.DisplayMember, "#A2");
			Assert.IsNull (a.ValueMember, "#A3");
			Assert.IsNull (a.LookupMember, "#A4");

			b = new LookupBindingPropertiesAttribute ("aa", "bb", "cc", "dd");
			Assert.AreSame ("aa", b.DataSource, "#B1");
			Assert.AreSame ("bb", b.DisplayMember, "#B2");
			Assert.AreSame ("cc", b.ValueMember, "#B3");
			Assert.AreSame ("dd", b.LookupMember, "#B4");

			c = new LookupBindingPropertiesAttribute ("aa", "bb", "cc", "dd");

			Assert.AreEqual ("aa", c.DataSource, "#C1");
			Assert.AreEqual ("bb", c.DisplayMember, "#C2");
			Assert.AreEqual ("cc", c.ValueMember, "#C3");
			Assert.AreEqual ("dd", c.LookupMember, "#C4");

			Assert.IsTrue (b.Equals (c), "#Eq1");
			Assert.AreEqual (b.GetHashCode (), c.GetHashCode (), "#Hash");

			d = LookupBindingPropertiesAttribute.Default;
			Assert.IsNull (d.DataSource, "#D1");
			Assert.IsNull (d.DisplayMember, "#D2");
			Assert.IsNull (d.ValueMember, "#D3");
			Assert.IsNull (d.LookupMember, "#D4");

			Assert.IsTrue (d.Equals (a), "#Eq2");
			Assert.IsFalse (a.Equals (b), "#Eq3");
		}
	}

}

#endif
