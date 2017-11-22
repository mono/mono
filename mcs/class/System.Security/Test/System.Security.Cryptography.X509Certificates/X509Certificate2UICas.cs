//
// X509Certificate2UITest.cs - CAS tests for X509Certificate2UI
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
#if !MOBILE


using NUnit.Framework;

using System;
using System.Reflection;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

using MonoTests.System.Security.Cryptography.X509Certificates;

namespace MonoCasTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	[Category ("CAS")]
	public class X509Certificate2UICas {

		private X509Certificate2UITest unit;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			unit = new X509Certificate2UITest ();
			unit.FixtureSetUp ();
		}

		[Test]
		[UIPermission (SecurityAction.Deny, Window=UIPermissionWindow.AllWindows)]
		[ExpectedException (typeof (SecurityException))]
		public void DisplayCertificate_Deny_Unrestricted ()
		{
			unit.DisplayCertificate ();
		}

		[Test]
		[UIPermission (SecurityAction.PermitOnly, Window = UIPermissionWindow.SafeSubWindows)]
		[ExpectedException (typeof (SecurityException))]
		public void DisplayCertificate_PermitOnly_SafeSubWindows ()
		{
			unit.DisplayCertificate ();
		}

		[Test]
		[UIPermission (SecurityAction.PermitOnly, Window = UIPermissionWindow.SafeTopLevelWindows)]
		[Ignore ("UI would block tests")]
		public void DisplayCertificate_PermitOnly_SafeTopLevelWindows ()
		{
			unit.DisplayCertificate ();
		}

		[Test]
		[UIPermission (SecurityAction.PermitOnly, Window = UIPermissionWindow.SafeSubWindows)]
		[ExpectedException (typeof (SecurityException))]
		public void DisplayCertificate_IntPtr_PermitOnly_SafeSubWindows ()
		{
			unit.DisplayCertificate_IntPtr_Zero ();
		}

		[Test]
		[UIPermission (SecurityAction.PermitOnly, Window = UIPermissionWindow.SafeTopLevelWindows)]
		[Ignore ("UI would block tests")]
		public void DisplayCertificate_IntPtr_PermitOnly_SafeTopLevelWindows ()
		{
			unit.DisplayCertificate_IntPtr_Zero ();
		}


		[Test]
		[UIPermission (SecurityAction.Deny, Window = UIPermissionWindow.AllWindows)]
		[ExpectedException (typeof (SecurityException))]
		public void SelectFromCollection_Deny_Unrestricted ()
		{
			unit.SelectFromCollection ();
		}

		[Test]
		[UIPermission (SecurityAction.PermitOnly, Window = UIPermissionWindow.SafeSubWindows)]
		[ExpectedException (typeof (SecurityException))]
		public void SelectFromCollection_PermitOnly_SafeSubWindows ()
		{
			unit.SelectFromCollection ();
		}

		[Test]
		[UIPermission (SecurityAction.PermitOnly, Window = UIPermissionWindow.SafeTopLevelWindows)]
		[Ignore ("UI would block tests")]
		public void SelectFromCollection_PermitOnly_SafeTopLevelWindows ()
		{
			unit.SelectFromCollection ();
		}

		[Test]
		[UIPermission (SecurityAction.PermitOnly, Window = UIPermissionWindow.SafeSubWindows)]
		[ExpectedException (typeof (SecurityException))]
		public void SelectFromCollection_IntPtr_PermitOnly_SafeSubWindows ()
		{
			unit.SelectFromCollection_IntPtr_Zero ();
		}

		[Test]
		[UIPermission (SecurityAction.PermitOnly, Window = UIPermissionWindow.SafeTopLevelWindows)]
		[Ignore ("UI would block tests")]
		public void SelectFromCollection_IntPtr_PermitOnly_SafeTopLevelWindows ()
		{
			unit.SelectFromCollection_IntPtr_Zero ();
		}

		// the methods accepting an IntPtr are documented as having a LinkDemand
		// and InheritanceDemand for UnmanagedCode. InheritanceDemand doesn't make
		// sense as the class is sealed but we can test the LinkDemand with reflection

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[Ignore ("UI would block tests")]
		public void LinkDemand_DisplayCertificate_Deny_UnmanagedCode ()
		{
			Type[] types = new Type[1] { typeof (X509Certificate2) };
			MethodInfo mi = typeof (X509Certificate2UI).GetMethod ("DisplayCertificate", types);
			mi.Invoke (null, new object[1] { unit.x509 });
			// no LinkDemand on the DisplayCertificate(X509Certificate2) method
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_DisplayCertificate_IntPtr_Deny_UnmanagedCode ()
		{
			Type[] types = new Type[2] { typeof (X509Certificate2), typeof (IntPtr) };
			MethodInfo mi = typeof (X509Certificate2UI).GetMethod ("DisplayCertificate", types);
			mi.Invoke (null, new object[2] { unit.x509, IntPtr.Zero } );
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[UIPermission (SecurityAction.PermitOnly, Window = UIPermissionWindow.SafeTopLevelWindows)]
		[Ignore ("UI would block tests")]
		public void LinkDemand_DisplayCertificate_IntPtr_Permit ()
		{
			Type[] types = new Type[2] { typeof (X509Certificate2), typeof (IntPtr) };
			MethodInfo mi = typeof (X509Certificate2UI).GetMethod ("DisplayCertificate", types);
			mi.Invoke (null, new object[2] { unit.x509, IntPtr.Zero });
		}


		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[Ignore ("UI would block tests")]
		public void LinkDemand_SelectFromCollection_Deny_UnmanagedCode ()
		{
			Type[] types = new Type[4] { typeof (X509Certificate2Collection), typeof (string), typeof (string), typeof (X509SelectionFlag) };
			MethodInfo mi = typeof (X509Certificate2UI).GetMethod ("SelectFromCollection", types);
			mi.Invoke (null, new object[4] { unit.coll, null, null, X509SelectionFlag.MultiSelection });
			// no LinkDemand on the SelectFromCollection(X509Certificate2Collection,string,string,X509SelectionFlag) method
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_SelectFromCollection_IntPtr_Deny_UnmanagedCode ()
		{
			Type[] types = new Type[5] { typeof (X509Certificate2Collection), typeof (string), typeof (string), typeof (X509SelectionFlag), typeof (IntPtr) };
			MethodInfo mi = typeof (X509Certificate2UI).GetMethod ("SelectFromCollection", types);
			mi.Invoke (null, new object[5] { unit.coll, null, null, X509SelectionFlag.MultiSelection, IntPtr.Zero });
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[UIPermission (SecurityAction.PermitOnly, Window = UIPermissionWindow.SafeTopLevelWindows)]
		[Ignore ("UI would block tests")]
		public void LinkDemand_SelectFromCollection_IntPtr_Permit ()
		{
			Type[] types = new Type[5] { typeof (X509Certificate2Collection), typeof (string), typeof (string), typeof (X509SelectionFlag), typeof (IntPtr) };
			MethodInfo mi = typeof (X509Certificate2UI).GetMethod ("SelectFromCollection", types);
			mi.Invoke (null, new object[5] { unit.coll, null, null, X509SelectionFlag.MultiSelection, IntPtr.Zero });
		}
	}
}
#endif
