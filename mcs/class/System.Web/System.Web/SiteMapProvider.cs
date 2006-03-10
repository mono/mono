//
// System.Web.SiteMapProvider
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
using System.Text;
using System.Configuration.Provider;
using System.Web.Util;
using System.Globalization;

namespace System.Web {
	public abstract class SiteMapProvider : ProviderBase {
		
		bool enableLocalization;
		SiteMapProvider parentProvider;
		SiteMapProvider rootProviderCache;
		bool securityTrimming;
		object resolveLock = new Object();
		bool resolving;
		
		protected virtual void AddNode (SiteMapNode node)
		{
			AddNode (node, null);
		}
		
		internal protected virtual void AddNode (SiteMapNode node, SiteMapNode parentNode)
		{
			throw new NotImplementedException ();
		}

		public virtual SiteMapNode FindSiteMapNode (HttpContext context)
		{
			if (context == null)
				return null;
			
			SiteMapNode ret = this.FindSiteMapNode (context.Request.RawUrl);
			if (ret == null)
				ret = this.FindSiteMapNode (context.Request.Path);
			return ret;
		}

		public abstract SiteMapNode FindSiteMapNode (string rawUrl);
		
		public virtual SiteMapNode FindSiteMapNodeFromKey (string key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			
			return null;
		}

		public abstract SiteMapNodeCollection GetChildNodes (SiteMapNode node);
		
		public virtual SiteMapNode GetCurrentNodeAndHintAncestorNodes (int upLevel)
		{
			if (upLevel < -1) throw new ArgumentOutOfRangeException ("upLevel");

			return CurrentNode;
		}
		
		public virtual SiteMapNode GetCurrentNodeAndHintNeighborhoodNodes (int upLevel, int downLevel)
		{
			if (upLevel < -1) throw new ArgumentOutOfRangeException ("upLevel");
			if (downLevel < -1) throw new ArgumentOutOfRangeException ("downLevel");
			
			return CurrentNode;
		}

		public abstract SiteMapNode GetParentNode (SiteMapNode node);
		
		public virtual SiteMapNode GetParentNodeRelativeToCurrentNodeAndHintDownFromParent (int walkupLevels, int relativeDepthFromWalkup)
		{
			if (walkupLevels < 0) throw new ArgumentOutOfRangeException ("walkupLevels");
			if (relativeDepthFromWalkup < 0) throw new ArgumentOutOfRangeException ("relativeDepthFromWalkup");
			
			SiteMapNode node = GetCurrentNodeAndHintAncestorNodes (walkupLevels);
			for (int n=0; n<walkupLevels && node != null; n++)
				node = GetParentNode (node);
				
			if (node == null) return null;

			HintNeighborhoodNodes (node, 0, relativeDepthFromWalkup);
			return node;
		}
		
		public virtual SiteMapNode GetParentNodeRelativeToNodeAndHintDownFromParent (SiteMapNode node, int walkupLevels, int relativeDepthFromWalkup)
		{
			if (walkupLevels < 0) throw new ArgumentOutOfRangeException ("walkupLevels");
			if (relativeDepthFromWalkup < 0) throw new ArgumentOutOfRangeException ("relativeDepthFromWalkup");
			if (node == null) throw new ArgumentNullException ("node");
			
			HintAncestorNodes (node, walkupLevels);
			for (int n=0; n<walkupLevels && node != null; n++)
				node = GetParentNode (node);
				
			if (node == null) return null;
			
			HintNeighborhoodNodes (node, 0, relativeDepthFromWalkup);
			return node;
		}
		
		protected internal abstract SiteMapNode GetRootNodeCore ();
		
		protected static SiteMapNode GetRootNodeCoreFromProvider (SiteMapProvider provider)
		{
			return provider.GetRootNodeCore ();
		}
		
		public virtual void HintAncestorNodes (SiteMapNode node, int upLevel)
		{
			if (upLevel < -1) throw new ArgumentOutOfRangeException ("upLevel");
			if (node == null) throw new ArgumentNullException ("node");
		}
		
		public virtual void HintNeighborhoodNodes (SiteMapNode node, int upLevel, int downLevel)
		{
			if (upLevel < -1) throw new ArgumentOutOfRangeException ("upLevel");
			if (downLevel < -1) throw new ArgumentOutOfRangeException ("downLevel");
			if (node == null) throw new ArgumentNullException ("node");
		}
		
		protected virtual void RemoveNode (SiteMapNode node)
		{
			throw new NotImplementedException ();
		}

		public override void Initialize (string name, NameValueCollection attributes)
		{
			base.Initialize (name, attributes);
			if (attributes["securityTrimmingEnabled"] != null)
				securityTrimming = (bool) Convert.ChangeType (attributes ["securityTrimmingEnabled"], typeof(bool));
		}
		
		[MonoTODO]
		public virtual bool IsAccessibleToUser (HttpContext context, SiteMapNode node)
		{
			if (context == null) throw new ArgumentNullException ("context");
			if (node == null) throw new ArgumentNullException ("node");

			return true;
		}
		
		public virtual SiteMapNode CurrentNode {
			get {
				if (HttpContext.Current != null) {
					SiteMapNode ret = ResolveSiteMapNode (HttpContext.Current);
					if (ret != null) return ret;
					return FindSiteMapNode (HttpContext.Current);
				} else
					return null;
			}
		}
		
		public virtual SiteMapProvider ParentProvider {
			get { return parentProvider; }
			set { parentProvider = value; }
		}
		
		public virtual SiteMapProvider RootProvider {
			get {
				lock (this) {
					if (rootProviderCache == null) {
						SiteMapProvider current = this;
						while (current.ParentProvider != null)
							current = current.ParentProvider;
						
						rootProviderCache = current;
					}
				}
				return rootProviderCache;
			}
		}
		
		protected SiteMapNode ResolveSiteMapNode (HttpContext context)
		{
			SiteMapResolveEventArgs args = new SiteMapResolveEventArgs (context, this);
			if (SiteMapResolve != null) {
				lock (resolveLock) {
					if (resolving) return null;
					resolving = true;
					SiteMapNode r = SiteMapResolve (this, args);
					resolving = false;
					return r;
				}
			}
			else
				return null;
		}
		
		public bool EnableLocalization {
			get { return enableLocalization; }
			set { enableLocalization = value; }
		}
		
		public bool SecurityTrimmingEnabled {
			get { return securityTrimming; }
		}

		string resourceKey;
		public string ResourceKey {
			get { return resourceKey; }
			set { resourceKey = value; }
		}

		public virtual SiteMapNode RootNode {
			get {
				SiteMapNode node = GetRootNodeCore ();
				if (IsAccessibleToUser (HttpContext.Current, node))
					return node;
				else
					return null;
			}
		}

		public event SiteMapResolveEventHandler SiteMapResolve;
	}
}
#endif

