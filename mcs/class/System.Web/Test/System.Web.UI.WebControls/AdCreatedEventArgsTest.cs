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
//
// System.Web.UI.WebControls.AdCreatedEventArgsTest.cs
//
// Author:
//	Jackson Harper (jackson@ximian.com)
//

using System;
using System.Collections;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	[TestFixture]
	public class AdCreatedEventArgsTest {

		[Test]
		public void Defaults ()
		{
			Hashtable table = new Hashtable ();
			AdCreatedEventArgs e = new AdCreatedEventArgs (table);
			Assert.AreEqual (e.AdProperties, table, "Constructor");

			e = new AdCreatedEventArgs (null);
			Assert.AreEqual (e.AdProperties, null, "Null Constructor");
		}

		[Test]
		public void SetPropsInCtor ()
		{
			Hashtable table = new Hashtable ();
			table ["AlternateText"] = "alt text";
			table ["ImageUrl"] = "image url";
			table ["NavigateUrl"] = "nav url";
			AdCreatedEventArgs e = new AdCreatedEventArgs (table);

			Assert.AreEqual (e.AlternateText, "alt text", "alt text");
			Assert.AreEqual (e.ImageUrl, "image url", "image url");
			Assert.AreEqual (e.NavigateUrl, "nav url", "nav url");
		}

		[Test]
		public void SetProps ()
		{
			AdCreatedEventArgs e = new AdCreatedEventArgs (null);

			e.AlternateText = "alt text";
			Assert.AreEqual (e.AlternateText, "alt text", "alt text");

			e.AlternateText = null;
			Assert.AreEqual (e.AlternateText, null, "null alt text");

			e.ImageUrl = "image url";
			Assert.AreEqual (e.ImageUrl, "image url", "image url");

			e.ImageUrl = null;
			Assert.AreEqual (e.ImageUrl, null, "null image url");

			e.NavigateUrl = "nav url";
			Assert.AreEqual (e.NavigateUrl, "nav url", "nav url");

			e.NavigateUrl = null;
			Assert.AreEqual (e.NavigateUrl, null, "null nav url");
		}

		[Test]
		public void ModifyProps ()
		{
			Hashtable table = new Hashtable ();
			table ["AlternateText"] = "alt text";
			table ["ImageUrl"] = "image url";
			table ["NavigateUrl"] = "nav url";
			AdCreatedEventArgs e = new AdCreatedEventArgs (table);

			e.AlternateText = "foo";
			Assert.AreEqual (e.AdProperties ["AlternateText"],
					"alt text", "alt text");

			e.ImageUrl = "bar";
			Assert.AreEqual (e.AdProperties ["ImageUrl"],
					"image url", "image url");

			e.NavigateUrl = "baz";
			Assert.AreEqual (e.AdProperties ["NavigateUrl"],
					"nav url", "nav url");
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void BadCastAlternateText ()
		{
			Hashtable table = new Hashtable ();
			table ["AlternateText"] = 52;
			AdCreatedEventArgs e = new AdCreatedEventArgs (table);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void BadCastImageUrl ()
		{
			Hashtable table = new Hashtable ();
			table ["ImageUrl"] = 52;
			AdCreatedEventArgs e = new AdCreatedEventArgs (table);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void BadCastNavigateUrl ()
		{
			Hashtable table = new Hashtable ();
			table ["NavigateUrl"] = 52;
			AdCreatedEventArgs e = new AdCreatedEventArgs (table);
		}
	}
}
