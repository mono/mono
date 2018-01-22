//
// SecurityTokenRequirementTest.cs
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
#if !MOBILE
using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Selectors
{
	[TestFixture]
	public class SecurityTokenRequirementTest
	{
		[Test]
		public void Constants ()
		{
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/identitymodel/securitytokenrequirement/KeySize",
				SecurityTokenRequirement.KeySizeProperty, "#1");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/identitymodel/securitytokenrequirement/KeyType",
				SecurityTokenRequirement.KeyTypeProperty, "#2");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/identitymodel/securitytokenrequirement/KeyUsage",
				SecurityTokenRequirement.KeyUsageProperty, "#3");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/identitymodel/securitytokenrequirement/RequireCryptographicToken",
				SecurityTokenRequirement.RequireCryptographicTokenProperty, "#4");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/identitymodel/securitytokenrequirement/TokenType",
				SecurityTokenRequirement.TokenTypeProperty, "#5");
		}

		[Test]
		public void DefaultValues ()
		{
			SecurityTokenRequirement r =
				new SecurityTokenRequirement ();
			Assert.AreEqual (0, r.KeySize, "#1");
			Assert.AreEqual (SecurityKeyType.SymmetricKey, r.KeyType, "#2");
			Assert.AreEqual (SecurityKeyUsage.Signature, r.KeyUsage, "#3");
			Assert.IsNull (r.TokenType, "#4");
			Assert.AreEqual (false, r.RequireCryptographicToken, "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TryGetPropertyTypeMismatch ()
		{
			SecurityTokenRequirement r =
				new SecurityTokenRequirement ();
			r.Properties ["urn:foo"] = 1;
			string s;
			r.TryGetProperty<string> ("urn:foo", out s);
		}

		[Test]
		public void TryGetPropertyTypeBaseMatch ()
		{
			SecurityTokenRequirement r =
				new SecurityTokenRequirement ();
			r.Properties ["urn:foo"] = 1;
			object o;
			r.TryGetProperty<object> ("urn:foo", out o);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TryGetPropertyTypeConvertible ()
		{
			SecurityTokenRequirement r =
				new SecurityTokenRequirement ();
			r.Properties ["urn:foo"] = 1;
			double d;
			r.TryGetProperty<double> ("urn:foo", out d);
		}
	}
}
#endif
