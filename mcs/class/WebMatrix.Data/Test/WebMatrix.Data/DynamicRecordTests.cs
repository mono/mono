// 
// DynamicRecordTests.cs
//  
// Author:
//       Jérémie "garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2011 Novell
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET_4_0

using System;
using System.Collections.Generic;

using WebMatrix.Data;

using NUnit.Framework;

namespace MonoTests.WebMatrix.Data
{
	[TestFixtureAttribute]
	public class DynamicRecordTests
	{
		DynamicRecord record;

		[SetUp]
		public void Setup ()
		{
			var fields = new Dictionary<string, object> () {
				{ "foo", 1 },
				{ "bar", 4.1f },
				{ "foobar", "foobar" }
			};
			record = new DynamicRecord (fields);
		}

		[Test]
		public void ColumnsTest ()
		{
			var columns = record.Columns;
			Assert.AreEqual (3, columns.Count);

			Assert.AreEqual ("foo", columns[0]);
			Assert.AreEqual ("bar", columns[1]);
			Assert.AreEqual ("foobar", columns[2]);
		}

		[Test]
		public void AccessByNameTest ()
		{
			Assert.AreEqual (1, record["foo"]);
			Assert.AreEqual (4.1f, record["bar"]);
			Assert.AreEqual ("foobar", record["foobar"]);
		}

		[Test]
		public void AccessByIndexTest ()
		{
			Assert.AreEqual (1, record[0]);
			Assert.AreEqual (4.1f, record[1]);
			Assert.AreEqual ("foobar", record[2]);
		}

		[Test]
		public void AccesByDynamicTest ()
		{
			dynamic r = record;

			Assert.AreEqual (1, r.foo);
			Assert.AreEqual (4.1f, r.bar);
			Assert.AreEqual ("foobar", r.foobar);
		}

		[Test]
		public void GetDynamicMemberNamesTest ()
		{
			var expected = new string[] { "foo", "bar", "foobar" };
			CollectionAssert.AreEquivalent (expected, record.GetDynamicMemberNames ());
		}
	}
}
#endif