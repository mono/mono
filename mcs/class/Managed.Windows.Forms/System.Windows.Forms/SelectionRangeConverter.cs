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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// NOT COMPLETE

using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Forms {
	public class SelectionRangeConverter : TypeConverter {
		#region Public Constructors
		public SelectionRangeConverter() {
		}
		#endregion	// Public Constructors

		#region Public Instance Methods
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string)) {
				return true;
			}
			return false;
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)) {
				return true;
			}
			return false;
		}

		[MonoTODO]
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			if (value is string) {

			}
			return base.ConvertFrom (context, culture, value);
		}

		[MonoTODO]
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
			return base.ConvertTo (context, culture, value, destinationType);
		}

		[MonoTODO]
		public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues) {
			return base.CreateInstance (context, propertyValues);
		}

		[MonoTODO]
		public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) {
			return base.GetCreateInstanceSupported (context);
		}

		[MonoTODO]
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) {
			return base.GetProperties(context, value, attributes);
		}

		[MonoTODO]
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
			return base.GetPropertiesSupported(context);
		}
		#endregion	// Public Instance Methods
	}
}
