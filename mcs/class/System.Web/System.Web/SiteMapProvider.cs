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
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Configuration.Provider;
using System.Web.Util;
using System.Globalization;
using System.Web.Configuration;

namespace System.Web {
	public abstract class SiteMapProvider : ProviderBase {
		static readonly object siteMapResolveEvent = new object ();
		
		internal object this_lock = new object ();
		
		bool enableLocalization;
		SiteMapProvider parentProvider;
		SiteMapProvider rootProviderCache;
		bool securityTrimming;
		object resolveLock = new Object();
		bool resolving;

		EventHandlerList events = new EventHandlerList ();
		
		public event SiteMapResolveEventHandler SiteMapResolve {
			add { events.AddHandler (siteMapResolveEvent, value); }
			remove { events.RemoveHandler (siteMapResolveEvent, value); }
		}
		
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

			HttpRequest req = context.Request;
			if (req == null)
				return null;
			
			SiteMapNode ret = this.FindSiteMapNode (req.RawUrl);
			if (ret == null)
				ret = this.FindSiteMapNode (req.Path);
			return ret;
		}

		public abstract SiteMapNode FindSiteMapNode (string rawUrl);
		
		public virtual SiteMapNode FindSiteMapNodeFromKey (string key)
		{
			/* msdn2 says this function always returns
			 * null, but it seems to just call
			 * FindSiteMapNode(string rawUrl) */
			return FindSiteMapNode (key);
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
			if (attributes ["securityTrimmingEnabled"] != null)
				securityTrimming = (bool) Convert.ChangeType (attributes ["securityTrimmingEnabled"], typeof (bool));
		}
		
		[MonoTODO ("need to implement cases 2 and 3")]
		public virtual bool IsAccessibleToUser (HttpContext context, SiteMapNode node)
		{
			if (context == null) throw new ArgumentNullException ("context");
			if (node == null) throw new ArgumentNullException ("node");

			if (!SecurityTrimmingEnabled)
				return true;

			/* The node is accessible (according to msdn2) if:
			 *
			 * 1. The Roles exists on node and the current user is in at least one of the specified roles.
			 *
			 * 2. The current thread has an associated WindowsIdentity that has file access to the requested URL and
			 * the URL is located within the directory structure for the application.
			 *
			 * 3. The current user is authorized specifically for the requested URL in the authorization element for
			 * the current application and the URL is located within the directory structure for the application. 
			*/

			/* 1. */
			IList roles = node.Roles;
			if (roles != null && roles.Count > 0) {
				foreach (string rolename in roles)
					if (rolename == "*" || context.User.IsInRole (rolename))
						return true;
			}
			
			/* 2. */
			/* XXX */

			/* 3. */
			string url = node.Url;
			if(!String.IsNullOrEmpty(url)) {
				// TODO check url is located within the current application

				if (VirtualPathUtility.IsAppRelative (url) || !VirtualPathUtility.IsAbsolute (url))
					url = VirtualPathUtility.Combine (VirtualPathUtility.AppendTrailingSlash (HttpRuntime.AppDomainAppVirtualPath), url);

				AuthorizationSection config = (AuthorizationSection) WebConfigurationManager.GetSection (
					"system.web/authorization",
					url);
				if (config != null)
					return config.IsValidUser (context.User, context.Request.HttpMethod);
			}

			return false;
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
				lock (this_lock) {
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
			SiteMapResolveEventHandler eh = events [siteMapResolveEvent] as SiteMapResolveEventHandler;

			if (eh != null) {
				lock (resolveLock) {
					if (resolving)
						return null;
					resolving = true;
					SiteMapResolveEventArgs args = new SiteMapResolveEventArgs (context, this);
					SiteMapNode r = eh (this, args);
					resolving = false;
					return r;
				}
			} else
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
				return ReturnNodeIfAccessible (node);
			}
		}

		internal static SiteMapNode ReturnNodeIfAccessible (SiteMapNode node)
		{
			if (node.IsAccessibleToUser (HttpContext.Current))
				return node;
			else
				throw new InvalidOperationException (); /* need
									 * a
									 * message
									 * here */
		}
	}
}
#endif

