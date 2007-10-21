//
// WebProxyTest.cs - NUnit Test Cases for System.Net.WebProxy
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class WebProxyTest
	{
		[Test]
		public void Constructors ()
		{
			WebProxy p = new WebProxy ();
			Assertion.Assert ("#1", p.Address == null);
			Assertion.AssertEquals ("#2", 0, p.BypassArrayList.Count);
			Assertion.AssertEquals ("#3", 0, p.BypassList.Length);
			Assertion.AssertEquals ("#4", false, p.BypassProxyOnLocal);
			try {
				p.BypassList = null;
				Assertion.Fail ("#5 not spec'd, but should follow ms.net implementation");
			} catch (ArgumentNullException) { }

			p = new WebProxy ("webserver.com", 8080);
			Assertion.AssertEquals ("#6", new Uri ("http://webserver.com:8080/"), p.Address);

			p = new WebProxy ("webserver");
			Assertion.AssertEquals ("#7", new Uri ("http://webserver"), p.Address);

			p = new WebProxy ("webserver.com");
			Assertion.AssertEquals ("#8", new Uri ("http://webserver.com"), p.Address);

			p = new WebProxy ("http://webserver.com");
			Assertion.AssertEquals ("#9", new Uri ("http://webserver.com"), p.Address);

			p = new WebProxy ("file://webserver");
			Assertion.AssertEquals ("#10", new Uri ("file://webserver"), p.Address);

			p = new WebProxy ("http://www.contoso.com", true, null, null);
			Assertion.AssertEquals ("#11", 0, p.BypassList.Length);
			Assertion.AssertEquals ("#12", 0, p.BypassArrayList.Count);

			try {
				p = new WebProxy ("http://contoso.com", true,
					new string [] { "?^!@#$%^&}{][" }, null);
				Assertion.Fail ("#13: illegal regular expression");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void BypassArrayList ()
		{
			Uri proxy1 = new Uri ("http://proxy.contoso.com");
			Uri proxy2 = new Uri ("http://proxy2.contoso.com");

			WebProxy p = new WebProxy (proxy1, true);
			p.BypassArrayList.Add ("http://proxy2.contoso.com");
			p.BypassArrayList.Add ("http://proxy2.contoso.com");
			Assertion.AssertEquals ("#1", 2, p.BypassList.Length);
			Assertion.Assert ("#2", !p.IsBypassed (new Uri ("http://www.google.com")));
			Assertion.Assert ("#3", p.IsBypassed (proxy2));
			Assertion.AssertEquals ("#4", proxy2, p.GetProxy (proxy2));

			p.BypassArrayList.Add ("?^!@#$%^&}{][");
			Assertion.AssertEquals ("#10", 3, p.BypassList.Length);
			try {
				Assertion.Assert ("#11", !p.IsBypassed (proxy2));
				Assertion.Assert ("#12", !p.IsBypassed (new Uri ("http://www.x.com")));
				Assertion.AssertEquals ("#13", proxy1, p.GetProxy (proxy2));
				// hmm... although #11 and #13 succeeded before (#3 resp. #4), 
				// it now fails to bypass, and the IsByPassed and GetProxy 
				// methods do not fail.. so when an illegal regular 
				// expression is added through this property it's ignored. 
				// probably an ms.net bug?? :(
			} catch (ArgumentException) {
				Assertion.Fail ("#15: illegal regular expression");
			}
		}

		[Test]
		public void BypassList ()
		{
			Uri proxy1 = new Uri ("http://proxy.contoso.com");
			Uri proxy2 = new Uri ("http://proxy2.contoso.com");

			WebProxy p = new WebProxy (proxy1, true);
			try {
				p.BypassList = new string [] { "http://proxy2.contoso.com", "?^!@#$%^&}{][" };
				Assertion.Fail ("#1");
			} catch (ArgumentException) {
				// weird, this way invalid regex's fail again..
			}

			Assertion.AssertEquals ("#2", 2, p.BypassList.Length);
			// but it did apparenly store the regex's !

			p.BypassList = new string [] { "http://www.x.com" };
			Assertion.AssertEquals ("#3", 1, p.BypassList.Length);

			try {
				p.BypassList = null;
				Assertion.Fail ("#4");
			} catch (ArgumentNullException) { }

			Assertion.AssertEquals ("#4", 1, p.BypassList.Length);
		}

		[Test]
		public void GetProxy ()
		{
		}

		[Test]
		public void IsByPassed ()
		{
			WebProxy p = new WebProxy ("http://proxy.contoso.com", true);
			Assertion.Assert ("#1", !p.IsBypassed (new Uri ("http://www.google.com")));
			Assertion.Assert ("#2", p.IsBypassed (new Uri ("http://localhost/index.html")));
			Assertion.Assert ("#3", p.IsBypassed (new Uri ("http://localhost:8080/index.html")));
			Assertion.Assert ("#4", p.IsBypassed (new Uri ("http://loopback:8080/index.html")));
			Assertion.Assert ("#5", p.IsBypassed (new Uri ("http://127.0.0.01:8080/index.html")));
			Assertion.Assert ("#6", p.IsBypassed (new Uri ("http://webserver/index.html")));
			Assertion.Assert ("#7", !p.IsBypassed (new Uri ("http://webserver.com/index.html")));

			p = new WebProxy ("http://proxy.contoso.com", false);
			Assertion.Assert ("#11", !p.IsBypassed (new Uri ("http://www.google.com")));
			Assertion.Assert ("#12: lamespec of ms.net", p.IsBypassed (new Uri ("http://localhost/index.html")));
			Assertion.Assert ("#13: lamespec of ms.net", p.IsBypassed (new Uri ("http://localhost:8080/index.html")));
			Assertion.Assert ("#14: lamespec of ms.net", p.IsBypassed (new Uri ("http://loopback:8080/index.html")));
			Assertion.Assert ("#15: lamespec of ms.net", p.IsBypassed (new Uri ("http://127.0.0.01:8080/index.html")));
			Assertion.Assert ("#16", !p.IsBypassed (new Uri ("http://webserver/index.html")));

			p.BypassList = new string [] { "google.com", "contoso.com" };
			Assertion.Assert ("#20", p.IsBypassed (new Uri ("http://www.google.com")));
			Assertion.Assert ("#21", p.IsBypassed (new Uri ("http://www.GOOGLE.com")));
			Assertion.Assert ("#22", p.IsBypassed (new Uri ("http://www.contoso.com:8080/foo/bar/index.html")));
			Assertion.Assert ("#23", !p.IsBypassed (new Uri ("http://www.contoso2.com:8080/foo/bar/index.html")));
			Assertion.Assert ("#24", !p.IsBypassed (new Uri ("http://www.foo.com:8080/contoso.com.html")));

			p.BypassList = new string [] { "https" };
			Assertion.Assert ("#30", !p.IsBypassed (new Uri ("http://www.google.com")));
			Assertion.Assert ("#31", p.IsBypassed (new Uri ("https://www.google.com")));
		}

		[Test]
		public void IsByPassed_Address_Null ()
		{
			WebProxy p = new WebProxy ((Uri) null, false);
			Assertion.Assert ("#1", p.IsBypassed (new Uri ("http://www.google.com")));

			p = new WebProxy ((Uri) null, true);
			Assertion.Assert ("#2", p.IsBypassed (new Uri ("http://www.google.com")));
		}

		[Test]
#if TARGET_JVM
		[Ignore ("TD BUG ID: 7213")]
#endif		
		public void IsByPassed_Host_Null ()
		{
			WebProxy p = new WebProxy ("http://proxy.contoso.com", true);
			try {
				p.IsBypassed (null);
				Assertion.Fail ("#A1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNotNull (ex.ParamName, "#A4");
				Assert.AreEqual ("host", ex.ParamName, "#A5");
				Assert.IsNull (ex.InnerException, "#A6");
			}
#else
			} catch (NullReferenceException) {
			}
#endif

			p = new WebProxy ((Uri) null);
			try {
				p.IsBypassed (null);
				Assertion.Fail ("#B1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("host", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}
#else
			} catch (NullReferenceException) {
			}
#endif

			p = new WebProxy ((Uri) null, true);
			try {
				p.IsBypassed (null);
				Assertion.Fail ("#C1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsNotNull (ex.ParamName, "#C4");
				Assert.AreEqual ("host", ex.ParamName, "#C5");
				Assert.IsNull (ex.InnerException, "#C6");
			}
#else
			} catch (NullReferenceException) {
			}
#endif
		}

		[Test]
#if TARGET_JVM
		[Ignore ("The MS compliant binary serialization is not supported")]
#endif		
		public void GetObjectData ()
		{
			SerializationInfo si = new SerializationInfo (typeof (WebHeaderCollection),
				new FormatterConverter ());

			WebProxy proxy = new WebProxy ("proxy.ximian.com");
			((ISerializable) proxy).GetObjectData (si, new StreamingContext ());
#if NET_2_0
			Assert.AreEqual (4, si.MemberCount, "#A1");
#else
			Assert.AreEqual (3, si.MemberCount, "#A1");
#endif
			int i = 0;
			foreach (SerializationEntry entry in si) {
				Assert.IsNotNull (entry.Name, "#A2:" + i);
				Assert.IsNotNull (entry.ObjectType, "#A3:" + i);

				switch (i) {
				case 0:
					Assert.AreEqual ("_BypassOnLocal", entry.Name, "#A4:" + i);
					Assert.AreEqual (typeof (bool), entry.ObjectType, "#A5:" + i);
					Assert.IsNotNull (entry.Value, "#A6:" + i);
					Assert.AreEqual (false, entry.Value, "#A7:" + i);
					break;
				case 1:
					Assert.AreEqual ("_ProxyAddress", entry.Name, "#A4:" + i);
					Assert.AreEqual (typeof (Uri), entry.ObjectType, "#A5:" + i);
					Assert.IsNotNull (entry.Value, "#A6:" + i);
					break;
				case 2:
					Assert.AreEqual ("_BypassList", entry.Name, "#A4:" + i);
					Assert.AreEqual (typeof (object), entry.ObjectType, "#A5:" + i);
					Assert.IsNull (entry.Value, "#A6:" + i);
					break;
#if NET_2_0
				case 3:
					Assert.AreEqual ("_UseDefaultCredentials", entry.Name, "#A4:" + i);
					Assert.AreEqual (typeof (bool), entry.ObjectType, "#A5:" + i);
					Assert.IsNotNull (entry.Value, "#A6:" + i);
					Assert.AreEqual (false, entry.Value, "#A7:" + i);
					break;
#endif
				}
				i++;
			}
		}
	}
}
