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

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace System.Web {
	public class SiteMapNode : IHierarchyData, INavigateUIData, ICloneable {
	
		private SiteMapNode () {}
		
		public SiteMapNode (SiteMapProvider provider, string key) : this (provider, key, null, null, null, null, null, null) {}
		public SiteMapNode (SiteMapProvider provider, string key, string url) : this (provider, key, url, null, null, null, null, null) {}
		public SiteMapNode (SiteMapProvider provider, string key, string url, string title) : this (provider, key, url, title, null, null, null, null) {}
		public SiteMapNode (SiteMapProvider provider, string key, string url, string title, string description) : this (provider, key, url, title, description, null, null, null) {}
		public SiteMapNode (SiteMapProvider provider, string key, string url, string title, string description, IList roles, NameValueCollection attributes, NameValueCollection resourceKeys)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");
			
			this.provider = provider;
			this.key = key;
			this.url = url;
			this.title = title;
			this.description = description;
			this.roles = roles;
			this.attributes = attributes;
			this.resourceKeys = resourceKeys;
		}

		public SiteMapDataSourceView GetDataSourceView (SiteMapDataSource owner, string viewName)
		{
			return new SiteMapDataSourceView (owner, viewName, this);
		}
		
		public SiteMapHierarchicalDataSourceView GetHierarchicalDataSourceView ()
		{
			return new SiteMapHierarchicalDataSourceView (this);
		}
		
		[MonoTODO]
		public bool IsAccessibleToUser (System.Web.HttpContext ctx)
		{
			throw new NotImplementedException ();
		}
		
		public override string ToString()
		{
			return Title;
		}

		public virtual bool HasChildNodes { get { return ChildNodes != null && ChildNodes.Count != 0; } }

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
			
			if (childNodes.Count > 0) {
				childNodes.AddRange (childNodes);
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
		
		[MonoTODO ("set")]
		public virtual SiteMapNodeCollection ChildNodes {
			get { return provider.GetChildNodes (this); } 
			set { CheckWritable (); }
		}

		public virtual SiteMapNode RootNode { get { return provider.RootProvider.RootNode; }  }
		
		SiteMapNodeCollection SiblingNodes {
			get {
				if (ParentNode != null)
					return ParentNode.ChildNodes;
				
				return null;
			}
		}
		
		[MonoTODO]
		protected string GetExplicitResourceString (string attributeName, bool b)
		{
			return null;
		}
		
		[MonoTODO]
		protected string GetImplicitResourceString (string attributeName)
		{
			return null;
		}
		
		[MonoTODO ("resource string?")]
		public string this [string key]
		{
			get {
				string val = null;
				if (provider.EnableLocalization) {
					val = GetExplicitResourceString (key, true);
					if (val == null) val = GetImplicitResourceString (key);
				}
				if (val != null) return null;
				if (attributes != null) return attributes [key];
				return null;
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
			node.roles = new ArrayList (roles);
			node.attributes = new NameValueCollection (attributes);
			if (cloneParentNodes && ParentNode != null)
				node.parent = (SiteMapNode) ParentNode.Clone (true);
			return node;
		}
		
		public override bool Equals (object ob)
		{
			SiteMapNode node = ob as SiteMapNode;
			if (node == null) return false;
			
			if (node.key != key ||
					node.url != url ||
					node.title != title ||
					node.description != description) {
				return false;
			}
			
			if ((roles == null || node.roles == null) && (roles != node.roles)) return false;
			if (roles.Count != node.roles.Count) return false;

			foreach (object role in roles)
				if (!node.roles.Contains (role)) return false;
				
			if ((attributes == null || node.attributes == null) && (attributes != node.attributes)) return false;
			if (attributes.Count != node.attributes.Count) return false;

			foreach (string k in attributes)
				if (attributes [k] != node.attributes [k]) return false;

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
		
		public virtual NameValueCollection Attributes {
			get { return attributes; } 
			set { CheckWritable (); attributes = value; }
		}
		
		public virtual string Description {
			get { return description != null ? description : ""; }
			set { CheckWritable (); description = value; }
		}
		
		[LocalizableAttribute (true)]
		public virtual string Title {
			get { return title != null ? title : ""; }
			set { CheckWritable (); title = value; }
		}
		
		public virtual string Url {
			get { return url != null ? url : ""; }
			set { CheckWritable (); url = value; }
		}
		
		public virtual IList Roles {
			get { return roles; }
			set { CheckWritable (); roles = value; }
		}
		
		public bool ReadOnly {
			get { return readOnly; }
			set { readOnly = value; }
		}
		
		[MonoTODO ("Do somethig with this")]
		public string ResourceKey {
			get { return resourceKey; }
			set { resourceKey = value; }
		}
		
		public string Key { get { return key; } }
		public SiteMapProvider Provider { get { return provider; } }
		
		#endregion
		
		#region INavigateUIData
		IHierarchicalEnumerable System.Web.UI.IHierarchyData.GetChildren () { return ChildNodes; }
		IHierarchicalEnumerable System.Web.UI.IHierarchyData.GetParent ()
		{
			if (ParentNode == null) return null; 
			return SiteMapNodeCollection.ReadOnly (new SiteMapNodeCollection (ParentNode));
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
		#endregion
		
	}
}
#endif

