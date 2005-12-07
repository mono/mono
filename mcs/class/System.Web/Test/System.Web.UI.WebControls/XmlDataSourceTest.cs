//
// Tests for System.Web.UI.WebControls.XmlDataSource.cs 
//
// Author:
//	Chris Toshok (toshok@novell.com)
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
//

#if NET_2_0

using NUnit.Framework;
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
	}

	[TestFixture]
	public class XmlDataSourceTest
	{
		[Test]
		public void Defaults ()
		{
			DSPoker p = new DSPoker ();

			Assert.AreEqual ("", p.Data, "A4");
			Assert.AreEqual ("", p.DataFile, "A5");
			Assert.AreEqual ("", p.Transform, "A9");
			Assert.AreEqual ("", p.TransformFile, "A10");
			Assert.AreEqual ("", p.XPath, "A11");
			Assert.AreEqual (0, p.CacheDuration, "A12");
			Assert.AreEqual (DataSourceCacheExpiry.Absolute, p.CacheExpirationPolicy, "A13");
			Assert.AreEqual ("", p.CacheKeyDependency, "A14");
			Assert.AreEqual (true, p.EnableCaching, "A15");
		}

		[Test]
		public void Attributes ()
		{
			DSPoker p = new DSPoker ();

			p.Data = Test1Document1;
			Assert.AreEqual (Test1Document1, p.Data, "A1");

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

			p.Data = Test1Document1;
			p.Transform = "transform";
			p.XPath = "xpath";
			
			object state = p.SaveState();
			DSPoker copy = new DSPoker ();
			copy.LoadState (state);
			Assert.AreEqual ("", copy.Data, "A1");
			Assert.AreEqual ("", copy.Transform, "A2");
			Assert.AreEqual ("", copy.XPath, "A3");


			p = new DSPoker ();
			p.DataFile = "DataFile";
			p.TransformFile = "TransformFile";

			state = p.SaveState();
			copy = new DSPoker ();

			copy.LoadState (state);
			Assert.AreEqual ("", copy.DataFile, "A1");
			Assert.AreEqual ("", copy.TransformFile, "A2");
		}

		const string Test1Document1 = 
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<IranHistoricalPlaces name=""places"">
 <Place name=""Taghe Bostan"">
  <City>Kermanshah</City>
  <Antiquity>2000</Antiquity>
 </Place>
 <Place name=""Persepolis"">
  <City>Shiraz</City>
  <Antiquity>2500</Antiquity>
 </Place>
</IranHistoricalPlaces>";
	}
}

#endif
