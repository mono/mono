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
// Copyright (c) 2005-2008 Novell, Inc.
//
// Authors:
//	Jonathan Chambers	jonathan.chambers@ansys.com
//	Ivan N. Zlatev		contact@i-nz.net
//

using System;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace System.Windows.Forms.PropertyGridInternal
{
	public class PropertiesTab : PropertyTab
	{
		public PropertiesTab () 
		{
		}

		public override PropertyDescriptorCollection GetProperties (object component, Attribute[] attributes)
		{
			return GetProperties (null, component, attributes);
		}

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object component, Attribute[] attributes)
		{
			if (component == null)
				return new PropertyDescriptorCollection (null);
			if (attributes == null)
				attributes = new Attribute[] { BrowsableAttribute.Yes };

			PropertyDescriptorCollection properties = null;
			TypeConverter converter = TypeDescriptor.GetConverter (component);
			if (converter != null && converter.GetPropertiesSupported ())
				properties = converter.GetProperties (context, component, attributes);
			if (properties == null)   // try 3: TypeDescriptor
				properties = TypeDescriptor.GetProperties (component, attributes);
			return properties;
		}

		public override PropertyDescriptor GetDefaultProperty (object obj)
		{
			if (obj == null)
				return null;

			return TypeDescriptor.GetDefaultProperty (obj);
		}

		public override string HelpKeyword {
			get { return "vs.properties"; }
		}

		public override string TabName {
			get { return "Properties"; }
		}
	}
}
