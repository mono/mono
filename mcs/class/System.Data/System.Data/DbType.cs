//
// System.Data.DbType.cs
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

namespace System.Data
{
	/// <summary>
	/// Gets the data type of a field, a property, or a Parameter object of a .NET data provider.
	/// </summary>
#if !NET_2_0
	[Serializable]
#endif
	public enum DbType
	{
		AnsiString = 0,
		Binary = 1,
		Byte = 2,
		Boolean = 3,
		Currency = 4,
		Date = 5,
		DateTime = 6,
		Decimal = 7,
		Double = 8,
		Guid = 9,
		Int16 = 10,
		Int32 = 11,
		Int64 = 12,
		Object = 13,
		SByte = 14,
		Single = 15,
		String = 16,
		Time = 17,
		UInt16 = 18,
		UInt32 = 19,
		UInt64 = 20,
		VarNumeric = 21,
		AnsiStringFixedLength = 22,
#if NET_2_0
		Xml = 25,
		DateTime2 = 26,
		DateTimeOffset = 27,
#endif
		StringFixedLength = 23
	}
}
