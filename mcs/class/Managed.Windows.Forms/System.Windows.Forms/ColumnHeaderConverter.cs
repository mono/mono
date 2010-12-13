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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	olivier Dufour olivier.duff@free.fr
//


// COMPLETE

using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace System.Windows.Forms
{
	public class ColumnHeaderConverter : ExpandableObjectConverter
	{
		public ColumnHeaderConverter ()
		{
		}

		public override Object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
		{
			if (destinationType == typeof (InstanceDescriptor) && value is ColumnHeader)
			{
				ConstructorInfo constructor_info;
				Type[] type;
				ColumnHeader column_header;

				column_header = (ColumnHeader)value;
				if (column_header.ImageIndex != -1)
				{
					type = new Type[] { typeof (int) };
					constructor_info = typeof (ColumnHeader).GetConstructor (type);
					if (constructor_info != null)
					{
						object[] arguments = new object[] { column_header.ImageIndex };
						return new InstanceDescriptor (constructor_info, (ICollection)arguments, false);
					}
				}
				else if (string.IsNullOrEmpty(column_header.ImageKey))
				{
					type = new Type[] { typeof (string) };
					constructor_info = typeof (ColumnHeader).GetConstructor (type);
					if (constructor_info != null)
					{
						object[] arguments = new object[] { column_header.ImageKey };
						return new InstanceDescriptor (constructor_info, (ICollection)arguments, false);
					}
				}
				else
				{
					type = Type.EmptyTypes;
					constructor_info = typeof (ColumnHeader).GetConstructor (type);
					if (constructor_info != null)
					{
						object[] arguments = new object[0] {};
						return new InstanceDescriptor (constructor_info, (ICollection)arguments, false);
					}
				}

			}
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (InstanceDescriptor))
				return true;
			else
				return base.CanConvertTo (context, destinationType);
		}
	}
}
