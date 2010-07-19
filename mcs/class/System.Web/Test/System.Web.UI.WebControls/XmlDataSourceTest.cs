//
// Tests for System.Web.UI.WebControls.XmlDataSource.cs 
//
// Author:
//	Chris Toshok (toshok@novell.com)
//      Yoni Klain   (yonik@mainsoft.com)   
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// NOTE: INCLUDE 2 CATEGORIES [Category ("NunitWeb")];[Category ("NotWorking")]
		

#if NET_2_0


using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Threading;

namespace MonoTests.System.Web.UI.WebControls
{
	class DSPoker : XmlDataSource
	{
		public DSPoker () {
			TrackViewState ();
		}

		public object SaveState () {
			return SaveViewState ();
		}
		public void LoadState (object o) {
			LoadViewState (o);
		}

		public HierarchicalDataSourceView  DoGetHierarchicalView(string viewPath)
		{
			 return base.GetHierarchicalView(viewPath);
		}

		public void DoOnTransforming (EventArgs e)
		{
			base.OnTransforming (e);
		}

		public void DoOnDataSourceChanged ()
		{
			base.OnDataSourceChanged (new EventArgs ());
		}
	}

	[TestFixture]
	public class XmlDataSourceTest
	{
		string data = @"<?xml version=""1.0"" encoding=""utf-8""?><IranHistoricalPlaces name=""places""><Place name=""Taghe Bostan""><City>Kermanshah</City><Antiquity>2000</Antiquity></Place><Place name=""Persepolis""><City>Shiraz</City><Antiquity>2500</Antiquity></Place></IranHistoricalPlaces>";

		[TestFixtureSetUp]
		public void CopyTestResources ()
		{
			WebTest.CopyResource (GetType (), "XMLDataSourceTest.xml", "XMLDataSourceTest.xml");
			WebTest.CopyResource (GetType (), "XMLDataSourceTest.xsl", "XMLDataSourceTest.xsl");
			WebTest.CopyResource (GetType (), "XMLDataSourceTest1.aspx", "XMLDataSourceTest1.aspx");
			WebTest.CopyResource (GetType (), "XMLDataSourceTest2.aspx", "XMLDataSourceTest2.aspx");
			WebTest.CopyResource (GetType (), "XMLDataSourceTest3.aspx", "XMLDataSourceTest3.aspx");
			WebTest.CopyResource (GetType (), "XMLDataSourceTest4.aspx", "XMLDataSourceTest4.aspx");
		}

		[Test]
		public void Defaults ()
		{
			DSPoker p = new DSPoker ();

			Assert.AreEqual ("", p.Data, "A4");
			Assert.AreEqual ("", p.DataFile, "A5");
			Assert.AreEqual ("", p.Transform, "A9");
			Assert.AreEqual ("", p.TransformFile, "A10");
			Assert.AreEqual ("", p.XPath, "A11");
			
			// Added
			Assert.AreEqual (null, p.TransformArgumentList, "A17");
		}

		[Test]
		public void Defaults_NotWorking ()
		{
			DSPoker p = new DSPoker ();
			Assert.AreEqual (0, p.CacheDuration, "A12");
			Assert.AreEqual (DataSourceCacheExpiry.Absolute, p.CacheExpirationPolicy, "A13");
			Assert.AreEqual ("", p.CacheKeyDependency, "A14");
			Assert.AreEqual (true, p.EnableCaching, "A15");
		}

		[Test]
		public void Attributes ()
		{
			DSPoker p = new DSPoker ();

			p.Data = data;
			Assert.AreEqual (data, p.Data, "A1");

			p.Transform = "transform";
			Assert.AreEqual ("transform", p.Transform, "A2");

			p.XPath = "xpath";
			Assert.AreEqual ("xpath", p.XPath, "A3");
		}

		[Test]
		public void ViewState ()
		{
			// XXX weird... something odd going on with
			// ViewState?  or are none of these stored?
			DSPoker p = new DSPoker ();

			p.Data = data;
			p.Transform = "transform";
			p.XPath = "xpath";

			object state = p.SaveState ();
			DSPoker copy = new DSPoker ();
			copy.LoadState (state);
			Assert.AreEqual ("", copy.Data, "A1");
			Assert.AreEqual ("", copy.Transform, "A2");
			Assert.AreEqual ("", copy.XPath, "A3");

			p = new DSPoker ();
			p.DataFile = "DataFile";
			p.TransformFile = "TransformFile";

			state = p.SaveState ();
			copy = new DSPoker ();

			copy.LoadState (state);
			Assert.AreEqual ("", copy.DataFile, "A1");
			Assert.AreEqual ("", copy.TransformFile, "A2");
		}

		#region help_results
		class eventAssert
		{
			private static int _testcounter;
			private static bool _eventChecker;
			private eventAssert ()
			{
				_testcounter = 0;
			}

			public static bool eventChecker
			{
				get
				{
					throw new NotImplementedException ();
				}
				set
				{
					_eventChecker = value;
				}
			}

			static private void testAdded ()
			{
				_testcounter++;
				_eventChecker = false;
			}

			public static void IsTrue (string msg)
			{
				Assert.IsTrue (_eventChecker, msg + "#" + _testcounter);
				testAdded ();

			}

			public static void IsFalse (string msg)
			{
				Assert.IsFalse (_eventChecker, msg + "#" + _testcounter);
				testAdded ();
			}
		}
		#endregion

		[Test]
		public void XmlDataSource_DataSourceViewChanged ()
		{
			DSPoker p = new DSPoker ();
			((IDataSource) p).DataSourceChanged += new EventHandler (XmlDataSourceTest_DataSourceChanged);
			p.DoOnDataSourceChanged ();
			eventAssert.IsTrue ("XmlDataSource"); // Assert include counter the first is zero

			p.Data = data;
			eventAssert.IsTrue ("XmlDataSource");
			p.Transform = "transform";
			eventAssert.IsTrue ("XmlDataSource");
			p.XPath = "xpath";
			eventAssert.IsTrue ("XmlDataSource");
			p.DataFile = "DataFile";
			eventAssert.IsTrue ("XmlDataSource");
			p.TransformFile = "TransformFile";
			eventAssert.IsTrue ("XmlDataSource");
		}

		void XmlDataSourceTest_DataSourceChanged (object sender, EventArgs e)
		{
			eventAssert.eventChecker = true;
			
		}

		[Test]
		[Category ("NunitWeb")]
		public void DataFile ()
		{
			new WebTest (PageInvoker.CreateOnLoad (datafile)).Run ();
		}

		public static void datafile (Page p)
		{
			string originalxml = @"<?xml version=""1.0"" encoding=""utf-8""?><bookstore xmlns:bk=""urn:samples""><book genre=""novel"" publicationdate=""1999"" bk:ISBN=""0192100262""><title>Pride and Prejudice</title><author><first-name>Jane</first-name><last-name>Austen</last-name></author><price>24.95</price>""
			</book><book genre=""novel"" publicationdate=""1985"" bk:ISBN=""0771008139""><title>The Handmaid's Tale</title><author><first-name>Margaret</first-name><last-name>Atwood</last-name></author><price>29.95</price></book></bookstore>";

			XmlDataSource ds = new XmlDataSource ();
			p.Form.Controls.Add (ds);
			ds.DataFile = "~/XMLDataSourceTest.xml";
			ds.DataBind ();
			string derivedxml = ((XmlDocument) ds.GetXmlDocument ()).InnerXml;
			HtmlDiff.AssertAreEqual (originalxml, derivedxml, "Loading xml");
		}

		[Test]
		public void GetXmlDocument ()
		{
			DSPoker p = new DSPoker ();
			p.Data = data;
			XmlDocument doc = p.GetXmlDocument ();
			HtmlDiff.AssertAreEqual (data, doc.InnerXml, "GetXmlDocument");
		}

		[Test]
		[Category ("NunitWeb")]
		public void XPath ()
		{
			Page page = new Page ();
			XmlDataSource ds = new XmlDataSource ();
			ds.ID = "ds";
			ds.Data = @"<?xml version=""1.0"" encoding=""utf-8""?>
					<bookstore xmlns:bk=""urn:samples"">
					  <book genre=""novel"" publicationdate=""1999"" bk:ISBN=""0192100262"">
					    <title>Pride and Prejudice</title>
					    <author>
					      <first-name>Jane</first-name>
					      <last-name>Austen</last-name>
					    </author>
					    <price>24.95</price>""
					  </book>
					  <book genre=""novel"" publicationdate=""1985"" bk:ISBN=""0771008139"">
					    <title>The Handmaid's Tale</title>
					    <author>
					      <first-name>Margaret</first-name>
					      <last-name>Atwood</last-name>
					    </author>
					    <price>29.95</price>
					  </book>
					</bookstore>";
			DataList list0 = new DataList ();
			DataList list1 = new DataList ();
			DataList list2 = new DataList ();
			page.Controls.Add (list0);
			page.Controls.Add (list1);
			page.Controls.Add (list2);
			page.Controls.Add (ds);
			list0.DataSourceID = "ds";
			list0.DataBind ();
			Assert.AreEqual (2, list0.Items.Count, "Before XPath elements");

			ds.XPath = "/bookstore/book [title='Pride and Prejudice']";
			list1.DataSourceID = "ds";
			list1.DataBind ();
			Assert.AreEqual (1, list1.Items.Count, "After XPath elements");

			ds.XPath = "bookstore/book [@genre='novel']";
			list2.DataSourceID = "ds";
			list2.DataBind ();
			Assert.AreEqual (2, list2.Items.Count, "After XPath property");
		}

		[Test]
		public void GetHierarchicalView ()
		{
			Page page = new Page ();
			DSPoker ds = new DSPoker ();
			ds.ID = "ds";
			ds.Data = @"<?xml version=""1.0"" encoding=""utf-8""?>
					<bookstore xmlns:bk=""urn:samples"">
					  <book genre=""novel"" publicationdate=""1999"" bk:ISBN=""0192100262"">
					    <title>Pride and Prejudice</title>
					    <author>
					      <first-name>Jane</first-name>
					      <last-name>Austen</last-name>
					    </author>
					    <price>24.95</price>
					  </book>
					  <book genre=""novel"" publicationdate=""1985"" bk:ISBN=""0771008139"">
					    <title>The Handmaid's Tale</title>
					    <author>
					      <first-name>Margaret</first-name>
					      <last-name>Atwood</last-name>
					    </author>
					    <price>29.95</price>
					  </book>
					</bookstore>";
			HierarchicalDataSourceView view = ds.DoGetHierarchicalView ("");
			IHierarchicalEnumerable num = view.Select ();
			foreach (object obj in num) {
				IHierarchyData hdata = num.GetHierarchyData (obj);
				XmlElement element = (XmlElement) hdata.Item;
				Assert.AreEqual ("bookstore", element.Name, "RootElementName");
				Assert.AreEqual ("Pride and PrejudiceJaneAusten24.95The Handmaid's TaleMargaretAtwood29.95", element.InnerText, "InnerText");
				Assert.AreEqual (2, element.ChildNodes.Count, "ChildElementNodes");
			}
		}

		[Test]
		[Category ("NunitWeb")]
		public void Transform ()
		{
			string origin = @"<div>
						<h2>Order</h2><hr>
						<table>
						  <tr>
						    <td>Customer</td>
						    <td><font color=""blue"">12345</font></td>
						    <td>Todd</td>
						    <td>Rowe</td>
						  </tr>
						</table>
						<hr></div>";
			string result = new WebTest ("XMLDataSourceTest1.aspx").Run();
			HtmlDiff.AssertAreEqual (origin, HtmlDiff.GetControlFromPageHtml(result) , "TransformFail");
		}

		[Test]
		[Category ("NunitWeb")]
		public void TransformFile ()
		{
			string origin = @"<div><h2>Order</h2>
						<hr>
						<table>
						  <tr>
						    <td>Customer</td>
						    <td><font color=""blue"">12345</font></td>
						    <td>Todd</td>
						    <td>Rowe</td>
						  </tr>
						</table>
						<hr>
					  </div>";
			string result = new WebTest ("XMLDataSourceTest2.aspx").Run ();
			HtmlDiff.AssertAreEqual (origin, HtmlDiff.GetControlFromPageHtml (result), "TransformFileFail");
		}

		[Test]
		[Category ("NunitWeb")]
		public void TransformArgumentList ()
		{
			string origin = @"<div>
					      <h2>Order</h2>
						<hr>
						<table>
						  <tr>
						    <td>Customer</td>
						    <td><font color=""blue"">12345purchased by: Mainsoft developers</font></td>
						    <td>Todd</td>
						    <td>Rowe</td>
						  </tr>
						</table>
						<hr>
					</div>";
			string result = new WebTest ("XMLDataSourceTest3.aspx").Run ();
			HtmlDiff.AssertAreEqual (origin, HtmlDiff.GetControlFromPageHtml (result), "TransformArgumentListFail");
		}

		[Test]
		[Category ("NunitWeb")]
#if TARGET_JVM
		[Category ("NotWorking")] // File watcher is not supported
#endif
		public void Save ()
		{
			string origin = @"<div>
						<h2>BookStore</h2><hr>
						<table>
						  <tr>
						    <td>Book</td>
						    <td><font color=""blue"">ThisIsATest</font></td>
						    <td></td>
						    <td></td>
						    <td>24.95</td>
						  </tr>
						</table><hr>
						<h2>BookStore</h2><hr>
						<table>
						  <tr>
						    <td>Book</td>
						    <td><font color=""blue"">The Handmaid's Tale</font></td>
						    <td></td>
						    <td></td>
						    <td>29.95</td>
						  </tr>
						</table><hr></div>";
			string result = new WebTest ("XMLDataSourceTest4.aspx").Run ();
			HtmlDiff.AssertAreEqual (origin, HtmlDiff.GetControlFromPageHtml (result), "TransformArgumentListFail");
		}

		//events test
		bool checker;

		[Test]
		public void Events ()
		{
			DSPoker p = new DSPoker ();
			p.Transforming += new EventHandler (p_Transforming);
			Assert.AreEqual (false, checker, "BeforeTransformingEvent");
			p.DoOnTransforming (new EventArgs ());
			Assert.AreEqual (true, checker, "AfterTransformingEvent");
		}

		void p_Transforming (object sender, EventArgs e)
		{
			checker = true;
		}

		//TODO: This is implementation specific test - remove it.
		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (NullReferenceException))]
		public void GetXmlDocumentException ()
		{
			DSPoker p = new DSPoker ();
			p.GetXmlDocument ();
		}
#if NET_4_0
		[Test]
		public void CacheKeyContext ()
		{
			var xds = new XmlDataSource ();

			Assert.AreEqual (String.Empty, xds.CacheKeyContext, "#A1");
			xds.CacheKeyContext = null;
			Assert.AreEqual (String.Empty, xds.CacheKeyContext, "#A2");
			xds.CacheKeyContext = "MyKey";
			Assert.AreEqual ("MyKey", xds.CacheKeyContext, "#A1");
		}
#endif
		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
	}	
}

#endif
