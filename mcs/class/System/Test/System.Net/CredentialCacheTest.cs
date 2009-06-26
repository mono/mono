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
			Assert.Fail ("#1: should have failed");
		} catch (ArgumentException) { }

		c.Add (new Uri("http://www.contoso.com/"), "**Unknown**", cred1);
		result = c.GetCredential (new Uri("http://www.contoso.com/"), "**Unknown**");
		Assert.AreEqual (result, cred1, "#3");
		c.Remove (new Uri("http://www.contoso.com/"), "**Unknown**");
		result = c.GetCredential (new Uri("http://www.contoso.com/"), "**Unknown**");
		Assert.IsTrue (result == null, "#4");

		c.Add (new Uri("http://www.contoso.com/"), "**Unknown**", cred1);
		result = c.GetCredential (new Uri("http://www.contoso.com"), "**Unknown**");
		Assert.AreEqual (result, cred1, "#5");
		c.Remove (new Uri("http://www.contoso.com"), "**Unknown**");
		result = c.GetCredential (new Uri("http://www.contoso.com"), "**Unknown**");
		Assert.IsTrue (result == null, "#6");

		c.Add (new Uri("http://www.contoso.com/portal/"), "**Unknown**", cred1);
		result = c.GetCredential (new Uri("http://www.contoso.com/portal/foo/bar.html"), "**Unknown**");
		Assert.AreEqual (result, cred1, "#7");
		c.Remove (new Uri("http://www.contoso.com"), "**Unknown**");
		result = c.GetCredential (new Uri("http://www.contoso.com"), "**Unknown**");
		Assert.IsTrue (result == null, "#8");

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/news/index.aspx"), "Basic");
		Assert.AreEqual (result, cred3, "#9");

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/news/index"), "Basic");
		Assert.AreEqual (result, cred3, "#10");

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/news/"), "Basic");
		Assert.AreEqual (result, cred3, "#11");
		
		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/news"), "Basic");
		Assert.AreEqual (result, cred4, "#12");

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/ne"), "Basic");
		Assert.AreEqual (result, cred4, "#13");

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal/"), "Basic");
		Assert.AreEqual (result, cred4, "#14");				

		result = c.GetCredential (new Uri("http://www.contoso.com:80/portal"), "Basic");
		Assert.AreEqual (result, cred5, "#15");

		result = c.GetCredential (new Uri("http://www.contoso.com:80/"), "Basic");
		Assert.AreEqual (result, cred5, "#16");

		result = c.GetCredential (new Uri("http://www.contoso.com"), "Basic");
		Assert.AreEqual (result, cred5, "#17");		

		/*		
		IEnumerator e = c.GetEnumerator ();
		while (e.MoveNext ()) {
			Console.WriteLine (e.Current.GetType () + " : " + e.Current.ToString ());
		}
		*/
#if NET_2_0
		result = c.GetCredential ("www.ximian.com", 80, "Basic");
		Assert.IsTrue (result == null, "#18");		

		c.Add ("www.ximian.com", 80, "Basic", cred1);

		try {
			c.Add ("www.ximian.com", 80, "Basic", cred1);
			Assert.Fail ("#19: should have failed");
		} catch (ArgumentException) { }

		result = c.GetCredential ("www.ximian.com", 80, "Basic");
		Assert.AreEqual (result, cred1, "#20");		

		c.Remove (new Uri("http://www.contoso.com"), "Basic");
		c.Add ("www.contoso.com", 80, "Basic", cred5);
		result = c.GetCredential (new Uri("http://www.contoso.com"), "Basic");
		Assert.IsTrue (result == null, "#21");		
#endif
	}
}

}

