//
// IssuedSecurityTokenParametersTest.cs
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
using System.IO;
using System.Net;
using System.Net.Security;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class IssuedSecurityTokenParametersTest
	{
		class MyParameters : IssuedSecurityTokenParameters
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
			Assert.AreEqual (false, tp.SupportsClientWindowsIdentityEx, "#6");
			Assert.AreEqual (true, tp.SupportsServerAuthenticationEx, "#7");

		}

		[Test]
		public void CreateRequestParameters ()
		{
			IssuedSecurityTokenParameters p =
				new IssuedSecurityTokenParameters ();
			p.ClaimTypeRequirements.Add (new ClaimTypeRequirement (ClaimTypes.Name, true));
			p.AdditionalRequestParameters.Add (new XmlDocument ()
				.CreateElement ("AdditionalFoo"));
			Collection<XmlElement> c = p.CreateRequestParameters (
				MessageSecurityVersion.Default,
				WSSecurityTokenSerializer.DefaultInstance);
			StringWriter sw = new StringWriter ();
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			foreach (XmlElement el in c) {
				XmlWriter w = XmlWriter.Create (sw, settings);
				el.WriteTo (w);
				w.Close ();
			}

			string expected = @"<t:KeyType xmlns:t='http://schemas.xmlsoap.org/ws/2005/02/trust'>http://schemas.xmlsoap.org/ws/2005/02/trust/SymmetricKey</t:KeyType><t:Claims xmlns:t='http://schemas.xmlsoap.org/ws/2005/02/trust'><wsid:ClaimType Uri='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name' Optional='true' xmlns:wsid='http://schemas.xmlsoap.org/ws/2005/05/identity' /></t:Claims><AdditionalFoo />";
			Assert.AreEqual (expected.Replace ('\'', '"'), sw.ToString ());
		}
	}
}
#endif
