
using NUnit.Framework;
using System;
using Mono.Directory.LDAP;

namespace MonoTests.Directory.LDAP
{
	public class BindSimpleTest : TestCase {
		public BindSimpleTest () :
			base ("[MonoTests.Directory.LDAP.BindSimpleTest]'") {}

		public BindSimpleTest (string name) :
			base (name) {}

		protected override void SetUp () {}

		protected override void TearDown () {}

		public static ITest Suite
		{
			get {
				return new TestSuite (typeof (BindSimpleTest));
			}
		}
		

		public void TestStuff() {
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
