//
// X500DistinguishedNameCas.cs - CAS unit tests for 
//	System.Security.Cryptography.X509Certificates.X500DistinguishedName
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
	public class X500DistinguishedNameCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[Category ("NotWorking")]
		public void ReuseUnitTests_Deny_Unrestricted ()
		{
			X500DistinguishedNameTest unit = new X500DistinguishedNameTest ();
			unit.FixtureSetUp ();
			unit.Constructor_AsnEncodedData_Empty ();
			unit.Constructor_AsnEncodedData ();
			unit.Constructor_ByteArray_Empty ();
			unit.Constructor_ByteArray ();
			unit.Constructor_String_Empty ();
			unit.Constructor_String ();
			unit.Constructor_String_Empty_Flags ();
			unit.Constructor_String_Flags_None ();
			unit.Constructor_String_Flags_Reversed ();
			unit.Constructor_X500DistinguishedName ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			Type[] types = new Type[1] { typeof (AsnEncodedData) };
			ConstructorInfo ci = typeof (X500DistinguishedName).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(AsnEncodedData)");
			Assert.IsNotNull (ci.Invoke (new object [1] { new AsnEncodedData (new byte[0]) }), "invoke");
		}
	}
}

#endif
