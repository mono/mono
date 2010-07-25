//
// System.Data.OleDb.OleDbType
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

    
    /**
     *
     */
    public enum OleDbType
    {

        Empty = 0,

        SmallInt = 2,

        Integer = 3,

        Single = 4,

        Double = 5,

        Currency = 6,

        Date = 7,

        BSTR = 8,

        IDispatch = 9,

        Error = 10,

        Boolean = 11,

        Variant = 12,

        IUnknown = 13,

        Decimal = 14,

        TinyInt = 16,

        UnsignedTinyInt = 17,

        UnsignedSmallInt = 18,

        UnsignedInt = 19,

        BigInt = 20,

        UnsignedBigInt = 21,

        Filetime = 64,

        Guid = 72,

        Binary = 128,

        Char = 129,

        WChar = 130,

        Numeric = 131,

        DBDate = 133,

        DBTime = 134,

        DBTimeStamp = 135,

        PropVariant = 138,

        VarNumeric = 139,

        VarChar = 200,

        LongVarChar = 201,

        VarWChar = 202,

        LongVarWChar = 203,

        VarBinary = 204,

        LongVarBinary = 205,

        
    }
}