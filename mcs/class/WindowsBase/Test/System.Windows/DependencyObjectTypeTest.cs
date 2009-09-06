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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Windows;
using System.Windows.Media;
using NUnit.Framework;

namespace MonoTests.System.Windows {

	[TestFixture]
	public class DependencyObjectTypeTest {

		[Test]
		public void Accessors ()
		{
			DependencyObjectType t = DependencyObjectType.FromSystemType (typeof (TestDepObj));
			Assert.AreEqual ("TestDepObj", t.Name);
			Assert.AreEqual (typeof (TestDepObj), t.SystemType);
			Assert.AreEqual (typeof (DependencyObject), t.BaseType.SystemType);
			// we can't test the Id field's value, as it's
			// not guaranteed to be anything in
			// particular.
		}

		[Test]
		public void IsInstanceOfType ()
		{
			DependencyObjectType t = DependencyObjectType.FromSystemType (typeof (TestDepObj));
			DependencyObjectType t2 = DependencyObjectType.FromSystemType (typeof (TestSubclass));
			Assert.IsTrue (t.IsInstanceOfType (new TestSubclass()));
			Assert.IsTrue (t2.IsSubclassOf (t));
			Assert.IsFalse (t.IsSubclassOf (t2));
		}

		[Test]
		public void TestCache ()
		{
			DependencyObjectType t = DependencyObjectType.FromSystemType (typeof (TestDepObj));
			DependencyObjectType t2 = DependencyObjectType.FromSystemType (typeof (TestDepObj));
			Assert.AreSame (t, t2);
		}
	}
}
