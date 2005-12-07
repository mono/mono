//
// System.Web.UI.WebControls.XmlDataSource
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//      Chris Toshok (toshok@ximian.com)
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
using System.Xml;
using System.Xml.Xsl;
using System.ComponentModel;
using System.IO;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DesignerAttribute ("System.Web.UI.Design.WebControls.XmlDataSourceDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
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
		
		XmlDocument xmlDocument;
		public XmlDocument GetXmlDocument ()
		{
			if (xmlDocument == null) {
				xmlDocument = new XmlDocument ();
				LoadXmlDocument (xmlDocument);
			}
			return xmlDocument;
		}
		
		[MonoTODO ("XSLT, schema")]
		void LoadXmlDocument (XmlDocument document)
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
			
			xmlDocument.Save (MapPathSecure (DataFile));
		}
		
		bool CanBeSaved {
			get {
				return Transform == "" && TransformFile == "" && DataFile != "";
			}
		}
		
		protected override HierarchicalDataSourceView GetHierarchicalView (string viewPath)
		{
			XmlNode doc = this.GetXmlDocument ();
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
			
			return new XmlDataSourceView (this, viewName);
		}
		
		ICollection IDataSource.GetViewNames ()
		{
			return new string [] { "DefaultView" };
		}
		
		[DefaultValue (0)]
		//[TypeConverter (typeof(DataSourceCacheDurationConverter))]
		[MonoTODO]
		public virtual int CacheDuration {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue (DataSourceCacheExpiry.Absolute)]
		[MonoTODO]
		public virtual DataSourceCacheExpiry CacheExpirationPolicy {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue ("")]
		[MonoTODO]
		public virtual string CacheKeyDependency {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue (true)]
		[MonoTODO]
		public virtual bool EnableCaching {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue ("")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("Inline XML data.")]
		[WebCategory ("Data")]
		[EditorAttribute ("System.ComponentModel.Design.MultilineStringEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
//		[TypeConverter (typeof(MultilineStringConverter))]
		public virtual string Data {
			get {
				string ret = ViewState ["Data"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (Data != value) {
					ViewState ["Data"] = value;
					xmlDocument = null;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.XmlDataFileEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public virtual string DataFile {
			get {
				string ret = ViewState ["DataFile"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (DataFile != value) {
					ViewState ["DataFile"] = value;
					xmlDocument = null;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		XsltArgumentList transformArgumentList;
		
		[BrowsableAttribute (false)]
		public virtual XsltArgumentList TransformArgumentList {
			get { return transformArgumentList; }
			set { transformArgumentList = value; }
		}
		
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[EditorAttribute ("System.ComponentModel.Design.MultilineStringEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValueAttribute ("")]
//		[TypeConverterAttribute (typeof(System.ComponentModel.MultilineStringConverter))]
		public virtual string Transform {
			get {
				string ret = ViewState ["Transform"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (Transform != value) {
					ViewState ["Transform"] = value;
					xmlDocument = null;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		[EditorAttribute ("System.Web.UI.Design.XslTransformFileEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValueAttribute ("")]
		public virtual string TransformFile {
			get {
				string ret = ViewState ["TransformFile"] as string;
				return ret != null ? ret : "";
			}
			set {
				if (TransformFile != value) {
					ViewState ["TransformFile"] = value;
					xmlDocument = null;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		[DefaultValueAttribute ("")]
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

