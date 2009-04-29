//
// MetaTable.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.Routing;
using System.Web.DynamicData.ModelProviders;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class MetaTable
	{
		internal MetaTable (MetaModel model, TableProvider provider, bool scaffold)
		{
			this.model = model;
			Provider = provider;
			Scaffold = scaffold;

			var l = new List<MetaColumn> ();
			foreach (var c in provider.Columns)
				l.Add (new MetaColumn (this, c));
			Columns = new ReadOnlyCollection<MetaColumn> (l);

			DisplayName = Name;
			DataContextType = provider.DataModel.ContextType;
			
			// FIXME: fill more properties.
		}

		MetaModel model;

		[MonoTODO]
		public AttributeCollection Attributes { get; private set; }

		[MonoTODO]
		public ReadOnlyCollection<MetaColumn> Columns { get; private set; }

		public string DataContextPropertyName {
			get { return Provider.Name; }	
		}

		public Type DataContextType { get; private set; }

		[MonoTODO]
		public MetaColumn DisplayColumn { get; private set; }

		[MonoTODO]
		public string DisplayName { get; private set; }

		public Type EntityType {
			get { return Provider.EntityType; }
		}

		[MonoTODO]
		public string ForeignKeyColumnsNames { get; private set; }

		[MonoTODO]
		public bool HasPrimaryKey { get; private set; }

		[MonoTODO]
		public bool IsReadOnly { get; private set; }

		public string ListActionPath {
			get { return GetActionPath ("List"); }
		}

		public MetaModel Model {
			get { return model; }
		}

		public string Name {
			get { return Provider.Name; }
		}

		[MonoTODO]
		public ReadOnlyCollection<MetaColumn> PrimaryKeyColumns { get; private set; }

		public TableProvider Provider { get; private set; }

		public bool Scaffold { get; private set; }

		[MonoTODO]
		public MetaColumn SortColumn { get; private set; }

		[MonoTODO]
		public bool SortDescending { get; private set; }

		[MonoTODO]
		public object CreateContext ()
		{
			return Activator.CreateInstance (EntityType);
		}

		public string GetActionPath (string action)
		{
			// returns /{appbase}/{table}/{action}.aspx
			return String.Concat (HttpContext.Current.Request.ApplicationPath, DisplayName, "/", action, ".aspx");
		}

		[MonoTODO]
		public string GetActionPath (string action, IList<object> primaryKeyValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetActionPath (string action, object row)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetActionPath (string action, RouteValueDictionary routeValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetActionPath (string action, IList<object> primaryKeyValues, string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetActionPath (string action, object row, string path)
		{
			throw new NotImplementedException ();
		}

		public MetaColumn GetColumn (string columnName)
		{
			MetaColumn mc;
			if (TryGetColumn (columnName, out mc))
				return mc;
			throw new ArgumentException (String.Format ("Column '{0}' does not exist in the meta table '{1}'", columnName, Name));
		}

		[MonoTODO]
		public string GetDisplayString (object row)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetPrimaryKeyString (IList<object> primaryKeyValues)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetPrimaryKeyString (object row)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IList<object> GetPrimaryKeyValues (object row)
		{
			throw new NotImplementedException ();
		}

		public IQueryable GetQuery ()
		{
			return GetQuery (CreateContext ());
		}

		[MonoTODO]
		public IQueryable GetQuery (object context)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return Name;
		}

		public bool TryGetColumn (string columnName, out MetaColumn column)
		{
			foreach (var m in Columns)
				if (m.Name == columnName) {
					column = m;
					return true;
				}
			column = null;
			return false;
		}
	}
}
