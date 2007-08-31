// 
// System.Web.Services.Discovery.DiscoveryRequestHandler.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
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

using System.Web;
using System.IO;
using System.Web.Services.Discovery;

namespace System.Web.Services.Discovery {
	public sealed class DiscoveryRequestHandler : IHttpHandler {
		
		#region Constructors

		public DiscoveryRequestHandler () 
		{
		}
		
		#endregion // Constructors

		#region Properties

		public bool IsReusable {
			get { return true; }
		}
		
		#endregion // Properties

		#region Methods

		public void ProcessRequest (HttpContext context)
		{
			string path = context.Request.PhysicalPath;
			FileStream fs = new FileStream (path, FileMode.Open, FileAccess.Read);
			DynamicDiscoveryDocument ddoc = DynamicDiscoveryDocument.Load (fs);
			fs.Close ();
			
			string url = context.Request.Url.ToString();
			int i = url.LastIndexOf ('/');
			if (i != -1) url = url.Substring (0,i+1);
			
			DiscoveryDocument doc = new DiscoveryDocument ();
			GetFiles (url, "", Path.GetDirectoryName(path), doc, ddoc);
			
			context.Response.ContentType = "text/xml; charset=utf-8";
			doc.Write (context.Response.OutputStream);
		}
		
		void GetFiles (string baseUrl, string relPath, string path, DiscoveryDocument doc, DynamicDiscoveryDocument ddoc)
		{
			string url = baseUrl + relPath;
			if (!url.EndsWith ("/")) url += "/";
			
			string[] files = Directory.GetFiles (path);
			foreach (string file in files)
			{
				string ext = Path.GetExtension (file).ToLower();
				if (ext == ".asmx")
				{
					ContractReference cref = new ContractReference ();
					cref.DocRef = url + Path.GetFileName (file);
					cref.Ref = cref.DocRef + "?wsdl";
					doc.References.Add (cref);
				}
				else if (ext == ".disco")
				{
					DiscoveryDocumentReference dref = new DiscoveryDocumentReference ();
					dref.Ref = url + Path.GetFileName (file);
					doc.References.Add (dref);
				}
			}
			string[] dirs = Directory.GetDirectories (path);
			
			foreach (string dir in dirs)
			{
				string rel = Path.Combine (relPath, Path.GetFileName(dir));
				if (!ddoc.IsExcluded (rel))
					GetFiles (baseUrl, rel, Path.Combine (path, dir), doc, ddoc);
			}
		}

		#endregion // Methods
	}
}
