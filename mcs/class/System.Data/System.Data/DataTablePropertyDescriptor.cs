//
// System.Data.DataTablePropertyDescriptor
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.ComponentModel;

namespace System.Data
{
	internal class DataTablePropertyDescriptor : PropertyDescriptor
	{
		DataTable table;

		public DataTablePropertyDescriptor (DataTable table) : base (table.TableName, null)
		{
			this.table = table;
		}

		public override object GetValue (object component)
		{
			DataViewManagerListItemTypeDescriptor desc = component as DataViewManagerListItemTypeDescriptor;
			if (desc == null)
				return null;

			DataView dv = new DataView (table);
			dv.dataViewManager = desc.DataViewManager;
			return dv;
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

