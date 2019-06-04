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
			Assert.IsTrue (p.Address == null, "#1");
			Assert.AreEqual (0, p.BypassArrayList.Count, "#2");
			Assert.AreEqual (0, p.BypassList.Length, "#3");
			Assert.AreEqual (false, p.BypassProxyOnLocal, "#4");
			try {
				p.BypassList = null;
				Assert.Fail ("#5 not spec'd, but should follow ms.net implementation");
			} catch (ArgumentNullException) { }

			p = new WebProxy ("webserver.com", 8080);
			Assert.AreEqual (new Uri ("http://webserver.com:8080/"), p.Address, "#6");

			p = new WebProxy ("webserver");
			Assert.AreEqual (new Uri ("http://webserver"), p.Address, "#7");

			p = new WebProxy ("webserver.com");
			Assert.AreEqual (new Uri ("http://webserver.com"), p.Address, "#8");

			p = new WebProxy ("http://webserver.com");
			Assert.AreEqual (new Uri ("http://webserver.com"), p.Address, "#9");

			p = new WebProxy ("file://webserver");
			Assert.AreEqual (new Uri ("file://webserver"), p.Address, "#10");

			p = new WebProxy ("http://www.contoso.com", true, null, null);
			Assert.AreEqual (0, p.BypassList.Length, "#11");
			Assert.AreEqual (0, p.BypassArrayList.Count, "#12");

			try {
				p = new WebProxy ("http://contoso.com", true,
					new string [] { "?^!@#$%^&}{][" }, null);
				Assert.Fail ("#13: illegal regular expression");
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
			Assert.AreEqual (2, p.BypassList.Length, "#1");
			Assert.IsTrue (!p.IsBypassed (new Uri ("http://www.example.com")), "#2");
			Assert.IsTrue (p.IsBypassed (proxy2), "#3");
			Assert.AreEqual (proxy2, p.GetProxy (proxy2), "#4");

			p.BypassArrayList.Add ("?^!@#$%^&}{][");
			Assert.AreEqual (3, p.BypassList.Length, "#10");
			try {
				Assert.IsTrue (!p.IsBypassed (proxy2), "#11");
				Assert.IsTrue (!p.IsBypassed (new Uri ("http://www.x.com")), "#12");
				Assert.AreEqual (proxy1, p.GetProxy (proxy2), "#13");
				// hmm... although #11 and #13 succeeded before (#3 resp. #4), 
				// it now fails to bypass, and the IsByPassed and GetProxy 
				// methods do not fail.. so when an illegal regular 
				// expression is added through this property it's ignored. 
				// probably an ms.net bug?? :(
			} catch (ArgumentException) {
				Assert.Fail ("#15: illegal regular expression");
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
				Assert.Fail ("#1");
			} catch (ArgumentException) {
				// weird, this way invalid regex's fail again..
			}

			Assert.AreEqual (2, p.BypassList.Length, "#2");
			// but it did apparenly store the regex's !

			p.BypassList = new string [] { "http://www.x.com" };
			Assert.AreEqual (1, p.BypassList.Length, "#3");

			try {
				p.BypassList = null;
				Assert.Fail ("#4");
			} catch (ArgumentNullException) { }

			Assert.AreEqual (1, p.BypassList.Length, "#4");
		}

		[Test]
		public void IsByPassed ()
		{
			WebProxy p = new WebProxy ("http://proxy.contoso.com", true);
			Assert.IsTrue (!p.IsBypassed (new Uri ("http://www.example.com")), "#1");
			Assert.IsTrue (p.IsBypassed (new Uri ("http://localhost/index.html")), "#2");
			Assert.IsTrue (p.IsBypassed (new Uri ("http://localhost:8080/index.html")), "#3");
			Assert.IsTrue (p.IsBypassed (new Uri ("http://loopback:8080/index.html")), "#4");
			Assert.IsTrue (p.IsBypassed (new Uri ("http://127.0.0.01:8080/index.html")), "#5");
			Assert.IsTrue (p.IsBypassed (new Uri ("http://webserver/index.html")), "#6");
			Assert.IsTrue (!p.IsBypassed (new Uri ("http://webserver.com/index.html")), "#7");

			p = new WebProxy ("http://proxy.contoso.com", false);
			Assert.IsTrue (!p.IsBypassed (new Uri ("http://www.example.com")), "#11");
			Assert.IsTrue (p.IsBypassed (new Uri ("http://localhost/index.html")), "#12: lamespec of ms.net");
			Assert.IsTrue (p.IsBypassed (new Uri ("http://localhost:8080/index.html")), "#13: lamespec of ms.net");
			Assert.IsTrue (p.IsBypassed (new Uri ("http://loopback:8080/index.html")), "#14: lamespec of ms.net");
			Assert.IsTrue (p.IsBypassed (new Uri ("http://127.0.0.01:8080/index.html")), "#15: lamespec of ms.net");
			Assert.IsTrue (!p.IsBypassed (new Uri ("http://webserver/index.html")), "#16");

			p.BypassList = new string [] { "example.com", "contoso.com" };
			Assert.IsTrue (p.IsBypassed (new Uri ("http://www.example.com")), "#20");
			Assert.IsTrue (p.IsBypassed (new Uri ("http://www.EXAMPLE.com")), "#21");
			Assert.IsTrue (p.IsBypassed (new Uri ("http://www.contoso.com:8080/foo/bar/index.html")), "#22");
			Assert.IsTrue (!p.IsBypassed (new Uri ("http://www.contoso2.com:8080/foo/bar/index.html")), "#23");
			Assert.IsTrue (!p.IsBypassed (new Uri ("http://www.foo.com:8080/contoso.com.html")), "#24");

			p.BypassList = new string [] { "https" };
			Assert.IsTrue (!p.IsBypassed (new Uri ("http://www.example.com")), "#30");
			Assert.IsTrue (p.IsBypassed (new Uri ("https://www.example.com")), "#31");
		}

		[Test]
		public void IsByPassed_Address_Null ()
		{
			WebProxy p = new WebProxy ((Uri) null, false);
			Assert.IsTrue (p.IsBypassed (new Uri ("http://www.example.com")), "#1");

			p = new WebProxy ((Uri) null, true);
			Assert.IsTrue (p.IsBypassed (new Uri ("http://www.example.com")), "#2");
		}

		[Test]
		public void IsByPassed_Host_Null ()
		{
			WebProxy p = new WebProxy ("http://proxy.contoso.com", true);
			try {
				p.IsBypassed (null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNotNull (ex.ParamName, "#A4");
				Assert.AreEqual ("host", ex.ParamName, "#A5");
				Assert.IsNull (ex.InnerException, "#A6");
			}

			p = new WebProxy ((Uri) null);
			try {
				p.IsBypassed (null);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("host", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}

			p = new WebProxy ((Uri) null, true);
			try {
				p.IsBypassed (null);
				Assert.Fail ("#C1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsNotNull (ex.ParamName, "#C4");
				Assert.AreEqual ("host", ex.ParamName, "#C5");
				Assert.IsNull (ex.InnerException, "#C6");
			}
		}

		[Test]
		public void GetObjectData ()
		{
			SerializationInfo si = new SerializationInfo (typeof (WebHeaderCollection),
				new FormatterConverter ());

			WebProxy proxy = new WebProxy ("proxy.ximian.com");
			((ISerializable) proxy).GetObjectData (si, new StreamingContext ());
			Assert.AreEqual (4, si.MemberCount, "#A1");
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
				case 3:
					Assert.AreEqual ("_UseDefaultCredentials", entry.Name, "#A4:" + i);
					Assert.AreEqual (typeof (bool), entry.ObjectType, "#A5:" + i);
					Assert.IsNotNull (entry.Value, "#A6:" + i);
					Assert.AreEqual (false, entry.Value, "#A7:" + i);
					break;
				}
				i++;
			}
		}
	}
}
