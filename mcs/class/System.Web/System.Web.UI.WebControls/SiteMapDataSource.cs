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
using System.Web.UI;
using System.Web.Util;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[PersistChildrenAttribute (false)]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.SiteMapDataSourceDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildrenAttribute (true)]
	public class SiteMapDataSource : HierarchicalDataSourceControl, IDataSource, IListSource
	{
		static string[] emptyNames = new string[] { string.Empty };
		
		SiteMapProvider provider;
		
		public virtual ICollection GetViewNames ()
		{
			return emptyNames;
		}
		
		public IList GetList ()
		{
			return ListSourceHelper.GetList (this);
		}
		
	    [BrowsableAttribute (false)]
	    [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public bool ContainsListCollection {
			get { return ListSourceHelper.ContainsListCollection (this); }
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
		public string SiteMapProvider {
			get {
				object o = ViewState ["SiteMapProvider"];
				if (o != null) return (string) o;
				else return string.Empty;
			}
			set {
				ViewState ["SiteMapProvider"] = value;
				OnDataSourceChanged (EventArgs.Empty);
			}
		}
		
	    [DefaultValueAttribute ("")]
	    [EditorAttribute ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	    [UrlPropertyAttribute]
		public string StartingNodeUrl {
			get {
				object o = ViewState ["StartingNodeUrl"];
				if (o != null) return (string) o;
				else return string.Empty;
			}
			set {
				ViewState ["StartingNodeUrl"] = value;
				OnDataSourceChanged (EventArgs.Empty);
			}
		}
		
	    [DefaultValueAttribute (false)]
		public bool StartFromCurrentNode {
			get {
				object o = ViewState ["StartFromCurrentNode"];
				if (o != null) return (bool) o;
				else return false;
			}
			set {
				ViewState ["StartFromCurrentNode"] = value;
				OnDataSourceChanged (EventArgs.Empty);
			}
		}
		
	    [DefaultValueAttribute (true)]
		public bool ShowStartingNode {
			get {
				object o = ViewState ["ShowStartingNode"];
				if (o != null) return (bool) o;
				else return true;
			}
			set {
				ViewState ["ShowStartingNode"] = value;
				OnDataSourceChanged (EventArgs.Empty);
			}
		}

		public DataSourceView GetView (string viewName)
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
		
		SiteMapNode GetStartNode (string viewPath)
		{
			if (viewPath != null && viewPath.Length != 0) {
				string url = MapUrl (StartingNodeUrl);
				return Provider.FindSiteMapNode (url);
			}
			else if (StartFromCurrentNode) {
				if (StartingNodeUrl.Length != 0)
					throw new InvalidOperationException ("StartingNodeUrl can't be set if StartFromCurrentNode is set to true.");
				return Provider.CurrentNode;
			}
			else if (StartingNodeUrl.Length != 0) {
				string url = MapUrl (StartingNodeUrl);
				SiteMapNode node = Provider.FindSiteMapNode (url);
				if (node == null) throw new ArgumentException ("Can't find a site map node for the url: " + StartingNodeUrl);
				return node;
			}
			else
				return Provider.RootNode;
		}
		
		string MapUrl (string url)
		{
			if (UrlUtils.IsRelativeUrl (url))
				return UrlUtils.Combine (HttpRuntime.AppDomainAppVirtualPath, url);
			else
				return UrlUtils.ResolveVirtualPathFromAppAbsolute (url);
		}
	}
}

#endif

