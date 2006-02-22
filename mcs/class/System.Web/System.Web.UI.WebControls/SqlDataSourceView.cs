//
// System.Web.UI.WebControls.SqlDataSourceView
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2003 Ben Maurer
// (C) Novell, Inc. (http://www.novell.com)
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
using System.Data;
using System.ComponentModel;
using System.Data.Common;

namespace System.Web.UI.WebControls {
	public class SqlDataSourceView : DataSourceView, IStateManager {

		HttpContext context;
		DbProviderFactory factory;
		DbConnection connection;

		public SqlDataSourceView (SqlDataSource owner, string name, HttpContext context)
			: base (owner, name)
		{
			this.owner = owner;
			this.name = name;
			this.context = context;
		}

		void InitConnection ()
		{
			if (connection == null) {
				factory = owner.GetDbProviderFactoryInternal ();
				connection = factory.CreateConnection ();
				connection.ConnectionString = owner.ConnectionString;
			}
		}

		public int Delete (IDictionary keys, IDictionary oldValues)
		{
			return ExecuteDelete (keys, oldValues);
		}
		
		[MonoTODO ("Handle keys, oldValues, parameters and check for path for AccessDBFile")]
		protected override int ExecuteDelete (IDictionary keys, IDictionary oldValues)
		{
			if (!CanDelete)
				throw new NotSupportedException("Delete operation is not supported");
			if (oldValues == null && conflictOptions == ConflictOptions.CompareAllValues)
				throw new InvalidOperationException ("oldValues parameters should be specified when ConflictOptions is set to CompareAllValues");

			InitConnection ();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = DeleteCommand;
			command.Connection = connection;

			OnDeleting (new SqlDataSourceCommandEventArgs (command));

			connection.Open ();
			Exception exception = null; 
			int result = -1;;
			try {
				result = command.ExecuteNonQuery();
			} catch (Exception e) {
				exception = e;
			}

			OnDeleted (new SqlDataSourceStatusEventArgs (command, result, exception));

			if (exception != null)
				throw exception;
			return result;
		}
		
		public int Insert (IDictionary values)
		{
			return Insert (values);
		}
		
		[MonoTODO ("Handle values and parameters")]
		protected override int ExecuteInsert (IDictionary values)
		{
			if (!CanInsert)
				throw new NotSupportedException ("Insert operation is not supported");

			InitConnection ();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = InsertCommand;
			command.Connection = connection;

			OnInserting (new SqlDataSourceCommandEventArgs (command));

			connection.Open();
			Exception exception = null;
			int result = -1;
			try {
				result = command.ExecuteNonQuery();
			}catch (Exception e) {
				exception = e;
			}

			OnInserted (new SqlDataSourceStatusEventArgs (command, result, exception));

			if (exception != null)
				throw exception;
			return result;
		}
				
		public IEnumerable Select (DataSourceSelectArguments arguments)
		{
			return ExecuteSelect (arguments);
		}

		[MonoTODO ("Handle @arguments, do replacement of command parameters")]
		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			DbProviderFactory f = owner.GetDbProviderFactoryInternal ();
			DbConnection c = f.CreateConnection ();

			c.ConnectionString = owner.ConnectionString;

			DbCommand command = f.CreateCommand ();

			command.CommandText = SelectCommand;
			command.Connection = c;
			command.CommandType = CommandType.Text;

			/* XXX do replacement of command parameters */

			OnSelecting (new SqlDataSourceSelectingEventArgs (command, arguments));

			if (owner.DataSourceMode == SqlDataSourceMode.DataSet) {
				DbDataAdapter adapter = f.CreateDataAdapter ();

				adapter.SelectCommand = command;

				DataSet dataset = new DataSet ();

				/* XXX MS calls Fill (DataSet dataset, string srcTable) - find out what the source table is */
				adapter.Fill (dataset, name);

				dataset.WriteXml (Console.Out);

				return dataset.CreateDataReader ();
			}
			else {
				throw new NotImplementedException ();
			}
		}

		public int Update (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			return ExecuteUpdate (keys, values, oldValues);
		}

		[MonoTODO ("Handle keys, values and oldValues")]
		protected override int ExecuteUpdate (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			if (!CanUpdate)
				throw new NotSupportedException ("Update operation is not supported");
			if (oldValues == null && conflictOptions == ConflictOptions.CompareAllValues)
				throw new InvalidOperationException ("oldValues parameters should be specified when ConflictOptions is set to CompareAllValues");

			InitConnection ();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = UpdateCommand;
			command.Connection = connection;

			OnUpdating (new SqlDataSourceCommandEventArgs (command));

			connection.Open ();
			Exception exception = null;
			int result = -1;
			try {
			 	result = command.ExecuteNonQuery ();
			}catch (Exception e) {
				exception = e;
			}

			OnUpdated (new SqlDataSourceStatusEventArgs (command, result, exception));

			if (exception != null)
				throw exception;
			return result;
		}

		void IStateManager.LoadViewState (object savedState)
		{
			LoadViewState (savedState);
		}
		
		object IStateManager.SaveViewState ()
		{
			return SaveViewState ();
		}
		
		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}
		
		protected virtual void LoadViewState (object savedState)
		{
			object [] vs = savedState as object [];
			if (vs == null)
				return;
			
			if (vs [0] != null) ((IStateManager) deleteParameters).LoadViewState (vs [0]);
			if (vs [1] != null) ((IStateManager) filterParameters).LoadViewState (vs [1]);
			if (vs [2] != null) ((IStateManager) insertParameters).LoadViewState (vs [2]);
			if (vs [3] != null) ((IStateManager) selectParameters).LoadViewState (vs [3]);
			if (vs [4] != null) ((IStateManager) updateParameters).LoadViewState (vs [4]);
			if (vs [5] != null) ((IStateManager) ViewState).LoadViewState (vs [5]);
		}

		protected virtual object SaveViewState ()
		{
			object [] vs = new object [6];
			
			if (deleteParameters != null) vs [0] = ((IStateManager) deleteParameters).SaveViewState ();
			if (filterParameters != null) vs [1] = ((IStateManager) filterParameters).SaveViewState ();
			if (insertParameters != null) vs [2] = ((IStateManager) insertParameters).SaveViewState ();
			if (selectParameters != null) vs [3] = ((IStateManager) selectParameters).SaveViewState ();
			if (updateParameters != null) vs [4] = ((IStateManager) updateParameters).SaveViewState ();
			if (viewState != null) vs [5] = ((IStateManager) viewState).SaveViewState ();
				
			foreach (object o in vs)
				if (o != null) return vs;
			return null;
		}
		
		protected virtual void TrackViewState ()
		{
			tracking = true;
			
			if (deleteParameters != null) ((IStateManager) deleteParameters).TrackViewState ();
			if (filterParameters != null) ((IStateManager) filterParameters).TrackViewState ();
			if (insertParameters != null) ((IStateManager) insertParameters).TrackViewState ();
			if (selectParameters != null) ((IStateManager) selectParameters).TrackViewState ();
			if (updateParameters != null) ((IStateManager) updateParameters).TrackViewState ();
			if (viewState != null) ((IStateManager) viewState).TrackViewState ();
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return IsTrackingViewState; }
		}

		bool cancelSelectOnNullParameter = true;
		public bool CancelSelectOnNullParameter {
			get { return cancelSelectOnNullParameter; }
			set { cancelSelectOnNullParameter = value; }
		}

		public override bool CanDelete {
			get { return DeleteCommand != null && DeleteCommand != ""; }
		}

		public override bool CanInsert {
			get { return InsertCommand != null && InsertCommand != ""; }
		}
		
		public override bool CanPage {
			/* according to MS, this is false in all cases */
			get { return false; }
		}

		public override bool CanRetrieveTotalRowCount {
			/* according to MS, this is false in all cases */
			get { return false; }
		}

		public override bool CanSort {
			get {
				/* we can sort if we're a DataSet, regardless of sort parameter name.
				   we can sort if we're a DataReader, if the sort parameter name is not null/"".
				*/
				return (owner.DataSourceMode == SqlDataSourceMode.DataSet
					|| (SortParameterName != null && SortParameterName != ""));
			}
		}
		
		public override bool CanUpdate {
			get { return UpdateCommand != null && UpdateCommand != ""; }
		}

		ConflictOptions conflictOptions = ConflictOptions.OverwriteChanges;
		public ConflictOptions ConflictDetection {
			get { return conflictOptions; }
			set { conflictOptions = value; }
		}

		public string DeleteCommand {
			get { return ViewState.GetString ("DeleteCommand", ""); }
			set { ViewState ["DeleteCommand"] = value; }
		}

		[MonoTODO]
		public SqlDataSourceCommandType DeleteCommandType {
			get { return (SqlDataSourceCommandType) ViewState.GetInt ("DeleteCommandType", (int)SqlDataSourceCommandType.StoredProcedure); }
			set { ViewState ["DeleteCommandType"] = value; }
		}

		[DefaultValueAttribute (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public ParameterCollection DeleteParameters {
			get { return GetParameterCollection (ref deleteParameters); }
		}
		
		public string FilterExpression {
			get { return ViewState.GetString ("FilterExpression", ""); }
			set { ViewState ["FilterExpression"] = value; }
		}

		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
		public ParameterCollection FilterParameters {
			get { return GetParameterCollection (ref filterParameters); }
		}
		
		public string InsertCommand {
			get { return ViewState.GetString ("InsertCommand", ""); }
			set { ViewState ["InsertCommand"] = value; }
		}

		[MonoTODO]
		public SqlDataSourceCommandType InsertCommandType {
			get { return (SqlDataSourceCommandType) ViewState.GetInt ("InsertCommandType", (int)SqlDataSourceCommandType.StoredProcedure); }
			set { ViewState ["InsertCommandType"] = value; }
		}

		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
		public ParameterCollection InsertParameters {
			get { return GetParameterCollection (ref insertParameters); }
		}

		protected bool IsTrackingViewState {
			get { return tracking; }
		}

		[MonoTODO]
		[DefaultValue ("{0}")]
		public string OldValuesParameterFormatString {
			get { return ViewState.GetString ("OldValuesParameterFormatString", "{0}"); }
			set { ViewState ["OldValuesParameterFormatString"] = value; }
		}
		
		public string SelectCommand {
			get { return ViewState.GetString ("SelectCommand", ""); }
			set { ViewState ["SelectCommand"] = value; }
		}

		[MonoTODO]
		public SqlDataSourceCommandType SelectCommandType {
			get { return (SqlDataSourceCommandType) ViewState.GetInt ("SelectCommandType", (int)SqlDataSourceCommandType.StoredProcedure); }
			set { ViewState ["SelectCommandType"] = value; }
		}
		
		public ParameterCollection SelectParameters {
			get { return GetParameterCollection (ref selectParameters); }
		}

		[MonoTODO]
		public string SortParameterName {
			get { return ViewState.GetString ("SortParameterName", ""); }
			set { ViewState ["SortParameterName"] = value; }
		}

		public string UpdateCommand {
			get { return ViewState.GetString ("UpdateCommand", ""); }
			set { ViewState ["UpdateCommand"] = value; }
		}

		[MonoTODO]
		public SqlDataSourceCommandType UpdateCommandType {
			get { return (SqlDataSourceCommandType) ViewState.GetInt ("UpdateCommandType", (int)SqlDataSourceCommandType.StoredProcedure); }
			set { ViewState ["UpdateCommandType"] = value; }
		}

		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
		public ParameterCollection UpdateParameters {
			get { return GetParameterCollection (ref updateParameters); }
		}
		
		void ParametersChanged (object source, EventArgs args)
		{
			OnDataSourceViewChanged (EventArgs.Empty);
		}
		
		ParameterCollection GetParameterCollection (ref ParameterCollection output)
		{
			if (output != null)
				return output;
			
			output = new ParameterCollection ();
			output.ParametersChanged += new EventHandler (ParametersChanged);
			
			if (IsTrackingViewState)
				((IStateManager) output).TrackViewState ();
			
			return output;
		}
		
		protected virtual string ParameterPrefix {
			get { return "@"; }
		}

		StateBag viewState;
		private StateBag ViewState {
			get {
				if (viewState != null)
					return viewState;

				viewState = new StateBag ();
				if (IsTrackingViewState)
					viewState.TrackViewState ();

				return viewState;
			}
		}

		ParameterCollection deleteParameters;
		ParameterCollection filterParameters;
		ParameterCollection insertParameters;
		ParameterCollection selectParameters;
		ParameterCollection updateParameters;

		bool tracking;
	
		string name;
		SqlDataSource owner;

		#region OnDelete
		static readonly object EventDeleted = new object ();
		protected virtual void OnDeleted (SqlDataSourceStatusEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceStatusEventHandler h = Events [EventDeleted] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Deleted {
			add { Events.AddHandler (EventDeleted, value); }
			remove { Events.RemoveHandler (EventDeleted, value); }
		}
		
		static readonly object EventDeleting = new object ();
		protected virtual void OnDeleting (SqlDataSourceCommandEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceCommandEventHandler h = Events [EventDeleting] as SqlDataSourceCommandEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceCommandEventHandler Deleting {
			add { Events.AddHandler (EventDeleting, value); }
			remove { Events.RemoveHandler (EventDeleting, value); }
		}
		#endregion

		#region OnFiltering
		static readonly object EventFiltering = new object ();
		protected virtual void OnFiltering (SqlDataSourceFilteringEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceFilteringEventHandler h = Events [EventFiltering] as SqlDataSourceFilteringEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceFilteringEventHandler Filtering {
			add { Events.AddHandler (EventFiltering, value); }
			remove { Events.RemoveHandler (EventFiltering, value); }
		}
		#endregion
		
		#region OnInsert
		static readonly object EventInserted = new object ();
		protected virtual void OnInserted (SqlDataSourceStatusEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceStatusEventHandler h = Events [EventInserted] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Inserted {
			add { Events.AddHandler (EventInserted, value); }
			remove { Events.RemoveHandler (EventInserted, value); }
		}
		
		static readonly object EventInserting = new object ();
		protected virtual void OnInserting (SqlDataSourceCommandEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceCommandEventHandler h = Events [EventInserting] as SqlDataSourceCommandEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceCommandEventHandler Inserting {
			add { Events.AddHandler (EventInserting, value); }
			remove { Events.RemoveHandler (EventInserting, value); }
		}
		#endregion
		
		#region OnSelect
		static readonly object EventSelected = new object ();
		protected virtual void OnSelected (SqlDataSourceStatusEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceStatusEventHandler h = Events [EventSelected] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Selected {
			add { Events.AddHandler (EventSelected, value); }
			remove { Events.RemoveHandler (EventSelected, value); }
		}
		
		static readonly object EventSelecting = new object ();
		protected virtual void OnSelecting (SqlDataSourceSelectingEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceSelectingEventHandler h = Events [EventSelecting] as SqlDataSourceSelectingEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceSelectingEventHandler Selecting {
			add { Events.AddHandler (EventSelecting, value); }
			remove { Events.RemoveHandler (EventSelecting, value); }
		}
		#endregion
		
		#region OnUpdate
		static readonly object EventUpdated = new object ();
		protected virtual void OnUpdated (SqlDataSourceStatusEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceStatusEventHandler h = Events [EventUpdated] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Updated {
			add { Events.AddHandler (EventUpdated, value); }
			remove { Events.RemoveHandler (EventUpdated, value); }
		}
		
		static readonly object EventUpdating = new object ();
		protected virtual void OnUpdating (SqlDataSourceCommandEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceCommandEventHandler h = Events [EventUpdating] as SqlDataSourceCommandEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceCommandEventHandler Updating {
			add { Events.AddHandler (EventUpdating, value); }
			remove { Events.RemoveHandler (EventUpdating, value); }
		}
		#endregion
				
	}
	
}
#endif

