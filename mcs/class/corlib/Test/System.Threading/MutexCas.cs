//
// MutexCas.cs - CAS unit tests for System.Threading.Mutex
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
using System.Security.Permissions;
using System.Threading;

namespace MonoCasTests.System.Threading {

	[TestFixture]
	[Category ("CAS")]
	public class MutexCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PartialTrust_DenyUnrestricted_Success ()
		{
			MonoTests.System.Threading.MutexTest mt = new MonoTests.System.Threading.MutexTest ();
			// call the few working unit tests
			mt.TestCtor1 ();
			mt.TestHandle ();
		}		

		// we use reflection to call Mutex as it's named constructors are protected by
		// a LinkDemand (which will be converted into full demand, i.e. a stack walk) 
		// when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Ctor_BoolString ()
		{
			Type[] parameters = new Type [2] { typeof (bool), typeof (string) };
			ConstructorInfo ci = typeof (Mutex).GetConstructor (parameters);
			Assert.IsNotNull (ci, "ctor(bool,string)");
			ci.Invoke (new object [2] { false, String.Empty });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Ctor_BoolStringOutBool ()
		{
			ConstructorInfo ci = null;
			// I don't know an easier way to deal with "out bool"
			ConstructorInfo[] cis = typeof (Mutex).GetConstructors ();
			for (int i=0; i < cis.Length; i++) {
				ParameterInfo[] pis = cis [i].GetParameters ();
				if (pis.Length == 3) {
					ci = cis [i];
					break;
				}
			}
			Assert.IsNotNull (ci, "ctor(bool,string,out bool)");
			// not sure the invoke would work - but it's enough to trigger the security check
			ci.Invoke (new object [3] { false, String.Empty, false });
		}
	}
}
