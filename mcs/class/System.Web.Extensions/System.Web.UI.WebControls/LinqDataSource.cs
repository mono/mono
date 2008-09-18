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
using System.Web.DynamicData;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
//	[ToolboxBitmap (typeof (LinqDataSource), "LinqDataSource.ico")]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class LinqDataSource : DataSourceControl, IDynamicDataSource
	{
		public LinqDataSource ()
		{
		}

		[MonoTODO]
		protected virtual LinqDataSourceView CreateView ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Delete (IDictionary keys, IDictionary oldValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override DataSourceView GetView (string viewName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override ICollection GetViewNames ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Insert (IDictionary values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void LoadViewState (object savedState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void OnInit (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void OnUnload (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override object SaveViewState ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void TrackViewState ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Update (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool AutoGenerateOrderByClause { get; set; }
		[MonoTODO]
		public bool AutoGenerateWhereClause { get; set; }
		[MonoTODO]
		public bool AutoPage { get; set; }
		[MonoTODO]
		public bool AutoSort { get; set; }
		[MonoTODO]
		public string ContextTypeName { get; set; }
		[MonoTODO]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ParameterCollection DeleteParameters { get; private set; }
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
		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public ParameterCollection GroupByParameters { get; private set; }
		[MonoTODO]
		Type IDynamicDataSource.ContextType { get; set; }
		[MonoTODO]
		string IDynamicDataSource.EntitySetName { get; set; }
		[MonoTODO]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ParameterCollection InsertParameters { get; private set; }
		[MonoTODO]
		public string OrderBy { get; set; }
		[MonoTODO]
		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public ParameterCollection OrderByParameters { get; private set; }
		[MonoTODO]
		public string OrderGroupsBy { get; set; }
		[MonoTODO]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ParameterCollection OrderGroupsByParameters { get; private set; }
		[MonoTODO]
		public string Select { get; set; }
		[MonoTODO]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ParameterCollection SelectParameters { get; private set; }
		[MonoTODO]
		public bool StoreOriginalValuesInViewState { get; set; }
		[MonoTODO]
		public string TableName { get; set; }
		[MonoTODO]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ParameterCollection UpdateParameters { get; private set; }
		[MonoTODO]
		public string Where { get; set; }
		[MonoTODO]
		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public ParameterCollection WhereParameters { get; private set; }

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
		event EventHandler<DynamicValidatorEventArgs> IDynamicDataSource.Exception {
			add { throw new NotImplementedException (); }
			remove { throw new NotImplementedException (); }
		}
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
	}
}
#endif
