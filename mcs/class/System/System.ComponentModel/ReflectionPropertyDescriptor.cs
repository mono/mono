//
// System.ComponentModel.PropertyDescriptor.cs
//
// Author:
//  Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) Novell, Inc.  
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;

namespace System.ComponentModel
{
	internal class ReflectionPropertyDescriptor : PropertyDescriptor
	{
		PropertyInfo _member;
		Type _type;
		Type _componentType;
		
		public ReflectionPropertyDescriptor (Type componentType, PropertyDescriptor oldPropertyDescriptor, Attribute [] attributes)
		: base (oldPropertyDescriptor, attributes)
		{
			_type = oldPropertyDescriptor.PropertyType;
			_componentType = componentType;
		}
							 
		public ReflectionPropertyDescriptor (Type componentType, string name, Type type, Attribute [] attributes)
		: base (name, attributes)
		{
			_type = type;
			_componentType = componentType;
		}
							 
		public ReflectionPropertyDescriptor (PropertyInfo info)
		: base (info.Name, (Attribute[])info.GetCustomAttributes (true))
		{
			_member = info;
			_componentType = _member.DeclaringType;
		}
		
		PropertyInfo GetEventInfo ()
		{
			if (_member == null) {
				_member = _componentType.GetProperty (Name);
				if (_member == null)
					throw new ArgumentException ("Accessor methods for the " + Name + " property are missing");
			}
			return _member;
		}		

		public override Type ComponentType 
		{ 
			get { return _componentType; }
		}

		public override bool IsReadOnly 
		{
			get
			{
				return !_member.CanWrite;
			}
		}

		public override Type PropertyType 
		{
			get
			{
				return _member.PropertyType;
			}
		}
		
		public override object GetValue (object component)
		{
			return _member.GetValue (component, null);
		}
		
		DesignerTransaction CreateTransaction (object obj)
		{
			Component com = obj as Component;
			if (com == null) return null;
			
			IDesignerHost dh = (IDesignerHost) com.Site.GetService (typeof(IDesignerHost));
			DesignerTransaction tran = dh.CreateTransaction ();
			IComponentChangeService ccs = (IComponentChangeService) com.Site.GetService (typeof(IComponentChangeService));
			ccs.OnComponentChanging (com, this);
			return tran;
		}
		
		void EndTransaction (object obj, DesignerTransaction tran, object oldValue, object newValue, bool commit)
		{
			if (tran == null) return;
			
			if (commit) {
				Component com = obj as Component;
				IComponentChangeService ccs = (IComponentChangeService) com.Site.GetService (typeof(IComponentChangeService));
				ccs.OnComponentChanged (com, this, oldValue, newValue);
				tran.Commit ();
			}
			else
				tran.Cancel ();
		}
		
		public override void SetValue (object component, object value)
		{
			DesignerTransaction tran = CreateTransaction (component);
			object old = GetValue (component);
			
			try
			{
				_member.SetValue (component, value, null);
				EndTransaction (component, tran, old, value, true);
			}
			catch
			{
				EndTransaction (component, tran, old, value, false);
				throw;
			}
		}

		public override void ResetValue (object component)
		{
			DefaultValueAttribute attrib = ((DefaultValueAttribute) Attributes[typeof (DefaultValueAttribute)]);
			if (attrib != null) {
				SetValue (component, attrib.Value); 
			}
			
			DesignerTransaction tran = CreateTransaction (component);
			object old = GetValue (component);
			
			try
			{
				MethodInfo mi = component.GetType().GetMethod ("Reset" + Name, Type.EmptyTypes);
				if (mi != null) mi.Invoke (component, null);
				EndTransaction (component, tran, old, GetValue (component), true);
			}
			catch
			{
				EndTransaction (component, tran, old, GetValue (component), false);
				throw;
			}
		}

		public override bool CanResetValue (object component)
		{
			DefaultValueAttribute attrib = ((DefaultValueAttribute) Attributes[typeof (DefaultValueAttribute)]);
			if (attrib != null) {
				object current = GetValue (component);
				if ((attrib.Value == null || current == null) && attrib.Value != current) return true;
				return !attrib.Value.Equals (current);
			}
			else {
				MethodInfo mi = component.GetType().GetMethod ("ShouldPersist" + Name, Type.EmptyTypes);
				if (mi != null) return (bool) mi.Invoke (component, null);
				mi = component.GetType().GetMethod ("Reset" + Name, Type.EmptyTypes);
				return (mi != null);
			}
		}

		public override bool ShouldSerializeValue (object component)
		{
			DefaultValueAttribute attrib = ((DefaultValueAttribute) Attributes[typeof (DefaultValueAttribute)]);
			if (attrib != null) {
				object current = GetValue (component);
				if ((attrib.Value == null || current == null) && attrib.Value != current) return true;
				return !attrib.Value.Equals (current);
			}
			else {
				MethodInfo mi = component.GetType().GetMethod ("ShouldSerialize" + Name, Type.EmptyTypes);
				if (mi != null) return (bool) mi.Invoke (component, null);
				return true;
			}
		}
	}
}

