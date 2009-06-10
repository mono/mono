//
// MetaTable.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2008-2009 Novell Inc. http://novell.com
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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Web.Caching;
using System.Web.UI;
using System.Web.Routing;
using System.Web.DynamicData.ModelProviders;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class MetaTable
	{
		RouteCollection routes;
		MetaColumn displayColumn;
		bool? entityHasToString;
		global::System.ComponentModel.AttributeCollection attributes;
		string displayName;
		
		internal MetaTable (MetaModel model, TableProvider provider, bool scaffold)
		{
			this.model = model;
			Provider = provider;
			Scaffold = scaffold;

			var columns = new List <MetaColumn> ();
			var primaryKeyColumns = new List <MetaColumn> ();
			var foreignKeyColumnNames = new List <string> ();
			
			foreach (var c in provider.Columns) {
				var mc = new MetaColumn (this, c);
				columns.Add (mc);
				if (c.IsPrimaryKey)
					primaryKeyColumns.Add (mc);

				// TODO: check it - doesn't seem to work that way on .NET
				if (c.IsForeignKeyComponent)
					foreignKeyColumnNames.Add (c.Name);
			}
			
			Columns = new ReadOnlyCollection <MetaColumn> (columns);
			PrimaryKeyColumns = new ReadOnlyCollection <MetaColumn> (primaryKeyColumns);
			if (foreignKeyColumnNames.Count == 0)
				ForeignKeyColumnsNames = String.Empty;
			else
				ForeignKeyColumnsNames = String.Join (",", foreignKeyColumnNames.ToArray ());
			
			DataContextType = provider.DataModel.ContextType;
			HasPrimaryKey = primaryKeyColumns.Count > 0;

			// FIXME: fill more properties.
		}

		MetaModel model;

		public global::System.ComponentModel.AttributeCollection Attributes {
			get {
				if (attributes == null) {
					attributes = TypeDescriptor.GetAttributes (EntityType);
					if (attributes == null)
						attributes = new global::System.ComponentModel.AttributeCollection (null);
				}
				
				return attributes;
			}
			
		}

		public ReadOnlyCollection<MetaColumn> Columns { get; private set; }

		public string DataContextPropertyName {
			get { return Provider.Name; }	
		}

		public Type DataContextType { get; private set; }

		public MetaColumn DisplayColumn {
			get {
				if (displayColumn == null)
					displayColumn = FindDisplayColumn ();
				return displayColumn;
			}
		}

		public string DisplayName {
			get {
				if (displayName == null)
					displayName = DetermineDisplayName ();

				return displayName;
			}
		}

		public Type EntityType {
			get { return Provider.EntityType; }
		}

		[MonoTODO]
		public string ForeignKeyColumnsNames { get; private set; }

		public bool HasPrimaryKey { get; private set; }

		[MonoTODO ("How it is determined?")]
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

		public ReadOnlyCollection<MetaColumn> PrimaryKeyColumns { get; private set; }

		public TableProvider Provider { get; private set; }

		public bool Scaffold { get; private set; }

		[MonoTODO]
		public MetaColumn SortColumn { get; private set; }

		[MonoTODO]
		public bool SortDescending { get; private set; }

		public object CreateContext ()
		{
			return Activator.CreateInstance (EntityType);
		}

		string DetermineDisplayName ()
		{
			DisplayNameAttribute attr = Attributes [typeof (DisplayNameAttribute)] as DisplayNameAttribute;

			if (attr == null)
				return Name;

			string name = attr.DisplayName;
			if (name == null)
				return String.Empty;

			return name;
		}
		
		void FillWithPrimaryKeys (RouteValueDictionary values, IList<object> primaryKeyValues)
		{
			if (primaryKeyValues == null)
				return;

			ReadOnlyCollection <MetaColumn> pkc = PrimaryKeyColumns;
			int pkcCount = pkc.Count;
			
			// Fill the above with primary keys using primaryKeyValues - .NET does not
			// check (again) whether there are enough elements in primaryKeyValues, it
			// just assumes there are at least as many of them as in the
			// PrimaryKeyColumns collection, so we'll emulate this bug here.
			if (primaryKeyValues.Count < pkc.Count)
				throw new ArgumentOutOfRangeException ("index");

			// This is so wrong (the generated URL might contain values assigned to
			// primary column keys which do not correspond with the column's type) but
			// this is how the upstream behaves, unfortunately.
			for (int i = 0; i < pkcCount; i++)
				values.Add (pkc [i].Name, primaryKeyValues [i]);
		}
		
		MetaColumn FindDisplayColumn ()
		{
			ReadOnlyCollection<MetaColumn> columns = Columns;

			// 1. The column that is specified by using the DisplayColumnAttribute attribute. 
			DisplayColumnAttribute attr = Attributes [typeof (DisplayColumnAttribute)] as DisplayColumnAttribute;
			if (attr != null) {
				string name = attr.DisplayColumn;
				foreach (MetaColumn mc in columns)
					if (String.Compare (name, mc.Name, StringComparison.Ordinal) == 0)
						return mc;
				
				throw new InvalidOperationException ("The display column '" + name + "' specified for the table '" + EntityType.Name + "' does not exist.");
			}

			// 2. The first string column that is not in the primary key.
			ReadOnlyCollection <MetaColumn> pkc = PrimaryKeyColumns;
			bool havePkc = pkc.Count > 0;
			foreach (MetaColumn mc in columns) {
				if (havePkc && pkc.Contains (mc))
					continue;
				if (mc.ColumnType == typeof (string))
					return mc;
			}

			// 3. The first string column that is in the primary key. 
			if (havePkc) {
				foreach (MetaColumn mc in pkc) {
					if (mc.ColumnType == typeof (string))
						return mc;
				}

				// 4. The first non-string column that is in the primary key.
				return pkc [0];
			}
			
			// No check, again, is made whether the columns collection contains enough
			// columns to perform successful lookup, we're emulating that here.
			if (columns.Count == 0)
				throw new ArgumentOutOfRangeException ("index");

			// Fallback - return the first column
			return columns [0];
		}

		public string GetActionPath (string action)
		{
			// You can see this is the call we should make by modifying one of the unit
			// tests (e.g. MetaTableTest.GetActinPath) and commenting out the line which
			// assigns a context to HttpContext.Current and looking at the exception
			// stack trace.
			return GetActionPath (action, (IList <object>)null);
		}

		public string GetActionPath (string action, IList<object> primaryKeyValues)
		{
			var values = new RouteValueDictionary ();
			values.Add ("Action", action);
			values.Add ("Table", Name);			

			FillWithPrimaryKeys (values, primaryKeyValues);
			
			// To see that this internal method is called, comment out setting of
			// HttpContext in the GetActionPath_Action_PrimaryKeyValues test and look at
			// the stack trace
			return GetActionPathFromRoutes (values);
		}

		public string GetActionPath (string action, object row)
		{
			// To see that this method is called, comment out setting of
			// HttpContext in the GetActionPath_Action_Row test and look at
			// the stack trace
			return GetActionPath (action, GetPrimaryKeyValues (row));
		}

		public string GetActionPath (string action, RouteValueDictionary routeValues)
		{
			// .NET doesn't check whether routeValues is null, we'll just "implement"
			// the behavior here...
			if (routeValues == null)
				throw new NullReferenceException ();

			// NO check is made whether those two are already in the dictionary...
			routeValues.Add ("Action", action);
			routeValues.Add ("Table", Name);
			
			// To see that this internal method is called, comment out setting of
			// HttpContext in the GetActionPath_Action_RouteValues test and look at
			// the stack trace
			return GetActionPathFromRoutes (routeValues);
		}

		public string GetActionPath (string action, IList<object> primaryKeyValues, string path)
		{
			var values = new RouteValueDictionary ();
			values.Add ("Action", String.IsNullOrEmpty (path) ? action : path);
			values.Add ("Table", Name);			

			FillWithPrimaryKeys (values, primaryKeyValues);
			
			// To see that this internal method is called, comment out setting of
			// HttpContext in the GetActionPath_Action_PrimaryKeyValues test and look at
			// the stack trace
			return GetActionPathFromRoutes (values);
		}

		public string GetActionPath (string action, object row, string path)
		{
			return GetActionPath (action, GetPrimaryKeyValues (row), path);
		}

		string GetActionPathFromRoutes (RouteValueDictionary values)
		{
			if (routes == null)
				routes = RouteTable.Routes;

			VirtualPathData vpd = routes.GetVirtualPath (DynamicDataRouteHandler.GetRequestContext (HttpContext.Current), values);
			return vpd == null ? String.Empty : vpd.VirtualPath;
		}
		
		public MetaColumn GetColumn (string columnName)
		{
			MetaColumn mc;
			if (TryGetColumn (columnName, out mc))
				return mc;
			throw new InvalidOperationException (String.Format ("Column '{0}' does not exist in the meta table '{1}'", columnName, Name));
		}

		public string GetDisplayString (object row)
		{
			if (row == null)
				return String.Empty;

			if (entityHasToString == null) {
				Type type = EntityType;
				MethodInfo pi = type == null ? null : type.GetMethod ("ToString", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				entityHasToString = pi != null;
			}

			if ((bool)entityHasToString)
				return row.ToString ();
			
			// Once again no check is made for row's type
			MetaColumn mc = DisplayColumn;
			object value = DataBinder.GetPropertyValue (row, mc.Name);
			if (value == null)
				return String.Empty;
			
			return value.ToString ();
		}

		public string GetPrimaryKeyString (IList<object> primaryKeyValues)
		{
			if (primaryKeyValues == null || primaryKeyValues.Count == 0)
				return String.Empty;
			
			var strings = new List <string> ();
			bool allNull = true;
			
			foreach (object o in primaryKeyValues) {
				if (o == null) {
					strings.Add (null);
					continue;
				}
				
				var str = o.ToString ();
				if (str == null) {
					strings.Add (null);
					continue;
				}

				allNull = false;
				strings.Add (str);
			}
			
			if (allNull) {
				strings = null;
				return String.Empty;
			}

			var ret = String.Join (",", strings.ToArray ());
			strings = null;
			
			return ret;
		}

		public string GetPrimaryKeyString (object row)
		{
			return GetPrimaryKeyString (GetPrimaryKeyValues (row));
		}

		public IList<object> GetPrimaryKeyValues (object row)
		{
			if (row == null)
				return null;

			ReadOnlyCollection <MetaColumn> pkc = PrimaryKeyColumns;
			int pkcCount = pkc.Count;
			var ret = new List <object> ();
			if (pkcCount == 0)
				return ret;
			
			// No check is made whether row is of correct type,
			// DataBinder.GetPropertyValue is called instead to fetch value of each
			// member of the row object corresponding to primary key columns.
			for (int i = 0; i < pkcCount; i++)
				ret.Add (DataBinder.GetPropertyValue (row, pkc [i].Name));
				
			return ret;
		}

		public IQueryable GetQuery ()
		{
			return GetQuery (CreateContext ());
		}

		public IQueryable GetQuery (object context)
		{
			return Provider.GetQuery (context == null ? CreateContext () : context);
		}

		public override string ToString ()
		{
			return Name;
		}

		public bool TryGetColumn (string columnName, out MetaColumn column)
		{
			if (String.IsNullOrEmpty (columnName))
				throw new ArgumentNullException ("columnName");
			
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
