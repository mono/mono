//
// System.ComponentModel.ExpandableObjectConverter.cs
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// Copyright (C) Tim Coleman, 2002
// (C) 2003 Andreas Nahr
//

using System;

namespace System.ComponentModel 
{
	public class ExpandableObjectConverter : TypeConverter 
	{

		public ExpandableObjectConverter ()
		{
		}

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context,
									    object value, Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties (value, attributes);
		}

		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
