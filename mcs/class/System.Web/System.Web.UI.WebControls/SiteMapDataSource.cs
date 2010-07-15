//
// System.Web.UI.WebControls.SiteMapDataSource.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Drawing;
using System.Web.UI;
using System.Web.Util;
using System.ComponentModel;
using System.Collections.Generic;

namespace System.Web.UI.WebControls
{
	[PersistChildrenAttribute (false)]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.SiteMapDataSourceDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildrenAttribute (true)]
	[ToolboxBitmap ("")]
	public class SiteMapDataSource : HierarchicalDataSourceControl, IDataSource, IListSource
	{
		static string[] emptyNames = new string[] { "DefaultView" };
		
		SiteMapProvider provider;
		
		public virtual ICollection GetViewNames ()
		{
			return emptyNames;
		}

		IList IListSource.GetList () {
			return GetList ();
		}

		bool IListSource.ContainsListCollection	{
			get { return ContainsListCollection; }
		}

		public virtual IList GetList ()
		{
			return ListSourceHelper.GetList (this);
		}
		
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual bool ContainsListCollection {
			get { return ListSourceHelper.ContainsListCollection (this); }
		}

		DataSourceView IDataSource.GetView (string viewName) {
			return GetView (viewName);
		}

		ICollection IDataSource.GetViewNames () {
			return GetViewNames ();
		}
		
		event EventHandler IDataSource.DataSourceChanged {
			add { ((IHierarchicalDataSource)this).DataSourceChanged += value; }
			remove { ((IHierarchicalDataSource)this).DataSourceChanged -= value; }
		}
		
		[BrowsableAttribute (false)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public SiteMapProvider Provider {
			get {
				if (provider == null) {
					if (this.SiteMapProvider.Length == 0) {
						provider = SiteMap.Provider;
						if (provider == null)
							throw new HttpException ("There is no default provider configured for the site.");
					} else {
						provider = SiteMap.Providers [this.SiteMapProvider];
						if (provider == null)
							throw new HttpException ("SiteMap provider '" + this.SiteMapProvider + "' not found.");
					}
				}
				return provider;
			}
			set {
				provider = value;
				OnDataSourceChanged (EventArgs.Empty);
			}
		}
		
		[DefaultValueAttribute ("")]
		public virtual string SiteMapProvider {
			get { return ViewState.GetString ("SiteMapProvider", ""); }
			set {
				ViewState ["SiteMapProvider"] = value;
				OnDataSourceChanged (EventArgs.Empty);
			}
		}
		
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		public virtual string StartingNodeUrl {
			get { return ViewState.GetString ("StartingNodeUrl", ""); }
			set {
				ViewState ["StartingNodeUrl"] = value;
				OnDataSourceChanged (EventArgs.Empty);
			}
		}

		[DefaultValue (0)]
		public virtual int StartingNodeOffset {
			get { return ViewState.GetInt ("StartingNodeOffset", 0); }
			set {
				ViewState ["StartingNodeOffset"] = value;
				OnDataSourceChanged (EventArgs.Empty);
			}
		}
		
		[DefaultValueAttribute (false)]
		public virtual bool StartFromCurrentNode {
			get { return ViewState.GetBool ("StartFromCurrentNode", false); }
			set {
				ViewState ["StartFromCurrentNode"] = value;
				OnDataSourceChanged (EventArgs.Empty);
			}
		}
		
		[DefaultValueAttribute (true)]
		public virtual bool ShowStartingNode {
			get { return ViewState.GetBool ("ShowStartingNode", true); }
			set {
				ViewState ["ShowStartingNode"] = value;
				OnDataSourceChanged (EventArgs.Empty);
			}
		}

		public virtual DataSourceView GetView (string viewName)
		{
			SiteMapNode node = GetStartNode (viewName);
			if (node == null)
				return new SiteMapDataSourceView (this, viewName, SiteMapNodeCollection.EmptyList);
			else if (ShowStartingNode)
				return new SiteMapDataSourceView (this, viewName, node);
			else
				return new SiteMapDataSourceView (this, viewName, node.ChildNodes);
		}

		protected override HierarchicalDataSourceView GetHierarchicalView (string viewPath)
		{
			SiteMapNode node = GetStartNode (viewPath);
			if (node == null)
				return new SiteMapHierarchicalDataSourceView (SiteMapNodeCollection.EmptyList);
			else if (ShowStartingNode || node == null)
				return new SiteMapHierarchicalDataSourceView (node);
			else
				return new SiteMapHierarchicalDataSourceView (node.ChildNodes);
		}
		
		[MonoTODO ("handle StartNodeOffsets > 0")]
		SiteMapNode GetStartNode (string viewPath)
		{
			SiteMapNode starting_node;

			if (viewPath != null && viewPath.Length != 0) {
				string url = MapUrl (StartingNodeUrl);
				return Provider.FindSiteMapNode (url);
			} else if (StartFromCurrentNode) {
				if (StartingNodeUrl.Length != 0)
					throw new InvalidOperationException ("StartingNodeUrl can't be set if StartFromCurrentNode is set to true.");
				starting_node = SiteMap.CurrentNode;
			} else if (StartingNodeUrl.Length != 0) {
				string url = MapUrl (StartingNodeUrl);
				SiteMapNode node = Provider.FindSiteMapNode (url);
				if (node == null) throw new ArgumentException ("Can't find a site map node for the url: " + StartingNodeUrl);

				starting_node = node;
			} else
				starting_node = Provider.RootNode;

			if (starting_node == null)
				return null;
			
			int i;
			if (StartingNodeOffset < 0) {
				for (i = StartingNodeOffset; i < 0; i ++) {
					if (starting_node.ParentNode == null)
						break;
					starting_node = starting_node.ParentNode;
				}
			} else if (StartingNodeOffset > 0) {
				List<SiteMapNode> pathCurrentToStartingNode = new List<SiteMapNode> ();
				SiteMapNode tmpNode = Provider.CurrentNode;
				while (tmpNode != null && tmpNode != starting_node) {
					pathCurrentToStartingNode.Insert (0, tmpNode);
					tmpNode = tmpNode.ParentNode;
				}
				if (tmpNode == starting_node &&
					StartingNodeOffset <= pathCurrentToStartingNode.Count) {
					// The requested node is in the same subtree as the starting_node
					// try to advance on this path.
					starting_node = pathCurrentToStartingNode [StartingNodeOffset - 1];
				}
			}

			return starting_node;
		}
		
		string MapUrl (string url)
		{
			if (String.IsNullOrEmpty (url))
				return String.Empty;
			if (UrlUtils.IsRelativeUrl (url))
				return UrlUtils.Combine (HttpRuntime.AppDomainAppVirtualPath, url);
			else
				return UrlUtils.ResolveVirtualPathFromAppAbsolute (url);
		}
	}
}

#endif


