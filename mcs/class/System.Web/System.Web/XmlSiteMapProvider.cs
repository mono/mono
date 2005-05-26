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

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Xml;
using System.Web.Util;
using System.IO;

namespace System.Web
{
	public class XmlSiteMapProvider : StaticSiteMapProvider, IDisposable
	{
		static readonly char [] seperators = { ';', ',' };
		bool building;
		string file;
		SiteMapNode root = null;
		FileSystemWatcher watcher;
		
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
					d.Load (file);
					
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
			return System.Web.Configuration.HandlersUtil.ExtractAttributeValue (name, n, true);
		}
		
		string GetOptionalAttribute (XmlNode n, string name)
		{
			return System.Web.Configuration.HandlersUtil.ExtractAttributeValue (name, n, true, true);
		}
		
		[MonoTODO]
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

		public void Dispose ()
		{
			watcher.Dispose ();
		}
		
		[MonoTODO]
		public override SiteMapNode FindSiteMapNode (string rawUrl)
		{
			return base.FindSiteMapNode (rawUrl); // why did they override this method!?
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
				
			if (File.Exists (file)) {
				watcher = new FileSystemWatcher ();
				watcher.Path = Path.GetFullPath (Path.GetDirectoryName (file));
				watcher.Filter = Path.GetFileName (file);
				watcher.Changed += new FileSystemEventHandler (OnFileChanged);
				watcher.EnableRaisingEvents = true;
			}
		}
		
		void OnFileChanged (object sender, FileSystemEventArgs args)
		{
			Clear ();
		}

		public override SiteMapNode RootNode {
			get {
				BuildSiteMap ();
				return root;
			}
		}
		
		protected internal override SiteMapNode GetRootNodeCore ()
		{
			return BuildSiteMap ();
		}
	}

}
#endif

