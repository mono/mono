//
// System.Web.SiteMapProvider
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
using System.Configuration.Provider;
using System.Web.Util;
using System.Globalization;

namespace System.Web {
	public abstract class SiteMapProvider : ProviderBase {
		
		bool enableLocalization;
		
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
					
					
						if (UrlUtils.IsRelativeUrl (url))
							url = UrlUtils.Combine (HttpRuntime.AppDomainAppVirtualPath, url);
						else
							url = UrlUtils.ResolveVirtualPathFromAppAbsolute (url);
						
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
								new CaseInsensitiveHashCodeProvider (),
								new CaseInsensitiveComparer ()
							);
						}
					}
				}
				return urlToNode;
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

		public override void Initialize (string name, NameValueCollection attributes)
		{ 
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
		
		SiteMapProvider parentProvider;
		public virtual SiteMapProvider ParentProvider {
			get { return parentProvider; }
			set { parentProvider = value; }
		}
		
		SiteMapProvider rootProviderCache;
		public virtual SiteMapProvider RootProvider {
			get {
				if (rootProviderCache == null) {
					lock (this) {
						if (rootProviderCache == null) {
							SiteMapProvider current = this;
							while (current.ParentProvider != null)
								current = current.ParentProvider;
							
							rootProviderCache = current;
						}
					}
				}
				return rootProviderCache;
			}
		}
		
		public bool EnableLocalization {
			get { return enableLocalization; }
			set { enableLocalization = value; }
		}

		public abstract SiteMapNode BuildSiteMap ();
		public abstract SiteMapNode RootNode { get; }
	
	}
}
#endif

