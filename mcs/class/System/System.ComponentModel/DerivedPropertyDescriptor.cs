//
// System.ComponentModel.DerivedPropertyDescriptor
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Reflection;

namespace System.ComponentModel
{
	class DerivedPropertyDescriptor : PropertyDescriptor
	{
		bool readOnly;
		Type componentType;
		Type propertyType;
		PropertyInfo prop;

		protected DerivedPropertyDescriptor (string name, Attribute [] attrs)
			: base (name, attrs)
		{
		}

		public DerivedPropertyDescriptor (string name, Attribute [] attrs, int dummy)
			: this (name, attrs)
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

		public override object GetValue (object component)
		{
			if (prop == null)
				prop = componentType.GetProperty (Name);

			return prop.GetValue (component, null);
		}

		public override Type ComponentType
		{
			get {
				return componentType;
			}

		}

		public override bool IsReadOnly
		{
			get {
				return readOnly;
				
			}

		}

		public override Type PropertyType
		{
			get {
				return propertyType;
			}

		}
	}
}

