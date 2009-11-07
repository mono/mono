//
// ExpandSegmentTests.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2009 Eric Maupin (http://www.ermau.com)
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

using System.Linq.Expressions;
using NUnit.Framework;

namespace System.Data.Services.Tests {
	[TestFixture]
	public class ExpandSegmentTests {
		[Test]
		public void CtorName()
		{
			var s = new ExpandSegment ("name", null);
			Assert.AreEqual ("name", s.Name);
			Assert.AreEqual (null, s.Filter);
			Assert.IsFalse (s.HasFilter);
		}

		[Test]
		public void CtorFilter()
		{
			var param = Expression.Parameter (typeof (bool), "b");
			var filter = Expression.Lambda (param, param);
			var s = new ExpandSegment ("name", filter);

			Assert.AreEqual ("name", s.Name);
			Assert.AreEqual (filter, s.Filter);
			Assert.IsTrue (s.HasFilter);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNullName()
		{
			new ExpandSegment (null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PathHasFilterException()
		{
			ExpandSegment.PathHasFilter (null);
		}

		[Test]
		public void PathHasFilter()
		{
			var param = Expression.Parameter (typeof (bool), "b");
			var filter = Expression.Lambda (param, param);

			Assert.IsTrue (ExpandSegment.PathHasFilter (new []
			{ new ExpandSegment ("first", null), new ExpandSegment ("second", filter), new ExpandSegment ("third", null) }));
		}

		[Test]
		public void PathDoesntHaveFilter()
		{
			Assert.IsFalse (ExpandSegment.PathHasFilter (new []
			{ new ExpandSegment ("first", null), new ExpandSegment ("second", null), new ExpandSegment ("third", null) }));
		}
	}
}