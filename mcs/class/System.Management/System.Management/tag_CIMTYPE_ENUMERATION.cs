//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
//

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
namespace System.Management
{
	internal enum tag_CIMTYPE_ENUMERATION
	{
		CIM_EMPTY = 0,
		CIM_SINT16 = 2,
		CIM_SINT32 = 3,
		CIM_REAL32 = 4,
		CIM_REAL64 = 5,
		CIM_STRING = 8,
		CIM_BOOLEAN = 11,
		CIM_OBJECT = 13,
		CIM_SINT8 = 16,
		CIM_UINT8 = 17,
		CIM_UINT16 = 18,
		CIM_UINT32 = 19,
		CIM_SINT64 = 20,
		CIM_UINT64 = 21,
		CIM_DATETIME = 101,
		CIM_REFERENCE = 102,
		CIM_CHAR16 = 103,
		CIM_ILLEGAL = 4095,
		CIM_FLAG_ARRAY = 8192
	}
}