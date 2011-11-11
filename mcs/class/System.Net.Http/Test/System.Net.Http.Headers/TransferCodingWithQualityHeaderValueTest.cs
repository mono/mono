//
// TransferCodingWithQualityHeaderValueTest.cs
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
	public class TransferCodingWithQualityHeaderValueTest
	{
		[Test]
		public void Ctor ()
		{
			var v = new TransferCodingWithQualityHeaderValue ("value");
			Assert.AreEqual ("value", v.Value, "#1");
			Assert.IsNull (v.Quality);
		}

		[Test]
		public void Quality ()
		{
			var v = new TransferCodingWithQualityHeaderValue ("value", 0.4);
			Assert.AreEqual ("value", v.Value, "#1");
			Assert.AreEqual (0.4, v.Quality, "#2");
		}

		[Test]
		public void Quality_Invalid ()
		{
			try {
				new TransferCodingWithQualityHeaderValue ("value", 4);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new TransferCodingWithQualityHeaderValue ("value") { Quality = -1 };
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}
	}
}