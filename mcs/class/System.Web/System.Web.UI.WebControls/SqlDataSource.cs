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
	[ToolboxBitmap ("")]
	public class SqlDataSource : DataSourceControl {

		static readonly string [] emptyNames = new string [] { "DefaultView" };
		
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
			if (String.IsNullOrEmpty (viewName) || (String.Compare (viewName, emptyNames [0], StringComparison.InvariantCultureIgnoreCase) == 0))
				return View;
			else
				throw new ArgumentException ("viewName");
		}
		
		protected virtual SqlDataSourceView CreateDataSourceView (string viewName)
		{
			SqlDataSourceView view = new SqlDataSourceView (this, viewName, this.Context);
			if (IsTrackingViewState)
				((IStateManager) view).TrackViewState ();
			return view;
		}

		protected virtual DbProviderFactory GetDbProviderFactory ()
		{
			DbProviderFactory f = null;

			if (!String.IsNullOrEmpty (ProviderName)) {
				f = DbProviderFactories.GetFactory(ProviderName);
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
			return emptyNames;
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

		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);
			Page.LoadComplete += OnPageLoadComplete;
		}

		void OnPageLoadComplete (object sender, EventArgs e)
		{
			FilterParameters.UpdateValues (Context, this);
			SelectParameters.UpdateValues (Context, this);
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

		[DefaultValue (true)]
		public virtual bool CancelSelectOnNullParameter {
			get { return View.CancelSelectOnNullParameter; }
			set { View.CancelSelectOnNullParameter = value; }
		}

		[DefaultValue (ConflictOptions.OverwriteChanges)]
		public ConflictOptions ConflictDetection {
			get { return View.ConflictDetection; }
			set { View.ConflictDetection = value; }
		}

		[DefaultValue (SqlDataSourceCommandType.Text)]
		public SqlDataSourceCommandType DeleteCommandType {
			get { return View.DeleteCommandType; }
			set { View.DeleteCommandType = value; }
		}

		[DefaultValue (SqlDataSourceCommandType.Text)]
		public SqlDataSourceCommandType InsertCommandType {
			get { return View.InsertCommandType; }
			set { View.InsertCommandType = value; }
		}

		[DefaultValue (SqlDataSourceCommandType.Text)]
		public SqlDataSourceCommandType SelectCommandType {
			get { return View.SelectCommandType; }
			set { View.SelectCommandType = value; }
		}

		[DefaultValue (SqlDataSourceCommandType.Text)]
		public SqlDataSourceCommandType UpdateCommandType {
			get { return View.UpdateCommandType; }
			set { View.UpdateCommandType = value; }
		}

		[DefaultValue ("{0}")]
		public string OldValuesParameterFormatString {
			get { return View.OldValuesParameterFormatString; }
			set { View.OldValuesParameterFormatString = value; }
		}

		[DefaultValue ("")]
		public string SortParameterName
		{
			get { return View.SortParameterName; }
			set { View.SortParameterName = value; }
		}

		[DefaultValueAttribute ("")]
		public string FilterExpression
		{
			get { return View.FilterExpression; }
			set { View.FilterExpression = value; }
		}

		// LAME SPEC: the event is raised on setting only when the old value is different
		// from the new one
		string providerName = String.Empty;
		[DefaultValueAttribute ("")]
		[TypeConverterAttribute ("System.Web.UI.Design.WebControls.DataProviderNameConverter, " + Consts.AssemblySystem_Design)]
		public virtual string ProviderName {
			get { return providerName; }
			set
			{
				if (providerName != value) {
					providerName = value;
					RaiseDataSourceChangedEvent (EventArgs.Empty);
				}
			}
		}

		// LAME SPEC: the event is raised on setting only when the old value is different
		// from the new one
		string connectionString = String.Empty;
#if NET_4_0
		[MergableProperty (false)]
#endif
		[EditorAttribute ("System.Web.UI.Design.WebControls.SqlDataSourceConnectionStringEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValueAttribute ("")]
		public virtual string ConnectionString {
			get { return connectionString; }
			set
			{
				if (connectionString != value) {
					connectionString = value;
					RaiseDataSourceChangedEvent (EventArgs.Empty);
				}
			}
		}

		// LAME SPEC: the event is raised on setting only when the old value is different
		// from the new one
		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		SqlDataSourceMode dataSourceMode = SqlDataSourceMode.DataSet;
		[DefaultValueAttribute (SqlDataSourceMode.DataSet)]
		public SqlDataSourceMode DataSourceMode {
			get { return dataSourceMode; }
			set {
				if (dataSourceMode != value) {
					dataSourceMode = value;
					RaiseDataSourceChangedEvent (EventArgs.Empty);
				}
			}
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
		
#region TODO

		int cacheDuration = 0;
		bool enableCaching = false;
		string cacheKeyDependency = null;
		string sqlCacheDependency = null;
		DataSourceCacheManager cache = null;
		DataSourceCacheExpiry cacheExpirationPolicy = DataSourceCacheExpiry.Absolute;

		internal DataSourceCacheManager Cache
		{
			get
			{
				if (cache == null)
					cache = new DataSourceCacheManager (CacheDuration, CacheKeyDependency, CacheExpirationPolicy, this, Context);
				return cache;
			}
		}
		
		[DefaultValue ("")]
		public virtual string CacheKeyDependency
		{
			get { return cacheKeyDependency != null ? cacheKeyDependency : string.Empty; }
			set { cacheKeyDependency = value; }
		}

		[MonoTODO ("SQLServer specific")]
		[DefaultValue ("")]
		public virtual string SqlCacheDependency {
			get { return sqlCacheDependency != null ? sqlCacheDependency : string.Empty; }
			set { sqlCacheDependency = value; }
		}

		[TypeConverter ("System.Web.UI.DataSourceCacheDurationConverter")]
		[DefaultValue (0)]
		public virtual int CacheDuration {
			get { return cacheDuration; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "The duration must be non-negative");

				cacheDuration = value;
			}
		}

		[DefaultValue (DataSourceCacheExpiry.Absolute)]
		public virtual DataSourceCacheExpiry CacheExpirationPolicy {
			get { return cacheExpirationPolicy; ; }
			set { cacheExpirationPolicy = value; }
		}

		[DefaultValue (false)]
		public virtual bool EnableCaching {
			get { return enableCaching; }
			set
			{
				if (DataSourceMode == SqlDataSourceMode.DataReader && value == true)
					throw new NotSupportedException ();
				enableCaching = value;
			}
		}

#endregion
		
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
					if (IsTrackingViewState)
						((IStateManager) view).TrackViewState ();
				}
				return view;
			}
		}
	}
}
#endif



