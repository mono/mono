//
// System.Web.SiteMapNode
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web {
	public class SiteMapNode : IHierarchyData, INavigateUIData {
		public SiteMapNode (ISiteMapProvider provider) : this (provider, null, null, null, null, null, null) {}
		public SiteMapNode (ISiteMapProvider provider, string url) : this (provider, url, null, null, null, null, null) {}
		public SiteMapNode (ISiteMapProvider provider, string url, string title) : this (provider, url, title, null, null, null, null) {}
		public SiteMapNode (ISiteMapProvider provider, string url, string title, string description) : this (provider, url, title, description, null, null, null) {}
		public SiteMapNode (ISiteMapProvider provider, string url, string title, string description, IList keywords) : this (provider, url, title, description, keywords, null, null) {}
		public SiteMapNode (ISiteMapProvider provider, string url, string title, string description, IList keywords, IList roles) : this (provider, url, title, description, keywords, roles, null) {}
		public SiteMapNode (ISiteMapProvider provider, string url, string title, string description, IList keywords, IList roles, NameValueCollection attributes)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");
			
			this.provider = provider;
			this.url = url;
			this.title = title;
			this.description = description;
			this.keywords = keywords;
			this.roles = roles;
			this.attributes = attributes;
		}

		public SiteMapDataSourceView GetDataSourceView ()
		{
			return new SiteMapDataSourceView (this);
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
				ISiteMapProvider provider = this.provider;
				
				do {
					SiteMapNode n = provider.GetParentNode (this);
					if (n != null)
						return n; 
					
					provider = provider.ParentProvider;
				} while (provider != null);
				return null;
			}
		}
		public virtual SiteMapNodeCollection ChildNodes { get { return provider.GetChildNodes (this); } }

		public virtual SiteMapNode RootNode { get { return provider.RootProvider.RootNode; }  }
		
		SiteMapNodeCollection SiblingNodes {
			get {
				if (ParentNode != null)
					return ParentNode.ChildNodes;
				
				return null;
			}
		}
		
		#region Field Accessors
		public virtual NameValueCollection Attributes { get { return attributes; } }
		public virtual string Description { get { return description != null ? description : ""; } }
		public virtual IList Keywords { get { return keywords; } }
		public virtual string Title { get { return title != null ? title : ""; } }
		public virtual string Url { get { return url != null ? url : ""; } }
		public virtual IList Roles { get { return roles; } }
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
		ISiteMapProvider provider;
		string url;
		string title;
		string description;
		IList keywords;
		IList roles;
		NameValueCollection attributes;
		#endregion
		
	}
}
#endif

