//
// System.Web.XmlSiteMapProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//	Marek Habersack <mhabersack@novell.com>
//
// (C) 2003 Ben Maurer
// (C) 2005-2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Web.Hosting;
using System.Web.Util;
using System.IO;

namespace System.Web
{
	public class XmlSiteMapProvider : StaticSiteMapProvider, IDisposable
	{
		static readonly char [] seperators = { ';', ',' };
		
		bool initialized;
		string fileVirtualPath;
		SiteMapNode root = null;
		List <FileSystemWatcher> watchers;
		Dictionary <string, bool> _childProvidersPresent;
		List <SiteMapProvider> _childProviders;
		
		Dictionary <string, bool> ChildProvidersPresent {
			get {
				if (_childProvidersPresent == null)
					_childProvidersPresent = new Dictionary <string, bool> ();

				return _childProvidersPresent;
			}
		}

		List <SiteMapProvider> ChildProviders {
			get {
				if (_childProviders == null)
					_childProviders = new List <SiteMapProvider> ();

				return _childProviders;
			}
		}
		
		protected internal override void AddNode (SiteMapNode node, SiteMapNode parentNode)
		{
			if (node == null)
				throw new ArgumentNullException ("node");

			if (parentNode == null)
				throw new ArgumentNullException ("parentNode");

			SiteMapProvider nodeProvider = node.Provider;
			if (nodeProvider != this)
				throw new ArgumentException ("SiteMapNode '" + node + "' cannot be found in current provider, only nodes in the same provider can be added.",
							     "node");

			SiteMapProvider parentNodeProvider = parentNode.Provider;
			if (nodeProvider != parentNodeProvider)
				throw new ArgumentException ("SiteMapNode '" + parentNode + "' cannot be found in current provider, only nodes in the same provider can be added.",
							     "parentNode");

			AddNodeNoCheck (node, parentNode);
		}

		void AddNodeNoCheck (SiteMapNode node, SiteMapNode parentNode)
		{
			base.AddNode (node, parentNode);
			SiteMapProvider nodeProvider = node.Provider;
			if (nodeProvider != this)
				RegisterChildProvider (nodeProvider.Name, nodeProvider);
		}
		
		protected virtual void AddProvider (string providerName, SiteMapNode parentNode)
		{
			if (parentNode == null)
				throw new ArgumentNullException ("parentNode");

			if (parentNode.Provider != this)
				throw new ArgumentException ("The Provider property of the parentNode does not reference the current provider.", "parentNode");

			SiteMapProvider smp = SiteMap.Providers [providerName];
			if (smp == null)
				throw new ProviderException ("Provider with name [" + providerName + "] was not found.");

			AddNode (smp.GetRootNodeCore ());
			RegisterChildProvider (providerName, smp);
		}

		void RegisterChildProvider (string name, SiteMapProvider smp)
		{
			Dictionary <string, bool> childProvidersPresent = ChildProvidersPresent;
			
			if (childProvidersPresent.ContainsKey (name))
				return;

			childProvidersPresent.Add (name, true);
			ChildProviders.Add (smp);
		}
		
		XmlNode FindStartingNode (string virtualPath, out bool enableLocalization)
		{
			XmlDocument d = GetConfigDocument (virtualPath);
			XmlElement docElement = d.DocumentElement;

			if (String.Compare ("siteMap", docElement.Name, StringComparison.Ordinal) != 0)
				throw new ConfigurationErrorsException ("Top element must be 'siteMap'");
			
			XmlNode enloc = docElement.Attributes ["enableLocalization"];
			if (enloc != null && !String.IsNullOrEmpty (enloc.Value))
				enableLocalization = (bool) Convert.ChangeType (enloc.Value, typeof (bool));
			else
				enableLocalization = false;

			XmlNodeList childNodes = docElement.ChildNodes;
			XmlNode node = null;
			
			foreach (XmlNode child in childNodes) {
				if (String.Compare ("siteMapNode", child.Name, StringComparison.Ordinal) != 0)
					// Only <siteMapNode> is allowed at the top
					throw new ConfigurationErrorsException ("Only <siteMapNode> elements are allowed at the document top level.");
				
				if (node != null)
					// Only one <siteMapNode> is allowed at the top
					throw new ConfigurationErrorsException ("Only one <siteMapNode> element is allowed at the document top level.");
				
				node = child;
			}
			
			if (node == null)
				throw new ConfigurationErrorsException ("Missing <siteMapNode> element at the document top level.");
			
			return node;
		}

		XmlDocument GetConfigDocument (string virtualPath)
		{
			if (String.IsNullOrEmpty (virtualPath))
				throw new ArgumentException ("The siteMapFile attribute must be specified on the XmlSiteMapProvider");
			
			string file = HostingEnvironment.MapPath (virtualPath);
			if (file == null)
				throw new HttpException ("Virtual path '" + virtualPath + "' cannot be mapped to physical path.");
			
			if (String.Compare (Path.GetExtension (file), ".sitemap", RuntimeHelpers.StringComparison) != 0)
				throw new InvalidOperationException (String.Format ("The file {0} has an invalid extension, only .sitemap files are allowed in XmlSiteMapProvider.",
										    String.IsNullOrEmpty (virtualPath) ? Path.GetFileName (file) : virtualPath));
			
			if (!File.Exists (file))
				throw new InvalidOperationException (String.Format ("The file '{0}' required by XmlSiteMapProvider does not exist.",
										    String.IsNullOrEmpty (virtualPath) ? Path.GetFileName (file) : virtualPath));

			ResourceKey = Path.GetFileName (file);
			CreateWatcher (file);
			
			XmlDocument d = new XmlDocument ();
			d.Load (file);

			return d;
		}

		public override SiteMapNode BuildSiteMap ()
		{
			if (root != null)
				return root;
			
			// Whenever you call AddNode, it tries to find dups, and will call this method
			// Is this a bug in MS??
			lock (this_lock) {
				if (root != null)
					return root;

				Clear ();
				bool enableLocalization;
				XmlNode node = FindStartingNode (fileVirtualPath, out enableLocalization);
				EnableLocalization = enableLocalization;
				BuildSiteMapRecursive (node, null);

				// if (builtRoot != root) {
				// 	root = builtRoot;
				// 	AddNode (root);
				// }

				return root;
			}
		}

		SiteMapNode ConvertToSiteMapNode (XmlNode xmlNode)
		{
			bool localize = EnableLocalization;
			string url = GetOptionalAttribute (xmlNode, "url");
			string title = GetOptionalAttribute (xmlNode, "title");
			string description = GetOptionalAttribute (xmlNode, "description");
			string roles = GetOptionalAttribute (xmlNode, "roles");
			string implicitResourceKey = GetOptionalAttribute (xmlNode, "resourceKey");
				
			// var keywordsList = new List <string> ();
			// if (keywords != null && keywords.Length > 0) {
			// 	foreach (string s in keywords.Split (seperators)) {
			// 		string ss = s.Trim ();
			// 		if (ss.Length > 0)
			// 			keywordsList.Add (ss);
			// 	}
			// }
				
			var rolesList = new List <string> ();
			if (roles != null && roles.Length > 0) {
				foreach (string s in roles.Split (seperators)) {
					string ss = s.Trim ();
					if (ss.Length > 0)
						rolesList.Add (ss);
				}
			}

			url = base.MapUrl (url);

			NameValueCollection attributes = null;
			NameValueCollection explicitResourceKeys = null;
			if (localize)
				CollectLocalizationInfo (xmlNode, ref title, ref description, ref attributes, ref explicitResourceKeys);
			else
				foreach (XmlNode att in xmlNode.Attributes)
					PutInCollection (att.Name, att.Value, ref attributes);

			string key = Guid.NewGuid ().ToString ();
			return new SiteMapNode (this, key, url, title, description, rolesList.AsReadOnly (),
						attributes, explicitResourceKeys, implicitResourceKey);		
		}

		void BuildSiteMapRecursive (XmlNode xmlNode, SiteMapNode parent)
		{
			if (xmlNode.Name != "siteMapNode")
				throw new ConfigurationException ("incorrect element name", xmlNode);
			
			string attrValue = GetNonEmptyOptionalAttribute (xmlNode, "provider");
			if (attrValue != null) {
				SiteMapProvider provider = SiteMap.Providers [attrValue];
				if (provider == null)
					throw new ProviderException ("Provider with name [" + attrValue + "] was not found.");

				provider.ParentProvider = this;
				SiteMapNode providerRoot = provider.GetRootNodeCore();

				if (parent == null)
					root = providerRoot;
				else
					AddNodeNoCheck (providerRoot, parent);
				return;
			}

			attrValue = GetNonEmptyOptionalAttribute (xmlNode, "siteMapFile");
			if (attrValue != null) {
				var nvc = new NameValueCollection ();
				nvc.Add ("siteMapFile", attrValue);

				string description = GetOptionalAttribute (xmlNode, "description");
				if (!String.IsNullOrEmpty (description))
					nvc.Add ("description", description);

				string name = MapUrl (attrValue);				
				var provider = new XmlSiteMapProvider ();
				provider.Initialize (name, nvc);
				
				SiteMapNode providerRoot = provider.GetRootNodeCore ();
				if (parent == null)
					root = providerRoot;
				else
					AddNodeNoCheck (providerRoot, parent);
				return;
			}

			SiteMapNode curNode = ConvertToSiteMapNode (xmlNode);
			if (parent == null)
				root = curNode;
			else
				AddNodeNoCheck (curNode, parent);
			
			XmlNodeList childNodes = xmlNode.ChildNodes;
			if (childNodes == null || childNodes.Count < 1)
				return;
			
			foreach (XmlNode child in childNodes) {
				if (child.NodeType != XmlNodeType.Element)
					continue;

				BuildSiteMapRecursive (child, curNode);
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

		protected override void Clear ()
		{
			base.Clear ();
			root = null;
			ChildProviders.Clear ();
			ChildProvidersPresent.Clear ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				foreach (FileSystemWatcher watcher in watchers)
					watcher.Dispose ();
				watchers = null;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
		}
		
		public override SiteMapNode FindSiteMapNode (string rawUrl)
		{
			SiteMapNode node = base.FindSiteMapNode (rawUrl);
			if (node != null)
				return node;

			node = RootNode;
			string url = MapUrl (rawUrl);
			if (node != null) {
				if (String.Compare (url, node.Url, RuntimeHelpers.StringComparison) == 0)
					return node;
			}
			
			foreach (SiteMapProvider smp in ChildProviders) {
				node = smp.FindSiteMapNode (url);
				if (node != null)
					return node;
			}

			return null;
		}

		public override SiteMapNode FindSiteMapNodeFromKey (string key)
		{
			SiteMapNode node = base.FindSiteMapNodeFromKey (key);
			if (node != null)
				return node;

			foreach (SiteMapProvider smp in ChildProviders) {
				node = smp.FindSiteMapNodeFromKey (key);
				if (node != null)
					return node;
			}

			return null;
		}

		public override void Initialize (string name, NameValueCollection attributes)
		{
			if (initialized)
				throw new InvalidOperationException ("XmlSiteMapProvider cannot be initialized twice.");

			initialized = true;
			if (attributes != null) {
				foreach (string key in attributes.AllKeys) {
					switch (key) {
						case "siteMapFile":
							fileVirtualPath = base.MapUrl (attributes ["siteMapFile"]);
							break;

						case "description":
						case "securityTrimmingEnabled":
							break;
							
						default:
							throw new ConfigurationErrorsException ("The attribute '" + key + "' is unexpected in the configuration of the '" + name + "' provider.");
					}
				}
			}
			
			base.Initialize (name, attributes != null ? attributes : new NameValueCollection ());
		}

		void CreateWatcher (string file)
		{
			var watcher = new FileSystemWatcher ();
			watcher.NotifyFilter |= NotifyFilters.Size;
			watcher.Path = Path.GetFullPath (Path.GetDirectoryName (file));
			watcher.Filter = Path.GetFileName (file);
			watcher.Changed += new FileSystemEventHandler (OnFileChanged);
			watcher.EnableRaisingEvents = true;

			if (watchers == null)
				watchers = new List <FileSystemWatcher> ();
			
			watchers.Add (watcher);
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

