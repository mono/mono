//
// ByteArrayContentTest.cs
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
using NUnit.Framework;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class ByteArrayContentTest
	{
		[Test]
		public void Ctor_Invalid ()
		{
			try {
				new ByteArrayContent (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				new ByteArrayContent (new byte[0], -1, 2);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new ByteArrayContent (new byte[0], 11, 1);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new ByteArrayContent (new byte[10], 9, 5);
				Assert.Fail ("#4");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Ctor ()
		{
			byte[] b = { 4, 6 };

			using (var m = new ByteArrayContent (b)) {
			}
		}

		[Test]
		public void CopyTo_Invalid ()
		{
			var m = new MemoryStream ();

			var sc = new ByteArrayContent (new byte[0]);
			try {
				sc.CopyToAsync (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void CopyToAsync ()
		{
			byte[] b = { 4, 2 };

			var sc = new ByteArrayContent (b);

			var dest = new MemoryStream ();
			var task = sc.CopyToAsync (dest);
			Assert.IsTrue (task.Wait (500));
			Assert.AreEqual (2, dest.Length, "#1");
		}

		[Test]
		public void LoadIntoBufferAsync ()
		{
			byte[] b = { 4 };

			var sc = new ByteArrayContent (b);
			var t = sc.LoadIntoBufferAsync (400);
			Assert.IsTrue (t.Wait (500));
		}

		[Test]
		public void ReadAsByteArrayAsync ()
		{
			byte[] b = { 4, 55 };

			var sc = new ByteArrayContent (b, 1, 1);
			var res = sc.ReadAsByteArrayAsync ().Result;
			Assert.AreEqual (1, res.Length, "#1");
			Assert.AreEqual (55, res[0], "#2");

			sc = new ByteArrayContent (b);
			res = sc.ReadAsByteArrayAsync ().Result;
			Assert.AreEqual (2, res.Length, "#10");
			Assert.AreEqual (55, res[1], "#11");
		}

		[Test]
		public void ReadAsStringAsync ()
		{
			byte[] b = { 77, 55 };

			var sc = new ByteArrayContent (b);
			var res = sc.ReadAsStringAsync ().Result;
			Assert.AreEqual ("M7", res, "#1");
		}
	}
}
