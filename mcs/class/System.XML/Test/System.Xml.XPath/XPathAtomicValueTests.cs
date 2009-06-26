//
// MonoTests.System.Xml.XPathAtomicValueTests.cs
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
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XPathAtomicValueTests
	{
		internal const string XdtNamespace = "http://www.w3.org/2003/11/xpath-datatypes";

		static XmlTypeCode [] AllTypeCode =
			new ArrayList (Enum.GetValues (typeof (XmlTypeCode))).ToArray (typeof (XmlTypeCode)) as XmlTypeCode [];

		static XmlQualifiedName [] allTypeNames;

		static XmlSchemaType [] allTypes;

		static string [] xstypes = new string [] {
			"anyType",
			"anySimpleType",
			"string",
			"boolean",
			"decimal",
			"float",
			"double",
			"duration",
			"dateTime",
			"time",
			"date",
			"gYearMonth",
			"gYear",
			"gMonthDay",
			"gDay",
			"gMonth",
			"hexBinary",
			"base64Binary",
			"anyUri",
			"QName",
			"NOTATION",
			"normalizedString",
			"token",
			"language",
			"NMTOKEN",
			"NMTOKENS",
			"Name",
			"NCName",
			"ID",
			"IDREF",
			"IDREFS",
			"ENTITY",
			"ENTITIES",
			"integer",
			"nonPositiveInteger",
			"negativeInteger",
			"long",
			"int",
			"short",
			"byte",
			"nonNegativeInteger",
			"unsignedLong",
			"unsignedInt",
			"unsignedShort",
			"unsignedByte",
			"positiveInteger"
		};

		static string [] xdttypes = {
			"anyAtomicType",
			"untypedAtomic",
			"yearMonthDuration",
			"dayTimeDuration"
		};

		private static XmlQualifiedName [] AllTypeNames {
			get {
				if (allTypeNames == null) {
					ArrayList al = new ArrayList ();
					foreach (string name in xstypes)
						AddXsType (name, XmlSchema.Namespace, al);
					foreach (string name in xdttypes)
						AddXsType (name, XdtNamespace, al);
					allTypeNames = al.ToArray (typeof (XmlQualifiedName)) as XmlQualifiedName [];
				}
				return allTypeNames;
			}
		}

		private static void AddXsType (string name, string ns, ArrayList al)
		{
			al.Add (new XmlQualifiedName (name, ns));
		}

		private static XmlSchemaType [] AllTypes {
			get {
				if (allTypes == null) {
					ArrayList al = new ArrayList ();
					foreach (XmlQualifiedName name in AllTypeNames) {
						XmlSchemaType t = XmlSchemaType.GetBuiltInSimpleType (name);
						if (t == null)
							t = XmlSchemaType.GetBuiltInComplexType (name);
						al.Add (t);
					}
					allTypes = al.ToArray (typeof (XmlSchemaType)) as XmlSchemaType [];
				}
				return allTypes;
			}
		}

		public void AssertAtomicValue (XPathAtomicValue av,
			bool isNode,
			Type valueType,
			XmlSchemaType xmlType,
			object typedValue,
			Type typedValueType,
			string value,
			object boolValue,
			object dateValue,
			object decimalValue,
			object doubleValue,
			object int32Value,
			object int64Value,
			object singleValue,
			int listCount)
		{
			Assert.AreEqual (isNode, av.IsNode, "IsNode");
			Assert.AreEqual (valueType, av.ValueType, "ValueType");
			Assert.AreEqual (xmlType, av.XmlType, "XmlType");
			Assert.AreEqual (typedValue, av.TypedValue, "TypedValue");
			Assert.AreEqual (typedValueType, typedValue.GetType (), "typedValue.GetType()");

			if (value != null)
				Assert.AreEqual (value, av.Value, "Value");
			else {
				try {
					value = av.Value;
					Assert.Fail ("not supported conversion to String.");
				} catch (InvalidCastException) {
				}
			}

			// FIXME: Failure cases could not be tested;
			// any kind of Exceptions are thrown as yet.
			if (boolValue != null)
				Assert.AreEqual (boolValue, av.ValueAsBoolean, "ValueAsBoolean");
			/*
			else {
				try {
					boolValue = av.ValueAsBoolean;
					Assert.Fail ("not supported conversion to Boolean.");
				} catch (InvalidCastException) {
				}
			}
			*/
			if (dateValue != null)
				Assert.AreEqual (dateValue, av.ValueAsDateTime, "ValueAsDateTime");
			/*
			else {
				try {
					dateValue = av.ValueAsDateTime;
					Assert.Fail ("not supported conversion to DateTime.");
				} catch (InvalidCastException) {
				}
			}
			*/
			if (decimalValue != null)
				Assert.AreEqual (decimalValue, av.ValueAsDecimal, "ValueAsDecimal");
			/*
			else {
				try {
					decimalValue = av.ValueAsDecimal;
					Assert.Fail ("not supported conversion to Decimal.");
				} catch (InvalidCastException) {
				}
			}
			*/
			if (doubleValue != null)
				Assert.AreEqual (doubleValue, av.ValueAsDouble, "ValueAsDouble");
			/*
			else {
				try {
					doubleValue = av.ValueAsDouble;
					Assert.Fail ("not supported conversion to Double.");
				} catch (InvalidCastException) {
				}
			}
			*/
			if (int32Value != null)
				Assert.AreEqual (int32Value, av.ValueAsInt32, "ValueAsInt32");
			/*
			else {
				try {
					int32Value = av.ValueAsInt32;
					Assert.Fail ("not supported conversion to Int32.");
				} catch (InvalidCastException) {
				}
			}
			*/
			if (int64Value != null)
				Assert.AreEqual (int64Value, av.ValueAsInt64, "ValueAsInt64");
			/*
			else {
				try {
					int64Value = av.ValueAsInt64;
					Assert.Fail ("not supported conversion to Int64.");
				} catch (InvalidCastException) {
				}
			}
			*/
			if (singleValue != null)
				Assert.AreEqual (singleValue, av.ValueAsSingle, "ValueAsSingle");
			/*
			else {
				try {
					singleValue = av.ValueAsSingle;
					Assert.Fail ("not supported conversion to Single.");
				} catch (InvalidCastException) {
				}
			}
			*/
			Assert.AreEqual (listCount, av.ValueAsList.Count, "ValueAsList.Count");
		}

		[Test]
		public void BooleanType ()
		{
			XmlSchemaType xstype = XmlSchemaType.GetBuiltInSimpleType (XmlTypeCode.Boolean);

			XPathAtomicValue av;

			// true
			av = new XPathAtomicValue (true, xstype);
			AssertAtomicValue (av,
				false,
				typeof (bool), // ValueType
				xstype, // XmlType
				true, // TypedValue
				typeof (bool), // actual Type of TypedValue
				"true", // string
				true, // bool
				null, // DateTime
				(decimal) 1, // decimal
				1.0, // double
				1, // int32
				1, // int64
				(float) 1.0, // single
				1); // array count

			// false
			av = new XPathAtomicValue (false, xstype);
			AssertAtomicValue (av,
				false,
				typeof (bool), // ValueType
				xstype, // XmlType
				false, // TypedValue
				typeof (bool), // actual Type of TypedValue
				"false", // string
				false, // bool
				null, // DateTime
				(decimal) 0, // decimal
				0.0, // double
				0, // int32
				0, // int64
				(float) 0.0, // single
				1); // array count

			// 0
			av = new XPathAtomicValue (false, xstype);
			AssertAtomicValue (av,
				false,
				typeof (bool), // ValueType
				xstype, // XmlType
				false, // TypedValue
				typeof (bool), // actual Type of TypedValue
				"false", // string
				false, // bool
				null, // DateTime
				(decimal) 0, // decimal
				0.0, // double
				0, // int32
				0, // int64
				(float) 0.0, // single
				1); // array count

			// 5
			av = new XPathAtomicValue (5, xstype);
			AssertAtomicValue (av,
				false,
				typeof (bool), // ValueType
				xstype, // XmlType
				true, // TypedValue
				typeof (bool), // actual Type of TypedValue
				"true", // string
				true, // bool
				null, // DateTime
				(decimal) 5, // decimal
				5.0, // double
				5, // int32
				5, // int64
				(float) 5.0, // single
				1); // array count

			// short
			short shortValue = 3;
			av = new XPathAtomicValue (shortValue, xstype);
			AssertAtomicValue (av,
				false,
				typeof (bool), // ValueType
				xstype, // XmlType
				true, // TypedValue
				typeof (bool), // actual Type of TypedValue
				"true", // string
				true, // bool
				null, // DateTime
				(decimal) 3, // decimal
				3.0, // double
				3, // int32
				3, // int64
				(float) 3.0, // single
				1); // array count

			// "1"
			av = new XPathAtomicValue ("1", xstype);
			AssertAtomicValue (av,
				false,
				typeof (bool), // ValueType
				xstype, // XmlType
				true, // TypedValue
				typeof (bool), // actual Type of TypedValue
				"true", // string
				true, // bool
				null, // DateTime
				(decimal) 1, // decimal
				1.0, // double
				1, // int32
				1, // int64
				(float) 1.0, // single
				1); // array count

			// new bool [] {true}
			av = new XPathAtomicValue (new bool [] {true}, xstype);
			AssertAtomicValue (av,
				false,
				typeof (bool), // ValueType
				xstype, // XmlType
				true, // TypedValue
				typeof (bool), // actual Type of TypedValue
				"true", // string
				true, // bool
				null, // DateTime
				(decimal) 1, // decimal
				1.0, // double
				1, // int32
				1, // int64
				(float) 1.0, // single
				1); // array count

			// new ArrayList (new int [] {6})
			av = new XPathAtomicValue (new ArrayList (new int [] {6}), xstype);
			AssertAtomicValue (av,
				false,
				typeof (bool), // ValueType
				xstype, // XmlType
				true, // TypedValue
				typeof (bool), // actual Type of TypedValue
				"true", // string
				true, // bool
				null, // DateTime
				(decimal) 6, // decimal
				6.0, // double
				6, // int32
				6, // int64
				(float) 6.0, // single
				1); // array count

			// Hashtable, [7] = 7
			Hashtable ht = new Hashtable ();
			ht [7] = 7;
			av = new XPathAtomicValue (ht, xstype);
			AssertAtomicValue (av,
				false,
				typeof (bool), // ValueType
				xstype, // XmlType
				true, // TypedValue
				typeof (bool), // actual Type of TypedValue
				"true", // string
				true, // bool
				null, // DateTime
				(decimal) 7, // decimal
				7.0, // double
				7, // int32
				7, // int64
				(float) 7.0, // single
				1); // array count

			// - MS.NET will fail here due to its bug -

			// another XPathAtomicValue that is bool
			av = new XPathAtomicValue (true, xstype);
			av = new XPathAtomicValue (av, xstype);
			AssertAtomicValue (av,
				false,
				typeof (bool), // ValueType
				xstype, // XmlType
				true, // TypedValue
				typeof (bool), // actual Type of TypedValue
				"true", // string
				true, // bool
				null, // DateTime
				(decimal) 1, // decimal
				1.0, // double
				1, // int32
				1, // int64
				(float) 1.0, // single
				1); // array count

			// Array, [0] = XPathAtomicValue
			av = new XPathAtomicValue (new XPathAtomicValue [] {av}, xstype);
			AssertAtomicValue (av,
				false,
				typeof (bool), // ValueType
				xstype, // XmlType
				true, // TypedValue
				typeof (bool), // actual Type of TypedValue
				"true", // string
				true, // bool
				null, // DateTime
				(decimal) 1, // decimal
				1.0, // double
				1, // int32
				1, // int64
				(float) 1.0, // single
				1); // array count

			// new bool [] {true, false}
			av = new XPathAtomicValue (new bool [] {true, false}, xstype);
			try {
				object o = av.ValueAsBoolean;
				Assert.Fail ("ArrayList must contain just one item to be castable to bool");
			} catch (InvalidCastException) {
			}

			// empty ArrayList
			av = new XPathAtomicValue (new ArrayList (), xstype);
			try {
				object o = av.ValueAsBoolean;
				Assert.Fail ("ArrayList must contain just one item to be castable to bool");
			} catch (InvalidCastException) {
			}

			// "True"
			av = new XPathAtomicValue ("True", xstype);
			try {
				object o = av.ValueAsBoolean;
				Assert.Fail ("\"True\" is not a boolean representation (\"true\" is).");
			} catch (InvalidCastException) {
			}

			// DateTime
			av = new XPathAtomicValue (DateTime.Now, xstype);
			try {
				object o = av.ValueAsBoolean;
				Assert.Fail ("DateTime should not be castable to bool.");
			} catch (InvalidCastException) {
			}

			// XmlText node that contains boolean representation value
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root>true</root>");
			XmlNode node = doc.DocumentElement.FirstChild;
			av = new XPathAtomicValue (node, xstype);
			try {
				object o = av.ValueAsBoolean;
				Assert.Fail ("XmlText cannot be castable to bool.");
			} catch (InvalidCastException) {
			}

			// XPathNavigator whose node points to text node whose 
			// value represents boolean string.
			av = new XPathAtomicValue (node.CreateNavigator (),
				xstype);
			try {
				object o = av.ValueAsBoolean;
				Assert.Fail ("XmlText cannot be castable to bool.");
			} catch (InvalidCastException) {
			}
		}
	}
}
#endif
