//
// System.Web.UI.WebControls.SqlDataSource
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sanjay Gupta (gsanjay@novell.com)
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2003 Ben Maurer
// (C) 2004-2006 Novell, Inc. (http://www.novell.com)
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
using System.Configuration;
using System.Drawing;
using System.Web.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.ComponentModel;

namespace System.Web.UI.WebControls {

	[ParseChildrenAttribute (true)]
	[PersistChildrenAttribute (false)]
	[DefaultPropertyAttribute ("SelectQuery")]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.SqlDataSourceDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultEventAttribute ("Selecting")]
	public class SqlDataSource : DataSourceControl {
		
		public SqlDataSource ()
		{
		}

		public SqlDataSource (string connectionString, string selectCommand)
		{
			ConnectionString = connectionString;
			SelectCommand = selectCommand;
		}
		
		public SqlDataSource (string providerName, string connectionString, string selectCommand)
		{
			ProviderName = providerName;
			ConnectionString = connectionString;
			SelectCommand = selectCommand;
		}

		protected override DataSourceView GetView (string viewName)
		{
			if (viewName == "" || viewName == null)
				return View;
			else
				throw new ArgumentException ("viewName");
		}
		
		protected virtual SqlDataSourceView CreateDataSourceView (string viewName)
		{
			SqlDataSourceView view = new SqlDataSourceView (this, viewName, this.Context);
			view.DataSourceViewChanged += new EventHandler (ViewChanged);
			if (IsTrackingViewState)
				((IStateManager) view).TrackViewState ();			
			return view;
		}

		protected virtual DbProviderFactory GetDbProviderFactory ()
		{
			DbProviderFactory f = null;

			if (ProviderName != null && ProviderName != "") {
				try {
					f = DbProviderFactories.GetFactory(ProviderName);
				}
				catch { /* nada */ }
				if (f != null)
					return f;
			}

			return SqlClientFactory.Instance;
		}

		internal DbProviderFactory GetDbProviderFactoryInternal ()
		{
			return GetDbProviderFactory ();
		}

		protected override ICollection GetViewNames ()
		{
			return new string [] { "DefaultView" };
		}
			
		public int Insert ()
		{
			return View.Insert (null);
		}
		
		public int Delete ()
		{
			return View.Delete (null, null);
		}
		
		public IEnumerable Select (DataSourceSelectArguments args)
		{
			return View.Select (args);			
		}
		
		public int Update ()
		{
			return View.Update (null, null, null);
		}
			
		protected override void LoadViewState (object savedState)
		{
			Pair p = savedState as Pair;
			if (p != null) {
				base.LoadViewState (p.First);
				((IStateManager) View).LoadViewState (p.Second);
			}
		}
		
		protected override object SaveViewState ()
		{
			object me = base.SaveViewState (), view = ((IStateManager) View).SaveViewState ();
			if (me != null || view != null)
				return new Pair (me, view);
			else
				return null;
		}
		
		protected override void TrackViewState ()
		{
			base.TrackViewState ();
			if (view != null)
				((IStateManager) view).TrackViewState ();
		}

#region TODO
		[MonoTODO]
		[DefaultValue ("")]
		public virtual string CacheKeyDependency {
			get { return ViewState.GetString ("CacheKeyDependency", ""); }
			set { ViewState ["CacheKeyDependency"] = value; }
		}

		[MonoTODO]
		[DefaultValue (true)]
		public virtual bool CancelSelectOnNullParameter {
			get { return ViewState.GetBool ("CancelSelectOnNullParameter", true); }
			set { ViewState["CancelSelectOnNullParameter"] = value; }
		}

		[MonoTODO]
		[DefaultValue (ConflictOptions.OverwriteChanges)]
		public ConflictOptions ConflictDetection {
			get { return (ConflictOptions) ViewState.GetInt ("ConflictDetection", (int)ConflictOptions.OverwriteChanges); }
			set { ViewState ["ConflictDetection"] = value; }
		}

		[MonoTODO]
		[DefaultValue (SqlDataSourceCommandType.Text)]
		public SqlDataSourceCommandType DeleteCommandType {
			get { return (SqlDataSourceCommandType) ViewState.GetInt ("DeleteCommandType", (int)SqlDataSourceCommandType.StoredProcedure); }
			set { ViewState ["DeleteCommandType"] = value; }
		}

		[MonoTODO]
		[DefaultValue (SqlDataSourceCommandType.Text)]
		public SqlDataSourceCommandType InsertCommandType {
			get { return (SqlDataSourceCommandType) ViewState.GetInt ("InsertCommandType", (int)SqlDataSourceCommandType.StoredProcedure); }
			set { ViewState ["InsertCommandType"] = value; }
		}

		[MonoTODO]
		[DefaultValue (SqlDataSourceCommandType.Text)]
		public SqlDataSourceCommandType SelectCommandType {
			get { return (SqlDataSourceCommandType) ViewState.GetInt ("SelectCommandType", (int)SqlDataSourceCommandType.StoredProcedure); }
			set { ViewState ["SelectCommandType"] = value; }
		}

		[MonoTODO]
		[DefaultValue (SqlDataSourceCommandType.Text)]
		public SqlDataSourceCommandType UpdateCommandType {
			get { return (SqlDataSourceCommandType) ViewState.GetInt ("UpdateCommandType", (int)SqlDataSourceCommandType.StoredProcedure); }
			set { ViewState ["UpdateCommandType"] = value; }
		}

		[MonoTODO]
		[DefaultValue ("{0}")]
		public string OldValuesParameterFormatString {
			get { return ViewState.GetString ("OldValuesParameterFormatString", "{0}"); }
			set { ViewState ["OldValuesParameterFormatString"] = value; }
		}
		
		[DefaultValue ("")]
		[MonoTODO]
		public virtual string SqlCacheDependency {
			get { return ViewState.GetString ("SqlCacheDependency", ""); }
			set { ViewState ["SqlCacheDependency"] = value; }
		}

		[MonoTODO]
		[DefaultValue ("")]
		public string SortParameterName {
			get { return ViewState.GetString ("SortParameterName", ""); }
			set { ViewState ["SortParameterName"] = value; }
		}

		[DefaultValue (0)]
		//		[TypeConverter (typeof (DataSourceCacheDurationConverter))]
		public virtual int CacheDuration {
			get { return ViewState.GetInt ("CacheDuration", 0); }
			set { ViewState ["CacheDuration"] = value; }
		}

		[DefaultValue (DataSourceCacheExpiry.Absolute)]
		public virtual DataSourceCacheExpiry CacheExpirationPolicy {
			get { return (DataSourceCacheExpiry) ViewState.GetInt ("CacheExpirationPolicy", (int)DataSourceCacheExpiry.Absolute); }
			set { ViewState ["CacheExpirationPolicy"] = value; }
		}

		[DefaultValue (false)]
		public virtual bool EnableCaching {
			get { return ViewState.GetBool ("EnableCaching", false); }
			set { ViewState ["EnableCaching"] = value; }
		}
#endregion

		[DefaultValueAttribute ("")]
		[TypeConverterAttribute ("System.Web.UI.Design.WebControls.DataProviderNameConverter, " + Consts.AssemblySystem_Design)]
		public virtual string ProviderName {
			get { return ViewState.GetString ("ProviderName", ""); }
			set { ViewState ["ProviderName"] = value; }
		}
		
		
		[EditorAttribute ("System.Web.UI.Design.WebControls.SqlDataSourceConnectionStringEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValueAttribute ("")]
		public virtual string ConnectionString {
			get { return ViewState.GetString ("ConnectionString", ""); }
			set { ViewState ["ConnectionString"] = value; }
		}
		
		[DefaultValueAttribute (SqlDataSourceMode.DataSet)]
		public SqlDataSourceMode DataSourceMode {
			get { return (SqlDataSourceMode) ViewState.GetInt ("DataSourceMode", (int)SqlDataSourceMode.DataSet); }
			set { ViewState ["DataSourceMode"] = value; }
		}
				
		[DefaultValueAttribute ("")]
		public string DeleteCommand {
			get { return View.DeleteCommand; }
			set { View.DeleteCommand = value; }
		}
		
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MergablePropertyAttribute (false)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
		public ParameterCollection DeleteParameters {
			get { return View.DeleteParameters; }
		}
		
		[DefaultValueAttribute (null)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MergablePropertyAttribute (false)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		public ParameterCollection FilterParameters {
			get { return View.FilterParameters; }
		}
		
		[DefaultValueAttribute ("")]
		public string InsertCommand {
			get { return View.InsertCommand; }
			set { View.InsertCommand = value; }
		}
		
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
		[MergablePropertyAttribute (false)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public ParameterCollection InsertParameters {
			get { return View.InsertParameters; }
		}

		[DefaultValueAttribute ("")]
		public string SelectCommand {
			get { return View.SelectCommand; }
			set { View.SelectCommand = value; }
		}
		
		[DefaultValueAttribute (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[MergablePropertyAttribute (false)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public ParameterCollection SelectParameters {
			get { return View.SelectParameters; }
		}
		
		[DefaultValueAttribute ("")]
		public string UpdateCommand {
			get { return View.UpdateCommand; }
			set { View.UpdateCommand = value; }
		}
		
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MergablePropertyAttribute (false)]
		[DefaultValueAttribute (null)]
		public ParameterCollection UpdateParameters {
			get { return View.UpdateParameters; }
		}
		
		[DefaultValueAttribute ("")]
		public string FilterExpression {
			get { return View.FilterExpression; }
			set { View.FilterExpression = value; }
		}
		
		public event SqlDataSourceStatusEventHandler Deleted {
			add { View.Deleted += value; }
			remove { View.Deleted -= value; }
		}
		
		public event SqlDataSourceCommandEventHandler Deleting {
			add { View.Deleting += value; }
			remove { View.Deleting -= value; }
		}
		
		public event SqlDataSourceStatusEventHandler Inserted {
			add { View.Inserted += value; }
			remove { View.Inserted -= value; }
		}
		
		public event SqlDataSourceFilteringEventHandler Filtering {
			add { View.Filtering += value; }
			remove { View.Filtering -= value; }
		}

		public event SqlDataSourceCommandEventHandler Inserting {
			add { View.Inserting += value; }
			remove { View.Inserting -= value; }
		}
		
		public event SqlDataSourceStatusEventHandler Selected {
			add { View.Selected += value; }
			remove { View.Selected -= value; }
		}
		
		public event SqlDataSourceSelectingEventHandler Selecting {
			add { View.Selecting += value; }
			remove { View.Selecting -= value; }
		}
		
		public event SqlDataSourceStatusEventHandler Updated {
			add { View.Updated += value; }
			remove { View.Updated -= value; }
		}
		
		public event SqlDataSourceCommandEventHandler Updating {
			add { View.Updating += value; }
			remove { View.Updating -= value; }
		}
		
		SqlDataSourceView view;
		SqlDataSourceView View {
			get {
				if (view == null) {
					view = CreateDataSourceView ("DefaultView");
					view.DataSourceViewChanged += new EventHandler (ViewChanged);
					if (IsTrackingViewState)
						((IStateManager) view).TrackViewState ();
				}
				return view;
			}
		}
		
		void ViewChanged (object source, EventArgs e)
		{
			RaiseDataSourceChangedEvent (e);
		}
	}
}
#endif

