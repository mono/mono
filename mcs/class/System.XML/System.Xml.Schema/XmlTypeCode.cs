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
