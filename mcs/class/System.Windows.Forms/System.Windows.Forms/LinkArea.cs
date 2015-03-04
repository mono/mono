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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//
//	Dennis Hayes, dennish@raytek.com
//	Andreas Nahr, ClassDevelopment@A-SoftTech.com
//	Jordi Mas i Hernandez, jordi@ximian.com
//


// COMPLETE

using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Forms
{
	[Serializable]
	[TypeConverter(typeof(LinkArea.LinkAreaConverter))]
	public struct LinkArea
	{
		#region LinkAreaConverter Class
		public class LinkAreaConverter : TypeConverter {
			public LinkAreaConverter() {
			}

			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
				if (sourceType == typeof(string)) {
					return true;
				}
				return base.CanConvertFrom(context, sourceType);
			}

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
				if (destinationType == typeof(string)) {
					return true;
				}
				return base.CanConvertTo(context, destinationType);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
				string[]	parts;
				int		start;
				int		length;

				if ((value == null) || !(value is String)) {
					return base.ConvertFrom (context, culture, value);
				}

				if (culture == null) {
					culture = CultureInfo.CurrentCulture;
				}

				parts = ((string)value).Split(culture.TextInfo.ListSeparator.ToCharArray());
				start = int.Parse(parts[0].Trim());
				length = int.Parse(parts[1].Trim());
				return new LinkArea(start, length);
			}

			public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
				LinkArea	l;

				if ((value == null) || !(value is LinkArea) || (destinationType != typeof(string))) {
					return base.ConvertTo (context, culture, value, destinationType);
				}

				if (culture == null) {
					culture = CultureInfo.CurrentCulture;
				}

				l = (LinkArea)value;


				return l.Start.ToString() + culture.TextInfo.ListSeparator + l.Length.ToString();
			}

			public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues) {
				return new LinkArea((int)propertyValues["Start"], (int)propertyValues["Length"]);
			}

			public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) {
				return true;
			}

			public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) {
				return TypeDescriptor.GetProperties(typeof(LinkArea), attributes);
			}

			public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
				return true;
			}
		}		
		#endregion	// LinkAreaConverter Class

		private int start;
		private int length;
	
		public LinkArea (int start, int length)
		{
			this.start = start;
			this.length = length;
		}
		
		#region Public Properties
		
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public int Length {
			get { return length; }
			set { length = value; }
		}				
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool IsEmpty {
			get {
				if (start == 0 && length == 0)
					return true;
				else
					return false;			
				
			}
		}
		
		#endregion //Public Properties
		
		#region Methods

		public override bool Equals (object o)
		{
			if (!(o is LinkArea)) 
				return false;			

			LinkArea comp = (LinkArea) o;
			return (comp.Start == start && comp.Length == length);
		}

		public override int GetHashCode ()
		{
			return start << 4 | length;
		}
		
		public override string ToString ()
		{
			return string.Format ("{{Start={0}, Length={1}}}", this.start.ToString (), this.length.ToString ());
		}
		
		public static bool operator == (LinkArea linkArea1, LinkArea linkArea2)
		{
			return (linkArea1.Length == linkArea2.Length) && (linkArea1.Start == linkArea2.Start);
		}

		public static bool operator != (LinkArea linkArea1, LinkArea linkArea2)
		{
			return !(linkArea1 == linkArea2);
		}
		#endregion //Methods
		
	}
}
