//
// RsaSecurityTokenParametersTest.cs
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
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class RsaSecurityTokenParametersTest
	{
		class MyRsaSecurityTokenParameters : RsaSecurityTokenParameters
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

			public SecurityKeyIdentifierClause CreateKeyClause (
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
			MyRsaSecurityTokenParameters tp = new MyRsaSecurityTokenParameters ();
			Assert.AreEqual (SecurityTokenInclusionMode.Never, tp.InclusionMode, "#1");
			Assert.AreEqual (SecurityTokenReferenceStyle.Internal, tp.ReferenceStyle, "#2");
			Assert.AreEqual (true, tp.RequireDerivedKeys, "#3");
			Assert.AreEqual (true, tp.HasAsymmetricKeyEx, "#4");
			Assert.AreEqual (true, tp.SupportsClientAuthenticationEx, "#5");
			Assert.AreEqual (false, tp.SupportsClientWindowsIdentityEx, "#6");
			Assert.AreEqual (true, tp.SupportsServerAuthenticationEx, "#7");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateKeyIdentifierClauseNullToken ()
		{
			MyRsaSecurityTokenParameters tp = new MyRsaSecurityTokenParameters ();
			tp.CreateKeyClause (
				null,
				SecurityTokenReferenceStyle.External);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateKeyIdentifierClauseWrongToken ()
		{
			MyRsaSecurityTokenParameters tp = new MyRsaSecurityTokenParameters ();
			tp.CreateKeyClause (
				new UserNameSecurityToken ("mono", "mono"),
				SecurityTokenReferenceStyle.External);
		}

		[Test]
		public void CreateKeyIdentifierClause ()
		{
			MyRsaSecurityTokenParameters tp = new MyRsaSecurityTokenParameters ();
			SecurityKeyIdentifierClause c = tp.CreateKeyClause (
				new RsaSecurityToken (RSA.Create ()),
				SecurityTokenReferenceStyle.Internal);
			Assert.IsTrue (c is RsaKeyIdentifierClause, "#1");
			c = tp.CreateKeyClause (
				new RsaSecurityToken (RSA.Create ()),
				SecurityTokenReferenceStyle.External);
			Assert.IsTrue (c is RsaKeyIdentifierClause, "#2");
		}
	}
}
#endif
