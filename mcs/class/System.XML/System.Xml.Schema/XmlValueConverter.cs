//
// XmlValueConverter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc,
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

#if NET_2_0

using System;

namespace System.Xml.Schema
{
	internal abstract class XmlValueConverter
	{
		[MonoTODO]
		protected XmlValueConverter ()
		{
		}

		public abstract object ChangeType (bool value, Type type);

		public abstract object ChangeType (DateTime value, Type type);

		public abstract object ChangeType (decimal value, Type type);

		public abstract object ChangeType (double value, Type type);

		public abstract object ChangeType (int value, Type type);

		public abstract object ChangeType (long value, Type type);

		public abstract object ChangeType (object value, Type type);

		public abstract object ChangeType (float value, Type type);

		public abstract object ChangeType (string value, Type type);

		public abstract object ChangeType (object value, Type type, IXmlNamespaceResolver nsResolver);

		public abstract object ChangeType (string value, Type type, IXmlNamespaceResolver nsResolver);

		public abstract bool ToBoolean (bool value);

		public abstract bool ToBoolean (DateTime value);

		public abstract bool ToBoolean (decimal value);

		public abstract bool ToBoolean (double value);

		public abstract bool ToBoolean (int value);

		public abstract bool ToBoolean (long value);

		public abstract bool ToBoolean (object value);

		public abstract bool ToBoolean (float value);

		public abstract bool ToBoolean (string value);

		public abstract DateTime ToDateTime (bool value);

		public abstract DateTime ToDateTime (DateTime value);

		public abstract DateTime ToDateTime (decimal value);

		public abstract DateTime ToDateTime (double value);

		public abstract DateTime ToDateTime (int value);

		public abstract DateTime ToDateTime (long value);

		public abstract DateTime ToDateTime (object value);

		public abstract DateTime ToDateTime (float value);

		public abstract DateTime ToDateTime (string value);

		public abstract decimal ToDecimal (bool value);

		public abstract decimal ToDecimal (DateTime value);

		public abstract decimal ToDecimal (decimal value);

		public abstract decimal ToDecimal (double value);

		public abstract decimal ToDecimal (int value);

		public abstract decimal ToDecimal (long value);

		public abstract decimal ToDecimal (object value);

		public abstract decimal ToDecimal (float value);

		public abstract decimal ToDecimal (string value);

		public abstract double ToDouble (bool value);

		public abstract double ToDouble (DateTime value);

		public abstract double ToDouble (decimal value);

		public abstract double ToDouble (double value);

		public abstract double ToDouble (int value);

		public abstract double ToDouble (long value);

		public abstract double ToDouble (object value);

		public abstract double ToDouble (float value);

		public abstract double ToDouble (string value);

		public abstract int ToInt32 (bool value);

		public abstract int ToInt32 (DateTime value);

		public abstract int ToInt32 (decimal value);

		public abstract int ToInt32 (double value);

		public abstract int ToInt32 (int value);

		public abstract int ToInt32 (long value);

		public abstract int ToInt32 (object value);

		public abstract int ToInt32 (float value);

		public abstract int ToInt32 (string value);

		public abstract long ToInt64 (bool value);

		public abstract long ToInt64 (DateTime value);

		public abstract long ToInt64 (decimal value);

		public abstract long ToInt64 (double value);

		public abstract long ToInt64 (int value);

		public abstract long ToInt64 (long value);

		public abstract long ToInt64 (object value);

		public abstract long ToInt64 (float value);

		public abstract long ToInt64 (string value);

		public abstract float ToSingle (bool value);

		public abstract float ToSingle (DateTime value);

		public abstract float ToSingle (decimal value);

		public abstract float ToSingle (double value);

		public abstract float ToSingle (int value);

		public abstract float ToSingle (long value);

		public abstract float ToSingle (object value);

		public abstract float ToSingle (float value);

		public abstract float ToSingle (string value);

		public abstract string ToString (bool value);

		public abstract string ToString (DateTime value);

		public abstract string ToString (decimal value);

		public abstract string ToString (double value);

		public abstract string ToString (int value);

		public abstract string ToString (long value);

		public abstract string ToString (object value);

		public abstract string ToString (object value, IXmlNamespaceResolver nsResolver);

		public abstract string ToString (float value);

		public abstract string ToString (string value);

		public abstract string ToString (string value, IXmlNamespaceResolver nsResolver);
	}

	internal class XsdNonPermissiveConverter : XmlValueConverter
	{
		readonly XmlTypeCode typeCode;

		public XsdNonPermissiveConverter (XmlTypeCode typeCode)
		{
			this.typeCode = typeCode;
		}

		public XmlTypeCode Code {
			get { return typeCode; }
		}

		public override object ChangeType (bool value, Type type)
		{
			return ChangeType ((object) value, type);
		}

		public override object ChangeType (DateTime value, Type type)
		{
			return ChangeType ((object) value, type);
		}

		public override object ChangeType (decimal value, Type type)
		{
			return ChangeType ((object) value, type);
		}

		public override object ChangeType (double value, Type type)
		{
			return ChangeType ((object) value, type);
		}

		public override object ChangeType (int value, Type type)
		{
			return ChangeType ((object) value, type);
		}

		public override object ChangeType (long value, Type type)
		{
			return ChangeType ((object) value, type);
		}

		public override object ChangeType (float value, Type type)
		{
			return ChangeType ((object) value, type);
		}

		public override object ChangeType (string value, Type type)
		{
			return ChangeType ((object) value, type);
		}

		public override object ChangeType (object value, Type type)
		{
			return ChangeType (value, type, null);
		}

		[MonoTODO]
		public override object ChangeType (object value, Type type, IXmlNamespaceResolver nsResolver)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (type == null)
				throw new ArgumentNullException ("type");
			switch (Type.GetTypeCode (value.GetType ())) {
			case TypeCode.Boolean:
				bool bvalue = (bool) value;
				switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean:
					return ToBoolean (bvalue);
				case TypeCode.DateTime:
					return ToDateTime (bvalue);
				case TypeCode.Decimal:
					return ToDecimal (bvalue);
				case TypeCode.Double:
					return ToDouble (bvalue);
				case TypeCode.Int32:
					return ToInt32 (bvalue);
				case TypeCode.Int64:
					return ToInt64 (bvalue);
				case TypeCode.Single:
					return ToSingle (bvalue);
				case TypeCode.String:
					return ToString (bvalue);
				}
				break;
//			case TypeCode.Byte:
//			case TypeCode.Char:
			case TypeCode.DateTime:
				DateTime dtvalue = (DateTime) value;
				switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean:
					return ToBoolean (dtvalue);
				case TypeCode.DateTime:
					return ToDateTime (dtvalue);
				case TypeCode.Decimal:
					return ToDecimal (dtvalue);
				case TypeCode.Double:
					return ToDouble (dtvalue);
				case TypeCode.Int32:
					return ToInt32 (dtvalue);
				case TypeCode.Int64:
					return ToInt64 (dtvalue);
				case TypeCode.Single:
					return ToSingle (dtvalue);
				case TypeCode.String:
					return ToString (dtvalue);
				}
				break;
//			case TypeCode.DBNull:
			case TypeCode.Decimal:
				decimal decvalue = (decimal) value;
				switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean:
					return ToBoolean (decvalue);
				case TypeCode.DateTime:
					return ToDateTime (decvalue);
				case TypeCode.Decimal:
					return ToDecimal (decvalue);
				case TypeCode.Double:
					return ToDouble (decvalue);
				case TypeCode.Int32:
					return ToInt32 (decvalue);
				case TypeCode.Int64:
					return ToInt64 (decvalue);
				case TypeCode.Single:
					return ToSingle (decvalue);
				case TypeCode.String:
					return ToString (decvalue);
				}
				break;
			case TypeCode.Double:
				double dblvalue = (double) value;
				switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean:
					return ToBoolean (dblvalue);
				case TypeCode.DateTime:
					return ToDateTime (dblvalue);
				case TypeCode.Decimal:
					return ToDecimal (dblvalue);
				case TypeCode.Double:
					return ToDouble (dblvalue);
				case TypeCode.Int32:
					return ToInt32 (dblvalue);
				case TypeCode.Int64:
					return ToInt64 (dblvalue);
				case TypeCode.Single:
					return ToSingle (dblvalue);
				case TypeCode.String:
					return ToString (dblvalue);
				}
				break;
//			case TypeCode.Empty:
//			case TypeCode.Int16:
			case TypeCode.Int32:
				int ivalue = (int) value;
				switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean:
					return ToBoolean (ivalue);
				case TypeCode.DateTime:
					return ToDateTime (ivalue);
				case TypeCode.Decimal:
					return ToDecimal (ivalue);
				case TypeCode.Double:
					return ToDouble (ivalue);
				case TypeCode.Int32:
					return ToInt32 (ivalue);
				case TypeCode.Int64:
					return ToInt64 (ivalue);
				case TypeCode.Single:
					return ToSingle (ivalue);
				case TypeCode.String:
					return ToString (ivalue);
				}
				break;
			case TypeCode.Int64:
				long lvalue = (long) value;
				switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean:
					return ToBoolean (lvalue);
				case TypeCode.DateTime:
					return ToDateTime (lvalue);
				case TypeCode.Decimal:
					return ToDecimal (lvalue);
				case TypeCode.Double:
					return ToDouble (lvalue);
				case TypeCode.Int32:
					return ToInt32 (lvalue);
				case TypeCode.Int64:
					return ToInt64 (lvalue);
				case TypeCode.Single:
					return ToSingle (lvalue);
				case TypeCode.String:
					return ToString (lvalue);
				}
				break;
//			case TypeCode.Object:
//			case TypeCode.SByte:
			case TypeCode.Single:
				float fvalue = (float) value;
				switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean:
					return ToBoolean (fvalue);
				case TypeCode.DateTime:
					return ToDateTime (fvalue);
				case TypeCode.Decimal:
					return ToDecimal (fvalue);
				case TypeCode.Double:
					return ToDouble (fvalue);
				case TypeCode.Int32:
					return ToInt32 (fvalue);
				case TypeCode.Int64:
					return ToInt64 (fvalue);
				case TypeCode.Single:
					return ToSingle (fvalue);
				case TypeCode.String:
					return ToString (fvalue);
				}
				break;
			case TypeCode.String:
				string svalue = (string) value;
				switch (Type.GetTypeCode (type)) {
				case TypeCode.Boolean:
					return ToBoolean (svalue);
				case TypeCode.DateTime:
					return ToDateTime (svalue);
				case TypeCode.Decimal:
					return ToDecimal (svalue);
				case TypeCode.Double:
					return ToDouble (svalue);
				case TypeCode.Int32:
					return ToInt32 (svalue);
				case TypeCode.Int64:
					return ToInt64 (svalue);
				case TypeCode.Single:
					return ToSingle (svalue);
				case TypeCode.String:
					return ToString (svalue);
				}
				break;
//			case TypeCode.UInt16:
//			case TypeCode.UInt32:
//			case TypeCode.UInt64:
			default:
				if (type == typeof (TimeSpan))
					// xs:duration, xdt:yearMonthDuration,
					// xdt:dayTimeDuration. FIXME: yMD to
					// dTD and dTD to yMD are not allowed.
					return ToTimeSpan (value);
				if (value.GetType () == typeof (byte [])) {
					// xs:base64 by default
					if (type == typeof (string))
						return XQueryConvert.Base64BinaryToString ((byte []) value);
					else if (type == typeof (byte []))
						return value;
				}
				if (value.GetType () == type) {
					if (type == typeof (XmlQualifiedName)) {
						// xs:QName and xs:NOTATION
						throw new NotImplementedException ();
					}
				}
				break;
			}
			throw Error (value.GetType (), type);
		}

		public override object ChangeType (string value, Type type, IXmlNamespaceResolver nsResolver)
		{
			return ChangeType ((object) value, type, nsResolver);
		}

		public TimeSpan ToTimeSpan (bool value)
		{
			throw Error (typeof (bool), typeof (TimeSpan));
		}

		public TimeSpan ToTimeSpan (DateTime value)
		{
			throw Error (typeof (bool), typeof (TimeSpan));
		}

		public TimeSpan ToTimeSpan (decimal value)
		{
			throw Error (typeof (bool), typeof (TimeSpan));
		}

		public TimeSpan ToTimeSpan (double value)
		{
			throw Error (typeof (bool), typeof (TimeSpan));
		}

		public TimeSpan ToTimeSpan (int value)
		{
			throw Error (typeof (bool), typeof (TimeSpan));
		}

		public TimeSpan ToTimeSpan (long value)
		{
			throw Error (typeof (bool), typeof (TimeSpan));
		}

		public virtual TimeSpan ToTimeSpan (object value)
		{
			// Allow on overriden converter for xs:duration,
			// xdt:dayTimeDuration and xdt:yearMonthDuration.
			throw Error (typeof (bool), typeof (TimeSpan));
		}

		public TimeSpan ToTimeSpan (float value)
		{
			throw Error (typeof (bool), typeof (TimeSpan));
		}

		public TimeSpan ToTimeSpan (string value)
		{
			throw Error (typeof (bool), typeof (TimeSpan));
		}

		protected InvalidCastException Error (Type valueType, Type destType)
		{
			return new InvalidCastException (String.Format ("The conversion from {0} value to {1} type via {2} type is not allowed.", valueType, destType, typeCode));
		}

		public override bool ToBoolean (bool value)
		{
			throw Error (typeof (bool), typeof (bool));
		}

		public override bool ToBoolean (DateTime value)
		{
			throw Error (typeof (DateTime), typeof (bool));
		}

		public override bool ToBoolean (decimal value)
		{
			throw Error (typeof (decimal), typeof (bool));
		}

		public override bool ToBoolean (double value)
		{
			throw Error (typeof (double), typeof (bool));
		}

		public override bool ToBoolean (int value)
		{
			throw Error (typeof (int), typeof (bool));
		}

		public override bool ToBoolean (long value)
		{
			throw Error (typeof (long), typeof (bool));
		}

		public override bool ToBoolean (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			throw Error (value.GetType (), typeof (bool));
		}

		public override bool ToBoolean (float value)
		{
			throw Error (typeof (float), typeof (bool));
		}

		public override bool ToBoolean (string value)
		{
			throw Error (typeof (string), typeof (bool));
		}

		public override DateTime ToDateTime (bool value)
		{
			throw Error (typeof (bool), typeof (DateTime));
		}

		public override DateTime ToDateTime (DateTime value)
		{
			throw Error (typeof (DateTime), typeof (DateTime));
		}

		public override DateTime ToDateTime (decimal value)
		{
			throw Error (typeof (decimal), typeof (DateTime));
		}

		public override DateTime ToDateTime (double value)
		{
			throw Error (typeof (double), typeof (DateTime));
		}

		public override DateTime ToDateTime (int value)
		{
			throw Error (typeof (int), typeof (DateTime));
		}

		public override DateTime ToDateTime (long value)
		{
			throw Error (typeof (long), typeof (DateTime));
		}

		public override DateTime ToDateTime (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			throw Error (value.GetType (), typeof (DateTime));
		}

		public override DateTime ToDateTime (float value)
		{
			throw Error (typeof (float), typeof (DateTime));
		}

		public override DateTime ToDateTime (string value)
		{
			throw Error (typeof (string), typeof (DateTime));
		}

		public override decimal ToDecimal (bool value)
		{
			throw Error (typeof (bool), typeof (decimal));
		}

		public override decimal ToDecimal (DateTime value)
		{
			throw Error (typeof (DateTime), typeof (decimal));
		}

		public override decimal ToDecimal (decimal value)
		{
			throw Error (typeof (decimal), typeof (decimal));
		}

		public override decimal ToDecimal (double value)
		{
			throw Error (typeof (double), typeof (decimal));
		}

		public override decimal ToDecimal (int value)
		{
			throw Error (typeof (int), typeof (decimal));
		}

		public override decimal ToDecimal (long value)
		{
			throw Error (typeof (long), typeof (decimal));
		}

		public override decimal ToDecimal (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			throw Error (value.GetType (), typeof (decimal));
		}

		public override decimal ToDecimal (float value)
		{
			throw Error (typeof (float), typeof (decimal));
		}

		public override decimal ToDecimal (string value)
		{
			throw Error (typeof (string), typeof (decimal));
		}

		public override double ToDouble (bool value)
		{
			throw Error (typeof (bool), typeof (double));
		}

		public override double ToDouble (DateTime value)
		{
			throw Error (typeof (DateTime), typeof (double));
		}

		public override double ToDouble (decimal value)
		{
			throw Error (typeof (decimal), typeof (double));
		}

		public override double ToDouble (double value)
		{
			throw Error (typeof (double), typeof (double));
		}

		public override double ToDouble (int value)
		{
			throw Error (typeof (int), typeof (double));
		}

		public override double ToDouble (long value)
		{
			throw Error (typeof (long), typeof (double));
		}

		public override double ToDouble (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			throw Error (value.GetType (), typeof (double));
		}

		public override double ToDouble (float value)
		{
			throw Error (typeof (float), typeof (double));
		}

		public override double ToDouble (string value)
		{
			throw Error (typeof (string), typeof (double));
		}

		public override float ToSingle (bool value)
		{
			throw Error (typeof (bool), typeof (float));
		}

		public override float ToSingle (DateTime value)
		{
			throw Error (typeof (DateTime), typeof (float));
		}

		public override float ToSingle (decimal value)
		{
			throw Error (typeof (decimal), typeof (float));
		}

		public override float ToSingle (double value)
		{
			throw Error (typeof (double), typeof (float));
		}

		public override float ToSingle (int value)
		{
			throw Error (typeof (int), typeof (float));
		}

		public override float ToSingle (long value)
		{
			throw Error (typeof (long), typeof (float));
		}

		public override float ToSingle (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			throw Error (value.GetType (), typeof (float));
		}

		public override float ToSingle (float value)
		{
			throw Error (typeof (float), typeof (float));
		}

		public override float ToSingle (string value)
		{
			throw Error (typeof (string), typeof (float));
		}

		public override int ToInt32 (bool value)
		{
			throw Error (typeof (bool), typeof (int));
		}

		public override int ToInt32 (DateTime value)
		{
			throw Error (typeof (DateTime), typeof (int));
		}

		public override int ToInt32 (decimal value)
		{
			throw Error (typeof (decimal), typeof (int));
		}

		public override int ToInt32 (double value)
		{
			throw Error (typeof (double), typeof (int));
		}

		public override int ToInt32 (int value)
		{
			throw Error (typeof (int), typeof (int));
		}

		public override int ToInt32 (long value)
		{
			throw Error (typeof (long), typeof (int));
		}

		public override int ToInt32 (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			throw Error (value.GetType (), typeof (int));
		}

		public override int ToInt32 (float value)
		{
			throw Error (typeof (float), typeof (int));
		}

		public override int ToInt32 (string value)
		{
			throw Error (typeof (string), typeof (int));
		}

		public override long ToInt64 (bool value)
		{
			throw Error (typeof (bool), typeof (long));
		}

		public override long ToInt64 (DateTime value)
		{
			throw Error (typeof (DateTime), typeof (long));
		}

		public override long ToInt64 (decimal value)
		{
			throw Error (typeof (decimal), typeof (long));
		}

		public override long ToInt64 (double value)
		{
			throw Error (typeof (double), typeof (long));
		}

		public override long ToInt64 (int value)
		{
			throw Error (typeof (int), typeof (long));
		}

		public override long ToInt64 (long value)
		{
			throw Error (typeof (long), typeof (long));
		}

		public override long ToInt64 (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			throw Error (value.GetType (), typeof (long));
		}

		public override long ToInt64 (float value)
		{
			throw Error (typeof (float), typeof (long));
		}

		public override long ToInt64 (string value)
		{
			throw Error (typeof (string), typeof (long));
		}

		public override string ToString (bool value)
		{
			throw Error (typeof (bool), typeof (string));
		}

		public override string ToString (DateTime value)
		{
			throw Error (typeof (DateTime), typeof (string));
		}

		public override string ToString (decimal value)
		{
			throw Error (typeof (decimal), typeof (string));
		}

		public override string ToString (double value)
		{
			throw Error (typeof (double), typeof (string));
		}

		public override string ToString (int value)
		{
			throw Error (typeof (int), typeof (string));
		}

		public override string ToString (long value)
		{
			throw Error (typeof (long), typeof (string));
		}

		public override string ToString (object value)
		{
			return ToString (value, null);
		}

		public override string ToString (object value, IXmlNamespaceResolver nsResolver)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			throw Error (value.GetType (), typeof (string));
		}

		public override string ToString (float value)
		{
			throw Error (typeof (float), typeof (string));
		}

		public override string ToString (string value)
		{
			return ToString (value, null);
		}
		
		public override string ToString (string value, IXmlNamespaceResolver nsResolver)
		{
			throw Error (typeof (string), typeof (string));
		}
	}

	internal class XsdLaxConverter : XsdNonPermissiveConverter
	{
		public XsdLaxConverter (XmlTypeCode code)
			: base (code)
		{
		}

		public override string ToString (bool value)
		{
			return XmlConvert.ToString (value);
		}

		public override string ToString (decimal value)
		{
			return XmlConvert.ToString (value);
		}

		public override string ToString (double value)
		{
			return XmlConvert.ToString (value);
		}

		public override string ToString (int value)
		{
			return XmlConvert.ToString (value);
		}

		public override string ToString (long value)
		{
			return XmlConvert.ToString (value);
		}

		public override string ToString (object value)
		{
			return ChangeType (value, typeof (string)) as string;
		}

		public override string ToString (float value)
		{
			return XmlConvert.ToString (value);
		}

		public override bool ToBoolean (bool value)
		{
			return value;
		}

		public override decimal ToDecimal (decimal value)
		{
			return value;
		}

		public override decimal ToDecimal (double value)
		{
			return (decimal) value;
		}

		public override decimal ToDecimal (int value)
		{
			return (decimal) value;
		}

		public override decimal ToDecimal (long value)
		{
			return (decimal) value;
		}

		[MonoTODO]
		public override decimal ToDecimal (object value)
		{
			return (decimal) ChangeType (value, typeof (decimal));
		}

		public override decimal ToDecimal (float value)
		{
			return (decimal) value;
		}

		public override double ToDouble (decimal value)
		{
			return (double) value;
		}

		public override double ToDouble (double value)
		{
			return (double) value;
		}

		public override double ToDouble (int value)
		{
			return (double) value;
		}

		public override double ToDouble (long value)
		{
			return (double) value;
		}

		[MonoTODO]
		public override double ToDouble (object value)
		{
			return (double) ChangeType (value, typeof (double));
		}

		public override double ToDouble (float value)
		{
			return (double) value;
		}

		public override float ToSingle (decimal value)
		{
			return (float) value;
		}

		public override float ToSingle (double value)
		{
			return (float) value;
		}

		public override float ToSingle (int value)
		{
			return (float) value;
		}

		public override float ToSingle (long value)
		{
			return (float) value;
		}

		[MonoTODO]
		public override float ToSingle (object value)
		{
			return (float) ChangeType (value, typeof (float));
		}

		public override float ToSingle (float value)
		{
			return (float) value;
		}

		public override int ToInt32 (int value)
		{
			return value;
		}

		public override int ToInt32 (long value)
		{
			return XQueryConvert.IntegerToInt (value);
		}

		[MonoTODO]
		public override int ToInt32 (object value)
		{
			return (int) ChangeType (value, typeof (int));
		}

		public override long ToInt64 (int value)
		{
			return value;
		}

		public override long ToInt64 (long value)
		{
			return value;
		}

		[MonoTODO]
		public override long ToInt64 (object value)
		{
			return (long) ChangeType (value, typeof (long));
		}
	}

	internal class XsdAnyTypeConverter : XsdNumericConverter
	{
		public XsdAnyTypeConverter (XmlTypeCode code)
			: base (code)
		{
		}

		#region boolean
		public override bool ToBoolean (decimal value)
		{
			return value != 0;
		}

		public override bool ToBoolean (double value)
		{
			return value != 0;
		}

		public override bool ToBoolean (int value)
		{
			return value != 0;
		}

		public override bool ToBoolean (long value)
		{
			return value != 0;
		}

		[MonoTODO]
		public override bool ToBoolean (object value)
		{
			return (bool) ChangeType (value, typeof (bool));
		}

		public override bool ToBoolean (float value)
		{
			return value != 0;
		}

		public override decimal ToDecimal (bool value)
		{
			return value ? 1 : 0;
		}

		public override double ToDouble (bool value)
		{
			return value ? 1 : 0;
		}

		public override float ToSingle (bool value)
		{
			return value ? 1 : 0;
		}

		public override int ToInt32 (bool value)
		{
			return value ? 1 : 0;
		}

		public override long ToInt64 (bool value)
		{
			return value ? 1 : 0;
		}
		#endregion

		#region string

		public override DateTime ToDateTime (DateTime value)
		{
			return value;
		}

		public override string ToString (DateTime value)
		{
			return XmlConvert.ToString (value);
		}

		public override string ToString (string value)
		{
			return value;
		}
		#endregion
	}

	internal class XsdStringConverter : XsdLaxConverter
	{
		public XsdStringConverter (XmlTypeCode code)
			: base (code)
		{
		}

		public override DateTime ToDateTime (DateTime value)
		{
			return value;
		}

		public override string ToString (DateTime value)
		{
			return XmlConvert.ToString (value);
		}

		public override string ToString (string value)
		{
			return value;
		}
	}

	internal class XsdNumericConverter : XsdLaxConverter
	{
		public XsdNumericConverter (XmlTypeCode code)
			: base (code)
		{
		}

		#region boolean
		public override bool ToBoolean (decimal value)
		{
			return value != 0;
		}

		public override bool ToBoolean (double value)
		{
			return value != 0;
		}

		public override bool ToBoolean (int value)
		{
			return value != 0;
		}

		public override bool ToBoolean (long value)
		{
			return value != 0;
		}

		[MonoTODO]
		public override bool ToBoolean (object value)
		{
			return (bool) ChangeType (value, typeof (bool));
		}

		public override bool ToBoolean (float value)
		{
			return value != 0;
		}

		public override decimal ToDecimal (bool value)
		{
			return value ? 1 : 0;
		}

		public override double ToDouble (bool value)
		{
			return value ? 1 : 0;
		}

		public override float ToSingle (bool value)
		{
			return value ? 1 : 0;
		}

		public override int ToInt32 (bool value)
		{
			return value ? 1 : 0;
		}

		public override long ToInt64 (bool value)
		{
			return value ? 1 : 0;
		}
		#endregion

		#region numeric with point to without point
		public override int ToInt32 (decimal value)
		{
			return XQueryConvert.DecimalToInt (value);
		}

		public override int ToInt32 (double value)
		{
			return XQueryConvert.DoubleToInt (value);
		}

		public override int ToInt32 (float value)
		{
			return XQueryConvert.FloatToInt (value);
		}

		public override long ToInt64 (decimal value)
		{
			return XQueryConvert.DecimalToInteger (value);
		}

		public override long ToInt64 (double value)
		{
			return XQueryConvert.DoubleToInteger (value);
		}

		public override long ToInt64 (float value)
		{
			return XQueryConvert.FloatToInteger (value);
		}
		#endregion
	}

	internal class XsdDateTimeConverter : XsdNonPermissiveConverter
	{
		public XsdDateTimeConverter (XmlTypeCode code)
			: base (code)
		{
		}

		public override string ToString (DateTime value)
		{
			return XmlConvert.ToString (value);
		}

		public override DateTime ToDateTime (DateTime value)
		{
			return value;
		}
	}

	internal class XsdBooleanConverter : XsdNumericConverter
	{
		public XsdBooleanConverter (XmlTypeCode code)
			: base (code)
		{
		}
	}

	internal class XsdMiscBaseConverter : XsdNonPermissiveConverter
	{
		public XsdMiscBaseConverter (XmlTypeCode code)
			: base (code)
		{
		}

		public override string ToString (string value)
		{
			return value;
		}

		public override object ChangeType (object value, Type type, IXmlNamespaceResolver nsResolver)
		{
			if (Code == XmlTypeCode.HexBinary) {
				if (value == null)
					throw new ArgumentNullException ("value");
				if (type == null)
					throw new ArgumentNullException ("type");
				if (value.GetType () == typeof (byte [])) {
					if (type == typeof (string))
						return XQueryConvert.HexBinaryToString ((byte []) value);
					else if (type == typeof (byte []))
						return value;
				}
			}
			return base.ChangeType (value, type, nsResolver);
		}
	}
}

#endif
