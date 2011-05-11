//
// System.Data.SqlDbType.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
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

using System;

namespace System.Data
{
	/// <summary>
	/// Specifies SQL Server data types.
	/// </summary>
#if !NET_2_0
	[Serializable]
#endif
	public enum SqlDbType
	{
		BigInt = 0,
		Binary = 1,
		Bit = 2,
		Char = 3,
		DateTime = 4,
		Decimal = 5,
		Float = 6,
		Image = 7,
		Int = 8,
		Money = 9,
		NChar = 10,
		NText = 11,
		NVarChar = 12,
		Real = 13,
		UniqueIdentifier = 14,
		SmallDateTime = 15,
		SmallInt = 16,
		SmallMoney = 17,
		Text = 18,
		Timestamp = 19,
		TinyInt = 20,
		VarBinary = 21,
		VarChar = 22,
		Variant = 23,
#if NET_2_0
		Xml = 25,
		Udt = 29,
		Date = 31,
		Time = 32,
		DateTime2 = 33,
		DateTimeOffset = 34
#endif
	}
}
