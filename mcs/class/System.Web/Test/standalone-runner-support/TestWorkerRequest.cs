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
using System.IO;
using System.Net;
using System.Web;
using System.Web.Hosting;

namespace StandAloneRunnerSupport
{
	sealed class TestWorkerRequest : SimpleWorkerRequest
	{
		static readonly char[] vpathTrimChars = { '/' };
		
		string page;
		string query;
		string appVirtualDir;
		string pathInfo;

		public bool IsPost { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public string StatusDescription { get; set; }
			
		public TestWorkerRequest (string page, string query, TextWriter output)
			: this (page, query, null, output)
		{
		}
		
		public TestWorkerRequest (string page, string query, string pathInfo, TextWriter output)
			: base (page, query, output)
		{
			this.page = page;
			this.query = query;
			this.appVirtualDir = GetAppPath ();
			this.pathInfo = pathInfo;
		}

		public override string GetFilePath ()
		{
			return page;
		}

		public override string GetHttpVerbName ()
		{
			if (IsPost)
				return "POST";

			return base.GetHttpVerbName ();
		}
		
		public override string GetPathInfo ()
		{
			if (pathInfo == null)
				return base.GetPathInfo ();

			return pathInfo;
		}
		
		public override string GetRawUrl ()
		{
			return TrimLeadingSlash (base.GetRawUrl ());
		}

		public override string GetUriPath ()
		{
			return TrimLeadingSlash (base.GetUriPath ());
		}

		public override void SendStatus (int code, string description)
		{
			StatusCode = (HttpStatusCode) code;
			StatusDescription = description;

			base.SendStatus (code, description);
		}
		
		static string TrimLeadingSlash (string input)
		{
			if (String.IsNullOrEmpty (input))
				return input;

			return "/" + input.TrimStart (vpathTrimChars);
		}
 	}
}
