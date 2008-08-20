//
// XmlDictionaryReaderQuotasTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Xml;
using NUnit.Framework;

using Q = System.Xml.XmlDictionaryReaderQuotas;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlDictionaryReaderQuotasTest
	{
		[Test]
		public void StaticAndDefaultValues ()
		{
			/*
			Assert.AreEqual (
				int.MaxValue, Q.DefaultMaxArrayLength,
				"static DefaultMaxArrayLength");
			Assert.AreEqual (
				int.MaxValue, Q.DefaultMaxBytesPerRead,
				"static DefaultMaxBytesPerRead");
			Assert.AreEqual (
				int.MaxValue, Q.DefaultMaxDepth,
				"static DefaultMaxDepth");
			Assert.AreEqual (
				int.MaxValue, Q.DefaultMaxNameTableCharCount,
				"static DefaultMaxNameTableCharCount");
			Assert.AreEqual (
				int.MaxValue, Q.DefaultMaxStringContentLength,
				"static DefaultMaxStringContentLength");
			*/

			Q q = new Q (); //Q.Default;

			Assert.AreEqual (
				0x4000, q.MaxArrayLength,
				"default - MaxArrayLength");
			Assert.AreEqual (
				0x1000, q.MaxBytesPerRead,
				"default - MaxBytesPerRead");
			Assert.AreEqual (
				0x20, q.MaxDepth,
				"default - MaxDepth");
			Assert.AreEqual (
				0x4000, q.MaxNameTableCharCount,
				"default - MaxNameTableCharCount");
			Assert.AreEqual (
				0x2000, q.MaxStringContentLength,
				"default - MaxStringContentLength");

			q = Q.Max;

			Assert.AreEqual (
				int.MaxValue, q.MaxArrayLength,
				"max - MaxArrayLength");
			Assert.AreEqual (
				int.MaxValue, q.MaxBytesPerRead,
				"max - MaxBytesPerRead");
			Assert.AreEqual (
				int.MaxValue, q.MaxDepth,
				"max - MaxDepth");
			Assert.AreEqual (
				int.MaxValue, q.MaxNameTableCharCount,
				"max - MaxNameTableCharCount");
			Assert.AreEqual (
				int.MaxValue, q.MaxStringContentLength,
				"max - MaxStringContentLength");
		}

		[Test]
		public void SetZero ()
		{
			// having ExpectedExceptions for each test and splitting
			// methods is messy, so am just having all of them
			// in this test.
			Q q = new Q ();
			try {
				q.MaxArrayLength = 0;
				Assert.Fail ("MaxArrayLength = 0 should fail.");
			} catch (ArgumentException) {
			}
			try {
				q.MaxBytesPerRead = 0;
				Assert.Fail ("MaxBytesPerRead = 0 should fail.");
			} catch (ArgumentException) {
			}
			try {
				q.MaxDepth = 0;
				Assert.Fail ("MaxDepth = 0 should fail.");
			} catch (ArgumentException) {
			}
			try {
				q.MaxNameTableCharCount = 0;
				Assert.Fail ("MaxNameTableCharCount = 0 should fail.");
			} catch (ArgumentException) {
			}
			try {
				q.MaxStringContentLength = 0;
				Assert.Fail ("MaxStringContentLength = 0 should fail.");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void ReadonlyCheck ()
		{
			// having ExpectedExceptions for each test and splitting
			// methods is messy, so am just having all of them
			// in this test.
			Q q = Q.Max;
			try {
				q.MaxArrayLength = int.MaxValue;
				Assert.Fail ("Default MaxArrayLength is readonly");
			} catch (InvalidOperationException) {
			}
			try {
				q.MaxBytesPerRead = int.MaxValue;
				Assert.Fail ("Default MaxBytesPerRead is readonly");
			} catch (InvalidOperationException) {
			}
			try {
				q.MaxDepth = int.MaxValue;
				Assert.Fail ("Default MaxDepth is readonly");
			} catch (InvalidOperationException) {
			}
			try {
				q.MaxNameTableCharCount = int.MaxValue;
				Assert.Fail ("Default MaxNameTableCharCount is readonly");
			} catch (InvalidOperationException) {
			}
			try {
				q.MaxStringContentLength = int.MaxValue;
				Assert.Fail ("Default MaxStringContentLength is readonly");
			} catch (InvalidOperationException) {
			}
		}
	}
}
