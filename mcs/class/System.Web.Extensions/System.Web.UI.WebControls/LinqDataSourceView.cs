//
// LinqDataSourceView.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc  http://novell.com
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
#if NET_3_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Security.Permissions;
using System.Web.DynamicData;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class LinqDataSourceView : DataSourceView, IStateManager
	{
		public LinqDataSourceView (LinqDataSource owner, string name, HttpContext context)
			: base (owner, name)
		{
			source = owner;
		}

		LinqDataSource source;

		bool tracking;
		ParameterCollection delete_parameters,
			insert_parameters,
			select_new_parameters,
			update_parameters;
		ParameterCollection group_by_parameters,
			order_by_parameters,
			order_group_by_parameters,
			where_parameters;

		IEnumerable<ParameterCollection> AllParameters {
			get {
				yield return delete_parameters;
				yield return insert_parameters;
				yield return select_new_parameters;
				yield return update_parameters;
				yield return group_by_parameters;
				yield return order_by_parameters;
				yield return order_group_by_parameters;
				yield return where_parameters;
			}
		}

		[MonoTODO]
		public bool AutoGenerateOrderByClause { get; set; }
		[MonoTODO]
		public bool AutoGenerateWhereClause { get; set; }
		[MonoTODO]
		public bool AutoPage { get; set; }
		[MonoTODO]
		public bool AutoSort { get; set; }

		public override bool CanDelete {
			get { return EnableDelete; }
		}

		public override bool CanInsert {
			get { return EnableInsert; }
		}

		public override bool CanPage {
			get { return true; }
		}

		public override bool CanRetrieveTotalRowCount {
			get { return true; }
		}

		public override bool CanSort {
			get { return true; }
		}

		public override bool CanUpdate {
			get { return EnableUpdate; }
		}

		protected virtual Type ContextType {
			get { return ((IDynamicDataSource) source).ContextType; }
		}

		public virtual string ContextTypeName {
			get { return source.ContextTypeName; }
			set { source.ContextTypeName = value; }
		}

		[MonoTODO]
		public bool EnableDelete { get; set; }
		[MonoTODO]
		public bool EnableInsert { get; set; }
		[MonoTODO]
		public bool EnableObjectTracking { get; set; }
		[MonoTODO]
		public bool EnableUpdate { get; set; }

		[MonoTODO]
		public string TableName { get; set; }

		[MonoTODO]
		public string GroupBy { get; set; }

		[MonoTODO]
		public string OrderBy { get; set; }

		[MonoTODO]
		public string OrderGroupsBy { get; set; }

		[MonoTODO]
		public string SelectNew { get; set; }

		[MonoTODO]
		public string Where { get; set; }

		public ParameterCollection DeleteParameters {
			get { return GetParameterCollection (ref delete_parameters, false, false); }
		}

		public ParameterCollection InsertParameters {
			get { return GetParameterCollection (ref insert_parameters, false, false); }
		}

		public ParameterCollection UpdateParameters {
			get { return GetParameterCollection (ref update_parameters, true, true); }
		}

		public ParameterCollection SelectNewParameters {
			get { return GetParameterCollection (ref select_new_parameters, false, false); }
		}

		public ParameterCollection WhereParameters {
			get { return GetParameterCollection (ref where_parameters, true, true); }
		}

		public ParameterCollection GroupByParameters {
			get { return GetParameterCollection (ref group_by_parameters, true, true); }
		}

		public ParameterCollection OrderByParameters {
			get { return GetParameterCollection (ref order_by_parameters, true, true); }
		}

		public ParameterCollection OrderGroupsByParameters {
			get { return GetParameterCollection (ref order_group_by_parameters, true, true); }
		}

		ParameterCollection GetParameterCollection (ref ParameterCollection output, bool propagateTrackViewState, bool subscribeChanged)
		{
			if (output != null)
				return output;
			
			output = new ParameterCollection ();
			if (subscribeChanged)
				output.ParametersChanged += new EventHandler (ParametersChanged);
			
			if (IsTrackingViewState && propagateTrackViewState)
				((IStateManager) output).TrackViewState ();
			
			return output;
		}

		void ParametersChanged (object source, EventArgs args)
		{
			OnDataSourceViewChanged (EventArgs.Empty);
		}

		public int Delete (IDictionary keys, IDictionary oldValues)
		{
			return ExecuteDelete (keys, oldValues);
		}

		public int Insert (IDictionary values)
		{
			return ExecuteInsert (values);
		}

		public IEnumerable Select (DataSourceSelectArguments arguments)
		{
			return ExecuteSelect (arguments);
		}

		public int Update (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			return ExecuteUpdate (keys, values, oldValues);
		}

		[MonoTODO]
		protected override int ExecuteDelete (IDictionary keys, IDictionary oldValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override int ExecuteInsert (IDictionary values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override int ExecuteUpdate (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void DeleteDataObject (object dataContext, object table, object oldDataObject)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void InsertDataObject (object dataContext, object table, object newDataObject)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void ResetDataObject (object table, object dataObject)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void UpdateDataObject (object dataContext, object table, object oldDataObject, object newDataObject)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual object CreateContext (Type contextType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual Type GetDataObjectType (Type tableType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual MemberInfo GetTableMemberInfo (Type contextType)
		{
			throw new NotImplementedException ();
		}

		#region Validation

		[MonoTODO]
		protected virtual void ValidateContextType (Type contextType, bool selecting)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void ValidateDeleteSupported (IDictionary keys, IDictionary oldValues)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void ValidateInsertSupported (IDictionary values)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void ValidateOrderByParameter (string name, string value)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void ValidateParameterName (string name)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void ValidateTableType (Type tableType, bool selecting)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void ValidateUpdateSupported (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region ViewState

		bool IStateManager.IsTrackingViewState {
			get { return IsTrackingViewState; }
		}

		protected bool IsTrackingViewState {
			get { return tracking; }
		}

		[MonoTODO]
		public bool StoreOriginalValuesInViewState { get; set; }

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
			int i = 0;
			foreach (var p in AllParameters) {
				if (vs [i] != null)
					((IStateManager) p).LoadViewState (vs [i]);
				i++;
			}
		}

		protected virtual object SaveViewState ()
		{
			object [] vs = new object [8];
			int i = 0;
			foreach (var p in AllParameters) {
				if (p != null)
					vs [i] = ((IStateManager) p).SaveViewState ();
				i++;
			}

			foreach (object o in vs)
				if (o != null)
					return vs;
			return null;
		}

		protected virtual void TrackViewState ()
		{
			tracking = true;

			foreach (var p in AllParameters)
				if (p != null)
					((IStateManager) p).TrackViewState ();
		}

		#endregion

		#region Events and Overridable Event Handler Invocations

		[MonoTODO]
		public event EventHandler<LinqDataSourceStatusEventArgs> ContextCreated;
		[MonoTODO]
		public event EventHandler<LinqDataSourceContextEventArgs> ContextCreating;
		[MonoTODO]
		public event EventHandler<LinqDataSourceDisposeEventArgs> ContextDisposing;
		[MonoTODO]
		public event EventHandler<LinqDataSourceStatusEventArgs> Deleted;
		[MonoTODO]
		public event EventHandler<LinqDataSourceDeleteEventArgs> Deleting;
		[MonoTODO]
		internal event EventHandler<DynamicValidatorEventArgs> Exception;
		[MonoTODO]
		public event EventHandler<LinqDataSourceStatusEventArgs> Inserted;
		[MonoTODO]
		public event EventHandler<LinqDataSourceInsertEventArgs> Inserting;
		[MonoTODO]
		public event EventHandler<LinqDataSourceStatusEventArgs> Selected;
		[MonoTODO]
		public event EventHandler<LinqDataSourceSelectEventArgs> Selecting;
		[MonoTODO]
		public event EventHandler<LinqDataSourceStatusEventArgs> Updated;
		[MonoTODO]
		public event EventHandler<LinqDataSourceUpdateEventArgs> Updating;

		protected virtual void OnContextCreated (LinqDataSourceStatusEventArgs e)
		{
			if (ContextCreated != null)
				ContextCreated (this, e);
		}

		protected virtual void OnContextCreating (LinqDataSourceContextEventArgs e)
		{
			if (ContextCreating != null)
				ContextCreating (this, e);
		}

		protected virtual void OnContextDisposing (LinqDataSourceDisposeEventArgs e)
		{
			if (ContextDisposing != null)
				ContextDisposing (this, e);
		}

		protected virtual void OnDeleted (LinqDataSourceStatusEventArgs e)
		{
			if (Deleted != null)
				Deleted (this, e);
		}

		protected virtual void OnDeleting (LinqDataSourceDeleteEventArgs e)
		{
			if (Deleting != null)
				Deleting (this, e);
		}

		protected virtual void OnException (DynamicValidatorEventArgs e)
		{
			if (Exception != null)
				Exception (this, e);
		}

		protected virtual void OnInserted (LinqDataSourceStatusEventArgs e)
		{
			if (Inserted != null)
				Inserted (this, e);
		}

		protected virtual void OnInserting (LinqDataSourceInsertEventArgs e)
		{
			if (Inserting != null)
				Inserting (this, e);
		}

		protected virtual void OnSelected (LinqDataSourceStatusEventArgs e)
		{
			if (Selected != null)
				Selected (this, e);
		}

		protected virtual void OnSelecting (LinqDataSourceSelectEventArgs e)
		{
			if (Selecting != null)
				Selecting (this, e);
		}

		protected virtual void OnUpdated (LinqDataSourceStatusEventArgs e)
		{
			if (Updated != null)
				Updated (this, e);
		}

		protected virtual void OnUpdating (LinqDataSourceUpdateEventArgs e)
		{
			if (Updating != null)
				Updating (this, e);
		}

		#endregion
	}
}
#endif
