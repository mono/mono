//
// CredentialCacheTest.cs - NUnit Test Cases for System.Net.CredentialCache
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;
using System.Net;
using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Net
{

[TestFixture]
public class CredentialCacheTest
{
	[Test]        
        public void All ()
        {
		CredentialCache c = new CredentialCache ();
		
		NetworkCredential cred1 = new NetworkCredential ("user1", "pwd1");
		NetworkCredential cred2 = new NetworkCredential ("user2", "pwd2");
		NetworkCredential cred3 = new NetworkCredential ("user3", "pwd3");
		NetworkCredential cred4 = new NetworkCredential ("user4", "pwd4");
		NetworkCredential cred5 = new NetworkCredential ("user5", "pwd5");
		
		c.Add (new Uri ("http://www.ximian.com"), "Basic", cred1);
		c.Add (new Uri ("http://www.ximian.com"), "Kerberos", cred2);
		
		c.Add (new Uri ("http://www.contoso.com/portal/news/index.aspx"), "Basic", cred1);
		c.Add (new Uri ("http://www.contoso.com/portal/news/index.aspx?item=1"), "Basic", cred2);
		c.Add (new Uri ("http://www.contoso.com/portal/news/index.aspx?item=12"), "Basic", cred3);
		c.Add (new Uri ("http://www.contoso.com/portal/"), "Basic", cred4);
		c.Add (new Uri ("http://www.contoso.com"), "Basic", cred5);
		
		NetworkCredential result = null;
	
		try {
			c.Add (new Uri("http://www.ximian.com"), "Basic", cred1);
			Assertion.Fail ("#1: should have failed");
		} catch (ArgumentException) { }

		c.Add (new Uri("http://www.contoso.com/"), "**Unknown**", cred1);
		result = c.GetCredential (new Uri("http://www.contoso.com/"), "**Unknown**");
		Assertion.AssertEquals ("#3", result, cred1);
		c.Remove (new Uri("http://www.contoso.com/"), "**Unknown**");
		result = c.GetCredential (new Uri("http://www.contoso.com/"), "**Unknown**");
		Assertion.Assert ("#4", result == null);

		c.Add (new Uri("http://www.contoso.com/"), "**Unknown**", cred1);
		result = c.GetCredential (new Uri("http://www.contoso.com"), "**Unknown**");
		Assertion.AssertEquals ("#5", result, cred1);
		c.Remove (new Uri("http://www.contoso.com"), "**Unknown**");
		result = c.GetCredential (new Uri("http://www.contoso.com"), "**Unknown**");
		Assertion.Assert ("#6", result == null);

		c.Add (new Uri("http://www.contoso.com/portal/"), "**Unknown**", cred1);
		result = c.GetCredential (new Uri("http://www.contoso.com/portal/foo/bar.html"), "**Unknown**");
		Assertion.AssertEquals ("#7", result, cred1);
		c.Remove (new Uri("http://www.contoso.com"), "**Unknown**");
		result = c.GetCredential (new Uri("http://www.contoso.com"), "**Unknown**");
		Assertion.Assert ("#8", result == null);

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/news/index.aspx"), "Basic");
		Assertion.AssertEquals ("#9", result, cred3);

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/news/index"), "Basic");
		Assertion.AssertEquals ("#10", result, cred3);

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/news/"), "Basic");
		Assertion.AssertEquals ("#11", result, cred3);
		
		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/news"), "Basic");
		Assertion.AssertEquals ("#12", result, cred4);

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/ne"), "Basic");
		Assertion.AssertEquals ("#13", result, cred4);

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/"), "Basic");
		Assertion.AssertEquals ("#14", result, cred4);				

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal"), "Basic");
		Assertion.AssertEquals ("#15", result, cred5);

		result = c.GetCredential (new Uri("http://www.contoso.com:80/"), "Basic");
		Assertion.AssertEquals ("#16", result, cred5);

		result = c.GetCredential (new Uri("http://www.contoso.com"), "Basic");
		Assertion.AssertEquals ("#17", result, cred5);		

		/*		
		IEnumerator e = c.GetEnumerator ();
		while (e.MoveNext ()) {
			Console.WriteLine (e.Current.GetType () + " : " + e.Current.ToString ());
		}
		*/
#if NET_2_0
		result = c.GetCredential ("www.ximian.com", 80, "Basic");
		Assertion.Assert ("#18", result == null);		

		c.Add ("www.ximian.com", 80, "Basic", cred1);

		try {
			c.Add ("www.ximian.com", 80, "Basic", cred1);
			Assertion.Fail ("#19: should have failed");
		} catch (ArgumentException) { }

		result = c.GetCredential ("www.ximian.com", 80, "Basic");
		Assertion.AssertEquals ("#20", result, cred1);		

		c.Remove (new Uri("http://www.contoso.com"), "Basic");
		c.Add ("www.contoso.com", 80, "Basic", cred5);
		result = c.GetCredential (new Uri("http://www.contoso.com"), "Basic");
		Assertion.Assert ("#21", result == null);		
#endif
	}
}

}

