//
// XQueryArithmeticOperator.cs
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
	public class XQueryArithmeticOperator
	{
		/// <summary>
		/// x + y
		/// </summary>
		public static XPathAtomicValue Add (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			// numeric, date, time, dateTime, yearMonthDuration, dayTimeDuration
			switch (lvalue.XmlType.TypeCode) {

			// numerics
			case XmlTypeCode.Integer:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
					return new XPathAtomicValue (lvalue.ValueAsInt64 + rvalue.ValueAsInt64, rvalue.XmlType);
				case XmlTypeCode.Decimal:
					return new XPathAtomicValue (lvalue.ValueAsDecimal + rvalue.ValueAsDecimal, rvalue.XmlType);
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble + rvalue.ValueAsDouble, rvalue.XmlType);
				}
				break;

			case XmlTypeCode.Decimal:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
				case XmlTypeCode.Decimal:
					return new XPathAtomicValue (lvalue.ValueAsDecimal + rvalue.ValueAsDecimal, rvalue.XmlType);
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble + rvalue.ValueAsDouble, rvalue.XmlType);
				}
				break;

			case XmlTypeCode.Float:
			case XmlTypeCode.Double:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
				case XmlTypeCode.Decimal:
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble + rvalue.ValueAsDouble, rvalue.XmlType);
				}
				break;

			// datetimes
			case XmlTypeCode.Time:
				if (rvalue.XmlType.TypeCode == XmlTypeCode.DayTimeDuration)
					goto case XmlTypeCode.DateTime;
				break;
			case XmlTypeCode.DateTime:
			case XmlTypeCode.Date:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.YearMonthDuration:
				case XmlTypeCode.DayTimeDuration:
					return new XPathAtomicValue (lvalue.ValueAsDateTime + new TimeSpan (rvalue.ValueAsDateTime.Ticks), lvalue.XmlType);
				}
				break;
			// durations
			case XmlTypeCode.YearMonthDuration:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Date:
				case XmlTypeCode.DateTime:
					return new XPathAtomicValue (lvalue.ValueAsDateTime + new TimeSpan (rvalue.ValueAsDateTime.Ticks), rvalue.XmlType);
				case XmlTypeCode.YearMonthDuration:
					return new XPathAtomicValue (new DateTime (lvalue.ValueAsDateTime.Ticks + rvalue.ValueAsDateTime.Ticks), XmlSchemaSimpleType.XdtYearMonthDuration);
				}
				break;
			case XmlTypeCode.DayTimeDuration:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Date:
				case XmlTypeCode.Time:
				case XmlTypeCode.DateTime:
					return new XPathAtomicValue (lvalue.ValueAsDateTime + new TimeSpan (rvalue.ValueAsDateTime.Ticks), rvalue.XmlType);
				case XmlTypeCode.DayTimeDuration:
					return new XPathAtomicValue (new DateTime (lvalue.ValueAsDateTime.Ticks + rvalue.ValueAsDateTime.Ticks), XmlSchemaSimpleType.XdtDayTimeDuration);
				}
				break;
			}

			throw new XmlQueryException (String.Format ("Not allowed arithmetic operation: {0} + {1}", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		/// <summary>
		/// x - y
		/// </summary>
		public static XPathAtomicValue Subtract (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			// numeric, date, time, dateTime, yearMonthDuration, dayTimeDuration
			switch (lvalue.XmlType.TypeCode) {

			// numerics
			case XmlTypeCode.Integer:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
					return new XPathAtomicValue (lvalue.ValueAsInt64 - rvalue.ValueAsInt64, rvalue.XmlType);
				case XmlTypeCode.Decimal:
					return new XPathAtomicValue (lvalue.ValueAsDecimal - rvalue.ValueAsDecimal, rvalue.XmlType);
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble - rvalue.ValueAsDouble, rvalue.XmlType);
				}
				break;

			case XmlTypeCode.Decimal:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
				case XmlTypeCode.Decimal:
					return new XPathAtomicValue (lvalue.ValueAsDecimal - rvalue.ValueAsDecimal, rvalue.XmlType);
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble - rvalue.ValueAsDouble, rvalue.XmlType);
				}
				break;

			case XmlTypeCode.Float:
			case XmlTypeCode.Double:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
				case XmlTypeCode.Decimal:
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble - rvalue.ValueAsDouble, rvalue.XmlType);
				}
				break;

			// datetimes
			case XmlTypeCode.Time:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Time:
					return new XPathAtomicValue (lvalue.ValueAsDateTime - rvalue.ValueAsDateTime, XmlSchemaSimpleType.XdtDayTimeDuration);
				case XmlTypeCode.DayTimeDuration:
					return new XPathAtomicValue (lvalue.ValueAsDateTime - new TimeSpan (rvalue.ValueAsDateTime.Ticks), lvalue.XmlType);
				}
				break;

			case XmlTypeCode.DateTime:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.DateTime:
					// FIXME: check fn:subtract-daytimes-yielding-dayTimeDuration()
					return new XPathAtomicValue (lvalue.ValueAsDateTime - rvalue.ValueAsDateTime, XmlSchemaSimpleType.XdtDayTimeDuration);
				case XmlTypeCode.YearMonthDuration:
				case XmlTypeCode.DayTimeDuration:
					return new XPathAtomicValue (lvalue.ValueAsDateTime - new TimeSpan (rvalue.ValueAsDateTime.Ticks), lvalue.XmlType);
				}
				break;

			case XmlTypeCode.Date:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Date:
					// FIXME: check fn:subtract-daytimes-yielding-dayTimeDuration()
					return new XPathAtomicValue (lvalue.ValueAsDateTime - rvalue.ValueAsDateTime, XmlSchemaSimpleType.XdtDayTimeDuration);
				case XmlTypeCode.YearMonthDuration:
				case XmlTypeCode.DayTimeDuration:
					return new XPathAtomicValue (lvalue.ValueAsDateTime - new TimeSpan (rvalue.ValueAsDateTime.Ticks), lvalue.XmlType);
				}
				break;

			// durations
			case XmlTypeCode.YearMonthDuration:
				if (rvalue.XmlType.TypeCode == XmlTypeCode.YearMonthDuration)
					return new XPathAtomicValue (new TimeSpan (lvalue.ValueAsDateTime.Ticks - rvalue.ValueAsDateTime.Ticks), XmlSchemaSimpleType.XdtYearMonthDuration);
				break;
			case XmlTypeCode.DayTimeDuration:
				if (rvalue.XmlType.TypeCode == XmlTypeCode.DayTimeDuration)
					return new XPathAtomicValue (new TimeSpan (lvalue.ValueAsDateTime.Ticks - rvalue.ValueAsDateTime.Ticks), XmlSchemaSimpleType.XdtDayTimeDuration);
				break;
			}

			throw new XmlQueryException (String.Format ("Not allowed arithmetic operation: {0} - {1}", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		/// <summary>
		/// x * y
		/// </summary>
		public static XPathAtomicValue Multiply (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			// numeric, date, time, dateTime, yearMonthDuration, dayTimeDuration
			switch (lvalue.XmlType.TypeCode) {

			// numerics
			case XmlTypeCode.Integer:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
					return new XPathAtomicValue (lvalue.ValueAsInt64 * rvalue.ValueAsInt64, rvalue.XmlType);
				case XmlTypeCode.Decimal:
					return new XPathAtomicValue (lvalue.ValueAsDecimal * rvalue.ValueAsDecimal, rvalue.XmlType);
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble * rvalue.ValueAsDouble, rvalue.XmlType);

				case XmlTypeCode.DayTimeDuration:
				case XmlTypeCode.YearMonthDuration:
					goto case XmlTypeCode.Decimal;
				}
				break;

			case XmlTypeCode.Decimal:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
				case XmlTypeCode.Decimal:
					return new XPathAtomicValue (lvalue.ValueAsDecimal * rvalue.ValueAsDecimal, rvalue.XmlType);
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble * rvalue.ValueAsDouble, rvalue.XmlType);

				case XmlTypeCode.YearMonthDuration:
				case XmlTypeCode.DayTimeDuration:
					return new XPathAtomicValue (new TimeSpan ((long) (lvalue.ValueAsDateTime.Ticks * rvalue.ValueAsDecimal)), rvalue.XmlType);
				}
				break;

			case XmlTypeCode.Float:
			case XmlTypeCode.Double:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
				case XmlTypeCode.Decimal:
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble * rvalue.ValueAsDouble, rvalue.XmlType);

				case XmlTypeCode.DayTimeDuration:
				case XmlTypeCode.YearMonthDuration:
					goto case XmlTypeCode.Decimal;
				}
				break;

			// durations
			case XmlTypeCode.DayTimeDuration:
			case XmlTypeCode.YearMonthDuration:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
				case XmlTypeCode.Decimal:
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return Multiply (rvalue, lvalue);
				}
				break;
			}

			throw new XmlQueryException (String.Format ("Not allowed arithmetic operation: {0} * {1}", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		/// <summary>
		/// x / y
		/// </summary>
		public static XPathAtomicValue Divide (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			// numeric, date, time, dateTime, yearMonthDuration, dayTimeDuration
			switch (lvalue.XmlType.TypeCode) {

			// numerics
			case XmlTypeCode.Integer:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
					return new XPathAtomicValue (lvalue.ValueAsInt64 / rvalue.ValueAsInt64, rvalue.XmlType);
				case XmlTypeCode.Decimal:
					return new XPathAtomicValue (lvalue.ValueAsDecimal / rvalue.ValueAsDecimal, rvalue.XmlType);
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble / rvalue.ValueAsDouble, rvalue.XmlType);
				}
				break;

			case XmlTypeCode.Decimal:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
				case XmlTypeCode.Decimal:
					return new XPathAtomicValue (lvalue.ValueAsDecimal / rvalue.ValueAsDecimal, rvalue.XmlType);
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble / rvalue.ValueAsDouble, rvalue.XmlType);
				}
				break;

			case XmlTypeCode.Float:
			case XmlTypeCode.Double:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
				case XmlTypeCode.Decimal:
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (lvalue.ValueAsDouble / rvalue.ValueAsDouble, rvalue.XmlType);

				case XmlTypeCode.DayTimeDuration:
				case XmlTypeCode.YearMonthDuration:
					goto case XmlTypeCode.Decimal;
				}
				break;

			// durations
			case XmlTypeCode.DayTimeDuration:
			case XmlTypeCode.YearMonthDuration:
				switch (rvalue.XmlType.TypeCode) {
				case XmlTypeCode.Integer:
				case XmlTypeCode.Decimal:
				case XmlTypeCode.Float:
				case XmlTypeCode.Double:
					return new XPathAtomicValue (new DateTime ((long) (lvalue.ValueAsDateTime.Ticks / rvalue.ValueAsDouble)), rvalue.XmlType);
				}
				break;
			}

			throw new XmlQueryException (String.Format ("Not allowed arithmetic operation: {0} div {1}", lvalue.XmlType.QualifiedName, rvalue.XmlType.QualifiedName));
		}

		/// <summary>
		/// x idiv y
		/// </summary>
		public static XPathAtomicValue IntDivide (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			return new XPathAtomicValue (lvalue.ValueAsInt64 / rvalue.ValueAsInt64, XmlSchemaSimpleType.XsInteger);
		}

		/// <summary>
		/// x imod y
		/// </summary>
		public static XPathAtomicValue Remainder (XPathAtomicValue lvalue, XPathAtomicValue rvalue)
		{
			return new XPathAtomicValue (lvalue.ValueAsInt64 % rvalue.ValueAsInt64, XmlSchemaSimpleType.XsInteger);
		}
	}
}
#endif
