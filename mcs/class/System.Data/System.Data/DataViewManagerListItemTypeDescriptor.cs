//
// System.Data.DataViewManagerListItemTypeDscriptor
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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

