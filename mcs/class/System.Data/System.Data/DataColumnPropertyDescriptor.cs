//
// System.Data.DataColumnPropertyDescriptor.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (c) copyright 2002 Daniel Morgan
//

using System;
using System.Data.Common;
using System.ComponentModel;
using System.Reflection;

namespace System.Data 
{
	public class DataColumnPropertyDescriptor : PropertyDescriptor 
	{
		private bool readOnly = true;
		private Type componentType = null;
		private Type propertyType = null;
		private PropertyInfo prop = null;
		private int columnIndex = 0;

		public DataColumnPropertyDescriptor (string name, int columnIndex, Attribute [] attrs)
			: base (name, attrs) 
		{
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

		private PropertyInfo GetPropertyInfo () 
		{
			string defaultMemberName = "";
			object[] attribs = componentType.GetCustomAttributes (true);
						
			for (int at = 0; at < attribs.Length; at++) {
				if (attribs[at] is DefaultMemberAttribute) {
					defaultMemberName = ((DefaultMemberAttribute) attribs[at]).MemberName;
					break;
				}
			}

			// FIXME: what do I do if a DefaultMemeberAttribute is not found?
			//        should I try looking for DefaultPropertyAttribute?
			if (defaultMemberName.Equals(""))
				throw new Exception("Default property not found.");

			Type[] parmTypes = new Type[1];
			parmTypes[0] = propertyType;
			PropertyInfo propertyInfo = componentType.GetProperty (defaultMemberName, parmTypes);
			return propertyInfo;
		}

		public override object GetValue (object component) 
		{
			// FIXME: what is the correct way to Get a Value?
			if(componentType == typeof(DataRowView) && component is DataRowView) {
				DataRowView drv = (DataRowView) component;
				return drv[base.Name];
			}
			else if(component == typeof(DbDataRecord) && component is DbDataRecord) {
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

		public override Type PropertyType {
			get {
				return propertyType;
			}
		}
	}
}
