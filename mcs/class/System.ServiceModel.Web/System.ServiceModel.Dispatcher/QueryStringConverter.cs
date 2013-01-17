//
// QueryStringConverter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace System.ServiceModel.Dispatcher
{
	public class QueryStringConverter
	{
		// "Service Operation Parameters and URLs"
		// http://msdn2.microsoft.com/en-us/library/bb412172.aspx
		public virtual bool CanConvert (Type type)
		{
			switch (Type.GetTypeCode (type)) {
			case TypeCode.DBNull:
			case TypeCode.Empty:
				return false;
			case TypeCode.Object:
				if (type == typeof (TimeSpan))
					return true;
				if (type == typeof (DateTimeOffset))
					return true;
				if (type == typeof (Guid))
					return true;
				if (type == typeof (object))
					return true;
//				if (type.GetCustomAttributes (typeof (TypeConverterAttribute), true).Length > 0)
//					return true;
				return false;
			default:
				return true;
			}
		}

		public virtual object ConvertStringToValue (string parameter, Type parameterType)
		{
			if (parameterType == null)
				throw new ArgumentNullException ("parameterType");

			if (!CanConvert (parameterType))
				throw new NotSupportedException (String.Format ("Conversion from the argument parameterType '{0}' is not supported", parameterType));

			if (parameterType.IsEnum)
				return Enum.Parse(parameterType, parameter, true);

			switch (Type.GetTypeCode (parameterType)) {
			case TypeCode.String:
				return parameter;
#if !NET_2_1
			case TypeCode.Char:
				return parameter != null ? Char.Parse (parameter) : default (char);
#endif
			case TypeCode.SByte:
				return parameter != null ? SByte.Parse (parameter, CultureInfo.InvariantCulture): default (sbyte);
			case TypeCode.Byte:
				return parameter != null ? Byte.Parse (parameter, CultureInfo.InvariantCulture): default (byte);
			case TypeCode.Int16:
				return parameter != null ? Int16.Parse (parameter, CultureInfo.InvariantCulture): default (short);
			case TypeCode.Int32:
				return parameter != null ? Int32.Parse (parameter, CultureInfo.InvariantCulture): default (int);
			case TypeCode.Int64:
				return parameter != null ? Int64.Parse (parameter, CultureInfo.InvariantCulture): default (long);
			case TypeCode.UInt16:
				return parameter != null ? UInt16.Parse (parameter, CultureInfo.InvariantCulture): default (ushort);
			case TypeCode.UInt32:
				return parameter != null ? UInt32.Parse (parameter, CultureInfo.InvariantCulture): default (uint);
			case TypeCode.UInt64:
				return parameter != null ? UInt64.Parse (parameter, CultureInfo.InvariantCulture): default (ulong);
			case TypeCode.DateTime:
				return parameter != null ? DateTime.Parse (parameter, CultureInfo.InvariantCulture): default (DateTime);
			case TypeCode.Boolean:
				return parameter != null ? Boolean.Parse (parameter): default (bool);
			case TypeCode.Single:
				return parameter != null ? Single.Parse (parameter, CultureInfo.InvariantCulture): default (float);
			case TypeCode.Double:
				return parameter != null ? Double.Parse (parameter, CultureInfo.InvariantCulture): default (double);
			case TypeCode.Decimal:
				return parameter != null ? Decimal.Parse (parameter, CultureInfo.InvariantCulture): default (decimal);
			case TypeCode.Object:
				if (parameterType == typeof (TimeSpan))
					return TimeSpan.Parse (parameter);
				if (parameterType == typeof (DateTimeOffset))
					return DateTimeOffset.Parse (parameter, CultureInfo.InvariantCulture);
				if (parameterType == typeof (Guid))
					return new Guid (parameter);
				break;
			}
			throw new NotSupportedException (String.Format ("Cannot convert parameter string '{0}' to parameter type '{1}'", parameter, parameterType));
		}

		public virtual string ConvertValueToString (object parameter, Type parameterType)
		{
			if (parameterType == null)
				throw new ArgumentNullException ("parameterType");
			if (parameterType.IsValueType && parameter == null)
				throw new ArgumentNullException ("parameter");

			if (parameter == null)
				return null;

			if (parameter.GetType () != parameterType)
				throw new InvalidCastException (String.Format ("This QueryStringConverter does not support cast from {0} to {1}", parameter.GetType (), parameterType));

			if (!CanConvert (parameterType))
				throw new NotSupportedException (String.Format ("Conversion from the argument parameterType '{0}' is not supported", parameterType));

			if (parameter is IFormattable)
				((IFormattable) parameter).ToString (null, CultureInfo.InvariantCulture);
			return parameter.ToString ();
		}
	}
}
