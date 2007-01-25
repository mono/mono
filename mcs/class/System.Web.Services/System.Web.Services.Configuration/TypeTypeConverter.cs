//
// TypeTypeConverter.cs
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Configuration;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;

#if NET_2_0

namespace System.Web.Services.Configuration
{
	internal class TypeTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type type)
		{
			return type == typeof (Type) || type == typeof (string);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type type)
		{
			return type == typeof (Type) || type == typeof (string);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (value is Type)
				return (Type) value;
			else if (value is string)
				return Type.GetType ((string) value);
			else
				throw new ArgumentException (String.Format ("Incompatible input value type: {0}", value.GetType ()));
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (destinationType == null)
				throw new ArgumentNullException ("destinationType");
			if (destinationType == typeof (Type))
				return (Type) value;
			if (destinationType == typeof (string))
				return ((Type) value).AssemblyQualifiedName;
			else
				throw new ArgumentException (String.Format ("Incompatible input destination type: {0}", destinationType));
		}
	}
}

#endif

