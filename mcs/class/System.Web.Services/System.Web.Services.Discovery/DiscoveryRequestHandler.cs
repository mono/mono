// 
// System.Web.Services.Discovery.DiscoveryRequestHandler.cs
//
// Author:
//   Dave Bettin (javabettin@yahoo.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Dave Bettin, 2002
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
			FileStream fs = new FileStream (path, FileMode.Open);
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
