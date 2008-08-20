//
// SspiSecurityTokenParametersTest.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using NUnit.Framework;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace MonoTests.System.ServiceModel.Security.Tokens
{
	[TestFixture]
	public class SspiSecurityTokenParametersTest
	{
		class MyParameters : SspiSecurityTokenParameters
		{
			public bool HasAsymmetricKeyEx {
				get { return HasAsymmetricKey; }
			}

			public bool SupportsClientAuthenticationEx {
				get { return SupportsClientAuthentication; }
			}

			public bool SupportsClientWindowsIdentityEx {
				get { return SupportsClientWindowsIdentity; }
			}

			public bool SupportsServerAuthenticationEx {
				get { return SupportsServerAuthentication; }
			}

			public SecurityKeyIdentifierClause CallCreateKeyIdentifierClause (
				SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
			{
				return CreateKeyIdentifierClause (token, referenceStyle);
			}

			public void InitRequirement (SecurityTokenRequirement requirement)
			{
				InitializeSecurityTokenRequirement (requirement);
			}
		}

		[Test]
		public void DefaultValues ()
		{
			MyParameters tp = new MyParameters ();
			Assert.AreEqual (SecurityTokenInclusionMode.AlwaysToRecipient, tp.InclusionMode, "#1");
			Assert.AreEqual (SecurityTokenReferenceStyle.Internal, tp.ReferenceStyle, "#2");
			Assert.AreEqual (true, tp.RequireDerivedKeys, "#3");

			Assert.AreEqual (false, tp.HasAsymmetricKeyEx, "#4");
			Assert.AreEqual (true, tp.SupportsClientAuthenticationEx, "#5");
			Assert.AreEqual (true, tp.SupportsClientWindowsIdentityEx, "#6");
			Assert.AreEqual (true, tp.SupportsServerAuthenticationEx, "#7");

			Assert.AreEqual (false, tp.RequireCancellation, "#2-1");
		}

		[Test]
		public void InitializeSecurityTokenParameters ()
		{
			MyParameters tp = new MyParameters ();
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			tp.InitRequirement (r);

			Assert.AreEqual (ServiceModelSecurityTokenTypes.Spnego, r.TokenType, "#1");
			Assert.AreEqual (false, r.Properties [ReqType.SupportSecurityContextCancellationProperty], "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderNoTargetAddress ()
		{
			MyParameters tp = new MyParameters ();
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			tp.InitRequirement (r);

			ClientCredentials cred = new ClientCredentials ();
			ClientCredentialsSecurityTokenManager manager =
				new ClientCredentialsSecurityTokenManager (cred);
			manager.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderNoSecurityBindingElement ()
		{
			MyParameters tp = new MyParameters ();
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			tp.InitRequirement (r);
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");

			ClientCredentials cred = new ClientCredentials ();
			ClientCredentialsSecurityTokenManager manager =
				new ClientCredentialsSecurityTokenManager (cred);
			manager.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderNoIssuerBindingContext ()
		{
			MyParameters tp = new MyParameters ();
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			tp.InitRequirement (r);
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");
			r.SecurityBindingElement = new SymmetricSecurityBindingElement ();

			ClientCredentials cred = new ClientCredentials ();
			ClientCredentialsSecurityTokenManager manager =
				new ClientCredentialsSecurityTokenManager (cred);
			//SecurityTokenProvider p =
				manager.CreateSecurityTokenProvider (r);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateProviderNoMessageSecurityVersion ()
		{
			MyParameters tp = new MyParameters ();
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			tp.InitRequirement (r);
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");
			r.SecurityBindingElement = new SymmetricSecurityBindingElement ();
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (new CustomBinding (),
					new BindingParameterCollection ());

			ClientCredentials cred = new ClientCredentials ();
			ClientCredentialsSecurityTokenManager manager =
				new ClientCredentialsSecurityTokenManager (cred);
			//SecurityTokenProvider p =
				manager.CreateSecurityTokenProvider (r);
		}

		[Test]
		public void CreateProvider ()
		{
			MyParameters tp = new MyParameters ();
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			tp.InitRequirement (r);
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");
			r.SecurityBindingElement = new SymmetricSecurityBindingElement ();
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (new CustomBinding (),
					new BindingParameterCollection ());
			r.MessageSecurityVersion = MessageSecurityVersion.Default.SecurityTokenVersion;

			ClientCredentials cred = new ClientCredentials ();
			ClientCredentialsSecurityTokenManager manager =
				new ClientCredentialsSecurityTokenManager (cred);
			//SecurityTokenProvider p =
				manager.CreateSecurityTokenProvider (r);
		}

		void CreateProviderOpenGetToken (bool getToken)
		{
			MyParameters tp = new MyParameters ();
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			tp.InitRequirement (r);
			r.TargetAddress = new EndpointAddress ("http://localhost:8080");
			r.SecurityBindingElement = new SymmetricSecurityBindingElement ();
			r.Properties [ReqType.IssuerBindingContextProperty] =
				new BindingContext (new CustomBinding (new HttpTransportBindingElement ()),
					new BindingParameterCollection ());
			r.MessageSecurityVersion = MessageSecurityVersion.Default.SecurityTokenVersion;
			// This is required at GetToken().
			r.SecurityAlgorithmSuite = SecurityAlgorithmSuite.Default;
			ClientCredentials cred = new ClientCredentials ();
			ClientCredentialsSecurityTokenManager manager =
				new ClientCredentialsSecurityTokenManager (cred);
			// TLS negotiation token provider is created.
			SecurityTokenProvider p =
				manager.CreateSecurityTokenProvider (r);

			((ICommunicationObject) p).Open ();

			if (!getToken)
				return;

			p.GetToken (TimeSpan.FromSeconds (5));
		}

		[Test]
		public void CreateProviderOpen ()
		{
			CreateProviderOpenGetToken (false);
		}

		[Test]
		[Ignore ("This ends up to fail to connect. Anyways it's too implementation dependent.")]
		public void CreateProviderGetToken ()
		{
			CreateProviderOpenGetToken (true);
		}

		[Test]
		public void CreateAuthenticator ()
		{
			MyParameters tp = new MyParameters ();
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			tp.InitRequirement (r);

			ClientCredentials cred = new ClientCredentials ();
			ClientCredentialsSecurityTokenManager manager =
				new ClientCredentialsSecurityTokenManager (cred);

			SecurityTokenResolver resolver;
//			SecurityTokenAuthenticator authenticator =
				manager.CreateSecurityTokenAuthenticator (r, out resolver);
		}
	}
}
