//
// XmlTypeCode.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// Note:
// Precisely to say, these types are based XPath Data Model, but Microsoft
// guys don't know how to make precise name.
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

namespace System.Xml.Schema
{
	public enum XmlTypeCode
	{
		AnyAtomicType, // xdt
		AnyItem, // xpath misc
		AnyNode, // node
		AnySimpleType,
		AnyType,
		AnyUri,
		Attribute, // node
		Base64Binary,
		Boolean,
		Byte,
		Comment, // node
		Date,
		DateTime,
		DayTimeDuration, // xdt
		Decimal,
		Document, // node
		Double,
		Duration,
		Element, // node
		// Entities is not primitive
		Entity,
		Float,
		GDay,
		GMonth,
		GMonthDay,
		GYear,
		GYearMonth,
		HexBinary,
		Id,
		Idref,
		// Idrefs is not primitive
		Int,
		Integer,
		Language,
		Long,
		Name,
		NCName,
		NegativeInteger,
		NmToken,
		// NmTokens is not primitive
		None,
		NonNegativeInteger,
		NonPositiveInteger,
		Normalizedstring,
		Notation,
		// there seems "ocument" enumeration in MS.NET, but it must be a bug
		PositiveInteger,
		ProcessingInstruction, // node
		QName,
		Short,
		String,
		Time,
		Token,
		UnsignedByte,
		UnsignedInt,
		UnsignedLong,
		UnsignedShort,
		UntypedAtomic, // xdt
		YearMonthDuration // xdt
	}
}
#endif
