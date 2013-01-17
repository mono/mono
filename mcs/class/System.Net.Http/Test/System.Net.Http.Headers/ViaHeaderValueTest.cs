//
// ViaHeaderValueTest.cs
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
	public class ViaHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new ViaHeaderValue (null, "rb");
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new ViaHeaderValue (" ", null);
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				new ViaHeaderValue ("a", null);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			try {
				new ViaHeaderValue ("a", "b", "::");
				Assert.Fail ("#4");
			} catch (FormatException) {
			}

			try {
				new ViaHeaderValue ("a", "b", null, "(aaa");
				Assert.Fail ("#5");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Ctor_ValidArguments ()
		{
			new ViaHeaderValue ("a", "b", null);
			new ViaHeaderValue ("a", "b", null, null);
		}

		[Test]
		public void Equals ()
		{
			var value = new ViaHeaderValue ("ab", "x");
			Assert.AreEqual (value, new ViaHeaderValue ("ab", "x"), "#1");
			Assert.AreEqual (value, new ViaHeaderValue ("AB", "X"), "#2");
			Assert.AreNotEqual (value, new ViaHeaderValue ("AA", "x"), "#3");

			value = new ViaHeaderValue ("ab", "DD", "cc");
			Assert.AreEqual (value, new ViaHeaderValue ("Ab", "DD", "cC"), "#4");
			Assert.AreNotEqual (value, new ViaHeaderValue ("AB", "DD"), "#5");
			Assert.AreNotEqual (value, new ViaHeaderValue ("Ab", "dd", "cc", "(c)"), "#6");
		}

		[Test]
		public void Parse ()
		{
			var res = ViaHeaderValue.Parse ("1.1 nowhere.com");
			Assert.IsNull (res.ProtocolName, "#1");
			Assert.AreEqual ("nowhere.com", res.ReceivedBy, "#2");
			Assert.AreEqual ("1.1", res.ProtocolVersion, "#3");
			Assert.AreEqual ("1.1 nowhere.com", res.ToString (), "#4");

			res = ViaHeaderValue.Parse ("foo / 1.1 nowhere.com:43   ( lalala ) ");
			Assert.AreEqual ("foo", res.ProtocolName, "#10");
			Assert.AreEqual ("1.1", res.ProtocolVersion, "#11");
			Assert.AreEqual ("nowhere.com:43", res.ReceivedBy, "#12");
			Assert.AreEqual ("( lalala )", res.Comment, "#13");
			Assert.AreEqual ("foo/1.1 nowhere.com:43 ( lalala )", res.ToString (), "#14");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				ViaHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				ViaHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				ViaHeaderValue.Parse ("a");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}

			try {
				ViaHeaderValue.Parse ("1 nowhere.com :43");
				Assert.Fail ("#4");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new ViaHeaderValue ("s", "p");
			Assert.IsNull (value.ProtocolName, "#1");
			Assert.AreEqual ("s", value.ProtocolVersion, "#2");
			Assert.AreEqual ("p", value.ReceivedBy, "#3");

			value = new ViaHeaderValue ("s", "rb", "name");
			Assert.AreEqual ("name", value.ProtocolName, "#4");
			Assert.AreEqual ("s", value.ProtocolVersion, "#5");
			Assert.AreEqual ("rb", value.ReceivedBy, "#6");

			value = new ViaHeaderValue ("s", "rb", "name", "(cmt)");
			Assert.AreEqual ("name", value.ProtocolName, "#7");
			Assert.AreEqual ("s", value.ProtocolVersion, "#8");
			Assert.AreEqual ("rb", value.ReceivedBy, "#9");
			Assert.AreEqual ("(cmt)", value.Comment, "#10");
		}

		[Test]
		public void TryParse ()
		{
			ViaHeaderValue res;
			Assert.IsTrue (ViaHeaderValue.TryParse ("a b", out res), "#1");
			Assert.AreEqual ("b", res.ReceivedBy, "#2");
			Assert.AreEqual ("a", res.ProtocolVersion, "#3");
			Assert.IsNull (res.Comment, "#4");
			Assert.IsNull (res.ProtocolName, "#5");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			ViaHeaderValue res;
			Assert.IsFalse (ViaHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
