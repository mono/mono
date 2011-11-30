//
// CacheControlHeaderValueTest.cs
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
using System.Linq;

namespace MonoTests.System.Net.Http.Headers
{
	[TestFixture]
	public class CacheControlHeaderValueTest
	{
		[Test]
		public void Ctor_Default ()
		{
			var value = new CacheControlHeaderValue ();

			Assert.AreEqual (0, value.Extensions.Count, "#1");
			Assert.IsNull (value.MaxAge, "#2");
			Assert.IsFalse (value.MaxStale, "#3");
			Assert.IsNull (value.MaxStaleLimit, "#4");
			Assert.IsNull (value.MinFresh, "#5");
			Assert.IsFalse (value.MustRevalidate, "#6");
			Assert.IsFalse (value.NoCache, "#7");
			Assert.IsFalse (value.NoStore, "#8");
			Assert.IsFalse (value.NoTransform, "#9");
			Assert.IsFalse (value.OnlyIfCached, "#10");
			Assert.IsFalse (value.Private, "#11");
			Assert.AreEqual (0, value.PrivateHeaders.Count, "#12");
			Assert.IsFalse (value.ProxyRevalidate, "#13");
			Assert.IsFalse (value.Public, "#14");
			Assert.IsNull (value.SharedMaxAge, "#15");
		}

		[Test]
		public void Equals ()
		{
			var value = new CacheControlHeaderValue ();
			Assert.AreEqual (value, new CacheControlHeaderValue (), "#1");
			Assert.AreNotEqual (value, new CacheControlHeaderValue () { MustRevalidate = true }, "#2");
		}

		[Test]
		public void Parse ()
		{
			var res = CacheControlHeaderValue.Parse ("audio");
			Assert.AreEqual ("audio", res.Extensions.First ().Name, "#1");
			Assert.IsNull (res.Extensions.First ().Value, "#2");

			res = CacheControlHeaderValue.Parse (null);
			Assert.IsNull (res, "#3");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				CacheControlHeaderValue.Parse ("audio/");
				Assert.Fail ("#1");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new CacheControlHeaderValue () {
				MaxAge = TimeSpan.MaxValue,
				MaxStale = true,
				MaxStaleLimit = TimeSpan.Zero,
				MinFresh = TimeSpan.MinValue,
				MustRevalidate = true,
				NoCache = true,
				NoStore = true,
				NoTransform = true,
				OnlyIfCached = true,
				Private = true,
				ProxyRevalidate = true,
				Public = true,
				SharedMaxAge = TimeSpan.MaxValue
			};
			
			Assert.AreEqual (0, value.Extensions.Count, "#1");
			Assert.AreEqual (TimeSpan.MaxValue, value.MaxAge, "#2");
			Assert.IsTrue (value.MaxStale, "#3");
			Assert.AreEqual (TimeSpan.Zero, value.MaxStaleLimit, "#4");
			Assert.AreEqual (TimeSpan.MinValue, value.MinFresh, "#5");
			Assert.IsTrue (value.MustRevalidate, "#6");
			Assert.IsTrue (value.NoCache, "#7");
			Assert.IsTrue (value.NoStore, "#8");
			Assert.IsTrue (value.NoTransform, "#9");
			Assert.IsTrue (value.OnlyIfCached, "#10");
			Assert.IsTrue (value.Private, "#11");
			Assert.AreEqual (0, value.PrivateHeaders.Count, "#12");
			Assert.IsTrue (value.ProxyRevalidate, "#13");
			Assert.IsTrue (value.Public, "#14");
			Assert.AreEqual (TimeSpan.MaxValue, value.SharedMaxAge, "#15");
		}

		[Test]
		public void TryParse ()
		{
			CacheControlHeaderValue res;
			Assert.IsTrue (CacheControlHeaderValue.TryParse ("*", out res), "#1");
			Assert.AreEqual (1, res.Extensions.Count, "#2");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			MediaTypeWithQualityHeaderValue res;
			Assert.IsFalse (MediaTypeWithQualityHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
