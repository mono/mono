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

		internal DataViewManager DataViewManager {
			get { return dvm; }
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
				descriptors [index++] = new DataTablePropertyDescriptor (table);

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
	}
}

