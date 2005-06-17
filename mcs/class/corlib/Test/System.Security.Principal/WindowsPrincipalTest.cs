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
	public class WindowsPrincipalTest : Assertion {

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
			AssertNotNull ("Identity", p.Identity);

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
			AssertNotNull ("Identity", p.Identity);

			Assert ("Administrator", !p.IsInRole (WindowsBuiltInRole.Administrator));
			Assert ("BackupOperator", !p.IsInRole (WindowsBuiltInRole.BackupOperator));
			Assert ("Guest", !p.IsInRole (WindowsBuiltInRole.Guest));
			Assert ("PowerUser", !p.IsInRole (WindowsBuiltInRole.PowerUser));
			Assert ("Replicator", !p.IsInRole (WindowsBuiltInRole.Replicator));
			Assert ("User", !p.IsInRole (WindowsBuiltInRole.User));

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
			Assert ("IsInRole(Null)", !p.IsInRole ((string)null));
		}

		[Test]
		public void Interface () 
		{
			WindowsPrincipal wp = new WindowsPrincipal (WindowsIdentity.GetAnonymous ());

			IPrincipal p = (wp as IPrincipal);
			AssertNotNull ("IPrincipal", p);
		}
	}
}
