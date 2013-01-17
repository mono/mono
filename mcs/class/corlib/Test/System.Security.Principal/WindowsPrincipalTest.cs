//
// WindowsPrincipalTest.cs - NUnit Test Cases for WindowsPrincipal
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Runtime.Serialization;
using System.Security.Principal;

namespace MonoTests.System.Security.Principal {

	[TestFixture]
	public class WindowsPrincipalTest {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNull () 
		{
			WindowsPrincipal p = new WindowsPrincipal (null);
		}

		[Test]
		public void Current () 
		{
			WindowsPrincipal p = new WindowsPrincipal (WindowsIdentity.GetCurrent ());

			bool test;
			// we don't Assert as we don't know the current user roles
			test = p.IsInRole (WindowsBuiltInRole.Administrator);
			test = p.IsInRole (WindowsBuiltInRole.BackupOperator);
			test = p.IsInRole (WindowsBuiltInRole.Guest);
			test = p.IsInRole (WindowsBuiltInRole.PowerUser);
			test = p.IsInRole (WindowsBuiltInRole.Replicator);
			test = p.IsInRole (WindowsBuiltInRole.User);

			// doesn't work under XP in a workgroup (ArgumentException)
//			test = p.IsInRole (WindowsBuiltInRole.AccountOperator);
//			test = p.IsInRole (WindowsBuiltInRole.PrintOperator);
//			test = p.IsInRole (WindowsBuiltInRole.SystemOperator);
		}

		[Test]
		public void Anonymous () 
		{
			WindowsPrincipal p = new WindowsPrincipal (WindowsIdentity.GetAnonymous ());

			Assert.IsFalse (p.IsInRole (WindowsBuiltInRole.Administrator), "Administrator");
			Assert.IsFalse (p.IsInRole (WindowsBuiltInRole.BackupOperator), "BackupOperator");
			Assert.IsFalse (p.IsInRole (WindowsBuiltInRole.Guest), "Guest");
			Assert.IsFalse (p.IsInRole (WindowsBuiltInRole.PowerUser), "PowerUser");
			Assert.IsFalse (p.IsInRole (WindowsBuiltInRole.Replicator), "Replicator");
			Assert.IsFalse (p.IsInRole (WindowsBuiltInRole.User), "User");

			// doesn't work under XP in a workgroup (ArgumentException)
//			Assert ("AccountOperator", !p.IsInRole (WindowsBuiltInRole.AccountOperator));
//			Assert ("PrintOperator", !p.IsInRole (WindowsBuiltInRole.PrintOperator));
//			Assert ("SystemOperator", !p.IsInRole (WindowsBuiltInRole.SystemOperator));
		}

		[Test]
		//[ExpectedException (typeof (ArgumentNullException))]
		public void IsInRole_Null ()
		{
			WindowsPrincipal p = new WindowsPrincipal (WindowsIdentity.GetAnonymous ());
			Assert.IsFalse (p.IsInRole ((string)null));
		}

		[Test]
		public void Interface () 
		{
			WindowsPrincipal wp = new WindowsPrincipal (WindowsIdentity.GetAnonymous ());

			IPrincipal p = (wp as IPrincipal);
			Assert.IsNotNull (p);
		}
	}
}
