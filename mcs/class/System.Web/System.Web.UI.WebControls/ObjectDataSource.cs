//
// System.Web.UI.WebControls.ObjectDataSource
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
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
using System.ComponentModel;
using System.IO;
using System.Security.Permissions;
using System.Drawing;

namespace System.Web.UI.WebControls
{
	
	[DefaultEventAttribute ("Selecting")]
	[DefaultPropertyAttribute ("TypeName")]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.ObjectDataSourceDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildrenAttribute (true)]
	[PersistChildrenAttribute (false)]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[ToolboxBitmap ("bitmap file goes here")]
	public class ObjectDataSource : DataSourceControl
	{
		static readonly string [] emptyNames = new string [] { "DefaultView" };
		ObjectDataSourceView defaultView;
		
		int cacheDuration = 0;
		bool enableCaching = false;
		string cacheKeyDependency = null;
		string sqlCacheDependency = null;
		DataSourceCacheManager cache = null;
		DataSourceCacheExpiry cacheExpirationPolicy = DataSourceCacheExpiry.Absolute;

		public ObjectDataSource ()
		{
		}
		
		public ObjectDataSource (string typeName, string selectMethod)
		{
			SelectMethod = selectMethod;
			TypeName = typeName;
		}
		
		ObjectDataSourceView DefaultView {
			get {
				if (defaultView == null) {
					defaultView = new ObjectDataSourceView (this, emptyNames [0], Context);
					if (IsTrackingViewState)
						((IStateManager) defaultView).TrackViewState ();
				}
				return defaultView;
			}
		}
		
		public event ObjectDataSourceStatusEventHandler Deleted {
			add { DefaultView.Deleted += value; }
			remove { DefaultView.Deleted -= value; }
		}
		
		public event ObjectDataSourceMethodEventHandler Deleting {
			add { DefaultView.Deleting += value; }
			remove { DefaultView.Deleting -= value; }
		}
		
		public event ObjectDataSourceFilteringEventHandler Filtering {
			add { DefaultView.Filtering += value; }
			remove { DefaultView.Filtering -= value; }
		}
		
		public event ObjectDataSourceStatusEventHandler Inserted {
			add { DefaultView.Inserted += value; }
			remove { DefaultView.Inserted -= value; }
		}
		
		public event ObjectDataSourceMethodEventHandler Inserting {
			add { DefaultView.Inserting += value; }
			remove { DefaultView.Inserting -= value; }
		}
		
		public event ObjectDataSourceObjectEventHandler ObjectCreated {
			add { DefaultView.ObjectCreated += value; }
			remove { DefaultView.ObjectCreated -= value; }
		}
		
		public event ObjectDataSourceObjectEventHandler ObjectCreating {
			add { DefaultView.ObjectCreating += value; }
			remove { DefaultView.ObjectCreating -= value; }
		}
		
		public event ObjectDataSourceDisposingEventHandler ObjectDisposing {
			add { DefaultView.ObjectDisposing += value; }
			remove { DefaultView.ObjectDisposing -= value; }
		}
		
		/*		public event ObjectDataSourceResolvingMethodEventHandler ResolvingMethod {
				add { DefaultView.ResolvingMethod += value; }
				remove { DefaultView.ResolvingMethod -= value; }
				}
		*/
		public event ObjectDataSourceStatusEventHandler Selected {
			add { DefaultView.Selected += value; }
			remove { DefaultView.Selected -= value; }
		}
		
		public event ObjectDataSourceSelectingEventHandler Selecting {
			add { DefaultView.Selecting += value; }
			remove { DefaultView.Selecting -= value; }
		}
		
		public event ObjectDataSourceStatusEventHandler Updated {
			add { DefaultView.Updated += value; }
			remove { DefaultView.Updated -= value; }
		}
		
		public event ObjectDataSourceMethodEventHandler Updating {
			add { DefaultView.Updating += value; }
			remove { DefaultView.Updating -= value; }
		}

		[DefaultValue (0)]
		[TypeConverter (typeof (DataSourceCacheDurationConverter))]
		public virtual int CacheDuration 
		{
			get {
				return cacheDuration;
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "The duration must be non-negative");

				cacheDuration = value;
			}
		}

		[DefaultValue (DataSourceCacheExpiry.Absolute)]
		public virtual DataSourceCacheExpiry CacheExpirationPolicy 
		{
			get {
				return cacheExpirationPolicy;
			}
			set {
				cacheExpirationPolicy = value;
			}
		}

		[DefaultValue ("")]
		public virtual string CacheKeyDependency 
		{
			get {
				return cacheKeyDependency != null ? cacheKeyDependency : string.Empty;
			}
			set {
				cacheKeyDependency = value;
			}
		}
		
		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute (ConflictOptions.OverwriteChanges)]
		public ConflictOptions ConflictDetection {
			get { return DefaultView.ConflictDetection; }
			set { DefaultView.ConflictDetection = value; }
		}

		[DefaultValue (false)]
		public bool ConvertNullToDBNull
		{
			get {
				return DefaultView.ConvertNullToDBNull;
			}
			set {
				DefaultView.ConvertNullToDBNull = value;
			}
		}
		
		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute ("")]
		public string DataObjectTypeName {
			get { return DefaultView.DataObjectTypeName; }
			set { DefaultView.DataObjectTypeName = value; }
		}
		
		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute ("")]
		public string DeleteMethod {
			get { return DefaultView.DeleteMethod; }
			set { DefaultView.DeleteMethod = value; }
		}
		
		[WebCategoryAttribute ("Data")]
		[MergablePropertyAttribute (false)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValueAttribute (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public ParameterCollection DeleteParameters {
			get { return DefaultView.DeleteParameters; }
		}

		[DefaultValue (false)]
		public virtual bool EnableCaching {
			get { return enableCaching; }
			set { enableCaching = value; }
		}
		
		[WebCategoryAttribute ("Paging")]
		[DefaultValueAttribute (false)]
		public bool EnablePaging {
			get { return DefaultView.EnablePaging; }
			set { DefaultView.EnablePaging = value; }
		}
		
		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute ("")]
		public string FilterExpression {
			get { return DefaultView.FilterExpression; }
			set { DefaultView.FilterExpression = value; }
		}
		
		[WebCategoryAttribute ("Data")]
		[MergablePropertyAttribute (false)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValueAttribute (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public ParameterCollection FilterParameters {
			get { return DefaultView.FilterParameters; }
		}
		
		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Data")]
		public string InsertMethod {
			get { return DefaultView.InsertMethod; }
			set { DefaultView.InsertMethod = value; }
		}
		
		[WebCategoryAttribute ("Data")]
		[MergablePropertyAttribute (false)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValueAttribute (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public ParameterCollection InsertParameters {
			get { return DefaultView.InsertParameters; }
		}
		
		[WebCategoryAttribute ("Paging")]
		[DefaultValueAttribute ("maximumRows")]
		public string MaximumRowsParameterName {
			get { return DefaultView.MaximumRowsParameterName; }
			set { DefaultView.MaximumRowsParameterName = value; }
		}

		[WebCategoryAttribute ("Data")]
		[DefaultValueAttribute ("{0}")]
		public string OldValuesParameterFormatString {
			get { return DefaultView.OldValuesParameterFormatString; }
			set { DefaultView.OldValuesParameterFormatString = value; }
		}
		
		[WebCategoryAttribute ("Paging")]
		[DefaultValueAttribute ("")]
		public string SelectCountMethod {
			get { return DefaultView.SelectCountMethod; }
			set { DefaultView.SelectCountMethod = value; }
		}
		
		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Data")]
		public string SelectMethod {
			get { return DefaultView.SelectMethod; }
			set { DefaultView.SelectMethod = value; }
		}
		
		[WebCategoryAttribute ("Data")]
		[MergablePropertyAttribute (false)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValueAttribute (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public ParameterCollection SelectParameters {
			get { return DefaultView.SelectParameters; }
		}

		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Data")]
		public string SortParameterName {
			get { return DefaultView.SortParameterName; }
			set { DefaultView.SortParameterName = value; }
		}

		[MonoTODO ("SQLServer specific")]
		[DefaultValue ("")]
		public virtual string SqlCacheDependency {
			get { return sqlCacheDependency != null ? sqlCacheDependency : string.Empty; }
			set { sqlCacheDependency = value; }
		}
		
		[WebCategoryAttribute ("Paging")]
		[DefaultValueAttribute ("startRowIndex")]
		public string StartRowIndexParameterName {
			get { return DefaultView.StartRowIndexParameterName; }
			set { DefaultView.StartRowIndexParameterName = value; }
		}
		
		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Data")]
		public string TypeName {
			get { return DefaultView.TypeName; }
			set { DefaultView.TypeName = value; }
		}
		
		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Data")]
		public string UpdateMethod {
			get { return DefaultView.UpdateMethod; }
			set { DefaultView.UpdateMethod = value; }
		}
		
		[WebCategoryAttribute ("Data")]
		[MergablePropertyAttribute (false)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValueAttribute (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public ParameterCollection UpdateParameters {
			get { return DefaultView.UpdateParameters; }
		}

		internal DataSourceCacheManager Cache {
			get {
				if (cache == null)
					cache = new DataSourceCacheManager (CacheDuration, CacheKeyDependency, CacheExpirationPolicy, this, Context);
				return cache;
			}
		}

		protected override DataSourceView GetView (string viewName)
		{
			if (viewName == null)
				throw new ArgumentException ("viewName");

			return DefaultView;
		}
		
		protected override ICollection GetViewNames ()
		{
			return emptyNames;
		}
		
		public IEnumerable Select ()
		{
			return DefaultView.Select (DataSourceSelectArguments.Empty);
		}
		
		public int Update ()
		{
			Hashtable empty = new Hashtable ();
			return DefaultView.Update (empty, empty, null);
		}

		public int Delete ()
		{
			Hashtable empty = new Hashtable ();
			return DefaultView.Delete (empty, null);
		}
		
		public int Insert ()
		{
			Hashtable empty = new Hashtable ();
			return DefaultView.Insert (empty);
		}
		
		protected internal override void OnInit (EventArgs e)
		{
			Page.LoadComplete += OnPageLoadComplete;
		}
		
		void OnPageLoadComplete (object sender, EventArgs e)
		{
			FilterParameters.UpdateValues (Context, this);
			SelectParameters.UpdateValues (Context, this);
		}
		
		protected override void LoadViewState (object savedState)
		{
			if (savedState == null) {
				base.LoadViewState (null);
				((IStateManager)DefaultView).LoadViewState (null);
			} else {
				Pair p = (Pair) savedState;
				base.LoadViewState (p.First);
				((IStateManager)DefaultView).LoadViewState (p.Second);
			}
		}

		protected override object SaveViewState()
		{
			object baseState = base.SaveViewState ();
			object viewState = ((IStateManager)DefaultView).SaveViewState ();
			if (baseState != null || viewState != null) return new Pair (baseState, viewState);
			else return null;
		}

		protected override void TrackViewState()
		{
			((IStateManager)DefaultView).TrackViewState ();
		}
	}
}
#endif


