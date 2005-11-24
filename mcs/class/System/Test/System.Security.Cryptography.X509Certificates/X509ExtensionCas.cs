//
// X509ExtensionCas.cs - CAS unit tests for 
//	System.Security.Cryptography.X509Certificates.X509Extension
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using MonoTests.System.Security.Cryptography.X509Certificates;

namespace MonoCasTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	[Category ("CAS")]
	public class X509ExtensionCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ReuseUnitTests_Deny_Unrestricted ()
		{
			X509ExtensionTest unit = new X509ExtensionTest ();
			unit.ConstructorEmpty ();
			unit.ConstructorAsnEncodedData ();
			unit.ConstructorAsnEncodedData_BadAsn ();
			unit.ConstructorAsnEncodedData_BadAsnTag ();
			unit.ConstructorAsnEncodedData_BadAsnLength ();
			unit.Build_NetscapeCertTypeExtension ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			Type[] types = new Type[2] { typeof (AsnEncodedData), typeof (bool) };
			ConstructorInfo ci = typeof (X509Extension).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(AsnEncodedData,bool)");
			AsnEncodedData aed = new AsnEncodedData (new Oid ("2.5.29.37"), new byte[] { 0x30, 0x05, 0x06, 0x03, 0x2A, 0x03, 0x04 });
			Assert.IsNotNull (ci.Invoke (new object [2] { aed, false }), "invoke");
		}
	}
}

#endif
