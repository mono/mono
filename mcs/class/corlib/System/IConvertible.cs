//
// System.IConvertible.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	interface IConvertible {

		public TypeCode GetTypeCode ();

		public bool     ToBoolean  (IFormatProvider provider);
		public byte     ToByte     (IFormatProvider provider);
		public char     ToChar     (IFormatProvider provider);
		public DateTime ToDateType (IFormatProvider provider);
		public Decimal  ToDecimal  (IFormatProvider provider);
		public Double   ToDouble   (IFormatProvider provider);
		public Int16    ToInt16    (IFormatProvider provider);
		public Int32    ToInt32    (IFormatProvider provider);
		public Int32    ToInt64    (IFormatProvider provider);
		public SByte    ToSByte    (IFormatProvider provider);
		public float    ToSingle   (IFormatProvider provider);
		public string   ToString   (IFormatProvider provider);
		public object   ToType     (Type conversionType, IFormatProvider provider);
		public UInt16   ToUInt16   (IFormatProvider provider);
		public UInt32   ToUInt32   (IFormatProvider provider);
		public UInt32   ToUInt64   (IFormatProvider provider);
	}
}
