//
// System.IConvertible.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public interface IConvertible {

		TypeCode GetTypeCode ();

		bool     ToBoolean  (IFormatProvider provider);
		byte     ToByte     (IFormatProvider provider);
		char     ToChar     (IFormatProvider provider);
		DateTime ToDateTime (IFormatProvider provider);
		Decimal  ToDecimal  (IFormatProvider provider);
		Double   ToDouble   (IFormatProvider provider);
		Int16    ToInt16    (IFormatProvider provider);
		Int32    ToInt32    (IFormatProvider provider);
		Int64    ToInt64    (IFormatProvider provider);
		SByte    ToSByte    (IFormatProvider provider);
		float    ToSingle   (IFormatProvider provider);
		string   ToString   (IFormatProvider provider);
		object   ToType     (Type conversionType, IFormatProvider provider);
		UInt16   ToUInt16   (IFormatProvider provider);
		UInt32   ToUInt32   (IFormatProvider provider);
		UInt64   ToUInt64   (IFormatProvider provider);
	}
}
