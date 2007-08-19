//
// System.ComponentModel.PropertyDescriptor.cs
//
// Author:
//  Lluis Sanchez Gual (lluis@ximian.com)
//  Ivan N. Zlatev (contact i-nZ.net)
// (C) Novell, Inc.
//

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
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;

namespace System.ComponentModel
{
	internal class ReflectionPropertyDescriptor : PropertyDescriptor
	{
		PropertyInfo _member;
		Type _componentType;

		public ReflectionPropertyDescriptor (Type componentType, PropertyDescriptor oldPropertyDescriptor, Attribute [] attributes)
		: base (oldPropertyDescriptor, attributes)
		{
			_componentType = componentType;
		}

		public ReflectionPropertyDescriptor (Type componentType, string name, Type type, Attribute [] attributes)
		: base (name, attributes)
		{
			_componentType = componentType;
		}

		public ReflectionPropertyDescriptor (PropertyInfo info)
		: base (info.Name, (Attribute[])info.GetCustomAttributes (true))
		{
			_member = info;
			_componentType = _member.DeclaringType;
		}

		PropertyInfo GetPropertyInfo ()
		{
			if (_member == null) {
				_member = _componentType.GetProperty (Name, BindingFlags.GetProperty |  BindingFlags.NonPublic |
									BindingFlags.Public | BindingFlags.Instance);
				if (_member == null)
					throw new ArgumentException ("Accessor methods for the " + Name + " property are missing");
			}
			return _member;
		}

		public override Type ComponentType {
			get { return _componentType; }
		}

		public override bool IsReadOnly {
			get {
				bool attr_ro = false;

				ReadOnlyAttribute attrib = ((ReadOnlyAttribute) Attributes[typeof (ReadOnlyAttribute)]);
				if (attrib != null)
					attr_ro = attrib.IsReadOnly;

				return !GetPropertyInfo ().CanWrite || attrib.IsReadOnly;
			}
		}

		public override Type PropertyType {
			get {
				return GetPropertyInfo ().PropertyType;
			}
		}

		public override object GetValue (object component)
		{
			component = MemberDescriptor.GetInvokee (_componentType, component);
			return GetPropertyInfo ().GetValue (component, null);
		}

		DesignerTransaction CreateTransaction (object obj)
		{
			IComponent com = obj as IComponent;
			if (com == null || com.Site == null)
				return null;

			IDesignerHost dh = (IDesignerHost) com.Site.GetService (typeof(IDesignerHost));
			if (dh == null)
				return null;

			DesignerTransaction tran = dh.CreateTransaction ();
			IComponentChangeService ccs = (IComponentChangeService) com.Site.GetService (typeof(IComponentChangeService));
			if (ccs != null)
				ccs.OnComponentChanging (com, this);
			return tran;
		}

		void EndTransaction (object obj, DesignerTransaction tran, object oldValue, object newValue, bool commit)
		{
			if (tran == null) {
				// FIXME: EventArgs might be differen type.
				OnValueChanged (obj, new PropertyChangedEventArgs (Name));
				return;
			}

			if (commit) {
				IComponent com = obj as IComponent;
				IComponentChangeService ccs = (IComponentChangeService) com.Site.GetService (typeof(IComponentChangeService));
				if (ccs != null)
					ccs.OnComponentChanged (com, this, oldValue, newValue);
				tran.Commit ();
				// FIXME: EventArgs might be differen type.
				OnValueChanged (obj, new PropertyChangedEventArgs (Name));
			} else
				tran.Cancel ();
		}

		public override void SetValue (object component, object value)
		{
			DesignerTransaction tran = CreateTransaction (component);
			
			object propertyHolder = MemberDescriptor.GetInvokee (_componentType, component);
			object old = GetValue (propertyHolder);

			try {
				GetPropertyInfo ().SetValue (propertyHolder, value, null);
				EndTransaction (component, tran, old, value, true);
			} catch {
				EndTransaction (component, tran, old, value, false);
				throw;
			}
		}

		MethodInfo FindPropertyMethod (object o, string method_name)
		{
			MethodInfo mi = null;
			string name = method_name + Name;

			foreach (MethodInfo m in o.GetType().GetMethods (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
				// XXX should we really not check the return type of the method?
				if (m.Name == name && m.GetParameters().Length == 0) {
					mi = m;
					break;
				}
			}

			return mi;
		}

		public override void ResetValue (object component)
		{
			object propertyHolder = MemberDescriptor.GetInvokee (_componentType, component);
			
			DefaultValueAttribute attrib = ((DefaultValueAttribute) Attributes[typeof (DefaultValueAttribute)]);
			if (attrib != null)
				SetValue (propertyHolder, attrib.Value);

			DesignerTransaction tran = CreateTransaction (component);
			object old = GetValue (propertyHolder);

			try {
				MethodInfo mi = FindPropertyMethod (propertyHolder, "Reset");
				if (mi != null)
					mi.Invoke (propertyHolder, null);
				EndTransaction (component, tran, old, GetValue (propertyHolder), true);
			} catch {
				EndTransaction (component, tran, old, GetValue (propertyHolder), false);
				throw;
			}
		}

		public override bool CanResetValue (object component)
		{
			component = MemberDescriptor.GetInvokee (_componentType, component);
			
			DefaultValueAttribute attrib = ((DefaultValueAttribute) Attributes[typeof (DefaultValueAttribute)]);
			if (attrib != null) {
				object current = GetValue (component);
				if (attrib.Value == null || current == null){
					if (attrib.Value != current)
						return true;
					if (attrib.Value == null && current == null)
						return false;
				}

				return !attrib.Value.Equals (current);
			} else {
#if NET_2_0
				if (!_member.CanWrite)
					return false;
#endif
				MethodInfo mi = FindPropertyMethod (component, "ShouldPersist");
				if (mi != null)
					return (bool) mi.Invoke (component, null);

				mi = FindPropertyMethod (component, "ShouldSerialize");
				if (mi != null && !((bool) mi.Invoke (component, null)))
					return false;

				mi = FindPropertyMethod (component, "Reset");
				return mi != null;
			}
		}

		public override bool ShouldSerializeValue (object component)
		{
			component = MemberDescriptor.GetInvokee (_componentType, component);

			if (IsReadOnly) {
				MethodInfo mi = FindPropertyMethod (component, "ShouldSerialize");
				if (mi != null)
					return (bool) mi.Invoke (component, null);
				return Attributes.Contains (DesignerSerializationVisibilityAttribute.Content);
			}

			DefaultValueAttribute attrib = ((DefaultValueAttribute) Attributes[typeof (DefaultValueAttribute)]);
			if (attrib != null) {
				object current = GetValue (component);
				if (attrib.Value == null || current == null)
					return attrib.Value != current;
				return !attrib.Value.Equals (current);
			}
			else {
				MethodInfo mi = FindPropertyMethod (component, "ShouldSerialize");
				if (mi != null)
					return (bool) mi.Invoke (component, null);
				// MSDN: If this method cannot find a DefaultValueAttribute or a ShouldSerializeMyProperty method, 
				// it cannot create optimizations and it returns true. 
				return true;
			}
		}
	}
}
