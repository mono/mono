//
// MetaChildrenColumn.cs
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
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.DynamicData.ModelProviders;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class MetaChildrenColumn : MetaColumn
	{
		internal MetaChildrenColumn (MetaTable table, ColumnProvider provider)
			: base (table, provider)
		{
			if (table == null)
				throw new ArgumentNullException ("table");
		}

		[MonoTODO]
		public MetaTable ChildTable { get; private set; }
		[MonoTODO]
		public MetaColumn ColumnInOtherTable { get; private set; }

		[MonoTODO]
		public string GetChildrenListPath (object row)
		{
			return ChildTable.GetActionPath (PageAction.List, row);
		}

		[MonoTODO]
		public string GetChildrenPath (string action, object row)
		{
			return ChildTable.GetActionPath (action, row);
		}

		[MonoTODO]
		public string GetChildrenPath (string action, object row, string path)
		{
			return ChildTable.GetActionPath (action, row, path);
		}

		internal override void Init ()
		{
			AssociationProvider association = Provider.Association;
			ColumnProvider otherColumn = association.ToColumn;
			string otherColumnName = otherColumn == null ? null : otherColumn.Name;
			MetaTable childTable = Model.GetTable (association.ToTable.Name, Table.DataContextType);
			ChildTable = childTable;
			if (childTable != null && !String.IsNullOrEmpty (otherColumnName))
				ColumnInOtherTable = childTable.GetColumn (otherColumnName);
		}
	}
}
