//
// System.Web.UI.DataSourceCacheDurationConverter.cs
//
// Authors:
//     Arina Itkes (arinai@mainsoft.com)
//     Marek Habersack <mhabersack@novell.com>
//
// (C) 2007 Mainsoft Co. (http://www.mainsoft.com)
// (C) 2010 Novell, Inc (http://novell.com/)
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Security.Permissions;

namespace System.Web.UI
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DataSourceCacheDurationConverter : Int32Converter
	{
		static readonly List <int> standardValues = new List <int> {
			0
		};
		
		public DataSourceCacheDurationConverter ()
		{
		}
		
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;

			return base.CanConvertTo (context, destinationType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				return null;

			string val = value as string;
			if (val != null && (val.Length == 0 || String.Compare ("infinite", val, StringComparison.OrdinalIgnoreCase) == 0))
				return (int)0;

			return base.ConvertFrom (context, culture, value);
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (string)) {
				if (value == null)
					return String.Empty;
				
				if (value is int && (int)value == 0)
					return "Infinite";
			}
			
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			return new StandardValuesCollection (standardValues);
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
