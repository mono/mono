//
// System.IConvertible.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Functions Implementing this interface should check out 
// System.Convert. Most of these methods are implemented 
// there for all these data types.
//
// System.Convert has ToType helper method for the object 
// ToType (Type conversionType, IFormatProvider provider)
// method. In most cases you can specify your ToType function
// as calling 
//
// public Type value; // value of this data type
// public object ToType(Type conversionType, IFormatProvider provider) {
//    Convert.ToType (value, conversionType, provider);
// } 
// 
// Which is just a wrapper for your ToType methods.
//
// See http://lists.ximian.com/archives/public/mono-list/2001-July/000525.html
// for more discussion on the topic
//

namespace System {

    public interface IConvertible {
	
	TypeCode GetTypeCode ();
	
	bool     ToBoolean  (IFormatProvider provider);
	byte     ToByte     (IFormatProvider provider);
	char     ToChar     (IFormatProvider provider);
	DateTime ToDateTime (IFormatProvider provider);
	decimal  ToDecimal  (IFormatProvider provider);
	double   ToDouble   (IFormatProvider provider);
	short    ToInt16    (IFormatProvider provider);
	int      ToInt32    (IFormatProvider provider);
	long     ToInt64    (IFormatProvider provider);
	sbyte    ToSByte    (IFormatProvider provider);
	float    ToSingle   (IFormatProvider provider);
	string   ToString   (IFormatProvider provider);
	object   ToType     (Type conversionType, IFormatProvider provider);
	ushort   ToUInt16   (IFormatProvider provider);
	uint     ToUInt32   (IFormatProvider provider);
	ulong    ToUInt64   (IFormatProvider provider);
    }
}
