//
// System.Web.SiteMapNode
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Resources;
using System.Security.Principal;

namespace System.Web {
	public class SiteMapNode : IHierarchyData, INavigateUIData, ICloneable {
	
		SiteMapNode () {}
		
		public SiteMapNode (SiteMapProvider provider, string key)
			: this (provider, key, null, null, null, null, null, null, null) {}
		public SiteMapNode (SiteMapProvider provider, string key, string url)
			: this (provider, key, url, null, null, null, null, null, null) {}
		public SiteMapNode (SiteMapProvider provider, string key, string url, string title)
			: this (provider, key, url, title, null, null, null, null, null) {}
		public SiteMapNode (SiteMapProvider provider, string key, string url, string title, string description)
			: this (provider, key, url, title, description, null, null, null, null) {}
		
		public SiteMapNode (SiteMapProvider provider, string key, string url, string title, string description,
				    IList roles, NameValueCollection attributes, NameValueCollection explicitResourceKeys,
				    string implicitResourceKey)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");
			if (key == null)
				throw new ArgumentNullException ("key");
			
			this.provider = provider;
			this.key = key;
			this.url = url;
			this.title = title;
			this.description = description;
			this.roles = roles;
			this.attributes = attributes;
			this.resourceKeys = explicitResourceKeys;
			this.resourceKey = implicitResourceKey;
		}

		public SiteMapDataSourceView GetDataSourceView (SiteMapDataSource owner, string viewName)
		{
			return new SiteMapDataSourceView (owner, viewName, this);
		}
		
		public SiteMapHierarchicalDataSourceView GetHierarchicalDataSourceView ()
		{
			return new SiteMapHierarchicalDataSourceView (this);
		}
		
		public virtual bool IsAccessibleToUser (System.Web.HttpContext context)
		{
			return provider.IsAccessibleToUser (context, this);
		}
		
		public override string ToString()
		{
			return Title;
		}

		public virtual bool HasChildNodes {
			get {
				SiteMapNodeCollection childNodes = ChildNodes;
				return childNodes != null && childNodes.Count > 0;
			}
		}

		public SiteMapNodeCollection GetAllNodes ()
		{
			SiteMapNodeCollection ret;
		
			ret = new SiteMapNodeCollection ();
			GetAllNodesRecursive (ret);
			return SiteMapNodeCollection.ReadOnly (ret);
		}
		
		void GetAllNodesRecursive(SiteMapNodeCollection c)
		{
			SiteMapNodeCollection childNodes = this.ChildNodes;

			if (childNodes != null && childNodes.Count > 0) {
				c.AddRange (childNodes);
				foreach (SiteMapNode n in childNodes)
					n.GetAllNodesRecursive (c);
			}
		}

		
		public virtual bool IsDescendantOf (SiteMapNode node)
		{
			for (SiteMapNode n = ParentNode; n != null; n = n.ParentNode)
				if (n == node) return true; 

			return false; 
		}
		
		public virtual SiteMapNode NextSibling {
			get {
				IList siblings = this.SiblingNodes;
				if (siblings == null)
					return null; 
				
				int pos = siblings.IndexOf (this);
				if (pos >= 0 && pos < siblings.Count - 1)
					return (SiteMapNode) siblings [pos + 1]; 
				
				return null; 
			}
		}
		
		public virtual SiteMapNode PreviousSibling {
			get {
				IList siblings = this.SiblingNodes;
				if (siblings == null)
					return null; 
				
				int pos = siblings.IndexOf (this);
				if (pos > 0 && pos < siblings.Count)
					return (SiteMapNode) siblings [pos - 1]; 
				
				return null; 
			}
		}
		
		public virtual SiteMapNode ParentNode {
			get {
				if (parent != null) return parent;
				
				SiteMapProvider provider = this.provider;
				
				do {
					parent = provider.GetParentNode (this);
					if (parent != null)
						return parent; 
					
					provider = provider.ParentProvider;
				} while (provider != null);
				return null;
			}
			set {
				CheckWritable ();
				parent = value;
			}
		}

		public virtual SiteMapNodeCollection ChildNodes {
			get {
				if (provider.SecurityTrimmingEnabled) {
					IPrincipal p = HttpContext.Current.User;
					if ((user == null && user != p) || user != null && user != p) {
						user = p;
						childNodes = provider.GetChildNodes (this);
					}
				} else if (childNodes == null) {
					childNodes = provider.GetChildNodes (this);
				}
				return childNodes;
			} 
			set {
				CheckWritable ();
				user = null;
				childNodes = value;
			}
		}

		public virtual SiteMapNode RootNode { get { return provider.RootProvider.RootNode; }  }
		
		SiteMapNodeCollection SiblingNodes {
			get {
				if (ParentNode != null)
					return ParentNode.ChildNodes;
				
				return null;
			}
		}
		
		protected string GetExplicitResourceString (string attributeName, string defaultValue, bool throwIfNotFound)
		{
			if (attributeName == null)
				throw new ArgumentNullException ("attributeName");
			
			if (resourceKeys != null){
				string[] values = resourceKeys.GetValues (attributeName);
				if (values != null && values.Length == 2) {
					try {
						object o = HttpContext.GetGlobalResourceObject (values [0], values [1]);
						if (o is string)
							return (string) o;
					}
					catch (MissingManifestResourceException) {
					}

					if (throwIfNotFound && defaultValue == null)
						throw new InvalidOperationException (String.Format ("The resource object with classname '{0}' and key '{1}' was not found.", values [0], values [1]));
				}
			}

			return defaultValue;
		}

		protected string GetImplicitResourceString (string attributeName)
		{
			if (attributeName == null)
				throw new ArgumentNullException ("attributeName");

			string resourceKey = ResourceKey;
			if (String.IsNullOrEmpty (resourceKey))
				return null;

			try {
				object o = HttpContext.GetGlobalResourceObject (provider.ResourceKey, resourceKey + "." + attributeName);
				if (o is string)
					return (string) o;
			} catch (MissingManifestResourceException) {
			}
			
			return null;
		}
		
		public virtual string this [string key]
		{
			get {
				if (provider.EnableLocalization) {
					string val = GetImplicitResourceString (key);
					if (val == null)
						val = GetExplicitResourceString (key, null, true);
					if (val != null)
						return val;
				}
				if (attributes != null) return attributes [key];
				return null;
			}
			set {
				CheckWritable ();
				if (attributes == null) attributes = new NameValueCollection ();
				attributes [key] = value;
			}
		}
		
		object ICloneable.Clone ()
		{
			return Clone (false);
		}
		
		public virtual SiteMapNode Clone ()
		{
			return Clone (false);
		}
		
		public virtual SiteMapNode Clone (bool cloneParentNodes)
		{
			SiteMapNode node = new SiteMapNode ();
			node.provider = provider;
			node.key = key;
			node.url = url;
			node.title = title;
			node.description = description;
			if (roles != null)
				node.roles = new ArrayList (roles);
			if (attributes != null)
				node.attributes = new NameValueCollection (attributes);
			if (cloneParentNodes && ParentNode != null)
				node.parent = (SiteMapNode) ParentNode.Clone (true);
			return node;
		}
				
		public override bool Equals (object obj)
		{
			SiteMapNode node = obj as SiteMapNode;
			if (node == null) return false;
			
			if (node.key != key ||
					node.url != url ||
					node.title != title ||
					node.description != description) {
				return false;
			}

			if (roles == null || node.roles == null) {
				if (roles != node.roles)
					return false;
			}
			else {
				if (roles.Count != node.roles.Count)
					return false;

				foreach (object role in roles)
					if (!node.roles.Contains (role)) return false;
			}
			if (attributes == null || node.attributes == null) {
				if (attributes != node.attributes)
					return false;
			}
			else {
				if (attributes.Count != node.attributes.Count)
					return false;

				foreach (string k in attributes)
					if (attributes[k] != node.attributes[k])
						return false;
			}
			return true;
		}
		
		public override int GetHashCode ()
		{
			return (key + url + title + description).GetHashCode ();
		}
		
		void CheckWritable ()
		{
			if (readOnly)
				throw new InvalidOperationException ("Can't modify read-only node");
		}
				
		#region Field Accessors
		
		protected NameValueCollection Attributes {
			get { return attributes; } 
			set { CheckWritable (); attributes = value; }
		}
		
		[Localizable (true)]
		public virtual string Description {
			get {
				string ret = null;
				
				if (provider.EnableLocalization) {
					ret = GetImplicitResourceString ("description");
					if (ret == null)
						ret = GetExplicitResourceString ("description", description, true);
				} else
					ret = description;
				
				return ret != null ? ret : String.Empty;
			}
			set { CheckWritable (); description = value; }
		}
		
		[LocalizableAttribute (true)]
		public virtual string Title {
			get {
				string ret = null;

				if (provider.EnableLocalization) {
					ret = GetImplicitResourceString ("title");
					if (ret == null)
						ret = GetExplicitResourceString ("title", title, true);
				} else
					ret = title;
				
				return ret != null ? ret : String.Empty;
			}
			set { CheckWritable (); title = value; }
		}
		
		public virtual string Url {
			get { return url != null ? url : ""; }
			set { CheckWritable (); url = value; }
		}
		
		public IList Roles {
			get { return roles; }
			set { CheckWritable (); roles = value; }
		}
		
		public bool ReadOnly {
			get { return readOnly; }
			set { readOnly = value; }
		}
		
		public string ResourceKey {
			get { return resourceKey; }
			set {
				if (ReadOnly)
					throw new InvalidOperationException ("The node is read-only.");
				resourceKey = value;
			}
		}
		
		public string Key { get { return key; } }
		public SiteMapProvider Provider { get { return provider; } }
		
		#endregion
		
		#region INavigateUIData
		IHierarchicalEnumerable System.Web.UI.IHierarchyData.GetChildren () { return ChildNodes; }
		IHierarchyData System.Web.UI.IHierarchyData.GetParent ()
		{
			return ParentNode;
		}

		bool System.Web.UI.IHierarchyData.HasChildren { get { return HasChildNodes; } }
		object System.Web.UI.IHierarchyData.Item { get { return this; } }
		string System.Web.UI.IHierarchyData.Path { get { return Url; } }
		string System.Web.UI.IHierarchyData.Type { get { return "SiteMapNode"; } }
		#endregion
		
		#region INavigateUIData
		string INavigateUIData.Name { get { return Title; }  }
		string INavigateUIData.NavigateUrl { get { return Url; } }
		string INavigateUIData.Value { get { return Title; } }
		#endregion

		#region Fields
		SiteMapProvider provider;
		string key;
		string url;
		string title;
		string description;
		IList roles;
		NameValueCollection attributes;
		NameValueCollection resourceKeys;
		bool readOnly;
		string resourceKey;
		SiteMapNode parent;
		SiteMapNodeCollection childNodes;
		IPrincipal user;
		#endregion
		
	}
}


