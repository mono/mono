//
// System.UriTypeConverter class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.ComponentModel;
#if !NET_2_1
using System.ComponentModel.Design.Serialization;
#endif
using System.Globalization;
using System.Reflection;

namespace System {

	public
#if MOONLIGHT
	sealed
#endif
	class UriTypeConverter : TypeConverter {

		public UriTypeConverter ()
		{
		}


		private bool CanConvert (Type type)
		{
			if (type == typeof (string))
				return true;
			if (type == typeof (Uri))
				return true;
#if NET_2_1
			return false;
#else
			return (type == typeof (InstanceDescriptor));
#endif
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == null)
				throw new ArgumentNullException ("sourceType");

			return CanConvert (sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == null)
				return false;

			return CanConvert (destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if (!CanConvertFrom (context, value.GetType ()))
				throw new NotSupportedException (Locale.GetText ("Cannot convert from value."));

			if (value is Uri)
				return value;

			string s = (value as string);
			if (s != null)
				return new Uri (s, UriKind.RelativeOrAbsolute);
#if !NET_2_1
			InstanceDescriptor id = (value as InstanceDescriptor);
			if (id != null) {
				return id.Invoke ();
			}
#endif
			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (!CanConvertTo (context, destinationType))
				throw new NotSupportedException (Locale.GetText ("Cannot convert to destination type."));

			Uri uri = (value as Uri);
			if (uri != null) {
				if (destinationType == typeof (string))
					return uri.ToString ();
				if (destinationType == typeof (Uri))
					return uri;
#if !NET_2_1
				if (destinationType == typeof (InstanceDescriptor)) {
					ConstructorInfo ci = typeof (Uri).GetConstructor (new Type [2] { typeof (string), typeof (UriKind) });
					return new InstanceDescriptor (ci , new object [] { uri.ToString (), uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative });
				}
#endif
			}

			return base.ConvertTo (context, culture, value, destinationType);
		}

#if !NET_2_1
		public override bool IsValid (ITypeDescriptorContext context, object value)
		{
			if (value == null)
				return false;

			// LAMESPEC: docs says that a string is valid only if we can create an URI
			// from it. However all strings seems to be accepted (see unit tests)
			return ((value is string) || (value is Uri));
		}
#endif
	}
}

#endif
