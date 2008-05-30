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
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Dennis Bartok (pbartok@novell.com)
// 	Ivan N. Zlatev (contact i-nz.net)
//
//

// NOT COMPLETE
using System.ComponentModel;
using System.Windows.Forms;

namespace System.Windows.Forms.Design 
{
	internal class ControlBindingsConverter : TypeConverter 
	{
		public ControlBindingsConverter() 
		{
		}

		// TODO: Should create some sort of a special PropertyDescriptor and handle the data binding
		//
		[MonoTODO]
		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, 
									    object value, Attribute[] attributes) 
		{
			PropertyDescriptorCollection properties = new PropertyDescriptorCollection (new PropertyDescriptor[0]);
			ControlBindingsCollection collection = value as ControlBindingsCollection;
#if NET_2_0
			object bindableComponent = collection.BindableComponent;
#else
			object bindableComponent = collection.Control;
#endif
			if (collection != null && bindableComponent != null) {
				foreach (PropertyDescriptor property in 
					 TypeDescriptor.GetProperties (bindableComponent, attributes)) {
					if (((BindableAttribute) property.Attributes[typeof (BindableAttribute)]).Bindable)
						properties.Add (new DataBindingPropertyDescriptor (property, attributes, true));
				}
			}
			return properties;
		}

		public override bool GetPropertiesSupported (ITypeDescriptorContext context) 
		{
			return true;
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;
			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, 
						  object value, Type destinationType)
		{
			if (destinationType == typeof (string))
			    return String.Empty;
			return base.ConvertTo (context, culture, value, destinationType);
		}


		[MonoTODO]
		private class DataBindingPropertyDescriptor : PropertyDescriptor
		{
			bool _readOnly;

			[MonoTODO]
			public DataBindingPropertyDescriptor (PropertyDescriptor property, Attribute [] attrs, bool readOnly)
				: base (property.Name, attrs)
			{
				_readOnly = readOnly;
			}

			[MonoTODO]
			public override object GetValue (object component)
			{
				// throw new NotImplementedException ();
				return null;
			}

			[MonoTODO]
			public override void SetValue (object component, object value)
			{
				// throw new NotImplementedException ();
			}

			[MonoTODO]
			public override void ResetValue (object component) 
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override bool CanResetValue (object component) 
			{
				// throw new NotImplementedException ();
				return false;
			}

			public override bool ShouldSerializeValue (object component)
			{
				return false;
			}

			[MonoTODO]
			public override Type PropertyType {
				get {
					// throw new NotImplementedException ();
					return typeof (DataBindingPropertyDescriptor);
				}
			}

			[MonoTODO]
			public override TypeConverter Converter {
				get {
					// throw new NotImplementedException ();
					return null;
				}
			}

			public override Type ComponentType {
				get { return typeof (ControlBindingsCollection); }
			}

			public override bool IsReadOnly {
				get { return _readOnly; }
			}
		}
	}
}
