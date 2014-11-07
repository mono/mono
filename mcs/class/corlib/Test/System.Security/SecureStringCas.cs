//
// SecureStringCas.cs - CAS unit tests for System.Security.SecureString
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
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

using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

using NUnit.Framework;
using MonoTests.System.Security;

namespace MonoCasTests.System.Security {

	[TestFixture]
	[Category ("CAS")]
	public class SecureStringCas {

		private const string NotSupported = "Not supported before Windows 2000 Service Pack 3";

		[SetUp]
		public virtual void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void ReuseUnitTest ()
		{
			try {
				SecureStringTest unit = new SecureStringTest ();
				unit.DefaultConstructor ();
				unit.UnsafeConstructor ();
				unit.ReadOnly ();
				unit.Disposed ();
			}
			catch (NotSupportedException) {
				Assert.Ignore (NotSupported);
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			try {
				ConstructorInfo ci = typeof (SecureString).GetConstructor (new Type[0]);
				Assert.IsNotNull (ci, "default .ctor()");
				Assert.IsNotNull (ci.Invoke (null), "invoke");
			}
			catch (NotSupportedException) {
				Assert.Ignore (NotSupported);
			}
		}
	}
}

#endif
