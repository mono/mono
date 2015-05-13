//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.Management
{
	internal class ManagementPathConverter : ExpandableObjectConverter
	{
		public ManagementPathConverter()
		{
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType != typeof(ManagementPath))
			{
				return base.CanConvertFrom(context, sourceType);
			}
			else
			{
				return true;
			}
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType != typeof(InstanceDescriptor))
			{
				return base.CanConvertTo(context, destinationType);
			}
			else
			{
				return true;
			}
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType != null)
			{
				if (value as ManagementPath != null && destinationType == typeof(InstanceDescriptor))
				{
					ManagementPath managementPath = (ManagementPath)value;
					Type[] typeArray = new Type[1];
					typeArray[0] = typeof(string);
					ConstructorInfo constructor = typeof(ManagementPath).GetConstructor(typeArray);
					if (constructor != null)
					{
						object[] path = new object[1];
						path[0] = managementPath.Path;
						return new InstanceDescriptor(constructor, path);
					}
				}
				return base.ConvertTo(context, culture, value, destinationType);
			}
			else
			{
				throw new ArgumentNullException("destinationType");
			}
		}
	}
}