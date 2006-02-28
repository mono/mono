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

// COMPLETE

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

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			string[]	parts;
			DateTime	start;
			DateTime	end;

			if ((value == null) || !(value is String)) {
				return base.ConvertFrom (context, culture, value);
			}

			if (culture == null) {
				culture = CultureInfo.CurrentCulture;
			}

			parts = ((string)value).Split(culture.TextInfo.ListSeparator.ToCharArray());

			start = (DateTime)TypeDescriptor.GetConverter(typeof(DateTime)).ConvertFromString(context, culture, parts[0]);
			end = (DateTime)TypeDescriptor.GetConverter(typeof(DateTime)).ConvertFromString(context, culture, parts[1]);

			return new SelectionRange(start, end);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
			SelectionRange	s;

			if ((value == null) || !(value is SelectionRange) || (destinationType != typeof(string))) {
				return base.ConvertTo (context, culture, value, destinationType);
			}

			if (culture == null) {
				culture = CultureInfo.CurrentCulture;
			}

			s = (SelectionRange)value;


			return s.Start.ToShortDateString() + culture.TextInfo.ListSeparator + s.End.ToShortDateString();
		}

		public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues) {
			return new SelectionRange((DateTime)propertyValues["Start"], (DateTime)propertyValues["End"]);
		}

		public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) {
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) {
			return TypeDescriptor.GetProperties(typeof(SelectionRange), attributes);
		}

		public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
			return true;
		}
		#endregion	// Public Instance Methods
	}
}
