//
// System.Data.OleDb.OleDbLiteral
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

namespace System.Data.OleDb
{

    using System;
    /**
     *
     */
    public enum OleDbLiteral
                                     {

        Invalid = 0,
        Binary_Literal = 1,

        Catalog_Name = 2,

        Catalog_Separator = 3,

        Char_Literal = 4,

        Column_Alias = 5,

        Column_Name = 6,

        Correlation_Name = 7,

        Cursor_Name = 8,

        Escape_Percent_Prefix = 9,

        Escape_Underscore_Prefix = 10,

        Index_Name = 11,

        Like_Percent = 12,

        Like_Underscore = 13,

        Procedure_Name = 14,

        Quote_Prefix = 15,

        Schema_Name = 16,

        Table_Name = 17,

        Text_Command = 18,

        User_Name = 19,

        View_Name = 20,

        Cube_Name = 21,

        Dimension_Name = 22,

        Hierarchy_Name = 23,

        Level_Name = 24,

        Member_Name = 25,

        Property_Name = 26,

        Schema_Separator = 27,

        Quote_Suffix = 28,

        Escape_Percent_Suffix = 29,

        Escape_Underscore_Suffix = 30,


    }
}