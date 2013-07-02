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
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)

#if !MOBILE

using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MonoTests.System.ComponentModel {

	[TestFixture]
	public class ComplexBindingPropertiesAttributeTest {

		[Test]
		public void CtorTest ()
		{
			ComplexBindingPropertiesAttribute a;

			a = new ComplexBindingPropertiesAttribute ("source", "member");
			Assert.AreEqual ("source", a.DataSource, "1");
			Assert.AreEqual ("member", a.DataMember, "2");

			a = new ComplexBindingPropertiesAttribute ("source");
			Assert.AreEqual ("source", a.DataSource, "3");
			Assert.AreEqual (null, a.DataMember, "4");

			a = new ComplexBindingPropertiesAttribute ();
			Assert.AreEqual (null, a.DataSource, "5");
			Assert.AreEqual (null, a.DataMember, "6");
		}

		[Test]
		public void EqualsTest ()
		{
			ComplexBindingPropertiesAttribute a;

			a = new ComplexBindingPropertiesAttribute ("source", "member");
			Assert.IsFalse (a.Equals (null), "1");
			Assert.IsFalse (a.Equals (new ComplexBindingPropertiesAttribute ("member", "source")), "2");
			Assert.IsTrue (a.Equals (new ComplexBindingPropertiesAttribute ("source", "member")), "3");
		}

		[Test]
		public void GetHashCodeTest ()
		{
			ComplexBindingPropertiesAttribute a;

			a = new ComplexBindingPropertiesAttribute ("source", "member");
			Assert.IsFalse (0 == a.GetHashCode (), "1");

			a = new ComplexBindingPropertiesAttribute ("source");
			Assert.IsFalse (0 == a.GetHashCode (), "2");
		}

		[Test]
		public void DefaultTest ()
		{
			Assert.AreEqual (ComplexBindingPropertiesAttribute.Default, new ComplexBindingPropertiesAttribute (), "1");
		}
	}

}

#endif
