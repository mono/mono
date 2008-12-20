// Test.cs created with MonoDevelop
// User: jeroen at 14:11 19-10-2008

using System;
using NUnit.Framework;

namespace LDAPConnectionRefusedNUnit
{
	[TestFixture()]
	public class Test
	{
		
		[Test()]
		[ExpectedException("Novell.Directory.Ldap.LdapException")]
		public void TestLDAPConnectionRefused()
		{
			Novell.Directory.Ldap.LdapConnection connection = new Novell.Directory.Ldap.LdapConnection();
			connection.Connect("localhost", 0);
		}
	}
}
