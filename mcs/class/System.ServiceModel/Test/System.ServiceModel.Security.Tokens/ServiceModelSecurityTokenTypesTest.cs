//
// ServiceModelSecurityTokenTypesTest.cs
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ServiceModelSecurityTokenTypesTest
	{
		[Test]
		public void Strings ()
		{
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/AnonymousSslnego", ServiceModelSecurityTokenTypes.AnonymousSslnego, "#1");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/MutualSslnego", ServiceModelSecurityTokenTypes.MutualSslnego, "#2");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/SecureConversation", ServiceModelSecurityTokenTypes.SecureConversation, "#3");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/SecurityContextToken", ServiceModelSecurityTokenTypes.SecurityContext, "#4");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/Spnego", ServiceModelSecurityTokenTypes.Spnego, "#5");
			Assert.AreEqual ("http://schemas.microsoft.com/ws/2006/05/servicemodel/tokens/SspiCredential", ServiceModelSecurityTokenTypes.SspiCredential, "#6");
		}
	}
}
#endif