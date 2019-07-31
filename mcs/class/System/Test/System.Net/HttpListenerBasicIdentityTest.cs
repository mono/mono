//
// HttpListenerBasicIdentityTest.cs
//	- Unit tests for System.Net.HttpListeneBasicIdentity
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
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
using System;
using System.Net;
using NUnit.Framework;

namespace MonoTests.System.Net {
	[TestFixture]
	public class HttpListenerBasicIdentityTest {
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Basic1 ()
		{
#if MONOTOUCH_WATCH
			Assert.Ignore ("HttpListenerBasicIdentity is not supported on watchOS");
#endif
			HttpListenerBasicIdentity bi = new HttpListenerBasicIdentity (null, null);
		}

		[Test]
		public void Basic2 ()
		{
#if MONOTOUCH_WATCH
			Assert.Ignore ("HttpListenerBasicIdentity is not supported on watchOS");
#endif
			HttpListenerBasicIdentity bi = new HttpListenerBasicIdentity ("", null);
			Assert.AreEqual ("Basic", bi.AuthenticationType, "#01");
			Assert.AreEqual ("", bi.Name, "#02");
			Assert.IsFalse (bi.IsAuthenticated, "#03");
			Assert.IsNull (bi.Password, "#04");
		}

		[Test]
		public void Basic3 ()
		{
#if MONOTOUCH_WATCH
			Assert.Ignore ("HttpListenerBasicIdentity is not supported on watchOS");
#endif
			HttpListenerBasicIdentity bi = new HttpListenerBasicIdentity ("hey", null);
			Assert.AreEqual ("Basic", bi.AuthenticationType, "#01");
			Assert.AreEqual ("hey", bi.Name, "#02");
			Assert.IsTrue (bi.IsAuthenticated, "#03");
			Assert.IsNull (bi.Password, "#04");
		}

		[Test]
		public void Basic4 ()
		{
#if MONOTOUCH_WATCH
			Assert.Ignore ("HttpListenerBasicIdentity is not supported on watchOS");
#endif
			HttpListenerBasicIdentity bi = new HttpListenerBasicIdentity ("hey", "pass");
			Assert.AreEqual ("Basic", bi.AuthenticationType, "#01");
			Assert.AreEqual ("hey", bi.Name, "#02");
			Assert.IsTrue (bi.IsAuthenticated, "#03");
			Assert.AreEqual ("pass", bi.Password, "#04");
		}
	}
}

