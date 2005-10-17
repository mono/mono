//
// SessionEndedEventArgsCas.cs 
//	- CAS unit tests for Microsoft.Win32.SessionEndedEventArgs
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
using Microsoft.Win32;

namespace MonoCasTests.Microsoft.Win32 {

	[TestFixture]
	[Category ("CAS")]
	public class SessionEndedEventArgsCas {

		[SetUp]
		public virtual void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Deny_Unrestricted ()
		{
			SessionEndedEventArgs seea = new SessionEndedEventArgs (SessionEndReasons.SystemShutdown);
			Assert.AreEqual (SessionEndReasons.SystemShutdown, seea.Reason, "Reason");
		}

		// LinkDemand

		// we use reflection to call this class as it is protected by a LinkDemand 
		// (which will be converted into full demand, i.e. a stack walk) when 
		// reflection is used (i.e. it gets testable).

		public virtual object Create ()
		{
			Type[] t = new Type[1] { typeof (SessionEndReasons) };
			ConstructorInfo ci = typeof (SessionEndedEventArgs).GetConstructor (t);
			Assert.IsNotNull (ci, ".ctor(SessionEndReasons)");
			return ci.Invoke (new object[1] { SessionEndReasons.SystemShutdown });
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_Unrestricted ()
		{
			Assert.IsNotNull (Create ());
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "MONO")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_Anything ()
		{
			// denying any permissions -> not full trust!
			Assert.IsNotNull (Create ());
		}

		[Test]
		[PermissionSet (SecurityAction.PermitOnly, Unrestricted = true)]
		public void LinkDemand_PermitOnly_Unrestricted ()
		{
			Assert.IsNotNull (Create ());
		}
	}
}
