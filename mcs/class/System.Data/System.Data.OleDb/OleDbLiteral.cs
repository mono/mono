//
// System.Data.OleDb.OleDbLiteral
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// Copyright (C) Rodrigo Moya, 2002
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
	public enum OleDbLiteral {
		Binary_Literal = 1,
		Catalog_Name = 2,
		Catalog_Separator = 3,
		Char_Literal = 4,
		Column_Alias = 5,
		Column_Name = 6,
		Correlation_Name = 7,
		Cube_Name = 21,
		Cursor_Name = 8,
		Dimension_Name = 22,
		Escape_Percent_Prefix = 9,
		Escape_Percent_Suffix = 29,
		Escape_Underscore_Prefix = 10,
		Escape_Underscore_Suffix = 30,
		Hierarchy_Name = 23,
		Index_Name = 11,
		Invalid = 0,
		Level_Name = 24,
		Like_Percent = 12,
		Like_Underscore = 13,
		Member_Name = 25,
		Procedure_Name = 14,
		Property_Name = 26,
		Quote_Prefix = 15,
		Quote_Suffix = 28,
		Schema_Name = 16,
		Schema_Separator = 27,
		Table_Name = 17,
		Text_Command = 18,
		User_Name = 19,
		View_Name = 20
	}
}
