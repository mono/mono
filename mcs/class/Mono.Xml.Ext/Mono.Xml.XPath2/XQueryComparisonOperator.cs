//
// XQueryComparisonOperator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Query;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.XPath2
{
	// FIXME: Handle complete type promotion and subtype substitution.
	// See XQuery 1.0 Appendix B.*.
	public class XQueryComparisonOperator
	{
		private static bool OpBooleanLessThan (bool b1, bool b2)
		{
			return !b1 && b2;
		}

		private static bool OpBooleanGreaterThan (bool b1, bool b2)
		{
			return b1 && !b2;
		}

		private static bool CompareEquality (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			switch (lvalue.XmlType.TypeCode) {
			case XmlTypeCode.Boolean:
				return lvalue.ValueAsBoolean == rvalue.ValueAsBoolean;
			case XmlTypeCode.UntypedAtomic:
			case XmlTypeCode.String:
				return lvalue.Value == rvalue.Value;
			case XmlTypeCode.Date:
			case XmlTypeCode.Time:
			case XmlTypeCode.DateTime:
			case XmlTypeCode.YearMonthDuration:
			case XmlTypeCode.DayTimeDuration:
				return lvalue.ValueAsDateTime == rvalue.ValueAsDateTime;
			case XmlTypeCode.HexBinary:
			case XmlTypeCode.Base64Binary:
			case XmlTypeCode.AnyUri:
			case XmlTypeCode.QName:
			case XmlTypeCode.Notation:
				throw new NotImplementedException ();
			}
			XmlQualifiedName nameL = lvalue.XmlType.QualifiedName != XmlQualifiedName.Empty ? lvalue.XmlType.QualifiedName : new XmlQualifiedName ("anyType", XmlSchema.Namespace);
			XmlQualifiedName nameR = rvalue.XmlType.QualifiedName != XmlQualifiedName.Empty ? rvalue.XmlType.QualifiedName : new XmlQualifiedName ("anyType", XmlSchema.Namespace);
			throw new XmlQueryException (String.Format ("Not allowed value comparison between {0} and {1}.", nameL, nameR));
		}

		public static bool ValueEQ (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			if (lvalue.XmlType.TypeCode == XmlTypeCode.Decimal &&
				rvalue.XmlType.TypeCode == XmlTypeCode.Decimal)
				return lvalue.ValueAsDecimal == rvalue.ValueAsDecimal;
			if (SequenceType.IsNumeric (lvalue.XmlType.TypeCode) &&
				SequenceType.IsNumeric (lvalue.XmlType.TypeCode))
				return lvalue.ValueAsDouble == rvalue.ValueAsDouble;
			if (lvalue.XmlType.TypeCode == rvalue.XmlType.TypeCode)
				return CompareEquality (lvalue, rvalue);

			throw new XmlQueryException (String.Format ("Not allowed value comparison between {0} and {1}.", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		public static bool ValueNE (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			if (lvalue.XmlType.TypeCode == XmlTypeCode.Decimal &&
				rvalue.XmlType.TypeCode == XmlTypeCode.Decimal)
				return lvalue.ValueAsDecimal != rvalue.ValueAsDecimal;
			if (SequenceType.IsNumeric (lvalue.XmlType.TypeCode) &&
				SequenceType.IsNumeric (lvalue.XmlType.TypeCode))
				return lvalue.ValueAsDouble != rvalue.ValueAsDouble;
			if (lvalue.XmlType.TypeCode == rvalue.XmlType.TypeCode)
				return !CompareEquality (lvalue, rvalue);

			throw new XmlQueryException (String.Format ("Not allowed value comparison between {0} and {1}.", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		private static bool CompareLT (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			switch (lvalue.XmlType.TypeCode) {
			case XmlTypeCode.Boolean:
				return OpBooleanLessThan (lvalue.ValueAsBoolean, rvalue.ValueAsBoolean);
			case XmlTypeCode.String:
				return lvalue.Value == rvalue.Value;
			case XmlTypeCode.Date:
			case XmlTypeCode.Time:
			case XmlTypeCode.DateTime:
			case XmlTypeCode.YearMonthDuration:
			case XmlTypeCode.DayTimeDuration:
				return lvalue.ValueAsDateTime < rvalue.ValueAsDateTime;
			}
			throw new XmlQueryException (String.Format ("Not allowed value comparison between {0} and {1}.", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		public static bool ValueLT (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			if (lvalue.XmlType.TypeCode == XmlTypeCode.Decimal &&
				rvalue.XmlType.TypeCode == XmlTypeCode.Decimal)
				return lvalue.ValueAsDecimal < rvalue.ValueAsDecimal;
			if (SequenceType.IsNumeric (lvalue.XmlType.TypeCode) &&
				SequenceType.IsNumeric (lvalue.XmlType.TypeCode))
				return lvalue.ValueAsDouble < rvalue.ValueAsDouble;
			if (lvalue.XmlType.TypeCode == rvalue.XmlType.TypeCode)
				return CompareLT (lvalue, rvalue);

			throw new XmlQueryException (String.Format ("Not allowed value comparison between {0} and {1}.", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		private static bool CompareLE (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			switch (lvalue.XmlType.TypeCode) {
			case XmlTypeCode.Boolean:
				return !OpBooleanGreaterThan (lvalue.ValueAsBoolean, rvalue.ValueAsBoolean);
			case XmlTypeCode.String:
				return lvalue.Value == rvalue.Value;
			case XmlTypeCode.Date:
			case XmlTypeCode.Time:
			case XmlTypeCode.DateTime:
			case XmlTypeCode.YearMonthDuration:
			case XmlTypeCode.DayTimeDuration:
				return lvalue.ValueAsDateTime <= rvalue.ValueAsDateTime;
			}
			throw new XmlQueryException (String.Format ("Not allowed value comparison between {0} and {1}.", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		public static bool ValueLE (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			if (lvalue.XmlType.TypeCode == XmlTypeCode.Decimal &&
				rvalue.XmlType.TypeCode == XmlTypeCode.Decimal)
				return lvalue.ValueAsDecimal <= rvalue.ValueAsDecimal;
			if (SequenceType.IsNumeric (lvalue.XmlType.TypeCode) &&
				SequenceType.IsNumeric (lvalue.XmlType.TypeCode))
				return lvalue.ValueAsDouble <= rvalue.ValueAsDouble;
			if (lvalue.XmlType.TypeCode == rvalue.XmlType.TypeCode)
				return CompareLE (lvalue, rvalue);

			throw new XmlQueryException (String.Format ("Not allowed value comparison between {0} and {1}.", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		private static bool CompareGT (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			switch (lvalue.XmlType.TypeCode) {
			case XmlTypeCode.Boolean:
				return OpBooleanGreaterThan (lvalue.ValueAsBoolean, rvalue.ValueAsBoolean);
			case XmlTypeCode.String:
				return lvalue.Value == rvalue.Value;
			case XmlTypeCode.Date:
			case XmlTypeCode.Time:
			case XmlTypeCode.DateTime:
			case XmlTypeCode.YearMonthDuration:
			case XmlTypeCode.DayTimeDuration:
				return lvalue.ValueAsDateTime > rvalue.ValueAsDateTime;
			}
			throw new XmlQueryException (String.Format ("Not allowed value comparison between {0} and {1}.", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		public static bool ValueGT (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			if (lvalue.XmlType.TypeCode == XmlTypeCode.Decimal &&
				rvalue.XmlType.TypeCode == XmlTypeCode.Decimal)
				return lvalue.ValueAsDecimal > rvalue.ValueAsDecimal;
			if (SequenceType.IsNumeric (lvalue.XmlType.TypeCode) &&
				SequenceType.IsNumeric (lvalue.XmlType.TypeCode))
				return lvalue.ValueAsDouble > rvalue.ValueAsDouble;
			if (lvalue.XmlType.TypeCode == rvalue.XmlType.TypeCode)
				return CompareGT (lvalue, rvalue);

			throw new XmlQueryException (String.Format ("Not allowed value comparison between {0} and {1}.", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		private static bool CompareGE (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			switch (lvalue.XmlType.TypeCode) {
			case XmlTypeCode.Boolean:
				return !OpBooleanLessThan (lvalue.ValueAsBoolean, rvalue.ValueAsBoolean);
			case XmlTypeCode.String:
				return lvalue.Value == rvalue.Value;
			case XmlTypeCode.Date:
			case XmlTypeCode.Time:
			case XmlTypeCode.DateTime:
			case XmlTypeCode.YearMonthDuration:
			case XmlTypeCode.DayTimeDuration:
				return lvalue.ValueAsDateTime >= rvalue.ValueAsDateTime;
			}
			throw new XmlQueryException (String.Format ("Not allowed value comparison between {0} and {1}.", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		public static bool ValueGE (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			if (lvalue.XmlType.TypeCode == XmlTypeCode.Decimal &&
				rvalue.XmlType.TypeCode == XmlTypeCode.Decimal)
				return lvalue.ValueAsDecimal >= rvalue.ValueAsDecimal;
			if (SequenceType.IsNumeric (lvalue.XmlType.TypeCode) &&
				SequenceType.IsNumeric (lvalue.XmlType.TypeCode))
				return lvalue.ValueAsDouble >= rvalue.ValueAsDouble;
			if (lvalue.XmlType.TypeCode == rvalue.XmlType.TypeCode)
				return CompareGE (lvalue, rvalue);

			throw new XmlQueryException (String.Format ("Not allowed value comparison between {0} and {1}.", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}
	}
}
#endif
