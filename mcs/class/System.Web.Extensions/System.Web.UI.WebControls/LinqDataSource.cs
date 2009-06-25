//
// LinqDataSource.cs
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
using System.Security.Permissions;
using System.Web;
using System.Web.DynamicData;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
//	[ToolboxBitmap (typeof (LinqDataSource), "LinqDataSource.ico")]
	[PersistChildren (false)]
	[ParseChildren (true)]
	[Designer ("System.Web.UI.Design.WebControls.LinqDataSourceDesigner, System.Web.Extensions.Design, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
	[DefaultEvent ("Selecting")]
	[ToolboxItemFilter ("System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", ToolboxItemFilterType.Require)]
	[DefaultProperty ("ContextTypeName")]
	public class LinqDataSource : DataSourceControl, IDynamicDataSource
	{
		static readonly string [] empty_names = new string [] { "DefaultView" };

		public LinqDataSource ()
		{
		}

		protected internal override void OnInit (EventArgs e)
		{
			Page.LoadComplete += OnPageLoadComplete;
		}

		void OnPageLoadComplete (object sender, EventArgs e)
		{
			SelectParameters.UpdateValues (Context, this);
			WhereParameters.UpdateValues (Context, this);
			GroupByParameters.UpdateValues (Context, this);
			OrderByParameters.UpdateValues (Context, this);
			OrderGroupsByParameters.UpdateValues (Context, this);
		}

		protected internal override void OnUnload (EventArgs e)
		{
			base.OnUnload (e);

			// no further things to do?
		}

		#region View

		LinqDataSourceView view;
		LinqDataSourceView View {
			get {
				if (view == null) {
					view = CreateView ();
					if (IsTrackingViewState)
						((IStateManager) view).TrackViewState ();
				}
				return view;
			}
		}

		protected virtual LinqDataSourceView CreateView ()
		{
			var view = new LinqDataSourceView (this, Guid.NewGuid ().ToString (), HttpContext.Current);
			if (IsTrackingViewState)
				((IStateManager) view).TrackViewState ();
			return view;
		}

		protected override DataSourceView GetView (string viewName)
		{
			if (String.IsNullOrEmpty (viewName) || (String.Compare (viewName, empty_names [0], StringComparison.InvariantCultureIgnoreCase) == 0))
				return View;
			throw new ArgumentException ("viewName must be 'DefaultView' in LinqDataSource");
		}

		protected override ICollection GetViewNames ()
		{
			return empty_names;
		}

		#endregion

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool AutoGenerateOrderByClause {
			get { return View.AutoGenerateOrderByClause; }
			set { View.AutoGenerateOrderByClause = value; }
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool AutoGenerateWhereClause {
			get { return View.AutoGenerateWhereClause; }
			set { View.AutoGenerateWhereClause = value; }
		}

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool AutoPage {
			get { return View.AutoPage; }
			set { View.AutoPage = value; }
		}

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool AutoSort {
			get { return View.AutoSort; }
			set { View.AutoSort = value; }
		}

		[Category ("Data")]
		[DefaultValue ("")]
		public string ContextTypeName { get; set; }

		Type context_type;

		[MonoTODO ("looks like we need System.Web.Query.Dynamic stuff or alternative")]
		Type IDynamicDataSource.ContextType {
			get {
				if (context_type != null && context_type.FullName == ContextTypeName)
					return context_type;

				if (String.IsNullOrEmpty (ContextTypeName))
					return null;
				
				// FIXME: retrieve type from BuildProvider or whatever.
				context_type = LoadType (ContextTypeName);
				if (context_type == null)
					throw new ArgumentException (String.Format ("Type '{0}' cannot be loaded", ContextTypeName));
				return context_type;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				context_type = value;
				ContextTypeName = context_type.FullName;
			}
		}

		private Type LoadType (string name)
		{
			foreach (var ass in AppDomain.CurrentDomain.GetAssemblies ())
				foreach (var type in ass.GetTypes ())
					if (type.FullName == name)
						return type;
			return null;
		}

		string IDynamicDataSource.EntitySetName {
			get { return TableName; }
			set { TableName = value; }
		}

		[Category ("Data")]
		[DefaultValue ("")]
		public string TableName {
			get { return View.TableName; }
			set { View.TableName = value; }
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnableDelete {
			get { return View.EnableDelete; }
			set { View.EnableDelete = value; }
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnableInsert {
			get { return View.EnableInsert; }
			set { View.EnableInsert = value; }
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnableObjectTracking {
			get { return View.EnableObjectTracking; }
			set { View.EnableObjectTracking = value; }
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnableUpdate {
			get { return View.EnableUpdate; }
			set { View.EnableUpdate = value; }
		}

		[Category ("Data")]
		[DefaultValue ("")]
		public string GroupBy {
			get { return View.GroupBy; }
			set { View.GroupBy = value; }
		}

		[Category ("Data")]
		[DefaultValue ("")]
		public string OrderBy {
			get { return View.OrderBy; }
			set { View.OrderBy = value; }
		}

		[Category ("Data")]
		[DefaultValue ("")]
		public string OrderGroupsBy {
			get { return View.OrderGroupsBy; }
			set { View.OrderGroupsBy = value; }
		}

		[Category ("Data")]
		[DefaultValue ("")]
		public string Select {
			get { return View.SelectNew; }
			set { View.SelectNew = value; }
		}

		[Category ("Data")]
		[DefaultValue ("")]
		public string Where {
			get { return View.Where; }
			set { View.Where = value; }
		}

		[MergableProperty (false)]
		[Category ("Data")]
		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ParameterCollection SelectParameters {
			get { return View.SelectNewParameters; }
		}

		[MergableProperty (false)]
		[Category ("Data")]
		[DefaultValue (null)]
		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public ParameterCollection WhereParameters {
			get { return View.WhereParameters; }
		}

		[MergableProperty (false)]
		[Category ("Data")]
		[DefaultValue (null)]
		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public ParameterCollection GroupByParameters {
			get { return View.GroupByParameters; }
		}

		[MergableProperty (false)]
		[Category ("Data")]
		[DefaultValue (null)]
		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public ParameterCollection OrderByParameters {
			get { return View.OrderByParameters; }
		}

		[MergableProperty (false)]
		[Category ("Data")]
		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ParameterCollection OrderGroupsByParameters {
			get { return View.OrderGroupsByParameters; }
		}

		[MergableProperty (false)]
		[Category ("Data")]
		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ParameterCollection DeleteParameters {
			get { return View.DeleteParameters; }
		}

		[MergableProperty (false)]
		[Category ("Data")]
		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ParameterCollection InsertParameters {
			get { return View.InsertParameters; }
		}

		[MergableProperty (false)]
		[Category ("Data")]
		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ParameterCollection UpdateParameters {
			get { return View.UpdateParameters; }
		}

		public int Delete (IDictionary keys, IDictionary oldValues)
		{
			return View.Delete (keys, oldValues);
		}

		public int Insert (IDictionary values)
		{
			return View.Insert (values);
		}

		public int Update (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			return View.Update (keys, values, oldValues);
		}

		#region ViewState

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool StoreOriginalValuesInViewState {
			get { return View.StoreOriginalValuesInViewState; }
			set { View.StoreOriginalValuesInViewState = value; }
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

		#endregion

		#region Events (Dispatching)

		public event EventHandler<LinqDataSourceStatusEventArgs> ContextCreated {
			add { View.ContextCreated += value; }
			remove { View.ContextCreated -= value; }
		}

		public event EventHandler<LinqDataSourceContextEventArgs> ContextCreating {
			add { View.ContextCreating += value; }
			remove { View.ContextCreating -= value; }
		}

		public event EventHandler<LinqDataSourceDisposeEventArgs> ContextDisposing {
			add { View.ContextDisposing += value; }
			remove { View.ContextDisposing -= value; }
		}

		public event EventHandler<LinqDataSourceStatusEventArgs> Deleted {
			add { View.Deleted += value; }
			remove { View.Deleted -= value; }
		}

		public event EventHandler<LinqDataSourceDeleteEventArgs> Deleting {
			add { View.Deleting += value; }
			remove { View.Deleting -= value; }
		}

		event EventHandler<DynamicValidatorEventArgs> IDynamicDataSource.Exception {
			add { View.Exception += value; }
			remove { View.Exception -= value; }
		}

		public event EventHandler<LinqDataSourceStatusEventArgs> Inserted {
			add { View.Inserted += value; }
			remove { View.Inserted -= value; }
		}

		public event EventHandler<LinqDataSourceInsertEventArgs> Inserting {
			add { View.Inserting += value; }
			remove { View.Inserting -= value; }
		}

		public event EventHandler<LinqDataSourceStatusEventArgs> Selected {
			add { View.Selected += value; }
			remove { View.Selected -= value; }
		}

		public event EventHandler<LinqDataSourceSelectEventArgs> Selecting {
			add { View.Selecting += value; }
			remove { View.Selecting -= value; }
		}

		public event EventHandler<LinqDataSourceStatusEventArgs> Updated {
			add { View.Updated += value; }
			remove { View.Updated -= value; }
		}

		public event EventHandler<LinqDataSourceUpdateEventArgs> Updating {
			add { View.Updating += value; }
			remove { View.Updating -= value; }
		}

		#endregion
	}
}
#endif
