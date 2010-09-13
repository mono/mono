//
// SecurityMessagePropertyTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Security.Cryptography.Xml;
using System.Threading;
using NUnit.Framework;

using MonoTests.System.ServiceModel.Channels;

namespace MonoTests.System.ServiceModel.Security
{
	[TestFixture]
	public class SecurityMessagePropertyTest
	{
		static X509Certificate2 cert = new X509Certificate2 ("Test/Resources/test.pfx", "mono");
		static X509Certificate2 cert2 = new X509Certificate2 ("Test/Resources/test2.pfx", "mono");

		[ServiceContract]
		public interface ICalc
		{
			[OperationContract]
			int Sum (int a, int b);

			[OperationContract (AsyncPattern = true)]
			IAsyncResult BeginSum (int a, int b, AsyncCallback cb, object state);

			int EndSum (IAsyncResult result);
		}

		public class CalcProxy : ClientBase<ICalc>, ICalc
		{
			public CalcProxy (Binding binding, EndpointAddress address)
				: base (binding, address)
			{
			}

			public int Sum (int a, int b)
			{
				return Channel.Sum (a, b);
			}

			public IAsyncResult BeginSum (int a, int b, AsyncCallback cb, object state)
			{
				return Channel.BeginSum (a, b, cb, state);
			}

			public int EndSum (IAsyncResult result)
			{
				return Channel.EndSum (result);
			}
		}

		public class CalcService : ICalc
		{
			public int Sum (int a, int b)
			{
				return a + b;
			}

			public IAsyncResult BeginSum (int a, int b, AsyncCallback cb, object state)
			{
				return new CalcAsyncResult (a, b, cb, state);
			}

			public int EndSum (IAsyncResult result)
			{
				CalcAsyncResult c = (CalcAsyncResult) result;
				return c.A + c.B;
			}
		}

		class CalcAsyncResult : IAsyncResult
		{
			public int A, B;
			AsyncCallback callback;
			object state;

			public CalcAsyncResult (int a, int b, AsyncCallback cb, object state)
			{
				A = a;
				B = b;
				callback = cb;
				this.state = state;
			}

			public object AsyncState {
				get { return state; }
			}

			public WaitHandle AsyncWaitHandle {
				get { return null; }
			}

			public bool CompletedSynchronously {
				get { return true; }
			}

			public bool IsCompleted {
				get { return true; }
			}
		}

		[Test]
		public void GetOrCreateNonSecureMessage ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "urn:myaction");
			SecurityMessageProperty p =
				SecurityMessageProperty.GetOrCreate (m);
			Assert.IsNull (p.InitiatorToken, "#1");
			Assert.IsNull (p.RecipientToken, "#2");
			Assert.IsNull (p.ProtectionToken, "#3");
			Assert.IsNull (p.TransportToken, "#4");
			Assert.IsNull (p.ExternalAuthorizationPolicies, "#5");
//			Assert.AreEqual (0, p.ExternalAuthorizationPolicies.Count, "#5");
			Assert.IsFalse (p.HasIncomingSupportingTokens, "#6");
			Assert.IsNotNull (p.IncomingSupportingTokens, "#6-2");
			Assert.AreEqual ("_", p.SenderIdPrefix, "#6-3");
			ServiceSecurityContext ssc = p.ServiceSecurityContext;
			Assert.IsNotNull (ssc, "#7");

			// not sure if it is worthy of testing though ...
			GenericIdentity identity = ssc.PrimaryIdentity as GenericIdentity;
			Assert.IsNotNull (identity, "#8-1");
			Assert.AreEqual ("", identity.Name, "#8-2");
			Assert.AreEqual ("", identity.AuthenticationType, "#8-3");

			Assert.AreEqual (0, ssc.AuthorizationPolicies.Count, "#9");
			Assert.IsTrue (ssc.IsAnonymous, "#10");
		}

		[Test]
		[Ignore ("This hangs on .NET")]
		// not sure how "good" this test is ... if it fails at
		// service side, it just results in timeout error.
		// The assertion makes sure that it passes all the tests, but
		// in case it failed, there is almost no hint ...
		public void GetOrCreateSecureMessage ()
		{
			bool passed = false;
			ServiceHost host = new ServiceHost (typeof (CalcService));
			InterceptorRequestContextHandler handler = delegate (MessageBuffer src) {
				Message msg = src.CreateMessage ();
				GetOrCreateSecureMessageAtService (msg);
				passed = true;
			};

			try {
				SymmetricSecurityBindingElement clisbe =
					new SymmetricSecurityBindingElement ();
				clisbe.ProtectionTokenParameters =
					new X509SecurityTokenParameters (X509KeyIdentifierClauseType.Thumbprint, SecurityTokenInclusionMode.Never);
				BindingElement transport = new HttpTransportBindingElement ();
				BindingElement sintercept = new InterceptorBindingElement (handler);
				CustomBinding b_res = new CustomBinding (clisbe,
					sintercept,
					transport);
				b_res.ReceiveTimeout = b_res.SendTimeout = TimeSpan.FromSeconds (5);
				host.AddServiceEndpoint (typeof (ICalc), b_res, "http://localhost:37564");

				ServiceCredentials cred = new ServiceCredentials ();
				cred.ServiceCertificate.Certificate = cert;
				cred.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
				host.Description.Behaviors.Add (cred);

				host.Open ();

				ProcessClient ();

			} finally {
				if (host.State == CommunicationState.Opened)
					host.Close ();
			}
			if (!passed)
				Assert.Fail ("Didn't pass the interceptor.");
		}

		void ProcessClient ()
		{
			SymmetricSecurityBindingElement svcsbe =
				new SymmetricSecurityBindingElement ();
			svcsbe.ProtectionTokenParameters =
				new X509SecurityTokenParameters (X509KeyIdentifierClauseType.Thumbprint, SecurityTokenInclusionMode.Never);

			BindingElement cintercept = new InterceptorBindingElement (null);
			CustomBinding b_req = new CustomBinding (svcsbe,
				cintercept,
				new HttpTransportBindingElement ());

			b_req.ReceiveTimeout = b_req.SendTimeout = TimeSpan.FromSeconds (5);
			EndpointAddress remaddr = new EndpointAddress (
				new Uri ("http://localhost:37564"),
				new X509CertificateEndpointIdentity (cert));
			CalcProxy proxy = new CalcProxy (b_req, remaddr);
			proxy.ClientCredentials.ClientCertificate.Certificate = cert2;

			proxy.Sum (1, 2);
			proxy.Close ();
		}

		static void GetOrCreateSecureMessageAtClient (Message msg)
		{
			foreach (object o in msg.Properties)
				if (o is SecurityMessageProperty)
					Assert.Fail ("The input msg should not contain SecurityMessageProperty yet.");
			SecurityMessageProperty p = SecurityMessageProperty.GetOrCreate (msg);

			Assert.AreEqual (null, p.InitiatorToken, "#1");
			Assert.AreEqual (null, p.RecipientToken, "#2");
			Assert.IsNull (p.ProtectionToken, "#3");
			Assert.IsNull (p.TransportToken, "#4");
			Assert.IsNull (p.ExternalAuthorizationPolicies, "#5");
//			Assert.AreEqual (0, p.ExternalAuthorizationPolicies.Count, "#5");
			Assert.IsFalse (p.HasIncomingSupportingTokens, "#6");
			Assert.IsNotNull (p.IncomingSupportingTokens, "#6-2");
			Assert.AreEqual ("_", p.SenderIdPrefix, "#6-3");
			ServiceSecurityContext ssc = p.ServiceSecurityContext;
			Assert.IsNotNull (ssc, "#7");

			// not sure if it is worthy of testing though ...
			GenericIdentity identity = ssc.PrimaryIdentity as GenericIdentity;
			Assert.IsNotNull (identity, "#8-1");
			Assert.AreEqual ("", identity.Name, "#8-2");
			Assert.AreEqual ("", identity.AuthenticationType, "#8-3");

			Assert.AreEqual (0, ssc.AuthorizationPolicies.Count, "#9");
			Assert.IsTrue (ssc.IsAnonymous, "#10");
		}

		static void GetOrCreateSecureMessageAtService (Message msg)
		{
			Assert.IsNull (msg.Properties.Security, "#0");
			foreach (object o in msg.Properties)
				if (o is SecurityMessageProperty)
					Assert.Fail ("The input msg should not contain SecurityMessageProperty yet.");
			SecurityMessageProperty p = SecurityMessageProperty.GetOrCreate (msg);
			Assert.IsNotNull (msg.Properties.Security, "#0-2");

			Assert.AreEqual (null, p.InitiatorToken, "#1");
			Assert.AreEqual (null, p.RecipientToken, "#2");
			Assert.IsNull (p.ProtectionToken, "#3");
			Assert.IsNull (p.TransportToken, "#4");
			Assert.IsNull (p.ExternalAuthorizationPolicies, "#5");
//			Assert.AreEqual (0, p.ExternalAuthorizationPolicies.Count, "#5");
			Assert.IsFalse (p.HasIncomingSupportingTokens, "#6");
			Assert.IsNotNull (p.IncomingSupportingTokens, "#6-2");
			Assert.AreEqual ("_", p.SenderIdPrefix, "#6-3");
			ServiceSecurityContext ssc = p.ServiceSecurityContext;
			Assert.IsNotNull (ssc, "#7");

			// not sure if it is worthy of testing though ...
			GenericIdentity identity = ssc.PrimaryIdentity as GenericIdentity;
			Assert.IsNotNull (identity, "#8-1");
			Assert.AreEqual ("", identity.Name, "#8-2");
			Assert.AreEqual ("", identity.AuthenticationType, "#8-3");

			Assert.AreEqual (0, ssc.AuthorizationPolicies.Count, "#9");
			Assert.IsTrue (ssc.IsAnonymous, "#10");
		}
	}
}
