//
// RuntimeEnvironmentCas.cs - CAS Unit Tests for RuntimeEnvironment
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
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

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

using NUnit.Framework;

namespace MonoCasTests.System.Runtime.InteropServices {

	[TestFixture]
	[Category ("CAS")]
	public class RuntimeEnvironmentCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager isn't enabled");
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PartialTrust_DenyUnrestricted_Success ()
		{
			Assembly corlib = typeof (int).Assembly;
#if NET_2_0
			Assert.IsTrue (RuntimeEnvironment.FromGlobalAccessCache (corlib), "corlib");
#else
			// note: mscorlib.dll wasn't in the GAC for 1.x
			Assert.IsFalse (RuntimeEnvironment.FromGlobalAccessCache (corlib), "corlib");
#endif
			Assembly corlib_test = Assembly.GetExecutingAssembly ();
			Assert.IsFalse (RuntimeEnvironment.FromGlobalAccessCache (corlib_test), "corlib_test");
		}

		// test Demand by denying the caller of the required privileges
		// (note: is should only be PathDiscovery but that's not easy to test)

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_GetRuntimeDirectory ()
		{
			Assert.IsNotNull (RuntimeEnvironment.GetRuntimeDirectory ());
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_SystemConfigurationFile ()
		{
			Assert.IsNotNull (RuntimeEnvironment.SystemConfigurationFile);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Deny_GetSystemVersion ()
		{
			Assert.IsNotNull (RuntimeEnvironment.GetSystemVersion ());
		}

		// test Demand by permiting only the required privileges
		// (note: is should only be PathDiscovery but that's not easy to test)

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PermitOnly_GetRuntimeDirectory ()
		{
			RuntimeEnvironment.GetRuntimeDirectory ();
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PermitOnly_SystemConfigurationFile ()
		{
			Assert.IsNotNull (RuntimeEnvironment.SystemConfigurationFile);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void PermitOnly_GetSystemVersion ()
		{
			Assert.IsNotNull (RuntimeEnvironment.GetSystemVersion ());
		}
	}
}
