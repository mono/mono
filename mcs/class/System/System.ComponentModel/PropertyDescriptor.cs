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

		[MonoTODO]
		public virtual void RemoveValueChanged(object component, System.EventHandler handler)
		{
			throw new NotImplementedException();
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

		[MonoTODO ("Not correctly implemented")]
		public override bool Equals(object obj)
		{
			if (!(obj is PropertyDescriptor))
				return false;
			if (obj == this)
				return true;
			return (((PropertyDescriptor) obj).AttributeArray == this.AttributeArray) &&
				(((PropertyDescriptor) obj).Attributes == this.Attributes) &&
				(((PropertyDescriptor) obj).DisplayName == this.DisplayName) &&
				(((PropertyDescriptor) obj).Name == this.Name);
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

		[MonoTODO ("Incorrect implementation")]
		public override int GetHashCode() 
		{
			return Name.GetHashCode ();
		}

		[MonoTODO]
		public virtual PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual object GetEditor(Type editorBaseType)
		{
			throw new NotImplementedException();
		}

		protected Type GetTypeFromName(string typeName)
		{
			return Type.GetType (typeName);
		}
	}
}

