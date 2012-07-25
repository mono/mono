//
// ContentDispositionHeaderValueTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
	public class ContentDispositionHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new ContentDispositionHeaderValue (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new ContentDispositionHeaderValue ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}
		}

		[Test]
		/*
		 * This fails on Windows with the .NET runtime:
		 * 
		 * Test Case Failures:
		 * 1) MonoTests.System.Net.Http.Headers.ContentDispositionHeaderValueTest.Equals : System.NullReferenceException : Der Objektverweis wurde nicht auf eine Objektinstanz festgelegt.
		 * bei System.Net.Http.Headers.ContentDispositionHeaderValue.set_Size(Nullable`1 value)
		 * bei MonoTests.System.Net.Http.Headers.ContentDispositionHeaderValueTest.Equals()
		 * 
		 */
		[Category ("NotWorking")]
		public void Equals ()
		{
			var value = new ContentDispositionHeaderValue ("x");
			Assert.AreEqual (value, new ContentDispositionHeaderValue ("x"), "#1");
			Assert.AreNotEqual (value, new ContentDispositionHeaderValue ("y"), "#2");

			value = new ContentDispositionHeaderValue ("attachment");
			value.Parameters.Add (new NameValueHeaderValue ("size", "66"));

			Assert.AreEqual (value, new ContentDispositionHeaderValue ("attachment") { Size = 66 }, "#3");
			Assert.AreNotEqual (value, new ContentDispositionHeaderValue ("attachment"), "#4");
			Assert.AreNotEqual (value, new ContentDispositionHeaderValue ("attachment") { FileName="g" }, "#5");
		}

		[Test]
		public void Parse ()
		{
			var res = ContentDispositionHeaderValue.Parse ("attachment");
			Assert.AreEqual ("attachment", res.DispositionType, "#1");
			Assert.AreEqual ("attachment", res.ToString (), "#2");

			res = ContentDispositionHeaderValue.Parse ("attachmen;filename=foo;size=44;  name=n2; filename*=UTF-8''Na%C3%AFve%20file.txt; creation-date=\"Wed, 02 Oct 2002 15:00:00 +0200\";modification-date=\"Wed, 02 Oct 2002 13:00:00 GMT\";read-date=\"Wed, 02 Oct 2002 15:00:00 +0000\";other=1");
			Assert.AreEqual (new DateTimeOffset (2002, 10, 2, 15, 0, 0, TimeSpan.FromHours (2)), res.CreationDate, "#10");
			Assert.AreEqual ("attachmen", res.DispositionType, "#11");
			Assert.AreEqual ("foo", res.FileName, "#12");
			Assert.AreEqual ("Naïve file.txt", res.FileNameStar, "#13");
			Assert.AreEqual (new DateTimeOffset (2002, 10, 2, 13, 0, 0, TimeSpan.Zero), res.ModificationDate, "#14");
			Assert.AreEqual ("n2", res.Name, "#15");
			Assert.AreEqual (8, res.Parameters.Count, "#16");
			Assert.AreEqual (new DateTimeOffset (2002, 10, 2, 15, 0, 0, TimeSpan.Zero), res.ReadDate, "#17");
			Assert.AreEqual (44, res.Size, "#18");
			Assert.AreEqual ("attachmen; filename=foo; size=44; name=n2; filename*=UTF-8''Na%C3%AFve%20file.txt; creation-date=\"Wed, 02 Oct 2002 15:00:00 +0200\"; modification-date=\"Wed, 02 Oct 2002 13:00:00 GMT\"; read-date=\"Wed, 02 Oct 2002 15:00:00 +0000\"; other=1", res.ToString (), "#19");

			res = ContentDispositionHeaderValue.Parse ("attachment; filename='foo.bar'");
			Assert.AreEqual ("attachment", res.DispositionType, "#21");
			Assert.AreEqual ("'foo.bar'", res.FileName, "#22");
			Assert.AreEqual ("attachment; filename='foo.bar'", res.ToString (), "#23");

			ContentDispositionHeaderValue.Parse ("aa;size=a4");

			res = ContentDispositionHeaderValue.Parse ("att;filename=\"=?utf-8?B?xI0=?=\"");
			Assert.AreEqual ("č", res.FileName, "#31");
			Assert.IsNull (res.FileNameStar, "#32");

			res = ContentDispositionHeaderValue.Parse ("att;filename=\"=?utf-?B?xI0=?=\"");
			Assert.AreEqual ("\"=?utf-?B?xI0=?=\"", res.FileName, "#41");

			res = ContentDispositionHeaderValue.Parse ("att;filename=\"=?utf-16?b?xI0=?=\"");
			Assert.AreEqual ("跄", res.FileName, "#51");

			res = ContentDispositionHeaderValue.Parse ("att;filename=\"=?utf-8?B?x/I0=?=\"");
			Assert.AreEqual ("\"=?utf-8?B?x/I0=?=\"", res.FileName, "#61");

			res = ContentDispositionHeaderValue.Parse ("att;filename*=utf-8''%C4%8Eas");
			Assert.AreEqual ("Ďas", res.FileNameStar, "#71");

			res = ContentDispositionHeaderValue.Parse ("att;filename*=btf-8''%C4%8E");
			Assert.IsNull (res.FileNameStar, "#72");

			res = ContentDispositionHeaderValue.Parse ("att;filename*=utf-8''%T4%8O%");
			Assert.AreEqual ("%T4%8O%", res.FileNameStar, "#73");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				ContentDispositionHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				ContentDispositionHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				ContentDispositionHeaderValue.Parse ("attachment; filename=foo.html ;");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}

			try {
				ContentDispositionHeaderValue.Parse ("attachment; filename=foo bar.html");
				Assert.Fail ("#4");
			} catch (FormatException) {
			}

			try {
				ContentDispositionHeaderValue.Parse ("\"attachment\"");
				Assert.Fail ("#5");
			} catch (FormatException) {
			}

			try {
				ContentDispositionHeaderValue.Parse ("att;filename*=utf-8''%T4%8╗");
				Assert.Fail ("#6");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new ContentDispositionHeaderValue ("ttt");
			Assert.IsNull (value.CreationDate, "#1");
			Assert.AreEqual ("ttt", value.DispositionType, "#2");
			Assert.IsNull (value.FileName, "#3");
			Assert.IsNull (value.FileNameStar, "#4");
			Assert.IsNull (value.ModificationDate, "#5");
			Assert.IsNull (value.Name, "#6");
			Assert.AreEqual (0, value.Parameters.Count, "#7");
			Assert.IsNull (value.ReadDate, "#8");
			Assert.IsNull (value.Size, "#9");

			value.Parameters.Add (new NameValueHeaderValue ("creation-date", "\"20 Jun 82 11:34:11\""));
			value.Parameters.Add (new NameValueHeaderValue ("filename", "g*"));
			value.Parameters.Add (new NameValueHeaderValue ("filename*", "ag*"));
			value.Parameters.Add (new NameValueHeaderValue ("modification-date", "\"20 Jun 22 4:6:22\""));
			value.Parameters.Add (new NameValueHeaderValue ("name", "nnn"));
			value.Parameters.Add (new NameValueHeaderValue ("read-date", "\"1 Jun 01 1:1:1\""));
			value.Parameters.Add (new NameValueHeaderValue ("size", "5"));

			Assert.AreEqual (new DateTimeOffset (1982, 6, 20, 11, 34, 11, TimeSpan.Zero), value.CreationDate, "#11");
			Assert.AreEqual ("g*", value.FileName, "#12");
			Assert.IsNull (value.FileNameStar, "#13");
			Assert.AreEqual (new DateTimeOffset (2022, 6, 20, 4, 6, 22, TimeSpan.Zero), value.ModificationDate, "#14");
			Assert.AreEqual ("nnn", value.Name, "#15");
			Assert.AreEqual (new DateTimeOffset (2001, 6, 1, 1, 1, 1, TimeSpan.Zero), value.ReadDate, "#16");
			Assert.AreEqual (5, value.Size, "#17");
		}

		[Test]
		public void Properties_FileName ()
		{
			var value = new ContentDispositionHeaderValue ("a");

			value.FileName = "aa";
			Assert.AreEqual ("aa", value.FileName, "#1");
			Assert.AreEqual (new NameValueHeaderValue ("filename", "aa"), value.Parameters.First (), "#2");

			value.FileName = "č";
			Assert.AreEqual ("č", value.FileName, "#11");
			Assert.AreEqual (new NameValueHeaderValue ("filename", "\"=?utf-8?B?xI0=?=\""), value.Parameters.First (), "#12");

			value.FileName = "(@)";
			Assert.AreEqual ("\"(@)\"", value.FileName, "#21");
			Assert.AreEqual (new NameValueHeaderValue ("filename", "\"(@)\""), value.Parameters.First (), "#22");
		}

		[Test]
		public void Properties_FileNameStar ()
		{
			var value = new ContentDispositionHeaderValue ("a");

			value.FileNameStar = "aa";
			Assert.AreEqual ("aa", value.FileNameStar, "#1");
			Assert.AreEqual (new NameValueHeaderValue ("filename*", "utf-8''aa"), value.Parameters.First (), "#2");

			value.FileNameStar = "č";
			Assert.AreEqual ("č", value.FileNameStar, "#11");
			Assert.AreEqual (new NameValueHeaderValue ("filename*", "utf-8''%C4%8D"), value.Parameters.First (), "#12");
		}

		[Test]
		public void Properties_ModificationDate ()
		{
			var value = new ContentDispositionHeaderValue ("a");

			value.ModificationDate = new DateTimeOffset (2010, 12, 30, 22, 34, 2, TimeSpan.Zero); //.FromHours (-3));
			Assert.AreEqual (new DateTimeOffset (2010, 12, 30, 22, 34, 2, TimeSpan.Zero), value.ModificationDate, "#1");
			Assert.AreEqual (new NameValueHeaderValue ("modification-date", "\"Thu, 30 Dec 2010 22:34:02 GMT\""), value.Parameters.First (), "#2");
		}

		[Test]
		public void Properties_Invalid ()
		{
			var value = new ContentDispositionHeaderValue ("a");
			try {
				value.Size = -9;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void TryParse ()
		{
			ContentDispositionHeaderValue res;
			Assert.IsTrue (ContentDispositionHeaderValue.TryParse ("attachment; filename*0*=ISO-8859-15''euro-sign%3d%a4; filename*=ISO-8859-1''currency-sign%3d%a4", out res), "#1");
			Assert.AreEqual ("attachment", res.DispositionType, "#2");
			Assert.AreEqual ("currency-sign=¤", res.FileNameStar, "#3");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			ContentDispositionHeaderValue res;
			Assert.IsFalse (ContentDispositionHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
