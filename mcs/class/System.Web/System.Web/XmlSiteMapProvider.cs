//
// System.Web.XmlSiteMapProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Xml;
using System.Web.Util;
using System.IO;

namespace System.Web {
	public class XmlSiteMapProvider : SiteMapProvider, IDisposable {
		static readonly char [] seperators = { ';', ',' };
		bool building;
		
		public override SiteMapNode BuildSiteMap ()
		{
			if (root != null)
				return root;
			// Whenever you call AddNode, it tries to find dups, and will call this method
			// Is this a bug in MS??
			if (building)
				return null;
			
			lock (this) {
				building = true;
				if (root != null)
					return root;
				XmlDocument d = new XmlDocument ();
				d.Load (file);
				
				root = BuildSiteMapRecursive (d.SelectSingleNode ("/siteMap/siteMapNode"));
				AddNode (root);
				building = false;
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
				if (keywords != null) {
					foreach (string s in keywords.Split (seperators)) {
						string ss = s.Trim ();
						if (ss.Length > 0)
							keywordsList.Add (ss);
					}
				}
				
				ArrayList rolesList = new ArrayList ();
				if (roles != null) {
					foreach (string s in roles.Split (seperators)) {
						string ss = s.Trim ();
						if (ss.Length > 0)
							rolesList.Add (ss);
					}
				}
				
				if (url != null && url.Length > 0 && UrlUtils.IsRelativeUrl (url)) {
					if (url [0] != '~')
						url = HttpRuntime.AppDomainAppVirtualPath + '/' + url;
					else
						url = UrlUtils.ResolveVirtualPathFromAppAbsolute (url);
				}
				
				SiteMapNode node = new SiteMapNode (this, url, title, description,
					ArrayList.ReadOnly (keywordsList), ArrayList.ReadOnly (rolesList),
					null); // TODO what do they want for attributes
				
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

		[MonoTODO]
		public void Dispose ()
		{
			// what do i do?
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
		}

		public override SiteMapNode RootNode {
			get {
				BuildSiteMap ();
				return root;
			}
		}
		
		string file;
		SiteMapNode root = null;
	}

}
#endif

