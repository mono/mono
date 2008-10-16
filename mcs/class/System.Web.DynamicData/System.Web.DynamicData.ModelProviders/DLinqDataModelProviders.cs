//
// DLinqDataModelProvider.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Security.Permissions;

using DMetaModel = System.Data.Linq.Mapping.MetaModel;
using DMetaTable = System.Data.Linq.Mapping.MetaTable;

namespace System.Web.DynamicData.ModelProviders
{
	class DLinqDataModelProvider : DataModelProvider
	{
		Func<object> factory;
		DMetaModel model;

		public DLinqDataModelProvider (Func<object> factory)
		{
			this.factory = factory;
			Type type = CreateContext ().GetType ();

			if (!typeof (DataContext).IsAssignableFrom (type))
				throw new ArgumentException (String.Format ("Type '{0}' is not supported as data context factory", type));

			this.factory = factory;

			model = new AttributeMappingSource ().GetModel (type);
			ContextType = model.ContextType;

			var l = new List<TableProvider> ();
			foreach (var m in model.GetTables ())
				l.Add (new DLinqTableProvider (this, m));
			tables = new ReadOnlyCollection<TableProvider> (l);
		}

		ReadOnlyCollection<TableProvider> tables;

		public override ReadOnlyCollection<TableProvider> Tables {
			get { return tables; }
		}

		public override object CreateContext ()
		{
			return factory ();
		}
	}

	class DLinqTableProvider : TableProvider
	{
		public DLinqTableProvider (DataModelProvider owner, DMetaTable meta)
			: base (owner)
		{
			EntityType = meta.RowType.Type;

			Name = meta.TableName;
			int idx = Name.LastIndexOf ('.');
			Name = idx < 0 ? Name : Name.Substring (idx + 1);

			var l = new List<ColumnProvider> ();
			foreach (var c in meta.RowType.DataMembers)
				l.Add (new DLinqColumnProvider (this, c));
			columns = new ReadOnlyCollection<ColumnProvider> (l);
		}

		MetaTable table;
		ReadOnlyCollection<ColumnProvider> columns;

		public override ReadOnlyCollection<ColumnProvider> Columns {
			get { return columns; }
		}

		public override IQueryable GetQuery (object context)
		{
			return ((DataContext) context).GetTable (EntityType);
		}

		public override string ToString ()
		{
			return base.ToString ();
		}
	}

	class DLinqColumnProvider : ColumnProvider
	{
		public DLinqColumnProvider (TableProvider owner, MetaDataMember meta)
			: base (owner)
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");
			if (meta == null)
				throw new ArgumentNullException ("meta");

			this.meta = meta;

			// FIXME: fill more
			Name = meta.Name;
			Nullable = meta.CanBeNull;
		}

		MetaDataMember meta;

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
