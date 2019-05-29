//
// IntranetZoneCredentialPolicyTest.cs 
//	- Unit tests for Microsoft.Win32.IntranetZoneCredentialPolicy
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
using System.Net;
using Microsoft.Win32;

namespace MonoTests.Microsoft.Win32 {

	public class Module: IAuthenticationModule {

		private string type;
		private bool pre_auth;
		private string token;

		public Module (string type, bool preAuth, string token)
		{
			this.type = type;
			pre_auth = preAuth;
			this.token = token;
		}

		public Authorization Authenticate (string challenge, WebRequest request, ICredentials credentials)
		{
			return new Authorization (token);
		}

		public string AuthenticationType {
			get { return type; }
		}

		public bool CanPreAuthenticate {
			get { return pre_auth; }
		}

		public Authorization PreAuthenticate (WebRequest request, ICredentials credentials)
		{
			return new Authorization (token);
		}
	}

	[TestFixture]
	public class IntranetZoneCredentialPolicyTest {

		private IntranetZoneCredentialPolicy policy;
		private Uri uri;
		private WebRequest request;
		private NetworkCredential credential;
		private IAuthenticationModule module;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			policy = new IntranetZoneCredentialPolicy ();
			uri = new Uri ("http://www.example.com");
			request = WebRequest.Create (uri);
			credential = new NetworkCredential ("me", "mine");
			module = new Module ("type", true, "token");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullUri ()
		{
			policy.ShouldSendCredential (null, request, credential, module);
		}

		[Test]
		public void NullRequest ()
		{
			Assert.IsFalse (policy.ShouldSendCredential (uri, null, credential, module));
		}

		[Test]
		public void NullCredential ()
		{
			Assert.IsFalse (policy.ShouldSendCredential (uri, request, null, module));
		}

		[Test]
		public void NullModule ()
		{
			Assert.IsFalse (policy.ShouldSendCredential (uri, request, credential, null));
		}

		[Test]
		public void Localhost ()
		{
			Uri localhost = new Uri ("http://localhost/");
			WebRequest wr = WebRequest.Create (uri);
			Assert.IsTrue (policy.ShouldSendCredential (localhost, wr, credential, module), "localhost");

			localhost = new Uri ("http://127.0.0.1");
			wr = WebRequest.Create (uri);
			Assert.IsFalse (policy.ShouldSendCredential (localhost, wr, credential, module), "127.0.0.1");
		}

		[Test]
		public void LocalhostWithoutWebRequest ()
		{
			Uri localhost = new Uri ("http://localhost/");
			Assert.IsTrue (policy.ShouldSendCredential (localhost, null, credential, module), "localhost");

			localhost = new Uri ("http://127.0.0.1");
			Assert.IsFalse (policy.ShouldSendCredential (localhost, null, credential, module), "127.0.0.1");
		}

		[Test]
		public void LocalhostWithoutCredentials ()
		{
			Uri localhost = new Uri ("http://localhost/");
			WebRequest wr = WebRequest.Create (uri);
			Assert.IsTrue (policy.ShouldSendCredential (localhost, wr, null, module), "localhost");

			localhost = new Uri ("http://127.0.0.1");
			wr = WebRequest.Create (uri);
			Assert.IsFalse (policy.ShouldSendCredential (localhost, wr, null, module), "127.0.0.1");
		}

		[Test]
		public void LocalhostWithoutModule ()
		{
			Uri localhost = new Uri ("http://localhost/");
			WebRequest wr = WebRequest.Create (uri);
			Assert.IsTrue (policy.ShouldSendCredential (localhost, wr, credential, null), "localhost");

			localhost = new Uri ("http://127.0.0.1");
			wr = WebRequest.Create (uri);
			Assert.IsFalse (policy.ShouldSendCredential (localhost, wr, credential, null), "127.0.0.1");
		}
	}
}

