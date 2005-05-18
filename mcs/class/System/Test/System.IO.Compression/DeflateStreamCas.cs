//
// DeflateStreamCas.cs -CAS unit tests for System.IO.Compression.DeflateStream
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace MonoCasTests.System.IO.Compression {

	[TestFixture]
	[Category ("CAS")]
	public class DeflateStreamCas {

		private const int timeout = 30000;
		private string message;

		static ManualResetEvent reset;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// this occurs with a "clean" stack (full trust)
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
			DeflateStream s = (DeflateStream)ar.AsyncState;
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
			DeflateStream cs = new DeflateStream (new MemoryStream (), CompressionMode.Decompress);
			message = "AsyncRead";
			reset.Reset ();
			IAsyncResult r = cs.BeginRead (new byte[0], 0, 0, new AsyncCallback (ReadCallback), cs);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
			cs.Close ();
		}

		private void WriteCallback (IAsyncResult ar)
		{
			DeflateStream s = (DeflateStream)ar.AsyncState;
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
			DeflateStream cs = new DeflateStream (new MemoryStream (), CompressionMode.Compress);
			message = "AsyncWrite";
			reset.Reset ();
			IAsyncResult r = cs.BeginWrite (new byte[1], 0, 1, new AsyncCallback (WriteCallback), cs);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
// the Close is currently buggy in Mono
//			cs.Close ();
		}
	}
}

#endif
