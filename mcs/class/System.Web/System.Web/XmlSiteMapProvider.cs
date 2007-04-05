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
using System.Globalization;
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

		protected internal override void AddNode (SiteMapNode node, SiteMapNode parentNode)
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
					d.Load (file);

					XmlNode enloc = d.DocumentElement.Attributes ["enableLocalization"];
					if (enloc != null && !String.IsNullOrEmpty (enloc.Value))
						EnableLocalization = (bool) Convert.ChangeType (enloc.Value, typeof (bool));
					
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

		void PutInCollection (string name, string value, ref NameValueCollection coll)
		{
			PutInCollection (name, null, value, ref coll);
		}
		
		void PutInCollection (string name, string classKey, string value, ref NameValueCollection coll)
		{
			if (coll == null)
				coll = new NameValueCollection ();
			if (!String.IsNullOrEmpty (classKey))
				coll.Add (name, classKey);
			coll.Add (name, value);
		}

		bool GetAttributeLocalization (string value, out string resClass, out string resKey, out string resDefault)
		{
			resClass = null;
			resKey = null;
			resDefault = null;

			if (String.IsNullOrEmpty (value))
				return false;
			string val = value.TrimStart (new char[] {' ', '\t'});
			if (val.Length < 11 ||
				String.Compare (val, 0, "$resources:", 0, 11, StringComparison.InvariantCultureIgnoreCase) != 0)
				return false;

			val = val.Substring (11);
			if (val.Length == 0)
				return false;
			string[] parts = val.Split (',');
			if (parts.Length < 2)
				return false;
			resClass = parts [0].Trim ();
			resKey = parts [1].Trim ();
			if (parts.Length == 3)
				resDefault = parts [2];
			else if (parts.Length > 3)
				resDefault = String.Join (",", parts, 2, parts.Length - 2);

			return true;
		}
		
		void CollectLocalizationInfo (XmlNode xmlNode, ref string title, ref string description,
					      ref NameValueCollection attributes,
					      ref NameValueCollection explicitResourceKeys)
		{
			string resClass;
			string resKey;
			string resDefault;

			if (GetAttributeLocalization (title, out resClass, out resKey, out resDefault)) {
				PutInCollection ("title", resClass, resKey, ref explicitResourceKeys);
				title = resDefault;
			}
			
			if (GetAttributeLocalization (description, out resClass, out resKey, out resDefault)) {
				PutInCollection ("description", resClass, resKey, ref explicitResourceKeys);
				description = resDefault;
			}

			string value;
			foreach (XmlNode att in xmlNode.Attributes) {
				if (GetAttributeLocalization (att.Value, out resClass, out resKey, out resDefault)) {
					PutInCollection (att.Name, resClass, resKey, ref explicitResourceKeys);
					value = resDefault;
				} else
					value = att.Value;
				PutInCollection (att.Name, value, ref attributes);
			}
		}
		
		SiteMapNode BuildSiteMapRecursive (XmlNode xmlNode)
		{
			if (xmlNode.Name != "siteMapNode")
				throw new ConfigurationException ("incorrect element name", xmlNode);
			
			string provider = GetNonEmptyOptionalAttribute (xmlNode, "provider");
			string siteMapFile = GetNonEmptyOptionalAttribute (xmlNode, "siteMapFile");
			
			if (provider != null) {
				foreach (SiteMapProvider smp in SiteMap.Providers) {
					if (string.Equals(smp.Name,provider, StringComparison.InvariantCulture)) {
						smp.ParentProvider = this;
						return smp.GetRootNodeCore();
					}
				}
				throw new ConfigurationException("Provider with name [" + provider + "] was not found.");
			} else if (siteMapFile != null) {
				throw new NotImplementedException ();
			} else {

				string url = GetOptionalAttribute (xmlNode, "url");
				string title = GetOptionalAttribute (xmlNode, "title");
				string description = GetOptionalAttribute (xmlNode, "description");
				string keywords = GetOptionalAttribute (xmlNode, "keywords");
				string roles = GetOptionalAttribute (xmlNode, "roles");
				string implicitResourceKey = GetOptionalAttribute (xmlNode, "resourceKey");
				
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

				NameValueCollection attributes = null;
				NameValueCollection explicitResourceKeys = null;
				if (EnableLocalization)
					CollectLocalizationInfo (xmlNode, ref title, ref description, ref attributes,
								 ref explicitResourceKeys);
				else
					foreach (XmlNode att in xmlNode.Attributes)
						PutInCollection (att.Name, att.Value, ref attributes);

				string key = Guid.NewGuid ().ToString ();
				SiteMapNode node = new SiteMapNode (this, key, url, title, description,
								    ArrayList.ReadOnly (rolesList),
								    attributes,
								    explicitResourceKeys,
								    implicitResourceKey);
					
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
			if (disposing)
				watcher.Dispose ();
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

			if (HttpContext.Current != null)
				file = HttpContext.Current.Request.MapPath (file, HttpRuntime.AppDomainAppVirtualPath, false);
			else
				throw new InvalidOperationException ("HttpContext is missing");

			if (File.Exists (file)) {
				watcher = new FileSystemWatcher ();
				watcher.Path = Path.GetFullPath (Path.GetDirectoryName (file));
				watcher.Filter = Path.GetFileName (file);
				watcher.Changed += new FileSystemEventHandler (OnFileChanged);
				watcher.EnableRaisingEvents = true;
			}
		}

		protected override void RemoveNode (SiteMapNode node)
		{
			base.RemoveNode (node);
		}

		[MonoTODO ("Not implemented")]
		protected virtual void RemoveProvider (string providerName)
		{
			throw new NotImplementedException ();
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

