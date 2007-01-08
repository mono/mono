//
// System.Web.XmlSiteMapProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2003 Ben Maurer
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Xml;
using System.Web.Util;
using System.IO;
using System.Web;
using System;
using Mainsoft.Web;
using Mainsoft.Web.Configuration;
using Mainsoft.Web.Util;

namespace Mainsoft.Web.AspnetConfig
{
	public class CustomXmlSiteMapProvider : StaticSiteMapProvider, IDisposable
	{
		static readonly char [] seperators = { ';', ',' };
		bool building;
		string file;
		SiteMapNode root = null;
#if !TARGET_JVM // Java platform does not support file notifications
		FileSystemWatcher watcher;
#endif

		protected override void AddNode (SiteMapNode node, SiteMapNode parentNode)
		{
			base.AddNode (node, parentNode);
		}

		protected virtual void AddProvider (string providerName, SiteMapNode parentNode)
		{
			throw new NotImplementedException ();
		}

		public override SiteMapNode BuildSiteMap ()
		{
			if (root != null)
				return root;
			// Whenever you call AddNode, it tries to find dups, and will call this method
			// Is this a bug in MS??
			if (building)
				return null;
			
			lock (this) {
				try {
					building = true;
					if (root != null)
						return root;
					XmlDocument d = new XmlDocument ();
					using (Stream source = GetType ().Assembly.GetManifestResourceStream ("Mainsoft.Web.AspnetConfig.aspnetconfig.Web.sitemap")) {
					if (source == null)
					throw new ArgumentException ("resource not found: ~/aspnetconfig/Web.sitemap");
						d.Load (source);
					}

					XmlNode nod = d.DocumentElement ["siteMapNode"];
					if (nod == null)
						throw new HttpException ("Invalid site map file: " + Path.GetFileName (file));
						
					root = BuildSiteMapRecursive (nod);
						
					AddNode (root);
				} finally {
					building = false;
				}
				return root;
			}
		}
		
		string GetNonEmptyOptionalAttribute (XmlNode n, string name)
		{
			return HandlersUtil.ExtractAttributeValue (name, n, true);
		}
		
		string GetOptionalAttribute (XmlNode n, string name)
		{
			return HandlersUtil.ExtractAttributeValue (name, n, true, true);
		}
		
		SiteMapNode BuildSiteMapRecursive (XmlNode xmlNode)
		{
			if (xmlNode.Name != "siteMapNode")
				throw new ConfigurationException ("incorrect element name", xmlNode);
			
			string provider = GetNonEmptyOptionalAttribute (xmlNode, "provider");
			string siteMapFile = GetNonEmptyOptionalAttribute (xmlNode, "siteMapFile");
			
			if (provider != null) {
				throw new NotImplementedException ();
			} else if (siteMapFile != null) {
				throw new NotImplementedException ();
			} else {

				string url = GetOptionalAttribute (xmlNode, "url");
				string title = GetOptionalAttribute (xmlNode, "title");
				string description = GetOptionalAttribute (xmlNode, "description");
				string keywords = GetOptionalAttribute (xmlNode, "keywords");
				string roles = GetOptionalAttribute (xmlNode, "roles");
				
				ArrayList keywordsList = new ArrayList ();
				if (keywords != null && keywords.Length > 0) {
					foreach (string s in keywords.Split (seperators)) {
						string ss = s.Trim ();
						if (ss.Length > 0)
							keywordsList.Add (ss);
					}
				}
				
				ArrayList rolesList = new ArrayList ();
				if (roles != null && roles.Length > 0) {
					foreach (string s in roles.Split (seperators)) {
						string ss = s.Trim ();
						if (ss.Length > 0)
							rolesList.Add (ss);
					}
				}

				if (!string.IsNullOrEmpty (url)) {
					if (UrlUtils.IsRelativeUrl (url))
						url = UrlUtils.Combine (HttpRuntime.AppDomainAppVirtualPath, url);
				}
				
				SiteMapNode node = new SiteMapNode (this, url, url, title, description,
					/*ArrayList.ReadOnly (keywordsList), */ArrayList.ReadOnly (rolesList), null,
					null, null); // TODO what do they want for attributes
					
				foreach (XmlNode child in xmlNode.ChildNodes) {
					if (child.NodeType != XmlNodeType.Element)
						continue;
					AddNode (BuildSiteMapRecursive (child), node);
				}
				
				return node;
			}
		}

		protected override void Clear ()
		{
			base.Clear ();
			root = null;
		}

		protected virtual void Dispose (bool disposing)
		{
#if !TARGET_JVM // Java platform does not support file notifications
			if (disposing)
				watcher.Dispose ();
#endif
		}

		public void Dispose ()
		{
			Dispose (true);
		}
		
		public override SiteMapNode FindSiteMapNode (string rawUrl)
		{
			return base.FindSiteMapNode (rawUrl); // why did they override this method!?
		}

		public override SiteMapNode FindSiteMapNodeFromKey (string key)
		{
			return base.FindSiteMapNodeFromKey (key); // why did they override this method!?
		}

		public override void Initialize (string name, NameValueCollection attributes)
		{

			base.Initialize (name, attributes);
			file = attributes ["siteMapFile"];

			if (file == null && file.Length == 0)
				throw new ArgumentException ("you must provide a file");
			
			if (UrlUtils.IsRelativeUrl (file))
				file = Path.Combine(HttpRuntime.AppDomainAppPath, file);
			else
				file = UrlUtils.ResolvePhysicalPathFromAppAbsolute (file);

#if !TARGET_JVM // Java platform does not support file notifications
			if (File.Exists (file)) {
				watcher = new FileSystemWatcher ();
				watcher.Path = Path.GetFullPath (Path.GetDirectoryName (file));
				watcher.Filter = Path.GetFileName (file);
				watcher.Changed += new FileSystemEventHandler (OnFileChanged);
				watcher.EnableRaisingEvents = true;
			}
#endif
		}

		protected override void RemoveNode (SiteMapNode node)
		{
			base.RemoveNode (node);
		}

		//[MonoTODO ("Not implemented")]
		protected virtual void RemoveProvider (string providerName)
		{
			throw new NotImplementedException ();
		}
#if !TARGET_JVM
		void OnFileChanged (object sender, FileSystemEventArgs args)
		{
			Clear ();
		}
#endif
		public override SiteMapNode RootNode {
			get {
				BuildSiteMap ();
				return root;
			}
		}
		
		protected  override SiteMapNode GetRootNodeCore ()
		{
			return BuildSiteMap ();
		}
	}

}


