//
// System.Web.UI.WebControls.XmlDataSource
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
using System.Xml;
using System.Xml.Xsl;
using System.ComponentModel;

namespace System.Web.UI.WebControls {
	public class XmlDataSource : HierarchicalDataSourceControl, IDataSource, IListSource {

		
		event EventHandler IDataSource.DataSourceChanged {
			add { ((IHierarchicalDataSource)this).DataSourceChanged += value; }
			remove { ((IHierarchicalDataSource)this).DataSourceChanged -= value; }
		}
		
		static object EventTransforming = new object ();
		public event EventHandler Transforming {
			add { Events.AddHandler (EventTransforming, value); }
			remove { Events.RemoveHandler (EventTransforming, value); }
		}
		
		protected virtual void OnTransforming (EventArgs e)
		{
			EventHandler eh = Events [EventTransforming] as EventHandler;
			if (eh != null)
				eh (this, e);
		}
		
		[MonoTODO]
		public XmlDataDocument GetXmlDataDocument ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Save ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void LoadViewState (object savedState)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override object SaveViewState ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void TrackViewState ()
		{
			throw new NotImplementedException ();
		}
		
		protected override HierarchicalDataSourceView GetHierarchicalView (string viewPath)
		{
			XmlNode doc = this.GetXmlDataDocument ();
			XmlNodeList ret = null;
			
			if (viewPath != "") {
				XmlNode n = doc.SelectSingleNode (viewPath);
				if (n != null)
					ret = n.ChildNodes;
			} else if (XPath != "") {
				ret = doc.SelectNodes (XPath);
			} else {
				ret = doc.ChildNodes;
			}
			
			return new XmlHierarchicalDataSourceView (ret);
		}
		
		IList IListSource.GetList ()
		{
			return ListSourceHelper.GetList (this);
		}
		
		bool IListSource.ContainsListCollection {
			get { return ListSourceHelper.ContainsListCollection (this); }
		}
		
		DataSourceView IDataSource.GetView (string viewName)
		{
			if (viewName == "")
				viewName = "DefaultView";
			
			return new XmlDataSourceView (this, viewName, GetXmlDataDocument ().SelectNodes (XPath != "" ? XPath : "."));
		}
		
		ICollection IDataSource.GetViewNames ()
		{
			return new string [] { "DefaultView" };
		}
		
		public virtual bool AutoSave {
			get {
				object ret = ViewState ["AutoSave"];
				return ret != null ? (bool)ret : true;
			}
			set {
				ViewState ["AutoSave"] = value;
			}
		}
		
		// TODO: stub these apis
		//protected virtual FileDataSourceCache Cache { get; }
		//public virtual int CacheDuration { get; set; }
		//public virtual DataSourceCacheExpiry CacheExpirationPolicy { get; set; }
		//public virtual string CacheKeyDependency { get; set; }
		//public virtual bool EnableCaching { get; set; }
		public virtual string Data {
			get {
				string ret = ViewState ["Data"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (Data != value) {
					ViewState ["Data"] = value;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		public virtual string DataFile {
			get {
				string ret = ViewState ["DataFile"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (DataFile != value) {
					ViewState ["DataFile"] = value;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		public virtual bool ReadOnly {
			get {
				object ret = ViewState ["ReadOnly"];
				return ret != null ? (bool)ret : true;
			}
			set {
				ViewState ["ReadOnly"] = value;
			}
		}
		
		public virtual string Schema {
			get {
				string ret = ViewState ["Schema"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (Schema != value) {
					ViewState ["Schema"] = value;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		public virtual string SchemaFile {
			get {
				string ret = ViewState ["SchemaFile"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (SchemaFile != value) {
					ViewState ["SchemaFile"] = value;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		XsltArgumentList transformArgumentList;
		public virtual XsltArgumentList TransformArgumentList {
			get { return transformArgumentList; }
			set { transformArgumentList = value; }
		}
		
		public virtual string Transform {
			get {
				string ret = ViewState ["Transform"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (Transform != value) {
					ViewState ["Transform"] = value;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		public virtual string TransformFile {
			get {
				string ret = ViewState ["TransformFile"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (TransformFile != value) {
					ViewState ["TransformFile"] = value;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		public virtual string XPath {
			get {
				string ret = ViewState ["XPath"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (XPath != value) {
					ViewState ["XPath"] = value;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
	}
}
#endif

