// BindSimpleTest.cs 
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using Mono.Directory.LDAP;

namespace MonoTests.Directory.LDAP
{
	[TestFixture]
	public class BindSimpleTest {

		[SetUp]
		public void GetReady () {}

		[TearDown]
		public void Clear () {}

		[Test]
		public void Stuff() 
		{
			string myLDAPPath = "ldap://ldap.toshok.org";
			string username = "cn=Manager,dc=toshok,dc=org", passwd = "evotest";
			try {
				LDAP ld = new LDAP (myLDAPPath);

				ld.BindSimple (username, passwd);

				Console.WriteLine("Successfully bound {0} at {1}", username, myLDAPPath);
			}
			catch(Exception e) {
				Console.WriteLine("The '" + myLDAPPath + "' path not found.");
				Console.WriteLine("Exception : " + e.Message);
			}
		}
	}
}
