//
// System.Web.UI.WebControls.SqlDataSource
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2003 Ben Maurer
// (C) 2004 Novell, Inc. (http://www.novell.com)
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

namespace System.Web.UI.WebControls {
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
			((IStateManager) View).TrackViewState ();
		}
		
		//protected virtual DataSourceCache Cache { get; }
		//public virtual int CacheDuration { get; set; }
		//public virtual DataSourceCacheExpiry CacheExpirationPolicy { get; set; }
		//public virtual string CacheKeyDependency { get; set; }
		//public virtual string SqlCacheDependency { get; set; }
		//public virtual bool EnableCaching { get; set; }
		
		public virtual string ProviderName {
			get {
				string val = ViewState ["ProviderName"] as string;
				return val == null ? "System.Data.SqlClient" : val;
			}
			set { ViewState ["ProviderName"] = value; }
		}
		
		
		public virtual string ConnectionString {
			get {
				string val = ViewState ["ConnectionString"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["ConnectionString"] = value; }
		}
		
		public SqlDataSourceMode DataSourceMode {
			get {
				object val = ViewState ["DataSourceMode"];
				return val == null ? SqlDataSourceMode.DataSet : (SqlDataSourceMode) val;
			}
			set { ViewState ["DataSourceMode"] = value; }
		}
				
		public string DeleteCommand {
			get { return View.DeleteCommand; }
			set { View.DeleteCommand = value; }
		}
		
		public ParameterCollection DeleteParameters {
			get { return View.DeleteParameters; }
		}
		
		public ParameterCollection FilterParameters {
			get { return View.FilterParameters; }
		}
		
		public string InsertCommand {
			get { return View.InsertCommand; }
			set { View.InsertCommand = value; }
		}
		public ParameterCollection InsertParameters {
			get { return View.InsertParameters; }
		}

		public string SelectCommand {
			get { return View.SelectCommand; }
			set { View.SelectCommand = value; }
		}
		
		public ParameterCollection SelectParameters {
			get { return View.SelectParameters; }
		}
		
		public string UpdateCommand {
			get { return View.UpdateCommand; }
			set { View.UpdateCommand = value; }
		}
		
		public ParameterCollection UpdateParameters {
			get { return View.UpdateParameters; }
		}
		
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
		
		public event SqlDataSourceCommandEventHandler Inserting {
			add { View.Inserting += value; }
			remove { View.Inserting -= value; }
		}
		
		public event SqlDataSourceStatusEventHandler Selected {
			add { View.Selected += value; }
			remove { View.Selected -= value; }
		}
		
		public event SqlDataSourceCommandEventHandler Selecting {
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
					view = new SqlDataSourceView (this, "DefaultView", this.Context);
					view.DataSourceViewChanged += new EventHandler (ViewChanged);
					if (IsTrackingViewState)
						((IStateManager) view).TrackViewState ();
				}
				return view;
			}
		}
		
		void ViewChanged (object source, EventArgs e)
		{
			OnDataSourceChanged (e);
		}
	}
}
#endif

