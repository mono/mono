//
// ApplicationHostCas.cs 
//	- CAS unit tests for System.Web.Hosting.ApplicationHost
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
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.Hosting;

namespace MonoCasTests.System.Web.Hosting {

	[TestFixture]
	[Category ("CAS")]
	public class ApplicationHostCas : AspNetHostingMinimal {

		private void CreateApplicationHost ()
		{
			try {
				ApplicationHost.CreateApplicationHost (null, null, null);
			}
			catch (NullReferenceException) {
				// MS
			}
			catch (NotImplementedException) {
				// Mono
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CreateApplicationHost_Deny_UnmanagedCode ()
		{
			CreateApplicationHost ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void CreateApplicationHost_PermitOnly_UnmanagedCode ()
		{
			CreateApplicationHost ();
		}

		// test for LinkDemand on class

		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// in this case testing with a (private) ctor isn't very conveniant
			// and the LinkDemand promotion (to Demand) will still occurs on other stuff
			// and (finally) we know that the CreateApplicationHost method is only 
			// protected for unmanaged code (which we can assert)
			try {
				MethodInfo mi = this.Type.GetMethod ("CreateApplicationHost");
				Assert.IsNotNull (mi, "method");
				return mi.Invoke (null, new object[3] { null, null, null });
			}
			catch (TargetInvocationException e) {
				// so we hit the same exception as we did for the ctor
				if (e.InnerException is NullReferenceException)
					return String.Empty; // MS
				else if (e.InnerException is NotImplementedException)
					return String.Empty; // Mono
				else
					return null;
			}
		}

		public override Type Type {
			get { return typeof (ApplicationHost); }
		}
	}
}
