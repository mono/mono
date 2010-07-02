//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2010 Novell, Inc http://novell.com/
//

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;

using StandAloneRunnerSupport;
using StandAloneTests;

using NUnit.Framework;

namespace StandAloneTests.SiteMapPathRendering
{
	[TestCase ("SiteMapPathRendering", "SiteMapPath does not use child controls when rendering.")]
	public sealed class SiteMapPathRendering_01 : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "SiteMapPathRendering"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			runItems.Add (new TestRunItem ("/Default.aspx", Default_Aspx));
			runItems.Add (new TestRunItem ("/Default.aspx?mode=foo", Default_Foo_Aspx));
			runItems.Add (new TestRunItem ("/Default.aspx?mode=bar", Default_Bar_Aspx));
			
			return true;
		}

		void Default_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<span id=\"sitemap\"><a href=\"#sitemap_SkipLink\"><img alt=\"Skip Navigation Links\" height=\"0\" width=\"0\" src=\"/WebResource.axd\" style=\"border-width:0px;\" /></a><span><a href=\"/Default.aspx\">Main</a></span><a id=\"sitemap_SkipLink\"></a></span><pre id=\"log\">\tSystem.Web.UI.WebControls.SiteMapNodeItem [sitemap$ctl00]\r\n\tSystem.Web.UI.WebControls.HyperLink [sitemap$ctl00$ctl00]\r\n</pre>";
			Helpers.ExtractAndCompareCodeFromHtml (Helpers.StripWebResourceAxdQuery (result), originalHtml, "#A1");
		}

		void Default_Foo_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<span id=\"sitemap\"><a href=\"#sitemap_SkipLink\"><img alt=\"Skip Navigation Links\" height=\"0\" width=\"0\" src=\"/WebResource.axd\" style=\"border-width:0px;\" /></a><span><a href=\"/Default.aspx\">Main</a></span><span> &gt; </span><span><a href=\"/Default.aspx?mode=foo\">Foo</a></span><a id=\"sitemap_SkipLink\"></a></span><pre id=\"log\">\tSystem.Web.UI.WebControls.SiteMapNodeItem [sitemap$ctl00]\r\n\tSystem.Web.UI.WebControls.HyperLink [sitemap$ctl00$ctl00]\r\n\tSystem.Web.UI.WebControls.SiteMapNodeItem [sitemap$ctl01]\r\n\tSystem.Web.UI.WebControls.Literal [sitemap$ctl01$ctl00]\r\n\tSystem.Web.UI.WebControls.SiteMapNodeItem [sitemap$ctl02]\r\n\tSystem.Web.UI.WebControls.HyperLink [sitemap$ctl02$ctl00]\r\n</pre>";
			Helpers.ExtractAndCompareCodeFromHtml (Helpers.StripWebResourceAxdQuery (result), originalHtml, "#A1");
		}

		void Default_Bar_Aspx (string result, TestRunItem runItem)
		{
			string originalHtml = "<span id=\"sitemap\"><a href=\"#sitemap_SkipLink\"><img alt=\"Skip Navigation Links\" height=\"0\" width=\"0\" src=\"/WebResource.axd\" style=\"border-width:0px;\" /></a><span><a href=\"/Default.aspx\">Main</a></span><span> &gt; </span><span><a href=\"/Default.aspx?mode=bar\">Bar</a></span><a id=\"sitemap_SkipLink\"></a></span><pre id=\"log\">\tSystem.Web.UI.WebControls.SiteMapNodeItem [sitemap$ctl00]\r\n\tSystem.Web.UI.WebControls.HyperLink [sitemap$ctl00$ctl00]\r\n\tSystem.Web.UI.WebControls.SiteMapNodeItem [sitemap$ctl01]\r\n\tSystem.Web.UI.WebControls.Literal [sitemap$ctl01$ctl00]\r\n\tSystem.Web.UI.WebControls.SiteMapNodeItem [sitemap$ctl02]\r\n\tSystem.Web.UI.WebControls.HyperLink [sitemap$ctl02$ctl00]\r\n</pre>";
			Helpers.ExtractAndCompareCodeFromHtml (Helpers.StripWebResourceAxdQuery (result), originalHtml, "#A1");
		}
	}
}
