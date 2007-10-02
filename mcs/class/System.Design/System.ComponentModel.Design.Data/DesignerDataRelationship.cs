//
// System.ComponentModel.Design.Data.DesignerDataRelationship
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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
using System.Security.Permissions;

namespace System.ComponentModel.Design.Data
{
	public sealed class DesignerDataRelationship
	{
		string name;
		ICollection parent_columns, child_columns;
		DesignerDataTable child_table;

		public DesignerDataRelationship (string name, ICollection parentColumns, DesignerDataTable childTable, ICollection childColumns)
		{
			this.name = name;
			this.parent_columns = parentColumns;
			this.child_table = childTable;
			this.child_columns = childColumns;
		}

		public string Name {
			get { return name; }
		}

		public ICollection ParentColumns {
			get { return parent_columns; }
		}

		public DesignerDataTable ChildTable {
			get { return child_table; }
		}

		public ICollection ChildColumns {
			get { return child_columns; }
		}
	}
}

#endif
