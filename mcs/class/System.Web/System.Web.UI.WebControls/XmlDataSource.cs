//
// System.Web.UI.WebControls.XmlDataSource
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
using System.Xml;
using System.Xml.Xsl;
using System.ComponentModel;
using System.IO;

namespace System.Web.UI.WebControls {

	[DefaultProperty ("DataFile")]
	[DefaultEvent ("Transforming")]
	[ParseChildren (true)]
	[PersistChildren (false)]
	[WebSysDescription ("Connect to an XML file.")]
//	[WebSysDisplayName ("XML file")]
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
		
		XmlDataDocument xmlDataDocument;
		public XmlDataDocument GetXmlDataDocument ()
		{
			if (xmlDataDocument == null) {
				xmlDataDocument = new XmlDataDocument ();
				LoadXmlDataDocument (xmlDataDocument);
			}
			return xmlDataDocument;
		}
		
		[MonoTODO ("XSLT, schema")]
		void LoadXmlDataDocument (XmlDataDocument document)
		{
			if (Transform == "" && TransformFile == "") {
				if (DataFile != "")
					document.Load (MapPathSecure (DataFile));
				else
					document.LoadXml (Data);
			} else {
				throw new NotImplementedException ("XSLT transform not implemented");
			}
		}

		public void Save ()
		{
			if (!CanBeSaved)
				throw new InvalidOperationException ();
			
			xmlDataDocument.Save (MapPathSecure (DataFile));
		}
		
		bool CanBeSaved {
			get {
				return !ReadOnly && Transform == "" && TransformFile == "" && DataFile != "";
			}
		}
		
		[MonoTODO]
		protected override void LoadViewState (object savedState)
		{
			base.LoadViewState (savedState);
		}
		
		[MonoTODO]
		protected override object SaveViewState ()
		{
			return base.SaveViewState ();
		}
		
		[MonoTODO]
		protected override void TrackViewState ()
		{
			base.TrackViewState ();
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
		
		[DefaultValue ("")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("Inline XML data.")]
		[WebCategory ("Data")]
//		[TypeConverter (typeof(MultilineStringConverter))]
		public virtual string Data {
			get {
				string ret = ViewState ["Data"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (Data != value) {
					ViewState ["Data"] = value;
					xmlDataDocument = null;
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
					xmlDataDocument = null;
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
					xmlDataDocument = null;
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
					xmlDataDocument = null;
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
					xmlDataDocument = null;
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
					xmlDataDocument = null;
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

