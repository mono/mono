//
// SamlConstantsTest.cs
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
using System.Globalization;
using System.IO;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Tokens
{
	[TestFixture]
	public class SamlConstantsTest
	{
		[Test]
		public void Constants ()
		{
			Assert.AreEqual ("EmailName", SamlConstants.EmailName, "#1");
			Assert.AreEqual ("urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress", SamlConstants.EmailNamespace, "#2");
			Assert.AreEqual ("urn:oasis:names:tc:SAML:1.0:cm:holder-of-key", SamlConstants.HolderOfKey, "#3");
			Assert.AreEqual (1, SamlConstants.MajorVersionValue, "#4");
			Assert.AreEqual (1, SamlConstants.MinorVersionValue, "#5");
			Assert.AreEqual ("urn:oasis:names:tc:SAML:1.0:assertion", SamlConstants.Namespace, "#6");
			Assert.AreEqual ("urn:oasis:names:tc:SAML:1.0:cm:sender-vouches", SamlConstants.SenderVouches, "#7");
			Assert.AreEqual ("UserName", SamlConstants.UserName, "#8");
			Assert.AreEqual ("urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName", SamlConstants.UserNameNamespace, "#9");
		}
	}
}
#endif