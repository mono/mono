//
// StructuralComparisonsTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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

using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Collections
{
	[TestFixture]
	public class StructuralComparisonsTest
	{
		[Test]
		public void EqualsTest ()
		{
			int[] a1 = new[] { 9, 1, 3, 4 };
			int[] a2 = new[] { 9, 1, 3, 4 };

			Assert.IsTrue (StructuralComparisons.StructuralEqualityComparer.Equals (a1, a2), "#1");
			Assert.IsFalse (StructuralComparisons.StructuralEqualityComparer.Equals (null, a2), "#2");
			Assert.IsFalse (StructuralComparisons.StructuralEqualityComparer.Equals (a1, null), "#3");
			Assert.IsTrue (StructuralComparisons.StructuralEqualityComparer.Equals (null, null), "#4");
			Assert.IsTrue (StructuralComparisons.StructuralEqualityComparer.Equals (4, 4), "#5");
			Assert.IsFalse (StructuralComparisons.StructuralEqualityComparer.Equals (4, 5), "#6");
		}
	}
}

#endif