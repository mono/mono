//
// SemaphoreCas.cs - CAS unit tests for System.Threading.Semaphore
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
using System.Threading;

using MonoTests.System.Threading;

namespace MonoCasTests.System.Threading {

	[TestFixture]
	[Category ("CAS")]
	public class SemaphoreCas {

		private SemaphoreTest unit;

		[TestFixtureSetUp]
		public void FixtureSetup ()
		{
			unit = new SemaphoreTest ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[Category ("NotWorking")] // not implemented in Mono
		public void ReuseUnitTest_Deny_Unrestricted ()
		{
			unit.Constructor_IntInt ();
			unit.Constructor_IntIntString_NullName ();
			unit.Constructor_IntIntStringBool_NegativeInitialCount ();
			unit.Constructor_IntIntStringBool_ZeroMaximumCount ();
			unit.Constructor_IntIntStringBool_InitialBiggerThanMaximum ();
			unit.Constructor_IntIntStringBool_NullName ();
			unit.Constructor_IntIntStringBoolSecurity_NegativeInitialCount ();
			unit.Constructor_IntIntStringBoolSecurity_ZeroMaximumCount ();
			unit.Constructor_IntIntStringBoolSecurity_InitialBiggerThanMaximum ();
			unit.Constructor_IntIntStringBoolSecurity_NullName ();
			unit.Constructor_IntIntStringBoolSecurity ();
			unit.OpenExisting_BadRights ();
			unit.Release ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		[Category ("NotWorking")] // not implemented in Mono
		public void ReuseUnitTest_Deny_UnmanagedCode ()
		{
			unit.AccessControl_Unnamed ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[Category ("NotWorking")] // not implemented in Mono
		public void ReuseUnitTest_PermitOnly_UnmanagedCode ()
		{
			unit.AccessControl_Unnamed ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			Type[] types = new Type[2] { typeof (int), typeof (int) };
			ConstructorInfo ci = typeof (Semaphore).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(int,int)");
			Assert.IsNotNull (ci.Invoke (new object[2] { 0, 1 }), "invoke");
		}
	}
}

#endif
