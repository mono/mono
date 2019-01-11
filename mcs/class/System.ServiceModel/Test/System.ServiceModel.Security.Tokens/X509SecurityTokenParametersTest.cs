//
// X509SecurityTokenParametersTest.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class X509SecurityTokenParametersTest
	{
		class MyX509SecurityTokenParameters : X509SecurityTokenParameters
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
			MyX509SecurityTokenParameters tp = new MyX509SecurityTokenParameters ();
			Assert.AreEqual (SecurityTokenInclusionMode.AlwaysToRecipient, tp.InclusionMode, "#1");
			Assert.AreEqual (SecurityTokenReferenceStyle.Internal, tp.ReferenceStyle, "#2");
			Assert.AreEqual (true, tp.RequireDerivedKeys, "#3");
			Assert.AreEqual (true, tp.HasAsymmetricKeyEx, "#4");
			Assert.AreEqual (true, tp.SupportsClientAuthenticationEx, "#5");
			Assert.AreEqual (true, tp.SupportsClientWindowsIdentityEx, "#6");
			Assert.AreEqual (true, tp.SupportsServerAuthenticationEx, "#7");

			Assert.AreEqual (X509KeyIdentifierClauseType.Any, tp.X509ReferenceStyle, "#2-1");
		}

		[Test]
		public void InitializeRequirement ()
		{
			MyX509SecurityTokenParameters p =
				new MyX509SecurityTokenParameters ();
			InitiatorServiceModelSecurityTokenRequirement r =
				new InitiatorServiceModelSecurityTokenRequirement ();
			int before = r.Properties.Count;
			p.InitRequirement (r);
			Assert.AreEqual (1, r.Properties.Count - before, "#1"); // i.e. only TokenType is set.
			Assert.AreEqual (SecurityTokenTypes.X509Certificate, r.TokenType, "#2");
		}

		[Test]
		public void CreateKeyIdentifierClause ()
		{
			SecurityKeyIdentifierClause clause;

			MyX509SecurityTokenParameters p =
				new MyX509SecurityTokenParameters ();
			X509SecurityToken token = new X509SecurityToken (
				new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono"));
			clause = p.CallCreateKeyIdentifierClause (
				token, SecurityTokenReferenceStyle.External);
			Assert.AreEqual (typeof (X509IssuerSerialKeyIdentifierClause), clause.GetType (), "#1");
			clause = p.CallCreateKeyIdentifierClause (
				token, SecurityTokenReferenceStyle.Internal);
			Assert.AreEqual (typeof (LocalIdKeyIdentifierClause), clause.GetType (), "#2");

			p.InclusionMode = SecurityTokenInclusionMode.Never;
			// it still results in LocalId reference
			clause = p.CallCreateKeyIdentifierClause (
				token, SecurityTokenReferenceStyle.Internal);
			Assert.AreEqual (typeof (LocalIdKeyIdentifierClause), clause.GetType (), "#3");
		}
	}
}
#endif
