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
		[MonoTODO]
		public LinqDataSourceView (LinqDataSource owner, string name, HttpContext context)
			: base (owner, name)
		{
			throw new NotImplementedException ();
		}

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

		[MonoTODO]
		public bool AutoGenerateOrderByClause { get; set; }
		[MonoTODO]
		public bool AutoGenerateWhereClause { get; set; }
		[MonoTODO]
		public bool AutoPage { get; set; }
		[MonoTODO]
		public bool AutoSort { get; set; }
		[MonoTODO]
		public override bool CanDelete {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override bool CanInsert {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override bool CanPage {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override bool CanRetrieveTotalRowCount {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override bool CanSort {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override bool CanUpdate {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		protected virtual Type ContextType {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public virtual string ContextTypeName { get; set; }
		[MonoTODO]
		public ParameterCollection DeleteParameters {
			get { throw new NotImplementedException (); }
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
		public string GroupBy { get; set; }
		[MonoTODO]
		public ParameterCollection GroupByParameters {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public ParameterCollection InsertParameters {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		bool IStateManager.IsTrackingViewState {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		protected bool IsTrackingViewState {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public string OrderBy { get; set; }
		[MonoTODO]
		public ParameterCollection OrderByParameters {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public string OrderGroupsBy { get; set; }
		[MonoTODO]
		public ParameterCollection OrderGroupsByParameters {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public string SelectNew { get; set; }
		[MonoTODO]
		public ParameterCollection SelectNewParameters {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public bool StoreOriginalValuesInViewState { get; set; }
		[MonoTODO]
		public string TableName { get; set; }
		[MonoTODO]
		public ParameterCollection UpdateParameters {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public string Where { get; set; }
		[MonoTODO]
		public ParameterCollection WhereParameters {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected virtual object CreateContext (Type contextType)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Delete (IDictionary keys, IDictionary oldValues)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void DeleteDataObject (object dataContext, object table, object oldDataObject)
		{
			throw new NotImplementedException ();
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
		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override int ExecuteUpdate (IDictionary keys, IDictionary values, IDictionary oldValues)
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
		[MonoTODO]
		public int Insert (IDictionary values)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void InsertDataObject (object dataContext, object table, object newDataObject)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		void IStateManager.LoadViewState (object savedState)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		object IStateManager.SaveViewState ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		void IStateManager.TrackViewState ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void LoadViewState (object savedState)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnContextCreated (LinqDataSourceStatusEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnContextCreating (LinqDataSourceContextEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnContextDisposing (LinqDataSourceDisposeEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnDeleted (LinqDataSourceStatusEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnDeleting (LinqDataSourceDeleteEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnException (DynamicValidatorEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnInserted (LinqDataSourceStatusEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnInserting (LinqDataSourceInsertEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnSelected (LinqDataSourceStatusEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnSelecting (LinqDataSourceSelectEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnUpdated (LinqDataSourceStatusEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnUpdating (LinqDataSourceUpdateEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void ResetDataObject (object table, object dataObject)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual object SaveViewState ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IEnumerable Select (DataSourceSelectArguments arguments)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void TrackViewState ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Update (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void UpdateDataObject (object dataContext, object table, object oldDataObject, object newDataObject)
		{
			throw new NotImplementedException ();
		}
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
	}
}
#endif
