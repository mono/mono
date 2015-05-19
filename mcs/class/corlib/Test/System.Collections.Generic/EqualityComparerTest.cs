//
// Unit tests for System.Collections.Generic.EqualityComparer
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

namespace MonoTests.System.Collections.Generic {

	[TestFixture]
	public class EqualityComparerTest {
		enum E
		{
			A,
			B
		}

		[Test]
		public void Default_GetHashCode_Null ()
		{
			// https://bugzilla.novell.com/show_bug.cgi?id=372892
			Assert.AreEqual (0, EqualityComparer<object>.Default.GetHashCode (null), "object");
			Assert.AreEqual (0, EqualityComparer<string>.Default.GetHashCode (null), "string");
		}

		[Test]
		public void NonGenericGetHashCodeForNullArgument ()
		{
			IEqualityComparer comparer = EqualityComparer<object>.Default;
			Assert.AreEqual (0, comparer.GetHashCode (null));
		}

		[Test] // #703027
		public void NonGenericEqualsForNullArguments ()
		{
			IEqualityComparer comparer = EqualityComparer<object>.Default;
			Assert.IsTrue (comparer.Equals (null, null));
		}

		[Test]
		public void EnumComparison ()
		{
			Assert.IsFalse (EqualityComparer<E>.Default.Equals (E.A, E.B));
			Assert.IsFalse (EqualityComparer<object>.Default.Equals (E.A, E.B));
		}
	}
}

