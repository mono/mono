//
// System.Runtime.Serialization.IFormatterConverter.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
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

namespace System.Runtime.Serialization {
	[CLSCompliant(false)]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisibleAttribute (true)]
#endif
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
