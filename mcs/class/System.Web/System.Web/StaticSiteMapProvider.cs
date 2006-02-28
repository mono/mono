//
// System.Web.StaticSiteMapProvider.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
// (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Web
{
	public abstract class StaticSiteMapProvider : SiteMapProvider
	{
		Hashtable nodeToParent;
		Hashtable nodeToChildren;
		Hashtable urlToNode;
		Hashtable keyToNode;
		
		internal protected override void AddNode (SiteMapNode node, SiteMapNode parentNode)
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
				
				if (FindSiteMapNodeFromKey (node.Key) != null)
					throw new InvalidOperationException (string.Format ("A node with key {0} already exists.",node.Key));
				KeyToNode [node.Key] = node;
				
				if (parentNode != null) {
					NodeToParent [node] = parentNode;
					if (NodeToChildren [parentNode] == null)
						NodeToChildren [parentNode] = new SiteMapNodeCollection ();
					
					((SiteMapNodeCollection) NodeToChildren [parentNode]).Add (node);
				}
			}
		}
		
		Hashtable NodeToParent {
			get {
				lock (this) {
					if (nodeToParent == null)
						nodeToParent = new Hashtable ();
				}
				return nodeToParent;
			}
		}
		
		Hashtable NodeToChildren {
			get {
				lock (this) {
					if (nodeToChildren == null)
						nodeToChildren = new Hashtable ();
				}
				return nodeToChildren;
			}
		}
		
		Hashtable UrlToNode {
			get {
				lock (this) {
					if (urlToNode == null) {
#if NET_2_0
						urlToNode = new Hashtable (StringComparer.InvariantCultureIgnoreCase);
#else
						urlToNode = new Hashtable (
							CaseInsensitiveHashCodeProvider.DefaultInvariant,
							CaseInsensitiveComparer.DefaultInvariant
						);
#endif
					}
				}
				return urlToNode;
			}
		}
		
		Hashtable KeyToNode {
			get {
				lock (this) {
					if (keyToNode == null)
						keyToNode = new Hashtable ();
				}
				return keyToNode;
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
				if (keyToNode != null)
					keyToNode.Clear ();
			}
		}

		public override SiteMapNode FindSiteMapNode (string rawUrl)
		{
			if (rawUrl == null)
				throw new ArgumentNullException ("rawUrl");
			
			if (rawUrl.Length > 0) {
				this.BuildSiteMap();
				rawUrl = UrlUtils.ResolveVirtualPathFromAppAbsolute (rawUrl);
				SiteMapNode node = (SiteMapNode) UrlToNode [rawUrl];
				if (node != null && IsAccessibleToUser (HttpContext.Current, node))
					return node;
			}
			return null;
		}

		public override SiteMapNodeCollection GetChildNodes (SiteMapNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			
			this.BuildSiteMap();
			SiteMapNodeCollection col = (SiteMapNodeCollection) NodeToChildren [node];
			if (col == null) return SiteMapNodeCollection.EmptyCollection;
			
			SiteMapNodeCollection ret = null;
			for (int n=0; n<col.Count; n++) {
				if (!IsAccessibleToUser (HttpContext.Current, col[n])) {
					if (ret == null) {
						ret = new SiteMapNodeCollection ();
						for (int m=0; m<n; m++)
							ret.Add (col[m]);
					}
				} else if (ret != null)
					ret.Add (col[n]);
			}
			
			if (ret != null) {
				if (ret.Count > 0)
					return SiteMapNodeCollection.ReadOnly (ret);
			} else
				return SiteMapNodeCollection.ReadOnly (col);
			
			return null;
		}
		
		public override SiteMapNode GetParentNode (SiteMapNode node)
		{
			if (node == null)
				throw new ArgumentNullException ("node");
			this.BuildSiteMap();
			SiteMapNode parent = (SiteMapNode) NodeToParent [node];
			return parent != null && IsAccessibleToUser (HttpContext.Current, parent) ? parent : null;
		}
		
		protected override void RemoveNode (SiteMapNode node)
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
		
		public override SiteMapNode FindSiteMapNodeFromKey (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			SiteMapNode ret = (SiteMapNode) KeyToNode [key];
			return ret != null && IsAccessibleToUser (HttpContext.Current, ret) ? ret : null;
		}

		public abstract SiteMapNode BuildSiteMap ();
	}
}
#endif

