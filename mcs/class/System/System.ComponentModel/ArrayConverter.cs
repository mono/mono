//
// System.ComponentModel.ArrayConverter
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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

using System.Globalization;

namespace System.ComponentModel
{
	public class ArrayConverter : CollectionConverter
	{
		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value,
						  Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException ("destinationType");

			if (destinationType == typeof (string) && (value is Array))
				return value.GetType ().Name + " Array";

			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context,
									    object value, Attribute[] attributes)
		{
			if (value == null)
				throw new NullReferenceException ();

			PropertyDescriptorCollection col = new PropertyDescriptorCollection (null);
			if (value is Array) {
				Array array = (Array)value;
				for (int i = 0; i < array.Length; i ++) {
					col.Add (new ArrayPropertyDescriptor (i, array.GetType()));
				}
			}

			return col;
		}

		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		internal class ArrayPropertyDescriptor : PropertyDescriptor
		{
			int index;
			Type array_type;
			public ArrayPropertyDescriptor (int index, Type array_type)
				: base (String.Format ("[{0}]", index), null)
			{
				this.index = index;
				this.array_type = array_type;
			}

			public override Type ComponentType {
				get { return array_type; }
			}

			public override Type PropertyType {
				get { return array_type.GetElementType(); }
			}

			public override bool IsReadOnly {
				get { return false; }
			}

			public override object GetValue (object component)
			{
				if (component == null)
					return null;

				return ((Array)component).GetValue (index);
			}

			public override void SetValue (object component, object value)
			{
				if (component == null)
					return;

				((Array)component).SetValue (value, index);
			}

			public override void ResetValue (object component)
			{
			}

			public override bool CanResetValue (object component)
			{
				return false;
			}

			public override bool ShouldSerializeValue (object component)
			{
				return false;
			}
		}
	}
}

