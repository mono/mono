//
// RangeItemHeaderValueTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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
using System.Net.Http.Headers;

namespace MonoTests.System.Net.Http.Headers
{
	[TestFixture]
	public class RangeItemHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new RangeItemHeaderValue (null, null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new RangeItemHeaderValue (5, 2);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new RangeItemHeaderValue (-1, 2);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Ctor ()
		{
			var v = new RangeItemHeaderValue (1, null);
			Assert.AreEqual ("1-", v.ToString (), "#1");

			v = new RangeItemHeaderValue (null, 1);
			Assert.AreEqual ("-1", v.ToString (), "#2");
		}

		[Test]
		public void Equals ()
		{
			var value = new RangeItemHeaderValue (5, null);
			Assert.AreEqual (value, new RangeItemHeaderValue (5, null), "#1");
			Assert.AreNotEqual (value, new RangeItemHeaderValue (6, null), "#2");
			Assert.AreNotEqual (value, new RangeItemHeaderValue (5, 10), "#3");
		}

		[Test]
		public void Properties ()
		{
			var value = new RangeItemHeaderValue (3, 23);
			Assert.AreEqual (3, value.From, "#1");
			Assert.AreEqual (23, value.To, "#2");

			value = new RangeItemHeaderValue (5, null);
			Assert.AreEqual (5, value.From, "#3");
			Assert.IsNull (value.To, "#4");
		}
	}
}
