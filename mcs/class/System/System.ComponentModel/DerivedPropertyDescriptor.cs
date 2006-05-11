//
// System.ComponentModel.DerivedPropertyDescriptor
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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

		public override void SetValue(object component,	object value) {
			
			if (prop == null)
				prop = componentType.GetProperty (Name);

			prop.SetValue (component, value, null);
			// FIXME: EventArgs might be differen type.
			OnValueChanged (component, new PropertyChangedEventArgs (Name));
		}

		[MonoTODO]
		public override void ResetValue(object component) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool CanResetValue(object component) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool ShouldSerializeValue(object component) {

			throw new NotImplementedException ();
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

