//
// System.Data.DataViewManagerListItemTypeDscriptor
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.ComponentModel;

namespace System.Data
{
	class DataViewManagerListItemTypeDescriptor : ICustomTypeDescriptor
	{
		DataViewManager dvm;
		PropertyDescriptorCollection propsCollection;

		internal DataViewManagerListItemTypeDescriptor (DataViewManager dvm)
		{
			this.dvm = dvm;
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			return new AttributeCollection (null);
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetClassName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string ICustomTypeDescriptor.GetComponentName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (System.Attribute[] attributes)
		{
			throw new NotImplementedException ();
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			DataSet ds = dvm.DataSet;
			if (ds == null)
				return null;

			DataTableCollection tables = ds.Tables;
			int index = 0;
			PropertyDescriptor [] descriptors  = new PropertyDescriptor [tables.Count];
			foreach (DataTable table in tables)
				descriptors [index++] = new TablePD (table);

			return new PropertyDescriptorCollection (descriptors);
		}

		[MonoTODO]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (System.Attribute[] attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException ();
		}

		class TablePD : PropertyDescriptor
		{
			DataTable table;

			public TablePD (DataTable table) : base (table.TableName, null)
			{
				this.table = table;
			}

			public override object GetValue (object component)
			{
				DataViewManagerListItemTypeDescriptor desc = component as DataViewManagerListItemTypeDescriptor;
				if (desc == null)
					return null;

				DataView dv = new DataView (table);
				dv.dataViewManager = desc.dvm;
				return dv;
			}

			public override bool CanResetValue (object component)
			{
				return false;
			}

			public override bool Equals (object other)
			{
				return other is TablePD && ((TablePD) other).table == table;
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
}

