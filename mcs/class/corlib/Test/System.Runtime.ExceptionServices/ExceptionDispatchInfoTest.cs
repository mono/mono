//
// ExceptionDispatchInfoTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
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
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

namespace MonoTests.System.Runtime.ExceptionServices
{
	[TestFixture]
	[Category ("BitcodeNotWorking")]
	public class ExceptionDispatchInfoTest
	{
		static string[] GetLines (string str)
		{
			var lines = str.Split (new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			// Ignore Metadata
			return lines.Where (l => !l.StartsWith ("[")).ToArray ();
		}

		[Test]
		public void Capture_InvalidArguments ()
		{
			try {
				ExceptionDispatchInfo.Capture (null);
				Assert.Fail ();
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void Capture ()
		{
			var e = new ApplicationException ("test");
			var edi = ExceptionDispatchInfo.Capture (e);
			Assert.AreEqual (e, edi.SourceException);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void Throw ()
		{
			Exception orig = null;
			var t = Task.Factory.StartNew (() => {
				try {
					throw new ApplicationException ("aaa");
				} catch (Exception e) {
					orig = e;
					return ExceptionDispatchInfo.Capture (e);
				}
			});

			var ed = t.Result;
			var orig_stack = orig.StackTrace;
			try {
				ed.Throw ();
				Assert.Fail ("#0");
			} catch (Exception e) {
				var s = GetLines (e.StackTrace);
				Assert.AreEqual (3, s.Length, "#1");
				Assert.AreEqual (orig, e, "#2");
				Assert.AreNotEqual (orig_stack, e.StackTrace, "#3");
			}
		}

		[Test]
		[Category ("StackWalks")]
		public void ThrowWithEmptyFrames ()
		{
			var edi = ExceptionDispatchInfo.Capture (new OperationCanceledException ());
			try {
				edi.Throw ();
				Assert.Fail ("#0");
			} catch (OperationCanceledException e) {
				Assert.IsTrue (!e.StackTrace.Contains("---"));
				var lines = GetLines (e.StackTrace);
				Assert.AreEqual (1, lines.Length, "#1");
			}
		}

		[Test]
		[Category ("StackWalks")]
		public void LastThrowWins ()
		{
			Exception e;
			try {
				throw new Exception ("test");
			} catch (Exception e2) {
				e = e2;
			}

			var edi = ExceptionDispatchInfo.Capture (e);

			try {
				edi.Throw ();
			} catch {
			}

			try {
				edi.Throw ();
			} catch (Exception ex) {
			}

			try {
				edi.Throw ();
			} catch (Exception ex) {
				var lines = GetLines (ex.StackTrace);
				Assert.AreEqual (3, lines.Length, "#1");
				Assert.IsTrue (lines [1].Contains ("---"), "#2");
			}
		}

		[Test]
		[Category ("StackWalks")]
		public void ThrowMultipleCaptures ()
		{
			Exception e;
			try {
				throw new Exception ("test");
			} catch (Exception e2) {
				e = e2;
			}

			var edi = ExceptionDispatchInfo.Capture (e);

			try {
				edi.Throw ();
			} catch (Exception e3) {
				edi = ExceptionDispatchInfo.Capture (e3);
			}

			try {
				edi.Throw ();
			} catch (Exception ex) {
				var lines = GetLines (ex.StackTrace);
				Assert.AreEqual (5, lines.Length, "#1");
				Assert.IsTrue (lines [1].Contains ("---"), "#2");
				Assert.IsTrue (lines [3].Contains ("---"), "#3");
			}
		}

		[Test]
		[Category ("StackWalks")]
		public void StackTraceUserCopy ()
		{
			try {
				try {
					throw new NotImplementedException ();
				} catch (Exception e) {
					var edi = ExceptionDispatchInfo.Capture (e);
					edi.Throw();
				}
			} catch (Exception ex) {
				var st = new StackTrace (ex, true);
				var lines = GetLines (st.ToString ());
				Assert.AreEqual (3, lines.Length, "#1");
				Assert.IsTrue (lines [1].Contains ("---"), "#2");
			}
		}
	}
}

