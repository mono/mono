//
// CodeAccessPermissionCas.cs - 
//	CAS unit tests for System.Security.CodeAccessPermission
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
using System.Collections;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.Security {

	[TestFixture]
	[Category ("CAS")]
	public class CodeAccessPermissionCas {

		private const SecurityPermissionFlag Both = SecurityPermissionFlag.RemotingConfiguration | SecurityPermissionFlag.UnmanagedCode;

		private bool result;

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
			result = false;
		}

		[SecurityPermission (SecurityAction.Demand, Assertion = true)]
		private void DemandAssertion ()
		{
			result = true;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Assertion = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Declarative_DenyAssertion_Assert ()
		{
			DemandAssertion ();
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		private void DemandUnmanagedCode ()
		{
			result = true;
		}

		[SecurityPermission (SecurityAction.Demand, RemotingConfiguration = true)]
		private void DemandRemotingConfiguration ()
		{
			result = true;
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true, RemotingConfiguration = true)]
		private void DemandBoth ()
		{
			result = true;
		}

		private void AssertSecurity (SecurityPermissionFlag flag)
		{
			bool unmanaged = ((flag & SecurityPermissionFlag.UnmanagedCode) != 0);
			bool remoting = ((flag & SecurityPermissionFlag.RemotingConfiguration) != 0);

			if (unmanaged && remoting)
				DemandBoth ();
			else if (unmanaged)
				DemandUnmanagedCode ();
			else if (remoting)
				DemandRemotingConfiguration ();
			else 
				Assert.Fail ("Invalid demand");
		}

		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		private void AssertUnmanagedCode (SecurityPermissionFlag flag)
		{
			AssertSecurity (flag);
		}

		[SecurityPermission (SecurityAction.Assert, RemotingConfiguration = true)]
		private void AssertRemotingConfiguration (SecurityPermissionFlag flag)
		{
			AssertSecurity (flag);
		}

		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true, RemotingConfiguration = true)]
		private void AssertBoth (SecurityPermissionFlag flag)
		{
			AssertSecurity (flag);
		}

		[SecurityPermission (SecurityAction.Assert, Unrestricted = true)]
		private void AssertUnrestricted (SecurityPermissionFlag flag)
		{
			AssertSecurity (flag);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		public void Declarative_DenyUnrestricted_AssertUnmanaged_DemandUnmanaged ()
		{
			AssertUnmanagedCode (SecurityPermissionFlag.UnmanagedCode);
			Assert.IsTrue (result);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Declarative_DenyUnrestricted_AssertUnmanaged_DemandRemoting ()
		{
			AssertUnmanagedCode (SecurityPermissionFlag.RemotingConfiguration);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Declarative_DenyUnrestricted_AssertUnmanaged_DemandBoth ()
		{
			AssertUnmanagedCode (Both);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Declarative_DenyUnrestricted_AssertRemoting_DemandUnmanaged ()
		{
			AssertRemotingConfiguration (SecurityPermissionFlag.UnmanagedCode);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		public void Declarative_DenyUnrestricted_AssertRemoting_DemandRemoting ()
		{
			AssertRemotingConfiguration (SecurityPermissionFlag.RemotingConfiguration);
			Assert.IsTrue (result);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Declarative_DenyUnrestricted_AssertRemoting_DemandBoth ()
		{
			AssertRemotingConfiguration (Both);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		public void Declarative_DenyUnrestricted_AssertBoth_DemandUnmanaged ()
		{
			AssertBoth (SecurityPermissionFlag.UnmanagedCode);
			Assert.IsTrue (result);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		public void Declarative_DenyUnrestricted_AssertBoth_DemandRemoting ()
		{
			AssertBoth (SecurityPermissionFlag.RemotingConfiguration);
			Assert.IsTrue (result);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		public void Declarative_DenyUnrestricted_AssertBoth_DemandBoth ()
		{
			AssertBoth (Both);
			Assert.IsTrue (result);
		}


		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		private void Assert_UnmanagedCode_RemotingConfiguration (SecurityPermissionFlag flag)
		{
			AssertRemotingConfiguration (flag);
		}

		[SecurityPermission (SecurityAction.Assert, RemotingConfiguration = true)]
		private void Assert_RemotingConfiguration_UnmanagedCode (SecurityPermissionFlag flag)
		{
			AssertUnmanagedCode (flag);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Declarative_DenyUnrestricted_AssertUnmanagedRemoting_DemandBoth ()
		{
			// no single stack frame can assert the whole SecurityPermission
			// which is different from a PermissionSet
			Assert_UnmanagedCode_RemotingConfiguration (Both);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		public void Declarative_DenyUnrestricted_AssertUnmanagedRemoting_DemandUnmanaged ()
		{
			Assert_UnmanagedCode_RemotingConfiguration (SecurityPermissionFlag.UnmanagedCode);
			Assert.IsTrue (result);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		public void Declarative_DenyUnrestricted_AssertUnmanagedRemoting_DemandRemoting ()
		{
			Assert_UnmanagedCode_RemotingConfiguration (SecurityPermissionFlag.RemotingConfiguration);
			Assert.IsTrue (result);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Declarative_DenyUnrestricted_AssertRemotingUnmanaged_DemandBoth ()
		{
			// no single stack frame can assert the whole SecurityPermission
			// which is different from a PermissionSet
			Assert_RemotingConfiguration_UnmanagedCode (Both);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		public void Declarative_DenyUnrestricted_AssertRemotingUnmanaged_DemandUnmanaged ()
		{
			Assert_RemotingConfiguration_UnmanagedCode (SecurityPermissionFlag.UnmanagedCode);
			Assert.IsTrue (result);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		public void Declarative_DenyUnrestricted_AssertRemotingUnmanaged_DemandRemoting ()
		{
			Assert_RemotingConfiguration_UnmanagedCode (SecurityPermissionFlag.RemotingConfiguration);
			Assert.IsTrue (result);
		}


		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		[ReflectionPermission (SecurityAction.Demand, ReflectionEmit = true)]
		private void Demand_Reflection_Unmanaged ()
		{
			result = true;
		}

		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		private void Assert_Unmanaged (bool call)
		{
			if (call)
				Demand_Reflection_Unmanaged ();
			else
				Assert_Reflection (true);
		}

		[ReflectionPermission (SecurityAction.Assert, ReflectionEmit = true)]
		private void Assert_Reflection (bool call)
		{
			if (call)
				Demand_Reflection_Unmanaged ();
			else
				Assert_Unmanaged (true);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		[ReflectionPermission (SecurityAction.Deny, Unrestricted = true)]
		public void Declarative_DenyUnrestricted_AssertReflectionUnmanaged_DemandUnmanagedReflection ()
		{
			Assert_Reflection (false);
			Assert.IsTrue (result);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		[ReflectionPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Declarative_DenyUnrestricted_AssertReflection_DemandUnmanagedReflection ()
		{
			Assert_Reflection (true);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		[ReflectionPermission (SecurityAction.Deny, Unrestricted = true)]
		public void Declarative_DenyUnrestricted_AssertUnmanagedReflection_DemandUnmanagedReflection ()
		{
			Assert_Unmanaged (false);
			Assert.IsTrue (result);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		[ReflectionPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Declarative_DenyUnrestricted_AssertUnmanaged_DemandUnmanagedReflection ()
		{
			Assert_Unmanaged (true);
		}

		[Test]
		[PermissionSet (SecurityAction.PermitOnly, Unrestricted = true)]
		public void Declarative_PermitOnly_Unrestricted ()
		{
			// permitonly unrestricted is (technically) a no-op
			DemandBoth ();
			Assert.IsTrue (result);
		}
	}
}
