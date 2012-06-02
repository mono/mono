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
		Type _propertyType;
		PropertyInfo getter, setter;
		bool accessors_inited;

		public ReflectionPropertyDescriptor (Type componentType, PropertyDescriptor oldPropertyDescriptor, Attribute [] attributes)
		: base (oldPropertyDescriptor, attributes)
		{
			_componentType = componentType;
			_propertyType = oldPropertyDescriptor.PropertyType;
		}

		public ReflectionPropertyDescriptor (Type componentType, string name, Type type, Attribute [] attributes)
		: base (name, attributes)
		{
			_componentType = componentType;
			_propertyType = type;
		}

		public ReflectionPropertyDescriptor (PropertyInfo info)
		: base (info.Name, null)
		{
			_member = info;
			_componentType = _member.DeclaringType;
			_propertyType = info.PropertyType;
		}

		PropertyInfo GetPropertyInfo ()
		{
			if (_member == null) {
				_member = _componentType.GetProperty (Name, BindingFlags.GetProperty | BindingFlags.NonPublic | 
								      BindingFlags.Public | BindingFlags.Instance,
								      null, this.PropertyType,
								      new Type[0], new ParameterModifier[0]);
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
				ReadOnlyAttribute attrib = ((ReadOnlyAttribute) Attributes[typeof (ReadOnlyAttribute)]);
				return !GetPropertyInfo ().CanWrite || attrib.IsReadOnly;
			}
		}

		public override Type PropertyType {
			get { return _propertyType; }
		}

		// The last added to the list attributes have higher precedence
		//
		protected override void FillAttributes (IList attributeList)
		{
			base.FillAttributes (attributeList);

			if (!GetPropertyInfo ().CanWrite)
				attributeList.Add (ReadOnlyAttribute.Yes);
			
			// PropertyDescriptor merges the attributes of both virtual and also "new" properties 
			// in the the component type hierarchy.
			// 
			int numberOfBaseTypes = 0;
			Type baseType = this.ComponentType;
			while (baseType != null && baseType != typeof (object)) {
				numberOfBaseTypes++;
				baseType = baseType.BaseType;
			}

			Attribute[][] hierarchyAttributes = new Attribute[numberOfBaseTypes][];
			baseType = this.ComponentType;
			while (baseType != null && baseType != typeof (object)) {
				PropertyInfo property = baseType.GetProperty (Name, BindingFlags.NonPublic |
									      BindingFlags.Public | BindingFlags.Instance | 
									      BindingFlags.DeclaredOnly, 
									      null, this.PropertyType,
									      new Type[0], new ParameterModifier[0]);
				if (property != null) {
					object[] attrObjects = property.GetCustomAttributes (false);
					Attribute[] attrsArray = new Attribute[attrObjects.Length];
					attrObjects.CopyTo (attrsArray, 0);
					// add in reverse order so that the base types have lower precedence
					hierarchyAttributes[--numberOfBaseTypes] = attrsArray;
				}
				baseType = baseType.BaseType;
			}

			foreach (Attribute[] attrArray in hierarchyAttributes) {
				if (attrArray != null) {
					foreach (Attribute attr in attrArray)
						attributeList.Add (attr);
				}
			}

			foreach (Attribute attribute in TypeDescriptor.GetAttributes (PropertyType))
				attributeList.Add (attribute);
		}

		public override object GetValue (object component)
		{
			component = MemberDescriptor.GetInvokee (_componentType, component);
			InitAccessors ();
			return getter.GetValue (component,  null);
		}

		DesignerTransaction CreateTransaction (object obj, string description)
		{
			IComponent com = obj as IComponent;
			if (com == null || com.Site == null)
				return null;

			IDesignerHost dh = (IDesignerHost) com.Site.GetService (typeof(IDesignerHost));
			if (dh == null)
				return null;

			DesignerTransaction tran = dh.CreateTransaction (description);
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

		/*
		This method exists because reflection is way too low level for what we need.
		A given virtual property that is partially overriden by a child won't show the
		non-overriden accessor in PropertyInfo. IOW:
		class Parent {
			public virtual string Prop { get; set; }
		}
		class Child : Parent {
			public override string Prop {
				get { return "child"; }
			}
		}
		PropertyInfo pi = typeof (Child).GetProperty ("Prop");
		pi.GetGetMethod (); //returns the MethodInfo for the overridden getter
		pi.GetSetMethod (); //returns null as no override exists
		*/
		void InitAccessors () {
			if (accessors_inited)
				return;
			PropertyInfo prop = GetPropertyInfo ();
			MethodInfo setterMethod, getterMethod;
			setterMethod = prop.GetSetMethod (true);
			getterMethod = prop.GetGetMethod (true);

			if (getterMethod != null)
				getter = prop;

			if (setterMethod != null)
				setter = prop;


			if (setterMethod != null && getterMethod != null) {//both exist
				accessors_inited = true;
				return;
			}
			if (setterMethod == null && getterMethod == null) {//neither exist, this is a broken property
				accessors_inited = true;
				return;
			}

			//In order to detect that this is a virtual property with override, we check the non null accessor
			MethodInfo mi = getterMethod != null ? getterMethod : setterMethod;

			if (mi == null || !mi.IsVirtual || (mi.Attributes & MethodAttributes.NewSlot) == MethodAttributes.NewSlot) {
				accessors_inited = true;
				return;
			}

			Type type = _componentType.BaseType;
			while (type != null && type != typeof (object)) {
				prop = type.GetProperty (Name, BindingFlags.GetProperty | BindingFlags.NonPublic | 
										      BindingFlags.Public | BindingFlags.Instance,
										      null, this.PropertyType,
										      new Type[0], new ParameterModifier[0]);
				if (prop == null) //nothing left to search
					break;
				if (setterMethod == null)
					setterMethod = mi = prop.GetSetMethod ();
				else
					getterMethod = mi = prop.GetGetMethod ();

				if (getterMethod != null && getter == null)
					getter = prop;
	
				if (setterMethod != null && setter == null)
					setter = prop;
				
				if (mi != null)
					break;
				type = type.BaseType;
			}
			accessors_inited = true;
		}

		public override void SetValue (object component, object value)
		{
			DesignerTransaction tran = CreateTransaction (component, "Set Property '" + Name + "'");
			
			object propertyHolder = MemberDescriptor.GetInvokee (_componentType, component);
			object old = GetValue (propertyHolder);

			try {
				InitAccessors ();
				setter.SetValue (propertyHolder, value, null);
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

			DesignerTransaction tran = CreateTransaction (component, "Reset Property '" + Name + "'");
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
				if (!_member.CanWrite)
					return false;

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
