//
// MaxMinLengthAttributeTest.cs
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.DataAnnotations
{
	[TestFixture]
	public class MaxMinLengthAttributeTest
	{
		[Test]
		public void CheckMinLength () {
			var attr = new MinLengthAttribute (2);

			Assert.IsTrue (attr.IsValid (null), "#A1");
			Assert.IsFalse (attr.IsValid ("1"), "#A2");

			Assert.IsTrue (attr.IsValid ("12"), "#A3");
			Assert.IsTrue (attr.IsValid ("123"), "#A4");

			Assert.IsFalse (attr.IsValid (BuildQuickList (1)), "#A5");
			Assert.IsTrue (attr.IsValid (BuildQuickList (2)), "#A6");
			Assert.IsTrue (attr.IsValid (BuildQuickList (3)), "#A7");
		}

		[Test]
		public void CheckMaxLength () {
			var attr = new MaxLengthAttribute (2);

			Assert.IsTrue (attr.IsValid (null), "#A1");
			Assert.IsTrue (attr.IsValid ("1"), "#A2");
			Assert.IsTrue (attr.IsValid ("12"), "#A3");

			Assert.IsFalse (attr.IsValid ("123"), "#A4");

			Assert.IsTrue (attr.IsValid (BuildQuickList (1)), "#A5");
			Assert.IsTrue (attr.IsValid (BuildQuickList (2)), "#A6");
			Assert.IsFalse (attr.IsValid (BuildQuickList (3)), "#A7");
		}

		List<string> BuildQuickList (int count) {
			var items = new List<string> ();
			
			for (int i = 0; i < count; i++) {
				items.Add(i.ToString());
			}

			return items;
		}
	}
}
