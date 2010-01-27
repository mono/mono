//
// XmlAtomicValue.cs
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
using System.Xml.XPath;

namespace System.Xml.Schema
{
	[MonoTODO] // This class is unused and thus won't be finished.
	public sealed class XmlAtomicValue : XPathItem, ICloneable
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
		//ICollection valueAsList;

		#region Constructors

		internal XmlAtomicValue (bool value, XmlSchemaType xmlType)
		{
			Init (value, xmlType);
		}
		
		private void Init (bool value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Boolean;
			this.booleanValue = value;
			schemaType = xmlType;
		}

		internal XmlAtomicValue (DateTime value, XmlSchemaType xmlType)
		{
			Init (value, xmlType);
		}
		
		private void Init (DateTime value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.DateTime;
			this.dateTimeValue = value;
			schemaType = xmlType;
		}

		internal XmlAtomicValue (decimal value, XmlSchemaType xmlType)
		{
			Init (value, xmlType);
		}
		
		private void Init (decimal value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Decimal;
			this.decimalValue = value;
			schemaType = xmlType;
		}

		internal XmlAtomicValue (double value, XmlSchemaType xmlType)
		{
			Init (value, xmlType);
		}
		
		private void Init (double value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Double;
			this.doubleValue = value;
			schemaType = xmlType;
		}

		internal XmlAtomicValue (int value, XmlSchemaType xmlType)
		{
			Init (value, xmlType);
		}
		
		private void Init (int value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Int;
			this.intValue = value;
			schemaType = xmlType;
		}

		internal XmlAtomicValue (long value, XmlSchemaType xmlType)
		{
			Init (value, xmlType);
		}
		
		private void Init (long value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Long;
			this.longValue = value;
			schemaType = xmlType;
		}

		internal XmlAtomicValue (float value, XmlSchemaType xmlType)
		{
			Init (value, xmlType);
		}
		
		private void Init (float value, XmlSchemaType xmlType)
		{
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.Float;
			this.floatValue = value;
			schemaType = xmlType;
		}

		internal XmlAtomicValue (string value, XmlSchemaType xmlType)
		{
			Init (value, xmlType);
		}
		
		private void Init (string value, XmlSchemaType xmlType)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");
			xmlTypeCode = XmlTypeCode.String;
			this.stringValue = value;
			schemaType = xmlType;
		}

		internal XmlAtomicValue (object value, XmlSchemaType xmlType)
		{
			Init (value, xmlType);
		}

		private void Init (object value, XmlSchemaType xmlType)
		{
			// It accepts any kind of object, but will be rejected on each value properties.
			if (value == null)
				throw new ArgumentNullException ("value");
			if (xmlType == null)
				throw new ArgumentNullException ("xmlType");

			switch (Type.GetTypeCode (value.GetType ())) {
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
				Init ((int) value, xmlType);
				return;
			case TypeCode.Decimal:
				Init ((decimal) value, xmlType);
				return;
			case TypeCode.Double:
				Init ((double) value, xmlType);
				return;
			case TypeCode.Single:
				Init ((float) value, xmlType);
				return;
			case TypeCode.Int64:
			case TypeCode.UInt32:
				Init ((long) value, xmlType);
				return;
			case TypeCode.String:
				Init ((string) value, xmlType);
				return;
			case TypeCode.DateTime:
				Init ((DateTime) value, xmlType);
				return;
			case TypeCode.Boolean:
				Init ((bool) value, xmlType);
				return;
			}

			ICollection col = value as ICollection;
			if (col != null && col.Count == 1) {
				if (col is IList)
					Init (((IList) col) [0], xmlType);
				else {
					IEnumerator en = col.GetEnumerator ();
					if (!en.MoveNext ())
						return;
					if (en.Current is DictionaryEntry)
						Init (((DictionaryEntry) en.Current).Value, xmlType);
					else
						Init (en.Current, xmlType);
				}
				return;
			}

			XmlAtomicValue another = value as XmlAtomicValue;
			if (another != null) {
				switch (another.xmlTypeCode) {
				case XmlTypeCode.Boolean:
					Init (another.booleanValue, xmlType);
					return;
				case XmlTypeCode.DateTime:
					Init (another.dateTimeValue, xmlType);
					return;
				case XmlTypeCode.Decimal:
					Init (another.decimalValue, xmlType);
					return;
				case XmlTypeCode.Double:
					Init (another.doubleValue, xmlType);
					return;
				case XmlTypeCode.Int:
					Init (another.intValue, xmlType);
					return;
				case XmlTypeCode.Long:
					Init (another.longValue, xmlType);
					return;
				case XmlTypeCode.Float:
					Init (another.floatValue, xmlType);
					return;
				case XmlTypeCode.String:
					Init (another.stringValue, xmlType);
					return;
				default:
					objectValue = another.objectValue;
					break;
				}
			}

			objectValue = value;
			schemaType = xmlType;
		}

		#endregion

		#region Methods

		object ICloneable.Clone ()
		{
			return this.Clone ();
		}

		public XmlAtomicValue Clone ()
		{
			return new XmlAtomicValue (this, schemaType);
		}

		public override object ValueAs (Type type, IXmlNamespaceResolver nsResolver)
		{
			switch (XmlTypeCodeFromRuntimeType (type, false)) {
			case XmlTypeCode.Int:
			case XmlTypeCode.Short:
			case XmlTypeCode.UnsignedShort:
				return ValueAsInt;
			case XmlTypeCode.Decimal:
				return ValueAsDecimal;
			case XmlTypeCode.Double:
			case XmlTypeCode.Float:
				return ValueAsDouble;
			case XmlTypeCode.Long:
			case XmlTypeCode.UnsignedInt:
				return ValueAsLong;
			case XmlTypeCode.String:
				return Value;
			case XmlTypeCode.DateTime:
				return ValueAsDateTime;
			case XmlTypeCode.Boolean:
				return ValueAsBoolean;
			case XmlTypeCode.Item:
				return TypedValue;
			case XmlTypeCode.QName:
				return XmlQualifiedName.Parse (Value, nsResolver, true);
			}
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return Value;
		}

		#endregion

		#region Properties

		// As long as I tried, neither of such XmlAtomicValue created
		// with XmlText that contains atomic value, XmlElement that
		// contains such XmlText, XmlDocument nor XPathNavigator 
		// created from such nodes returned false. So it won't be 
		// true on this class. Apparently this class needs more
		// documentation.
		public override bool IsNode {
			get { return false; }
		}

		internal XmlTypeCode ResolvedTypeCode {
			get {
				if (schemaType != XmlSchemaComplexType.AnyType)
					return schemaType.TypeCode;
				else
					return xmlTypeCode;
			}
		}

		public override object TypedValue {
			get {
				switch (ResolvedTypeCode) {
				case XmlTypeCode.Boolean:
					return ValueAsBoolean;
				case XmlTypeCode.DateTime:
					return ValueAsDateTime;
				case XmlTypeCode.Decimal:
					return ValueAsDecimal;
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return ValueAsDouble;
				case XmlTypeCode.Long:
					return ValueAsLong;
				case XmlTypeCode.Int:
					return ValueAsInt;
				case XmlTypeCode.String:
					return Value;
				}
				return objectValue;
			}
		}

		// This method works like ValueAsString.
		public override string Value {
			get {
				switch (ResolvedTypeCode) {
				case XmlTypeCode.Boolean:
					stringValue = XQueryConvert.BooleanToString (ValueAsBoolean);
					break;
				case XmlTypeCode.DateTime:
					stringValue = XQueryConvert.DateTimeToString (ValueAsDateTime);
					break;
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					stringValue = XQueryConvert.DoubleToString (ValueAsDouble);
					break;
				case XmlTypeCode.Decimal:
					stringValue = XQueryConvert.DecimalToString (ValueAsDecimal);
					break;
				case XmlTypeCode.NonPositiveInteger:
				case XmlTypeCode.NonNegativeInteger:
				case XmlTypeCode.NegativeInteger:
				case XmlTypeCode.Long:
				case XmlTypeCode.UnsignedLong:
				case XmlTypeCode.PositiveInteger:
					stringValue = XQueryConvert.IntegerToString (ValueAsLong);
					break;
				case XmlTypeCode.Int:
				case XmlTypeCode.Short:
				case XmlTypeCode.Byte:
				case XmlTypeCode.UnsignedInt:
				case XmlTypeCode.UnsignedShort:
				case XmlTypeCode.UnsignedByte:
					stringValue = XQueryConvert.IntToString (ValueAsInt);
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
					throw new InvalidCastException (String.Format ("Conversion from runtime type {0} to {1} is not supported", objectValue.GetType (), XmlTypeCode.String));
				else
					throw new InvalidCastException (String.Format ("Conversion from schema type {0} (type code {1}, resolved type code {2}) to {3} is not supported.", schemaType.QualifiedName, xmlTypeCode, ResolvedTypeCode, XmlTypeCode.String));
			}
		}

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

				throw new InvalidCastException (String.Format ("Conversion from {0} to {1} is not supported", schemaType.QualifiedName, XmlSchemaSimpleType.XsBoolean.QualifiedName));
			}
		}

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

				throw new InvalidCastException (String.Format ("Conversion from {0} to {1} is not supported", schemaType.QualifiedName, XmlSchemaSimpleType.XsDateTime.QualifiedName));
			}
		}

		// Unlike the other ValueAs...() functions, this is not part
		// of the XPathItem abstract class, so it's not an override
		public decimal ValueAsDecimal {
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

				throw new InvalidCastException (String.Format ("Conversion from {0} to {1} is not supported", schemaType.QualifiedName, XmlSchemaSimpleType.XsDecimal.QualifiedName));
			}
		}

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

				throw new InvalidCastException (String.Format ("Conversion from {0} to {1} is not supported", schemaType.QualifiedName, XmlSchemaSimpleType.XsDouble.QualifiedName));
			}
		}

		public override int ValueAsInt {
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

				throw new InvalidCastException (String.Format ("Conversion from {0} to {1} is not supported", schemaType.QualifiedName, XmlSchemaSimpleType.XsInt.QualifiedName));
			}
		}

		public override long ValueAsLong {
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

				throw new InvalidCastException (String.Format ("Conversion from {0} to {1} is not supported", schemaType.QualifiedName, XmlSchemaSimpleType.XsLong.QualifiedName));
			}
		}

		public override Type ValueType {
			get { return schemaType.Datatype.ValueType; }
		}

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
