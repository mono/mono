//
// System.Data.DataTablePropertyDescriptor
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;

namespace System.Data
{
	internal class DataTablePropertyDescriptor : PropertyDescriptor
	{
		DataTable table;

		internal DataTablePropertyDescriptor (DataTable table) : base (table.TableName, null)
		{
			this.table = table;
		}

		public DataTable Table {
			get { return table; }
		}

		public override object GetValue (object component)
		{
			DataViewManagerListItemTypeDescriptor desc = component as DataViewManagerListItemTypeDescriptor;
			if (desc == null)
				return null;

			return new DataView (table, desc.DataViewManager);
		}

		public override bool CanResetValue (object component)
		{
			return false;
		}

		public override bool Equals (object other)
		{
			return (other is DataTablePropertyDescriptor && 
				((DataTablePropertyDescriptor) other).table == table);
		}

		public override int GetHashCode ()
		{
			return table.GetHashCode ();
		}

		public override bool ShouldSerializeValue (object component)
		{
			return false;
		}

		public override void ResetValue (object component)
		{
		}

		public override void SetValue (object component, object value)
		{
		}

		public override bool IsReadOnly
		{
			get { return false; }
		}

		public override Type ComponentType
		{
			get { return typeof (DataRowView); }
		}

		public override Type PropertyType
		{
			get { return typeof (IBindingList); }
		}
	}
}

