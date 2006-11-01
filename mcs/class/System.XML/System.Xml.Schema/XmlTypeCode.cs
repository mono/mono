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
		None,
		Item,
		Node,
		Document, // node
		Element, // node
		Attribute, // node
		Namespace, // node
		ProcessingInstruction, // node
		Comment, // node
		Text,	// node
		AnyAtomicType, // xdt
		UntypedAtomic, // xdt
		String,
		Boolean,
		Decimal,
		Float,
		Double,
		Duration,
		DateTime,
		Time,
		Date,
		GYearMonth,
		GYear,
		GMonthDay,
		GDay,
		GMonth,
		HexBinary,
		Base64Binary,
		AnyUri,
		QName,
		Notation,
		NormalizedString,
		Token,
		Language,
		NmToken, // NmTokens is not primitive
		Name,
		NCName,
		Id,
		Idref, // Idrefs is not primitive
		Entity, // Entities is not primitive
		Integer,
		NonPositiveInteger,
		NegativeInteger,
		Long,
		Int,
		Short,
		Byte,
		NonNegativeInteger,
		UnsignedLong,
		UnsignedInt,
		UnsignedShort,
		UnsignedByte,
		PositiveInteger,
		YearMonthDuration, // xdt
		DayTimeDuration, // xdt
	}
}
#endif
