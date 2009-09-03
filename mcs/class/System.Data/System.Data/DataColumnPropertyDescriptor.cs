//
// System.Data.DataColumnPropertyDescriptor.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (c) copyright 2002 Daniel Morgan
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
using System.Data.Common;
using System.ComponentModel;
using System.Reflection;

namespace System.Data 
{
	internal class DataColumnPropertyDescriptor : PropertyDescriptor 
	{
		private bool readOnly = true;
		private Type componentType = null;
		private Type propertyType = null;
		private bool browsable = true;

		//private PropertyInfo prop = null;
		private int columnIndex = 0;

		public DataColumnPropertyDescriptor (string name, int columnIndex, Attribute [] attrs)
			: base (name, attrs) 
		{
			this.columnIndex = columnIndex;
		}

		public DataColumnPropertyDescriptor (DataColumn dc)
			: base (dc.ColumnName, null) 
		{
			this.columnIndex = dc.Ordinal;
			this.componentType = typeof(DataRowView);
			this.propertyType = dc.DataType;
			this.readOnly = dc.ReadOnly;
		}

		public void SetReadOnly (bool value) 
		{
			readOnly = value;
		}
		
		public void SetComponentType (Type type) 
		{
			componentType = type;
		}

		public void SetPropertyType (Type type) 
		{
			propertyType = type;
		}

		public void SetBrowsable (bool browsable)
		{
			this.browsable = browsable;
		}

		public override object GetValue (object component) 
		{
			// FIXME: what is the correct way to Get a Value?
			if(componentType == typeof(DataRowView) && component is DataRowView) {
				DataRowView drv = (DataRowView) component;
				return drv[base.Name];
			}
			else if(componentType == typeof(DbDataRecord) && component is DbDataRecord) {
				DbDataRecord dr = (DbDataRecord) component;
				return dr[columnIndex];
			}
			throw new InvalidOperationException();

			/*
			if (prop == null)
				prop = GetPropertyInfo ();		
							
			// FIXME: should I allow multiple parameters?											
			object[] parms = new object[1];
			parms[0] = base.Name;
			return prop.GetValue (component, parms);
			*/
		}

		public override void SetValue(object component,	object value) 
		{
			DataRowView drv = (DataRowView) component;
			drv[base.Name] = value;
			/*
			if (prop == null)
				prop = GetPropertyInfo ();		

			if (readOnly == true) {
				// FIXME: what really happens if read only?
				throw new Exception("Property is ReadOnly");
			}
			
			// FIXME: should I allow multiple parameters?
			object[] parms = new Object[1];
			parms[0] = base.Name;
			prop.SetValue (component, value, parms);
			*/
		}

		[MonoTODO]
		public override void ResetValue(object component) 
		{
			// FIXME:
		}

		[MonoTODO]
		public override bool CanResetValue(object component) 
		{
			return false; // FIXEME
		}

		[MonoTODO]
		public override bool ShouldSerializeValue(object component) 
		{
			return false;
		}

		public override Type ComponentType {
			get {
				return componentType;
			}
		}

		public override bool IsReadOnly {
			get {
				return readOnly;	
			}
		}

		public override bool IsBrowsable {
			get { return browsable && base.IsBrowsable; }
		}

		public override Type PropertyType {
			get {
				return propertyType;
			}
		}
	}
}
