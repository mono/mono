//
// GlobalizationSectionTest.cs 
//	- unit tests for System.Web.Configuration.GlobalizationSection
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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
using System.Configuration;
using System.Reflection;
using System.Text;
using System.IO;
using System.Xml;
using System.Web.Configuration;
using System.Web;
using System.Web.Security;

using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.Configuration {

	[TestFixture]
	public class GlobalizationSectionTest  {

		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (GetType (), "GlobalizationEncodingName.aspx", "GlobalizationEncodingName.aspx");
		}
		
		[Test]
		public void Defaults ()
		{
			GlobalizationSection g = new GlobalizationSection ();

			Assert.AreEqual ("", g.Culture, "A1");
			Assert.IsFalse (g.EnableBestFitResponseEncoding, "A2");
			Assert.IsFalse (g.EnableClientBasedCulture, "A3");

			// XXX FileEncoding?

			Assert.AreEqual (Encoding.UTF8, g.RequestEncoding, "A5");
			Assert.AreEqual ("", g.ResourceProviderFactoryType, "A6");
			Assert.AreEqual (Encoding.UTF8, g.ResponseHeaderEncoding, "A7");
			Assert.AreEqual ("", g.UICulture, "A8");
		}

		[Test]
		public void PreSerialize ()
		{
			StringWriter sw;
			XmlWriter writer;
			MethodInfo mi = typeof (GlobalizationSection).GetMethod ("PreSerialize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			GlobalizationSection s;
			object[] parms = new object[1];
			bool failed;

			sw = new StringWriter ();
			writer = new XmlTextWriter (sw);

			s = new GlobalizationSection();
			parms[0] = writer;

			/* 1 */
			mi.Invoke (s, parms);

			/* 2 */
			failed = true;
			try {
				s.Culture = "illegal-culture";
				mi.Invoke (s, parms);
			}
			catch (TargetInvocationException e) {
				Assert.AreEqual (typeof (ConfigurationErrorsException), e.InnerException.GetType (), "A2");
				failed = false;
			}
			Assert.IsFalse (failed, "A2");

			/* 3 */
			failed = true;
			try {
				s.Culture = "";
				s.UICulture = "illegal-culture";
				mi.Invoke (s, parms);
			}
			catch (TargetInvocationException e) {
				Assert.AreEqual (typeof (ConfigurationErrorsException), e.InnerException.GetType (), "A3");
				failed = false;
			}
			Assert.IsFalse (failed, "A3");

			/* 4 */
			s.Culture = "";
			s.UICulture = "";
			s.ResourceProviderFactoryType = "invalid-type";
			mi.Invoke (s, parms);

			/* 5  (null writer) */
			parms[0] =null;
			mi.Invoke (s, parms);
		}

		[Test]
		public void PostDeserialize ()
		{
			MethodInfo mi = typeof (GlobalizationSection).GetMethod ("PostDeserialize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			GlobalizationSection s;
			object[] parms = new object[0];
			bool failed;

			s = new GlobalizationSection();

			/* 1 */
			mi.Invoke (s, parms);

			/* 2 */
			failed = true;
			try {
				s.Culture = "illegal-culture";
				mi.Invoke (s, parms);
			}
			catch (TargetInvocationException e) {
				Assert.AreEqual (typeof (ConfigurationErrorsException), e.InnerException.GetType (), "A2");
				failed = false;
			}
			Assert.IsFalse (failed, "A2");

			failed = true;
			try {
				s.Culture = "";
				s.UICulture = "illegal-culture";
				mi.Invoke (s, parms);
			}
			catch (TargetInvocationException e) {
				Assert.AreEqual (typeof (ConfigurationErrorsException), e.InnerException.GetType (), "A3");
				failed = false;
			}
			Assert.IsFalse (failed, "A3");

			s.Culture = "";
			s.UICulture = "";
			s.ResourceProviderFactoryType = "invalid-type";
			mi.Invoke (s, parms);
		}

		[Test]
		public void GlobalizationEncodingName ()
		{
			string pageHtml = new WebTest ("GlobalizationEncodingName.aspx").Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);
			string originalHtml = "GOOD";
			Assert.AreEqual (originalHtml, renderedHtml, "#A1");
		}
		
	}
}

#endif
