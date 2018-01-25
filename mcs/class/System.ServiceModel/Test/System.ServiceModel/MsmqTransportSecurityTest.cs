//
// MsmqTransportSecurityTest.cs
//
// Author:
//	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class MsmqTransportSecurityTest
	{
		[Test]
		public void DefaultValues ()
		{
			MsmqTransportSecurity security = new MsmqTransportSecurity ();

			Assert.AreEqual (MsmqAuthenticationMode.WindowsDomain, security.MsmqAuthenticationMode, "#A1");
			Assert.AreEqual (MsmqEncryptionAlgorithm.RC4Stream, security.MsmqEncryptionAlgorithm, "#A2");
			Assert.AreEqual (ProtectionLevel.Sign, security.MsmqProtectionLevel, "#A3");
			Assert.AreEqual (MsmqSecureHashAlgorithm.Sha1, security.MsmqSecureHashAlgorithm, "#A4");
		}
	}
}
#endif
