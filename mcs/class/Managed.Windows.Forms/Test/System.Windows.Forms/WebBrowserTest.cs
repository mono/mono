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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

#if NET_2_0

using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using io=System.IO;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class WebBrowserTest
	{
		WebBrowser wb = null;
		
		[SetUp]
		public void SetUp()
		{
			wb = new WebBrowser ();
			bool start = false;
			wb.DocumentCompleted += delegate (object sender, WebBrowserDocumentCompletedEventArgs e) {
				if (e.Url.Equals ("about:blank")) {
					start = true;
				}
			};
			
			while (!start){}
		}
		
		public string CreateTestPage (string html)
		{
			io.FileStream f = null;
			string path;
			Random rnd;
			int num = 0;

			rnd = new Random ();
			do {
				num = rnd.Next ();
				num++;
				path = io.Path.Combine (io.Path.GetTempPath(), "html" + num.ToString("x") + ".html");

				try {
					f = new io.FileStream (path, io.FileMode.CreateNew, io.FileAccess.ReadWrite, io.FileShare.Read,
							    8192, (io.FileOptions) 1);
				}
				catch (global::System.Security.SecurityException) {
					// avoid an endless loop
					throw;
				}
				catch {
				}
			} while (f == null);
			io.StreamWriter s = new io.StreamWriter (f);
			s.Write (html);			
			f.Close();
			return path;
		}
		
		
		[Test]
		public void LoadingEventsTest()
		{
			string html = "<html><head></head><body></body></html>";
			string url = "file://" + CreateTestPage (html);
			string events = String.Empty;
			bool stop = false;
			wb.DocumentCompleted += delegate (object sender, WebBrowserDocumentCompletedEventArgs e) {
				if (e.Url.Equals (url)) {
					stop = true;
					Assert.AreEqual("navigatingnavigated", events, "#A1");					
				}
			};
			wb.Navigating += delegate (object sender1, WebBrowserNavigatingEventArgs e1) {
				if (e1.Url.Equals (url))
					events += "navigating";
			};
			wb.Navigated += delegate (object sender1, WebBrowserNavigatedEventArgs e1) {
				if (e1.Url.Equals (url))
					events += "navigated";
			};
			
			wb.Navigate (url);
			while (!stop){}
		}		

		[Test]
		public void InnerHtmlTest()
		{
			string html = "<html><head></head><body><div id=\"testid\"></div></body></html>";
			string url = "file://" + CreateTestPage (html);
			string test = "testing inner html";
			bool stop = false;
			wb.DocumentCompleted += delegate (object sender, WebBrowserDocumentCompletedEventArgs e) {
				Console.Error.WriteLine (wb.Document.Body.InnerHtml);
				if (e.Url.Equals (url)) {
					stop = true;
					Assert.IsNotNull (wb.Document, "#A1");
					HtmlElement elem = wb.Document.GetElementById ("testid");
					Assert.IsNotNull (elem, "#A2");
					elem.InnerHtml = test;
					string ret = elem.InnerHtml;
					Assert.AreEqual (ret, test, "#A3");
				}
			};			
			wb.Navigate (url);
			while (!stop){}
		}		
	
	}
}
#endif