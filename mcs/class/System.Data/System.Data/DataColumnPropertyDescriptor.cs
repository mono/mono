//
// System.Data.DataColumnPropertyDescriptor.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (c) copyright 2002 Daniel Morgan
//

using System;
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

		public DataColumnPropertyDescriptor (string name, Attribute [] attrs)
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
			if (prop == null)
				prop = GetPropertyInfo ();		
							
			// FIXME: should I allow multiple parameters?											
			object[] parms = new object[1];
			parms[0] = base.Name;
			return prop.GetValue (component, parms);
		}

		public override void SetValue(object component,	object value) 
		{
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
		}

		[MonoTODO]
		public override void ResetValue(object component) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool CanResetValue(object component) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool ShouldSerializeValue(object component) 
		{
			throw new NotImplementedException ();
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
