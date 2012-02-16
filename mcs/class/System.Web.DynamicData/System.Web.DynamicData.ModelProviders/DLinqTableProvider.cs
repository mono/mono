//
// DLinqTableProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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
}
