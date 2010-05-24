//
// JsonQueryStringConverter.cs
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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Dispatcher
{
	public class JsonQueryStringConverter : QueryStringConverter
	{
		DataContractJsonSerializer serializer = new DataContractJsonSerializer (typeof (object));

		internal string CustomWrapperName { get; set; }

		public override bool CanConvert (Type type)
		{
			// almost copy from QueryStringConverter, except that DBNull and XmlQualifiedName are supported
			switch (Type.GetTypeCode (type)) {
			//case TypeCode.DBNull:
			case TypeCode.Empty:
				return false;
			case TypeCode.Object:
				if (type == typeof (TimeSpan))
					return true;
				if (type == typeof (DateTimeOffset))
					return true;
				if (type == typeof (Guid))
					return true;
				if (type == typeof (XmlQualifiedName))
					return true;
				if (type == typeof (object))
					return true;
//				if (type.GetCustomAttributes (typeof (TypeConverterAttribute), true).Length > 0)
//					return true;

				// FIXME: it should return false for things like List<OfPrivateType>.
				return type.IsPublic || type.IsNestedPublic;
			default:
				return true;
			}
		}

		public override object ConvertStringToValue (string parameter, Type parameterType)
		{
			if (parameterType == null)
				throw new ArgumentNullException ("parameterType");

			if (!CanConvert (parameterType))
				throw new NotSupportedException (String.Format ("Conversion from the argument parameterType '{0}' is not supported", parameterType));

			// In general .NET JSON parser is sloppy. It accepts 
			// such a string that is actually invalid in terms of
			// the target type in JSON context.

			switch (Type.GetTypeCode (parameterType)) {
			case TypeCode.String:
				// LAMESPEC LAMESPEC LAMESPEC: we cannot give "foo" as the string value input (even if they are escaped as %22!)
				if (parameter == null)
					return null;
				if (parameter.Length > 1 && parameter [0] == '"' && parameter [parameter.Length - 1] == '"')
					return parameter.Substring (1, parameter.Length - 2);
				else if (parameter [0] != '"')
					return parameter;
				break;
#if !NET_2_1
			case TypeCode.Char:
				return parameter != null ? Char.Parse (parameter): default (char);
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
			}

			if (parameter == null)
				return null;


			DataContractJsonSerializer serializer =
				new DataContractJsonSerializer (parameterType);
			// hmm, it costs so silly though.
			return serializer.ReadObject (new MemoryStream (Encoding.UTF8.GetBytes (parameter)));
		}

		bool IsKnownType (Type t)
		{
			switch (Type.GetTypeCode (t)) {
			case TypeCode.Object:
				if (t == typeof (Guid) ||
				    t == typeof (DBNull) ||
				    t == typeof (DateTimeOffset) ||
				    t == typeof (TimeSpan) ||
				    t == typeof (XmlQualifiedName))
					return true;
				return false;
			default:
				return true;
			}
		}

		public override string ConvertValueToString (object parameter, Type parameterType)
		{
			if (parameterType == null)
				throw new ArgumentNullException ("parameterType");

			if (!CanConvert (parameterType))
				throw new NotSupportedException (String.Format ("Conversion from the argument parameterType '{0}' is not supported", parameterType));

			if (parameter == null)
				return null;

			if (parameter is DBNull)
				return "{}";

			parameterType = ToActualType (parameterType);

			if (parameter is IConvertible)
				parameter = ((IConvertible) parameter).ToType (parameterType, CultureInfo.InvariantCulture);

			switch (Type.GetTypeCode (parameterType)) {
			case TypeCode.String:
				string s = parameter is IFormattable ?
					((IFormattable) parameter).ToString (null, CultureInfo.InvariantCulture) :
					parameter.ToString ();
				StringBuilder sb = new StringBuilder (s);
				sb.Replace ("\"", "\\\"");
				sb.Insert (0, "\"");
				sb.Append ('\"');
				return sb.ToString ();
			default:
				if (parameterType == typeof (XmlQualifiedName)) {
					var qname = (XmlQualifiedName) parameter;
					return String.Concat ("\"", qname.Name, ":", qname.Namespace, "\"");
				}
				return parameter.ToString ();
			}

			throw new NotImplementedException ();
		}

		Type ToActualType (Type t)
		{
			switch (Type.GetTypeCode (t)) {
			case TypeCode.DBNull: // though DBNull.Value input is converted to "{}". This result is used for String input.
			case TypeCode.Char:
			case TypeCode.String:
				return typeof (string);
			case TypeCode.SByte:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
//				return typeof (long);
				return typeof (decimal);
			case TypeCode.Byte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
//				return typeof (ulong);
				return typeof (decimal);
			case TypeCode.DateTime:
			case TypeCode.Boolean:
				return t;
			case TypeCode.Single:
			case TypeCode.Double:
//				return typeof (double);
				return typeof (decimal);
			case TypeCode.Decimal:
				return typeof (decimal);
			default:
				return t;
			}
		}
	}
}
