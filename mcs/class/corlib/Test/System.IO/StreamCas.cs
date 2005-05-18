//
// StreamCas.cs - CAS unit tests for System.IO.Stream
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace MonoCasTests.System.IO {

	// System.IO.Stream is an abstract class, so we use our own inherited
	// class for the tests

	public class NonAbstractStream : Stream {

		private long _pos;
		private long _length;

		public override bool CanRead {
			get { return true; }
		}

		public override bool CanSeek {
			get { return true; }
		}

		public override bool CanWrite {
			get { return true; }
		}

		public override void Flush ()
		{
		}

		public override long Length {
			get { return _length; }
		}

		public override long Position {
			get { return _pos; }
			set { _pos = value; }
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			_pos += count;
			return count;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			_pos = offset;
			return _pos;
		}

		public override void SetLength (long value)
		{
			_length = value;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			_pos += count;
		}
	}

	[TestFixture]
	[Category ("CAS")]
	public class StreamCas {

		private const int timeout = 30000;
		private string message;

		static ManualResetEvent reset;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			reset = new ManualResetEvent (false);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			reset.Close ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// async tests (for stack propagation)

		private void ReadCallback (IAsyncResult ar)
		{
			NonAbstractStream s = (NonAbstractStream) ar.AsyncState;
			s.EndRead (ar);
			try {
				// can we do something bad here ?
				Assert.IsNotNull (Environment.GetEnvironmentVariable ("USERNAME"));
				message = "Expected a SecurityException";
			}
			catch (SecurityException) {
				message = null;
				reset.Set ();
			}
			catch (Exception e) {
				message = e.ToString ();
			}
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		public void AsyncRead ()
		{
			NonAbstractStream s = new NonAbstractStream ();
			message = "AsyncRead";
			reset.Reset ();
			IAsyncResult r = s.BeginRead (null, 0, 0, new AsyncCallback (ReadCallback), s);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		private void WriteCallback (IAsyncResult ar)
		{
			NonAbstractStream s = (NonAbstractStream)ar.AsyncState;
			s.EndWrite (ar);
			try {
				// can we do something bad here ?
				Assert.IsNotNull (Environment.GetEnvironmentVariable ("USERNAME"));
				message = "Expected a SecurityException";
			}
			catch (SecurityException) {
				message = null;
				reset.Set ();
			}
			catch (Exception e) {
				message = e.ToString ();
			}
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		public void AsyncWrite ()
		{
			NonAbstractStream s = new NonAbstractStream ();
			message = "AsyncWrite";
			reset.Reset ();
			IAsyncResult r = s.BeginWrite (null, 0, 0, new AsyncCallback (WriteCallback), s);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}
	}
}
