//
// System.ComponentModel.PropertyDescriptor.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.ComponentModel
{
	[ComVisible (true)]
	public abstract class PropertyDescriptor : MemberDescriptor
	{
		protected PropertyDescriptor (MemberDescriptor reference)
		: base (reference)
		{
		}

		protected PropertyDescriptor (MemberDescriptor reference, Attribute [] attrs)
		: base (reference, attrs)
		{
		}

		protected PropertyDescriptor (string name, Attribute [] attrs)
		: base (name, attrs)
		{
		}

		public abstract Type ComponentType { get; }

		public virtual TypeConverter Converter {
			get { return TypeDescriptor.GetConverter (PropertyType); }
		}

		public virtual bool IsLocalizable {
			get {
				foreach (Attribute attr in AttributeArray){
					if (attr is LocalizableAttribute)
						return ((LocalizableAttribute) attr).IsLocalizable;
				}

				return false;
			}
		}

		public abstract bool IsReadOnly { get; }

		public abstract Type PropertyType { get; }

		public DesignerSerializationVisibility SerializationVisibility {
			get {
				foreach (Attribute attr in AttributeArray) {
					if (attr is DesignerSerializationVisibilityAttribute){
						DesignerSerializationVisibilityAttribute a;

						a = (DesignerSerializationVisibilityAttribute) attr;

						return a.Visibility;
					}
				}

				//
				// Is this a good default if we cant find the property?
				//
				return DesignerSerializationVisibility.Hidden;
			}
		}

		Hashtable notifiers;

		public virtual void AddValueChanged (object component, EventHandler handler)
		{
			EventHandler component_notifiers;

			if (component == null)
				throw new ArgumentNullException ("component");

			if (handler == null)
				throw new ArgumentNullException ("handler");

			if (notifiers == null)
				notifiers = new Hashtable ();

			component_notifiers = (EventHandler) notifiers [component];

			if (component_notifiers != null)
				component_notifiers += handler;
			else
				notifiers [component] = handler;
		}

		public virtual void RemoveValueChanged (object component, System.EventHandler handler)
		{
			EventHandler component_notifiers;

			if (component == null)
				throw new ArgumentNullException ("component");

			if (handler == null)
				throw new ArgumentNullException ("handler");

			if (notifiers == null) return;

			component_notifiers = (EventHandler) notifiers [component];
			component_notifiers -= handler;
			
			if (component_notifiers == null)
				notifiers.Remove (component);
			else
				notifiers [component] = handler;
		}

		protected virtual void OnValueChanged (object component, EventArgs e)
		{
			if (notifiers == null)
				return;

			EventHandler component_notifiers = (EventHandler) notifiers [component];

			if (component_notifiers == null)
				return;

			component_notifiers (component, e);
		}

		public abstract object GetValue (object component);

		public abstract void SetValue (object component, object value);

		public abstract void ResetValue (object component);

		public abstract bool CanResetValue (object component);

		public abstract bool ShouldSerializeValue (object component);

		protected object CreateInstance(System.Type type)
		{
			return Assembly.GetExecutingAssembly ().CreateInstance (type.Name);
		}

		public override bool Equals(object obj)
		{
			if (!base.Equals (obj)) return false;
			PropertyDescriptor other = obj as PropertyDescriptor;
			if (other == null) return false;
			return other.PropertyType == PropertyType;
		}

		public PropertyDescriptorCollection GetChildProperties()
		{
			return GetChildProperties (null, null);
		}

		public PropertyDescriptorCollection GetChildProperties(object instance)
		{
			return GetChildProperties (instance, null);
		}

		public PropertyDescriptorCollection GetChildProperties(Attribute[] filter)
		{
			return GetChildProperties (null, filter);
		}

		public override int GetHashCode() 
		{
			return base.GetHashCode ();
		}

		public virtual PropertyDescriptorCollection GetChildProperties (object instance, Attribute[] filter)
		{
			return TypeDescriptor.GetProperties (instance, filter);
		}

		public virtual object GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor (PropertyType, editorBaseType);
		}

		protected Type GetTypeFromName(string typeName)
		{
			return Type.GetType (typeName);
		}
	}
}

