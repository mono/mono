// QueryRootDSETest.cs 
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
// 

using System;
using NUnit.Framework;
using Mono.Directory.LDAP;

namespace MonoTests.Directory.LDAP
{
	[TestFixture]
	public class QueryRootDSETest {

		[Test]
		public void Stuff() 
		{
			string myLDAPPath = "ldap://ldap.toshok.org";
			try {
				Mono.Directory.LDAP.LDAP ld = new Mono.Directory.LDAP.LDAP (myLDAPPath);
				LDAPMessage res, entry;
				string[] attrs = { "+", null };

				/* don't bind, we do this anonymously */

				ld.Search ("" /* root dse */,
					   SearchScope.Base,
					   "(objectclass=*)",
					   attrs, false,
					   TimeSpan.FromSeconds(10), 0 /* no size limit */,
					   out res);

				if (res == null) {
				  Console.WriteLine ("the search failed");
				}

				Console.WriteLine ("There are {0} entries", res.CountEntries());

				entry = res.FirstEntry();
				if (entry == null)
				  Console.WriteLine ("null returned from res.FirstEntry");

				string[] extensions = entry.GetValues ("supportedExtension");

				if (extensions != null) {
				  foreach( String e in extensions )
				    Console.WriteLine ("Supported Extension: {0}\n", e);
				}
				else {
					Console.WriteLine ("null returned from entry.GetValues\n");
				}
			}
			catch(Exception e) {
				Console.WriteLine("The '" + myLDAPPath + "' path not found.");
				Console.WriteLine("Exception : " + e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}
	}
}
