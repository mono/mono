//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Xaml;
using NUnit.Framework;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class AttachableMemberIdentifierTest
	{
		[Test]
		public void NullTypeName ()
		{
			// It is not rejected. No dot.
			Assert.AreEqual ("Foo", new AttachableMemberIdentifier (null, "Foo").ToString (), "#1");
		}
		
		[Test]
		public void EmptyMemberName ()
		{
			// It is not rejected. Trailing dot.
			Assert.AreEqual ("System.String.", new AttachableMemberIdentifier (typeof (string), "").ToString (), "#1");
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("System.String.Foo", new AttachableMemberIdentifier (typeof (string), "Foo").ToString (), "#1");
			Assert.AreEqual ("System.Int32.Foo", new AttachableMemberIdentifier (typeof (int), "Foo").ToString (), "#2");
		}

		[Test]
		public void Equivalence ()
		{
			var a1 = new AttachableMemberIdentifier (typeof (string), "Foo");
			var a2 = new AttachableMemberIdentifier (typeof (string), "Foo");
			var a3 = new AttachableMemberIdentifier (null, "Foo");
			var a4 = new AttachableMemberIdentifier (null, "Foo");
			var a5 = new AttachableMemberIdentifier (typeof (string), null);
			var a6 = new AttachableMemberIdentifier (typeof (string), null);
			Assert.IsTrue (a1 == a2, "#1");
			Assert.IsFalse (a1 == a3, "#2");
			Assert.IsFalse (a1 == a5, "#3");
			Assert.IsTrue (a3 == a4, "#4");
			Assert.IsFalse (a3 == a1, "#5");
			Assert.IsFalse (a3 == a5, "#6");
			Assert.IsTrue (a5 == a6, "#7");
			Assert.IsFalse (a5 == a1, "#8");
			Assert.IsFalse (a5 == a3, "#9");
			Assert.IsTrue (a1.Equals (a2),"#11");
			Assert.IsFalse (a1.Equals (a3),"#12");
			Assert.IsFalse (a1.Equals (a5),"#13");
			Assert.IsTrue (a3.Equals (a4),"#14");
			Assert.IsFalse (a3.Equals (a1),"#15");
			Assert.IsFalse (a3.Equals (a5),"#16");
			Assert.IsTrue (a5.Equals (a6),"#17");
			Assert.IsFalse (a5.Equals (a1),"#18");
			Assert.IsFalse (a5.Equals (a3),"#19");
		}
	}
}
