//
// DataTypeAttribute.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Pablo Ruiz García <pablo.ruiz@gmail.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
// Copyright (C) 2013 Pablo Ruiz García
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

namespace System.ComponentModel.DataAnnotations
{
#if NET_4_0
	[AttributeUsage (AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
#else
	[AttributeUsage (AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = false)]
#endif
	public class DataTypeAttribute : ValidationAttribute
	{
		public DataTypeAttribute (DataType dataType)
		{
			DataType = dataType;

			DisplayFormatAttribute displayFormat;
			switch (dataType) {
				case DataType.Time:
					displayFormat = new DisplayFormatAttribute ();
					displayFormat.ApplyFormatInEditMode = true;
					displayFormat.ConvertEmptyStringToNull = true;
					displayFormat.DataFormatString = "{0:t}";
#if NET_4_0
					displayFormat.HtmlEncode = true;
#endif
					break;
				case DataType.Date:
					displayFormat = new DisplayFormatAttribute ();
					displayFormat.ApplyFormatInEditMode = true;
					displayFormat.ConvertEmptyStringToNull = true;
					displayFormat.DataFormatString = "{0:d}";
#if NET_4_0
					displayFormat.HtmlEncode = true;
#endif
					break;
				case DataType.Currency:
					displayFormat = new DisplayFormatAttribute ();
					displayFormat.ApplyFormatInEditMode = false;
					displayFormat.ConvertEmptyStringToNull = true;
					displayFormat.DataFormatString = "{0:C}";
#if NET_4_0
					displayFormat.HtmlEncode = true;
#endif
					break;

				default:
					displayFormat = null;
					break;
			}

			DisplayFormat = displayFormat;
		}

		public DataTypeAttribute (string customDataType)
		{
			CustomDataType = customDataType;
		}

		public string CustomDataType { get; private set; }
		public DataType DataType { get; private set; }
		public DisplayFormatAttribute DisplayFormat { get; protected set; }

		public virtual string GetDataTypeName ()
		{
			DataType dt = DataType;
			if (dt == DataType.Custom)
				return CustomDataType;

			return dt.ToString ();
		}

		public override bool IsValid (object value)
		{
			// Returns alwasy true 	
			// See: http://msdn.microsoft.com/en-us/library/cc679235.aspx
			return true;
		}
	}
}
