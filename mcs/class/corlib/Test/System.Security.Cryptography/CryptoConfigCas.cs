//
// CryptoConfigCas.cs -
//	CAS unit tests for System.Security.Cryptography.CryptoConfig
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

using NUnit.Framework;

using System;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;

namespace MonoCasTests.System.Security.Cryptography {

	[TestFixture]
	[Category ("CAS")]
	public class CryptoConfigCas {

		private MethodInfo create1;
		private MethodInfo create2;

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");

			Type t = typeof (CryptoConfig);

			Type[] one = new Type [1] { typeof (string) };
			create1 = t.GetMethod ("CreateFromName", one);

			Type[] two = new Type [2] { typeof (string), typeof (object[]) };
			create2 = t.GetMethod ("CreateFromName", two);
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PartialTrust_DenyUnrestricted_Success ()
		{
			MonoTests.System.Security.Cryptography.CryptoConfigTest cct = new MonoTests.System.Security.Cryptography.CryptoConfigTest ();
			// call most (all but arguments checking) unit tests from CryptoConfigTest
			cct.CCToString ();
			cct.CreateFromName ();
			cct.EncodeOID ();
			cct.MapNameToOID ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		[Category ("NotWorking")]
		public void PartialTrust_DenyUnrestricted_CreateFromURL ()
		{
			MonoTests.System.Security.Cryptography.CryptoConfigTest cct = new MonoTests.System.Security.Cryptography.CryptoConfigTest ();
			// this test must load System.Security to create some types
			cct.CreateFromURL ();
		}

		// we use reflection to call CryptoConfig as the CreateFromName methods are 
		// protected by LinkDemand (which will be converted into full demand, i.e. 
		// a stack walk) when reflection is used (i.e. it gets testable).

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void CreateFromName1 ()
		{
			Assert.IsNotNull (create1.Invoke (null, new object [1] { "DES" }), "1");
			// No restriction for the string only version
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateFromName2 ()
		{
			Assert.IsNotNull (create2.Invoke (null, new object [2] { "DES", new object [0] }), "2");
		}
	}
}
