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
using System.Data.SqlClient;

namespace System.Web.UI.WebControls {
	public class SqlDataSourceView : DataSourceView, IStateManager {

		SqlCommand command;
		SqlConnection connection;

		public SqlDataSourceView (SqlDataSource owner, string name)
		{
			this.owner = owner;
			this.name = name;
			connection = new SqlConnection (owner.ConnectionString);
		}

		public int Delete (IDictionary keys, IDictionary oldValues)
		{
			return ExecuteDelete (keys, oldValues);
		}
		
		[MonoTODO ("Handle keys and oldValues and parameters")]
		protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues)
		{
			command = new SqlCommand (this.DeleteCommand, connection);
			connection.Open ();
			int result = command.ExecuteNonQuery ();
			connection.Close ();
			return result;			
		}
		
		public int Insert (IDictionary values)
		{
			return Insert (values);
		}
		
		[MonoTODO ("Handle values and parameters")]
		protected override int ExecuteInsert (IDictionary values)
		{
			command = new SqlCommand (this.InsertCommand, connection);
			connection.Open ();
			int result = command.ExecuteNonQuery ();
			connection.Close ();
			return result;
		}
				
		public IEnumerable Select (DataSourceSelectArguments arguments)
		{
			
			return ExecuteSelect (arguments);
		}

		[MonoTODO("Extra method to keep things compiling, need to remove later")]
		public override IEnumerable Select()
		{
			throw new NotImplementedException ("Not required");
		}

		[MonoTODO ("Handle arguments")]
		protected internal override IEnumerable ExecuteSelect (
						DataSourceSelectArguments arguments)
		{
			command = new SqlCommand (this.SelectCommand, connection);
			SqlDataSourceCommandEventArgs cmdEventArgs = new SqlDataSourceCommandEventArgs (command);
			OnSelecting (cmdEventArgs);
			connection.Open ();
			SqlDataReader reader = command.ExecuteReader ();
			int resultCount =0;
			/*while (reader.Read ())
				resultCount++;
			Console.WriteLine ("reader returned "+resultCount);*/
			IEnumerable enums = null; 
			Exception exception = null;
			try {
				//enums = reader.GetEnumerator();
				throw new NotImplementedException ("SqlDataReader doesnt implements GetEnumerator method yet");
			} catch (Exception e) {
				exception = e;
			}
			SqlDataSourceStatusEventArgs statusEventArgs = 
				new SqlDataSourceStatusEventArgs (command, reader.RecordsAffected, exception);
			OnSelected (statusEventArgs);
			return enums;
			
		}

		public int Update(IDictionary keys, IDictionary values,
			IDictionary oldValues)
		{
			return ExecuteUpdate (keys, values, oldValues);
		}

		[MonoTODO ("Handle keys, values and oldValues")]
		protected override int ExecuteUpdate (IDictionary keys,
					IDictionary values, IDictionary oldValues)
		{
			command = new SqlCommand (this.UpdateCommand, connection);
			connection.Open ();
			int result = command.ExecuteNonQuery();
			connection.Close();
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
			if (vs [5] != null) ((IStateManager) viewState).LoadViewState (vs [5]);
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
		
		protected bool IsTrackingViewState {
			get { return tracking; }
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return IsTrackingViewState; }
		}
		
		public string DeleteCommand {
			get {
				string val = ViewState ["DeleteCommand"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["DeleteCommand"] = value; }
		}
		
		public string FilterExpression {
			get {
				string val = ViewState ["FilterExpression"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["FilterExpression"] = value; }
		}
		
		public string InsertCommand {
			get {
				string val = ViewState ["InsertCommand"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["InsertCommand"] = value; }
		}
		
		public string SelectCommand {
			get {
				string val = ViewState ["SelectCommand"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["SelectCommand"] = value; }
		}
		
		public string UpdateCommand {
			get {
				string val = ViewState ["UpdateCommand"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["UpdateCommand"] = value; }
		}
		
		public string SortExpression {
			get {
				string val = ViewState ["SortExpression"] as string;
				return val == null ? "" : val;
			}
			set { ViewState ["SortExpression"] = value; }
		}
		
		public override bool CanDelete {
			get { return DeleteCommand != ""; }
		}
		
		public override bool CanInsert {
			get { return UpdateCommand != ""; }
		}
		
		public override bool CanSort {
			get { return owner.DataSourceMode == SqlDataSourceMode.DataSet; }
		}
		
		public override bool CanUpdate {
			get { return UpdateCommand != ""; }
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
		
		public ParameterCollection DeleteParameters {
			get { return GetParameterCollection (ref deleteParameters); }
		}
		
		public ParameterCollection FilterParameters {
			get { return GetParameterCollection (ref filterParameters); }
		}
		
		public ParameterCollection InsertParameters {
			get { return GetParameterCollection (ref insertParameters); }
		}
		
		public ParameterCollection SelectParameters {
			get { return GetParameterCollection (ref selectParameters); }
		}
		
		public ParameterCollection UpdateParameters {
			get { return GetParameterCollection (ref updateParameters); }
		}
		
		
		public override string Name {
			get { return name; }
		}
		
		protected virtual string ParameterPrefix {
			get { return "@"; }
		}

		StateBag viewState;
		protected StateBag ViewState {
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
		protected virtual void OnSelecting (SqlDataSourceCommandEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceCommandEventHandler h = Events [EventSelecting] as SqlDataSourceCommandEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceCommandEventHandler Selecting {
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

