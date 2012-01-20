//
// EntityTagHeaderValueTest.cs
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
	public class EntityTagHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new EntityTagHeaderValue (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new EntityTagHeaderValue ("a");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Equals ()
		{
			var tfhv = new EntityTagHeaderValue ("\"abc\"");
			Assert.AreEqual (tfhv, new EntityTagHeaderValue ("\"abc\""), "#1");
			Assert.AreNotEqual (tfhv, new EntityTagHeaderValue ("\"AbC\""), "#2");
			Assert.AreNotEqual (tfhv, new EntityTagHeaderValue ("\"AA\""), "#3");
		}

		[Test]
		public void Parse ()
		{
			var res = EntityTagHeaderValue.Parse ("\"c\"");
			Assert.AreEqual ("\"c\"", res.Tag, "#1");
			Assert.IsFalse (res.IsWeak, "#2");
			Assert.AreEqual ("\"c\"", res.ToString (), "#3");

			res = EntityTagHeaderValue.Parse ("W/ \"mm\"");
			Assert.AreEqual ("\"mm\"", res.Tag, "#11");
			Assert.IsTrue (res.IsWeak, "#12");
			Assert.AreEqual ("W/\"mm\"", res.ToString (), "#13");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				EntityTagHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				EntityTagHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				EntityTagHeaderValue.Parse ("W / \"a\"");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}


		[Test]
		public void Properties ()
		{
			var etv = new EntityTagHeaderValue ("\"tag\"", true);
			Assert.AreEqual ("\"tag\"", etv.Tag, "#1");
			Assert.IsTrue (etv.IsWeak, "#2");

			Assert.AreEqual ("*", EntityTagHeaderValue.Any.Tag, "#3");
			Assert.IsFalse (EntityTagHeaderValue.Any.IsWeak, "#4");
		}

		[Test]
		public void TryParse ()
		{
			EntityTagHeaderValue res;
			Assert.IsTrue (EntityTagHeaderValue.TryParse ("\"\"", out res), "#1");
			Assert.AreEqual ("\"\"", res.Tag, "#2");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			EntityTagHeaderValue res;
			Assert.IsFalse (EntityTagHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
