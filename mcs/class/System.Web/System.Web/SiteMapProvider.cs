//
// System.Web.SiteMapProvider
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
using System.Configuration.Provider;
using System.Web.Util;
using System.Globalization;

namespace System.Web {
	public abstract class SiteMapProvider : ISiteMapProvider, IProvider {
		
		public void AddNode (SiteMapNode node)
		{
			AddNode (node, null);
		}
		
		public void AddNode (SiteMapNode node, SiteMapNode parentNode)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			
			lock (this) {
				string url = node.Url;
				if (url != null && url.Length > 0) {
					
						url = UrlUtils.ResolveVirtualPathFromAppAbsolute (url);
						if (UrlUtils.IsRooted (url))
							url = UrlUtils.Combine (HttpRuntime.AppDomainAppVirtualPath, url);
						
						if (FindSiteMapNode (url) != null)
							throw new InvalidOperationException ();
					
					UrlToNode [url] = node;
				}
				
				if (parentNode != null) {
					NodeToParent [node] = parentNode;
					if (NodeToChildren [parentNode] == null)
						NodeToChildren [parentNode] = new SiteMapNodeCollection ();
					
					((SiteMapNodeCollection) NodeToChildren [parentNode]).Add (node);
				}
			}
		}
		
		Hashtable nodeToParent;
		Hashtable NodeToParent {
			get {
				if (nodeToParent == null) {
					lock (this) {
						if (nodeToParent == null)
							nodeToParent = new Hashtable ();
					}
				}
				return nodeToParent;
			}
		}
		
		Hashtable nodeToChildren;
		Hashtable NodeToChildren {
			get {
				if (nodeToChildren == null) {
					lock (this) {
						if (nodeToChildren == null)
							nodeToChildren = new Hashtable ();
					}
				}
				return nodeToChildren;
			}
		}
		
		Hashtable urlToNode;
		Hashtable UrlToNode {
			get {
				if (urlToNode == null) {
					lock (this) {
						if (urlToNode == null) {
							urlToNode = new Hashtable (
								new CaseInsensitiveHashCodeProvider (CultureInfo.InvariantCulture),
								new CaseInsensitiveComparer (CultureInfo.InvariantCulture)
							);
						}
					}
				}
				return nodeToChildren;
			}
		}
		
		protected virtual void Clear ()
		{
			lock (this) {
				if (urlToNode != null)
					urlToNode.Clear ();
				if (nodeToChildren != null)
					nodeToChildren.Clear ();
				if (nodeToParent != null)
					nodeToParent.Clear ();
			}
		}

		public virtual SiteMapNode FindSiteMapNode (string rawUrl)
		{
			if (rawUrl == null)
				throw new ArgumentNullException ("rawUrl");
			
			if (rawUrl.Length > 0) {
				this.BuildSiteMap();
				rawUrl = UrlUtils.ResolveVirtualPathFromAppAbsolute (rawUrl);
				return (SiteMapNode) UrlToNode [rawUrl];
			}
			return null;
		}
		
		public virtual SiteMapNodeCollection GetChildNodes (SiteMapNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			
			this.BuildSiteMap();
			SiteMapNodeCollection ret = (SiteMapNodeCollection) NodeToChildren [node];
			
			if (ret != null)
				return SiteMapNodeCollection.ReadOnly (ret);
			
			return null;
		}
		
		public virtual SiteMapNode GetParentNode(SiteMapNode node) {
			if (node == null)
				throw new ArgumentNullException ("node");
			this.BuildSiteMap();
			return (SiteMapNode) NodeToParent [node];
		}
		
		public void RemoveNode (SiteMapNode node)
		{
	
			if (node == null)
				throw new ArgumentNullException("node");
			
			lock (this) {
				SiteMapNode parent = (SiteMapNode) NodeToParent [node];
				if (NodeToParent.Contains (node))
					NodeToParent.Remove (node);
				
				if (node.Url != null && node.Url.Length > 0 && UrlToNode.Contains (node.Url))
					UrlToNode.Remove (node.Url);
				
				if (parent != null) {
					SiteMapNodeCollection siblings = (SiteMapNodeCollection) NodeToChildren [node];
					if (siblings != null && siblings.Contains (node))
						siblings.Remove (node);
				}
			}
		}

		public virtual void Initialize (string name, NameValueCollection attributes)
		{ 
			this.name = name;
			if (attributes != null)
				description = attributes ["description"];
		
		}
		
		public virtual SiteMapNode CurrentNode {
			get {
				SiteMapNode ret;
				
				if (HttpContext.Current != null) {
					ret = this.FindSiteMapNode (HttpContext.Current.Request.RawUrl);
					if (ret == null)
						ret = this.FindSiteMapNode (HttpContext.Current.Request.Path);

					return ret;
				}
				
				return null;
			}
		}
		
		string description;
		public virtual string Description {
			get { return description != null ? description : "SiteMapProvider"; }
		}
		
		string name;
		public virtual string Name {
			get { return name; }
		}
		
		ISiteMapProvider parentProvider;
		public virtual ISiteMapProvider ParentProvider {
			get { return parentProvider; }
			set { parentProvider = value; }
		}
		
		ISiteMapProvider rootProviderCache;
		public virtual ISiteMapProvider RootProvider {
			get {
				if (rootProviderCache == null) {
					lock (this) {
						if (rootProviderCache == null) {
							ISiteMapProvider current = this;
							while (current.ParentProvider != null)
								current = current.ParentProvider;
							
							rootProviderCache = current;
						}
					}
				}
				return rootProviderCache;
			}
		}

		public abstract SiteMapNode BuildSiteMap ();
		public abstract SiteMapNode RootNode { get; }
	
	}
}
#endif

