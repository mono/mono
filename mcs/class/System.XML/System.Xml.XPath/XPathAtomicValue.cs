//
// XPathAtomicValue.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
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

using System.Collections;
using System.Xml;
using System.Xml.Schema;

namespace System.Xml.XPath
{
	public sealed class XPathAtomicValue : XPathItem, ICloneable
	{
		bool booleanValue;
		DateTime dateTimeValue;
		decimal decimalValue;
		double doubleValue;
		int intValue;
		long longValue;
		object objectValue;
		float floatValue;
		string stringValue;
		XmlSchemaType schemaType;
		XmlTypeCode xmlTypeCode;
		ICollection valueAsList;

		#region Constructors

		[MonoTODO]
		public XPathAtomicValue (bool value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Boolean;
			this.booleanValue = value;
			schemaType = xmlType;
		}

		[MonoTODO]
		public XPathAtomicValue (DateTime value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.DateTime;
			this.dateTimeValue = value;
			schemaType = xmlType;
		}

		[MonoTODO]
		public XPathAtomicValue (decimal value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Decimal;
			this.decimalValue = value;
			schemaType = xmlType;
		}

		[MonoTODO]
		public XPathAtomicValue (double value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Double;
			this.doubleValue = value;
			schemaType = xmlType;
		}

		[MonoTODO]
		public XPathAtomicValue (int value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Int;
			this.intValue = value;
			schemaType = xmlType;
		}

		[MonoTODO]
		public XPathAtomicValue (long value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Long;
			this.longValue = value;
			schemaType = xmlType;
		}

		[MonoTODO]
		public XPathAtomicValue (object value, XmlSchemaType xmlType)
		{
			// It accepts any kind of object, but will be rejected on each value properties.
			if (value == null)
				throw new ArgumentNullException ("value");
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");

			if (value is XPathAtomicValue)
				objectValue = ((XPathAtomicValue) value).TypedValue;
			else
				objectValue = value;
			schemaType = xmlType;
		}

		[MonoTODO]
		public XPathAtomicValue (float value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Float;
			this.floatValue = value;
			schemaType = xmlType;
		}

		[MonoTODO]
		public XPathAtomicValue (string value, XmlSchemaType xmlType)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.String;
			this.stringValue = value;
			schemaType = xmlType;
		}

		#endregion

		#region Methods

		object ICloneable.Clone ()
		{
			return this.Clone ();
		}

		[MonoTODO]
		public XPathAtomicValue Clone ()
		{
			return new XPathAtomicValue (this, schemaType);
		}

		[MonoTODO]
		public override object ValueAs (Type type, IXmlNamespaceResolver nsResolver)
		{
			switch (XmlTypeCodeFromRuntimeType (type, false)) {
			case XmlTypeCode.Int:
			case XmlTypeCode.Short:
			case XmlTypeCode.UnsignedShort:
				return ValueAsInt32;
			case XmlTypeCode.Decimal:
				return ValueAsDecimal;
			case XmlTypeCode.Double:
				return ValueAsDouble;
			case XmlTypeCode.Float:
				return ValueAsSingle;
			case XmlTypeCode.Long:
			case XmlTypeCode.UnsignedInt:
				return ValueAsInt64;
			case XmlTypeCode.String:
				return Value;
			case XmlTypeCode.DateTime:
				return ValueAsDateTime;
			case XmlTypeCode.Boolean:
				return ValueAsBoolean;
			case XmlTypeCode.Item:
				return TypedValue;
			case XmlTypeCode.QName:
				return XmlQualifiedName.Parse (Value, nsResolver);
			}
			if (type.GetInterface ("System.Collections.ICollection") != null)
				return ValueAsList;
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		// As long as I tried, neither of such XPathAtomicValue created
		// with XmlText that contains atomic value, XmlElement that
		// contains such XmlText, XmlDocument nor XPathNavigator 
		// created from such nodes returned false. So it won't be 
		// true on this class. Apparently this class needs more
		// documentation.
		public override bool IsNode {
			get { return false; }
		}

		[MonoTODO]
		public override object TypedValue {
			get {
				switch (xmlTypeCode) {
				case XmlTypeCode.Boolean:
					return ValueAsBoolean;
				case XmlTypeCode.DateTime:
					return ValueAsDateTime;
				case XmlTypeCode.Decimal:
					return ValueAsDecimal;
				case XmlTypeCode.Double:
					return ValueAsDouble;
				case XmlTypeCode.Long:
					return ValueAsInt64;
				case XmlTypeCode.Int:
					return ValueAsInt32;
				case XmlTypeCode.Float:
					return ValueAsSingle;
				case XmlTypeCode.String:
					return Value;
				}
				return objectValue;
			}
		}

		[MonoTODO]
		// This method works like ValueAsString.
		public override string Value {
			get {
				if (stringValue != null)
					return stringValue;
				switch (xmlTypeCode) {
				case XmlTypeCode.Boolean:
					stringValue = XQueryConvert.BooleanToString (booleanValue);
					break;
				case XmlTypeCode.DateTime:
					stringValue = XQueryConvert.DateTimeToString (dateTimeValue);
					break;
				case XmlTypeCode.Decimal:
					stringValue = XQueryConvert.DecimalToString (decimalValue);
					break;
				case XmlTypeCode.Double:
					stringValue = XQueryConvert.DoubleToString (doubleValue);
					break;
				case XmlTypeCode.Long:
					stringValue = XQueryConvert.IntegerToString (longValue);
					break;
				case XmlTypeCode.Int:
					stringValue = XQueryConvert.IntToString (intValue);
					break;
				case XmlTypeCode.Float:
					stringValue = XQueryConvert.FloatToString (floatValue);
					break;
				case XmlTypeCode.String:
					return stringValue;

				case XmlTypeCode.None:
				case XmlTypeCode.Item:
				case XmlTypeCode.AnyAtomicType:
					switch (XmlTypeCodeFromRuntimeType (objectValue.GetType (), false)) {
					case XmlTypeCode.String:
						stringValue = (string) objectValue;
						break;
					case XmlTypeCode.DateTime:
						stringValue = XQueryConvert.DateTimeToString ((DateTime) objectValue);
						break;
					case XmlTypeCode.Boolean:
						stringValue = XQueryConvert.BooleanToString ((bool) objectValue);
						break;
					case XmlTypeCode.Float:
						stringValue = XQueryConvert.FloatToString ((float) objectValue);
						break;
					case XmlTypeCode.Double:
						stringValue = XQueryConvert.DoubleToString ((double) objectValue);
						break;
					case XmlTypeCode.Decimal:
						stringValue = XQueryConvert.DecimalToString ((decimal) objectValue);
						break;
					case XmlTypeCode.Long:
						stringValue = XQueryConvert.IntegerToString ((long) objectValue);
						break;
					case XmlTypeCode.Int:
						stringValue = XQueryConvert.IntToString ((int) objectValue);
						break;
					}
					break;
				}
				if (stringValue != null)
					return stringValue;

				if (objectValue != null)
					throw new InvalidOperationException (String.Format ("Conversion from runtime type {0} to {1} is not supported", objectValue.GetType (), XmlTypeCode.String));
				else
					throw new InvalidOperationException (String.Format ("Conversion from schema type {0} (type code {1}) to {2} is not supported", schemaType.QualifiedName, xmlTypeCode, XmlTypeCode.String));
			}
		}

		[MonoTODO]
		public override bool ValueAsBoolean {
			get {
				switch (xmlTypeCode) {
				case XmlTypeCode.Boolean:
					return booleanValue;
				case XmlTypeCode.Decimal:
					return XQueryConvert.DecimalToBoolean (decimalValue);
				case XmlTypeCode.Double:
					return XQueryConvert.DoubleToBoolean (doubleValue);
				case XmlTypeCode.Long:
					return XQueryConvert.IntegerToBoolean (longValue);
				case XmlTypeCode.Int:
					return XQueryConvert.IntToBoolean (intValue);
				case XmlTypeCode.Float:
					return XQueryConvert.FloatToBoolean (floatValue);
				case XmlTypeCode.String:
					return XQueryConvert.StringToBoolean (stringValue);

				case XmlTypeCode.None:
				case XmlTypeCode.Item:
				case XmlTypeCode.AnyAtomicType:
					if (objectValue is bool)
						return (bool) objectValue;
					break;

				}

				throw new InvalidOperationException (String.Format ("Conversion from {0} to {1} is not supported", schemaType, XmlTypeCode.Boolean));
			}
		}

		[MonoTODO]
		public override DateTime ValueAsDateTime {
			get {
				switch (xmlTypeCode) {
				case XmlTypeCode.DateTime:
					return dateTimeValue;
				case XmlTypeCode.String:
					return XQueryConvert.StringToDateTime (stringValue);
				case XmlTypeCode.None:
				case XmlTypeCode.Item:
				case XmlTypeCode.AnyAtomicType:
					if (objectValue is DateTime)
						return (DateTime) objectValue;
					break;

				}

				throw new InvalidOperationException (String.Format ("Conversion from {0} to {1} is not supported", schemaType, XmlTypeCode.DateTime));
			}
		}

		[MonoTODO]
		public override decimal ValueAsDecimal {
			get {
				switch (xmlTypeCode) {
				case XmlTypeCode.Boolean:
					return XQueryConvert.BooleanToDecimal (booleanValue);
				case XmlTypeCode.Decimal:
					return decimalValue;
				case XmlTypeCode.Double:
					return XQueryConvert.DoubleToDecimal (doubleValue);
				case XmlTypeCode.Long:
					return XQueryConvert.IntegerToDecimal (longValue);
				case XmlTypeCode.Int:
					return XQueryConvert.IntToDecimal (intValue);
				case XmlTypeCode.Float:
					return XQueryConvert.FloatToDecimal (floatValue);
				case XmlTypeCode.String:
					return XQueryConvert.StringToDecimal (stringValue);
				case XmlTypeCode.None:
				case XmlTypeCode.Item:
				case XmlTypeCode.AnyAtomicType:
					if (objectValue is decimal)
						return (decimal) objectValue;
					break;

				}

				throw new InvalidOperationException (String.Format ("Conversion from {0} to {1} is not supported", schemaType, XmlTypeCode.Decimal));
			}
		}

		[MonoTODO]
		public override double ValueAsDouble {
			get {
				switch (xmlTypeCode) {
				case XmlTypeCode.Boolean:
					return XQueryConvert.BooleanToDouble (booleanValue);
				case XmlTypeCode.Decimal:
					return XQueryConvert.DecimalToDouble (decimalValue);
				case XmlTypeCode.Double:
					return doubleValue;
				case XmlTypeCode.Long:
					return XQueryConvert.IntegerToDouble (longValue);
				case XmlTypeCode.Int:
					return XQueryConvert.IntToDouble (intValue);
				case XmlTypeCode.Float:
					return XQueryConvert.FloatToDouble (floatValue);
				case XmlTypeCode.String:
					return XQueryConvert.StringToDouble (stringValue);
				case XmlTypeCode.None:
				case XmlTypeCode.Item:
				case XmlTypeCode.AnyAtomicType:
					if (objectValue is double)
						return (double) objectValue;
					break;

				}

				throw new InvalidOperationException (String.Format ("Conversion from {0} to {1} is not supported", schemaType, XmlTypeCode.Double));
			}
		}

		[MonoTODO]
		public override int ValueAsInt32 {
			get {
				switch (xmlTypeCode) {
				case XmlTypeCode.Boolean:
					return XQueryConvert.BooleanToInt (booleanValue);
				case XmlTypeCode.Decimal:
					return XQueryConvert.DecimalToInt (decimalValue);
				case XmlTypeCode.Double:
					return XQueryConvert.DoubleToInt (doubleValue);
				case XmlTypeCode.Long:
					return XQueryConvert.IntegerToInt (longValue);
				case XmlTypeCode.Int:
					return intValue;
				case XmlTypeCode.Float:
					return XQueryConvert.FloatToInt (floatValue);
				case XmlTypeCode.String:
					return XQueryConvert.StringToInt (stringValue);
				case XmlTypeCode.None:
				case XmlTypeCode.Item:
				case XmlTypeCode.AnyAtomicType:
					if (objectValue is int)
						return (int) objectValue;
					break;

				}

				throw new InvalidOperationException (String.Format ("Conversion from {0} to {1} is not supported", schemaType, XmlTypeCode.Int));
			}
		}

		[MonoTODO]
		public override long ValueAsInt64 {
			get {
				switch (xmlTypeCode) {
				case XmlTypeCode.Boolean:
					return XQueryConvert.BooleanToInteger (booleanValue);
				case XmlTypeCode.Decimal:
					return XQueryConvert.DecimalToInteger (decimalValue);
				case XmlTypeCode.Double:
					return XQueryConvert.DoubleToInteger (doubleValue);
				case XmlTypeCode.Long:
					return longValue;
				case XmlTypeCode.Int:
					return XQueryConvert.IntegerToInt (intValue);
				case XmlTypeCode.Float:
					return XQueryConvert.FloatToInteger (floatValue);
				case XmlTypeCode.String:
					return XQueryConvert.StringToInteger (stringValue);
				case XmlTypeCode.None:
				case XmlTypeCode.Item:
				case XmlTypeCode.AnyAtomicType:
					if (objectValue is long)
						return (long) objectValue;
					break;

				}

				throw new InvalidOperationException (String.Format ("Conversion from {0} to {1} is not supported", schemaType, XmlTypeCode.Long));
			}
		}

		[MonoTODO]
		public override float ValueAsSingle {
			get {
				switch (xmlTypeCode) {
				case XmlTypeCode.Boolean:
					return XQueryConvert.BooleanToFloat (booleanValue);
				case XmlTypeCode.Decimal:
					return XQueryConvert.DecimalToFloat (decimalValue);
				case XmlTypeCode.Double:
					return XQueryConvert.DoubleToFloat (doubleValue);
				case XmlTypeCode.Float:
					return floatValue;
				case XmlTypeCode.Int:
					return XQueryConvert.FloatToInt (intValue);
				case XmlTypeCode.Long:
					return XQueryConvert.IntegerToFloat (longValue);
				case XmlTypeCode.String:
					return XQueryConvert.StringToFloat (stringValue);
				case XmlTypeCode.None:
				case XmlTypeCode.Item:
				case XmlTypeCode.AnyAtomicType:
					if (objectValue is float)
						return (float) objectValue;
					break;

				}

				throw new InvalidOperationException (String.Format ("Conversion from {0} to {1} is not supported", schemaType, XmlTypeCode.Float));
			}
		}

		[MonoTODO]
		public override ICollection ValueAsList {
			get {
				if (valueAsList != null)
					return valueAsList;
				if (objectValue is ICollection)
					valueAsList = objectValue as ICollection;
				else if (objectValue is Array)
					valueAsList = new ArrayList ((Array) objectValue);
				else if (xmlTypeCode != XmlTypeCode.None) {
					ArrayList al = new ArrayList ();
					al.Add (TypedValue);
					valueAsList = al;
				}
				else
					throw new NotImplementedException ();
				return valueAsList;
			}
		}

		[MonoTODO]
		public override Type ValueType {
			get { return schemaType.Datatype.ValueType; }
		}

		[MonoTODO]
		public override XmlSchemaType XmlType {
			get { return schemaType; }
		}

		#endregion

		#region internal static members

		internal static Type RuntimeTypeFromXmlTypeCode (XmlTypeCode typeCode)
		{
			switch (typeCode) {
			case XmlTypeCode.Int:
				return typeof (int);
			case XmlTypeCode.Decimal:
				return typeof (decimal);
			case XmlTypeCode.Double:
				return typeof (double);
			case XmlTypeCode.Float:
				return typeof (float);
			case XmlTypeCode.Long:
				return typeof (long);
			case XmlTypeCode.Short:
				return typeof (short);
			case XmlTypeCode.UnsignedShort:
				return typeof (ushort);
			case XmlTypeCode.UnsignedInt:
				return typeof (uint);
			case XmlTypeCode.String:
				return typeof (string);
			case XmlTypeCode.DateTime:
				return typeof (DateTime);
			case XmlTypeCode.Boolean:
				return typeof (bool);
			case XmlTypeCode.Item:
				return typeof (object);
			}
			throw new NotSupportedException (String.Format ("XQuery internal error: Cannot infer Runtime Type from XmlTypeCode {0}.", typeCode));
		}

		internal static XmlTypeCode XmlTypeCodeFromRuntimeType (Type cliType, bool raiseError)
		{
			switch (Type.GetTypeCode (cliType)) {
			case TypeCode.Int32:
				return XmlTypeCode.Int;
			case TypeCode.Decimal:
				return XmlTypeCode.Decimal;
			case TypeCode.Double:
				return XmlTypeCode.Double;
			case TypeCode.Single:
				return XmlTypeCode.Float;
			case TypeCode.Int64:
				return XmlTypeCode.Long;
			case TypeCode.Int16:
				return XmlTypeCode.Short;
			case TypeCode.UInt16:
				return XmlTypeCode.UnsignedShort;
			case TypeCode.UInt32:
				return XmlTypeCode.UnsignedInt;
			case TypeCode.String:
				return XmlTypeCode.String;
			case TypeCode.DateTime:
				return XmlTypeCode.DateTime;
			case TypeCode.Boolean:
				return XmlTypeCode.Boolean;
			case TypeCode.Object:
				return XmlTypeCode.Item;
			}
			if (raiseError)
				throw new NotSupportedException (String.Format ("XQuery internal error: Cannot infer XmlTypeCode from Runtime Type {0}", cliType));
			else
				return XmlTypeCode.None;
		}
		#endregion
	}
}

#endif
