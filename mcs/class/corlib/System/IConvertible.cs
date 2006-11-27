//
// System.IConvertible.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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
using System.Runtime.InteropServices;

namespace System {

#if NET_2_0
    [ComVisible(true)]
#endif
    [CLSCompliant(false)]
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
