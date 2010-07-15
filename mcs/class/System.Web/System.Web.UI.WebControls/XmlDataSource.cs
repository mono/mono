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
using System.Drawing;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.ComponentModel;
using System.IO;
using System.Web.Caching;
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
	[ToolboxBitmap ("")]
	public class XmlDataSource : HierarchicalDataSourceControl, IDataSource, IListSource {

		string _data = string.Empty;
		string _transform = string.Empty;
		string _xpath = string.Empty;
		string _dataFile = string.Empty;
		string _transformFile = string.Empty;
		string _cacheKeyDependency = string.Empty;
		bool _enableCaching = true;
		int _cacheDuration = 0;
		bool _documentNeedsUpdate;
		
		DataSourceCacheExpiry _cacheExpirationPolicy = DataSourceCacheExpiry.Absolute;
		static readonly string [] emptyNames = new string [] { "DefaultView" };
		
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
			if (_documentNeedsUpdate)
				UpdateXml ();
			
			if (xmlDocument == null && EnableCaching)
				xmlDocument = GetXmlDocumentFromCache ();

			if (xmlDocument == null) {
				xmlDocument = LoadXmlDocument ();
				UpdateCache ();
			}

			return xmlDocument;
		}

		[MonoTODO ("schema")]
		XmlDocument LoadXmlDocument ()
		{
			XmlDocument document = LoadFileOrData (DataFile, Data);
			if (String.IsNullOrEmpty (TransformFile) && String.IsNullOrEmpty (Transform))
				return document;

			XslTransform xslTransform = new XslTransform ();
			XmlDocument xsl = LoadFileOrData (TransformFile, Transform);
			xslTransform.Load (xsl);

			OnTransforming (EventArgs.Empty);

			XmlDocument transofrResult = new XmlDocument ();
			transofrResult.Load (xslTransform.Transform (document, TransformArgumentList));

			return transofrResult;
		}

		XmlDocument LoadFileOrData (string filename, string data)
		{
			XmlDocument document = new XmlDocument ();
			if (!String.IsNullOrEmpty (filename)) {
				Uri uri;
				if (Uri.TryCreate (filename, UriKind.Absolute, out uri))
					document.Load (filename);
				else
					document.Load (MapPathSecure (filename));
			} else
				if (!String.IsNullOrEmpty (data))
					document.LoadXml (data);
			return document;
		}

		XmlDocument GetXmlDocumentFromCache ()
		{
			if (DataCache != null)
				return (XmlDocument) DataCache [GetDataKey ()];

			return null;
		}

		string GetDataKey ()
		{
			Page page = Page;
			string p = page != null ? page.ToString () : "NullPage";
			
			return TemplateSourceDirectory + "_" + p + "_" + ID;
		}

		Cache DataCache
		{
			get
			{
				if (HttpContext.Current != null)
					return HttpContext.Current.InternalCache;

				return null;
			}
		}

		void UpdateCache ()
		{
			if (!EnableCaching)
				return;

			if (DataCache == null)
				return;

			if (DataCache [GetDataKey ()] != null)
				DataCache.Remove (GetDataKey ());

			DateTime absoluteExpiration = Cache.NoAbsoluteExpiration;
			TimeSpan slidindExpiraion = Cache.NoSlidingExpiration;

			if (CacheDuration > 0) {
				if (CacheExpirationPolicy == DataSourceCacheExpiry.Absolute)
					absoluteExpiration = DateTime.Now.AddSeconds (CacheDuration);
				else
					slidindExpiraion = new TimeSpan (CacheDuration * 10000L);
			}

			CacheDependency dependency = null;
			if (CacheKeyDependency.Length > 0)
				dependency = new CacheDependency (new string [] { }, new string [] { CacheKeyDependency });
			else
				dependency = new CacheDependency (new string [] { }, new string [] { });

			DataCache.Add (GetDataKey (), xmlDocument, dependency,
				absoluteExpiration, slidindExpiraion, CacheItemPriority.Default, null);
		}
		
		// If datafile changed, then DO NOT USE the cached data, but update it.
		void UpdateXml()
		{
			xmlDocument = LoadXmlDocument (); 
			UpdateCache ();
			_documentNeedsUpdate = false;
		}

		public void Save ()
		{
			if (!CanBeSaved)
				throw new InvalidOperationException ();

			if (xmlDocument != null)
				xmlDocument.Save (MapPathSecure (DataFile));
		}
		
		bool CanBeSaved {
			get {
				return Transform == String.Empty && TransformFile == String.Empty && DataFile != String.Empty;
			}
		}
		
		protected override HierarchicalDataSourceView GetHierarchicalView (string viewPath)
		{
			XmlNode doc = this.GetXmlDocument ();
			XmlNodeList ret = null;
			
			if (!String.IsNullOrEmpty (viewPath)) {
				XmlNode n = doc.SelectSingleNode (viewPath);
				if (n != null)
					ret = n.ChildNodes;
			} else if (!String.IsNullOrEmpty (XPath)) {
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
			if (String.IsNullOrEmpty (viewName))
				viewName = "DefaultView";
			
			return new XmlDataSourceView (this, viewName);
		}
		
		ICollection IDataSource.GetViewNames ()
		{
			return emptyNames;
		}
		
		[DefaultValue (0)]
		[TypeConverter (typeof(DataSourceCacheDurationConverter))]
		public virtual int CacheDuration {
			get {
				return _cacheDuration;
			}
			set {
				_cacheDuration = value;
			}
		}

		[DefaultValue (DataSourceCacheExpiry.Absolute)]
		public virtual DataSourceCacheExpiry CacheExpirationPolicy {
			get {
				return _cacheExpirationPolicy;
			}
			set {
				_cacheExpirationPolicy = value;
			}
		}

		[DefaultValue ("")]
		public virtual string CacheKeyDependency {
			get {
				return _cacheKeyDependency;
			}
			set {
				_cacheKeyDependency = value;
			}
		}

		[DefaultValue (true)]
		public virtual bool EnableCaching {
			get {
				return _enableCaching;
			}
			set {
				_enableCaching = value;
			}
		}

		[DefaultValue ("")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("Inline XML data.")]
		[WebCategory ("Data")]
		[EditorAttribute ("System.ComponentModel.Design.MultilineStringEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
//		[TypeConverter (typeof(MultilineStringConverter))]
		public virtual string Data {
			get { return _data; }
			set {
				if (_data != value) {
					_data = value;
					_documentNeedsUpdate = true;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.XmlDataFileEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MonoLimitation ("Absolute path to the file system is not supported; use a relative URI instead.")]
		public virtual string DataFile {
			get { return _dataFile; }
			set {
				if (_dataFile != value) {
					_dataFile = value;
					_documentNeedsUpdate = true;
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
			get { return _transform; }
			set {
				if (_transform != value) {
					_transform = value; 
					_documentNeedsUpdate = true;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		[EditorAttribute ("System.Web.UI.Design.XslTransformFileEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValueAttribute ("")]
		[MonoLimitation ("Absolute path to the file system is not supported; use a relative URI instead.")]
		public virtual string TransformFile {
			get { return _transformFile; }
			set {
				if (_transformFile != value) {
					_transformFile = value;
					_documentNeedsUpdate = true;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
		
		[DefaultValueAttribute ("")]
		public virtual string XPath {
			get { return _xpath; }
			set {
				if (_xpath != value) {
					_xpath = value;
					OnDataSourceChanged(EventArgs.Empty);
				}
			}
		}
	}
}
#endif

