//
// ZoneTest.cs - NUnit Test Cases for Zone
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class ZoneTest : Assertion {

		[Test]
		public void MyComputer () 
		{
			Zone z = new Zone (SecurityZone.MyComputer);
			AssertEquals ("MyComputer.SecurityZone", SecurityZone.MyComputer, z.SecurityZone);
			Assert ("MyComputer.ToString", (z.ToString ().IndexOf ("<Zone>MyComputer</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("MyComputer.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("MyComputer.CreateIdentityPermission", p);

			Assert ("MyComputer.MyComputer.Equals", z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("MyComputer.Intranet.Equals", !z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("MyComputer.Trusted.Equals", !z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("MyComputer.Internet.Equals", !z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("MyComputer.Untrusted.Equals", !z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("MyComputer.NoZone.Equals", !z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("MyComputer.Null.Equals", !z.Equals (null));
		}

		[Test]
		public void Intranet () 
		{
			Zone z = new Zone (SecurityZone.Intranet);
			AssertEquals ("Intranet.SecurityZone", SecurityZone.Intranet, z.SecurityZone);
			Assert ("Intranet.ToString", (z.ToString ().IndexOf ("<Zone>Intranet</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("Intranet.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("Intranet.CreateIdentityPermission", p);

			Assert ("Intranet.MyComputer.Equals", !z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("Intranet.Intranet.Equals", z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("Intranet.Trusted.Equals", !z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("Intranet.Internet.Equals", !z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("Intranet.Untrusted.Equals", !z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("Intranet.NoZone.Equals", !z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("Intranet.Null.Equals", !z.Equals (null));
		}

		[Test]
		public void Trusted () 
		{
			Zone z = new Zone (SecurityZone.Trusted);
			AssertEquals ("Trusted.SecurityZone", SecurityZone.Trusted, z.SecurityZone);
			Assert ("Trusted.ToString", (z.ToString ().IndexOf ("<Zone>Trusted</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("Trusted.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("Trusted.CreateIdentityPermission", p);

			Assert ("Trusted.MyComputer.Equals", !z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("Trusted.Intranet.Equals", !z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("Trusted.Trusted.Equals", z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("Trusted.Internet.Equals", !z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("Trusted.Untrusted.Equals", !z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("Trusted.NoZone.Equals", !z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("Trusted.Null.Equals", !z.Equals (null));
		}

		[Test]
		public void Internet () 
		{
			Zone z = new Zone (SecurityZone.Internet);
			AssertEquals ("Internet.SecurityZone", SecurityZone.Internet, z.SecurityZone);
			Assert ("Internet.ToString", (z.ToString ().IndexOf ("<Zone>Internet</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("Internet.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("Internet.CreateIdentityPermission", p);

			Assert ("Internet.MyComputer.Equals", !z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("Internet.Intranet.Equals", !z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("Internet.Trusted.Equals", !z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("Internet.Internet.Equals", z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("Internet.Untrusted.Equals", !z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("Internet.NoZone.Equals", !z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("Internet.Null.Equals", !z.Equals (null));
		}

		[Test]
		public void Untrusted () 
		{
			Zone z = new Zone (SecurityZone.Untrusted);
			AssertEquals ("Untrusted.SecurityZone", SecurityZone.Untrusted, z.SecurityZone);
			Assert ("Untrusted.ToString", (z.ToString ().IndexOf ("<Zone>Untrusted</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("Untrusted.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("Untrusted.CreateIdentityPermission", p);

			Assert ("Untrusted.MyComputer.Equals", !z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("Untrusted.Intranet.Equals", !z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("Untrusted.Trusted.Equals", !z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("Untrusted.Internet.Equals", !z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("Untrusted.Untrusted.Equals", z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("Untrusted.NoZone.Equals", !z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("Untrusted.Null.Equals", !z.Equals (null));
		}

		[Test]
		public void NoZone () 
		{
			Zone z = new Zone (SecurityZone.NoZone);
			AssertEquals ("NoZone.SecurityZone", SecurityZone.NoZone, z.SecurityZone);
			Assert ("NoZone.ToString", (z.ToString ().IndexOf ("<Zone>NoZone</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("NoZone.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("NoZone.CreateIdentityPermission", p);

			Assert ("NoZone.MyComputer.Equals", !z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("NoZone.Intranet.Equals", !z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("NoZone.Trusted.Equals", !z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("NoZone.Internet.Equals", !z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("NoZone.Untrusted.Equals", !z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("NoZone.NoZone.Equals", z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("NoZone.Null.Equals", !z.Equals (null));
		}
	}
}
