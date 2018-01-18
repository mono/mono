//
// UserNameSecurityTokenParametersTest.cs
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
	public class UserNameSecurityTokenParametersTest
	{
		class MyUserNameSecurityTokenParameters : UserNameSecurityTokenParameters
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
			MyUserNameSecurityTokenParameters tp = new MyUserNameSecurityTokenParameters ();
			Assert.AreEqual (SecurityTokenInclusionMode.AlwaysToRecipient, tp.InclusionMode, "#1");
			Assert.AreEqual (SecurityTokenReferenceStyle.Internal, tp.ReferenceStyle, "#2");
			Assert.AreEqual (false, tp.RequireDerivedKeys, "#3");
			Assert.AreEqual (false, tp.HasAsymmetricKeyEx, "#4");
			Assert.AreEqual (true, tp.SupportsClientAuthenticationEx, "#5");
			Assert.AreEqual (true, tp.SupportsClientWindowsIdentityEx, "#6");
			Assert.AreEqual (false, tp.SupportsServerAuthenticationEx, "#7");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateKeyIdentifierClauseNullToken ()
		{
			new MyUserNameSecurityTokenParameters ().CreateKeyClause (null, SecurityTokenReferenceStyle.Internal);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateKeyIdentifierClauseNotSupportedToken ()
		{
			new MyUserNameSecurityTokenParameters ().CreateKeyClause (new RsaSecurityToken (RSA.Create ()), SecurityTokenReferenceStyle.Internal);
		}

		[Test]
		public void CreateKeyIdentifierClause ()
		{
			MyUserNameSecurityTokenParameters p =
				new MyUserNameSecurityTokenParameters ();
			UserNameSecurityToken token =
				new UserNameSecurityToken ("mono", "pass");
			SecurityKeyIdentifierClause c = p.CreateKeyClause (token, SecurityTokenReferenceStyle.Internal);
			Assert.IsTrue (c is LocalIdKeyIdentifierClause, "#1");

			try {
				p.CreateKeyClause (token, SecurityTokenReferenceStyle.External);
				Assert.Fail ("External identifier clause cannot be created.");
			} catch (NotSupportedException) {
			}
		}
	}
}
#endif
