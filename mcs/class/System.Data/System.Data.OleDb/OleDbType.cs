//
// System.Data.OleDb.OleDbType
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
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

using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public enum OleDbType {
		BigInt = 20,
		Binary = 128,
		Boolean = 11,
		BSTR = 8,
		Char = 129,
		Currency = 6,
		Date = 7,
		DBDate = 133,
		DBTime = 134,
		DBTimeStamp = 135,
		Decimal = 14,
		Double = 5,
		Empty = 0,
		Error = 10,
		Filetime = 64,
		Guid = 72,
		IDispatch = 9,
		Integer = 3,
		IUnknown = 13,
		LongVarBinary = 205,
		LongVarChar = 201,
		LongVarWChar = 203,
		Numeric = 131,
		PropVariant = 138,
		Single = 4,
		SmallInt = 2,
		TinyInt = 16,
		UnsignedBigInt = 21,
		UnsignedInt = 19,
		UnsignedSmallInt = 18,
		UnsignedTinyInt = 17,
		VarBinary = 204,
		VarChar = 200,
		Variant = 12,
		VarNumeric = 139,
		VarWChar = 202,
		WChar = 130
	}
}
