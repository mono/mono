//
// System.Runtime.Serialization.IFormatterConverter.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

namespace System.Runtime.Serialization {
	[CLSCompliant(false)]
	public interface IFormatterConverter {
		object Convert (object o, Type t);
		object Convert (object o, TypeCode tc);
		
		bool        ToBoolean  (object o);
		byte        ToByte     (object o);
		char        ToChar     (object o);
		DateTime    ToDateTime (object o);
		Decimal     ToDecimal  (object o);
		double      ToDouble   (object o);
		Int16       ToInt16    (object o);
		Int32       ToInt32    (object o);
		Int64       ToInt64    (object o);
		sbyte       ToSByte    (object o);
		float       ToSingle   (object o);
		string      ToString   (object o);
		UInt16      ToUInt16   (object o);
		UInt32      ToUInt32   (object o);
		UInt64      ToUInt64   (object o);
	}
}
